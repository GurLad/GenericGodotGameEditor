using Godot;
using System;

public partial class InputBox : Control
{
    private static PackedScene template;

    // Exports
    [Export]
    private Label prompt;
    [Export]
    private LineEdit input;
    [Export]
    private Button okayButton;
    [Export]
    private Button cancelButton;
    // Properties
    private Action<string> onFinish;

    public static InputBox Show(Control parent, string prompt, Action<string> onFinish)
    {
        InputBox newBox = template.Instantiate<InputBox>();
        parent.GetParent().AddChild(newBox);
        newBox.prompt.Text = prompt;
        newBox.onFinish = onFinish;
        newBox.Visible = true;
        newBox.input.GrabFocus();
        return newBox;
    }

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
            input.TextSubmitted += (s) => Submit(s);
            okayButton.Pressed += () => Submit(input.Text);
            cancelButton.Pressed += () => Submit(null);
        }
    }

    public void Submit(string s)
    {
        if (s != null)
        {
            onFinish(s);
        }
        QueueFree();
    }
}
