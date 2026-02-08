using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int currentScore;
    private int highScore;
    private int allTimeHighScore;

    public UnityEvent<int> OnScoreUpdated;
    public UnityEvent<int> OnHighScoreUpdated;
    public UnityEvent OnNewHighScore;

    private const string HIGH_SCORE_EASY = "HighScore_Easy";
    private const string HIGH_SCORE_MEDIUM = "HighScore_Medium";
    private const string HIGH_SCORE_HARD = "HighScore_Hard";
    private const string ALL_TIME_HIGH = "AllTimeHighScore";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (OnScoreUpdated == null) OnScoreUpdated = new UnityEvent<int>();
        if (OnHighScoreUpdated == null) OnHighScoreUpdated = new UnityEvent<int>();
        if (OnNewHighScore == null) OnNewHighScore = new UnityEvent();
    }

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreUpdated.Invoke(currentScore);
    }

    public void UpdateScore(int newScore)
    {
        currentScore = newScore;
        OnScoreUpdated.Invoke(currentScore);
    }

    public void LoadHighScore(Difficulty difficulty)
    {
        string key = GetHighScoreKey(difficulty);
        highScore = PlayerPrefs.GetInt(key, 0);
        allTimeHighScore = PlayerPrefs.GetInt(ALL_TIME_HIGH, 0);
        OnHighScoreUpdated.Invoke(highScore);
    }

    public bool CheckAndSaveHighScore(int finalScore, Difficulty difficulty)
    {
        bool isNewHigh = false;

        string key = GetHighScoreKey(difficulty);
        int currentHigh = PlayerPrefs.GetInt(key, 0);

        if (finalScore > currentHigh)
        {
            PlayerPrefs.SetInt(key, finalScore);
            highScore = finalScore;
            isNewHigh = true;
        }

        if (finalScore > allTimeHighScore)
        {
            PlayerPrefs.SetInt(ALL_TIME_HIGH, finalScore);
            allTimeHighScore = finalScore;
        }

        PlayerPrefs.Save();

        if (isNewHigh)
        {
            OnNewHighScore.Invoke();
        }

        return isNewHigh;
    }

    private string GetHighScoreKey(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return HIGH_SCORE_EASY;
            case Difficulty.Medium: return HIGH_SCORE_MEDIUM;
            case Difficulty.Hard: return HIGH_SCORE_HARD;
            default: return HIGH_SCORE_MEDIUM;
        }
    }

    public int GetHighScore(Difficulty difficulty)
    {
        string key = GetHighScoreKey(difficulty);
        return PlayerPrefs.GetInt(key, 0);
    }

    public int GetAllTimeHighScore()
    {
        return PlayerPrefs.GetInt(ALL_TIME_HIGH, 0);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
