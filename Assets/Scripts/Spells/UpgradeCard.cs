using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Attached to each upgrade button — displays one upgrade option and applies it when clicked
public class UpgradeCard : MonoBehaviour
{
    [SerializeField] private Image icon;                // card icon image
    [SerializeField] private TMP_Text upgradeNameText;  // upgrade name label
    [SerializeField] private TMP_Text spellNameText;    // which spell will be upgraded
    [SerializeField] private TMP_Text descriptionText;  // upgrade description label

    private SpellUpgrade upgrade;   // the upgrade this card currently represents
    private Spell targetSpell;      // the spell this upgrade will be applied to

    // populate this card with a randomly assigned upgrade and spell
    public void Setup(SpellUpgrade upgrade, Spell spell)
    {
        this.upgrade = upgrade;
        this.targetSpell = spell;

        icon.sprite = upgrade.icon;
        upgradeNameText.text = upgrade.upgradeName;
        spellNameText.text = spell.displayName;
        descriptionText.text = upgrade.description;
    }

    // called by the button's OnClick — apply upgrade and close the level up panel
    public void OnSelected()
    {
        LevelUpManager.LUMInstance.ApplyUpgrade(upgrade, targetSpell);
    }
}
