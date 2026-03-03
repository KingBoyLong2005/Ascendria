using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [SerializeField] private bool _addToDontDestroyOnLoad = false;

    private GameObject _emptyHolder;
    private static GameObject _gameObjectEmpty;
    private static GameObject _particleSystemEmpty;
    private static GameObject _soundFxEmpty;

    // Map: prefab -> pool tương ứng
    private static Dictionary<GameObject, ObjectPool<GameObject>> _poolMap;
    private static Dictionary<GameObject, GameObject> _cloneToPrefabMap;

    public enum PoolType{
        GameObject,
        ParticleSystem,
        SoundFx,
    }

    public static PoolType poolingType;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _poolMap = new Dictionary<GameObject, ObjectPool<GameObject>>();
        _cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

        SetupEmpties();
    }

    private void SetupEmpties()
    {
        _emptyHolder = new GameObject("Object Pools");

        _gameObjectEmpty = new GameObject("Game Objects");
        _gameObjectEmpty.transform.SetParent(_emptyHolder.transform);

        _particleSystemEmpty = new GameObject("Particle Systems");
        _particleSystemEmpty.transform.SetParent(_emptyHolder.transform);

        _soundFxEmpty = new GameObject("Sound FXs");
        _soundFxEmpty.transform.SetParent(_emptyHolder.transform);

        if(_addToDontDestroyOnLoad)
            DontDestroyOnLoad(_particleSystemEmpty.transform.root);
    }

    // ===============================
    //  CREATE POOL
    // ===============================
    private static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot, PoolType pt = PoolType.GameObject)
    {
        if (_poolMap.ContainsKey(prefab))
            return; // Đã có pool thì bỏ qua

        // Tạo pool cho prefab này
        var pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObject(prefab, pos, rot, pt),          // Factory
            actionOnGet: TakeFromPool,         // OnGet
            actionOnRelease: ReturnedToPool,       // OnRelease
            actionOnDestroy: DestroyObject              // Destroy pooled object
        );

        _poolMap.Add(prefab, pool);
    }

    // ===============================
    //  SPAWN
    // ===============================
    private static T Spawn<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRot, PoolType pt = PoolType.GameObject) where T : Object
    {
        // Nếu chưa có pool → tự động tạo
        if (!_poolMap.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, spawnPos, spawnRot, pt);
            Debug.LogWarning($"Created pool for prefab: {objectToSpawn.name}");
        }
           
        var obj = _poolMap[objectToSpawn].Get();
        if (obj != null)
        {
            if (!_cloneToPrefabMap.ContainsKey(obj))
            {
                _cloneToPrefabMap.Add(obj, objectToSpawn);
            }

            obj.transform.position = spawnPos;
            obj.transform.rotation = spawnRot;
            obj.SetActive(true);

            if(typeof(T) == typeof(GameObject))
            {
                return obj as T;
            }

            T component = obj.GetComponent<T>();
            if (component == null) 
            {
                Debug.LogError($"Object {objectToSpawn.name} doesn't have component of type {typeof(T)}"); 
                return null;
            }

            return component;
        }

        return null;
    }

    public static T Spawn<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRot, PoolType pt = PoolType.GameObject) where T : Component
    {
        return Spawn<T>(typePrefab.gameObject, spawnPos, spawnRot, pt);
    }

    public static GameObject Spawn(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRot, PoolType pt = PoolType.GameObject)
    {
        return Spawn<GameObject>(objectToSpawn, spawnPos, spawnRot, pt);
    }

    // ===============================
    //  DESPAWN
    // ===============================
    public static void Despawn(GameObject obj, PoolType pt = PoolType.GameObject)
    {
        if (_cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
        {
            GameObject parentObj = SetParentObject(pt);

            if (obj.transform.parent != parentObj.transform) 
            { 
                obj.transform.SetParent(parentObj.transform);
            }

            if(_poolMap.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
        }
        else
        {
            Debug.LogWarning("Trying to return an object that is not pooled: " + obj.name);
        }
    }

    // ===============================
    //  CALLBACKS
    // ===============================

    private static GameObject CreateObject(GameObject prefab, Vector3 pos, Quaternion rot, PoolType pt = PoolType.GameObject)
    {
        // prefab.SetActive(false);
        
        var obj = Instantiate(prefab, pos, rot);
        prefab.SetActive(false);
        // prefab.SetActive(true);

        GameObject parentObject = SetParentObject(pt);
        obj.transform.SetParent(parentObject.transform);

        return obj;
    }

    private static void TakeFromPool(GameObject obj)
    {
        obj.SetActive(true);
    }

    private static void ReturnedToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    private static void DestroyObject(GameObject obj)
    {
        if (_cloneToPrefabMap.ContainsKey(obj))
        {
            _cloneToPrefabMap.Remove(obj);
        }
    }

    private static GameObject SetParentObject(PoolType pt) 
    {
        switch (pt) { 
            case PoolType.GameObject:
                return _gameObjectEmpty;
            case PoolType.ParticleSystem:
                return _particleSystemEmpty;
            case PoolType.SoundFx:
                return _soundFxEmpty;
            default:
                return null;
        }
    }
}
