using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speed")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    private Vector2 moveInput;
    private bool isSprinting;

    private Rigidbody rb;
    private Transform modelSlot;
    private Animator animator;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        modelSlot = transform.Find("ModelSlot");

        if (modelSlot == null)
        {
            Debug.LogError(
                "PlayerMovement: Could not find ModelSlot.",
                this
            );

            return;
        }

        animator = modelSlot.GetComponentInChildren<Animator>();
    }

    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    private void FixedUpdate()
    {
        Vector3 direction = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );

        direction = Vector3.ClampMagnitude(direction, 1f);

        bool isMoving =
            direction.sqrMagnitude > 0.001f;

        float currentSpeed = isSprinting
            ? runSpeed
            : walkSpeed;

        Vector3 newPosition =
            rb.position
            + direction
            * currentSpeed
            * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);

        if (isMoving && modelSlot != null)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction,
                    Vector3.up
                );

            modelSlot.rotation =
                Quaternion.Slerp(
                    modelSlot.rotation,
                    targetRotation,
                    rotationSpeed
                    * Time.fixedDeltaTime
                );
        }

        if (animator != null)
        {
            float animationSpeed;

            if (!isMoving)
            {
                animationSpeed = 0f;
            }
            else if (isSprinting)
            {
                animationSpeed = 5f;
            }
            else
            {
                animationSpeed = 1f;
            }

            animator.SetFloat(
                SpeedHash,
                animationSpeed
            );
        }
    }
}