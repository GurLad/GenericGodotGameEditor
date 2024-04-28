using Godot;
using System;

namespace GGE.Internal
{
    public partial class MessageBox : Control
    {
        private static PackedScene template;

        // Exports
        [Export]
        private RichTextLabel title;
        [Export]
        private Label text;
        [Export]
        private Button yesButton;
        [Export]
        private Button noButton;
        // Properties
        private Action yesAction;

        public static MessageBox ShowConfirmDialogue(Control parent, string title, string text, Action yesAction)
        {
            MessageBox newBox = template.Instantiate<MessageBox>();
            parent.GetParent().AddChild(newBox);
            newBox.title.Text = "[center][b]" + title + "[/b][/center]";
            newBox.text.Text = text;
            newBox.yesAction = yesAction;
            newBox.Visible = true;
            newBox.yesButton.GrabFocus();
            return newBox;
        }

        public static MessageBox ShowConfirmDialogue(Control parent, string text, Action yesAction) =>
            ShowConfirmDialogue(parent, "Warning", text, yesAction);

        public static MessageBox ShowMessage(Control parent, string title, string text, Action postAction = null)
        {
            MessageBox newBox = ShowConfirmDialogue(parent, title, text, postAction);
            newBox.yesButton.Text = "Okay";
            newBox.noButton.Visible = false;
            return newBox;
        }

        public static MessageBox ShowMessage(Control parent, string text, Action postAction = null) =>
            ShowMessage(parent, "Info", text, postAction);

        public static MessageBox ShowError(Control parent, string title, string text, Action postAction = null)
        {
            return ShowMessage(parent, title, text, postAction);
        }

        public static MessageBox ShowError(Control parent, string text, Action postAction = null) =>
            ShowMessage(parent, "Error", text, postAction);

        public override void _Ready()
        {
            base._Ready();
            if (template == null)
            {
                template = new PackedScene();
                template.Pack(this);
                QueueFree();
            }
            else
            {
                yesButton.Pressed += () => Submit(true);
                noButton.Pressed += () => Submit(false);
            }
        }

        public void Submit(bool yes)
        {
            if (yes)
            {
                yesAction?.Invoke();
            }
            QueueFree();
        }
    }
}
