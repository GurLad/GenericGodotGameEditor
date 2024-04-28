using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GGE.Internal;

public static class FileSystem
{
    /// <summary>
    /// How should the FileSystem detect whether a directory is a file or a folder?
    /// 1. Marker - each file/folder will be saved alongside an empty *.file/*.folder file, similar to Godot *.import files.
    ///    Then, loop over these files instead of the user directories. Works better with Git (since it can't detect folder addition/deletion).
    /// 2. Suffix - files will always end with ".f", while folders (directories) will end with ".d".
    /// 3. Auto - If a directory includes any sub-directories, it will be considered a folder. Otherwise, a file.
    /// </summary>
    private enum DirectorySavingMode { Marker, Suffix, Auto }
    // Consts
    public const char SEPERATOR = '\\';
    private const string DEFAULT_DIRECTORY = "./GameData";
    private const string GAME_DATA_DIRECTORY_FILE = "user://GameDataDirectory.data";
    private const DirectorySavingMode DIRECTORY_SAVING_MODE = DirectorySavingMode.Marker;
    // Properties
    private static string _gameDataDirectory = null;
    public static string GameDataDirectory
    {
        set
        {
            _gameDataDirectory = value;
            using FileAccess dataFile = FileAccess.Open(GAME_DATA_DIRECTORY_FILE, FileAccess.ModeFlags.Write);
            dataFile.StoreString(value);
        }
        get
        {
            if (_gameDataDirectory == null)
            {
                if (!FileAccess.FileExists(GAME_DATA_DIRECTORY_FILE))
                {
                    // Create the file
                    using FileAccess temp = FileAccess.Open(GAME_DATA_DIRECTORY_FILE, FileAccess.ModeFlags.Write);
                    temp.StoreString(DEFAULT_DIRECTORY);
                    // Create the directory
                    DirAccess.MakeDirRecursiveAbsolute(DEFAULT_DIRECTORY);
                    // Add a .gdignore file, just in case
                    using FileAccess temp2 = FileAccess.Open(DEFAULT_DIRECTORY + SEPERATOR + ".gdignore", FileAccess.ModeFlags.Write);
                }
                using FileAccess dataFile = FileAccess.Open(GAME_DATA_DIRECTORY_FILE, FileAccess.ModeFlags.Read);
                _gameDataDirectory = dataFile.GetAsText();
            }
            return _gameDataDirectory;
        }
    }

    public static void CreateDataFolder(string dataFolder)
    {
        DirAccess.MakeDirRecursiveAbsolute(GameDataDirectory + SEPERATOR + dataFolder);
    }

    public static string LoadTextFile(string path, string extension = ".json")
    {
        using var file = FileAccess.Open(path + extension, FileAccess.ModeFlags.Read);
        return file?.GetAsText();
    }

    private static Image LoadImageFile(string path, string extension = ".png")
    {
        if (FileAccess.FileExists(path + extension))
        {
            return Image.LoadFromFile(path + extension);
        }
        else
        {
            return null;
        }
    }

    public static Texture2D LoadTextureFile(string path, string extension = ".png")
    {
        Image img = LoadImageFile(path, extension);
        return img != null ? ImageTexture.CreateFromImage(img) : null;
    }

    public static List<Texture2D> LoadAnimatedTextureFile(string path, int numFrames, string extension = ".png")
    {
        return LoadImageFile(path, extension)?.Split(numFrames) ?? new List<Texture2D>();
    }

    public static string GetFolderPath(this AGameDataLoader gameDataLoader, string name, string folder, bool save)
    {
        string folderPath = GameDataDirectory + SEPERATOR + gameDataLoader.DataFolder + SEPERATOR +
            (folder.Length > 0 ? (folder + SEPERATOR) : "") + name.FixFileName();
        if (DIRECTORY_SAVING_MODE == DirectorySavingMode.Suffix)
        {
#pragma warning disable CS0162 // Unreachable code detected
            folderPath += ".f";
#pragma warning restore CS0162 // Unreachable code detected
        }
        if (!DirAccess.DirExistsAbsolute(folderPath))
        {
            if (save)
            {
                DirAccess.MakeDirRecursiveAbsolute(folderPath);
                if (DIRECTORY_SAVING_MODE == DirectorySavingMode.Marker)
                {
#pragma warning disable CS0162 // Unreachable code detected
                    using FileAccess temp = FileAccess.Open(folderPath + ".file", FileAccess.ModeFlags.Write);
#pragma warning restore CS0162 // Unreachable code detected
                }
            }
            else
            {
                throw new Exception("Invalid file! (" + name + " at " + folderPath + ")");
            }
        }
        return folderPath;
    }

    public static List<string> GetFoldersAt(string path)
    {
#pragma warning disable CS0162 // Unreachable code detected
        switch (DIRECTORY_SAVING_MODE)
        {
            case DirectorySavingMode.Marker:
                return DirAccess.GetFilesAt(path).ToList().FindAll(a => a.EndsWith(".folder")).ConvertAll(a => a.Substr(0, a.Length - ".folder".Length));
            case DirectorySavingMode.Suffix:
                return DirAccess.GetDirectoriesAt(path).ToList().FindAll(a => a.EndsWith(".d")).ConvertAll(a => a.Substr(0, a.Length - ".d".Length));
            case DirectorySavingMode.Auto:
                return DirAccess.GetDirectoriesAt(path).ToList().FindAll(a =>
                    DirAccess.GetDirectoriesAt(path + SEPERATOR + a).Length > 0);
            default:
                throw new Exception("Impossible");
        }
#pragma warning restore CS0162 // Unreachable code detected
    }

    public static List<string> GetFilesAt(string path)
    {
#pragma warning disable CS0162 // Unreachable code detected
        switch (DIRECTORY_SAVING_MODE)
        {
            case DirectorySavingMode.Marker:
                return DirAccess.GetFilesAt(path).ToList().FindAll(a => a.EndsWith(".file")).ConvertAll(a => a.Substr(0, a.Length - ".file".Length));
            case DirectorySavingMode.Suffix:
                return DirAccess.GetDirectoriesAt(path).ToList().FindAll(a => a.EndsWith(".f")).ConvertAll(a => a.Substr(0, a.Length - ".f".Length));
            case DirectorySavingMode.Auto:
                return DirAccess.GetDirectoriesAt(path).ToList().FindAll(a =>
                    DirAccess.GetDirectoriesAt(path + SEPERATOR + a).Length <= 0);
            default:
                throw new Exception("Impossible");
        }
#pragma warning restore CS0162 // Unreachable code detected
    }

    public static string GetFolderName(string item)
    {
        return DIRECTORY_SAVING_MODE == DirectorySavingMode.Suffix ? (item + ".d") : item;
    }

    public static string GetFileName(string item)
    {
        return DIRECTORY_SAVING_MODE == DirectorySavingMode.Suffix ? (item + ".f") : item;
    }

    public static void CreateFolder(string folderFullPath, string name)
    {
        name = GetFolderName(name);
        string path = folderFullPath + SEPERATOR + name;
        DirAccess.MakeDirRecursiveAbsolute(path);
        if (DIRECTORY_SAVING_MODE == DirectorySavingMode.Marker)
        { 
#pragma warning disable CS0162 // Unreachable code detected
            using FileAccess temp = FileAccess.Open(path + ".folder", FileAccess.ModeFlags.Write);
#pragma warning restore CS0162 // Unreachable code detected
        }
    }

    public static void DeleteFolder(string folderFullPath, string name)
    {
        name = GetFolderName(name);
        GD.Print(folderFullPath + SEPERATOR + name);
        DeleteRecursive(folderFullPath + SEPERATOR + name);
        if (DIRECTORY_SAVING_MODE == DirectorySavingMode.Marker)
        {
#pragma warning disable CS0162 // Unreachable code detected
            DirAccess.RemoveAbsolute(folderFullPath + SEPERATOR + name + ".folder");
#pragma warning restore CS0162 // Unreachable code detected
        }
    }

    public static void DeleteFile(string folderFullPath, string name)
    {
        name = GetFileName(name);
        GD.Print(folderFullPath + SEPERATOR + name);
        DeleteRecursive(folderFullPath + SEPERATOR + name);
        if (DIRECTORY_SAVING_MODE == DirectorySavingMode.Marker)
        {
#pragma warning disable CS0162 // Unreachable code detected
            DirAccess.RemoveAbsolute(folderFullPath + SEPERATOR + name + ".file");
#pragma warning restore CS0162 // Unreachable code detected
        }
    }

    private static void DeleteRecursive(string path)
    {
        DirAccess.GetFilesAt(path).ToList().ForEach(a => DirAccess.RemoveAbsolute(path + "\\" + a));
        DirAccess.GetDirectoriesAt(path).ToList().ForEach(a => { DeleteRecursive(path + "\\" + a); DirAccess.RemoveAbsolute(a); });
        DirAccess.RemoveAbsolute(path);
    }
}
