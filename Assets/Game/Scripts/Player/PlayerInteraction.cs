using System;
using System.Linq.Expressions;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private float raycastDistance = 30f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Debug")]
    [SerializeField] private bool showRaycastGizmos = true;

    private void Update()
    {
        if (showRaycastGizmos)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            Vector3 direction = transform.forward;

            Debug.DrawRay(rayOrigin, direction * raycastDistance, Color.red, 0f); 
        }    

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Bấm E");
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;

        Debug.Log("Thử interact");

        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, raycastDistance, interactableLayer))
        {
            if (hit.collider.TryGetComponent<Interactable>(out var interactable))
            {
                //GameEventSystem.Trigger(this, new InteractionEventArgs(interactable, interactable.interactType));
                var t = interactable.interactType;
                switch (t)
                {
                    case InteractionType.BossGate:
                        GameplayEvents.RaiseBossGateInteracted(interactable);
                        break;
                    case InteractionType.Chest:
                        GameplayEvents.RaiseChestInteracted(interactable);
                        break;
                    case InteractionType.Event:
                        GameplayEvents.RaiseEventInteracted(interactable);
                        break;
                    default:
                        break;
                }
                //Log
                Debug.Log($"Đã tương tác với {interactable.interactType}");
            }
        }
    }
}
