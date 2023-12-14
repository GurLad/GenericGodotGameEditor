using Godot;
using System;

public partial class GameDataEditor : Node
{
    [Export]
    public AGameDataLoader DataLoader { get; private set; }
}
