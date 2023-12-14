using Godot;
using System;
using System.Text.Json;

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
        return JsonSerializer.Serialize(this, typeof(SampleSerializableData), new JsonSerializerOptions { WriteIndented = true });
    }

    public void Load(string data)
    {
        SampleSerializableData newData = (SampleSerializableData)JsonSerializer.Deserialize(data, typeof(SampleSerializableData));
        Description = newData.Description;
        Number = newData.Number;
    }

    public void Clear()
    {
        Description = "Empty desc";
        Number = -1;
    }
}
