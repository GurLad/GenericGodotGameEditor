using Godot;
using System;

public partial class Sample : Node
{
    // Exports
    [Export]
    private string name;
    [Export]
    private SampleLoader loader;
    // Properties
    private SampleSerializableData data => loader.Data;

    public override void _Ready()
    {
        base._Ready();
        loader.Load(name);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouse mouseEvent && mouseEvent.ButtonMask == MouseButtonMask.Left)
        {
            GD.Print(data.Number + ": " + data.Description);
        }
    }
}
