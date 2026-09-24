using GdsSharp.Lib.Binary;
using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals;

public class GdsHeader : IGdsWriteableRecord
{
    public const int RecordSize = 4;

    /// <summary>
    ///     Total length of the record, including the header itself.
    ///     When setting make sure you include <see cref="RecordSize" />.
    /// </summary>
    public ushort Length { get; init; }

    /// <summary>
    ///     Number of bytes to read after the header.
    /// </summary>
    public ushort NumToRead => (ushort)(Length - RecordSize);

    /// <summary>
    ///     Record type.
    /// </summary>
    public ushort Code { get; init; }

    public ushort GetLength()
    {
        return RecordSize;
    }

    public static GdsHeader ReadFrom(GdsBinaryReader reader)
    {
        return new GdsHeader
        {
            Length = reader.ReadUInt16(),
            Code = reader.ReadUInt16()
        };
    }

    public void Write(GdsBinaryWriter writer)
    {
        writer.Write(Length);
        writer.Write(Code);
    }
}
