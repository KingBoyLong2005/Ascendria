using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Sword")]
public class Sword : Weapon
{
    public GameObject slashEffectPrefab;
    public LayerMask enemyMask;
    

    public override void Attack(WeaponContext ctx)
    {
        Vector3 forward = ctx.forward;
        Vector3 hitboxCenter = ctx.spawnPos;

        Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);
        Quaternion offset = Quaternion.Euler(90f, 0f, -60f);

        Vector3 boxSize = new Vector3(size, 0.25f, range);

        // --- HITBOX ---
        Collider[] hits = Physics.OverlapBox(
            hitboxCenter,
            boxSize * 0.5f,
            rot,
            enemyMask
        );


        foreach (var h in hits)
        {
            var e = h.GetComponent<EnemyAI>();
            if (e != null)
                e.TakeDamage(damage);
        }

        // --- EFFECT ---
        if (slashEffectPrefab != null)
        {
            GameObject fx = Instantiate(
                slashEffectPrefab,
                hitboxCenter,
                rot * offset
            );
            Destroy(fx, 0.4f);
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        
        switch (rarity)
    {
        case Rarity.Common:
            damage += 2f;
            range += 0.2f;
            cooldown *= 0.98f;  // Giảm cooldown nhẹ
            size += 1f;
            break;

        case Rarity.Uncommon:
            damage += 4f;
            range += 0.3f;
            cooldown *= 0.96f;
            size += 1f;
            break;

        case Rarity.Rare:
            damage += 7f;
            range += 0.5f;
            cooldown *= 0.93f;
            size += 1.5f;
            break;

        case Rarity.Epic:
            damage += 12f;
            range += 0.8f;
            cooldown *= 0.90f;
            size += 2f;
            break;

        case Rarity.Legendary:
            damage += 20f;
            range += 1.2f;
            cooldown *= 0.85f; 
            size += 3f;
            break;
    }

    level++;
    }
}

// using UnityEngine;

// public class Sword : Weapon
// {
//     public GameObject slashEffectPrefab;
//     public LayerMask enemyMask;

//     void Start()
//     {
        
//     }
//     public override void Attack(Vector3 PlayerPos)
//     {
//         Vector3 forward = transform.forward;

//         // Tính vị trí spawn
//         Vector3 hitboxCenter = transform.position + forward * (range * 0.5f);
//         Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);

//         Vector3 boxSize = new Vector3(size, 0.25f, range);

//         // Hitbox
//         Collider[] hits = Physics.OverlapBox(hitboxCenter, boxSize * 0.5f, rot, enemyMask);

//         foreach (var h in hits)
//         {
//             var e = h.GetComponent<EnemyAI>();
//             if (e != null)
//                 e.TakeDamage(damage);
//         }
//         Quaternion offset = Quaternion.Euler(90f, 0f, 0f);
//         // Hiệu ứng chém
//         if (slashEffectPrefab != null)
//         {
//             GameObject fx = Instantiate(slashEffectPrefab, PlayerPos, rot* offset);
//             Destroy(fx, 0.4f);
//         }
//     }

//     public override void LevelUp()
//     {
//         level++;
//         damage *= 1.25f;
//         range += 0.1f;
//         cooldown *= 0.9f;
//     }
// }
