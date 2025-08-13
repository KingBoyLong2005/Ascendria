// using UnityEngine;

// interface IInteractable
// {
//     public void Interact();
// }

// public class Interactor : MonoBehaviour
// {
//     public Transform interactorSource;
//     public float interactRange;
//     public GameObject interactionUI;

//     void Update()
//     {
//         Ray r = new Ray(interactorSource.position, interactorSource.forward);
//         // Vẽ ray mỗi frame để kiểm tra
//         Debug.DrawRay(interactorSource.position, interactorSource.forward * interactRange, Color.red);


//         if (Physics.Raycast(r, out RaycastHit hitInfo, interactRange))
//         {
//             if (hitInfo.collider.GetComponentInParent<IInteractable>() is IInteractable interactObj)
//             {
//                 if (interactionUI != null && !interactionUI.activeSelf)
//                 {
//                     Debug.Log("Hiển thị");
//                     interactionUI.SetActive(true); // Hiện nút E
//                 }

//                 if (Input.GetKeyDown(KeyCode.E))
//                 {
//                     interactObj.Interact();
//                 }

//                 return;
//             }
//         }

//         // Nếu không trúng object hợp lệ → ẩn UI
//         if (interactionUI.activeSelf)
//         {
//             interactionUI.SetActive(false);
//         }
//     }
// }
