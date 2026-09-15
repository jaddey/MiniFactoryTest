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
        // —брасываем монеты
        _factory.AddCoins(-_factory.Currency);

        // —брасываем все машины
        foreach (var machine in _factory.Machines)
        {
            machine.SaveTimeSinceLastProduction(0f);
            machine.SetProducing(true);
            machine.Lock(); // Ѕлокируем машину
            // —брасываем уровень до 1 (если нужно)
            // (¬ текущей реализации уровень сбрасываетс€ при блокировке, но если нет Ч нужно добавить сброс уровн€)
        }

        // —брасываем Boost
        _boostManager.StopBoost();

        // —охран€ем сброшенное состо€ние
        _saveSystem.SaveGame(_factory);

        Debug.Log("»гра сброшена на стартовые значени€!");
    }
}