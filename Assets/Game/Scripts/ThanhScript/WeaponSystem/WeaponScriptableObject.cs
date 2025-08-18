//using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Weapon", order = 1)]
public class WeaponScriptableObject : ScriptableObject
{
    //public ImpactTypae impactType;
    public WeaponType Type;
    public string Name;
    public GameObject Prefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public ShootConfigScriptableObject ShootConfig;
    public TrailConfigScriptableObject TrailConfig;

    private MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private float LastShootTime;
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour; 
        LastShootTime = 0;
        TrailPool = new ObjectPool<TrailRenderer> (CreateTrail);

        Model = Instantiate (Prefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);
        
        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
    }

    public TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");

        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }

    public void Shoot() 
    {
        float ShootCooldown = 1/ShootConfig.FireRate;

        if(Time.time > LastShootTime + ShootCooldown)
        {
            LastShootTime = Time.time;

            ShootSystem.Play();

            Vector3 spreadAmount = ShootConfig.GetSpread();
            Vector3 ShootDirect = Model.transform.forward + spreadAmount;

            if (Physics.Raycast(ShootSystem.transform.position,
                                ShootDirect,
                                out RaycastHit hit,
                                float.MaxValue,
                                ShootConfig.HitMask))
            {
                ActiveMonoBehaviour.StartCoroutine(
                    PlayTrail(ShootSystem.transform.position, 
                              hit.point, 
                              hit)
                    );
            }
            else 
            {
                ActiveMonoBehaviour.StartCoroutine(
                    PlayTrail(ShootSystem.transform.position, 
                              ShootSystem.transform.position + (ShootDirect * TrailConfig.MissDistance), 
                              new RaycastHit())
                    );
            }
        }
    }
    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;
        yield return null;

        instance.emitting = true;

        float distance = Vector3.Distance(startPoint, endPoint);
        float remainDistance = distance;
        while (remainDistance > 0) 
        {
            instance.transform.position = Vector3.Lerp(startPoint, 
                                                       endPoint, 
                                                       Mathf.Clamp01(1 - (remainDistance/distance)));
            remainDistance-= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = endPoint;

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;

        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);

    }
}
