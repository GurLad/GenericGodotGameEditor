using Godot;
using System;

public partial class SampleSerializableDataEditor : ASerializableDataEditor
{
    // Exports
    [Export]
    private TextEdit descriptionEditor;
    [Export]
    private SpinBox numberEditor;
    [Export]
    private SampleLoader loader { get; set; }
    // Properties
    private SampleSerializableData data => loader.Data;

    public override void _Ready()
    {
        base._Ready();
        descriptionEditor.TextChanged += () => data.Description = descriptionEditor.Text;
        numberEditor.ValueChanged += (i) => data.Number = (int)i;
        ConnectToLoader(loader);
    }

    protected override void Refresh()
    {
        descriptionEditor.Text = data.Description;
        numberEditor.Value = data.Number;
    }
}
