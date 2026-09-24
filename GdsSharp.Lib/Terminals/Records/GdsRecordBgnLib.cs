using GdsSharp.Lib.Binary;
using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordBgnLib : IGdsSimpleRead, IGdsSimpleWrite
{
    public short LastModificationTimeYear { get; set; }
    public short LastModificationTimeMonth { get; set; }
    public short LastModificationTimeDay { get; set; }
    public short LastModificationTimeHour { get; set; }
    public short LastModificationTimeMinute { get; set; }
    public short LastModificationTimeSecond { get; set; }

    public short LastAccessTimeYear { get; set; }
    public short LastAccessTimeMonth { get; set; }
    public short LastAccessTimeDay { get; set; }
    public short LastAccessTimeHour { get; set; }
    public short LastAccessTimeMinute { get; set; }
    public short LastAccessTimeSecond { get; set; }

    public ushort Code => 0x0102;

    public ushort GetLength()
    {
        return 24;
    }

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        LastModificationTimeYear = reader.ReadInt16();
        LastModificationTimeMonth = reader.ReadInt16();
        LastModificationTimeDay = reader.ReadInt16();
        LastModificationTimeHour = reader.ReadInt16();
        LastModificationTimeMinute = reader.ReadInt16();
        LastModificationTimeSecond = reader.ReadInt16();
        LastAccessTimeYear = reader.ReadInt16();
        LastAccessTimeMonth = reader.ReadInt16();
        LastAccessTimeDay = reader.ReadInt16();
        LastAccessTimeHour = reader.ReadInt16();
        LastAccessTimeMinute = reader.ReadInt16();
        LastAccessTimeSecond = reader.ReadInt16();
    }

    public void Write(GdsBinaryWriter writer)
    {
        writer.Write(LastModificationTimeYear);
        writer.Write(LastModificationTimeMonth);
        writer.Write(LastModificationTimeDay);
        writer.Write(LastModificationTimeHour);
        writer.Write(LastModificationTimeMinute);
        writer.Write(LastModificationTimeSecond);
        writer.Write(LastAccessTimeYear);
        writer.Write(LastAccessTimeMonth);
        writer.Write(LastAccessTimeDay);
        writer.Write(LastAccessTimeHour);
        writer.Write(LastAccessTimeMinute);
        writer.Write(LastAccessTimeSecond);
    }
}