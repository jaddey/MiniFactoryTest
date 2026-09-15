using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Factory _factory;
    [SerializeField] private BoostManager _boostManager;
    [SerializeField] private OfflineProgress _offlineProgress;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private TextMeshProUGUI _boostStatusText;
    [SerializeField] private TextMeshProUGUI _incomePerMinuteText;

    [Header("Offline Reward UI")]
    [SerializeField] private GameObject _offlineRewardPanel;
    [SerializeField] private TextMeshProUGUI _offlineRewardText;
    [SerializeField] private Button _claimButton;

    private void Start()
    {
        _offlineRewardPanel.SetActive(false);
        _offlineProgress.OnOfflineIncomeReady += ShowOfflineReward;
        _claimButton.onClick.AddListener(ClaimOfflineReward);
        UpdateUI();
    }

    private void Update()
    {
        UpdateBoostStatus();
        UpdateIncomePerMinute();
    }

    private void OnEnable()
    {
        _factory.OnCurrencyChanged += UpdateCoins;
        _boostManager.OnBoostStarted += UpdateBoostStatus;
        _boostManager.OnBoostFinished += UpdateBoostStatus;
        _factory.OnMachinesUpdated += UpdateIncomePerMinute;
    }

    private void OnDisable()
    {
        _factory.OnCurrencyChanged -= UpdateCoins;
        _boostManager.OnBoostStarted -= UpdateBoostStatus;
        _boostManager.OnBoostFinished -= UpdateBoostStatus;
        _factory.OnMachinesUpdated -= UpdateIncomePerMinute;
    }

    private void ShowOfflineReward(int coins, float time)
    {
        if (coins > 0 && time > 0)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            _offlineRewardText.text = $"Вас не было в игре {minutes} мин {seconds} сек\nВаша награда: {coins:N0} монет";
            _offlineRewardPanel.SetActive(true);
        }
    }

    private void ClaimOfflineReward()
    {
        _offlineProgress.ClaimOfflineIncome(_factory);
        _offlineRewardPanel.SetActive(false);
    }

    private void UpdateUI()
    {
        UpdateCoins(_factory.Currency);
        UpdateBoostStatus();
        UpdateIncomePerMinute();
    }

    private void UpdateCoins(int coins)
    {
        _coinsText.text = $"Coins: {coins:N0}";
    }

    private void UpdateBoostStatus()
    {
        if (_boostManager.IsActive)
        {
            int remainingSeconds = Mathf.CeilToInt(_boostManager.RemainingTime);
            _boostStatusText.text = $"Boost: {remainingSeconds} sec (x{_boostManager.Multiplier})";
            _boostStatusText.color = Color.green;
        }
        else
        {
            _boostStatusText.text = "Boost: Inactive";
            _boostStatusText.color = Color.red;
        }
    }

    private void UpdateIncomePerMinute()
    {
        float coinsPerSecond = _factory.GetTotalProductionPerSecond();
        int coinsPerMinute = Mathf.RoundToInt(coinsPerSecond * 60f);
        if (_boostManager.IsActive)
        {
            coinsPerMinute = Mathf.RoundToInt(coinsPerSecond * 60f * _boostManager.Multiplier);
        }
        _incomePerMinuteText.text = $"Income: {coinsPerMinute:N0} coins/min";
    }
}