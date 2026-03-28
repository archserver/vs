using UnityEngine;

// Restores health to the player when picked up, then destroys itself
public class HeartHealth : MonoBehaviour
{
    [SerializeField] private float healthValue = 1f;    // health restored to the player on pickup

    // when the player walks into the heart, heal them and remove the heart
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController.PlayerInstance.HealPlayer(healthValue);
            GameStats.GSInstance.heartsGathered++;
            Destroy(gameObject);
        }
    }

    // decrement the spawner counter whether this heart was picked up or expired
    private void OnDestroy()
    {
        if (ObjectSpawner.OSInstance != null)
            ObjectSpawner.OSInstance.OnObjectPickedUp(false);
    }
}
