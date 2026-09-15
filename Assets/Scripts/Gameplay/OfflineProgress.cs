using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class OfflineProgress : MonoBehaviour
{
    [SerializeField] private SaveSystem _saveSystem;

    public event Action<OfflineRewardData> OnOfflineIncomeReady;

    private OfflineRewardData _pendingReward;

    private float MaxOfflineTime => GameConfigManager.Config?.offlineProduction?.maxOfflineTime ?? 7200f;

    // Вызывается при запуске игры и при возврате из паузы.
    // Считает награду ОДИН раз и кеширует — UI показывает ровно то,
    // что будет выдано по кнопке.
    public void CheckOfflineIncome(Factory factory)
    {
        _pendingReward = CalculateReward(factory);

        if (_pendingReward.TotalCoins > 0)
            OnOfflineIncomeReady?.Invoke(_pendingReward);
    }

    // Начисляет ровно ту сумму, что была показана в UI.
    public void ClaimOfflineIncome(Factory factory)
    {
        if (_pendingReward == null || _pendingReward.TotalCoins <= 0) return;

        factory.AddCoins(_pendingReward.TotalCoins);

        // Доводим прогресс циклов машин до состояния на момент возврата,
        // чтобы те же циклы не посчитались второй раз в живом режиме.
        for (int i = 0; i < _pendingReward.Machines.Count; i++)
        {
            _pendingReward.Machines[i].SaveTimeSinceLastProduction(
                _pendingReward.CycleRemainders[i]);
        }

        _pendingReward = null;

        // Фиксируем новое время выхода, чтобы награда не была выдана повторно.
        _saveSystem.SaveGame(factory);
    }

    private OfflineRewardData CalculateReward(Factory factory)
    {
        var result = new OfflineRewardData();

        var saveData = _saveSystem.LoadGame();
        DateTime lastExitTime = new DateTime(saveData.LastExitTimeTicks);
        DateTime currentTime = DateTime.Now;

        double totalSeconds = (currentTime - lastExitTime).TotalSeconds;
        if (totalSeconds <= 0) return result;

        float offlineTime = Mathf.Min((float)totalSeconds, MaxOfflineTime);
        result.OfflineTime = offlineTime;

        var activeMachines = factory.Machines
            .Where(m => m.State == MachineState.Unlocked).ToList();
        if (activeMachines.Count == 0) return result;
        result.Machines = activeMachines;

        // --- 1. Сколько из offline-времени действовал буст ---
        float boostTime = 0f;
        int boostMultiplier = 1;

        if (saveData.BoostState != null && saveData.BoostState.IsActive)
        {
            DateTime boostEndTime = new DateTime(saveData.BoostState.EndTime);
            if (lastExitTime < boostEndTime)
            {
                double boostLeftAtExit = (boostEndTime - lastExitTime).TotalSeconds;
                boostTime = Mathf.Min(offlineTime, (float)boostLeftAtExit);
                boostMultiplier = saveData.BoostState.Multiplier;
            }
        }

        result.BoostTime = boostTime;
        result.BoostMultiplier = boostMultiplier;
        float normalTime = offlineTime - boostTime;

        // --- 2. Монеты по двум окнам ---
        // Окно буста — с сохранённым множителем, остальное время — базово.
        // Учитываем и незавершённый цикл на момент выхода (carry).
        int totalCoins = 0;
        foreach (var machine in activeMachines)
        {
            float carry = machine.TimeSinceLastProduction;

            int boostCycles = CountCycles(machine, carry, boostTime, out carry);
            int normalCycles = CountCycles(machine, carry, normalTime, out carry);

            totalCoins += boostCycles * machine.CoinsPerCycle * boostMultiplier
                        + normalCycles * machine.CoinsPerCycle;

            result.CycleRemainders.Add(carry);
        }

        result.TotalCoins = totalCoins;
        return result;
    }

    private int CountCycles(Machine machine, float carry, float window, out float newCarry)
    {
        float elapsed = carry + window;
        int cycles = Mathf.FloorToInt(elapsed / machine.CycleDuration);
        newCarry = elapsed - cycles * machine.CycleDuration;
        return cycles;
    }
}

public class OfflineRewardData
{
    public int TotalCoins;
    public float OfflineTime;   // сколько всего не было игрока (сек)
    public float BoostTime;     // сколько из этого действовал буст (сек)
    public int BoostMultiplier; // множитель буста (1 — если буста не было)
    public List<Machine> Machines = new List<Machine>();
    public List<float> CycleRemainders = new List<float>();
}