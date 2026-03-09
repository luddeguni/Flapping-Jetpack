using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Handles game restart and overall loop.
///
/// SETUP:
///   1. Attach to an empty "GameManager" GameObject.
///   2. In your Game Over UI panel, add a Button and wire its onClick to GameManager.RestartGame().
/// </summary>
public class GameManager : MonoBehaviour
{
    [Tooltip("Assign your Player GameObject here.")]
    public GameObject playerPrefab;

    [Tooltip("The spawn position for the player.")]
    public Transform playerSpawnPoint;

    private ScoreManager scoreManager;
    private CoinSpawner coinSpawner;

    void Awake()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        coinSpawner = FindFirstObjectByType<CoinSpawner>();
    }

    /// <summary>
    /// Called by the Restart button in the Game Over panel.
    /// Reloads the active scene — simplest and cleanest approach.
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}