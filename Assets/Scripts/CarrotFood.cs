using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CarrotFood : MonoBehaviour
{
    public const float ScaleValue = 0.04f;

    [Header("Launch Physics")]
    [SerializeField] private float airborneDuration = 2f;
    [SerializeField] private float mass = 0.15f;
    [SerializeField] private float spinForce = 8f;

    private Collider foodCollider;
    private Rigidbody rb;
    private bool collectible = true;
    private bool eaten;

    private void Awake()
    {
        foodCollider = GetComponent<Collider>();
        foodCollider.isTrigger = true;

        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!collectible || eaten)
        {
            return;
        }

        AnimalActor animal =
            other.GetComponentInParent<AnimalActor>();

        if (animal == null)
        {
            return;
        }

        eaten = true;

        animal.AddTotalScale(ScaleValue);

        Destroy(gameObject);
    }

    public void Launch(Vector3 velocity)
    {
        eaten = false;
        collectible = false;

        foodCollider.isTrigger = false;

        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb =
                gameObject.AddComponent<Rigidbody>();
        }

        rb.mass = mass;
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(
            velocity,
            ForceMode.VelocityChange
        );

        rb.AddTorque(
            Random.insideUnitSphere * spinForce,
            ForceMode.VelocityChange
        );

        StartCoroutine(SettleRoutine());
    }

    private IEnumerator SettleRoutine()
    {
        yield return new WaitForSeconds(
            airborneDuration
        );

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        foodCollider.isTrigger = true;
        collectible = true;

        if (rb != null)
        {
            Rigidbody bodyToRemove = rb;
            rb = null;
            Destroy(bodyToRemove);
        }
    }
}