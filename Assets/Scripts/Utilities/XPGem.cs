using Unity.Netcode;
using UnityEngine;

// Grants XP to the player when picked up, then destroys itself
public class XPGem : NetworkBehaviour
{
    [SerializeField] private int xpValue = 1;   // XP granted to the player on pickup

    // when the player walks into the gem, give them XP and remove the gem
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;
        player.GainExperience(xpValue);
        if (GameStats.GSInstance != null) GameStats.GSInstance.gemsGathered++;
        if (NetworkGameManager.IsSolo)
            Destroy(gameObject);
        else if (IsServer)
            NetworkObject.Despawn(true);
    }

    // decrement the spawner counter whether this gem was picked up or expired
    public override void OnDestroy()
    {
        if (ObjectSpawner.OSInstance != null)
            ObjectSpawner.OSInstance.OnObjectPickedUp(true);
    }
}
