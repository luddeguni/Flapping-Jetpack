using UnityEngine;
using TMPro; // Uses TextMeshPro - swap for UnityEngine.UI.Text if needed

/// <summary>
/// ScoreManager - Tracks survival time and coin pickups.
///
/// SETUP:
///   1. Create an empty GameObject called "ScoreManager" and attach this script.
///   2. In the Inspector, drag a TMP_Text UI element into the "scoreText" field.
///   3. Optionally add a "bestScoreText" field for high score display.
///   4. Coin bonus and points-per-second are configurable in the Inspector.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Scoring")]
    [Tooltip("Points earned per second of survival.")]
    public float pointsPerSecond = 10f;

    [Tooltip("Bonus points awarded per coin collected.")]
    public int coinBonus = 50;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text bestScoreText;
    public GameObject gameOverPanel; // Optional: a panel shown on death

    // ---- Private ----
    private float survivalScore = 0f;
    private int coinScore = 0;
    private bool isRunning = false;

    private const string BestScoreKey = "BestScore";

    void Start()
    {
        StartRun();
        UpdateBestScoreDisplay();
    }

    void Update()
    {
        if (!isRunning) return;

        survivalScore += pointsPerSecond * Time.deltaTime;
        RefreshUI();
    }

    // ---- Public API ----

    public void StartRun()
    {
        survivalScore = 0f;
        coinScore = 0;
        isRunning = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        RefreshUI();
    }

    public void AddCoinBonus()
    {
        coinScore += coinBonus;
        RefreshUI();
    }

    public void OnPlayerDied()
    {
        isRunning = false;
        SaveBestScore();
        UpdateBestScoreDisplay();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public int TotalScore => Mathf.FloorToInt(survivalScore) + coinScore;

    // ---- Private Helpers ----

    void RefreshUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {TotalScore}";
    }

    void SaveBestScore()
    {
        int best = PlayerPrefs.GetInt(BestScoreKey, 0);
        if (TotalScore > best)
        {
            PlayerPrefs.SetInt(BestScoreKey, TotalScore);
            PlayerPrefs.Save();
        }
    }

    void UpdateBestScoreDisplay()
    {
        if (bestScoreText != null)
            bestScoreText.text = $"Best: {PlayerPrefs.GetInt(BestScoreKey, 0)}";
    }
}