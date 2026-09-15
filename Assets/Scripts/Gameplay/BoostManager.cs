using UnityEngine;
using System;

public class BoostManager : MonoBehaviour
{
    [SerializeField] private float _defaultDuration = 30f;
    [SerializeField] private int _defaultMultiplier = 2;
    [SerializeField] private Machine[] _machines; // Массив всех машин для управления анимациями

    private bool _isActive = false;
    private DateTime _endTime;
    private int _currentMultiplier = 1;

    public event Action OnBoostStarted;
    public event Action OnBoostFinished;

    public bool IsActive => _isActive;
    public int Multiplier => _currentMultiplier;
    public float RemainingTime => _isActive ? Mathf.Max(0f, (float)(_endTime - DateTime.Now).TotalSeconds) : 0f;

    private void Start()
    {
        _machines = FindObjectsOfType<Machine>(); // Автоматически находим все машины
    }

    private void Update()
    {
        if (_isActive && DateTime.Now >= _endTime)
        {
            StopBoost();
        }
    }

    public void StartBoost()
    {
        StartBoost(_defaultDuration, _defaultMultiplier);
    }

    public void StartBoost(float duration, int multiplier)
    {
        if (_isActive) return;
        _isActive = true;
        _currentMultiplier = multiplier;
        _endTime = DateTime.Now.AddSeconds(duration);

        // Ускоряем анимации всех машин
        foreach (var machine in _machines)
        {
            machine.SetAnimationSpeed(_currentMultiplier);
        }

        OnBoostStarted?.Invoke();
    }

    public void StopBoost()
    {
        _isActive = false;
        _currentMultiplier = 1;

        // Возвращаем скорость анимаций к нормальной
        foreach (var machine in _machines)
        {
            machine.SetAnimationSpeed(1f);
        }

        OnBoostFinished?.Invoke();
    }

    public BoostSaveData GetSaveData()
    {
        return new BoostSaveData
        {
            IsActive = _isActive,
            EndTime = _endTime.Ticks,
            Multiplier = _currentMultiplier
        };
    }

    public void LoadBoostState(BoostSaveData saveData)
    {
        if (saveData.IsActive && new DateTime(saveData.EndTime) > DateTime.Now)
        {
            _isActive = true;
            _endTime = new DateTime(saveData.EndTime);
            _currentMultiplier = saveData.Multiplier;

            // Применяем скорость анимации при загрузке
            foreach (var machine in _machines)
            {
                machine.SetAnimationSpeed(_currentMultiplier);
            }
        }
        else
        {
            StopBoost();
        }
    }
}

// Класс для сохранения состояния Boost
[System.Serializable]
public class BoostSaveData
{
    public bool IsActive;
    public long EndTime; // В тиках (для точности)
    public int Multiplier;
}