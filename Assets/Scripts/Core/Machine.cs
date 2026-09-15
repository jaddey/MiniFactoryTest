using UnityEngine;
using System;

public enum MachineState
{
    Locked,
    Unlocked
}

public class Machine : MonoBehaviour
{
    [SerializeField] private int _id;
    [SerializeField] private int _baseCoinsPerCycle = 1; // Базовая производительность (например, 1 или 25)
    [SerializeField] private float _cycleDuration = 3f;
    [SerializeField] private int _unlockCost = 100;
    [SerializeField] private int _baseUpgradeCost = 50;

    [SerializeField] private MachineState _state = MachineState.Locked;
    [SerializeField] private Animation _animation;
    [SerializeField] private string _animationClipName = "MachineAnimation";

    private int _level = 1;
    private float _timeSinceLastProduction = 0f;
    private bool _isProducing = true;
    private float _defaultAnimationSpeed = 1f;

    public event Action<Machine> OnStateChanged;
    public event Action<Machine> OnLevelChanged;
    public event Action<Machine, int> OnProduced;

    public int Id => _id;
    public MachineState State => _state;
    public int Level => _level;
    public int UnlockCost => _unlockCost;
    public int UpgradeCost => _baseUpgradeCost * _level;
    public float TimeSinceLastProduction => _timeSinceLastProduction;
    public int CoinsPerCycle => _baseCoinsPerCycle * _level; // Умножаем базовую производительность на уровень
    public float CycleDuration => _cycleDuration / (1 + (_level - 1) * 0.1f); // Ускорение цикла при улучшении
    public bool IsProducing => _isProducing;

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
        if (_state != MachineState.Unlocked) return false;
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