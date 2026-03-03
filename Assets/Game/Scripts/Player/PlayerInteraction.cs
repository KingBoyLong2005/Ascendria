using System;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private InteractDetector interactDetector;
    private InteractionUI interactionUI;

    private void Awake()
    {
        interactionUI = InteractionUI.Instance;

        if (interactionUI == null)
            interactionUI = FindFirstObjectByType<InteractionUI>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (interactionUI != null && !interactionUI.IsShowing)
                return;
            Debug.Log("Bấm E");
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Debug.Log("Thử interact");

        if (interactDetector == null)
        {
            return;
        }

        var interactable = interactDetector.Current;

        if (interactable == null)
        {
            return;
        }

        interactionUI?.Hide();

        GameEventSystem.Trigger(this,
            new InteractionEventArgs(interactable, interactable.interactType));


        Debug.Log($"Đã tương tác với {interactable.interactType}");
    }
}
