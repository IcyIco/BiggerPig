using UnityEngine;
using UnityEngine.Rendering;

public sealed class ObjectScatter : MonoBehaviour
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private int count = 100;
    [SerializeField] private Vector2 areaSize = new Vector2(190f, 190f);
    [SerializeField] private float groundY;
    [SerializeField] private Vector2 scaleRange = new Vector2(1f, 1f);
    [SerializeField] private bool disableShadows;
    [SerializeField] private int seed = 12345;

    [ContextMenu("Generate Objects")]
    private void GenerateObjects()
    {
        ClearObjects();

        if (objectPrefab == null)
        {
            Debug.LogError("ObjectScatter: Object prefab is missing.", this);
            return;
        }

        Random.State previousState = Random.state;
        Random.InitState(seed);

        for (int i = 0; i < count; i++)
        {
            GameObject instance = Instantiate(
                objectPrefab,
                transform
            );

            float x = Random.Range(
                -areaSize.x * 0.5f,
                areaSize.x * 0.5f
            );

            float z = Random.Range(
                -areaSize.y * 0.5f,
                areaSize.y * 0.5f
            );

            instance.transform.localPosition =
                new Vector3(x, groundY, z);

            instance.transform.localRotation =
                Quaternion.Euler(
                    0f,
                    Random.Range(0f, 360f),
                    0f
                );

            float scale = Random.Range(
                scaleRange.x,
                scaleRange.y
            );

            instance.transform.localScale =
                Vector3.one * scale;

            if (disableShadows)
            {
                Renderer[] renderers =
                    instance.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers)
                {
                    renderer.shadowCastingMode =
                        ShadowCastingMode.Off;

                    renderer.receiveShadows = false;
                }
            }

            instance.isStatic = true;
        }

        Random.state = previousState;
    }

    [ContextMenu("Clear Objects")]
    private void ClearObjects()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(
                transform.GetChild(i).gameObject
            );
        }
    }
}