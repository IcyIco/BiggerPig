using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class AnimalDefinition
{
    public string displayName;
    public GameObject modelPrefab;
    public RuntimeAnimatorController animatorController;

    [Min(0.01f)]
    public float scale = 1f;
}

public sealed class AnimalLibrary : MonoBehaviour
{
    [SerializeField]
    private List<AnimalDefinition> animals =
        new List<AnimalDefinition>();

    public AnimalDefinition[] GetAnimals()
    {
        return animals.ToArray();
    }
}