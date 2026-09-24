using GdsSharp.Lib.Binary;
using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordUnits : IGdsSimpleRead, IGdsSimpleWrite
{
    public double UserUnits { get; set; }
    public double PhysicalUnits { get; set; }

    public double UserUnitInMeters => PhysicalUnits / UserUnits;

    public ushort Code => 0x0305;

    public ushort GetLength()
    {
        return 16;
    }

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        UserUnits = reader.ReadDouble();
        PhysicalUnits = reader.ReadDouble();
    }

    public void Write(GdsBinaryWriter writer)
    {
        writer.Write(UserUnits);
        writer.Write(PhysicalUnits);
    }
}