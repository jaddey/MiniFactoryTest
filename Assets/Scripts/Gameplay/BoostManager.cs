using UnityEngine;
using System;

public interface IBoostManager
{
    bool IsActive { get; }
    int Multiplier { get; }
    float RemainingTime { get; }
    BoostSaveData GetSaveData();
    void LoadBoostState(BoostSaveData saveData);
}

public class BoostManager : MonoBehaviour, IBoostManager
{
    private bool _isActive = false;
    private DateTime _endTime;
    private int _currentMultiplier = 1;
    [SerializeField] private Machine[] _machines;

    public bool IsActive => _isActive;
    public int Multiplier => _currentMultiplier;
    public float RemainingTime => _isActive ? Mathf.Max(0f, (float)(_endTime - DateTime.Now).TotalSeconds) : 0f;

    private float DefaultDuration => GameConfigManager.Config?.boost?.defaultDuration ?? 30f;
    private int DefaultMultiplier => GameConfigManager.Config?.boost?.defaultMultiplier ?? 2;

    private void Start()
    {
        _machines = FindObjectsOfType<Machine>();
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
        StartBoost(DefaultDuration, DefaultMultiplier);
    }

    public void StartBoost(float duration, int multiplier)
    {
        if (_isActive) return;
        _isActive = true;
        _currentMultiplier = multiplier;
        _endTime = DateTime.Now.AddSeconds(duration);

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
        foreach (var machine in _machines)
        {
            machine.SetAnimationSpeed(1f);
        }
        OnBoostFinished?.Invoke();
    }

    public BoostSaveData GetSaveData()
    {
        Debug.Log($"[BoostManager] Saving Boost state: IsActive={_isActive}, Multiplier={_currentMultiplier}, EndTime={_endTime}");
        return new BoostSaveData
        {
            IsActive = _isActive,
            EndTime = _endTime.Ticks,
            Multiplier = _currentMultiplier 
        };
    }

    public void LoadBoostState(BoostSaveData saveData)
    {
        Debug.Log($"[BoostManager] Loading Boost state: IsActive={saveData.IsActive}, Multiplier={saveData.Multiplier}, EndTime={new DateTime(saveData.EndTime)}");
        if (saveData.IsActive && new DateTime(saveData.EndTime) > DateTime.Now)
        {
            _isActive = true;
            _endTime = new DateTime(saveData.EndTime);
            _currentMultiplier = saveData.Multiplier;
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

    public event Action OnBoostStarted;
    public event Action OnBoostFinished;
}
[System.Serializable]
public class BoostSaveData
{
    public bool IsActive;
    public long EndTime;
    public int Multiplier;
}