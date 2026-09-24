using System.Linq.Expressions;
using System.Reflection;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib.Abstractions;

public class GdsStreamOperator
{
    protected static readonly Dictionary<ushort, Func<IGdsRecord>> Activators = new();

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

    /// <summary>
    ///     Initializes activators for all records.
    /// </summary>
    static GdsStreamOperator()
    {
        var assembly = Assembly.GetAssembly(typeof(GdsStreamOperator));
        if (assembly is null) throw new InvalidOperationException("Could not get assembly");

        // Get compiled activator for all records
        var recordTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IGdsRecord).IsAssignableFrom(t));
        foreach (var recordType in recordTypes)
        {
            var activator = Expression.Lambda<Func<IGdsRecord>>(Expression.New(recordType)).Compile();
            var record = activator.Invoke();
            if (record is null) throw new InvalidOperationException($"Could not get activator for {recordType.Name}");
            Activators.Add(record.Code, activator);
        }

        // Add activator for no data records
        foreach (var value in Enum.GetValues<GdsRecordNoDataType>())
            Activators.Add((ushort)value, () => new GdsRecordNoData { Type = value });

        // Register activators for known alternative record codes
        foreach (var (alias, primary) in CodeAliases)
            if (Activators.TryGetValue(primary, out var activator))
                Activators.Add(alias, activator);
    }
}