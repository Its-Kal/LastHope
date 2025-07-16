using UnityEngine;
using System.Collections;

public class MapGen : MonoBehaviour
{
    public GameObject startPrefab;         // Prefab untuk Start
    public GameObject[] obstaclePrefabs;   // Prefab untuk Obstacle
    public GameObject finishPrefab;        // Prefab untuk Finish
    public Transform startPoint;           // Posisi awal (bisa empty GameObject di scene)
    public int obstacleCount = 10;
    public float obstacleSpacing = 40f;
    public float finishYOffset = 5f;
    public float finishYExtraOffset = 0f; // Offset tambahan untuk Y Finish
    public float spawnDelay = 2f; // Tambahkan ini
    public float startXOffset = 10f; // Contoh: geser 10 ke kanan
    public Transform player; // Drag player ke sini di Inspector
    public float destroyDistance = 20f; // Jarak di belakang player untuk destroy
    public float obstacleDestroyTime = 20f; // Waktu destroy obstacle (detik)

    void Start()
    {
        StartCoroutine(GenerateMapStepByStep());
    }

    IEnumerator GenerateMapStepByStep()
    {
        // Cache Renderer untuk startPrefab
        Renderer startRenderer = startPrefab.GetComponentInChildren<Renderer>();
        float startLength = startRenderer != null ? startRenderer.bounds.size.z : 1f;
        Vector3 startPos = startPoint.position + new Vector3(startXOffset, 0, startLength * 0.5f);
        GameObject startObj = Instantiate(startPrefab, startPos, Quaternion.identity);
        // Tidak ada AddAutoDestroy di sini

        // Hapus OUT (startPoint) yang transform di scene
        Destroy(startPoint.gameObject);

        // Spawn player di atas Start
        if (player != null)
        {
            Vector3 playerSpawnPos = startObj.transform.position + new Vector3(0, 1f, 0);
            player.position = playerSpawnPos;
        }

        float currentZ = startPos.z + startLength * 0.5f;
        GameObject lastObstacle = null;

        // 2. Spawn Obstacles satu per satu
        for (int i = 0; i < obstacleCount; i++)
        {
            int prefabIndex = i % obstaclePrefabs.Length;
            GameObject prefab = obstaclePrefabs[prefabIndex];
            Renderer rend = prefab.GetComponentInChildren<Renderer>();
            float prefabLength = rend != null ? rend.bounds.size.z : 1f;

            currentZ += obstacleSpacing + prefabLength * 0.5f;

            Vector3 spawnPos = new Vector3(
                startPoint.position.x + startXOffset,
                startPoint.position.y,
                currentZ
            );
            GameObject obstacle = Instantiate(prefab, spawnPos, Quaternion.identity);
            AddAutoDestroy(obstacle); // Hanya obstacle yang auto-destroy

            currentZ += prefabLength * 0.5f;
            lastObstacle = obstacle;

            yield return new WaitForSeconds(spawnDelay);
        }

        // FINISH
        Renderer finishRenderer = finishPrefab.GetComponentInChildren<Renderer>();
        float finishLength = finishRenderer != null ? finishRenderer.bounds.size.z : 1f;
        Vector3 finishPos = new Vector3(
            lastObstacle.transform.position.x,
            lastObstacle.transform.position.y + finishYOffset + finishYExtraOffset,
            currentZ + finishLength * 0.5f
        );
        GameObject finishObj = Instantiate(finishPrefab, finishPos, Quaternion.identity);
        // Tidak ada AddAutoDestroy di sini
    }

    // Helper untuk menambah AutoDestroy
    void AddAutoDestroy(GameObject obj)
    {
        AutoDestroy ad = obj.AddComponent<AutoDestroy>();
        ad.destroyAfterSeconds = obstacleDestroyTime; // Atur dari Inspector
    }
}

public class AutoDestroy : MonoBehaviour
{
    public float destroyAfterSeconds = 10f; // Waktu sebelum destroy

    void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }
}