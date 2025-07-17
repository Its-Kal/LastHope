using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Added for List

public class MapGen : MonoBehaviour
{
    public GameObject startPrefab;         // Prefab untuk Start
    public GameObject[] obstaclePrefabs;   // Prefab untuk Obstacle
    public GameObject finishPrefab;        // Prefab untuk Finish
    public Transform startPoint;           // Posisi awal (bisa empty GameObject di scene)
    public int obstacleCount = 10;
    public float obstacleSpacing = 80f;
    public float finishYOffset = 5f;
    public float finishYExtraOffset = 0f; // Offset tambahan untuk Y Finish
    public float finishSpacing = 5f; // Jarak antara obstacle terakhir dan finish (ditambah)
    public float finishXOffset = 3f;  // Offset ke kanan untuk finish
    public float spawnDelay = 2f; // Tambahkan ini
    public float startXOffset = 10f; // Contoh: geser 10 ke kanan
    public GameObject playerPrefab; // Drag prefab player ke sini di Inspector
    public Transform player; // Drag player ke sini di Inspector (tidak dipakai lagi, update otomatis)
    public float destroyDistance = 20f; // Jarak di belakang player untuk destroy
    // public float obstacleDestroyTime = 20f; // Tidak dipakai lagi
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private int nextObstacleIndex = 0; // Untuk cycling prefab
    private float currentZ = 0f; // Track posisi Z obstacle terakhir
    private bool finishSpawned = false; // Agar finish hanya di-spawn sekali

    void Start()
    {
        // Cache Renderer untuk startPrefab
        Renderer startRenderer = startPrefab.GetComponentInChildren<Renderer>();
        float startLength = startRenderer != null ? startRenderer.bounds.size.z : 1f;
        Vector3 startPos = startPoint.position + new Vector3(startXOffset, 0, startLength * 0.5f);
        GameObject startObj = Instantiate(startPrefab, startPos, Quaternion.identity);
        // Hapus startPoint hanya jika dia object di scene, bukan asset
        if (startPoint != null && startPoint.gameObject.scene.IsValid())
        {
            Destroy(startPoint.gameObject);
        }

        // Spawn player di posisi collider dengan tag "SpawnCar"
        GameObject spawnCarObj = GameObject.FindGameObjectWithTag("SpawnCar");
        if (spawnCarObj != null && playerPrefab != null)
        {
            Vector3 spawnPos = spawnCarObj.transform.position;
            GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player = playerObj.transform; // update reference jika perlu
        }

        // Inisialisasi currentZ
        currentZ = startPos.z + startLength * 0.5f;
        // Spawn 3 obstacle awal
        for (int i = 0; i < 3; i++)
        {
            SpawnNextObstacle();
        }
    }

    void SpawnNextObstacle()
    {
        if (finishSpawned) return;
        if (nextObstacleIndex >= obstacleCount)
        {
            // Spawn finishPrefab
            Renderer finishRenderer = finishPrefab.GetComponentInChildren<Renderer>();
            float finishLength = finishRenderer != null ? finishRenderer.bounds.size.z : 1f;
            currentZ += finishSpacing;
            Vector3 finishPos = new Vector3(
                startPoint.position.x + startXOffset + finishXOffset,
                startPoint.position.y + finishYOffset + finishYExtraOffset,
                currentZ
            );
            Instantiate(finishPrefab, finishPos, Quaternion.identity);
            finishSpawned = true;
            return;
        }
        int prefabIndex = nextObstacleIndex % obstaclePrefabs.Length;
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
        spawnedObstacles.Add(obstacle);
        currentZ += prefabLength * 0.5f;
        nextObstacleIndex++;
    }

    // Tidak perlu AddAutoDestroy lagi

    void Update()
    {
        if (player == null) return;
        // Hapus obstacle paling depan jika player sudah melewati obstacle berikutnya
        while (spawnedObstacles.Count > 1)
        {
            GameObject first = spawnedObstacles[0];
            GameObject next = spawnedObstacles[1];
            if (first == null || next == null)
            {
                spawnedObstacles.RemoveAt(0);
                continue;
            }
            if (player.position.z > next.transform.position.z)
            {
                Destroy(first);
                spawnedObstacles.RemoveAt(0);
                // Setelah destroy, spawn obstacle baru jika belum mencapai finish
                if (spawnedObstacles.Count < 3)
                {
                    SpawnNextObstacle();
                }
            }
            else
            {
                break; // Belum waktunya hapus
            }
        }
    }
}