// Combined MeleeWeaponBehaviour + temporary hitbox creation
// This file merges hitbox logic into the MeleeWeaponBehaviour so you don't need a separate MeleeHitbox prefab.
// It still supports using a visual prefab (data.prefab) for the slash effect — if you have an animated prefab, it will be instantiated but the damage is controlled here.
// Original uploaded file (for reference): /mnt/data/MeleeWeaponBehaviour.cs

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class MeleeWeaponBehaviour : MonoBehaviour
{
    [Header("Data")]
    public WeaponData data; // WeaponData ScriptableObject with baseDamage, range, attackRate, prefab (optional visual)

    [Header("Spawn Settings")]
    [Tooltip("How many small hitbox segments to spawn around the player. Higher = denser coverage.")]
    public int segments = 8;
    [Tooltip("Radius multiplier applied when placing hitboxes around the player. 1 = data.range.")]
    public float radiusMultiplier = 1f;
    [Tooltip("Vertical offset (Y) to spawn hitboxes at so they aren't under the floor")]
    public float yOffset = 0.6f;
    [Tooltip("How long a spawned hitbox object lives in seconds (should be short, e.g. 0.08 - 0.16)")]
    public float hitboxLife = 0.12f;

    [Header("Damage options")]
    [Tooltip("If true, each enemy can only take damage once PER ATTACK (recommended)")]
    public bool oneHitPerEnemyPerAttack = true;

    // internal
    private float lastAttackTime = -999f;

    // used to ensure each enemy only damaged once per attack when not using separate hitbox component
    private HashSet<int> alreadyDamagedThisAttack = new HashSet<int>();

    // Use this method to trigger the melee attack. "user" is typically the player transform.
    public void TryUse(Transform user, Vector3 aimDirection)
    {
        if (data == null) return;
        float cooldown = 1f / Mathf.Max(0.0001f, data.attackRate);
        if (Time.time < lastAttackTime + cooldown) return;

        lastAttackTime = Time.time;
        alreadyDamagedThisAttack.Clear();

        Vector3 center = user.position;
        center.y += yOffset;

        float spawnRadius = data.range * radiusMultiplier;

        // compute aiming yaw so we can optionally limit spawn to forward arc later
        float aimYaw = Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg;

        int seg = Mathf.Max(1, segments);
        for (int i = 0; i < seg; i++)
        {
            float angle = i * (360f / seg);

            // skip behind if you want front-only (uncomment and tweak threshold)
            float diff = Mathf.DeltaAngle(angle, aimYaw);
            if (Mathf.Abs(diff) > 100f) continue; // only spawn within ±100° of aim

            Quaternion rot = Quaternion.Euler(0f, angle, 0f);
            Vector3 dir = rot * Vector3.forward;
            Vector3 spawnPos = center + dir * (spawnRadius * 0.5f);

            // create a temporary hitbox GameObject (no need for prefab)
            GameObject hb = new GameObject("temp_hitbox");
            hb.transform.position = spawnPos;
            hb.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            hb.transform.localScale = Vector3.one * (spawnRadius * 0.5f);

            // add trigger collider
            SphereCollider sc = hb.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            // radius scaled by scale; use 1 unit for base and scale object instead
            sc.radius = 0.5f;

            // add the helper component that will call back to this script for damage
            TemporaryHitbox th = hb.AddComponent<TemporaryHitbox>();
            th.Setup(this, data.baseDamage, hitboxLife, LayerMask.GetMask("Enemy"), oneHitPerEnemyPerAttack);

            // spawn visual if provided (visual shouldn't carry collider/Rigidbody)
            if (data.prefab != null)
            {
                GameObject v = Instantiate(data.prefab, spawnPos, hb.transform.rotation, hb.transform);
                // if prefab has animator/sprite animation, let it play; optionally destroy with hitbox
            }

            // destroy hitbox root after life seconds (TemporaryHitbox also destroys self on expiry)
            Destroy(hb, hitboxLife + 0.05f);
        }
    }

    // Called by TemporaryHitbox when it detects an enemy collider; centralizes damage rules
    internal void ApplyDamageOnce(GameObject enemyObj, float damageAmount, bool perHitbox)
    {
        if (enemyObj == null) return;
        int id = enemyObj.GetInstanceID();
        if (oneHitPerEnemyPerAttack && alreadyDamagedThisAttack.Contains(id)) return;
        alreadyDamagedThisAttack.Add(id);

        var enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            // enemy.TakeDamage(damageAmount);
        }
    }

    // helper class that runs on each temporary hitbox instance
    class TemporaryHitbox : MonoBehaviour
    {
        MeleeWeaponBehaviour owner;
        float damage;
        float life;
        LayerMask enemyLayer;
        bool perHitboxOnly;

        public void Setup(MeleeWeaponBehaviour owner, float damage, float life, LayerMask enemyLayer, bool perHitboxOnly)
        {
            this.owner = owner;
            this.damage = damage;
            this.life = life;
            this.enemyLayer = enemyLayer;
            this.perHitboxOnly = perHitboxOnly;
            // schedule destroy
            Destroy(this.gameObject, life);
            // immediate overlap to hit enemies that are already inside
            DoInitialOverlap();
        }

        void DoInitialOverlap()
        {
            float r = GetComponent<SphereCollider>() != null ? (GetComponent<SphereCollider>().radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z)) : 0.5f;
            Collider[] hits = Physics.OverlapSphere(transform.position, r, enemyLayer);
            foreach (var c in hits)
            {
                if (c == null) continue;
                // call owner to apply damage
                owner.ApplyDamageOnce(c.gameObject, damage, perHitboxOnly);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;
            owner.ApplyDamageOnce(other.gameObject, damage, perHitboxOnly);
        }
    }
}
