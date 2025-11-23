using UnityEngine;

public class ProjectileWeaponBehaviour : MonoBehaviour
{
    public WeaponData data;
    private float lastFire;

    public void TryUse(Transform user, Vector3 aimDirection)
    {
        if (Time.time < lastFire + (1f / data.attackRate)) return;
        if (data.prefab == null) return;

        // Spawn projectile at user's position (hoặc vị trí firepoint)
        GameObject go = Instantiate(data.prefab, user.position, Quaternion.identity);
        var proj = go.GetComponent<Projectile>();
        if (proj != null)
            proj.Initialize(aimDirection.normalized, data.baseDamage, user.gameObject);
        // nếu projectile không có script, add velocity:
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = aimDirection.normalized * 20f; // tốc độ sample

        lastFire = Time.time;
    }
}
