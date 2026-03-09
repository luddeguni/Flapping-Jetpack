using UnityEngine;

/// <summary>
/// CoinSpawner - Continuously spawns coins in the world for the player to collect.
///
/// SETUP:
///   1. Create an empty GameObject called "CoinSpawner" and attach this script.
///   2. Create a Coin prefab:
///        - Sprite of your choice
///        - CircleCollider2D with IsTrigger = TRUE
///        - Tag set to "Coin"
///        - Optionally add the CoinRotate script for a spinning effect
///   3. Assign the coin prefab to the "coinPrefab" field in the Inspector.
///   4. Adjust spawn area, interval, and pool size as needed.
/// </summary>
public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Prefab")]
    public GameObject coinPrefab;

    [Header("Spawn Area")]
    [Tooltip("Rightmost X position to spawn coins.")]
    public float spawnX = 8f;

    [Tooltip("Vertical range: coins spawn between -spawnYRange and +spawnYRange.")]
    public float spawnYRange = 3.5f;

    [Header("Spawn Settings")]
    [Tooltip("Seconds between each coin spawn.")]
    public float spawnInterval = 2.5f;

    [Tooltip("Maximum active coins in the scene at once.")]
    public int maxCoins = 8;

    [Tooltip("Coins further left than this X value get recycled.")]
    public float despawnX = -10f;

    // ---- Private ----
    private float timer = 0f;
    private GameObject[] pool;
    private int poolIndex = 0;

    void Start()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("CoinSpawner: No coin prefab assigned!");
            return;
        }

        // Pre-warm pool
        pool = new GameObject[maxCoins];
        for (int i = 0; i < maxCoins; i++)
        {
            pool[i] = Instantiate(coinPrefab);
            pool[i].SetActive(false);
        }
    }

    void Update()
    {
        if (pool == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnCoin();
        }

        // Recycle coins that have drifted too far left (if you scroll the camera/world)
        // Remove this loop if coins are stationary
        foreach (var coin in pool)
        {
            if (coin.activeSelf && coin.transform.position.x < despawnX)
                coin.SetActive(false);
        }
    }

    void SpawnCoin()
    {
        // Find the next inactive coin in the pool
        for (int i = 0; i < maxCoins; i++)
        {
            int idx = (poolIndex + i) % maxCoins;
            if (!pool[idx].activeSelf)
            {
                float y = Random.Range(-spawnYRange, spawnYRange);
                pool[idx].transform.position = new Vector3(spawnX, y, 0f);
                pool[idx].SetActive(true);
                poolIndex = (idx + 1) % maxCoins;
                return;
            }
        }
        // All coins are active — pool is full, skip this spawn
    }
}