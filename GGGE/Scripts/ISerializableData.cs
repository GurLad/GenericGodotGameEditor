using Godot;
using System;

public interface ISerializableData
{
    public string Save();

    public void Load(string data);

    public void Clear();
}
