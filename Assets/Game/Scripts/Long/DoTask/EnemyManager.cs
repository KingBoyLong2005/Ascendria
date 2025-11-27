using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public float lifeTime = 5f;

    void Awake()
    {
        Debug.Log($"Spawn tại {transform.position}");
    }
    void Start()
    {
        Die();
    }
    private void Die()
    {
        Destroy(gameObject, lifeTime);
        Debug.Log($"Enemy chết! InstanceID = {gameObject.GetInstanceID()}");
    }
}
