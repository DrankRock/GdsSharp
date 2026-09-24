using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordBgnExt : GenericGdsRecord<int>
{
    public override ushort Code => 0x3203;
}
