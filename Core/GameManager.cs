using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Playing, Paused, GameOver }

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private BlockSpawner blockSpawner;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private UndoManager undoManager;

    [Header("Settings")]
    [SerializeField] private int maxUndos = 3;

    [Header("Events")]
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent OnGameStart;
    public UnityEvent OnGameOver;

    public GameState CurrentState { get; private set; } = GameState.Menu;
    public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Easy;

    private int currentScore = 0;

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

        if (OnScoreChanged == null) OnScoreChanged = new UnityEvent<int>();
        if (OnGameStart == null) OnGameStart = new UnityEvent();
        if (OnGameOver == null) OnGameOver = new UnityEvent();
    }

    public void StartGame(Difficulty difficulty)
    {
        CurrentDifficulty = difficulty;
        CurrentState = GameState.Playing;

        currentScore = 0;
        OnScoreChanged.Invoke(currentScore);

        if (undoManager != null)
        {
            undoManager.ResetUndos(maxUndos);
        }

        if (gridManager != null)
        {
            gridManager.InitializeGrid();
        }

        if (blockSpawner != null)
        {
            blockSpawner.SetDifficulty(difficulty);
            blockSpawner.SpawnNewSet();
        }

        OnGameStart.Invoke();
    }

    public void OnBlockPlaced(int scoreToAdd, int linesCleared, bool isCombo)
    {
        currentScore += scoreToAdd;
        OnScoreChanged.Invoke(currentScore);

        if (scoreManager != null)
        {
            scoreManager.UpdateScore(currentScore);
        }

        if (blockSpawner != null && blockSpawner.AllPiecesPlaced())
        {
            blockSpawner.SpawnNewSet();
        }

        CheckGameOver();
    }

    public void SetScore(int score)
    {
        currentScore = score;
        OnScoreChanged.Invoke(currentScore);

        if (scoreManager != null)
        {
            scoreManager.UpdateScore(currentScore);
        }
    }

    private void CheckGameOver()
    {
        if (CurrentState != GameState.Playing) return;
        if (blockSpawner == null || gridManager == null) return;

        List<BlockPieceData> remainingPieces = blockSpawner.GetRemainingPieces();
        if (remainingPieces.Count == 0) return;

        bool canFit = gridManager.CanAnyPieceFit(remainingPieces);

        if (!canFit)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        CurrentState = GameState.GameOver;

        bool isNewHighScore = false;
        if (scoreManager != null)
        {
            int currentHigh = scoreManager.GetAllTimeHighScore();
            isNewHighScore = currentScore > currentHigh;
            if (isNewHighScore)
            {
                scoreManager.CheckAndSaveHighScore(currentScore, CurrentDifficulty);
            }
        }

        if (uiManager != null)
        {
            uiManager.ShowGameOver(currentScore, isNewHighScore);
        }

        OnGameOver.Invoke();
    }

    public void TryUndo()
    {
        if (CurrentState != GameState.Playing) return;

        if (undoManager != null && undoManager.CanUndo())
        {
            undoManager.TryUndo();
        }
    }

    public void RestartGame()
    {
        StartGame(CurrentDifficulty);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    public void ReturnToMenu()
    {
        CurrentState = GameState.Menu;
        Time.timeScale = 1f;

        if (gridManager != null)
        {
            gridManager.ClearGrid();
        }

        if (blockSpawner != null)
        {
            blockSpawner.ClearAllPieces();
        }
    }

    public int GetCurrentScore() { return currentScore; }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}