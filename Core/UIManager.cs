using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("===== SCREENS =====")]
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject achievementsScreen;

    [Header("===== MAIN MENU =====")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button achievementsButton;
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private TextMeshProUGUI menuHighScoreText;

    [Header("===== GAME SCREEN =====")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private Button undoButton;
    [SerializeField] private TextMeshProUGUI undoCountText;

    [Header("===== GAME OVER =====")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private GameObject newHighScoreObject;
    [SerializeField] private Button gameOverRestartButton;
    [SerializeField] private Button gameOverMenuButton;

    [Header("===== SETTINGS =====")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Button settingsBackButton;

    [Header("===== ACHIEVEMENTS =====")]
    [SerializeField] private Transform achievementListContent;
    [SerializeField] private TextMeshProUGUI achievementCountText;
    [SerializeField] private Button achievementsBackButton;

    private Difficulty lastGameDifficulty;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        SetupAllButtons();
    }

    private void Start()
    {
        ShowMainMenu();
    }

    private void SetupAllButtons()
    {
        if (playButton) playButton.onClick.AddListener(OnPlayClicked);
        if (settingsButton) settingsButton.onClick.AddListener(ShowSettings);
        if (achievementsButton) achievementsButton.onClick.AddListener(ShowAchievements);

        if (easyButton) easyButton.onClick.AddListener(() => StartGameWithDifficulty(Difficulty.Easy));
        if (mediumButton) mediumButton.onClick.AddListener(() => StartGameWithDifficulty(Difficulty.Medium));
        if (hardButton) hardButton.onClick.AddListener(() => StartGameWithDifficulty(Difficulty.Hard));

        if (backButton) backButton.onClick.AddListener(OnBackClicked);
        if (restartGameButton) restartGameButton.onClick.AddListener(OnRestartClicked);
        if (undoButton) undoButton.onClick.AddListener(OnUndoClicked);

        if (gameOverRestartButton) gameOverRestartButton.onClick.AddListener(OnRestartClicked);
        if (gameOverMenuButton) gameOverMenuButton.onClick.AddListener(OnBackClicked);

        if (settingsBackButton) settingsBackButton.onClick.AddListener(ShowMainMenu);
        if (soundToggle) soundToggle.onValueChanged.AddListener(OnSoundToggled);
        if (musicToggle) musicToggle.onValueChanged.AddListener(OnMusicToggled);
        if (vibrationToggle) vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);

        if (achievementsBackButton) achievementsBackButton.onClick.AddListener(ShowMainMenu);
    }

    private void OnPlayClicked()
    {
        if (difficultyPanel) difficultyPanel.SetActive(true);
    }

    private void StartGameWithDifficulty(Difficulty difficulty)
    {
        HideAllScreens();
        if (gameScreen) gameScreen.SetActive(true);
        if (modeText) modeText.text = difficulty.ToString().ToUpper() + " MODE";
        lastGameDifficulty = difficulty;
        if (GameManager.Instance) GameManager.Instance.StartGame(difficulty);
        if (AchievementManager.Instance) AchievementManager.Instance.OnGameStarted();
        UpdateBestScore();
    }

    private void OnBackClicked()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance) GameManager.Instance.ReturnToMenu();
        ShowMainMenu();
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        HideAllScreens();
        if (gameScreen) gameScreen.SetActive(true);
        if (GameManager.Instance) GameManager.Instance.RestartGame();
    }

    private void OnUndoClicked()
    {
        if (GameManager.Instance) GameManager.Instance.TryUndo();
    }

    private void OnSoundToggled(bool value)
    {
        if (SettingsManager.Instance) SettingsManager.Instance.SetSoundEnabled(value);
    }

    private void OnMusicToggled(bool value)
    {
        if (SettingsManager.Instance) SettingsManager.Instance.SetMusicEnabled(value);
    }

    private void OnVibrationToggled(bool value)
    {
        if (SettingsManager.Instance) SettingsManager.Instance.SetVibrationEnabled(value);
    }

    public void ShowMainMenu()
    {
        HideAllScreens();
        if (mainMenuScreen) mainMenuScreen.SetActive(true);
        if (difficultyPanel) difficultyPanel.SetActive(false);
        UpdateMenuHighScore();
    }

    public void ShowSettings()
    {
        HideAllScreens();
        if (settingsScreen)
        {
            settingsScreen.SetActive(true);
            UpdateSettingsUI();
        }
    }

    public void ShowAchievements()
    {
        HideAllScreens();
        if (achievementsScreen)
        {
            achievementsScreen.SetActive(true);
            PopulateAchievements();
        }
    }

    public void ShowGameOver(int finalScore, bool isNewHighScore)
    {
        if (gameOverScreen) gameOverScreen.SetActive(true);
        if (finalScoreText) finalScoreText.text = "Score: " + finalScore.ToString();
        if (newHighScoreObject) newHighScoreObject.SetActive(isNewHighScore);
        if (AchievementManager.Instance) AchievementManager.Instance.OnGameOver(lastGameDifficulty, finalScore);
    }

    private void PopulateAchievements()
    {
        if (AchievementManager.Instance == null)
        {
            Debug.LogError("AchievementManager not found!");
            return;
        }

        if (achievementListContent == null)
        {
            Debug.LogError("achievementListContent not assigned in UIManager!");
            return;
        }

        // Clear existing items
        foreach (Transform child in achievementListContent)
            Destroy(child.gameObject);

        List<Achievement> achievements = AchievementManager.Instance.GetAllAchievements();
        int unlocked = 0;

        foreach (Achievement a in achievements)
        {
            // Create achievement item
            GameObject item = new GameObject(a.id);
            item.transform.SetParent(achievementListContent, false);

            // Add RectTransform
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(350, 80);

            // Add background image
            Image bg = item.AddComponent<Image>();
            bg.color = a.unlocked ? new Color(0.2f, 0.6f, 0.2f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f);

            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(item.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(15, 5);
            textRt.offsetMax = new Vector2(-15, -5);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            string checkmark = a.unlocked ? "<color=green>[OK]</color> " : "<color=gray>[ ]</color> ";
            tmp.text = checkmark + a.title + "\n<size=80%>" + a.description + "</size>";
            tmp.fontSize = 20;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            if (a.unlocked) unlocked++;
        }

        // Update count text
        if (achievementCountText)
            achievementCountText.text = unlocked + " / " + achievements.Count + " Unlocked";
    }

    private void UpdateSettingsUI()
    {
        if (!SettingsManager.Instance) return;
        if (soundToggle) soundToggle.isOn = SettingsManager.Instance.SoundEnabled;
        if (musicToggle) musicToggle.isOn = SettingsManager.Instance.MusicEnabled;
        if (vibrationToggle) vibrationToggle.isOn = SettingsManager.Instance.VibrationEnabled;
    }

    public void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = score.ToString();
        if (AchievementManager.Instance) AchievementManager.Instance.OnScoreChanged(score);
    }

    public void UpdateUndoCount(int count)
    {
        if (undoCountText) undoCountText.text = count.ToString();
        if (undoButton) undoButton.interactable = count > 0;
    }

    // Called when lines are cleared - for achievements
    public void OnLinesCleared(int lines, bool isCombo)
    {
        if (AchievementManager.Instance) 
            AchievementManager.Instance.OnLineCleared(lines, isCombo);
    }

    public void UpdateBestScore()
    {
        if (bestScoreText && ScoreManager.Instance)
            bestScoreText.text = ScoreManager.Instance.GetAllTimeHighScore().ToString();
    }

    private void UpdateMenuHighScore()
    {
        if (menuHighScoreText && ScoreManager.Instance)
            menuHighScoreText.text = "Best: " + ScoreManager.Instance.GetAllTimeHighScore().ToString();
    }

    private void HideAllScreens()
    {
        if (mainMenuScreen) mainMenuScreen.SetActive(false);
        if (gameScreen) gameScreen.SetActive(false);
        if (gameOverScreen) gameOverScreen.SetActive(false);
        if (pauseScreen) pauseScreen.SetActive(false);
        if (settingsScreen) settingsScreen.SetActive(false);
        if (achievementsScreen) achievementsScreen.SetActive(false);
    }

    private void OnEnable()
    {
        if (GameManager.Instance) GameManager.Instance.OnScoreChanged.AddListener(UpdateScore);
        if (UndoManager.Instance) UndoManager.Instance.OnUndoCountChanged.AddListener(UpdateUndoCount);
    }

    private void OnDisable()
    {
        if (GameManager.Instance) GameManager.Instance.OnScoreChanged.RemoveListener(UpdateScore);
        if (UndoManager.Instance) UndoManager.Instance.OnUndoCountChanged.RemoveListener(UpdateUndoCount);
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }
}