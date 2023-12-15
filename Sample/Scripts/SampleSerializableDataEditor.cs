using Godot;
using System;

public partial class SampleSerializableDataEditor : ASerializableDataEditor<SampleSerializableData>
{
    // Exports
    [Export]
    private TextEdit descriptionEditor;
    [Export]
    private SpinBox numberEditor;

    public override void _Ready()
    {
        base._Ready();
        descriptionEditor.TextChanged += () => { data.Description = descriptionEditor.Text; SetDirty(); };
        numberEditor.ValueChanged += (i) => { data.Number = (int)i; SetDirty(); };
    }

    protected override void Refresh()
    {
        descriptionEditor.Text = data.Description;
        numberEditor.Value = data.Number;
    }
}
