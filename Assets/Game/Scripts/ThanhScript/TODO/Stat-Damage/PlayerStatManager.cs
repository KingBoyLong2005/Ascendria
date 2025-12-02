// PlayerStatManager.cs — handles player stats & damage
using UnityEngine;

public class PlayerStatManager : MonoBehaviour
{
    public static PlayerStatManager Instance { get; private set; }

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentHealth = maxHealth;
    }

    public void ApplyDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("Player took damage: " + damage + ", current health: " + currentHealth);
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died");
        // handle death: game over, respawn, etc.
    }
}

