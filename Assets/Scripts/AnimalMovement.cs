using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public sealed class AnimalMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private Transform modelSlot;

    private Rigidbody rb;
    private Animator animator;
    private Vector2 moveInput;
    private bool isSprinting;
    private bool controlEnabled = true;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (modelSlot == null)
        {
            modelSlot = transform.Find("ModelSlot");
        }

        if (modelSlot == null)
        {
            Debug.LogError(
                "AnimalMovement: ModelSlot is missing.",
                this
            );

            enabled = false;
        }
    }

    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator;

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);
        }
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput =
            Vector2.ClampMagnitude(input, 1f);
    }

    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;
    }

    public void SetControlEnabled(bool value)
    {
        controlEnabled = value;

        if (!controlEnabled && animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);
        }
    }

    public void OnMove(InputValue value)
    {
        SetMoveInput(value.Get<Vector2>());
    }

    public void OnSprint(InputValue value)
    {
        SetSprinting(value.isPressed);
    }

    private void FixedUpdate()
    {
        if (!controlEnabled)
        {
            if (animator != null)
            {
                animator.SetFloat(SpeedHash, 0f);
            }

            return;
        }

        Vector3 direction =
            new Vector3(
                moveInput.x,
                0f,
                moveInput.y
            );

        bool isMoving =
            direction.sqrMagnitude > 0.001f;

        float speed =
            isSprinting
                ? runSpeed
                : walkSpeed;

        if (isMoving)
        {
            Vector3 newPosition =
                rb.position
                + direction
                * speed
                * Time.fixedDeltaTime;

            rb.MovePosition(newPosition);

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
            animator.SetFloat(
                SpeedHash,
                isMoving ? speed : 0f
            );
        }
    }
}