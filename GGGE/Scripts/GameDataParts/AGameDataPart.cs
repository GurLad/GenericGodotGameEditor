using Godot;
using System;

namespace GGE.Internal
{
    public abstract class AGameDataPart<NodeType, RecordType> : AGameDataPart
    {
        protected const char SEPERATOR = '-';
        protected virtual string DATA_FILE => "Data.data";

        public NodeType SourceNode { get; init; }
        protected string fileExtension { get; init; }

        protected AGameDataPart(string name, NodeType sourceNode, string fileExtension)
        {
            Name = name;
            SourceNode = sourceNode;
            this.fileExtension = fileExtension;
        }

        protected string GetFullPath(string folderPath, bool includeExtension = true) =>
            folderPath + FileSystem.SEPERATOR + Name + (includeExtension ? fileExtension : "");

        protected abstract RecordType SaveToRecordInternal();

        public override object SaveToRecord() => SaveToRecordInternal();

        protected abstract void LoadFromRecordInternal(RecordType record);

        public override void LoadFromRecord(object record)
        {
            if (record != null && record is RecordType recordType)
            {
                LoadFromRecordInternal(recordType);
            }
            else
            {
                throw new Exception("Record type mismatch! " + Name + fileExtension + ", got " + (record ?? "null"));
            }
        }
    }
}

public abstract class AGameDataPart
{
    public string Name { get; set; }

    public abstract void Save(string folderPath);

    public abstract object SaveToRecord();

    public abstract void Load(string folderPath);

    public abstract void LoadFromRecord(object record);

    public abstract void Clear();
}
