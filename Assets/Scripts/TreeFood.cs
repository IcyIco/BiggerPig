using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class TreeFood : MonoBehaviour
{
    public const float RequiredScale = 2.2f;

    [SerializeField]
    private float requiredScale =
        RequiredScale;

    [SerializeField]
    private float growthAmount =
        CarrotFood.ScaleValue;

    private bool eaten;

    private void OnCollisionEnter(Collision collision)
    {
        TryEat(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryEat(other);
    }

    private void TryEat(Collider other)
    {
        if (eaten)
        {
            return;
        }

        AnimalActor animal =
            other.GetComponentInParent<AnimalActor>();

        if (animal == null)
        {
            return;
        }

        if (animal.TotalScale < requiredScale)
        {
            return;
        }

        eaten = true;

        animal.AddTotalScale(growthAmount);

        Destroy(gameObject);
    }
}