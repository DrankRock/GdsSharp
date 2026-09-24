using GdsSharp.Lib.Binary;
using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordColRow : IGdsSimpleRead, IGdsSimpleWrite
{
    public short NumCols { get; set; }
    public short NumRows { get; set; }

    public ushort Code => 0x1302;

    public ushort GetLength()
    {
        return 4;
    }

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        NumCols = reader.ReadInt16();
        NumRows = reader.ReadInt16();
    }

    public void Write(GdsBinaryWriter writer)
    {
        writer.Write(NumCols);
        writer.Write(NumRows);
    }
}