using Godot;
using System;
using GGE.Internal;

public partial class ImageEditor : Control
{
    // Exports
    [Export]
    private string dataKey;
    [Export]
    private AGameDataLoader loader;
    [Export]
    private Vector2I targetResolution;
    [ExportCategory("Internal exports")]
    [Export]
    private TextureRect previewRect;
    [Export]
    private Button browseButton;
    [Export]
    private FileDialog fileDialog;
    // Properties
    private Sprite2D data;

    [Signal]
    public delegate void OnDirtyEventHandler();

    public override void _Ready()
    {
        base._Ready();
        previewRect.CustomMinimumSize = targetResolution;
        data = loader.GetSprite(dataKey);
        OnDirty += () => loader.EmitSignal(AGameDataLoader.SignalName.OnDirty);
        loader.OnExternalChange += () => previewRect.Texture = data.Texture;
        // Init file dialog
        if (fileDialog == null)
        {
            fileDialog = new FileDialog();
            fileDialog.Init();
        }
        fileDialog.FileSelected += (path) =>
        {
            previewRect.Texture = data.Texture = ImageTexture.CreateFromImage(Image.LoadFromFile(path));
            SetDirty();
        };
        browseButton.Pressed += fileDialog.Show;
        AddChild(fileDialog);
    }

    private void SetDirty() => EmitSignal(SignalName.OnDirty);
}
