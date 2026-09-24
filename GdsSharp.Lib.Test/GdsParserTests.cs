using System.Reflection;
using FluentAssertions;
using GdsSharp.Lib.Binary;
using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.NonTerminals.Elements;
using GdsSharp.Lib.NonTerminals.Enum;
using GdsSharp.Lib.Terminals;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib.Test;

public class GdsParserTests
{
    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("gds3d_example.gds")]
    public void TestParserDoesntCrash(string manifestFile)
    {
        using var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        using var stream = new GdsTokenStream(fileStream);
        var parser = new GdsParser(stream);
        var file = parser.Parse();
        file.Materialize();
    }

    private static GdsRecordBgnLib ValidBgnLib() => new()
    {
        LastModificationTimeYear = 2024,
        LastModificationTimeMonth = 1,
        LastModificationTimeDay = 1,
        LastAccessTimeYear = 2024,
        LastAccessTimeMonth = 1,
        LastAccessTimeDay = 1
    };

    private static GdsRecordBgnStr ValidBgnStr() => new()
    {
        CreationTimeYear = 2024,
        CreationTimeMonth = 1,
        CreationTimeDay = 1,
        LastModificationTimeYear = 2024,
        LastModificationTimeMonth = 1,
        LastModificationTimeDay = 1
    };

    [Test]
    public void TestParserParsesPathWithExtensions()
    {
        using var ms = new MemoryStream();
        GdsWriter.Write(new List<IGdsRecord>
        {
            new GdsRecordHeader { Value = 600 },
            ValidBgnLib(),
            new GdsRecordLibName { Value = "TEST" },
            new GdsRecordUnits(),
            ValidBgnStr(),
            new GdsRecordStrName { Value = "EXT" },
            new GdsRecordNoData { Type = GdsRecordNoDataType.Path },
            new GdsRecordLayer { Value = 1 },
            new GdsRecordDataType { Value = 0 },
            new GdsRecordPathType { Value = 4 },
            new GdsRecordWidth { Value = 200 },
            new GdsRecordBgnExt { Value = 150 },
            new GdsRecordEndExt { Value = 150 },
            new GdsRecordXy { NumPoints = 2, Coordinates = new[] { new GdsPoint(0, 0), new GdsPoint(1000, 0) } },
            new GdsRecordNoData { Type = GdsRecordNoDataType.EndEl },
            new GdsRecordNoData { Type = GdsRecordNoDataType.EndStr },
            new GdsRecordNoData { Type = GdsRecordNoDataType.EndLib }
        }, ms);
        ms.Position = 0;

        var file = GdsFile.From(ms);
        var path = file.Structures.Single().Elements.Single().Element.Should().BeOfType<GdsPathElement>().Subject;

        path.PathType.Should().Be((GdsPathType)4);
        path.Width.Should().Be(200);
        path.BeginExtension.Should().Be(150);
        path.EndExtension.Should().Be(150);
    }

    [Test]
    public void TestParserSkipsUnknownRecords()
    {
        using var ms = new MemoryStream();
        var writer = new GdsBinaryWriter(ms);
        // Header for an unknown record (type 0x55) with a 4 byte payload
        writer.Write((ushort)8);
        writer.Write((ushort)0x5502);
        writer.Write((short)42);
        writer.Write((short)43);
        GdsWriter.Write(new List<IGdsRecord>
        {
            new GdsRecordHeader { Value = 600 },
            ValidBgnLib(),
            new GdsRecordLibName { Value = "TEST" },
            new GdsRecordUnits(),
            ValidBgnStr(),
            new GdsRecordStrName { Value = "UNK" },
            new GdsRecordNoData { Type = GdsRecordNoDataType.Boundary },
            new GdsRecordLayer { Value = 2 },
            new GdsRecordDataType { Value = 0 },
            new GdsRecordXy
            {
                NumPoints = 5,
                Coordinates = new[]
                {
                    new GdsPoint(0, 0), new GdsPoint(100, 0), new GdsPoint(100, 100), new GdsPoint(0, 100),
                    new GdsPoint(0, 0)
                }
            },
            new GdsRecordNoData { Type = GdsRecordNoDataType.EndEl },
            new GdsRecordNoData { Type = GdsRecordNoDataType.EndStr },
            new GdsRecordNoData { Type = GdsRecordNoDataType.EndLib }
        }, ms);
        ms.Position = 0;

        var file = GdsFile.From(ms);
        var boundary = file.Structures.Single().Elements.Single().Element.Should().BeOfType<GdsBoundaryElement>().Subject;

        boundary.Layer.Should().Be(2);
    }

    [Test]
    public void TestParserParsesPathWithCanonicalPathTypeCode()
    {
        using var ms = new MemoryStream();
        var writer = new GdsBinaryWriter(ms);
        WriteSimpleRecord(writer, new GdsRecordHeader { Value = 600 });
        WriteSimpleRecord(writer, ValidBgnLib());
        WriteSimpleRecord(writer, new GdsRecordLibName { Value = "TEST" });
        WriteSimpleRecord(writer, new GdsRecordUnits());
        WriteSimpleRecord(writer, ValidBgnStr());
        WriteSimpleRecord(writer, new GdsRecordStrName { Value = "CANON" });
        writer.Write((ushort)4);
        writer.Write((ushort)0x0900); // PATH
        WriteSimpleRecord(writer, new GdsRecordLayer { Value = 1 });
        WriteSimpleRecord(writer, new GdsRecordDataType { Value = 0 });
        writer.Write((ushort)6);
        writer.Write((ushort)0x2002); // PATHTYPE with its canonical GDSII record number
        writer.Write((short)1);
        WriteSimpleRecord(writer, new GdsRecordWidth { Value = 100 });
        WriteSimpleRecord(writer, new GdsRecordXy
        {
            NumPoints = 2,
            Coordinates = new[] { new GdsPoint(0, 0), new GdsPoint(500, 0) }
        });
        writer.Write((ushort)4);
        writer.Write((ushort)0x1100); // ENDEL
        writer.Write((ushort)4);
        writer.Write((ushort)0x0700); // ENDSTR
        writer.Write((ushort)4);
        writer.Write((ushort)0x0400); // ENDLIB
        ms.Position = 0;

        var file = GdsFile.From(ms);
        var path = file.Structures.Single().Elements.Single().Element.Should().BeOfType<GdsPathElement>().Subject;

        path.PathType.Should().Be(GdsPathType.Rounded);
        path.Width.Should().Be(100);
    }

    private static void WriteSimpleRecord(GdsBinaryWriter writer, IGdsWriteableRecord record)
    {
        writer.Write((ushort)(record.GetLength() + GdsHeader.RecordSize));
        writer.Write(record.Code);
        record.Write(writer);
    }
}