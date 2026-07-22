using UnityEngine;

public sealed class RoundSpawner : MonoBehaviour
{
    private const int AiCount = 7;
    private const int RequiredAnimalCount = AiCount + 1;

    [SerializeField] private AnimalLibrary animalLibrary;
    [SerializeField] private AnimalActor player;
    [SerializeField] private AnimalActor aiPrefab;
    [SerializeField] private Transform aiParent;
    [SerializeField] private Transform[] aiSpawnPoints;

    private void Start()
    {
        SpawnRound();
    }

    private void SpawnRound()
    {
        AnimalDefinition[] animals = animalLibrary.GetAnimals();

        if (animals.Length < RequiredAnimalCount)
        {
            Debug.LogError(
                $"RoundSpawner: At least {RequiredAnimalCount} animals are required.",
                this
            );
            return;
        }

        if (aiSpawnPoints.Length < AiCount)
        {
            Debug.LogError(
                $"RoundSpawner: At least {AiCount} spawn points are required.",
                this
            );
            return;
        }

        // Shuffle the copy without changing the Inspector list.
        Shuffle(animals);

        player.SetAnimal(animals[0]);

        for (int i = 0; i < AiCount; i++)
        {
            Transform spawnPoint = aiSpawnPoints[i];

            AnimalActor ai = Instantiate(
                aiPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                aiParent
            );

            ai.SetAnimal(animals[i + 1]);
        }
    }

    private static void Shuffle(AnimalDefinition[] animals)
    {
        // Fisher-Yates gives every ordering an equal chance.
        for (int i = animals.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            AnimalDefinition temporary = animals[i];
            animals[i] = animals[randomIndex];
            animals[randomIndex] = temporary;
        }
    }
}