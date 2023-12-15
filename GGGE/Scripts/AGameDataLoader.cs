using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract partial class AGameDataLoader : Node
{
    public abstract string DataFolder { get; }
    protected abstract Dictionary<string, ISerializableData> gameDatas { get; }
    protected abstract Dictionary<string, Sprite2D> sprites { get; }
    protected virtual string iconKey => "";
    public bool Visible
    {
        set
        {
            GetChildren().ToList().ForEach(a =>
            {
                if (a.HasMethod("set_visible"))
                {
                    a.Call("set_visible", value);
                }
            });
        }
    }

    [Signal]
    public delegate void OnDirtyEventHandler();

    [Signal]
    public delegate void OnExternalChangeEventHandler();

    public void Load(string name, string folder)
    {
        var preloadedData = GameDataPreloader.Current?.GetRecord(DataFolder, name, folder);
        if (preloadedData != null)
        {
            LoadFromRecord(preloadedData);
            return;
        }
        string folderPath = this.GetFolderPath(name, folder, false);
        foreach (var gameData in gameDatas)
        {
            using var file = FileAccess.Open(folderPath + FileSystem.SEPERATOR + gameData.Key + ".json", FileAccess.ModeFlags.Read);
            if (file != null)
            {
                gameData.Value.Load(file.GetAsText());
            }
            else
            {
                gameData.Value.Clear();
            }
        }
        foreach (var sprite in sprites)
        {
            if (FileAccess.FileExists(folderPath + FileSystem.SEPERATOR + sprite.Key + ".png"))
            {
                sprite.Value.Texture = ImageTexture.CreateFromImage(Image.LoadFromFile(folderPath + FileSystem.SEPERATOR + sprite.Key + ".png"));
            }
            else
            {
                sprite.Value.Texture = new Texture2D();
            }
        }
        EmitSignal(SignalName.OnExternalChange);
    }

    public void Load(string name)
    {
        var preloadedData = GameDataPreloader.Current?.GetRecord(DataFolder, name);
        if (preloadedData != null)
        {
            LoadFromRecord(preloadedData);
        }
        else
        {
            throw new Exception(name + " not found!");
        }
    }

    private void LoadFromRecord(GameDataPreloader.GameDataRecord record)
    {
        foreach (var gameData in gameDatas)
        {
            gameData.Value.Load(record.GameDatas[gameData.Key]);
        }
        foreach (var sprite in sprites)
        {
            sprite.Value.Texture = ImageTexture.CreateFromImage(record.Sprites[sprite.Key]);
        }
    }

    public void Save(string name, string folder)
    {
        string folderPath = this.GetFolderPath(name, folder, true);
        foreach (var gameData in gameDatas)
        {
            using var file = FileAccess.Open(folderPath + FileSystem.SEPERATOR + gameData.Key + ".json", FileAccess.ModeFlags.Write);
            if (file == null)
            {
                throw new Exception("Error creating file " + (folderPath + FileSystem.SEPERATOR + gameData.Key + ".json") + "!");
            }
            file.StoreString(gameData.Value.Save());
        }
        foreach (var sprite in sprites)
        {
            sprite.Value.Texture?.GetImage()?.SavePng(folderPath + FileSystem.SEPERATOR + sprite.Key + ".png");
        }
    }

    public GameDataPreloader.GameDataRecord SaveToRecord()
    {
        GameDataPreloader.GameDataRecord record = new GameDataPreloader.GameDataRecord(new Dictionary<string, string>(), new Dictionary<string, Image>());
        foreach (var gameData in gameDatas)
        {
            record.GameDatas.Add(gameData.Key, gameData.Value.Save());
        }
        foreach (var sprite in sprites)
        {
            record.Sprites.Add(sprite.Key, sprite.Value.Texture?.GetImage());
        }
        return record;
    }

    public void New()
    {
        gameDatas.Values.ToList().ForEach(a => a.Clear());
        sprites.Values.ToList().ForEach(a => a.Texture = new Texture2D());
        EmitSignal(SignalName.OnExternalChange);
    }

    public Texture2D GetIcon(string name, string folder = "")
    {
        if (iconKey == "") return null;
        string folderPath = this.GetFolderPath(name, folder, true);
        if (FileAccess.FileExists(folderPath + FileSystem.SEPERATOR + iconKey + ".png"))
        {
            return ImageTexture.CreateFromImage(Image.LoadFromFile(folderPath + FileSystem.SEPERATOR + iconKey + ".png"));
        }
        else
        {
            return new Texture2D();
        }
    }

    public T GetData<T>(string key) where T : ISerializableData
    {
        return gameDatas.ContainsKey(key) ? (gameDatas[key] is T result ? result :
            throw new Exception("Type mismatch! " + key + " isn't " + typeof(T))) :
            throw new Exception("No key! " + key);
    }
}
