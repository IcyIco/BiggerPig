using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AnimalMovement))]
public sealed class AnimalActor : MonoBehaviour
{
    private const float MinimumScaleAdvantage = 1f;
    private const float MinimumTotalScale = 0.5f;
    private const float StunDuration = 1.5f;
    private const float HitProtectionDuration = 0.5f;
    private const float KnockbackForce = 4f;

    private const float CarrotSpawnHeight = 0.6f;
    private const float CarrotSpawnRadius = 0.7f;
    private const float MinimumHorizontalSpeed = 2.5f;
    private const float MaximumHorizontalSpeed = 5f;
    private const float MinimumUpwardSpeed = 3f;
    private const float MaximumUpwardSpeed = 5f;
    private const int CarrotsPerFrame = 10;

    [SerializeField] private Transform modelSlot;
    [SerializeField] private CarrotFood carrotPrefab;

    private BoxCollider boxCollider;
    private Rigidbody rb;
    private AnimalMovement movement;
    private GameObject modelInstance;
    private Animator animator;
    private Vector3 baseColliderSize;
    private Vector3 baseColliderCenter;
    private bool hitProtected;

    public float TotalScale { get; private set; }

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<AnimalMovement>();

        baseColliderSize = boxCollider.size;
        baseColliderCenter = boxCollider.center;

        if (modelSlot == null)
        {
            Debug.LogError("AnimalActor: ModelSlot is missing.", this);
            enabled = false;
        }
    }

    public void SetAnimal(AnimalDefinition definition)
    {
        if (modelSlot == null)
        {
            return;
        }

        if (definition == null)
        {
            Debug.LogError("AnimalActor: Animal definition is missing.", this);
            return;
        }

        if (definition.modelPrefab == null)
        {
            Debug.LogError("AnimalActor: Model prefab is missing.", this);
            return;
        }

        if (definition.animatorController == null)
        {
            Debug.LogError("AnimalActor: Animator controller is missing.", this);
            return;
        }

        ClearModel();

        modelInstance = Instantiate(definition.modelPrefab, modelSlot);
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localRotation = Quaternion.identity;

        TotalScale = definition.scale;
        ApplyScale();

        animator = modelInstance.GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            Debug.LogError(
                "AnimalActor: Animator is missing from the model.",
                modelInstance
            );

            movement.SetAnimator(null);
            return;
        }

        animator.runtimeAnimatorController =
            definition.animatorController;

        animator.applyRootMotion = false;
        animator.Rebind();
        animator.Update(0f);

        movement.SetAnimator(animator);
    }

    public void AddTotalScale(float amount)
    {
        if (modelInstance == null)
        {
            return;
        }

        TotalScale = Mathf.Max(
            MinimumTotalScale,
            TotalScale + amount
        );

        ApplyScale();
    }

    private void OnCollisionEnter(Collision collision)
    {
        AnimalActor otherActor =
            collision.collider.GetComponentInParent<AnimalActor>();

        if (otherActor == null || otherActor == this)
        {
            return;
        }

        if (TotalScale - otherActor.TotalScale <
            MinimumScaleAdvantage)
        {
            return;
        }

        Vector3 knockbackDirection =
            otherActor.transform.position - transform.position;

        if (!otherActor.TryReceiveHit(
                knockbackDirection,
                out float lostScale))
        {
            return;
        }

        if (carrotPrefab == null)
        {
            Debug.LogError(
                "AnimalActor: Carrot prefab is missing.",
                this
            );

            return;
        }

        int carrotCount = Mathf.RoundToInt(
            lostScale / CarrotFood.ScaleValue
        );

        if (carrotCount > 0)
        {
            StartCoroutine(
                ScatterCarrotsRoutine(
                    otherActor.transform.position,
                    carrotCount
                )
            );
        }
    }

    private bool TryReceiveHit(
        Vector3 knockbackDirection,
        out float lostScale)
    {
        lostScale = 0f;

        if (hitProtected || modelInstance == null)
        {
            return false;
        }

        if (TotalScale <= MinimumTotalScale + 0.001f)
        {
            return false;
        }

        lostScale = TotalScale - MinimumTotalScale;
        TotalScale = MinimumTotalScale;

        ApplyScale();

        hitProtected = true;
        StartCoroutine(StunRoutine(knockbackDirection));

        return true;
    }

    private IEnumerator StunRoutine(Vector3 knockbackDirection)
    {
        // Temporarily hand movement over to physics.
        movement.SetMoveInput(Vector2.zero);
        movement.SetSprinting(false);
        movement.enabled = false;

        if (animator != null)
        {
            animator.speed = 0f;
        }

        knockbackDirection = Vector3.ProjectOnPlane(
            knockbackDirection,
            Vector3.up
        );

        if (knockbackDirection.sqrMagnitude < 0.001f)
        {
            knockbackDirection = transform.forward;
        }

        knockbackDirection.Normalize();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(
            knockbackDirection * KnockbackForce
            + Vector3.up * KnockbackForce * 0.25f,
            ForceMode.VelocityChange
        );

        yield return new WaitForSeconds(StunDuration);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (animator != null)
        {
            animator.speed = 1f;
        }

        movement.enabled = true;

        yield return new WaitForSeconds(
            HitProtectionDuration
        );

        hitProtected = false;
    }

    private IEnumerator ScatterCarrotsRoutine(
        Vector3 center,
        int carrotCount)
    {
        for (int i = 0; i < carrotCount; i++)
        {
            Vector2 circle = Random.insideUnitCircle;

            if (circle.sqrMagnitude < 0.001f)
            {
                circle = Vector2.right;
            }

            circle.Normalize();

            Vector3 horizontalDirection =
                new Vector3(circle.x, 0f, circle.y);

            Vector3 spawnPosition =
                center
                + Vector3.up * CarrotSpawnHeight
                + horizontalDirection
                * Random.Range(0f, CarrotSpawnRadius);

            CarrotFood carrot = Instantiate(
                carrotPrefab,
                spawnPosition,
                Random.rotation
            );

            float horizontalSpeed = Random.Range(
                MinimumHorizontalSpeed,
                MaximumHorizontalSpeed
            );

            float upwardSpeed = Random.Range(
                MinimumUpwardSpeed,
                MaximumUpwardSpeed
            );

            carrot.Launch(
                horizontalDirection * horizontalSpeed
                + Vector3.up * upwardSpeed
            );

            // Limit large drops to ten carrots per frame.
            if ((i + 1) % CarrotsPerFrame == 0)
            {
                yield return null;
            }
        }
    }

    private void ApplyScale()
    {
        if (modelInstance == null)
        {
            return;
        }

        modelInstance.transform.localScale =
            Vector3.one * TotalScale;

        boxCollider.size =
            baseColliderSize * TotalScale;

        boxCollider.center =
            baseColliderCenter * TotalScale;
    }

    private void ClearModel()
    {
        for (int i = modelSlot.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(modelSlot.GetChild(i).gameObject);
        }

        modelInstance = null;
        animator = null;
        TotalScale = 0f;

        movement.SetAnimator(null);
    }
}