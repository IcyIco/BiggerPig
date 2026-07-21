using UnityEngine;

public sealed class FenceLoopGenerator : MonoBehaviour
{
    [SerializeField] private GameObject fencePrefab;
    [SerializeField] private float width = 190f;
    [SerializeField] private float length = 190f;
    [SerializeField] private float groundY;
    [SerializeField] private bool lengthAlongX = true;
    [SerializeField] private bool disableFenceColliders = true;

    [ContextMenu("Generate Fence Loop")]
    private void GenerateFenceLoop()
    {
        ClearFenceLoop();

        if (fencePrefab == null)
        {
            Debug.LogError("FenceLoopGenerator: Fence prefab is missing.", this);
            return;
        }

        float sourceLength = MeasureFenceLength();

        if (sourceLength <= 0.001f)
        {
            Debug.LogError("FenceLoopGenerator: Fence length could not be measured.", this);
            return;
        }

        GenerateHorizontalSide(length * 0.5f, width, sourceLength, true);
        GenerateHorizontalSide(-length * 0.5f, width, sourceLength, false);
        GenerateVerticalSide(width * 0.5f, length, sourceLength, true);
        GenerateVerticalSide(-width * 0.5f, length, sourceLength, false);
    }

    private void GenerateHorizontalSide(
        float z,
        float sideLength,
        float sourceLength,
        bool north)
    {
        int count = Mathf.Max(
            1,
            Mathf.RoundToInt(sideLength / sourceLength)
        );

        float step = sideLength / count;
        float stretch = step / sourceLength;

        for (int i = 0; i < count; i++)
        {
            float x =
                -sideLength * 0.5f +
                step * 0.5f +
                i * step;

            float rotationY;

            if (lengthAlongX)
            {
                rotationY = north ? 0f : 180f;
            }
            else
            {
                rotationY = north ? 90f : -90f;
            }

            CreateFence(
                new Vector3(x, groundY, z),
                rotationY,
                stretch
            );
        }
    }

    private void GenerateVerticalSide(
        float x,
        float sideLength,
        float sourceLength,
        bool east)
    {
        int count = Mathf.Max(
            1,
            Mathf.RoundToInt(sideLength / sourceLength)
        );

        float step = sideLength / count;
        float stretch = step / sourceLength;

        for (int i = 0; i < count; i++)
        {
            float z =
                -sideLength * 0.5f +
                step * 0.5f +
                i * step;

            float rotationY;

            if (lengthAlongX)
            {
                rotationY = east ? 90f : -90f;
            }
            else
            {
                rotationY = east ? 0f : 180f;
            }

            CreateFence(
                new Vector3(x, groundY, z),
                rotationY,
                stretch
            );
        }
    }

    private void CreateFence(
        Vector3 localPosition,
        float rotationY,
        float stretch)
    {
        GameObject instance = Instantiate(
            fencePrefab,
            transform
        );

        instance.transform.localPosition = localPosition;
        instance.transform.localRotation =
            Quaternion.Euler(0f, rotationY, 0f);

        Vector3 scale = fencePrefab.transform.localScale;

        if (lengthAlongX)
        {
            scale.x *= stretch;
        }
        else
        {
            scale.z *= stretch;
        }

        instance.transform.localScale = scale;

        if (disableFenceColliders)
        {
            Collider[] colliders =
                instance.GetComponentsInChildren<Collider>(true);

            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
        }

        instance.isStatic = true;
    }

    private float MeasureFenceLength()
    {
        GameObject sample = Instantiate(fencePrefab);

        sample.transform.position = Vector3.zero;
        sample.transform.rotation = Quaternion.identity;

        Renderer[] renderers =
            sample.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            DestroyObject(sample);
            return 0f;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float measuredLength =
            lengthAlongX ? bounds.size.x : bounds.size.z;

        DestroyObject(sample);
        return measuredLength;
    }

    [ContextMenu("Clear Fence Loop")]
    private void ClearFenceLoop()
    {
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
            DestroyImmediate(target);
        }
    }
}