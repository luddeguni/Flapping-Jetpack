using UnityEngine;

/// <summary>
/// Coin - Moves the coin leftward so it drifts toward (and past) the player.
/// Attach this to your Coin prefab.
///
/// If a DifficultyManager exists in the scene, the coin automatically
/// matches the current game speed. Otherwise it uses the default moveSpeed.
/// </summary>
public class Coin : MonoBehaviour
{
    [Tooltip("Base speed if no DifficultyManager is present (units/sec).")]
    public float moveSpeed = 4f;

    private DifficultyManager difficultyManager;

    void OnEnable()
    {
        // Cache reference each time the coin is activated (pooled objects)
        if (difficultyManager == null)
            difficultyManager = FindFirstObjectByType<DifficultyManager>();
    }

    void Update()
    {
        float speed = (difficultyManager != null) ? difficultyManager.CurrentSpeed : moveSpeed;
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }
}
