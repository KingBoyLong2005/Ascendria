using UnityEngine;

// Script này được áp dụng cho mọi Interactive Object
public class InteractiveObject : MonoBehaviour
{
    private Vector3 myPosition;

    public void Initialize(Vector3 spawnPosition)
    {
        myPosition = spawnPosition;
    }

    // Giả sử va chạm với một collider có tag "Player" là tương tác
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // KÍCH HOẠT TÍN HIỆU: Player tương tác với Boss Gate
            //GameEventManager.Instance.TriggerBossGateEntered(myPosition);

            // Tắt hoặc hủy cổng boss sau khi tương tác
            //gameObject.SetActive(false);
        }
    }
}