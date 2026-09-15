using UnityEngine;
using System;

public enum MachineState
{
    Locked,
    Unlocked
}

public class Machine : MonoBehaviour
{
    [SerializeField] private int _id; // ID машины (для поиска в конфиге)

    private MachineState _state = MachineState.Locked;
    private int _level = 1;
    private float _timeSinceLastProduction = 0f;
    private bool _isProducing = true;
    [SerializeField] private Animation _animation;
    [SerializeField] private string _animationClipName = "MachineAnimation";
    private float _defaultAnimationSpeed = 1f;

    public event Action<Machine> OnStateChanged;
    public event Action<Machine> OnLevelChanged;
    public event Action<Machine, int> OnProduced;

    public int Id => _id;
    public MachineState State => _state;
    public int Level => _level;
    public int UnlockCost => GetConfig()?.unlockCost ?? 0;
    public int UpgradeCost => GetConfig()?.baseUpgradeCost * _level ?? 0;
    public float TimeSinceLastProduction => _timeSinceLastProduction;
    public int CoinsPerCycle => GetConfig()?.baseCoinsPerCycle * _level ?? 0;
    public float CycleDuration => GetConfig()?.baseCycleDuration / (1 + (_level - 1) * 0.1f) ?? 3f;
    public bool IsProducing => _isProducing;
    public int MaxLevel => GetConfig()?.maxLevel ?? 10;
    public string Name => GetConfig()?.name ?? $"Машина {_id}";

    private MachineConfig GetConfig()
    {
        if (GameConfigManager.Config?.machines == null) return null;
        foreach (var config in GameConfigManager.Config.machines)
        {
            if (config.id == _id) return config;
        }
        return null;
    }

    private void Start()
    {
        UpdateAnimationState();
    }

    public bool Unlock()
    {
        if (_state == MachineState.Unlocked) return false;
        _state = MachineState.Unlocked;
        OnStateChanged?.Invoke(this);
        UpdateAnimationState();
        return true;
    }

    public bool Lock()
    {
        if (_state == MachineState.Locked) return false;
        _state = MachineState.Locked;
        _level = 1;
        OnStateChanged?.Invoke(this);
        UpdateAnimationState();
        return true;
    }

    public bool Upgrade()
    {
        if (_state != MachineState.Unlocked || _level >= MaxLevel) return false;
        _level++;
        OnLevelChanged?.Invoke(this);
        UpdateAnimationState();
        return true;
    }

    public void SetProducing(bool isProducing)
    {
        _isProducing = isProducing;
        UpdateAnimationState();
    }

    public void SetAnimationSpeed(float speed)
    {
        if (_animation == null || string.IsNullOrEmpty(_animationClipName)) return;
        _animation[_animationClipName].speed = _defaultAnimationSpeed * speed;
    }

    private void UpdateAnimationState()
    {
        if (_animation == null || string.IsNullOrEmpty(_animationClipName)) return;
        bool shouldPlay = _state == MachineState.Unlocked && _isProducing;
        if (shouldPlay)
        {
            _animation[_animationClipName].speed = _defaultAnimationSpeed;
            _animation.Play(_animationClipName);
        }
        else
        {
            _animation.Stop(_animationClipName);
        }
    }

    private void Update()
    {
        if (_state != MachineState.Unlocked || !_isProducing) return;
        _timeSinceLastProduction += Time.deltaTime;
        if (_timeSinceLastProduction >= CycleDuration)
        {
            _timeSinceLastProduction = 0f;
            OnProduced?.Invoke(this, CoinsPerCycle);
        }
    }

    public void SaveTimeSinceLastProduction(float time)
    {
        _timeSinceLastProduction = time;
    }

    public float GetTimeSinceLastProduction() => _timeSinceLastProduction;
}