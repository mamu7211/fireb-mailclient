using Feirb.Web.Services;
using FluentAssertions;

namespace Feirb.Web.Tests.Services;

public class UnsavedChangesServiceTests
{
    private readonly UnsavedChangesService _sut = new();

    [Fact]
    public void HasUnsavedChanges_NoForms_ReturnsFalse() =>
        _sut.HasUnsavedChanges.Should().BeFalse();

    [Fact]
    public void HasUnsavedChanges_CleanForm_ReturnsFalse()
    {
        var form = new FakeTrackedForm();

        _sut.Register(form);

        _sut.HasUnsavedChanges.Should().BeFalse();
    }

    [Fact]
    public void HasUnsavedChanges_DirtyForm_ReturnsTrue()
    {
        var form = new FakeTrackedForm { HasUnsavedChanges = true };

        _sut.Register(form);

        _sut.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public void HasUnsavedChanges_MixedForms_ReturnsTrue()
    {
        var clean = new FakeTrackedForm();
        var dirty = new FakeTrackedForm { HasUnsavedChanges = true };

        _sut.Register(clean);
        _sut.Register(dirty);

        _sut.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public void Unregister_RemovesForm()
    {
        var form = new FakeTrackedForm { HasUnsavedChanges = true };
        _sut.Register(form);

        _sut.Unregister(form);

        _sut.HasUnsavedChanges.Should().BeFalse();
    }

    [Fact]
    public void Unregister_RaisesOnChange()
    {
        var form = new FakeTrackedForm();
        _sut.Register(form);
        var changed = false;
        _sut.OnChange += () => changed = true;

        _sut.Unregister(form);

        changed.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAllAsync_NoForms_ReturnsTrueAsync()
    {
        var result = await _sut.SaveAllAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAllAsync_CallsSubmitOnDirtyFormsOnlyAsync()
    {
        var clean = new FakeTrackedForm();
        var dirty = new FakeTrackedForm { HasUnsavedChanges = true };
        _sut.Register(clean);
        _sut.Register(dirty);

        await _sut.SaveAllAsync();

        clean.SubmitCallCount.Should().Be(0);
        dirty.SubmitCallCount.Should().Be(1);
    }

    [Fact]
    public async Task SaveAllAsync_AllSucceed_ReturnsTrueAsync()
    {
        var form1 = new FakeTrackedForm { HasUnsavedChanges = true };
        var form2 = new FakeTrackedForm { HasUnsavedChanges = true };
        _sut.Register(form1);
        _sut.Register(form2);

        var result = await _sut.SaveAllAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAllAsync_FirstFails_StopsAndReturnsFalseAsync()
    {
        var failing = new FakeTrackedForm { HasUnsavedChanges = true, SubmitResult = false };
        var second = new FakeTrackedForm { HasUnsavedChanges = true };
        _sut.Register(failing);
        _sut.Register(second);

        var result = await _sut.SaveAllAsync();

        result.Should().BeFalse();
        second.SubmitCallCount.Should().Be(0);
    }

    [Fact]
    public void DiscardAll_ResetsAllForms()
    {
        var form1 = new FakeTrackedForm { HasUnsavedChanges = true };
        var form2 = new FakeTrackedForm { HasUnsavedChanges = true };
        _sut.Register(form1);
        _sut.Register(form2);

        _sut.DiscardAll();

        form1.ResetCallCount.Should().Be(1);
        form2.ResetCallCount.Should().Be(1);
    }

    [Fact]
    public void DiscardAll_RaisesOnChange()
    {
        var changed = false;
        _sut.OnChange += () => changed = true;

        _sut.DiscardAll();

        changed.Should().BeTrue();
    }

    [Fact]
    public void NotifyChanged_RaisesOnChange()
    {
        var changed = false;
        _sut.OnChange += () => changed = true;

        _sut.NotifyChanged();

        changed.Should().BeTrue();
    }

    private sealed class FakeTrackedForm : ITrackedForm
    {
        public bool HasUnsavedChanges { get; set; }
        public bool SubmitResult { get; set; } = true;
        public int SubmitCallCount { get; private set; }
        public int ResetCallCount { get; private set; }

        public Task<bool> SubmitAsync()
        {
            SubmitCallCount++;
            return Task.FromResult(SubmitResult);
        }

        public void ResetDirtyState()
        {
            ResetCallCount++;
            HasUnsavedChanges = false;
        }
    }
}
