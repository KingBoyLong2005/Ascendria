using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public float jumpHeight = 2.0f;
    private float gravity = -9.8f;
    [SerializeField] private bool shouldFaceMoveDirection = false;
    [SerializeField] private Transform cameraTransform;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector3 velocity;

    public bool isUIOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        //Debug.Log($"Move Input: {moveInput}");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log($"Jumping: {context.performed} - IsGrounded: {characterController.isGrounded}");
        if(context.performed && characterController.isGrounded)
        {
            //Debug.Log($"Jumping");
            velocity.y = Mathf.Sqrt( jumpHeight * -2f * gravity );
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isUIOpen)
        {
            return;
        }

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;
        characterController.Move( moveDirection * speed * Time.deltaTime);

        
        if (shouldFaceMoveDirection && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }
         
        //Vector3 move = new Vector3( moveInput.x, 0, moveInput.y );
        //characterController.Move( move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
