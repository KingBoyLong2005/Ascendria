using System;
using UnityEngine;
public class InteractionEventArgs : EventArgs
{
    public Interactable Interactable { get; }
    public InteractionType Type => Interactable.interactType;

    public InteractionEventArgs(Interactable interactable)
    {
        Interactable = interactable;
    }
}

public class PlayerInteraction : MonoBehaviour
{
    private float raycastDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Debug")]
    [SerializeField] private bool showRaycastGizmos = true;

    public static event EventHandler<InteractionEventArgs> OnInteracted;

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
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, raycastDistance, interactableLayer))
        {
            if (hit.collider.TryGetComponent<Interactable>(out var interactable))
            {
                OnInteracted?.Invoke(this, new InteractionEventArgs(interactable));

                //Log
                Debug.Log($"Đã tương tác với {interactable.interactType}");
            }
        }
    }
}



//PlayerInteraction.OnInteracted += //tên event function

//OnPlayerInteract(object sender, InteractionEventArgs e)
//{
//    if (e.Type == InteractionType.Chest)
//    {
//        Debug.Log($"Mở rương");
//    }    
//}