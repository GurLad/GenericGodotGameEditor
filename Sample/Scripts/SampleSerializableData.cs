using Godot;
using System;

public partial class SampleSerializableData : Node, ISerializableData
{
    // Exports
    [Export]
    private Label numberLabel;
    // Properties
    public string Description { get; set; }
    public int Number { get => int.Parse(numberLabel.Text); set => numberLabel.Text = value.ToString(); }

    public string Save()
    {
        return Json.Stringify(new Godot.Collections.Array() { Description, Number });
    }

    public void Load(string data)
    {
        var newData = Json.ParseString(data).AsGodotArray();
        Description = newData[0].AsString();
        Number = newData[1].AsInt32();
    }

    public void Clear()
    {
        Description = "Empty desc";
        Number = -1;
    }
}
