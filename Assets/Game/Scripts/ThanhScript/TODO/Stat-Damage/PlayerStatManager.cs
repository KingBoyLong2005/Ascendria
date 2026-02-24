// PlayerStatManager.cs � handles player stats & damage
using System;
using StarterAssets;
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
    private float baseDiscard = 2f;

    public float baseDamage = 0f;
    public float baseLuck = 0f;
    public float baseWealth = 0f;
    public float baseWise = 0f;
    public bool activeHpRegen = false;
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

    private float damageModifierFlat = 0f;
    private float damageModifierMult = 1f;

    private float luckModifierFlat = 0f;
    private float luckModifierMult = 1f;

    private float wealthModifierFlat = 0f;
    private float wealthModifierMult = 1f;

    private float wiseModifierFlat = 0f;
    private float wiseModifierMult = 1f;

    private float coinModifierFlat = 0f;
    private float coinModifierMult = 1f;

    private float discardModifierFlat = 0f;
    private float discardModifierMult = 1f;

    private ProfileCharacterLoader loaderProfile;

    float timer;
    public float ValueHpRegenPerSecond = 1;
    private void Awake()
    {
        loaderProfile = FindFirstObjectByType<ProfileCharacterLoader>();
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        ApplyStats();
        currentHealth = baseMaxHealth;
        OnPlayerHealthChange?.Invoke(this, new OnPlayerHealthChangeEventArgs(currentHealth, baseMaxHealth));
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f && activeHpRegen)
        {
            timer = 0f;
            RestoreHealth(ValueHpRegenPerSecond); // ✅ 1 lần/giây
        }
    }
    private void ApplyStats()
    {
        baseMaxHealth = loaderProfile.MaxHP;
        baseAttack = loaderProfile.attack;
        baseMoveSpeed = loaderProfile.moveSpeed;
        baseArmor = loaderProfile.armor;

        baseLuck = loaderProfile.luck;
        baseWealth = loaderProfile.wealth;
        baseWise = loaderProfile.wise;
        // PlayerMoveManager.Instance.airJumpActive = false;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnPlayerHealthChange?.Invoke(this, new OnPlayerHealthChangeEventArgs(currentHealth, MaxHealth));
        Debug.Log("Player took damage: " + damage + ", current health: " + currentHealth);

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
        FindFirstObjectByType<GameOver>().GameOverActive();
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
    // Damage
    public float Damage
    {
        get
        {
            return(baseDamage + damageModifierFlat) * damageModifierMult;
        }
    }
    // LUCK
    public float Luck
    {
        get
        {
            return (baseLuck + luckModifierFlat) * luckModifierMult;
        }
    }

    // WEALTH
    public float Wealth
    {
        get
        {
            return (baseWealth + wealthModifierFlat) * wealthModifierMult;
        }
    }

    // WISE
    public float Wise
    {
        get
        {
            return (baseWise + wiseModifierFlat) * wiseModifierMult;
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
    // ITEMS DISCARD ATTEMPT
    public float Discard
    {
        get
        {
            return (baseDiscard + discardModifierFlat) * discardModifierMult;
        }
    }

    // STAT MODIFYING METHODS
    public void ModifyHealth(float addFlat = 0f, float mult = 1f)
    {
        healthModifierFlat += addFlat;
        healthModifierMult *= mult;
        // hồi máu khi tăng máu
        RestoreHealth(addFlat);
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
    public void ModifyDamage(float addFlat = 0f, float mult = 1f)
    {
        damageModifierFlat += addFlat;
        damageModifierMult += mult;
    }
    public void ModifyLuck(float addFlat = 0f, float mult = 1f)
    {
        luckModifierFlat += addFlat;
        luckModifierMult *= mult;
    }

    public void ModifyWealth(float addFlat = 0f, float mult = 1f)
    {
        wealthModifierFlat += addFlat;
        wealthModifierMult *= mult;
    }

    public void ModifyWise(float addFlat = 0f, float mult = 1f)
    {
        wiseModifierFlat += addFlat;
        wiseModifierMult *= mult;
    }

    public void ModifyDiscard(float addFlat = 0f, float mult = 1f)
    {
        discardModifierFlat += addFlat;
        discardModifierMult *= mult;
    }
}

