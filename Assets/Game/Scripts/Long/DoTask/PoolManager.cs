using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    // Map: prefab -> pool tương ứng
    private Dictionary<GameObject, IObjectPool<GameObject>> poolMap = new Dictionary<GameObject, IObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ===============================
    //  CREATE POOL
    // ===============================
    public void CreatePool(GameObject prefab, int defaultCapacity = 10, int maxSize = 100)
    {
        if (poolMap.ContainsKey(prefab))
            return; // Đã có pool thì bỏ qua

        // Tạo pool cho prefab này
        var pool = new ObjectPool<GameObject>(
            () => CreateObject(prefab),          // Factory
            obj => OnTakeFromPool(obj),         // OnGet
            obj => OnReturnedToPool(obj),       // OnRelease
            obj => Destroy(obj),                // Destroy pooled object
            false,
            defaultCapacity,
            maxSize
        );

        poolMap.Add(prefab, pool);
    }

    // ===============================
    //  SPAWN
    // ===============================
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // Nếu chưa có pool → tự động tạo
        if (!poolMap.ContainsKey(prefab))
            CreatePool(prefab);

        var obj = poolMap[prefab].Get();
        obj.transform.SetPositionAndRotation(position, rotation);

        return obj;
    }

    // ===============================
    //  DESPAWN
    // ===============================
    public void Despawn(GameObject prefab, GameObject instance)
    {
        if (!poolMap.ContainsKey(prefab))
        {
            Debug.LogWarning($"PoolManager: Không có pool cho prefab {prefab.name}");
            Destroy(instance);
            return;
        }

        poolMap[prefab].Release(instance);
    }

    // ===============================
    //  CALLBACKS
    // ===============================

    private GameObject CreateObject(GameObject prefab)
    {
        var obj = Instantiate(prefab);
        obj.SetActive(false);
        return obj;
    }

    private void OnTakeFromPool(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void OnReturnedToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}
