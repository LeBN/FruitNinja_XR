using UnityEngine;
using System.Collections;

public class FruitSpawnerManager : MonoBehaviour
{
    [Header("Spawners")]
    public Transform[] spawners;

    [Header("Objets a lancer")]
    public GameObject[] fruitPrefabs;
    public GameObject bombPrefab;
    public GameObject goldenFruitPrefab;


    [Header("Parametres de spawn")]
    [Range(0f, 1f)]
    public float bombSpawnChance = 0.15f;
    public float spawnInterval = 1.5f;
    public float launchForce = 4f;
    public float minRandomDelay = 0.3f;
    [Tooltip("Nombre d'objets lancés par vague (réglé par la difficulté).")]
    public int minObjectsPerWave = 1;
    public int maxObjectsPerWave = 3;

    private bool spawningActive = false;

    private IEnumerator SpawnRoutine()
    {
        while (spawningActive)
        {
            yield return new WaitForSeconds(spawnInterval + Random.Range(-minRandomDelay, minRandomDelay));

            // Choisit combien d’objets spawnent cette fois (selon la difficulté)
            int objectsToSpawn = Random.Range(minObjectsPerWave, Mathf.Max(minObjectsPerWave, maxObjectsPerWave) + 1);
            for (int i = 0; i < objectsToSpawn; i++)
            {
                SpawnRandomObject();
                yield return new WaitForSeconds(0.2f); // petit délai entre les spawns d’un même groupe
            }
        }
    }

    private void SpawnRandomObject()
    {
        if (spawners.Length == 0) return;

        Transform spawner = spawners[Random.Range(0, spawners.Length)];
        GameObject prefabToSpawn = null;

        float goldenChance = 0.10f;


        // Fruit doré (10 %), sinon bombe (selon la difficulté), sinon fruit normal.
        // (Avant, le "else if" écrasait la bombe par un fruit : les bombes n'apparaissaient presque jamais.)
        if (goldenFruitPrefab != null && Random.value < goldenChance)
            prefabToSpawn = goldenFruitPrefab;
        else if (bombPrefab != null && Random.value < bombSpawnChance)
            prefabToSpawn = bombPrefab;
        else if (fruitPrefabs.Length > 0)
            prefabToSpawn = fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];

        if (prefabToSpawn == null) return;

        // Le canon du lanceur surgit, tire le fruit (son 3D) puis disparaît
        LanceurCanon canon = spawner.GetComponent<LanceurCanon>();
        if (canon != null)
            canon.Lancer(() => LaunchObject(prefabToSpawn, canon.PointDeSortie));
        else
            LaunchObject(prefabToSpawn, spawner.position + Vector3.up * 0.5f);
    }

    private void LaunchObject(GameObject prefabToSpawn, Vector3 spawnPos)
    {
        if (!spawningActive) return;
        GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // Auto-destruction après 1,5 s
        Destroy(obj, 1.5f);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Direction verticale avec tres legere deviation
            Vector3 launchDir = Vector3.up + new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
            rb.AddForce(launchDir.normalized * launchForce, ForceMode.VelocityChange);
            rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.VelocityChange);
        }
    }

    public void StartSpawning()
    {
        if (!spawningActive)
        {
            spawningActive = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        spawningActive = false;
        StopAllCoroutines();
        foreach (var spawner in spawners)
        {
            var canon = spawner != null ? spawner.GetComponent<LanceurCanon>() : null;
            if (canon != null) canon.Cacher();
        }
    }
}
