using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    
    private CharacterController characterController;
    private Vector3 velocity;
    private float cameraPitch = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleCamera();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (characterController.isGrounded)
        {
            jumpInput = true;
        }
    }

    private void HandleMovement()
    {
        // Calculate movement direction
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Handle jumping
        if (jumpInput)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpInput = false;
        }

        // Apply gravity
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleCamera()
    {
        // Horizontal rotation (rotate the player body)
        float yaw = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up * yaw);

        // Vertical rotation (rotate the camera)
        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }
}
