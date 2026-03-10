using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Weapons/Sword")]
public class Sword : Weapon
{
    public float knockbackForce = 0f;
    
    public GameObject slashEffectPrefab;
    public LayerMask enemyMask;

    public int projectileCount = 1;

    private struct UpgradeConfig
    {
        public int count;
        public float damage, projectiles, knockback, sizeMultiplier;
        public UpgradeConfig(int c, float dmg, float proj, float kb, float size)
        {
            count = c; damage = dmg; projectiles = proj; 
            knockback = kb; sizeMultiplier = size;
        }
    }

    private static readonly Dictionary<Rarity, UpgradeConfig> configs = new()
    {
        { Rarity.Common,     new(1, 2f, 1f, 0.5f, 1.20f) },
        { Rarity.Uncommon,   new(1, 2.4f, 1f, 0.6f, 1.24f) },
        { Rarity.Rare,       new(2, 2.8f, 1f, 0.7f, 1.28f) },
        { Rarity.Epic,       new(2, 3.2f, 2f, 0.8f, 1.32f) },
        { Rarity.Legendary,  new(2, 4f, 2f, 1f, 1.40f) }
    };

    public override void Attack(WeaponContext ctx)
    {
        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset = projectileCount > 1 
                ? Random.Range(-30f, 30f) 
                : 0f;
            
            Quaternion rot = Quaternion.LookRotation(ctx.forward) * Quaternion.Euler(0f, angleOffset, 0f);
            Vector3 sizeBox = new Vector3(baseSize, 0.25f, baseRange);

            HitBoxManager.Instance.RequestBox(ctx.spawnPos, sizeBox, rot, enemyMask, 
                (col) => OnHitEnemy(col, ctx));
            AudioManager.Instance.PlaySFX(PrefabDatabase.Instance.swordSfx);
            if (slashEffectPrefab != null)
            {
                GameObject fx = Instantiate(slashEffectPrefab, ctx.spawnPos, 
                    rot * Quaternion.Euler(90f, 0f, -60f));
                fx.transform.localScale = new Vector3(baseSize, baseSize, baseRange);
                Destroy(fx, 0.4f);
            }
        }
    }

    private void OnHitEnemy(Collider col, WeaponContext ctx)
    {
        var enemy = col.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, baseDamage);
            
            if (knockbackForce > 0f)
            {
                var rb = enemy.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (enemy.transform.position - ctx.owner.position).normalized;
                    dir.y = 0f;
                    rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
                }
            }
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return;
        ApplyUpgrade(cfg, level);
        level++;
    }

    private void ApplyUpgrade(UpgradeConfig cfg, int seed)
    {
        var stats = UpgradeHelper.GetRandomStats(cfg.count, 4, seed);
        foreach (int stat in stats)
        {
            switch (stat)
            {
                case 0: baseDamage += cfg.damage; break;
                case 1: projectileCount += Mathf.RoundToInt(cfg.projectiles); break;
                case 2: knockbackForce += cfg.knockback; break;
                case 3: 
                    baseSize *= cfg.sizeMultiplier;
                    baseRange *= cfg.sizeMultiplier;
                    break;
            }
        }
    }

    public override List<string> GetUpgradePreview(Rarity rarity, int seed)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return new List<string> { "Unknown" };

        var stats = UpgradeHelper.GetRandomStats(cfg.count, 4, seed);
        var preview = new List<string>();
        
        foreach (int stat in stats)
        {
            preview.Add(stat switch
            {
                0 => $"+{cfg.damage} Damage",
                1 => $"+{Mathf.RoundToInt(cfg.projectiles)} Slash",
                2 => $"+{cfg.knockback} Knockback",
                3 => $"{UpgradeHelper.FormatPercent(cfg.sizeMultiplier)} Size",
                _ => "Unknown"
            });
        }
        
        return preview;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return "Unknown";
        return $"Random {cfg.count} of: Damage, Slashes, Knockback, Size";
    }

    // ==================== PROJECTILE MANAGEMENT ====================
    public override bool TryAddProjectile(int amount)
    {
        projectileCount += amount;
        Debug.Log($"<color=cyan>⚔️ {weaponName}: {projectileCount - amount} → {projectileCount} slashes</color>");
        return true;
    }

    public override bool TryRemoveProjectile(int amount)
    {
        int oldCount = projectileCount;
        projectileCount = Mathf.Max(1, projectileCount - amount);
        Debug.Log($"<color=orange>⚔️ {weaponName}: {oldCount} → {projectileCount} slashes</color>");
        return true;
    }
}