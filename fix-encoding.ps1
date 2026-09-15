# Fix encoding of all .cs files in Assets
$utf8Strict = New-Object System.Text.UTF8Encoding($false, $true)
$utf8Bom    = New-Object System.Text.UTF8Encoding($true)
$cp1251     = [System.Text.Encoding]::GetEncoding(1251)
$latin2     = [System.Text.Encoding]::GetEncoding("iso-8859-2")

$files = Get-ChildItem -Path Assets -Recurse -Filter *.cs

foreach ($f in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($f.FullName)

    # Case 1: BOM already present
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        Write-Host "SKIP (already UTF-8 BOM): $($f.Name)" -ForegroundColor DarkGray
        continue
    }

    # Check if valid UTF-8
    $isValidUtf8 = $true
    try { [void]$utf8Strict.GetString($bytes) } catch { $isValidUtf8 = $false }

    if (-not $isValidUtf8) {
        # Case 2: legacy Windows-1251
        $text = $cp1251.GetString($bytes)
        [System.IO.File]::WriteAllText($f.FullName, $text, $utf8Bom)
        Write-Host "FIXED (was 1251):     $($f.Name)" -ForegroundColor Green
        continue
    }

    # Case 3: valid UTF-8, check for double-encoded mojibake
    $text = [System.Text.Encoding]::UTF8.GetString($bytes)
    $suspicious = [regex]::Matches($text, '[\u00C0-\u017F]').Count

    if ($suspicious -gt 10) {
        $bytes2 = $latin2.GetBytes($text)
        $fixed  = $cp1251.GetString($bytes2)

        if ($fixed -match '[\u0400-\u04FF]' -and [regex]::Matches($fixed, '[\u00C0-\u017F]').Count -lt 5) {
            [System.IO.File]::WriteAllText($f.FullName, $fixed, $utf8Bom)
            Write-Host "FIXED (was mojibake): $($f.Name)" -ForegroundColor Green
        } else {
            Write-Host "!!! CHECK MANUALLY:   $($f.Name)" -ForegroundColor Yellow
        }
    } else {
        # Case 4: plain UTF-8 without BOM
        [System.IO.File]::WriteAllText($f.FullName, $text, $utf8Bom)
        Write-Host "FIXED (added BOM):    $($f.Name)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Done. Check result with: git diff"