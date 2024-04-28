using Godot;
using System;

public class GameDataSerializablePart : GGE.Internal.AGameDataPart<ISerializableData, string>
{
    public GameDataSerializablePart(string name, ISerializableData sourceNode, string fileExtension = ".json") :
        base(name, sourceNode, fileExtension) { }

    public override void Clear()
    {
        SourceNode.Clear();
    }

    public override void Load(string folderPath)
    {
        string file = FileSystem.LoadTextFile(GetFullPath(folderPath, false), fileExtension);
        if (file != null)
        {
            SourceNode.Load(file);
        }
        else
        {
            SourceNode.Clear();
        }
    }

    protected override void LoadFromRecordInternal(string record)
    {
        SourceNode.Load(record);
    }

    public override void Save(string folderPath)
    {
        using var file = FileAccess.Open(GetFullPath(folderPath), FileAccess.ModeFlags.Write);
        if (file == null)
        {
            throw new Exception("Error creating file " + GetFullPath(folderPath) + "!");
        }
        file.StoreString(SourceNode.Save());
    }

    protected override string SaveToRecordInternal()
    {
        return SourceNode.Save();
    }
}
