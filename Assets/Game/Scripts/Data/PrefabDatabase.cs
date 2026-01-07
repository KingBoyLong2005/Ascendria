using UnityEngine;

[CreateAssetMenu(fileName ="PrefabDatabase", menuName = "Game/PrefabDatabase")]
public class PrefabDatabase : ScriptableObject
{
    [Header("Map Prefabs")]
    public GameObject firstMapPrefab;
    public GameObject secondMapPrefab;
    public GameObject thirdMapPrefab;
    public GameObject bossGatePrefab;

    [Header("Player Prefabs")]
    public GameObject playerPrefab;

    [Header("Enemy / Boss Prefabs")]
    public GameObject bossPrefab;
    public GameObject enemyPrefab;
    public GameObject enemyDemonPrefab;

    [Header("Interactable Prefabs")]
    public GameObject chest;
    public GameObject eventObject;

    [Header("Drop Prefabs")]
    public GameObject health;
    public GameObject exp;
    public GameObject coin;

    [Header("Audio BGM")]
    public AudioClip mapTheme;
    public AudioClip bossTheme;

    [Header("Audio SFX")]
    public AudioClip gateOpenSfx;
    public AudioClip playerSpawnSfx;
    public AudioClip bossSpawnSfx;

    [Header("UI Prefabs")]
    public GameObject bossHPBar;

    private static PrefabDatabase _instance;
    public static PrefabDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<PrefabDatabase>("PrefabDatabase");
                if (_instance == null)
                {
                    Debug.LogError("PrefabDatabase not found in Resources!");
                }
            }
            return _instance;
        }
    }
}
