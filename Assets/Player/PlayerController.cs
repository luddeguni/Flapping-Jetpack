using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerController - Flappy Bird inspired movement.
/// Uses the New Input System package.
///
/// FEATURES:
///   - Hold to rise, release to fall
///   - Screen boundary push (bounces off top/bottom of screen)
///   - Extra life: first wall hit triggers 2s of blinking invincibility
///   - Landing on top/bottom of pillars doesn't kill you
///
/// SETUP:
///   1. Add this script to your Player GameObject.
///   2. Player needs a Rigidbody2D (Gravity Scale = 0, Collision Detection = Continuous).
///   3. Player needs a Collider2D (IsTrigger = false for obstacles).
///   4. Tag coins as "Coin" and give them a Collider2D with IsTrigger = true.
///   5. Needs a SpriteRenderer for the blink effect.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Upward velocity applied while holding the button.")]
    public float riseForce = 8f;

    [Tooltip("How quickly the upward force fades out after releasing (higher = faster fade).")]
    public float riseFadeSpeed = 6f;

    [Tooltip("Maximum downward (fall) speed.")]
    public float maxFallSpeed = -12f;

    [Tooltip("Gravity applied when not holding button.")]
    public float gravityStrength = 20f;

    [Header("Screen Boundaries")]
    [Tooltip("Push force applied when hitting the top or bottom of the screen.")]
    public float boundaryPushForce = 5f;

    [Tooltip("Padding from screen edge in viewport units (0.02 = 2% from edge).")]
    public float boundaryPadding = 0.02f;

    [Header("Extra Life")]
    [Tooltip("How many extra lives the player starts with.")]
    public int extraLives = 1;

    [Tooltip("Duration of invincibility after losing a life (seconds).")]
    public float invincibilityDuration = 4f;

    [Tooltip("How fast the sprite blinks at the START of invincibility (toggles per second).")]
    public float blinkRateStart = 20f;

    [Tooltip("How fast the sprite blinks at the END of invincibility (slower = becoming solid).")]
    public float blinkRateEnd = 3f;

    // ---- Private ----
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float currentVerticalVelocity = 0f;
    private bool isHolding = false;
    private bool isDead = false;

    // Extra life / invincibility
    private int currentExtraLives;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private Collider2D playerCollider;

    // Screen bounds (world space)
    private float screenTop;
    private float screenBottom;
    private Camera mainCam;

    private ScoreManager scoreManager;

    // New Input System
    private InputAction flyAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        currentExtraLives = extraLives;

        mainCam = Camera.main;
        UpdateScreenBounds();

        flyAction = new InputAction("Fly", binding: "<Keyboard>/space");
        flyAction.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable()
    {
        flyAction.Enable();
    }

    void OnDisable()
    {
        flyAction.Disable();
    }

    void Update()
    {
        if (isDead) return;

        isHolding = flyAction.IsPressed();

        // Update screen bounds in case the window resizes
        UpdateScreenBounds();

        // Handle invincibility timer and blink
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

            // Blink speed slows down over time (fast at start → slow at end)
            float progress = 1f - Mathf.Clamp01(invincibilityTimer / invincibilityDuration);
            float currentBlinkRate = Mathf.Lerp(blinkRateStart, blinkRateEnd, progress);

            // Blink the sprite
            if (spriteRenderer != null)
            {
                bool visible = (Mathf.FloorToInt(Time.time * currentBlinkRate) % 2) == 0;
                spriteRenderer.enabled = visible;
            }

            if (invincibilityTimer <= 0f)
            {
                EndInvincibility();
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isHolding)
        {
            currentVerticalVelocity = Mathf.Lerp(currentVerticalVelocity, riseForce, riseFadeSpeed * Time.fixedDeltaTime * 2f);
        }
        else
        {
            currentVerticalVelocity -= gravityStrength * Time.fixedDeltaTime;

            if (currentVerticalVelocity > 0f)
            {
                currentVerticalVelocity = Mathf.MoveTowards(currentVerticalVelocity, 0f, riseFadeSpeed * Time.fixedDeltaTime);
            }
        }

        // Clamp to max fall speed
        currentVerticalVelocity = Mathf.Max(currentVerticalVelocity, maxFallSpeed);

        rb.linearVelocity = new Vector2(0f, currentVerticalVelocity);

        // ---- Screen boundary push ----
        float playerY = transform.position.y;

        if (playerY >= screenTop)
        {
            // Push down
            transform.position = new Vector3(transform.position.x, screenTop, transform.position.z);
            currentVerticalVelocity = -boundaryPushForce;
        }
        else if (playerY <= screenBottom)
        {
            // Push up
            transform.position = new Vector3(transform.position.x, screenBottom, transform.position.z);
            currentVerticalVelocity = boundaryPushForce;
        }
    }

    // ---- Collision: solid obstacles ----
    void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;
        if (isInvincible) return; // ignore collisions during invincibility

        // Check collision direction
        foreach (ContactPoint2D contact in col.contacts)
        {
            // Wall hit (mostly horizontal normal) → lose a life or die
            if (Mathf.Abs(contact.normal.x) > Mathf.Abs(contact.normal.y))
            {
                if (currentExtraLives > 0)
                {
                    // Use an extra life — become invincible and blink
                    currentExtraLives--;
                    StartInvincibility();
                    return;
                }
                else
                {
                    Die();
                    return;
                }
            }
        }

        // Top/bottom hit — just block movement, no death
    }

    // Also ignore collisions that continue during invincibility
    void OnCollisionStay2D(Collision2D col)
    {
        // Do nothing during invincibility — physics still blocks movement
        // but we don't process damage
    }

    // ---- Trigger: coins + pass-through during invincibility ----
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Coin"))
        {
            scoreManager?.AddCoinBonus();
            other.gameObject.SetActive(false);
            return;
        }

        // During invincibility we phase through everything (obstacles become triggers)
        if (isInvincible) return;

        // Kill zones etc.
        if (currentExtraLives > 0)
        {
            currentExtraLives--;
            StartInvincibility();
        }
        else
        {
            Die();
        }
    }

    void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // Disable collider so player passes through obstacles
        if (playerCollider != null)
            playerCollider.isTrigger = true;

        // Give a small push so the player doesn't get stuck
        currentVerticalVelocity = boundaryPushForce * 0.5f;
    }

    void EndInvincibility()
    {
        isInvincible = false;

        // Re-enable solid collisions
        if (playerCollider != null)
            playerCollider.isTrigger = false;

        // Make sure sprite is visible
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    void Die()
    {
        isDead = true;
        isInvincible = false;
        rb.linearVelocity = Vector2.zero;

        // Reset visuals and collider
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        if (playerCollider != null)
            playerCollider.isTrigger = false;

        scoreManager?.OnPlayerDied();
        Invoke(nameof(DisablePlayer), 0.15f);
    }

    void DisablePlayer()
    {
        gameObject.SetActive(false);
    }

    void UpdateScreenBounds()
    {
        if (mainCam == null) return;
        Vector3 top = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 1f - boundaryPadding, 0f));
        Vector3 bot = mainCam.ViewportToWorldPoint(new Vector3(0.5f, boundaryPadding, 0f));
        screenTop = top.y;
        screenBottom = bot.y;
    }

    public bool IsDead => isDead;
    public int ExtraLivesRemaining => currentExtraLives;
    public bool IsInvincible => isInvincible;
}
