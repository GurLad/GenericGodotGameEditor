using Godot;
using System;

public abstract partial class AGameDataEditor : Node
{
    [Signal]
    public delegate void OnDirtyEventHandler();

    protected void ConnectToLoader(AGameDataLoader gameDataLoader)
    {
        OnDirty += () => gameDataLoader.EmitSignal(AGameDataLoader.SignalName.OnDirty);
    }

    protected void SetDirty() => EmitSignal(SignalName.OnDirty);
}
