using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameDataBrowser : VBoxContainer
{
    [Export]
    private ItemList dataList;
    [Export]
    private BaseButton saveButton;
    [Export]
    private BaseButton saveAsButton;
    [Export]
    private BaseButton newButton;
    [Export]
    private BaseButton deleteButton;
    [Export]
    private BaseButton newFolderButton;
    [Export]
    private BaseButton backButton;
    [Export]
    private BaseButton exitButton;
    [Export]
    private Label title;
    [Export]
    private Container loaderContainer;
    [Export]
    private Texture2D folderIcon;

    private Control gameEditorPanel;
    private AGameDataLoader dataLoader;
    private int folderCount = 0;
    private string folderPath => FileSystem.GameDataDirectory + FileSystem.SEPERATOR + dataLoader.DataFolder;
    private string folderFullPath => folderPath + (folderAddition != "" ? (FileSystem.SEPERATOR + folderAddition) : "");
    private string _folderAddition = "";
    private string folderAddition
    {
        get => _folderAddition;
        set
        {
            _folderAddition = value;
            backButton.Disabled = string.IsNullOrEmpty(folderAddition);
        }
    }
    private string _selected;
    private string selected
    {
        get => _selected;
        set
        {
            _selected = value;
            title.Text = !string.IsNullOrEmpty(selected) ? selected : "(Untitled)";
            title.Text += dirty ? " *" : "";
        }
    }
    private bool _dirty;
    private bool dirty
    {
        get => _dirty;
        set
        {
            _dirty = value;
            selected = selected;
        }
    }

    public void Init(Control gameEditorPanel, PackedScene sceneDataEditor)
    {
        // Replace the editor with self
        this.gameEditorPanel = gameEditorPanel;
        gameEditorPanel.GetParent().AddChild(this);
        gameEditorPanel.Visible = false;
        // Create a new editor
        GameDataEditor dataEditor = sceneDataEditor.Instantiate<GameDataEditor>();
        loaderContainer.AddChild(dataEditor);
        dataLoader = dataEditor.DataLoader;
        dataLoader.OnDirty += () => dirty = true;
        // Invalidate previous data
        GameDataPreloader.Current?.InvalidateFolder(dataLoader.DataFolder);
        // Init data
        UpdateDataList();
        selected = null;
        folderAddition = "";
    }

    public override void _Ready()
    {
        base._Ready();
        saveButton.Pressed += Save;
        saveAsButton.Pressed += SaveAs;
        newButton.Pressed += New;
        deleteButton.Pressed += Delete;
        newFolderButton.Pressed += NewFolder;
        backButton.Pressed += Back;
        exitButton.Pressed += Exit;
        dataList.ItemActivated += (i) => Navigate((int)i);
        dataList.ItemSelected += (i) => deleteButton.Disabled = false;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.CtrlPressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.S:
                    if (keyEvent.ShiftPressed)
                    {
                        SaveAs();
                    }
                    else
                    {
                        Save();
                    }
                    break;
                case Key.N:
                    New();
                    break;
                default:
                    break;
            }
        }
    }

    private void Save()
    {
        if (selected != null)
        {
            dataLoader.Save(selected, folderAddition);
            UpdateDataList();
            dataList.Select(dataList.FindIndex(selected));
            deleteButton.Disabled = false;
            dirty = false;
        }
        else
        {
            SaveAs();
        }
    }

    private void SaveAs()
    {
        InputBox.Show(this, "Enter new " + dataLoader.DataFolder.ToLower() + " name:", (name) =>
        {
            selected = name;
            Save();
        });
    }

    private void Navigate(int index)
    {
        if (dirty)
        {
            AskDiscardChanges(() => Navigate(index));
            return;
        }
        string name = dataList.GetItemText(index);
        if (index < folderCount) // Folder
        {
            folderAddition = folderAddition != "" ? (folderAddition + FileSystem.SEPERATOR + FileSystem.GetFolderName(name)) : FileSystem.GetFolderName(name);
            UpdateDataList();
        }
        else // File
        {
            selected = name.Trim();
            dataLoader.Load(name, folderAddition);
            dirty = false;
        }
    }

    private void Delete()
    {
        int[] selectedIndexes = dataList.GetSelectedItems();
        List<string> names = selectedIndexes.ToList().ConvertAll(a => dataList.GetItemText(a));
        MessageBox.ShowConfirmDialogue(this, "Are you sure you want to delete " + string.Join(", ", names) + "?", () =>
        {
            for (int i = 0; i < selectedIndexes.Length; i++)
            {
                string selectedItem = names[i];
                if (selectedIndexes[i] < folderCount)
                {
                    FileSystem.DeleteFolder(folderFullPath, selectedItem);
                }
                else
                {
                    FileSystem.DeleteFile(folderFullPath, selectedItem);
                }
            }
            UpdateDataList();
        });
    }

    private void New()
    {
        if (dirty)
        {
            AskDiscardChanges(() => New());
            return;
        }
        selected = null;
        dataLoader.New();
        dirty = false;
    }

    private void NewFolder()
    {
        InputBox.Show(this, "Enter new folder name:", (name) =>
        {
            FileSystem.CreateFolder(folderFullPath, name);
            UpdateDataList();
        });
    }

    private void Back()
    {
        int index = folderAddition.LastIndexOf(FileSystem.SEPERATOR);
        folderAddition = index > 0 ? folderAddition.Substr(0, index) : "";
        UpdateDataList();
    }

    private void Exit()
    {
        GameDataPreloader.Current?.ReloadFolder(dataLoader.DataFolder);
        gameEditorPanel.Visible = true;
        QueueFree();
    }

    private void UpdateDataList()
    {
        List<string> folders = FileSystem.GetFoldersAt(folderFullPath);
        List<string> files = FileSystem.GetFilesAt(folderFullPath);
        dataList.Clear();
        folders.ToList().ForEach(a => dataList.SetItemIcon(dataList.AddItem(a), folderIcon));
        files.ToList().ForEach(a => dataList.SetItemIcon(dataList.AddItem(a), dataLoader.GetIcon(a, folderAddition)));
        folderCount = folders.Count;
        deleteButton.Disabled = true;
    }

    private void AskDiscardChanges(Action postDiscard)
    {
        MessageBox.ShowConfirmDialogue(this, "Discard changes?", () =>
        {
            dirty = false;
            postDiscard();
        });
    }
}

public static class GameDataBrowserExtensions
{
    public static int FindIndex(this ItemList itemList, string query)
    {
        for (int i = 0; i < itemList.ItemCount; i++)
        {
            if (itemList.GetItemText(i) == query)
            {
                return i;
            }
        }
        return -1;
    }

    public static string FixFileName(this string str)
    {
        return str.Replace("\"", "").Replace("\\", "").Replace("/", "").Replace(":", "").Replace("?", "").Replace("|", "").Replace("*", "").Replace("<", "").Replace(">", "");
    }
}
