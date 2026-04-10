using UnityEngine;

/// <summary>
/// ObstacleSpawner - Spawns paired floor/roof obstacles with a random gap.
///
/// Each obstacle column has a TOP piece (ceiling bar) and a BOTTOM piece (floor bar)
/// with a gap between them that the player must fly through.
///
/// SETUP:
///   1. Create an empty GameObject called "ObstacleSpawner" and attach this script.
///   2. Create a simple obstacle prefab:
///        - A Square sprite (same one you already use for Ground)
///        - SpriteRenderer (any color/sprite you like)
///        - BoxCollider2D with IsTrigger = FALSE
///        - Tag set to "Ground"
///        - Attach the Obstacle.cs script
///      OR leave obstaclePrefab empty and the spawner will generate them at runtime.
///   3. Adjust gap size, spawn interval, speed, and pool size in the Inspector.
///
/// The spawner reuses objects via pooling to avoid garbage collection.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefab (optional)")]
    [Tooltip("Prefab for a single obstacle bar. If empty, spawner creates plain squares at runtime.")]
    public GameObject obstaclePrefab;

    [Header("Spawn Settings")]
    [Tooltip("X position where new columns spawn (off-screen right).")]
    public float spawnX = 54f;

    [Tooltip("Seconds between each new column.")]
    public float spawnInterval = 2.5f;

    [Tooltip("How fast obstacles scroll left (units/sec). Should match coin speed.")]
    public float scrollSpeed = 4f;

    [Header("Gap Settings")]
    [Tooltip("Vertical size of the gap the player flies through.")]
    public float gapSize = 7f;

    [Tooltip("Minimum Y center of the gap.")]
    public float gapMinY = -2f;

    [Tooltip("Maximum Y center of the gap.")]
    public float gapMaxY = 2f;

    [Header("Single Pillar Chance")]
    [Tooltip("Chance (0-1) that a column spawns with only one pillar (top OR bottom) instead of both.")]
    [Range(0f, 1f)]
    public float singlePillarChance = 0.3f;

    [Tooltip("How much of the screen (0-1) a single pillar covers. 0.5 = half the screen, 0.7 = most of it.")]
    [Range(0.3f, 0.85f)]
    public float singlePillarScreenCoverage = 0.6f;

    [Header("Bar Dimensions")]
    [Tooltip("How tall each obstacle bar is (should be big enough to block the screen).")]
    public float barHeight = 10f;

    [Tooltip("How wide each obstacle bar is.")]
    public float barWidth = 1.2f;

    [Header("Pool Settings")]
    [Tooltip("Max obstacle columns in the pool.")]
    public int maxColumns = 8;

    [Tooltip("X position where columns get recycled (off-screen left).")]
    public float despawnX = -12f;

    // ---- Private ----
    private Transform[][] pool;     // pool[i][0] = top bar, pool[i][1] = bottom bar
    private int poolIndex = 0;
    private float timer = 0f;

    void Start()
    {
        pool = new Transform[maxColumns][];

        for (int i = 0; i < maxColumns; i++)
        {
            pool[i] = new Transform[2];

            // Top bar
            pool[i][0] = CreateBar("ObstacleTop_" + i);
            pool[i][0].gameObject.SetActive(false);

            // Bottom bar
            pool[i][1] = CreateBar("ObstacleBot_" + i);
            pool[i][1].gameObject.SetActive(false);
        }
    }

    Transform CreateBar(string name)
    {
        GameObject bar;

        if (obstaclePrefab != null)
        {
            bar = Instantiate(obstaclePrefab);
            bar.name = name;
        }
        else
        {
            // Create a plain square at runtime
            bar = new GameObject(name);
            var sr = bar.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSquareSprite();
            sr.color = new Color(0.3f, 0.3f, 0.3f, 1f); // dark grey
            var col = bar.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            bar.tag = "Ground";
        }

        // Add the Obstacle movement script if not already on it
        if (bar.GetComponent<Obstacle>() == null)
            bar.AddComponent<Obstacle>();

        bar.transform.parent = transform;
        return bar.transform;
    }

    void Update()
    {
        if (pool == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnColumn();
        }

        // Recycle columns that have scrolled off-screen (check whichever pillar is active)
        for (int i = 0; i < maxColumns; i++)
        {
            bool topActive = pool[i][0].gameObject.activeSelf;
            bool botActive = pool[i][1].gameObject.activeSelf;

            if (topActive && pool[i][0].position.x < despawnX)
                pool[i][0].gameObject.SetActive(false);

            if (botActive && pool[i][1].position.x < despawnX)
                pool[i][1].gameObject.SetActive(false);
        }
    }

    void SpawnColumn()
    {
        // Find an inactive pair
        for (int attempt = 0; attempt < maxColumns; attempt++)
        {
            int idx = (poolIndex + attempt) % maxColumns;
            if (!pool[idx][0].gameObject.activeSelf)
            {
                // Decide if this is a single-pillar column
                // 0 = both pillars, 1 = top only (gap at bottom), 2 = bottom only (gap at top)
                int pillarMode = 0;
                if (Random.value < singlePillarChance)
                {
                    pillarMode = Random.value < 0.5f ? 1 : 2;
                }

                if (pillarMode == 0)
                {
                    // ---- DOUBLE PILLAR: top + bottom with gap between ----
                    float gapCenter = Random.Range(gapMinY, gapMaxY);

                    float topY = gapCenter + gapSize / 2f + barHeight / 2f;
                    pool[idx][0].position = new Vector3(spawnX, topY, 0f);
                    pool[idx][0].localScale = new Vector3(barWidth, barHeight, 1f);
                    pool[idx][0].gameObject.SetActive(true);
                    var topObs = pool[idx][0].GetComponent<Obstacle>();
                    if (topObs != null) topObs.moveSpeed = scrollSpeed;

                    float botY = gapCenter - gapSize / 2f - barHeight / 2f;
                    pool[idx][1].position = new Vector3(spawnX, botY, 0f);
                    pool[idx][1].localScale = new Vector3(barWidth, barHeight, 1f);
                    pool[idx][1].gameObject.SetActive(true);
                    var botObs = pool[idx][1].GetComponent<Obstacle>();
                    if (botObs != null) botObs.moveSpeed = scrollSpeed;
                }
                else
                {
                    // ---- SINGLE PILLAR: same gap size, but only one bar ----
                    // The pillar height = screen height minus the gap, so the
                    // opening left is exactly gapSize — same as double pillars.
                    Camera cam = Camera.main;
                    float screenTop = cam != null ? cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)).y : 5f;
                    float screenBot = cam != null ? cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0f)).y : -5f;
                    float screenH = screenTop - screenBot;
                    float pillarH = screenH - gapSize;

                    if (pillarMode == 1)
                    {
                        // Top pillar: hangs from the top, leaves gapSize opening at the bottom
                        float y = screenTop - pillarH / 2f;
                        pool[idx][0].position = new Vector3(spawnX, y, 0f);
                        pool[idx][0].localScale = new Vector3(barWidth, pillarH, 1f);
                        pool[idx][0].gameObject.SetActive(true);
                        var obs = pool[idx][0].GetComponent<Obstacle>();
                        if (obs != null) obs.moveSpeed = scrollSpeed;
                        pool[idx][1].gameObject.SetActive(false);
                    }
                    else
                    {
                        // Bottom pillar: rises from the bottom, leaves gapSize opening at the top
                        float y = screenBot + pillarH / 2f;
                        pool[idx][1].position = new Vector3(spawnX, y, 0f);
                        pool[idx][1].localScale = new Vector3(barWidth, pillarH, 1f);
                        pool[idx][1].gameObject.SetActive(true);
                        var obs = pool[idx][1].GetComponent<Obstacle>();
                        if (obs != null) obs.moveSpeed = scrollSpeed;
                        pool[idx][0].gameObject.SetActive(false);
                    }
                }

                poolIndex = (idx + 1) % maxColumns;
                return;
            }
        }
        // Pool full, skip this spawn
    }

    /// <summary>
    /// Creates a 1x1 white sprite at runtime so we don't need any asset reference.
    /// You can replace this with your own sprite by assigning a prefab instead.
    /// </summary>
    private Sprite cachedSprite;
    Sprite MakeWhiteSquareSprite()
    {
        if (cachedSprite != null) return cachedSprite;

        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        cachedSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        return cachedSprite;
    }
}
