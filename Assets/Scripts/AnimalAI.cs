using System;
using UnityEngine;

[RequireComponent(typeof(AnimalMovement))]
[RequireComponent(typeof(AnimalActor))]
public sealed class AnimalAI : MonoBehaviour
{
    [Header("Sight")]
    [SerializeField] private float sightRadius = 12f;
    [SerializeField] private float targetLostDistance = 15f;
    [SerializeField] private float searchInterval = 0.25f;

    [Header("Decision Timing")]
    [SerializeField] private float minimumDuration = 1.5f;
    [SerializeField] private float maximumDuration = 4f;

    [Header("Wander")]
    [Range(0f, 1f)]
    [SerializeField] private float idleChance = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float sprintChance = 0.1f;

    private AnimalMovement movement;
    private AnimalActor actor;

    private Transform target;
    private float searchTimer;
    private float remainingTime;

    private void Awake()
    {
        movement = GetComponent<AnimalMovement>();
        actor = GetComponent<AnimalActor>();
    }

    private void Start()
    {
        ChooseWanderAction();
        searchTimer = UnityEngine.Random.Range(0f, searchInterval);
    }

    private void Update()
    {
        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            searchTimer = searchInterval;
            target = FindTarget();
        }

        if (target != null && IsTargetValid(target))
        {
            MoveToTarget();
            return;
        }

        target = null;
        UpdateWander();
    }

    private Transform FindTarget()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(
            transform.position,
            sightRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        Transform nearestCarrot = null;
        float nearestCarrotDistance = float.PositiveInfinity;

        Transform nearestTree = null;
        float nearestTreeDistance = float.PositiveInfinity;

        foreach (Collider nearbyCollider in nearbyObjects)
        {
            AnimalActor nearbyAnimal =
                nearbyCollider.GetComponentInParent<AnimalActor>();

            if (nearbyAnimal == actor)
            {
                continue;
            }

            CarrotFood carrot =
                nearbyCollider.GetComponentInParent<CarrotFood>();

            if (carrot != null)
            {
                float distance =
                    GetFlatSqrDistance(carrot.transform.position);

                if (distance < nearestCarrotDistance
                    && IsVisible(carrot.transform))
                {
                    nearestCarrotDistance = distance;
                    nearestCarrot = carrot.transform;
                }

                continue;
            }

            if (actor.TotalScale < TreeFood.RequiredScale)
            {
                continue;
            }

            TreeFood tree =
                nearbyCollider.GetComponentInParent<TreeFood>();

            if (tree == null)
            {
                continue;
            }

            float treeDistance =
                GetFlatSqrDistance(tree.transform.position);

            if (treeDistance < nearestTreeDistance
                && IsVisible(tree.transform))
            {
                nearestTreeDistance = treeDistance;
                nearestTree = tree.transform;
            }
        }

        if (nearestCarrot != null)
        {
            return nearestCarrot;
        }

        return nearestTree;
    }

    private bool IsTargetValid(Transform currentTarget)
    {
        if (currentTarget == null)
        {
            return false;
        }

        float maximumDistance =
            targetLostDistance * targetLostDistance;

        if (GetFlatSqrDistance(currentTarget.position)
            > maximumDistance)
        {
            return false;
        }

        CarrotFood carrot =
            currentTarget.GetComponent<CarrotFood>();

        if (carrot != null)
        {
            return IsVisible(currentTarget);
        }

        TreeFood tree =
            currentTarget.GetComponent<TreeFood>();

        if (tree != null)
        {
            return actor.TotalScale >= TreeFood.RequiredScale
                && IsVisible(currentTarget);
        }

        return false;
    }

    private void MoveToTarget()
    {
        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            movement.SetMoveInput(Vector2.zero);
            movement.SetSprinting(false);
            return;
        }

        direction.Normalize();

        movement.SetMoveInput(
            new Vector2(direction.x, direction.z)
        );

        movement.SetSprinting(false);
    }

    private bool IsVisible(Transform targetRoot)
    {
        Vector3 origin =
            transform.position
            + Vector3.up
            * Mathf.Max(0.5f, actor.TotalScale * 0.5f);

        Vector3 destination =
            targetRoot.position + Vector3.up * 0.2f;

        Vector3 direction =
            destination - origin;

        float distance = direction.magnitude;

        if (distance <= 0.001f)
        {
            return true;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction.normalized,
            distance,
            ~0,
            QueryTriggerInteraction.Collide
        );

        Array.Sort(
            hits,
            (first, second) =>
                first.distance.CompareTo(second.distance)
        );

        foreach (RaycastHit hit in hits)
        {
            AnimalActor hitAnimal =
                hit.collider.GetComponentInParent<AnimalActor>();

            if (hitAnimal == actor)
            {
                continue;
            }

            Transform hitTransform =
                hit.collider.transform;

            if (hitTransform == targetRoot
                || hitTransform.IsChildOf(targetRoot))
            {
                return true;
            }

            return false;
        }

        return false;
    }

    private void UpdateWander()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            ChooseWanderAction();
        }
    }

    private void ChooseWanderAction()
    {
        remainingTime = UnityEngine.Random.Range(
            minimumDuration,
            maximumDuration
        );

        if (UnityEngine.Random.value < idleChance)
        {
            movement.SetMoveInput(Vector2.zero);
            movement.SetSprinting(false);
            return;
        }

        Vector2 direction =
            UnityEngine.Random.insideUnitCircle.normalized;

        movement.SetMoveInput(direction);

        movement.SetSprinting(
            UnityEngine.Random.value < sprintChance
        );
    }

    private float GetFlatSqrDistance(
        Vector3 targetPosition
    )
    {
        Vector3 difference =
            targetPosition - transform.position;

        difference.y = 0f;

        return difference.sqrMagnitude;
    }

    private void OnDisable()
    {
        if (movement == null)
        {
            return;
        }

        movement.SetMoveInput(Vector2.zero);
        movement.SetSprinting(false);
    }
}