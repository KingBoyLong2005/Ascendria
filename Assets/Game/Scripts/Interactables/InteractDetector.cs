using UnityEngine;

public class InteractDetector : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;

    private Interactable current;

    public Interactable Current => current;

    private void OnTriggerEnter(Collider other)
    {
        if (current != null)
            return;

        if (((1 << other.gameObject.layer) & interactableLayer) == 0)
            return;

        if (other.TryGetComponent<Interactable>(out var interactable))
        {
            current = interactable;

            // bật nút E
            // Thêm viền cho object ?

            Debug.Log($"Vào vùng tương tác: {interactable.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (current == null)
            return;

        if (other.TryGetComponent<Interactable>(out var interactable))
        {
            if (interactable != current)
                return;

            current = null;

            // Tắt nút E
            // Tắt viền

            Debug.Log("Ra khỏi vùng tương tác");
        }
    }
}
