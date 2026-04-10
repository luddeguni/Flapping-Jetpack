using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ScoreManager - Tracks survival time and coin pickups.
///
/// SETUP:
///   1. Create an empty GameObject called "ScoreManager" and attach this script.
///   2. Optionally drag TMP_Text UI elements into scoreText / bestScoreText fields.
///      If left empty, the script auto-creates a live score HUD in the top-left corner.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Scoring")]
    [Tooltip("Points earned per second of survival.")]
    public float pointsPerSecond = 10f;

    [Tooltip("Bonus points awarded per coin collected.")]
    public int coinBonus = 50;

    [Header("UI References (auto-created if empty)")]
    public TMP_Text scoreText;
    public TMP_Text bestScoreText;
    public GameObject gameOverPanel;

    [Header("Auto HUD Settings")]
    [Tooltip("Font size for the live score display.")]
    public int hudFontSize = 40;
    public Color hudColor = Color.white;

    // ---- Private ----
    private float survivalScore = 0f;
    private int coinScore = 0;
    private bool isRunning = false;

    private const string BestScoreKey = "BestScore";

    void Start()
    {
        // Auto-create score HUD if no text fields are assigned
        if (scoreText == null)
            BuildScoreHUD();

        StartRun();
        UpdateBestScoreDisplay();
    }

    void BuildScoreHUD()
    {
        // Create a Canvas for the in-game HUD
        GameObject canvasObj = new GameObject("ScoreHUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Score text — top center, big and visible
        scoreText = CreateHUDText(canvasObj.transform, "LiveScore", "0");
        RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 1f);
        scoreRect.anchorMax = new Vector2(0.5f, 1f);
        scoreRect.pivot = new Vector2(0.5f, 1f);
        scoreRect.anchoredPosition = new Vector2(0, -30);
        scoreRect.sizeDelta = new Vector2(500, 80);

        // Best score text — top right, smaller
        bestScoreText = CreateHUDText(canvasObj.transform, "BestScore", "Best: 0");
        bestScoreText.fontSize = hudFontSize * 0.55f;
        bestScoreText.color = new Color(1f, 0.85f, 0.3f, 0.9f);
        bestScoreText.alignment = TextAlignmentOptions.TopRight;
        RectTransform bestRect = bestScoreText.GetComponent<RectTransform>();
        bestRect.anchorMin = new Vector2(1f, 1f);
        bestRect.anchorMax = new Vector2(1f, 1f);
        bestRect.pivot = new Vector2(1f, 1f);
        bestRect.anchoredPosition = new Vector2(-20, -30);
        bestRect.sizeDelta = new Vector2(400, 60);

        Debug.Log("[ScoreManager] Auto-created score HUD");
    }

    TMP_Text CreateHUDText(Transform parent, string name, string content)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        TMP_Text tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = hudFontSize;
        tmp.color = hudColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        // Add outline for readability against any background
        tmp.outlineWidth = 0.3f;
        tmp.outlineColor = Color.black;
        return tmp;
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
            scoreText.text = TotalScore.ToString();
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