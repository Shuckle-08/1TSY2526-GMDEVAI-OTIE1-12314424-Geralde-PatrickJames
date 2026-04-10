using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool rotateToMovement = true;
    [SerializeField, Range(0f, 1f)] private float inputDeadzone = 0.1f;

    [Header("References")]
    [SerializeField] private CharacterController characterController;

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    private void Update()
    {
        if (characterController == null)
        {
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);
        bool hasInput = moveDirection.sqrMagnitude >= inputDeadzone * inputDeadzone;

        if (hasInput && moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }
        else if (!hasInput)
        {
            moveDirection = Vector3.zero;
        }

        Vector3 delta = moveDirection * (moveSpeed * Time.deltaTime);
        characterController.Move(delta);

        if (rotateToMovement && hasInput)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        }
    }
}
