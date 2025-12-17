using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Canvas uiCanvas;
    private GameObject healthBarPrefab;
    private HealthBar healthBar;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadPrefabsFromDatabase();
    }

    private void LoadPrefabsFromDatabase()
    {
        if (PrefabDatabase.Instance.healthBarPrefab != null)
        {
            healthBarPrefab = PrefabDatabase.Instance.healthBarPrefab;
            healthBar = healthBarPrefab.GetComponent<HealthBar>();
            Debug.Log("Health Bar added from db to manager");
        }
        //if (prefabDatabase.enemyPrefab2 != null)
        //    enemyPrefabList.Add(prefabDatabase.enemyPrefab2);
        //// Repeat for all prefab fields — or use reflection/array if many
    }
}
