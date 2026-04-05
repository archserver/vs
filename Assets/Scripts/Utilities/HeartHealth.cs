using Unity.Netcode;
using UnityEngine;

// Restores health to the player when picked up, then destroys itself
public class HeartHealth : NetworkBehaviour
{
    [SerializeField] private float healthValue = 1f;    // health restored to the player on pickup

    // when the player walks into the heart, heal them and remove the heart
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;
        player.HealPlayer(healthValue);
        if (GameStats.GSInstance != null) GameStats.GSInstance.heartsGathered++;
        if (NetworkGameManager.IsSolo)
            Destroy(gameObject);
        else if (IsServer)
            NetworkObject.Despawn(true);
    }

    // decrement the spawner counter whether this heart was picked up or expired
    public override void OnDestroy()
    {
        if (ObjectSpawner.OSInstance != null)
            ObjectSpawner.OSInstance.OnObjectPickedUp(false);
    }
}
