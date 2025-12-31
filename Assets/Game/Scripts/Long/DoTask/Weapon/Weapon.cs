using UnityEngine;

public struct WeaponContext
{
    public Vector3 spawnPos;
    public Vector3 forward;
    public Transform owner;
}

public abstract class Weapon : UpgradableItem
{
    public string weaponName = "DefaultWeapon";
    [Header("Stats")]
    public float baseDamage = 10f;
    public float baseRange = 2f;
    public float baseSize = 1f;
    public float baseCooldown = 1f;

    [Header("Upgrade Level")]

    protected float timer = 0f;

    protected float cooldownMultiplier = 1f;
    public float Cooldown => baseCooldown * cooldownMultiplier;
    public void Tick(float dt, WeaponContext ctx)
    {
        timer += dt;
        if (timer >= Cooldown)
        {
            timer = 0f;
            Attack(ctx);
        }
    }

    public abstract void Attack(WeaponContext ctx);
    public void ApplyCooldownReduction(float percent)
    {
        cooldownMultiplier *= (1f - percent);
        cooldownMultiplier = Mathf.Clamp(cooldownMultiplier, 0.1f, 1f);
    }
}

// using UnityEngine;

// public abstract class Weapon : MonoBehaviour
// {
//     public string weaponName = "DefaultWeapon";

//     [Header("Stats")]
//     public float damage = 10f;
//     public float range = 2f;
//     public float size = 1f;
//     public float cooldown = 1f;

//     [Header("Upgrade Level")]
//     public int level = 1;

//     protected float timer = 0f;

//     public void Tick(float dt, Vector3 PlayPos)
//     {
//         timer += dt;
//         if (timer >= cooldown)
//         {
//             timer = 0f;
//             Attack(PlayPos);
//         }
//     }

//     public abstract void Attack(Vector3 PlayerPos);
//     public abstract void LevelUp();
// }
