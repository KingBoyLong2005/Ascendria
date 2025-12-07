using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshManager : MonoBehaviour
{
    [Header("Prefab Option")]
    public static NavMeshManager Instance {get ; private set;}
    public GameObject navMeshPrefab; // Prefab có NavMeshSurface
    public bool spawnPrefabSurface = false; // Nếu true: tạo prefab mới
    public bool useExistingSurface = true; // Nếu true: dùng NavMeshSurface hiện có trong scene

    private NavMeshSurface surface;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void LoadNavMesh()
    {
        if (spawnPrefabSurface && navMeshPrefab != null)
        {
            GameObject instance = Instantiate(navMeshPrefab);
            surface = instance.GetComponent<NavMeshSurface>();
            Debug.Log("Prefab có NavMeshSurface");
            if (surface == null)
            {
                Debug.LogError("Prefab không có NavMeshSurface!");
                return;
            }
        }
        else if (useExistingSurface)
        {
            surface = FindFirstObjectByType<NavMeshSurface>();
            Debug.Log("Không thấy NavMeshSurFace trong scene");
            if (surface == null)
            {
                Debug.LogError("Không tìm thấy NavMeshSurface nào trong scene!");
                return;
            }
        }

        BakeNavMesh();
    }

    private void BakeNavMesh()
    {
        if (surface != null)
        {
            surface.BuildNavMesh();
            Debug.Log("NavMesh đã được bake!");
        }
        else
        {
            Debug.LogWarning("Chưa Bake dc");
        }
    }
}
