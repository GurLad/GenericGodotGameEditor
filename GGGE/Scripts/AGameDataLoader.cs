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

    [Signal]
    public delegate void OnDirtyEventHandler();

    [Signal]
    public delegate void OnExternalChangeEventHandler();

    public void Load(string name, string folder = "")
    {
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

    public void Save(string name, string folder = "")
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
}
