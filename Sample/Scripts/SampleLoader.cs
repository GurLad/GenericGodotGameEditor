using Godot;
using System;
using System.Collections.Generic;

public partial class SampleLoader : AGameDataLoader
{
    [Export]
    public SampleSerializableData Data { get; set; }
    [Export]
    public Sprite2D Sprite { get; set; }

    public override string DataFolder => "Sample";

    protected override Dictionary<string, ISerializableData> gameDatas => new Dictionary<string, ISerializableData>() { { "SampleJson", Data } };

    protected override Dictionary<string, Sprite2D> sprites => new Dictionary<string, Sprite2D>() { { "SampleSprite", Sprite } };

    protected override string iconKey => "SampleSprite";
}
