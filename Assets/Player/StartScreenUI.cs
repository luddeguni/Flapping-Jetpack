using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StartScreenUI - Shows a start screen that freezes the game until the player presses Start.
///
/// SETUP:
///   1. Create an empty GameObject called "StartScreen" and attach this script.
///   2. That's it! The screen auto-creates its own canvas, title, and button.
///   3. The game is paused (Time.timeScale = 0) until Start is pressed.
/// </summary>
public class StartScreenUI : MonoBehaviour
{
    [Header("Text")]
    public string gameTitle = "FLAPPING JETPACK";
    public string buttonLabel = "START";

    [Header("Colors")]
    public Color overlayColor = new Color(0f, 0f, 0f, 0.85f);
    public Color titleColor = Color.white;
    public Color buttonColor = new Color(0.2f, 0.75f, 0.3f, 1f);
    public Color buttonTextColor = Color.white;

    [Header("Sizing")]
    public int titleFontSize = 72;
    public int buttonFontSize = 42;

    private GameObject panel;

    void Awake()
    {
        // Freeze the game immediately
        Time.timeScale = 0f;
        BuildUI();
    }

    void BuildUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("StartScreenCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // on top of everything
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Dark overlay
        panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = overlayColor;
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        TMP_Text titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = gameTitle;
        titleText.fontSize = titleFontSize;
        titleText.color = titleColor;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 100);
        titleRect.sizeDelta = new Vector2(800, 100);

        // Best score
        int best = PlayerPrefs.GetInt("BestScore", 0);
        if (best > 0)
        {
            GameObject bestObj = new GameObject("BestScore");
            bestObj.transform.SetParent(panel.transform, false);
            TMP_Text bestText = bestObj.AddComponent<TextMeshProUGUI>();
            bestText.text = $"Best: {best}";
            bestText.fontSize = 36;
            bestText.color = new Color(1f, 0.85f, 0.3f, 1f);
            bestText.alignment = TextAlignmentOptions.Center;
            RectTransform bestRect = bestObj.GetComponent<RectTransform>();
            bestRect.anchorMin = new Vector2(0.5f, 0.5f);
            bestRect.anchorMax = new Vector2(0.5f, 0.5f);
            bestRect.pivot = new Vector2(0.5f, 0.5f);
            bestRect.anchoredPosition = new Vector2(0, 20);
            bestRect.sizeDelta = new Vector2(400, 50);
        }

        // Start button
        GameObject btnObj = new GameObject("StartButton");
        btnObj.transform.SetParent(panel.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = buttonColor;
        Button btn = btnObj.AddComponent<Button>();
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, -80);
        btnRect.sizeDelta = new Vector2(300, 80);

        // Button text
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TMP_Text btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = buttonLabel;
        btnText.fontSize = buttonFontSize;
        btnText.color = buttonTextColor;
        btnText.alignment = TextAlignmentOptions.Center;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        btn.onClick.AddListener(StartGame);
    }

    void StartGame()
    {
        Time.timeScale = 1f;
        Destroy(panel.transform.parent.gameObject); // remove the whole canvas
        Destroy(gameObject); // remove this script's object too
    }
}
