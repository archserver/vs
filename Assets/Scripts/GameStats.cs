using System.IO;
using UnityEngine;

// The data to saved to disk — fields are serializable
[System.Serializable]
public class LifetimeStatsData
{
    public int zombiesKilled;
    public int wyvernsKilled;
    public int gemsGathered;
    public int heartsGathered;
    public int highestLevel;
    public float totalXPGained;
    public float totalDamageDone;
    public float totalHealthRecovered;
    public float totalDamageTaken;
    public int spellsCast;
    public int totalGamesPlayed;
}

// Current session stats, accumulates them into lifetime totals to save to disk
public class GameStats : MonoBehaviour
{
    public static GameStats GSInstance;

    // --- current session ---
    public int zombiesKilled;
    public int wyvernsKilled;
    public int gemsGathered;
    public int heartsGathered;
    public int highestLevel;
    public float totalXPGained;
    public float totalDamageDone;
    public float totalHealthRecovered;
    public float totalDamageTaken;
    public int spellsCast;

    // --- lifetime totals loaded and saved to disk ---
    public LifetimeStatsData lifetime = new LifetimeStatsData();

    private static string SavePath => Path.Combine(Application.persistentDataPath, "lifetimestats.json");

    private void Awake()
    {
        if (GSInstance != null && GSInstance != this)
            Destroy(this);
        else
            GSInstance = this;

        LoadLifetimeStats();
        ResetSessionStats();
    }

    // clear session stats at the start of each run
    public void ResetSessionStats()
    {
        zombiesKilled        = 0;
        wyvernsKilled        = 0;
        gemsGathered         = 0;
        heartsGathered       = 0;
        highestLevel         = 1;
        totalXPGained        = 0f;
        totalDamageDone      = 0f;
        totalHealthRecovered = 0f;
        totalDamageTaken     = 0f;
        spellsCast           = 0;
    }

    // add session's results into lifetime totals and save to disk
    public void CommitSessionToLifetime()
    {
        lifetime.zombiesKilled        += zombiesKilled;
        lifetime.wyvernsKilled        += wyvernsKilled;
        lifetime.gemsGathered         += gemsGathered;
        lifetime.heartsGathered       += heartsGathered;
        lifetime.totalXPGained        += totalXPGained;
        lifetime.totalDamageDone      += totalDamageDone;
        lifetime.totalHealthRecovered += totalHealthRecovered;
        lifetime.totalDamageTaken     += totalDamageTaken;
        lifetime.spellsCast           += spellsCast;
        lifetime.totalGamesPlayed++;

        // highest level is the best across all runs
        if (highestLevel > lifetime.highestLevel)
            lifetime.highestLevel = highestLevel;

        SaveLifetimeStats();
    }

    // write lifetime stats to a JSON file in the device storage
    private void SaveLifetimeStats()
    {
        string json = JsonUtility.ToJson(lifetime, prettyPrint: true);
        File.WriteAllText(SavePath, json);
    }

    // read lifetime stats from disk — start fresh if no save file exists 
    private void LoadLifetimeStats()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            lifetime = JsonUtility.FromJson<LifetimeStatsData>(json);
        }
        else
        {
            lifetime = new LifetimeStatsData();
        }
    }
}
