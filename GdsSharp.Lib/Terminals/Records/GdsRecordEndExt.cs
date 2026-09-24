using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordEndExt : GenericGdsRecord<int>
{
    public override ushort Code => 0x3303;
}
