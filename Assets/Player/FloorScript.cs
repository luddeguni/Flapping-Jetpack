using UnityEngine;

/// <summary>
/// FloorScript - Two-layer parallax scrolling background.
///
/// SETUP:
///   1. Create TWO quad/sprite GameObjects for the background:
///      - BACK LAYER: your colour strip (wellwellwell.png) — scrolls slowly behind
///      - FRONT LAYER: your stars texture — scrolls faster on top
///   2. Both need a Renderer (SpriteRenderer or MeshRenderer with a material).
///   3. Assign them to frontRenderer and backRenderer in the Inspector.
///   4. Make sure both textures have Wrap Mode set to REPEAT in Unity's texture import settings.
///   5. The front layer material should use a transparent shader (e.g. Sprites/Default)
///      so the colour layer shows through the black parts of the stars.
///
/// The DifficultyManager will update "speed" over time automatically.
/// </summary>
public class FloorScript : MonoBehaviour
{
    [Header("Speed")]
    [Tooltip("Base scroll speed (units/sec). Updated by DifficultyManager.")]
    public float speed = 2f;

    [Header("Front Layer (Stars)")]
    [Tooltip("The Renderer for the front stars layer. Scrolls at full speed.")]
    public Renderer frontRenderer;

    [Header("Back Layer (Colours)")]
    [Tooltip("The Renderer for the back colour layer. Scrolls slower for parallax.")]
    public Renderer backRenderer;

    [Tooltip("How much slower the back layer scrolls compared to front (0.3 = 30% of front speed).")]
    [Range(0.05f, 0.9f)]
    public float backLayerSpeedRatio = 0.3f;

    void Start()
    {
        // Auto-find renderers if not assigned
        if (frontRenderer == null)
        {
            frontRenderer = GetComponent<Renderer>();
            if (frontRenderer != null)
                Debug.Log("[FloorScript] Auto-found front Renderer on this GameObject.");
        }

        if (frontRenderer == null && backRenderer == null)
            Debug.LogWarning("[FloorScript] No renderers assigned! Assign frontRenderer and/or backRenderer.");
    }

    void Update()
    {
        float offset = speed * Time.deltaTime;

        // Front layer (stars) — scrolls at full speed
        if (frontRenderer != null)
        {
            frontRenderer.material.mainTextureOffset += new Vector2(offset, 0);
        }

        // Back layer (colours) — scrolls slower for parallax depth
        if (backRenderer != null)
        {
            backRenderer.material.mainTextureOffset += new Vector2(offset * backLayerSpeedRatio, 0);
        }
    }
}
