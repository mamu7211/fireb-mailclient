namespace Feirb.Web.Services;

public sealed class UnsavedChangesService
{
    public bool HasUnsavedChanges { get; private set; }

    public Func<Task>? SaveAsync { get; private set; }

    public Func<Task>? DiscardAsync { get; private set; }

    public event Action? OnChange;

    public void SetUnsavedChanges(bool hasChanges, Func<Task>? saveAsync = null, Func<Task>? discardAsync = null)
    {
        HasUnsavedChanges = hasChanges;
        SaveAsync = saveAsync;
        DiscardAsync = discardAsync;
        OnChange?.Invoke();
    }

    public void Clear()
    {
        HasUnsavedChanges = false;
        SaveAsync = null;
        DiscardAsync = null;
        OnChange?.Invoke();
    }
}
