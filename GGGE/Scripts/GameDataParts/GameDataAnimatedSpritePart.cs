using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameDataAnimatedSpritePart : GGE.Internal.AGameDataPart<AnimatedSprite2D, SpriteFrames>
{
    protected override string DATA_FILE => "AnimationData.data";

    private bool lockAnimations = true;
    private List<AnimationData> baseAnimations = new List<AnimationData>();
    private SpriteFrames spriteFrames => SourceNode.SpriteFrames;

    public GameDataAnimatedSpritePart(string name, AnimatedSprite2D sourceNode, string fileExtension = ".png") :
        base(name, sourceNode, fileExtension)
    {
        lockAnimations = false;
    }

    public GameDataAnimatedSpritePart(string name, AnimatedSprite2D sourceNode, string fileExtension, params AnimationData[] baseAnimations) :
        base(name, sourceNode, fileExtension)
    {
        lockAnimations = true;
        this.baseAnimations = baseAnimations.ToList();
    }

    public GameDataAnimatedSpritePart(string name, AnimatedSprite2D sourceNode, string fileExtension, params string[] baseAnimations) :
        this(name, sourceNode, fileExtension, baseAnimations.ToList().ConvertAll(a => new AnimationData(a, 1, true)).ToArray())
    { }

    public GameDataAnimatedSpritePart(string name, AnimatedSprite2D sourceNode, string fileExtension, int defaultSpeed, bool defaultLoop, params string[] baseAnimations) :
        this(name, sourceNode, fileExtension, baseAnimations.ToList().ConvertAll(a => new AnimationData(a, defaultSpeed, defaultLoop)).ToArray())
    { }

    public override void Clear()
    {
        spriteFrames.ClearAll();
        if (lockAnimations)
        {
            baseAnimations.ForEach(a =>
            {
                if (a.Name != "default")
                {
                    spriteFrames.AddAnimation(a.Name);
                }
                spriteFrames.SetAnimationSpeed(a.Name, a.Speed);
                spriteFrames.SetAnimationLoop(a.Name, a.Loops);
            });
        }
    }

    public override void Load(string folderPath)
    {
        spriteFrames.ClearAll();
        string basePath = GetFullPath(folderPath, false);
        string data = FileSystem.LoadTextFile(basePath + SEPERATOR + DATA_FILE, "");
        List<AnimationData> animationData = data.JsonToObject<List<AnimationData>>();
        List<string> animations = lockAnimations ? baseAnimations.ConvertAll(a => a.Name) : animationData.ConvertAll(a => a.Name);
        for (int i = 0; i < animations.Count; i++)
        {
            string animation = animations[i];
            if (animation != "default")
            {
                spriteFrames.AddAnimation(animation);
            }
            if (i < animationData.Count && animation == animationData[i].Name)
            {
                List<Texture2D> frames = FileSystem.LoadAnimatedTextureFile(basePath + SEPERATOR + animation, animationData[i].NumFrames);
                frames.ForEach(a => spriteFrames.AddFrame(animation, a));
                if (animationData[i].Speed > 0)
                {
                    spriteFrames.SetAnimationSpeed(animation, animationData[i].Speed);
                }
                else if (lockAnimations)
                {
                    spriteFrames.SetAnimationSpeed(animation, baseAnimations[i].Speed);
                }
                spriteFrames.SetAnimationLoop(animation, animationData[i].Loops);
            }
        }
    }

    protected override void LoadFromRecordInternal(SpriteFrames record)
    {
        if (lockAnimations)
        {
            List<string> recordAnimations = record.GetAnimationNames().ToList();
            if (baseAnimations.Count != recordAnimations.Count ||
                baseAnimations.Find(a => !recordAnimations.Contains(a.Name)) != null)
            {
                throw new Exception("Inconsistent record vs. source!" +
                    "\nSource: " + string.Join(", ", baseAnimations) +
                    "\nRecord: " + string.Join(", ", recordAnimations));
            }
        }
        SourceNode.SpriteFrames = record;
    }

    public override void Save(string folderPath)
    {
        string basePath = GetFullPath(folderPath, false);
        List<AnimationData> animationData = new List<AnimationData>();
        List<string> animations = lockAnimations ? baseAnimations.ConvertAll(a => a.Name) : spriteFrames.GetAnimationNames().ToList();
        for (int i = 0; i < animations.Count; i++)
        {
            string animation = animations[i];
            Image template = spriteFrames.GetFrameTexture(animation, 0)?.GetImage();
            if (template != null)
            {
                int numFrames = spriteFrames.GetFrameCount(animation);
                double speed = spriteFrames.GetAnimationSpeed(animation);
                // If animations are locked and the speed wasn't changed, save -1 to make changing default speed easier
                if (lockAnimations && Mathf.Abs(baseAnimations[i].Speed - speed) < 0.01)
                {
                    speed = -1;
                }
                bool loops = spriteFrames.GetAnimationLoop(animation);
                animationData.Add(new AnimationData(animation, numFrames, speed, loops));
                Image result = Image.Create(template.GetWidth() * numFrames, template.GetHeight(), false, template.GetFormat());
                Rect2I sourceRect = new Rect2I(0, 0, template.GetWidth(), template.GetHeight());
                for (int j = 0; j < numFrames; j++)
                {
                    result.BlitRect(spriteFrames.GetFrameTexture(animation, j)?.GetImage(), sourceRect, new Vector2I(j * template.GetWidth(), 0));
                }
                result.SavePng(basePath + SEPERATOR + animation + fileExtension);
            }
        }
        using var dataFile = FileAccess.Open(basePath + SEPERATOR + DATA_FILE, FileAccess.ModeFlags.Write);
        if (dataFile == null)
        {
            throw new Exception("Error creating file " + (basePath + SEPERATOR + DATA_FILE) + "!");
        }
        dataFile.StoreString(animationData.ToJson());
    }

    protected override SpriteFrames SaveToRecordInternal()
    {
        return spriteFrames;
    }

    public record AnimationData(string Name, int NumFrames, double Speed, bool Loops)
    {
        public AnimationData(string Name, double Speed, bool Loops) : this(Name, 0, Speed, Loops) { }

        [System.Text.Json.Serialization.JsonConstructor]
        public AnimationData(string Name, int NumFrames) : this(Name, NumFrames, -1, true) { }
    }
}
