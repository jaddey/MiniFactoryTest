using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MachineUI : MonoBehaviour
{
    [SerializeField] private Machine _machine;
    [SerializeField] private Factory _factory;
    [SerializeField] private int _maxLevel = 10;

    [Header("UI Elements")]
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _actionButtonText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _statusText;

    private void Start()
    {
        if (_actionButton != null)
        {
            _actionButton.onClick.AddListener(OnActionButtonClicked);
        }
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void OnActionButtonClicked()
    {
        if (_machine == null || _factory == null) return;

        if (_machine.State == MachineState.Locked)
        {
            if (_factory.Currency >= _machine.UnlockCost)
            {
                _factory.UnlockMachine(_machine);
            }
            else
            {
                Debug.LogWarning("Недостаточно монет для разблокировки!");
            }
        }
        else if (_machine.State == MachineState.Unlocked)
        {
            if (_machine.Level >= _machine.MaxLevel)
            {
                Debug.LogWarning($"Максимальный уровень ({_machine.MaxLevel}) уже достигнут!");
                return;
            }

            if (_factory.Currency >= _machine.UpgradeCost)
            {
                _factory.UpgradeMachine(_machine);
            }
            else
            {
                Debug.LogWarning("Недостаточно монет для улучшения!");
            }
        }
    }

    private void UpdateUI()
    {
        if (_machine == null || _actionButtonText == null || _levelText == null) return;

        if (_machine.State == MachineState.Locked)
        {
            _actionButtonText.text = $"Разблокировать - {_machine.UnlockCost} монет";
            _levelText.text = $"{_machine.Name} | Заблокировано";
            if (_statusText != null)
                _statusText.text = "";
        }
        else
        {
            if (_machine.Level >= _machine.MaxLevel)
            {
                _actionButtonText.text = "Макс. уровень";
                _actionButton.interactable = false;
            }
            else
            {
                _actionButtonText.text = $"Улучшить - {_machine.UpgradeCost} монет";
                _actionButton.interactable = true;
            }
            _levelText.text = $"{_machine.Name} | Уровень: {_machine.Level}/{_machine.MaxLevel}";
            if (_statusText != null)
                _statusText.text = $"Производит: {_machine.CoinsPerCycle} монет за {_machine.CycleDuration:F1} сек";
        }
    }
}