using UnityEngine;

/// <summary>
/// DifficultyManager - Gradually ramps up the game difficulty over time.
///
/// Controls:
///   - Scroll speed (obstacles + coins move faster)
///   - Gap size (obstacle openings get smaller)
///   - Spawn interval (obstacles appear more frequently)
///
/// SETUP:
///   1. Create an empty GameObject called "DifficultyManager" and attach this script.
///   2. It will automatically find ObstacleSpawner, CoinSpawner, and FloorScript in the scene.
///   3. Tweak the min/max values to control how hard the game eventually gets.
///
/// The difficulty increases linearly over "rampDuration" seconds, then stays at max.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Seconds it takes to go from easiest to hardest difficulty.")]
    public float rampDuration = 120f;

    [Header("Scroll Speed")]
    [Tooltip("Starting scroll speed (units/sec).")]
    public float startSpeed = 4f;
    [Tooltip("Maximum scroll speed at full difficulty.")]
    public float maxSpeed = 10f;

    [Header("Obstacle Gap")]
    [Tooltip("Starting gap size (easy, big opening).")]
    public float startGapSize = 7f;
    [Tooltip("Minimum gap size at full difficulty (hard, tight opening).")]
    public float minGapSize = 3f;

    [Header("Obstacle Spawn Interval")]
    [Tooltip("Starting seconds between obstacle columns.")]
    public float startSpawnInterval = 2.5f;
    [Tooltip("Minimum interval at full difficulty (more frequent obstacles).")]
    public float minSpawnInterval = 1.2f;

    [Header("Coin Spawn Interval")]
    [Tooltip("Starting seconds between coin spawns.")]
    public float startCoinInterval = 2.5f;
    [Tooltip("Minimum coin interval at full difficulty.")]
    public float minCoinInterval = 1.0f;

    // ---- References (auto-found) ----
    private ObstacleSpawner obstacleSpawner;
    private CoinSpawner coinSpawner;
    private FloorScript floorScript;

    private float elapsed = 0f;

    void Start()
    {
        obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();
        coinSpawner = FindFirstObjectByType<CoinSpawner>();
        floorScript = FindFirstObjectByType<FloorScript>();

        // Apply starting values
        ApplyDifficulty(0f);

        // Debug: verify connections and starting values
        Debug.Log($"[DifficultyManager] ObstacleSpawner found: {obstacleSpawner != null}");
        Debug.Log($"[DifficultyManager] Starting gap: {startGapSize}, Min gap: {minGapSize}, Ramp: {rampDuration}s");
        if (obstacleSpawner != null)
            Debug.Log($"[DifficultyManager] ObstacleSpawner.gapSize is now: {obstacleSpawner.gapSize}");
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / rampDuration);
        ApplyDifficulty(t);
    }

    /// <summary>
    /// Apply difficulty based on progress t (0 = start, 1 = max difficulty).
    /// </summary>
    void ApplyDifficulty(float t)
    {
        float currentSpeed = Mathf.Lerp(startSpeed, maxSpeed, t);
        float currentGap = Mathf.Lerp(startGapSize, minGapSize, t);
        float currentObstacleInterval = Mathf.Lerp(startSpawnInterval, minSpawnInterval, t);
        float currentCoinInterval = Mathf.Lerp(startCoinInterval, minCoinInterval, t);

        // Update obstacle spawner
        if (obstacleSpawner != null)
        {
            obstacleSpawner.scrollSpeed = currentSpeed;
            obstacleSpawner.gapSize = currentGap;
            obstacleSpawner.spawnInterval = currentObstacleInterval;
        }

        // Update coin spawner & existing coin speeds
        if (coinSpawner != null)
        {
            coinSpawner.spawnInterval = currentCoinInterval;
        }

        // Update background scroll to match game speed
        if (floorScript != null)
        {
            floorScript.speed = currentSpeed * 0.5f; // background scrolls a bit slower for parallax feel
        }
    }

    /// <summary>
    /// Current difficulty progress from 0 (easiest) to 1 (hardest).
    /// Useful if other scripts want to know the current difficulty.
    /// </summary>
    public float DifficultyProgress => Mathf.Clamp01(elapsed / rampDuration);

    /// <summary>
    /// Current scroll speed. Useful for Coin.cs to match obstacle speed.
    /// </summary>
    public float CurrentSpeed => Mathf.Lerp(startSpeed, maxSpeed, DifficultyProgress);
}
