using UnityEngine;

[RequireComponent(typeof(AnimalMovement))]
[RequireComponent(typeof(AnimalActor))]
public sealed class AnimalAI : MonoBehaviour
{
    private const float SightRadius = 12f;
    private const float TargetLostDistance = 15f;
    private const float SearchInterval = 0.25f;

    private const float MinimumWanderDuration = 1.5f;
    private const float MaximumWanderDuration = 4f;
    private const float IdleChance = 0.25f;
    private const float SprintChance = 0.1f;

    private AnimalMovement movement;
    private AnimalActor actor;

    private Transform target;
    private float searchTimer;
    private float wanderTimer;

    private void Awake()
    {
        movement = GetComponent<AnimalMovement>();
        actor = GetComponent<AnimalActor>();
    }

    private void Start()
    {
        ChooseWanderAction();

        // Spread searches across different frames.
        searchTimer = Random.Range(0f, SearchInterval);
    }

    private void Update()
    {
        searchTimer -= Time.deltaTime;

        if (target != null && IsTargetValid(target))
        {
            MoveToTarget();
            return;
        }

        target = null;

        if (searchTimer <= 0f)
        {
            searchTimer = SearchInterval;
            target = FindTarget();
        }

        if (target != null)
        {
            MoveToTarget();
            return;
        }

        UpdateWander();
    }

    private Transform FindTarget()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(
            transform.position,
            SightRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        Transform nearestCarrot = null;
        float nearestCarrotDistance = float.PositiveInfinity;

        Transform nearestTree = null;
        float nearestTreeDistance = float.PositiveInfinity;

        bool canEatTrees =
            actor.TotalScale >= TreeFood.RequiredScale;

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
                float carrotDistance =
                    GetFlatSqrDistance(carrot.transform.position);

                if (carrotDistance < nearestCarrotDistance)
                {
                    nearestCarrotDistance = carrotDistance;
                    nearestCarrot = carrot.transform;
                }

                continue;
            }

            if (!canEatTrees)
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

            if (treeDistance < nearestTreeDistance)
            {
                nearestTreeDistance = treeDistance;
                nearestTree = tree.transform;
            }
        }

        // Carrots take priority over trees.
        return nearestCarrot != null
            ? nearestCarrot
            : nearestTree;
    }

    private bool IsTargetValid(Transform currentTarget)
    {
        if (currentTarget == null)
        {
            return false;
        }

        float maximumDistance =
            TargetLostDistance * TargetLostDistance;

        if (GetFlatSqrDistance(currentTarget.position) >
            maximumDistance)
        {
            return false;
        }

        if (currentTarget.GetComponent<CarrotFood>() != null)
        {
            return true;
        }

        if (currentTarget.GetComponent<TreeFood>() != null)
        {
            return actor.TotalScale >= TreeFood.RequiredScale;
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

    private void UpdateWander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            ChooseWanderAction();
        }
    }

    private void ChooseWanderAction()
    {
        wanderTimer = Random.Range(
            MinimumWanderDuration,
            MaximumWanderDuration
        );

        if (Random.value < IdleChance)
        {
            movement.SetMoveInput(Vector2.zero);
            movement.SetSprinting(false);
            return;
        }

        Vector2 direction =
            Random.insideUnitCircle.normalized;

        movement.SetMoveInput(direction);

        movement.SetSprinting(
            Random.value < SprintChance
        );
    }

    private float GetFlatSqrDistance(Vector3 targetPosition)
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