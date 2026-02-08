using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public bool unlocked;
    public int targetValue;
    public int currentValue;

    public Achievement(string i, string t, string d, int target)
    {
        id = i;
        title = t;
        description = d;
        targetValue = target;
        currentValue = 0;
        unlocked = false;
    }
}

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    public UnityEvent<Achievement> OnAchievementUnlocked;

    private Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();
    private int totalLinesCleared = 0;
    private int totalCombos = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (OnAchievementUnlocked == null)
                OnAchievementUnlocked = new UnityEvent<Achievement>();
            InitializeAchievements();
            LoadAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAchievements()
    {
        achievements.Clear();
        achievements["first_game"] = new Achievement("first_game", "First Steps", "Play your first game", 1);
        achievements["first_line"] = new Achievement("first_line", "Line Clear", "Clear your first line", 1);
        achievements["score_100"] = new Achievement("score_100", "Century", "Score 100 points", 100);
        achievements["score_500"] = new Achievement("score_500", "High Scorer", "Score 500 points", 500);
        achievements["score_1000"] = new Achievement("score_1000", "Block Master", "Score 1000 points", 1000);
        achievements["clear_5"] = new Achievement("clear_5", "Line Clearer", "Clear 5 lines total", 5);
        achievements["clear_10"] = new Achievement("clear_10", "Line Expert", "Clear 10 lines total", 10);
        achievements["combo_3"] = new Achievement("combo_3", "Combo King", "Get 3 combos", 3);
        achievements["easy_win"] = new Achievement("easy_win", "Easy Victory", "Score 50+ on Easy", 1);
        achievements["medium_win"] = new Achievement("medium_win", "Medium Victory", "Score 50+ on Medium", 1);
        achievements["hard_win"] = new Achievement("hard_win", "Hard Victory", "Score 50+ on Hard", 1);
    }

    public void OnGameStarted()
    {
        UpdateProgress("first_game", 1);
    }

    public void OnScoreChanged(int score)
    {
        UpdateProgress("score_100", score);
        UpdateProgress("score_500", score);
        UpdateProgress("score_1000", score);
    }

    public void OnLineCleared(int lines, bool isCombo)
    {
        totalLinesCleared += lines;
        UpdateProgress("first_line", totalLinesCleared);
        UpdateProgress("clear_5", totalLinesCleared);
        UpdateProgress("clear_10", totalLinesCleared);

        if (isCombo)
        {
            totalCombos++;
            UpdateProgress("combo_3", totalCombos);
        }
    }

    public void OnGameOver(Difficulty difficulty, int score)
    {
        if (score >= 50)
        {
            if (difficulty == Difficulty.Easy) UpdateProgress("easy_win", 1);
            else if (difficulty == Difficulty.Medium) UpdateProgress("medium_win", 1);
            else if (difficulty == Difficulty.Hard) UpdateProgress("hard_win", 1);
        }
    }

    private void UpdateProgress(string id, int value)
    {
        if (!achievements.ContainsKey(id)) return;
        Achievement a = achievements[id];
        if (a.unlocked) return;

        a.currentValue = value;
        if (a.currentValue >= a.targetValue)
        {
            a.unlocked = true;
            a.currentValue = a.targetValue;
            Debug.Log("Achievement Unlocked: " + a.title);
            OnAchievementUnlocked?.Invoke(a);
        }
        SaveAchievements();
    }

    public List<Achievement> GetAllAchievements()
    {
        return new List<Achievement>(achievements.Values);
    }

    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var a in achievements.Values)
            if (a.unlocked) count++;
        return count;
    }

    private void SaveAchievements()
    {
        foreach (var kvp in achievements)
        {
            PlayerPrefs.SetInt("Ach_" + kvp.Key + "_u", kvp.Value.unlocked ? 1 : 0);
            PlayerPrefs.SetInt("Ach_" + kvp.Key + "_v", kvp.Value.currentValue);
        }
        PlayerPrefs.SetInt("TotalLines", totalLinesCleared);
        PlayerPrefs.SetInt("TotalCombos", totalCombos);
        PlayerPrefs.Save();
    }

    private void LoadAchievements()
    {
        foreach (var kvp in achievements)
        {
            kvp.Value.unlocked = PlayerPrefs.GetInt("Ach_" + kvp.Key + "_u", 0) == 1;
            kvp.Value.currentValue = PlayerPrefs.GetInt("Ach_" + kvp.Key + "_v", 0);
        }
        totalLinesCleared = PlayerPrefs.GetInt("TotalLines", 0);
        totalCombos = PlayerPrefs.GetInt("TotalCombos", 0);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
