using UnityEngine;
using System;
using System.Linq;

public class OfflineProgress : MonoBehaviour
{
    [SerializeField] private float _maxOfflineTime = 7200f; // 2 часа
    [SerializeField] private SaveSystem _saveSystem;

    public event Action<int, float> OnOfflineIncomeReady; // (монеты, врем€ просто€)

    public void CheckOfflineIncome(Factory factory)
    {
        var saveData = _saveSystem.LoadGame();
        DateTime lastExitTime = new DateTime(saveData.LastExitTimeTicks);
        DateTime currentTime = DateTime.Now;

        double offlineTimeTotalSeconds = (currentTime - lastExitTime).TotalSeconds;
        float offlineTime = Mathf.Min((float)offlineTimeTotalSeconds, _maxOfflineTime);
        if (offlineTime <= 0f)
        {
            OnOfflineIncomeReady?.Invoke(0, 0f);
            return;
        }

        // ѕолучаем список активных машин
        var activeMachines = factory.Machines
            .Where(m => m.State == MachineState.Unlocked)
            .ToList();

        if (activeMachines.Count == 0)
        {
            OnOfflineIncomeReady?.Invoke(0, 0f);
            return;
        }

        // –ассчитываем врем€ действи€ Boost во врем€ offline
        float boostOfflineTime = 0f;
        int boostMultiplier = 1;

        if (saveData.BoostState.IsActive)
        {
            DateTime boostEndTime = new DateTime(saveData.BoostState.EndTime);
            if (lastExitTime <= boostEndTime)
            {
                double boostTimeLeftAtExit = (boostEndTime - lastExitTime).TotalSeconds;
                boostOfflineTime = Mathf.Min(offlineTime, (float)boostTimeLeftAtExit);
                boostMultiplier = saveData.BoostState.Multiplier;
            }
        }

        // –ассчитываем монеты дл€ каждой машины отдельно
        int totalCoins = 0;
        foreach (var machine in activeMachines)
        {
            // —колько полных циклов произвела машина за врем€ offline
            float cyclesInOffline = offlineTime / machine.CycleDuration;
            int fullCycles = Mathf.FloorToInt(cyclesInOffline);
            totalCoins += fullCycles * machine.CoinsPerCycle;
        }

        // ”читываем Boost дл€ времени, когда он был активен
        int boostCoins = 0;
        foreach (var machine in activeMachines)
        {
            float cyclesInBoostTime = boostOfflineTime / machine.CycleDuration;
            int fullBoostCycles = Mathf.FloorToInt(cyclesInBoostTime);
            boostCoins += fullBoostCycles * machine.CoinsPerCycle * (boostMultiplier - 1); // только дополнительные монеты от Boost
        }

        int coinsToAdd = totalCoins + boostCoins;

        OnOfflineIncomeReady?.Invoke(coinsToAdd, offlineTime);
    }

    public void ClaimOfflineIncome(Factory factory)
    {
        var saveData = _saveSystem.LoadGame();
        DateTime lastExitTime = new DateTime(saveData.LastExitTimeTicks);
        DateTime currentTime = DateTime.Now;

        double offlineTimeTotalSeconds = (currentTime - lastExitTime).TotalSeconds;
        float offlineTime = Mathf.Min((float)offlineTimeTotalSeconds, _maxOfflineTime);
        if (offlineTime <= 0f) return;

        var activeMachines = factory.Machines
            .Where(m => m.State == MachineState.Unlocked)
            .ToList();

        if (activeMachines.Count == 0) return;

        float boostOfflineTime = 0f;
        int boostMultiplier = 1;

        if (saveData.BoostState.IsActive)
        {
            DateTime boostEndTime = new DateTime(saveData.BoostState.EndTime);
            if (lastExitTime <= boostEndTime)
            {
                double boostTimeLeftAtExit = (boostEndTime - lastExitTime).TotalSeconds;
                boostOfflineTime = Mathf.Min(offlineTime, (float)boostTimeLeftAtExit);
                boostMultiplier = saveData.BoostState.Multiplier;
            }
        }

        int totalCoins = 0;
        foreach (var machine in activeMachines)
        {
            float cyclesInOffline = offlineTime / machine.CycleDuration;
            int fullCycles = Mathf.FloorToInt(cyclesInOffline);
            totalCoins += fullCycles * machine.CoinsPerCycle;
        }

        int boostCoins = 0;
        foreach (var machine in activeMachines)
        {
            float cyclesInBoostTime = boostOfflineTime / machine.CycleDuration;
            int fullBoostCycles = Mathf.FloorToInt(cyclesInBoostTime);
            boostCoins += fullBoostCycles * machine.CoinsPerCycle * (boostMultiplier - 1);
        }

        int coinsToAdd = totalCoins + boostCoins;

        if (coinsToAdd > 0)
        {
            factory.AddCoins(coinsToAdd);
            Debug.Log($"Offline income claimed: +{coinsToAdd} coins for {offlineTime:F0} seconds");
        }

        _saveSystem.SaveGame(factory);
    }
}