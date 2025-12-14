// PlayerStatManager.cs � handles player stats & damage
using UnityEngine;

public class PlayerStatManager : MonoBehaviour
{
    public static PlayerStatManager Instance { get; private set; }

    [Header("Base Stats")]
    public float baseMaxHealth = 100f;
    public float baseAttack = 10f;
    public float baseMoveSpeed = 5f;
    public float baseArmor = 5f;

    // Current / effective stats
    private float currentHealth;

    // Modifiers (flat and/or multiplicative) for extensibility
    private float healthModifierFlat = 0f;
    private float healthModifierMult = 1f;

    private float attackModifierFlat = 0f;
    private float attackModifierMult = 1f;

    private float moveSpeedModifierFlat = 0f;
    private float moveSpeedModifierMult = 1f;

    private float armorModifierFlat = 0f;
    private float armorModifierMult = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentHealth = baseMaxHealth;
    }

    public void TakeDamage(float damage)
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
        Debug.Log($"Player died with: {currentHealth} left");
        // handle death: game over, respawn, etc.
    }

    // HEALTH GETTER
    public float MaxHealth
    {
        get
        {
            return (baseMaxHealth + healthModifierFlat) * healthModifierMult;
        }
    }
    // DAMAGE GETTER
    public float Attack
    {
        get
        {
            return (baseAttack + attackModifierFlat) * attackModifierMult;
        }
    }
    // MOVE SPEED GETTER
    public float MoveSpeed
    {
        get
        {
            return (baseMoveSpeed + moveSpeedModifierFlat) * moveSpeedModifierMult;
        }
    }
    // ARMOR GETTER
    public float Armor
    {
        get
        {
            return (baseArmor + armorModifierFlat) * armorModifierMult;
        }
    }

    // STAT MODIFYING METHODS
    public void ModifyHealth(float addFlat = 0f, float mult = 1f)
    {
        healthModifierFlat += addFlat;
        healthModifierMult *= mult;
        // Phải tăng cả currentHealth chứ
        Debug.Log($"MaxHealth mới: {MaxHealth}, Current: {currentHealth}");
    }
    public void ModifyAttack(float addFlat = 0f, float mult = 1f)
    {
        attackModifierFlat += addFlat;
        attackModifierMult *= mult;
    }
    public void ModifyMoveSpeed(float addFlat = 0f, float mult = 1f)
    {
        moveSpeedModifierFlat += addFlat;
        moveSpeedModifierMult *= mult;
    }
    public void ModifyArmor(float addFlat = 0f, float mult = 1f)
    {
        armorModifierFlat += addFlat;
        armorModifierMult *= mult;
    }
}

