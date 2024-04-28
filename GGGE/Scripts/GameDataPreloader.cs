using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameDataPreloader : Node
{
    public static GameDataPreloader Current;

    [Export]
    private PackedScene[] sceneDataLoaders;

    private List<AGameDataLoader> dataLoaders;
    private Dictionary<string, List<(string, string, GameDataRecord)>> preloadedData = new Dictionary<string, List<(string, string, GameDataRecord)>>();

    public override void _Ready()
    {
        base._Ready();
        Current = this;
        dataLoaders = sceneDataLoaders.ToList().ConvertAll(a => a.Instantiate<AGameDataLoader>());
        dataLoaders.ForEach(a => { a.Visible = false; AddChild(a); LoadAll(a); });
    }

    public GameDataRecord GetRecord(string dataFolder, string name, string folderAddition)
    {
        if (!preloadedData.ContainsKey(dataFolder))
        {
            return null;
        }
        int index = preloadedData[dataFolder].FindIndex(a => a.Item1 == name && a.Item2 == folderAddition);
        return index >= 0 ? preloadedData[dataFolder][index].Item3 : null;
    }

    public GameDataRecord GetRecord(string dataFolder, string name)
    {
        if (!preloadedData.ContainsKey(dataFolder))
        {
            return null;
        }
        int index = preloadedData[dataFolder].FindIndex(a => a.Item1 == name);
        return index >= 0 ? preloadedData[dataFolder][index].Item3 : null;
    }

    public void InvalidateFolder(string dataFolder)
    {
        if (!preloadedData.ContainsKey(dataFolder))
        {
            return;
        }
        preloadedData[dataFolder].Clear();
    }

    public void ReloadFolder(string dataFolder)
    {
        LoadAll(dataLoaders.Find(a => a.DataFolder == dataFolder));
    }

    public List<string> GetAllNames(string dataFolder)
    {
        return preloadedData[dataFolder].ConvertAll(a => a.Item1);
    }

    public Texture2D GetIcon(string dataFolder, string dataName)
    {
        return dataLoaders.Find(a => a.DataFolder == dataFolder)?.GetIcon(dataName);
    }

    private void LoadAll(AGameDataLoader template)
    {
        string folderPath = FileSystem.GameDataDirectory + FileSystem.SEPERATOR + template.DataFolder;
        void LoadInner(string folderAddition)
        {
            string folderFullPath = folderPath + (folderAddition != "" ? (FileSystem.SEPERATOR + folderAddition) : "");
            List<string> folders = FileSystem.GetFoldersAt(folderFullPath);
            List<string> files = FileSystem.GetFilesAt(folderFullPath);
            files.ForEach(a =>
            {
                template.Load(a, folderAddition);
                preloadedData[template.DataFolder].Add((a, folderAddition, template.SaveToRecord()));
            });
            folders.ForEach(a =>
            {
                LoadInner(folderAddition != "" ? (folderAddition + FileSystem.SEPERATOR + FileSystem.GetFolderName(a)) : FileSystem.GetFolderName(a));
            });
        }

        FileSystem.CreateDataFolder(template.DataFolder);
        if (preloadedData.ContainsKey(template.DataFolder))
        {
            preloadedData[template.DataFolder].Clear();
        }
        else
        {
            preloadedData[template.DataFolder] = new List<(string, string, GameDataRecord)>();
        }
        LoadInner("");
    }

    public record GameDataRecord
    (
        List<(string, object)> Records
    ) { }
}
