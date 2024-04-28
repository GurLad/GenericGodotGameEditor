using Godot;
using System;

namespace GGE.Internal
{
    public partial class GameEditorButton : BaseButton
    {
        [Export]
        private PackedScene SceneDataEditor;
        [ExportCategory("Shared")]
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
}
