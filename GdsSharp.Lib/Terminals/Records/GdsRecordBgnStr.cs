using GdsSharp.Lib.Binary;
using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordBgnStr : IGdsSimpleRead, IGdsSimpleWrite
{
    public short CreationTimeYear { get; set; }
    public short CreationTimeMonth { get; set; }
    public short CreationTimeDay { get; set; }
    public short CreationTimeHour { get; set; }
    public short CreationTimeMinute { get; set; }
    public short CreationTimeSecond { get; set; }

    public short LastModificationTimeYear { get; set; }
    public short LastModificationTimeMonth { get; set; }
    public short LastModificationTimeDay { get; set; }
    public short LastModificationTimeHour { get; set; }
    public short LastModificationTimeMinute { get; set; }
    public short LastModificationTimeSecond { get; set; }

    public ushort Code => 0x0502;

    public ushort GetLength()
    {
        return 24;
    }

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        CreationTimeYear = reader.ReadInt16();
        CreationTimeMonth = reader.ReadInt16();
        CreationTimeDay = reader.ReadInt16();
        CreationTimeHour = reader.ReadInt16();
        CreationTimeMinute = reader.ReadInt16();
        CreationTimeSecond = reader.ReadInt16();
        LastModificationTimeYear = reader.ReadInt16();
        LastModificationTimeMonth = reader.ReadInt16();
        LastModificationTimeDay = reader.ReadInt16();
        LastModificationTimeHour = reader.ReadInt16();
        LastModificationTimeMinute = reader.ReadInt16();
        LastModificationTimeSecond = reader.ReadInt16();
    }

    public void Write(GdsBinaryWriter writer)
    {
        writer.Write(CreationTimeYear);
        writer.Write(CreationTimeMonth);
        writer.Write(CreationTimeDay);
        writer.Write(CreationTimeHour);
        writer.Write(CreationTimeMinute);
        writer.Write(CreationTimeSecond);
        writer.Write(LastModificationTimeYear);
        writer.Write(LastModificationTimeMonth);
        writer.Write(LastModificationTimeDay);
        writer.Write(LastModificationTimeHour);
        writer.Write(LastModificationTimeMinute);
        writer.Write(LastModificationTimeSecond);
    }
}