using UnityEngine;

public sealed class ObjectScatter : MonoBehaviour
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private int count = 100;
    [SerializeField] private Vector2 areaSize = new Vector2(190f, 190f);

    [ContextMenu("Generate Objects")]
    private void GenerateObjects()
    {
        // Validate every setting before replacing the existing objects.
        if (objectPrefab == null)
        {
            Debug.LogError("ObjectScatter: Object prefab is missing.", this);
            return;
        }

        if (count <= 0)
        {
            Debug.LogError("ObjectScatter: Count must be greater than zero.", this);
            return;
        }

        if (areaSize.x <= 0f || areaSize.y <= 0f)
        {
            Debug.LogError("ObjectScatter: Area size must be greater than zero.", this);
            return;
        }

        ClearObjects();

        for (int i = 0; i < count; i++)
        {
            GameObject instance = Instantiate(objectPrefab, transform);

            // Choose a random position inside the local XZ area.
            float x = Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
            float z = Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);
            instance.transform.localPosition = new Vector3(x, 0f, z);

            // Rotate only around Y so the object stays upright.
            float rotationY = Random.Range(0f, 360f);
            instance.transform.localRotation =
                Quaternion.Euler(0f, rotationY, 0f);
        }
    }

    [ContextMenu("Clear Objects")]
    private void ClearObjects()
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