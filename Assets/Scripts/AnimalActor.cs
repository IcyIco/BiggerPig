using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AnimalMovement))]
public sealed class AnimalActor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform modelSlot;
    [SerializeField] private CarrotFood carrotPrefab;

    [Header("Hit")]
    [SerializeField] private float minimumScaleAdvantage = 1f;
    [SerializeField] private float minimumTotalScale = 0.5f;
    [SerializeField] private float stunDuration = 1.5f;
    [SerializeField] private float hitProtectionDuration = 0.5f;
    [SerializeField] private float knockbackForce = 4f;

    [Header("Carrot Scatter")]
    [SerializeField] private float carrotSpawnHeight = 0.6f;
    [SerializeField] private float carrotSpawnRadius = 0.7f;
    [SerializeField]
    private Vector2 horizontalSpeedRange =
        new Vector2(2.5f, 5f);
    [SerializeField]
    private Vector2 upwardSpeedRange =
        new Vector2(3f, 5f);

    private static readonly Vector3 BaseColliderSize =
        new Vector3(1f, 1.3f, 1f);

    private static readonly Vector3 BaseColliderCenter =
        new Vector3(0f, 0.6f, 0f);

    private BoxCollider boxCollider;
    private Rigidbody rb;
    private AnimalMovement movement;
    private GameObject modelInstance;
    private Animator animator;
    private bool hitProtected;

    public AnimalDefinition Definition { get; private set; }

    public float GrowthScale { get; private set; } = 1f;

    public float TotalScale
    {
        get
        {
            if (Definition == null)
            {
                return GrowthScale;
            }

            return Definition.scale * GrowthScale;
        }
    }

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<AnimalMovement>();

        if (modelSlot == null)
        {
            modelSlot = transform.Find("ModelSlot");
        }

        if (modelSlot == null)
        {
            Debug.LogError(
                "AnimalActor: ModelSlot is missing.",
                this
            );

            enabled = false;
        }
    }

    public void SetAnimal(AnimalDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogError(
                "AnimalActor: Animal definition is missing.",
                this
            );

            return;
        }

        if (definition.modelPrefab == null)
        {
            Debug.LogError(
                "AnimalActor: Model prefab is missing.",
                this
            );

            return;
        }

        if (definition.animatorController == null)
        {
            Debug.LogError(
                "AnimalActor: Animator controller is missing.",
                this
            );

            return;
        }

        ClearModel();

        Definition = definition;
        GrowthScale = 1f;
        hitProtected = false;

        modelInstance = Instantiate(
            definition.modelPrefab,
            modelSlot
        );

        modelInstance.name = definition.displayName;
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localRotation = Quaternion.identity;

        ApplyScale();

        animator =
            modelInstance.GetComponentInChildren<Animator>(true);

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

    public void SetGrowthScale(float scale)
    {
        GrowthScale = Mathf.Max(0.01f, scale);
        ApplyScale();
    }

    public void SetTotalScale(float totalScale)
    {
        if (Definition == null)
        {
            return;
        }

        float safeScale =
            Mathf.Max(minimumTotalScale, totalScale);

        GrowthScale =
            safeScale / Definition.scale;

        ApplyScale();
    }

    public void AddTotalScale(float amount)
    {
        SetTotalScale(TotalScale + amount);
    }

    public void Grow(float amount)
    {
        AddTotalScale(amount);
    }

    private void OnCollisionEnter(Collision collision)
    {
        AnimalActor otherActor =
            collision.collider.GetComponentInParent<AnimalActor>();

        if (otherActor == null || otherActor == this)
        {
            return;
        }

        float scaleDifference =
            TotalScale - otherActor.TotalScale;

        if (scaleDifference < minimumScaleAdvantage)
        {
            return;
        }

        Vector3 knockbackDirection =
            otherActor.transform.position - transform.position;

        bool hitSucceeded =
            otherActor.TryReceiveHit(
                knockbackDirection,
                out float lostScale
            );

        if (!hitSucceeded)
        {
            return;
        }

        if (carrotPrefab == null)
        {
            Debug.LogError(
                "AnimalActor: Carrot prefab is missing. The hit worked, but no carrots were spawned.",
                this
            );

            return;
        }

        int carrotCount =
            Mathf.RoundToInt(
                lostScale / CarrotFood.ScaleValue
            );

        if (carrotCount <= 0)
        {
            return;
        }

        StartCoroutine(
            ScatterCarrotsRoutine(
                otherActor.transform.position,
                carrotCount
            )
        );
    }

    private bool TryReceiveHit(
        Vector3 knockbackDirection,
        out float lostScale
    )
    {
        lostScale = 0f;

        if (hitProtected || Definition == null)
        {
            return false;
        }

        float oldScale = TotalScale;

        if (oldScale <= minimumTotalScale + 0.001f)
        {
            return false;
        }

        lostScale =
            oldScale - minimumTotalScale;

        SetTotalScale(minimumTotalScale);

        hitProtected = true;

        StartCoroutine(
            StunRoutine(knockbackDirection)
        );

        return true;
    }

    private IEnumerator StunRoutine(
        Vector3 knockbackDirection
    )
    {
        movement.SetMoveInput(Vector2.zero);
        movement.SetSprinting(false);
        movement.enabled = false;

        if (animator != null)
        {
            animator.speed = 0f;
        }

        knockbackDirection =
            Vector3.ProjectOnPlane(
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
            knockbackDirection * knockbackForce
            + Vector3.up * knockbackForce * 0.25f,
            ForceMode.VelocityChange
        );

        yield return new WaitForSeconds(stunDuration);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (animator != null)
        {
            animator.speed = 1f;
        }

        movement.enabled = true;

        yield return new WaitForSeconds(
            hitProtectionDuration
        );

        hitProtected = false;
    }

    private IEnumerator ScatterCarrotsRoutine(
        Vector3 center,
        int carrotCount
    )
    {
        for (int i = 0; i < carrotCount; i++)
        {
            Vector2 circle =
                Random.insideUnitCircle;

            if (circle.sqrMagnitude < 0.001f)
            {
                circle = Vector2.right;
            }

            circle.Normalize();

            Vector3 horizontalDirection =
                new Vector3(
                    circle.x,
                    0f,
                    circle.y
                );

            Vector3 spawnPosition =
                center
                + Vector3.up * carrotSpawnHeight
                + horizontalDirection
                * Random.Range(0f, carrotSpawnRadius);

            CarrotFood carrot =
                Instantiate(
                    carrotPrefab,
                    spawnPosition,
                    Random.rotation
                );

            float horizontalSpeed =
                Random.Range(
                    horizontalSpeedRange.x,
                    horizontalSpeedRange.y
                );

            float upwardSpeed =
                Random.Range(
                    upwardSpeedRange.x,
                    upwardSpeedRange.y
                );

            Vector3 launchVelocity =
                horizontalDirection * horizontalSpeed
                + Vector3.up * upwardSpeed;

            carrot.Launch(launchVelocity);

            if ((i + 1) % 10 == 0)
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

        float scale = TotalScale;

        modelInstance.transform.localScale =
            Vector3.one * scale;

        boxCollider.size =
            BaseColliderSize * scale;

        boxCollider.center =
            BaseColliderCenter * scale;
    }

    private void ClearModel()
    {
        if (modelSlot == null)
        {
            return;
        }

        for (int i = modelSlot.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                modelSlot.GetChild(i).gameObject
            );
        }

        modelInstance = null;
        animator = null;
        Definition = null;

        movement.SetAnimator(null);
    }
}