using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Handles the level up panel — randomly assign upgrades to spells and apply the player's choice
public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager LUMInstance;   // pointer so PlayerController can trigger it

    [SerializeField] private GameObject levelUpPanel;       // the panel to show on level up
    [SerializeField] private UpgradeCard[] upgradeCards;    // the 4 upgrade card buttons
    [SerializeField] private List<SpellUpgrade> upgradePool; // all possible upgrades to pick from

    // prevent duplicate instances when changing scenes
    private void Awake()
    {
        if (LUMInstance != null && LUMInstance != this)
            Destroy(this);
        else
            LUMInstance = this;
    }

    // hide the panel at start — keeping it active in the scene so Awake() always runs
    private void Start()
    {
        levelUpPanel.SetActive(false);
    }

    // called by PlayerController when the player levels up
    public void ShowLevelUp()
    {
        // Play LevelUo Audio
        AudioController.ACInstance.PlaySound(AudioController.ACInstance.levelup);
        // find all spells currently on the player
        Spell[] spells = PlayerController.PlayerInstance.GetComponentsInChildren<Spell>();
        if (spells.Length == 0 || upgradePool.Count == 0) return;

        // randomly assign an upgrade and spell to each of the 4 cards
        foreach (UpgradeCard card in upgradeCards)
        {
            SpellUpgrade randomUpgrade = upgradePool[Random.Range(0, upgradePool.Count)];
            Spell randomSpell = spells[Random.Range(0, spells.Length)];
            card.Setup(randomUpgrade, randomSpell);
        }

        // show the panel and pause the game
        levelUpPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // called by the selected UpgradeCard — apply the upgrade to the spell and resume
    public void ApplyUpgrade(SpellUpgrade upgrade, Spell spell)
    {
        switch (upgrade.property)
        {
            case UpgradeProperty.Damage:          spell.damage += upgrade.upgradeAmount;          break;
            case UpgradeProperty.CastFrequency:   spell.castFrequency -= upgrade.upgradeAmount;   break; // lower = casts more often
            case UpgradeProperty.Duration:        spell.duration += upgrade.upgradeAmount;        break;
            case UpgradeProperty.Range:           spell.range += upgrade.upgradeAmount;           break;
            case UpgradeProperty.DamageFrequency: spell.damageFrequency -= upgrade.upgradeAmount; break; // lower = ticks faster
            case UpgradeProperty.Force:           spell.force += upgrade.upgradeAmount;           break;
            case UpgradeProperty.FreezeDuration:  spell.freezeDuration += upgrade.upgradeAmount;  break;
            case UpgradeProperty.VampiricPercent: spell.vampiricPercent += upgrade.upgradeAmount; break;
            case UpgradeProperty.Speed:           spell.speed += upgrade.upgradeAmount;           break;
            case UpgradeProperty.Piercing:        spell.piercing += (int)upgrade.upgradeAmount;   break;
            case UpgradeProperty.ChainCount:      spell.chainCount += (int)upgrade.upgradeAmount; break;
            case UpgradeProperty.ChainRange:      spell.chainRange += upgrade.upgradeAmount;      break;
        }

        // hide the panel and resume the game
        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
