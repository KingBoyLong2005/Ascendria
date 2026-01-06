// PlayerStatManager.cs � handles player stats & damage
using System;
using UnityEngine;

public class PlayerStatManager : MonoBehaviour
{
    public static PlayerStatManager Instance { get; private set; }

    [Header("Base Stats")]
    public float baseMaxHealth = 100f;
    public float baseAttack = 10f;
    public float baseMoveSpeed = 5f;
    public float baseArmor = 5f;
    private float baseCoin = 5f;

    public event EventHandler<OnPlayerHealthChangeEventArgs> OnPlayerHealthChange;
    public class OnPlayerHealthChangeEventArgs : EventArgs
    {
        public float currentHealth;
        public float maxHealth;
        public OnPlayerHealthChangeEventArgs(float current, float max)
        {
            currentHealth = current;
            maxHealth = max;
        }
    }

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

    private float coinModifierFlat = 0f;
    private float coinModifierMult = 1f;
    private ProfileCharacterLoader loaderProfile;

    private void Awake()
    {
        loaderProfile = FindFirstObjectByType<ProfileCharacterLoader>();
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        ApplyStats();
        currentHealth = baseMaxHealth;
        OnPlayerHealthChange?.Invoke(this, new OnPlayerHealthChangeEventArgs(currentHealth, baseMaxHealth));
    }
    private void ApplyStats()
    {
        baseMaxHealth = loaderProfile.MaxHP ;
        Instance.baseMoveSpeed = loaderProfile.MoveSpeed;
        // PlayerStatManager.Instance.base = profile.attackSpeed;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnPlayerHealthChange?.Invoke(this, new OnPlayerHealthChangeEventArgs(currentHealth, MaxHealth));
        //Debug.Log("Player took damage: " + damage + ", current health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void RestoreHealth(float amount)
    {
        currentHealth += amount;
        
        //Debug.Log("Player heal: " + amount + ", current health: " + currentHealth);
        //Debug.Log("Max health: " + MaxHealth);

        if (currentHealth >= MaxHealth)
        {
            currentHealth = MaxHealth;
        }

        OnPlayerHealthChange?.Invoke(this, new OnPlayerHealthChangeEventArgs(currentHealth, MaxHealth));
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
    // COIN GETTER
    public float Coin
    {
        get
        {
            return (baseCoin + coinModifierFlat) * coinModifierMult;
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
    public void ModifyCoin(float addFlat = 0f, float mult = 1f)
    {
        coinModifierFlat += addFlat;
        coinModifierMult *= mult;
    }
}

