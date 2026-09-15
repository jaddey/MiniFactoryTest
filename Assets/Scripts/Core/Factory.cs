using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class Factory : MonoBehaviour
{
    public event Action<int> OnCurrencyChanged;
    public event Action OnMachinesUpdated;

    [SerializeField] private Machine[] _machines;
    [SerializeField] private BoostManager _boostManager;
    [SerializeField] private OfflineProgress _offlineProgress;
    [SerializeField] private SaveSystem _saveSystem;

    private int _currency = 0;
    private bool _isProducing = true;

    public int Currency
    {
        get => _currency;
        private set
        {
            _currency = value;
            OnCurrencyChanged?.Invoke(_currency);
        }
    }

    public IEnumerable<Machine> Machines => _machines;
    public BoostManager BoostManager => _boostManager;

    public bool IsProducing
    {
        get => _isProducing;
        set
        {
            _isProducing = value;
            foreach (var machine in _machines)
                machine.SetProducing(value);
        }
    }

    private void Start()
    {
        var saveData = _saveSystem.LoadGame();
        Currency = saveData.Currency;
        _boostManager.LoadBoostState(saveData.BoostState);

        LoadMachines(saveData);
        SubscribeToMachines();

        // ѕровер€ем offline награду при старте
        _offlineProgress.CheckOfflineIncome(this);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        IsProducing = !pauseStatus;
        if (pauseStatus)
        {
            // —охран€ем врем€ выхода при паузе (сворачивании)
            _saveSystem.SaveGame(this);
        }
        else
        {
            // ѕри возвращении из паузы провер€ем offline награду
            _offlineProgress.CheckOfflineIncome(this);
        }
    }

    private void OnApplicationQuit()
    {
        _saveSystem.SaveGame(this);
    }

    private void LoadMachines(GameSaveData saveData)
    {
        foreach (var machine in _machines)
        {
            var machineSaveData = saveData.MachineStates.FirstOrDefault(m => m.Id == machine.Id);
            if (machineSaveData != null)
            {
                machine.SaveTimeSinceLastProduction(machineSaveData.TimeSinceLastProduction);
                if (machineSaveData.IsUnlocked)
                    machine.Unlock();
                for (int j = 1; j < machineSaveData.Level; j++)
                    machine.Upgrade();
            }
        }
    }

    private void SubscribeToMachines()
    {
        foreach (var machine in _machines)
        {
            machine.OnProduced += OnMachineProduced;
            machine.OnStateChanged += (m) => OnMachinesUpdated?.Invoke();
            machine.OnLevelChanged += (m) => OnMachinesUpdated?.Invoke();
        }
    }

    private void OnMachineProduced(Machine machine, int coins)
    {
        int boostMultiplier = _boostManager.IsActive ? _boostManager.Multiplier : 1;
        AddCoins(coins * boostMultiplier);
    }

    public bool UnlockMachine(Machine machine)
    {
        if (machine.State == MachineState.Unlocked) return false;
        if (Currency < machine.UnlockCost) return false;

        SpendCoins(machine.UnlockCost);
        machine.Unlock();
        return true;
    }

    public bool UpgradeMachine(Machine machine)
    {
        if (machine.State != MachineState.Unlocked) return false;
        if (Currency < machine.UpgradeCost) return false;

        SpendCoins(machine.UpgradeCost);
        machine.Upgrade();
        return true;
    }

    public void AddCoins(int amount)
    {
        Currency += amount;
    }

    public bool SpendCoins(int amount)
    {
        if (Currency < amount) return false;
        Currency -= amount;
        return true;
    }

    public float GetTotalProductionPerSecond()
    {
        return _machines
            .Where(m => m.State == MachineState.Unlocked)
            .Sum(m => (float)m.CoinsPerCycle / m.CycleDuration);
    }
}