// EnemyData.cs  (template)
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    //public GameObject prefab;
    public string enemyName;
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float attack = 10f;
    public float armor = 10f;
}
