using Godot;
using System;

public partial class GameEditorButton : BaseButton
{
    [Export]
    private PackedScene SceneDataEditor;
    [Export]
    private PackedScene SceneGameDataBrowser;
    [Export]
    private Control GameEditorPanel;

    public override void _Ready()
    {
        base._Ready();
        Pressed += () => SceneGameDataBrowser.Instantiate<GameDataBrowser>().Init(GameEditorPanel, SceneDataEditor);
    }
}
