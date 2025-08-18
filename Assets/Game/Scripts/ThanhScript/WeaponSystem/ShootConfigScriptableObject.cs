using UnityEngine;

[CreateAssetMenu(fileName = "Shoot Config", menuName = "Weapons/Shoot Config", order = 4)]
public class ShootConfigScriptableObject : ScriptableObject
{
    public LayerMask HitMask;
    public Vector3 Spread = new Vector3(0.03f, 0.03f, 0.03f);
    public float FireRate = 0.25f;

    public Vector3 GetSpread()
    {
        Vector3 ShootDirect = new Vector3(Random.Range(-Spread.x, Spread.x),
                                          Random.Range(-Spread.y, Spread.y),
                                          Random.Range(-Spread.z, Spread.z));

        //ShootDirect.Normalize();

        return ShootDirect;
    }
}
