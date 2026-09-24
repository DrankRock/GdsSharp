using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib.Abstractions;

public class GdsStreamOperator
{
    protected static readonly Dictionary<ushort, Func<IGdsRecord>> Activators = new()
    {
        [0x0002] = () => new GdsRecordHeader(),
        [0x0102] = () => new GdsRecordBgnLib(),
        [0x0206] = () => new GdsRecordLibName(),
        [0x0305] = () => new GdsRecordUnits(),
        [0x0400] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.EndLib },
        [0x0502] = () => new GdsRecordBgnStr(),
        [0x0606] = () => new GdsRecordStrName(),
        [0x0700] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.EndStr },
        [0x0800] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.Boundary },
        [0x0900] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.Path },
        [0x0A00] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.Sref },
        [0x0B00] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.Aref },
        [0x0C00] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.Text },
        [0x0D02] = () => new GdsRecordLayer(),
        [0x0E02] = () => new GdsRecordDataType(),
        [0x0F03] = () => new GdsRecordWidth(),
        [0x1003] = () => new GdsRecordXy { NumPoints = 0, Coordinates = [] },
        [0x1100] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.EndEl },
        [0x1206] = () => new GdsRecordSName(),
        [0x1302] = () => new GdsRecordColRow(),
        [0x1500] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.Node },
        [0x1602] = () => new GdsRecordTextType(),
        [0x1701] = () => new GdsRecordPresentation(),
        [0x1906] = () => new GdsRecordString(),
        [0x1A01] = () => new GdsRecordSTrans(),
        [0x1B05] = () => new GdsRecordMag(),
        [0x1C05] = () => new GdsRecordAngle(),
        [0x1F06] = () => new GdsRecordRefLibs(),
        [0x2006] = () => new GdsRecordFonts(),
        [0x2102] = () => new GdsRecordPathType(),
        [0x2202] = () => new GdsRecordGenerations(),
        [0x2601] = () => new GdsRecordElFlags(),
        [0x2A02] = () => new GdsRecordNodeType(),
        [0x2B02] = () => new GdsRecordPropAttr(),
        [0x2C06] = () => new GdsRecordPropValue(),
        [0x2D00] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.Box },
        [0x2E02] = () => new GdsRecordBoxType(),
        [0x2F03] = () => new GdsRecordPlex(),
        [0x3202] = () => new GdsRecordTapeNum(),
        [0x3203] = () => new GdsRecordBgnExt(),
        [0x3302] = () => new GdsRecordTapeCode(),
        [0x3303] = () => new GdsRecordEndExt(),
        [0x3602] = () => new GdsRecordFormat(),
        [0x3706] = () => new GdsRecordMask(),
        [0x3800] = () => new GdsRecordNoData { Type = GdsRecordNoDataType.EndMasks }
    };

    /// <summary>
    ///     Alternative record codes that resolve to the activator of the given primary code.
    ///     Some tools in the wild write records using legacy Calma record numbers or
    ///     deviating from the GDSII specification.
    /// </summary>
    private static readonly Dictionary<ushort, ushort> CodeAliases = new()
    {
        [0x3003] = 0x3203, // BGNEXTN written with its legacy Calma record number
        [0x3103] = 0x3303, // ENDEXTN written with its legacy Calma record number
        [0x2002] = 0x2102  // PATHTYPE written with its canonical GDSII record number
    };

    static GdsStreamOperator()
    {
        foreach (var (alias, primary) in CodeAliases)
            if (Activators.TryGetValue(primary, out var activator))
                Activators.Add(alias, activator);
    }
}
