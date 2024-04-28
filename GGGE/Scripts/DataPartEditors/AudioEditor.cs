using GGE.Internal;
using Godot;
using System;

public partial class AudioEditor : Node
{
    // Exports
    [Export]
    private string dataKey;
    [Export]
    private AGameDataLoader loader;
    [ExportCategory("Internal exports")]
    [Export]
    private Button browseButton;
    [Export]
    private Button playButton;
    [Export]
    private Label pathLabel;
    [Export]
    private FileDialog fileDialog;
    // Properties
    private GameDataAudioStreamPart.StreamWithPath data;

    [Signal]
    public delegate void OnDirtyEventHandler();

    public override void _Ready()
    {
        base._Ready();
        data = loader.GetAudioStream(dataKey);
        OnDirty += () => loader.EmitSignal(AGameDataLoader.SignalName.OnDirty);
        loader.OnExternalChange += () => UpdateLabel(data.Path);
        // Init file dialog
        if (fileDialog == null)
        {
            fileDialog = new FileDialog();
            fileDialog.Init();
            fileDialog.Filters = new string[] { "*.ogg ; OGG sound files" };
        }
        fileDialog.FileSelected += (path) =>
        {
            data.Path = path;
            UpdateLabel(path);
            SetDirty();
        };
        browseButton.Pressed += fileDialog.Show;
        playButton.Pressed += () => data.Player.Play();
        AddChild(fileDialog);
    }

    private void SetDirty() => EmitSignal(SignalName.OnDirty);

    private void UpdateLabel(string path)
    {
        if (pathLabel != null)
        {
            pathLabel.Text = path.Length > 0 ? path.Substring(path.Replace("/", "\\").LastIndexOf("\\") + 1) : "";
        }
    }
}
