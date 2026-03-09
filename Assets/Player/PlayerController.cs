using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerController - Flappy Bird inspired movement.
/// Uses the New Input System package.
///
/// SETUP:
///   1. Add this script to your Player GameObject.
///   2. Player needs a Rigidbody2D (Gravity Scale = 0, Collision Detection = Continuous).
///   3. Player needs a Collider2D (IsTrigger = false for obstacles).
///   4. Tag coins as "Coin" and give them a Collider2D with IsTrigger = true.
///   5. Everything else the player can hit should NOT be tagged "Coin".
///
/// INPUT:
///   Uses the new Input System. By default listens for Space OR any gamepad South button.
///   No Input Action Asset needed — bindings are created in code.
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

    // ---- Private ----
    private Rigidbody2D rb;
    private float currentVerticalVelocity = 0f;
    private bool isHolding = false;
    private bool isDead = false;

    private ScoreManager scoreManager;

    // New Input System
    private InputAction flyAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // We handle gravity manually
        scoreManager = FindFirstObjectByType<ScoreManager>();

        // Build the fly action in code — Space key OR gamepad South button (A / Cross)
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

        // IsPressed() = true for the entire duration the button is held
        isHolding = flyAction.IsPressed();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isHolding)
        {
            // Instantly apply rise force (or lerp up for smoother feel)
            currentVerticalVelocity = Mathf.Lerp(currentVerticalVelocity, riseForce, riseFadeSpeed * Time.fixedDeltaTime * 2f);
        }
        else
        {
            // Fade the upward velocity and apply gravity downward
            currentVerticalVelocity -= gravityStrength * Time.fixedDeltaTime;

            // Fade out any remaining upward momentum gradually (the "fade" feel)
            if (currentVerticalVelocity > 0f)
            {
                currentVerticalVelocity = Mathf.MoveTowards(currentVerticalVelocity, 0f, riseFadeSpeed * Time.fixedDeltaTime);
            }
        }

        // Clamp to max fall speed
        currentVerticalVelocity = Mathf.Max(currentVerticalVelocity, maxFallSpeed);

        rb.linearVelocity = new Vector2(0f, currentVerticalVelocity);
    }

    // ---- Collision: solid obstacles ----
    void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;
        Die();
    }

    // ---- Trigger: coins ----
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Coin"))
        {
            scoreManager?.AddCoinBonus();
            other.gameObject.SetActive(false); // hide / destroy the coin
        }
        else
        {
            // Anything else with a trigger (e.g. kill zones) also kills the player
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        scoreManager?.OnPlayerDied();

        // Optional: play death animation, then disable
        // For now just disable after a tiny delay so camera can see it
        Invoke(nameof(DisablePlayer), 0.15f);
    }

    void DisablePlayer()
    {
        gameObject.SetActive(false);
    }

    public bool IsDead => isDead;
}