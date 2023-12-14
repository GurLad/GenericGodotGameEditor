using Godot;
using System;

public abstract partial class ASerializableDataEditor : Node
{
    [Signal]
    public delegate void OnDirtyEventHandler();

    protected void ConnectToLoader(AGameDataLoader gameDataLoader)
    {
        OnDirty += () => gameDataLoader.EmitSignal(AGameDataLoader.SignalName.OnDirty);
        gameDataLoader.OnExternalChange += Refresh;
    }

    protected void SetDirty() => EmitSignal(SignalName.OnDirty);

    protected abstract void Refresh();
}
