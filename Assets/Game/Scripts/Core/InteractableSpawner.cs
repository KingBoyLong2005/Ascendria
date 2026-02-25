using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class InteractableSpawner
{
    //private readonly MapManager01 mapManager;
    private GameObject chestPrefab;
    private GameObject eventObjectMiniBoss;
    private GameObject eventObjectCaptureObject;
    private GameObject eventObjectGreedAltar;
    private GameObject eventObjectSacrificialAltar;
    private GameObject eventObjectTravelingMerchant;

    private float minDistance = 5.0f; //between object

    private struct SpawnRequest
    {
        public GameObject prefab;
        public int amount;

        public SpawnRequest(GameObject prefab, int amount)
        {
            this.prefab = prefab;
            this.amount = amount;
        }
    }

    public InteractableSpawner()
    {
        //this.mapManager = GameManager.Instance.GetMapManager;
        chestPrefab = PrefabDatabase.Instance.chest;
        eventObjectMiniBoss = PrefabDatabase.Instance.eventMiniBoss;
        eventObjectCaptureObject = PrefabDatabase.Instance.eventCaptureObject;
        eventObjectGreedAltar = PrefabDatabase.Instance.eventGreedAltar;
        eventObjectSacrificialAltar = PrefabDatabase.Instance.eventSacrificialAltar;
        eventObjectTravelingMerchant = PrefabDatabase.Instance.eventTravelingMerchant;
    }
        
    public void SpawnAll()
    {
        List<SpawnRequest> requests = new List<SpawnRequest>()
        {
            new SpawnRequest(chestPrefab, GameConfig.Instance.maxChestNums),
            new SpawnRequest(eventObjectMiniBoss, GameConfig.Instance.maxInteractObjectNum),
            new SpawnRequest(eventObjectCaptureObject, GameConfig.Instance.maxInteractObjectNum),
            new SpawnRequest(eventObjectGreedAltar, GameConfig.Instance.maxInteractObjectNum),
            new SpawnRequest(eventObjectSacrificialAltar, GameConfig.Instance.maxInteractObjectNum),
            new SpawnRequest(eventObjectTravelingMerchant, GameConfig.Instance.maxInteractObjectNum)
        };

        List<Vector3> finalPositions = new List<Vector3>();

        Dictionary<GameObject, List<Vector3>> result = new Dictionary<GameObject, List<Vector3>>();

        NavMeshTriangulation navMesh = NavMesh.CalculateTriangulation();

        foreach (var req in requests)
        {
            List<Vector3> positions = new List<Vector3>();
            GeneratePositions(finalPositions, positions, navMesh, req.amount);

            result.Add(req.prefab, positions);
        }

        foreach (var kv in result)
        {
            GameObject prefab = kv.Key;
            float yOffset = GetPrefabYOffset(prefab);

            foreach (Vector3 pos in kv.Value)
            {
                Vector3 spawnPos = pos + Vector3.up * yOffset;
                UnityEngine.Object.Instantiate(prefab, spawnPos, Quaternion.identity);
            }
        }
    }

    private void GeneratePositions(List<Vector3> finalPositions, List<Vector3> localList,
                                   NavMeshTriangulation navMesh, int amount)
    {
        int maxAttempts = amount * 30;
        int attempts = 0;

        while (localList.Count < amount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 p = GetRandomPointOnNavMesh(navMesh);

            //Kiểm tra overlap
            if (!IsValid(finalPositions, p)) continue;

            finalPositions.Add(p);
            localList.Add(p);
        }
    }

    private bool IsValid(List<Vector3> list, Vector3 p)
    {
        foreach (var pos in list)
        {
            if ((pos - p).sqrMagnitude < minDistance * minDistance)
                return false;
        }
        return true;
    }

    private Vector3 GetRandomPointOnNavMesh(NavMeshTriangulation navMesh)
    {
        int t = UnityEngine.Random.Range(0, navMesh.indices.Length / 3);
        int i0 = navMesh.indices[t * 3];
        int i1 = navMesh.indices[t * 3 + 1];
        int i2 = navMesh.indices[t * 3 + 2];

        Vector3 a = navMesh.vertices[i0];
        Vector3 b = navMesh.vertices[i1];
        Vector3 c = navMesh.vertices[i2];

        float r1 = Mathf.Sqrt(UnityEngine.Random.value);
        float r2 = UnityEngine.Random.value;

        return (1 - r1) * a + r1 * (1 - r2) * b + r1 * r2 * c;
    }

    private float GetPrefabYOffset(GameObject prefab)
    {
        var renderer = prefab.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.extents.y;
        }

        var collider = prefab.GetComponentInChildren<Collider>();
        if (collider != null)
        {
            return collider.bounds.extents.y;
        }

        return 0.5f; // fallback
    }
}
