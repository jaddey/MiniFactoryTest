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

    private void Awake()
    {
        // Скрываем панель именно здесь, а не в Start():
        // Start может выполниться ПОСЛЕ того, как Factory.Start() уже показал
        // награду через событие OnOfflineIncomeReady — и скрыл бы её обратно.
        _offlineRewardPanel.SetActive(false);
        _claimButton.onClick.AddListener(ClaimOfflineReward);
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        UpdateBoostStatus();
        UpdateIncomePerMinute();
    }

    private void OnEnable()
    {
        // Подписка именно в OnEnable: он выполняется до ВСЕХ Start(),
        // поэтому событие из Factory.Start() гарантированно будет поймано.
        _offlineProgress.OnOfflineIncomeReady += ShowOfflineReward;
        _factory.OnCurrencyChanged += UpdateCoins;
        _boostManager.OnBoostStarted += UpdateBoostStatus;
        _boostManager.OnBoostFinished += UpdateBoostStatus;
        _factory.OnMachinesUpdated += UpdateIncomePerMinute;
    }

    private void OnDisable()
    {
        _offlineProgress.OnOfflineIncomeReady -= ShowOfflineReward;
        _factory.OnCurrencyChanged -= UpdateCoins;
        _boostManager.OnBoostStarted -= UpdateBoostStatus;
        _boostManager.OnBoostFinished -= UpdateBoostStatus;
        _factory.OnMachinesUpdated -= UpdateIncomePerMinute;
    }

    private void ShowOfflineReward(OfflineRewardData reward)
    {
        if (reward == null || reward.TotalCoins <= 0) return;

        int minutes = Mathf.FloorToInt(reward.OfflineTime / 60f);
        int seconds = Mathf.FloorToInt(reward.OfflineTime % 60f);

        string boostInfo = reward.BoostTime > 0f
            ? $"\nИз них буст x{reward.BoostMultiplier}: {Mathf.FloorToInt(reward.BoostTime)} сек"
            : "";

        _offlineRewardText.text =
            $"Вас не было: {minutes} мин {seconds} сек{boostInfo}\nЗаработано: {reward.TotalCoins:N0} монет";
        _offlineRewardPanel.SetActive(true);
    }

    private void ClaimOfflineReward()
    {
        // Начисляется ровно та сумма, что была показана на панели
        // (кеширована в OfflineProgress на момент расчёта).
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