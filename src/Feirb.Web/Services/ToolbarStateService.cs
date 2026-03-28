using Feirb.Web.Components.UI;

namespace Feirb.Web.Services;

public sealed class ToolbarAction(string label, ButtonVariant variant, Func<Task> onClickAsync, string? icon = null)
{
    public string Label { get; } = label;
    public ButtonVariant Variant { get; } = variant;
    public Func<Task> OnClickAsync { get; } = onClickAsync;
    public string? Icon { get; } = icon;
}

public sealed class ToolbarStateService
{
    private readonly List<ToolbarAction> _actions = [];
    private int _version;

    public IReadOnlyList<ToolbarAction> Actions => _actions;

    public event Action? OnChange;

    public int SetActions(IEnumerable<ToolbarAction> actions)
    {
        _actions.Clear();
        _actions.AddRange(actions);
        _version++;
        OnChange?.Invoke();
        return _version;
    }

    public void ClearIfCurrent(int version)
    {
        if (_version == version)
        {
            _actions.Clear();
            OnChange?.Invoke();
        }
    }

    public void Clear()
    {
        _actions.Clear();
        _version++;
        OnChange?.Invoke();
    }
}
