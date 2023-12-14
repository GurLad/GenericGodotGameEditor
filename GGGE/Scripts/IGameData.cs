using Godot;
using System;

public interface IGameData
{
    public string Save();

    public void Load(string data);

    public void Clear();
}
