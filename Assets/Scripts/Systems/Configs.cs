[System.Serializable]
public class MachineConfig
{
    public int id;
    public string name;
    public int unlockCost;
    public int baseUpgradeCost;
    public int baseCoinsPerCycle;
    public float baseCycleDuration;
    public int maxLevel;
}

[System.Serializable]
public class BoostConfig
{
    public float defaultDuration;
    public int defaultMultiplier;
}

[System.Serializable]
public class OfflineProductionConfig
{
    public float maxOfflineTime;
}

[System.Serializable]
public class GameConfig
{
    public MachineConfig[] machines;
    public BoostConfig boost;
    public OfflineProductionConfig offlineProduction;
}