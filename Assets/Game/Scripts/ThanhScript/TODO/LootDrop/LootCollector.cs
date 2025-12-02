// LootCollector.cs — attach to player (or child of player)
using UnityEngine;

public class LootCollector : MonoBehaviour
{
    public float collectRadius = 5f;

    void Update()
    {
        // Scan for loot within radius
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            collectRadius,
            LayerMask.GetMask("Loot")  // assume loot objects are on “Loot” layer
        );

        foreach (var c in hits)
        {
            Loot loot = c.GetComponent<Loot>();
            if (loot != null)
            {
                loot.Magnetize(transform);
            }
        }
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}

