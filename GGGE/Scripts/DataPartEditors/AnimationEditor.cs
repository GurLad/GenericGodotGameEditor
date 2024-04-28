using Godot;
using System;
using System.Collections.Generic;
using GGE.Internal;

public partial class AnimationEditor : Control
{
    public enum DataEditorMode { Allow, ReadOnly, Hidden }

    // Exports
    [Export]
    private string animationName = "default";
    [Export]
    private string dataKey;
    [Export]
    private AGameDataLoader loader;
    [Export]
    private Vector2I targetResolution;
    [ExportGroup("Optional data editors")]
    [Export]
    private DataEditorMode frameCountEditMode;
    [Export]
    private DataEditorMode speedEditMode;
    [Export]
    private DataEditorMode loopEditMode;
    [ExportCategory("Internal exports")]
    [ExportGroup("Animation editing")]
    [Export]
    private TextureRect previewRect;
    [Export]
    private Button browseButton;
    [Export]
    private Button togglePreview;
    [Export]
    private FileDialog fileDialog;
    [ExportGroup("Data editing")]
    [Export]
    private SpinBox frameCountEdit;
    [Export]
    private SpinBox speedEdit;
    [Export]
    private CheckBox loopEdit;
    // Properties
    private AnimatedSprite2D data;
    private int currentFrame;
    private Timer previewTimer;
    private List<Texture2D> _frames = new List<Texture2D>();
    private List<Texture2D> frames
    {
        get => _frames;
        set
        {
            _frames = value;
            while (data.SpriteFrames.GetFrameCount(animationName) > 0)
            {
                data.SpriteFrames.RemoveFrame(animationName, 0);
            }
            frames.ForEach(a => data.SpriteFrames.AddFrame(animationName, a));
        }
    }

    [Signal]
    public delegate void OnDirtyEventHandler();

    public override void _Ready()
    {
        base._Ready();
        previewRect.CustomMinimumSize = targetResolution;
        data = loader.GetAnimatedSprite(dataKey);
        OnDirty += () => loader.EmitSignal(AGameDataLoader.SignalName.OnDirty);
        loader.OnExternalChange += () =>
        {
            frames.Clear();
            for (int i = 0; i < data.SpriteFrames.GetFrameCount(animationName); i++)
            {
                frames.Add(data.SpriteFrames.GetFrameTexture(animationName, i));
            }
            UpdateTimerData();
            UpdatePreview();
        };
        // Init preview timer
        AddChild(previewTimer = new Timer());
        UpdateTimerData();
        previewTimer.Timeout += () =>
        {
            if (frames.Count > 0)
            {
                currentFrame = (currentFrame + 1) % frames.Count;
                UpdatePreview();
                if (currentFrame >= frames.Count - 1 && !data.SpriteFrames.GetAnimationLoop(animationName))
                {
                    previewTimer.Stop();
                    togglePreview.Text = "Play";
                }
            }
        };
        togglePreview.Pressed += () =>
        {
            if (previewTimer.IsStopped())
            {
                previewTimer.Start();
                togglePreview.Text = "Stop";
            }
            else
            {
                previewTimer.Stop();
                togglePreview.Text = "Play";
            }
        };
        // Init file dialog
        if (fileDialog == null)
        {
            fileDialog = new FileDialog();
            fileDialog.Init();
        }
        fileDialog.FileSelected += (path) =>
        {
            Image source = Image.LoadFromFile(path);
            if (targetResolution.X > 0)
            {
                frames = source.Split(source.GetWidth() / targetResolution.X);
                UpdatePreview();
                SetDirty();
            }
            else
            {
                // Not ideal, but it works
                Node parentTemp = this;
                Control parent = this;
                while (parentTemp != null)
                {
                    //GD.Print(parentTemp.Name);
                    if (parentTemp is Control control)
                    {
                        parent = control;
                    }
                    parentTemp = parentTemp.GetParent();
                };
                InputBox.Show(parent, "Enter frame count:", (s) =>
                {
                    int frameCount;
                    if (int.TryParse(s, out frameCount) && frameCount > 0)
                    {
                        frames = source.Split(frameCount);
                        UpdatePreview();
                        SetDirty();
                    }
                    else
                    {
                        MessageBox.ShowError(parent, "Invalid frame count!");
                    }
                });
            }
        };
        browseButton.Pressed += fileDialog.Show;
        AddChild(fileDialog);
        // Init extra editors
        if (frameCountEdit != null)
        {
            frameCountEdit.Rounded = true;
            InitExtraEditor(frameCountEdit, frameCountEditMode,
                () => frameCountEdit.ValueChanged += (i) =>
                {
                    if (frames.Count > 0)
                    {
                        int value = Mathf.RoundToInt(i);
                        if (value != frames.Count)
                        {
                            frames = frames.Combine().Split(value);
                        }
                    }
                    SetDirty();
                },
                () => frameCountEdit.Value = data.SpriteFrames.GetFrameCount(animationName),
                (editable) => frameCountEdit.Editable = editable);
        }
        if (speedEdit != null)
        {
            InitExtraEditor(speedEdit, speedEditMode,
                () => speedEdit.ValueChanged += (i) => { data.SpriteFrames.SetAnimationSpeed(animationName, i); SetDirty(); },
                () => speedEdit.Value = data.SpriteFrames.GetAnimationSpeed(animationName),
                (editable) => speedEdit.Editable = editable);
        }
        if (loopEdit != null)
        {
            InitExtraEditor(loopEdit, loopEditMode,
                () => loopEdit.Toggled += (b) => { data.SpriteFrames.SetAnimationLoop(animationName, b); SetDirty(); },
                () => loopEdit.ButtonPressed = data.SpriteFrames.GetAnimationLoop(animationName),
                (editable) => loopEdit.Disabled = !editable);
        }
    }

    private void UpdatePreview()
    {
        if (frames.Count > 0)
        {
            previewRect.Texture = frames[currentFrame % frames.Count];
        }
        else
        {
            previewRect.Texture = null;
        }
    }

    private void UpdateTimerData()
    {
        previewTimer.WaitTime = data.SpriteFrames.GetAnimationSpeed(animationName);
        previewTimer.WaitTime = previewTimer.WaitTime > 0 ? 1 / previewTimer.WaitTime : 1;
        previewTimer.OneShot = false;
    }

    private void InitExtraEditor(Control editor, DataEditorMode mode, Action setValueChanged, Action refresh, Action<bool> setEditable)
    {
        if (mode == DataEditorMode.Allow)
        {
            setValueChanged();
            setEditable(true);
        }
        else
        {
            setEditable(false);
        }
        if (mode != DataEditorMode.Hidden)
        {
            loader.OnExternalChange += () => refresh();
            OnDirty += () => refresh();
            editor.GetParent<Control>().Visible = true;
        }
        else
        {
            editor.GetParent<Control>().Visible = false;
        }
    }

    private void SetDirty() => EmitSignal(SignalName.OnDirty);
}
