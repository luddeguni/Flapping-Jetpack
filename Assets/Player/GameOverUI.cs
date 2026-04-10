using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameOverUI - Builds a game-over overlay at runtime with score display and restart button.
///
/// SETUP:
///   1. Create an empty GameObject called "GameOverUI" and attach this script.
///   2. That's it! The script creates its own Canvas, panel, text, and button.
///   3. It hooks into ScoreManager (shows/hides on death) and GameManager (restart).
///
/// If you prefer to design your own UI, you can skip this script entirely and just
/// wire up a panel + button manually to ScoreManager.gameOverPanel and GameManager.RestartGame().
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Colors")]
    public Color panelColor = new Color(0f, 0f, 0f, 0.75f);
    public Color textColor = Color.white;
    public Color buttonColor = new Color(0.2f, 0.6f, 1f, 1f);
    public Color buttonTextColor = Color.white;

    [Header("Sizing")]
    public int titleFontSize = 60;
    public int scoreFontSize = 42;
    public int bestScoreFontSize = 32;
    public int buttonFontSize = 36;

    // ---- References built at runtime ----
    private GameObject panel;
    private TMP_Text finalScoreText;
    private TMP_Text bestScoreText;

    private ScoreManager scoreManager;
    private GameManager gameManager;

    void Awake()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        gameManager = FindFirstObjectByType<GameManager>();

        BuildUI();

        // Register this panel with ScoreManager so it shows on death
        if (scoreManager != null)
            scoreManager.gameOverPanel = panel;

        // Start hidden
        panel.SetActive(false);
    }

    void OnEnable()
    {
        // Update score text every time the panel becomes visible
        if (panel != null && panel.activeSelf)
            UpdateScoreDisplay();
    }

    void Update()
    {
        // Keep score text updated while panel is visible
        if (panel != null && panel.activeSelf)
            UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreManager != null)
        {
            if (finalScoreText != null)
                finalScoreText.text = $"Score: {scoreManager.TotalScore}";

            if (bestScoreText != null)
            {
                int best = PlayerPrefs.GetInt("BestScore", 0);
                bestScoreText.text = $"Best: {best}";
            }
        }
    }

    void BuildUI()
    {
        // ---- Canvas ----
        GameObject canvasObj = new GameObject("GameOverCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // on top of everything
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // ---- Dark overlay panel ----
        panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = panelColor;
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // ---- "GAME OVER" title ----
        CreateText(panel.transform, "TitleText", "GAME OVER", titleFontSize, textColor, new Vector2(0, 120));

        // ---- Score text ----
        finalScoreText = CreateText(panel.transform, "ScoreText", "Score: 0", scoreFontSize, textColor, new Vector2(0, 30));

        // ---- Best score text ----
        bestScoreText = CreateText(panel.transform, "BestScoreText", "Best: 0", bestScoreFontSize, new Color(1f, 0.85f, 0.3f, 1f), new Vector2(0, -30));

        // ---- Restart button ----
        GameObject btnObj = new GameObject("RestartButton");
        btnObj.transform.SetParent(panel.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = buttonColor;
        Button btn = btnObj.AddComponent<Button>();
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -110);
        btnRect.sizeDelta = new Vector2(300, 70);

        // Button text
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TMP_Text btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "RESTART";
        btnText.fontSize = buttonFontSize;
        btnText.color = buttonTextColor;
        btnText.alignment = TextAlignmentOptions.Center;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        // Wire up the restart button
        if (gameManager != null)
            btn.onClick.AddListener(gameManager.RestartGame);
        else
            btn.onClick.AddListener(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            });
    }

    TMP_Text CreateText(Transform parent, string name, string content, int fontSize, Color color, Vector2 position)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        TMP_Text tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(600, 80);
        return tmp;
    }
}
