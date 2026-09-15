using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

[System.Serializable]
public class MachineSaveData
{
    public int Id;
    public bool IsUnlocked;
    public int Level;
    public float TimeSinceLastProduction;
}

[System.Serializable]
public class GameSaveData
{
    public int Currency;
    public List<MachineSaveData> MachineStates = new List<MachineSaveData>();
    public long LastExitTimeTicks;
    public BoostSaveData BoostState = new BoostSaveData { IsActive = false, Multiplier = 1 };
}

public class SaveSystem : MonoBehaviour
{
    private string _savePath => Path.Combine(Application.persistentDataPath, "game_save.json");

    public GameSaveData LoadGame()
    {
        if (!File.Exists(_savePath))
        {
            return new GameSaveData
            {
                Currency = 0,
                LastExitTimeTicks = 0,
                MachineStates = new List<MachineSaveData>(),
                BoostState = new BoostSaveData { IsActive = false, Multiplier = 1 }
            };
        }

        string json = File.ReadAllText(_savePath);
        return JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();
    }

    public void SaveGame(Factory factory)
    {
        var saveData = new GameSaveData
        {
            Currency = factory.Currency,
            LastExitTimeTicks = DateTime.Now.Ticks,
            MachineStates = new List<MachineSaveData>(),
            BoostState = factory.BoostManager.GetSaveData()
        };

        foreach (var machine in factory.Machines)
        {
            saveData.MachineStates.Add(new MachineSaveData
            {
                Id = machine.Id,
                IsUnlocked = machine.State == MachineState.Unlocked,
                Level = machine.Level,
                TimeSinceLastProduction = machine.GetTimeSinceLastProduction()
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(_savePath, json);
    }
}