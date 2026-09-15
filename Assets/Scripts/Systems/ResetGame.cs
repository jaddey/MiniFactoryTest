using UnityEngine;
using UnityEngine.UI;

public class ResetGame : MonoBehaviour
{
    [SerializeField] private Factory _factory;
    [SerializeField] private BoostManager _boostManager;
    [SerializeField] private SaveSystem _saveSystem;
    [SerializeField] private Button _resetButton;

    private void Start()
    {
        if (_resetButton != null)
        {
            _resetButton.onClick.AddListener(ResetAll);
        }
    }

    public void ResetAll()
    {
        // Сбрасываем валюту
        _factory.AddCoins(-_factory.Currency);

        // Сбрасываем все машины
        foreach (var machine in _factory.Machines)
        {
            machine.SaveTimeSinceLastProduction(0f);
            machine.SetProducing(true);
            machine.Lock();
        }

        // Сбрасываем Boost
        _boostManager.StopBoost();

        // Сохраняем состояние
        _saveSystem.SaveGame(_factory);

        Debug.Log("Игра сброшена на начальные настройки!");
    }
}