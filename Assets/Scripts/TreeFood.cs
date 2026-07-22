using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class TreeFood : MonoBehaviour
{
    public const float RequiredScale = 2.2f;

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

        if (animal == null ||
            animal.TotalScale < RequiredScale)
        {
            return;
        }

        // Prevent multiple collision callbacks from eating the same tree.
        eaten = true;

        // Trees and carrots provide the same growth amount.
        animal.AddTotalScale(CarrotFood.ScaleValue);

        Destroy(gameObject);
    }
}