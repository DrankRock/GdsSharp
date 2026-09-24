using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordWidth : GenericGdsRecord<int>
{
    /// <summary>
    ///     Width in database units. A negative value indicates absolute width.
    /// </summary>
    public override int Value { get; set; }

    /// <summary>
    ///     True if the width is absolute (encoded as a negative width in the GDSII stream).
    /// </summary>
    public bool IsAbsolute => Value < 0;

    public override ushort Code => 0x0F03;
}
