using UnityEngine;
using System.IO;
using System;

public class GameConfigManager : MonoBehaviour
{
    private static GameConfig _config;
    public static GameConfig Config => _config;

    private void Awake()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "game_config.json");
        if (!File.Exists(filePath))
        {
            Debug.LogError("Конфигурационный файл не найден: " + filePath);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }

        string json = File.ReadAllText(filePath);
        try
        {
            _config = JsonUtility.FromJson<GameConfig>(json);
            Debug.Log("Конфигурация загружена успешно!");
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка парсинга конфигурации: " + e.Message);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        if (_config == null)
        {
            Debug.LogError("Конфигурация не загружена!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}