using Godot;
using System;

public abstract partial class ASerializableDataEditor<T> : Node where T : ISerializableData
{
    // Exports
    [Export]
    private string dataKey;
    [Export]
    private AGameDataLoader loader;
    // Properties
    private T _data;
    protected T data => _data;

    [Signal]
    public delegate void OnDirtyEventHandler();

    public override void _Ready()
    {
        base._Ready();
        _data = loader.GetData<T>(dataKey);
        OnDirty += () => loader.EmitSignal(AGameDataLoader.SignalName.OnDirty);
        loader.OnExternalChange += Refresh;
        Refresh();
    }

    protected void SetDirty() => EmitSignal(SignalName.OnDirty);

    protected abstract void Refresh();
}
