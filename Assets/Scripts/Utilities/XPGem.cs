using UnityEngine;

// Grants XP to the player when picked up, then destroys itself
public class XPGem : MonoBehaviour
{
    [SerializeField] private int xpValue = 1;   // XP granted to the player on pickup

    // when the player walks into the gem, give them XP and remove the gem
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController.PlayerInstance.GainExperience(xpValue);
            GameStats.GSInstance.gemsGathered++;
            Destroy(gameObject);
        }
    }

    // decrement the spawner counter whether this gem was picked up or expired
    private void OnDestroy()
    {
        if (ObjectSpawner.OSInstance != null)
            ObjectSpawner.OSInstance.OnObjectPickedUp(true);
    }
}
