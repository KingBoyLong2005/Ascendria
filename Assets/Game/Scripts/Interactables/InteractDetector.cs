using UnityEngine;

public class InteractDetector : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;

    private Interactable current;

    public Interactable Current => current;

    private InteractionUI interactionUI;

    private void Awake()
    {
        interactionUI = InteractionUI.Instance;

        if (interactionUI == null)
            interactionUI = FindFirstObjectByType<InteractionUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (current != null)
            return;

        if (((1 << other.gameObject.layer) & interactableLayer) == 0)
            return;

        if (other.TryGetComponent<Interactable>(out var interactable))
        {
            current = interactable;

            interactionUI?.Show();

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

            interactionUI?.Hide();

            Debug.Log("Ra khỏi vùng tương tác");
        }
    }
}
