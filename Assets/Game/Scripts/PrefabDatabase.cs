using UnityEngine;

[CreateAssetMenu(fileName ="PrefabDatabase", menuName = "Game/PrefabDatabase")]
public class PrefabDatabase : ScriptableObject
{
    [Header("Map Prefabs")]
    public GameObject firstMapPrefab;
    public GameObject secondMapPrefab;
    public GameObject thirdMapPrefab;

    [Header("Player Prefabs")]
    public GameObject playerPrefab;

    [Header("Enemy / Boss Prefabs")]
    public GameObject bossPrefab;
    public GameObject enemyPrefab;
}
