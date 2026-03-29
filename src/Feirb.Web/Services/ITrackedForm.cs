namespace Feirb.Web.Services;

public interface ITrackedForm
{
    bool HasUnsavedChanges { get; }

    Task<bool> SubmitAsync();

    void ResetDirtyState();
}
