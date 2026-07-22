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

    private void Awake()
    {
        foodCollider = GetComponent<Collider>();
        foodCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!collectible)
        {
            return;
        }

        AnimalActor animal =
            other.GetComponentInParent<AnimalActor>();

        if (animal == null)
        {
            return;
        }

        // Prevent multiple animals from collecting the same carrot.
        collectible = false;

        animal.AddTotalScale(ScaleValue);
        Destroy(gameObject);
    }

    public void Launch(Vector3 velocity)
    {
        // The carrot cannot be collected while it is airborne.
        collectible = false;
        foodCollider.isTrigger = false;

        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        rb.AddForce(velocity, ForceMode.VelocityChange);
        rb.AddTorque(
            Random.insideUnitSphere * spinForce,
            ForceMode.VelocityChange
        );

        StartCoroutine(SettleRoutine());
    }

    private IEnumerator SettleRoutine()
    {
        yield return new WaitForSeconds(airborneDuration);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        Destroy(rb);
        rb = null;

        // Return to a lightweight collectible trigger.
        foodCollider.isTrigger = true;
        collectible = true;
    }
}