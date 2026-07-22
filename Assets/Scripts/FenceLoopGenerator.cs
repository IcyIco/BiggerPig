using UnityEngine;

public sealed class FenceLoopGenerator : MonoBehaviour
{
    [SerializeField] private GameObject fencePrefab;
    [SerializeField] private float width = 190f;
    [SerializeField] private float length = 190f;

    [ContextMenu("Generate Fence Loop")]
    private void GenerateFenceLoop()
    {
        // Validate every setting before replacing the existing fence.
        if (fencePrefab == null)
        {
            Debug.LogError("FenceLoopGenerator: Fence prefab is missing.", this);
            return;
        }

        if (width <= 0f || length <= 0f)
        {
            Debug.LogError("FenceLoopGenerator: Width and length must be greater than zero.", this);
            return;
        }

        float sourceLength = MeasureFenceLength();

        if (sourceLength <= 0.001f)
        {
            Debug.LogError("FenceLoopGenerator: Fence length could not be measured.", this);
            return;
        }

        ClearFenceLoop();

        GenerateHorizontalSide(length * 0.5f, width, sourceLength, true);
        GenerateHorizontalSide(-length * 0.5f, width, sourceLength, false);
        GenerateVerticalSide(width * 0.5f, length, sourceLength, true);
        GenerateVerticalSide(-width * 0.5f, length, sourceLength, false);
    }

    private void GenerateHorizontalSide(
        float z, float sideLength, float sourceLength, bool north)
    {
        // Divide the side evenly, then stretch each segment to remove gaps.
        int count = Mathf.Max(1, Mathf.RoundToInt(sideLength / sourceLength));
        float step = sideLength / count;
        float stretch = step / sourceLength;

        for (int i = 0; i < count; i++)
        {
            float x = -sideLength * 0.5f + step * 0.5f + i * step;

            // North and south sides face opposite directions.
            float rotationY = north ? 0f : 180f;

            CreateFence(new Vector3(x, 0f, z), rotationY, stretch);
        }
    }

    private void GenerateVerticalSide(
        float x, float sideLength, float sourceLength, bool east)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(sideLength / sourceLength));
        float step = sideLength / count;
        float stretch = step / sourceLength;

        for (int i = 0; i < count; i++)
        {
            float z = -sideLength * 0.5f + step * 0.5f + i * step;

            // East and west sides are rotated 90 degrees from the horizontal sides.
            float rotationY = east ? 90f : -90f;

            CreateFence(new Vector3(x, 0f, z), rotationY, stretch);
        }
    }

    private void CreateFence(
        Vector3 localPosition, float rotationY, float stretch)
    {
        GameObject instance = Instantiate(fencePrefab, transform);

        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.Euler(0f, rotationY, 0f);

        // Scale the fence length to match the calculated spacing.
        Vector3 scale = fencePrefab.transform.localScale;
        scale.x *= stretch;
        instance.transform.localScale = scale;

        instance.isStatic = true;
    }

    private float MeasureFenceLength()
    {
        // Use a temporary instance to measure the rendered fence model.
        GameObject sample = Instantiate(fencePrefab);
        sample.transform.rotation = Quaternion.identity;

        Renderer renderer = sample.GetComponentInChildren<Renderer>(true);

        if (renderer == null)
        {
            DestroyObject(sample);
            return 0f;
        }

        float measuredLength = renderer.bounds.size.x;

        DestroyObject(sample);
        return measuredLength;
    }

    [ContextMenu("Clear Fence Loop")]
    private void ClearFenceLoop()
    {
        // Remove children from last to first because the list is changing.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyObject(transform.GetChild(i).gameObject);
        }
    }

    private static void DestroyObject(GameObject target)
    {
        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            // Edit mode requires immediate destruction.
            DestroyImmediate(target);
        }
    }
}