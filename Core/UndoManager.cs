using UnityEngine;
using UnityEngine.Events;

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance { get; private set; }

    [SerializeField] private int maxUndos = 3;
    private int remainingUndos;

    public UnityEvent<int> OnUndoCountChanged;

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
        OnUndoCountChanged?.Invoke(remainingUndos);
    }

    public bool TryUndo()
    {
        if (remainingUndos <= 0)
        {
            return false;
        }

        remainingUndos--;
        OnUndoCountChanged?.Invoke(remainingUndos);
        return true;
    }

    public int GetRemainingUndos()
    {
        return remainingUndos;
    }

    public bool HasUndos()
    {
        return remainingUndos > 0;
    }

    public bool CanUndo()
    {
        return remainingUndos > 0;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}