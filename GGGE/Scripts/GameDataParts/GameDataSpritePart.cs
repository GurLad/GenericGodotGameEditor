using Godot;
using System;

public class GameDataSpritePart : GGE.Internal.AGameDataPart<Sprite2D, Image>
{
    public GameDataSpritePart(string name, Sprite2D sourceNode, string fileExtension = ".png") : base(name, sourceNode, fileExtension) { }

    public override void Clear()
    {
        SourceNode.Texture = null;
    }

    public override void Load(string folderPath)
    {
        SourceNode.Texture = FileSystem.LoadTextureFile(GetFullPath(folderPath, false), fileExtension);
    }

    protected override void LoadFromRecordInternal(Image record)
    {
        SourceNode.Texture = ImageTexture.CreateFromImage(record);
    }

    public override void Save(string folderPath)
    {
        SourceNode.Texture?.GetImage()?.SavePng(GetFullPath(folderPath));
    }

    protected override Image SaveToRecordInternal()
    {
        return SourceNode.Texture?.GetImage();
    }
}
