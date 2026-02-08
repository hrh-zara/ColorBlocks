using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance { get; private set; }

    [SerializeField] private int maxUndos = 3;
    private int remainingUndos;

    public UnityEvent<int> OnUndoCountChanged;

    // Stack of saved states for undo
    private Stack<GridState> savedStates = new Stack<GridState>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (OnUndoCountChanged == null) OnUndoCountChanged = new UnityEvent<int>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetUndos(int maxCount)
    {
        maxUndos = maxCount;
        remainingUndos = maxCount;
        savedStates.Clear();
        OnUndoCountChanged?.Invoke(remainingUndos);
    }

    // Call this BEFORE placing a piece to save the current state
    public void SaveStateForUndo(GridState state)
    {
        if (state != null)
        {
            savedStates.Push(state);
            Debug.Log("State saved for undo. Stack size: " + savedStates.Count);
        }
    }

    public bool TryUndo()
    {
        if (remainingUndos <= 0)
        {
            Debug.Log("No undos remaining!");
            return false;
        }

        if (savedStates.Count == 0)
        {
            Debug.Log("No states to undo!");
            return false;
        }

        // Get the previous state
        GridState previousState = savedStates.Pop();

        // Restore the grid
        if (GridManager.Instance != null)
        {
            GridManager.Instance.RestoreState(previousState);
        }

        // Restore the score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetScore(previousState.score);
        }

        remainingUndos--;
        OnUndoCountChanged?.Invoke(remainingUndos);

        Debug.Log("Undo performed! Remaining: " + remainingUndos);
        return true;
    }

    public int GetRemainingUndos()
    {
        return remainingUndos;
    }

    public bool HasUndos()
    {
        return remainingUndos > 0 && savedStates.Count > 0;
    }

    public bool CanUndo()
    {
        return remainingUndos > 0 && savedStates.Count > 0;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}