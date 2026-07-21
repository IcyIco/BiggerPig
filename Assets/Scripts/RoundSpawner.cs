using System.Collections.Generic;
using UnityEngine;

public class RoundSpawner : MonoBehaviour
{
    [Header("Round")]
    [SerializeField] private AnimalLibrary animalLibrary;
    [SerializeField] private AnimalActor player;
    [SerializeField] private AnimalActor aiPrefab;
    [SerializeField] private Transform aiParent;
    [SerializeField] private Transform[] aiSpawnPoints;

    private readonly List<AnimalActor> spawnedAI =
        new List<AnimalActor>();

    private void Start()
    {
        SpawnRound();
    }

    public void SpawnRound()
    {
        AnimalDefinition[] animals =
            animalLibrary.GetAnimals();

        if (animals.Length < 8)
        {
            Debug.LogError(
                "RoundSpawner: At least eight animals are required.",
                this
            );

            return;
        }

        if (aiSpawnPoints.Length < 7)
        {
            Debug.LogError(
                "RoundSpawner: Seven spawn points are required.",
                this
            );

            return;
        }

        ClearAI();
        Shuffle(animals);

        player.SetAnimal(animals[0]);

        for (int i = 0; i < 7; i++)
        {
            Transform spawnPoint =
                aiSpawnPoints[i];

            AnimalActor ai =
                Instantiate(
                    aiPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation,
                    aiParent
                );

            ai.SetAnimal(animals[i + 1]);

            spawnedAI.Add(ai);
        }
    }

    private void ClearAI()
    {
        foreach (AnimalActor ai in spawnedAI)
        {
            if (ai != null)
            {
                Destroy(ai.gameObject);
            }
        }

        spawnedAI.Clear();
    }

    private static void Shuffle<T>(T[] items)
    {
        for (
            int i = items.Length - 1;
            i > 0;
            i--
        )
        {
            int randomIndex =
                Random.Range(0, i + 1);

            T temporary = items[i];
            items[i] = items[randomIndex];
            items[randomIndex] = temporary;
        }
    }
}