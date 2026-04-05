using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Manages all UI elements on the canvas — health, experience, timers, and panels
public class UIController : MonoBehaviour
{
    public static UIController UIInstance;          // poimter so any script can update the UI

    [SerializeField] private Slider playerHealthSlider;         // health bar slider
    [SerializeField] private TextMeshProUGUI playerHealthText;  // health bar txt
    [SerializeField] private Slider playerExpSlider;            // experience bar slider
    [SerializeField] private TextMeshProUGUI playerExpText;     // experience bar txt
    public GameObject gameOverPanel;                            // shown when the player dies
    public GameObject pausedPanel;                              // shown when the game is paused
    [SerializeField] private TMP_Text aliveTimeText;            // displays how long the player has survived

    [Header("Game Over Stats")]
    [SerializeField] private TMP_Text statsText;                // single text block on the game over panel

    // prevent duplicate UI instances when changing scenes
    private void Awake()
    {
        if (UIInstance != null && UIInstance != this)
            Destroy(this);
        else
            UIInstance = this;
    }

    // refresh the health bar and label to match current player health
    public void UpdateHealthSlider()
    {
        playerHealthSlider.maxValue = PlayerController.PlayerInstance.playerMaxHealth;
        playerHealthSlider.value = PlayerController.PlayerInstance.playerHealth;
        playerHealthText.text = "Health: " + playerHealthSlider.value.ToString("F1") + "/" + playerHealthSlider.maxValue.ToString("F1");
    }

    // refresh the experience bar and label to match current XP and level
    public void UpdateExpSlider()
    {
        playerExpSlider.maxValue = PlayerController.PlayerInstance.xpThreshold;
        playerExpSlider.value = PlayerController.PlayerInstance.playerExperience;
        playerExpText.text = "Level: " + PlayerController.PlayerInstance.playerLevel + " EXP: " + playerExpSlider.value + "/" + playerExpSlider.maxValue;
    }

    // populate the stats block on the game over panel with this session and lifetime totals
    public void ShowGameOverStats()
    {
        if (statsText == null) return;
        GameStats s = GameStats.GSInstance;
        LifetimeStatsData l = s.lifetime;
        statsText.text =
            "                       This Run   |   All Time\n" +
            "Zombies Killed:    "   + PadStat(s.zombiesKilled)                       + " | " + l.zombiesKilled        + "\n" +
            "Wyverns Killed:    "   + PadStat(s.wyvernsKilled)                       + " | " + l.wyvernsKilled        + "\n" +
            "XP Gems:           "   + PadStat(s.gemsGathered)                        + " | " + l.gemsGathered         + "\n" +
            "Hearts:            "   + PadStat(s.heartsGathered)                      + " | " + l.heartsGathered       + "\n" +
            "Highest Level:     "   + PadStat(s.highestLevel)                        + " | " + l.highestLevel         + "\n" +
            "XP Gained:         "   + PadStat(s.totalXPGained)                       + " | " + l.totalXPGained        + "\n" +
            "Damage Done:       "   + PadStat(s.totalDamageDone.ToString("F1"))      + " | " + l.totalDamageDone.ToString("F1")      + "\n" +
            "Health Recovered:  "   + PadStat(s.totalHealthRecovered.ToString("F1")) + " | " + l.totalHealthRecovered.ToString("F1") + "\n" +
            "Damage Taken:      "   + PadStat(s.totalDamageTaken.ToString("F1"))     + " | " + l.totalDamageTaken.ToString("F1")     + "\n" +
            "Spells Cast:       "   + PadStat(s.spellsCast)                          + " | " + l.spellsCast           + "\n" +
            "Games Played:      "   + "           | " + l.totalGamesPlayed;
    }

    // right-pad a value so columns stay roughly aligned in a monospaced font
    private string PadStat(object value)
    {
        return value.ToString().PadRight(10);
    }

    // update the survival timer display — formats seconds into mm:ss
    public void UpdateTime(float timer)
    {
        float min = Mathf.FloorToInt(timer / 60f);
        float sec = Mathf.FloorToInt(timer % 60f);
        aliveTimeText.text = "Alive: " + min + ":" + sec.ToString("00");
    }
}
