using Godot;
using System;

public partial class GameDataAudioStreamPart : GGE.Internal.AGameDataPart<GameDataAudioStreamPart.StreamWithPath, AudioStream>
{
    public GameDataAudioStreamPart(string name, AudioStreamPlayer player) :
        base(name, new StreamWithPath(player), ".ogg") { }

    public override void Clear()
    {
        SourceNode.Path = "";
    }

    public override void Load(string folderPath)
    {
        SourceNode.Path = GetFullPath(folderPath);
    }

    protected override void LoadFromRecordInternal(AudioStream record)
    {
        SourceNode.Player.Stream = record;
    }

    public override void Save(string folderPath)
    {
        if (string.IsNullOrEmpty(SourceNode.Path))
        {
            if (SourceNode.Player.Stream != null)
            {
                throw new Exception("Trying to save a stream without a defined path!");
            }
            else
            {
                return; // There's nothing to save
            }
        }
        if (SourceNode.Path != GetFullPath(folderPath))
        {
            DirAccess.CopyAbsolute(SourceNode.Path, GetFullPath(folderPath));
        }
    }

    protected override AudioStream SaveToRecordInternal()
    {
        return SourceNode.Player.Stream;
    }

    public class StreamWithPath
    {
        public AudioStreamPlayer Player { get; init; }
        private string _path = "";
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                if (!string.IsNullOrEmpty(Path) && FileAccess.FileExists(Path))
                {
                    Player.Stream = AudioStreamOggVorbis.LoadFromFile(Path);
                }
                else
                {
                    _path = "";
                    Player.Stream = null;
                }
            }
        }

        public StreamWithPath(AudioStreamPlayer player) => Player = player;
    }
}
