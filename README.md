# GdsSharp

[![NuGet](https://img.shields.io/nuget/v/GdsSharp.svg)](https://www.nuget.org/packages/GdsSharp/)\
A library for reading, editing, and writing [Calma GDSII](https://en.wikipedia.org/wiki/GDSII) files.
The library supports reading and writing in a streaming fashion so it can handle large files.

> [!IMPORTANT]
> **This is a fork.** The upstream project had not been updated in ~3 years and could not import
> several real-world GDSII files. This fork fixes those import failures, modernizes the build and
> dependencies, and makes the library safe to use in AOT/trimmed environments.
> See [Why this fork exists](#why-this-fork-exists) and [Changes in this fork](#changes-in-this-fork).

## Why this fork exists

A production GDSII file (A 6.9 MB photomask layout) failed to import
with a hard crash:

```
InvalidOperationException: Could not find activator for code 0x3003 (12291)
```

The file opens fine in KLayout. Root cause analysis found several problems:

1. **No support for extended paths.** The file contains 103 `PATH` elements, 64 of which are
   *pathtype 4* paths that require `BGNEXTN`/`ENDEXTN` records. The library had no record classes
   for these records at all. Some tools also write these records with the legacy Calma record
   numbers (`0x30`/`0x31` instead of the modern `0x32`/`0x33`), as is the case for this file.
2. **Unknown records crashed the reader.** Instead of skipping records it did not know (as
   KLayout does), the lexer threw an exception. Any slightly nonstandard-but-valid file was
   unreadable.
3. **The internal record table deviates from the GDSII specification** for several records
   (e.g. `PATHTYPE`, `GENERATIONS`, `BOX`/`BOXTYPE`), so spec-compliant files produced by other
   tools could also crash or misparse.
4. **Latent data-loss bugs.** Negative (`absolute`) widths were silently converted to positive
   values on read, and timestamps stored as *years since 1900* (the industry convention) were read
   as year `98 AD` instead of `1998`.

This fork fixes all of the above.

## How it was fixed

- `BGNEXTN`/`ENDEXTN` records are now fully supported: parsed into new
  `GdsPathElement.BeginExtension` / `GdsPathElement.EndExtension` fields and written back on
  export (round-trip safe). Both modern (`0x3203`/`0x3303`) and legacy Calma codes
  (`0x3003`/`0x3103`) are accepted on input; output always uses the modern codes.
- Unknown record codes are now **skipped** using the record's self-describing length field,
  instead of crashing. This matches the behavior of KLayout.
- A small code-alias table maps record codes seen in the wild to their implementations
  (legacy `BGNEXTN`/`ENDEXTN`, and the canonical `PATHTYPE` code `0x2002`).
- `GdsRecordWidth` no longer strips the sign: negative widths (absolute width in GDSII) are
  preserved, `IsAbsolute` works, and `GdsPathElement.IsAbsoluteWidth` /
  `GdsTextElement.IsAbsoluteWidth` are now populated correctly.
- GDSII timestamps stored as years-since-1900 are normalized to real years on parse
  (`98` → `1998`). Full-year timestamps are unaffected.
- The reflection-based infrastructure (`Expression.Compile()` activators, assembly scanning and
  reflection-based property read/write) was replaced with an explicit record table and explicit
  `Read`/`Write` implementations per record. The library no longer uses reflection emit, which
  makes it compatible with NativeAOT and trimming scenarios (e.g. engine/game deployments).
- The NuGet package now ships XML documentation for IntelliSense
  (`GenerateDocumentationFile=true`).

## Building

Requirements: the .NET SDK (8.0 LTS or 10.0 LTS).

```sh
# Restore + build (library, tests, benchmarks, example generator)
dotnet build GdsSharp.sln -c Release

# Run the test suite (51 tests on net8.0 and net10.0)
dotnet test GdsSharp.Lib.Test -c Release

# Create a NuGet package
dotnet pack GdsSharp.Lib -c Release -o ./artifacts
```

The library targets `net8.0` and `net10.0` (the EOL `net7.0` target was dropped).

## Usage

### Installing

```sh
dotnet add package GdsSharp
```

> [!NOTE]
> Until this fork is published under its own package id, you can consume a locally built package:
> run `dotnet pack GdsSharp.Lib -c Release -o C:\gds-local`, then add a `NuGet.config` next to your
> solution with `<packageSources><add key="local" value="C:\gds-local" /></packageSources>` and
> install the package from there.

### Reading a GDSII file

```csharp
using var fileStream = File.OpenRead("file.gds");
var file = GdsFile.From(fileStream);
```

### Writing a GDSII file

```csharp
using var fileStream = File.OpenWrite("file.gds");
file.WriteTo(fileStream);
```

### Editing example

An example of how to create a GDS file from scratch and creating some shapes using the helpers can be found in
the [example project](GdsGenerator/Program.cs).
The created GDS file can be seen below.

As you can see the path in the bottom part of the image is created using the path builder.
In this screenshot the path builder has been set to create elements with a maximum number of 200 vertices.
The path builder will automatically split your high resolution path into multiple elements if the number of vertices exceeds the maximum.
![image](https://github.com/user-attachments/assets/30f91036-09c0-4903-827e-1e57a663ba86)

## Helpers
Some helper functions are provided to draw curved shapes like circles and Bézier curves.

### Circle
```csharp
// Creates a circle at (0, 0) with radius 100. 
// An optional fourth parameter can be used to specify the number of points to use for the discretization of the circle.
var elemCircle = CircleBuilder.CreateCircle(x: 0, y: 0, radius: 100, numPoints: 128),
```

### Rect

```csharp
// Creates a rectangle at (0, 0) with width 100 and height 200.
var elemRect = RectBuilder.CreateRect(x: 0, y: 0, width: 100, height: 200);
```

### Bézier curve

The Bézier curve is created using a builder pattern. The curve is defined by a list of control points, the library supports a maximum of 16 control points per curve.
Like the circle, an optional parameter can be used to specify the number of points to use for the discretization of the curve.
```csharp
// Generate a line from a Bézier curve with width 200.
// When using BuildPolygon the curve will be a GdsBoundaryElement.
var elemPoly = new BezierBuilder()
    .AddPoint(x: 0, y: 0)
    .AddPoint(0, 1000)
    .AddPoint(1000, 1000)
    .AddPoint(1000, 0)
    .BuildPolygon(width: 200, numVertices: 128);

// When using BuildLine the curve will be a GdsPathElement.
// BuildPolygon is recommended because it produces a smaller error in the curve.
var elemLine = new BezierBuilder()
    .AddPoint(x: 0, y: 0)
    .AddPoint(0, 1000)
    .AddPoint(1000, 1000)
    .AddPoint(1000, 0)
    .BuildLine(width: 200, numVertices: 128);
```

### Path builder

GdsSharp also includes a path builder that can be used to create paths with multiple segments.
The output of this code can be seen on the bottom part of the picture above.

```csharp
// Use the path builder to create a path
IEnumerable<GdsElement> elements = new PathBuilder(
        100f,
        new Vector2(-3100, -3300),
        Vector2.UnitX)
    // Straight ahead for 2000 units
    .Straight(2000)

    // Bend 45 degrees to the left with a radius of 500 units
    .BendDeg(-45, 500)

    // Generate shape like <=>
    .Straight(100, widthEnd: 250)
    .Straight(100)
    .Straight(100, widthEnd: 100)

    // Some more bends
    .BendDeg(-45, 500)
    .Straight(100)
    .Straight(200, 250)
    .BendDeg(180, 300)
    .BendDeg(-180, 300)

    // Example of using a function to change the width
    .BendDeg(-180, 900, f => MathF.Cos(f * 50) * 100 + 150)

    // PathBuilder also supports Bézier curves
    .Bezier(b => b
            .AddPoint(0, 0)
            .AddPoint(0, 1000)
            .AddPoint(2000, 1000)
            .AddPoint(1000, 0),
        t => 250 - (250 - 50) * t)
    .Straight(800)

    // Build the path in sections of 200 vertices
    // This is the 'official' maximum number of vertices per element in GDSII
    // In practice, the number of vertices per element can be much higher
    .Build(maxVertices: 200);
```

## Extended paths (pathtype 4)

This fork exposes path end extensions, which the upstream library silently dropped:

```csharp
var file = GdsFile.From(File.OpenRead("mask.gds"));

foreach (var structure in file.Structures)
foreach (var element in structure.Elements)
{
    if (element.Element is GdsPathElement path && path.PathType == (GdsPathType)4)
    {
        Console.WriteLine($"Path with {path.Points.Count} points, " +
                          $"extensions: begin={path.BeginExtension}, end={path.EndExtension}");
    }
}
```

Extensions are written back on export, so files round-trip without losing them.

## Changes in this fork

Detailed changelog of everything that was done, in order:

1. **Modernized the project** (it had not been touched in ~3 years):
   - Target frameworks `net7.0;net8.0` → `net8.0;net10.0` (net7 is end-of-life).
   - NUnit 3.13 → 4.6, test adapter, analyzers, `Microsoft.NET.Test.Sdk`, coverlet, BenchmarkDotNet
     and Nuke build tooling all updated to current versions.
   - GitHub Actions workflows regenerated with modern action versions.
2. **Fixed the import crash** for real-world files:
   - Added `BGNEXTN`/`ENDEXTN` record classes, model fields and (de)serialization.
   - Unknown record codes are skipped instead of throwing.
   - Added alias resolution for legacy/alternative record codes.
   - Added regression tests for all of the above (including round-trip tests).
3. **Fixed data-corruption bugs**:
   - Negative (absolute) widths are preserved instead of silently negated.
   - Legacy years-since-1900 timestamps are normalized on read.
4. **AOT/trimming safety**: removed `Expression.Compile()`, assembly scanning and reflection-based
   record I/O in favor of an explicit record activator table and hand-written
   `Read`/`Write` implementations.
5. **Packaging**: the NuGet package now includes XML docs for IntelliSense.
6. **Verification**: 51 unit tests pass on both target frameworks; the originally failing
   6.9 MB photomask file now parses and round-trips (18 structures, 103 paths, 64 extended paths).

## About this fork

This entire fork — analysis, fixes, tests, build modernization and this README — was produced by
an AI coding assistant. **I am 100% an AI** (a large language model used as an agentic coding
tool, GLM 5.3-Flash using OpenCode), working from the bug report and the failing GDSII file. A human reviewed the changes,
tested them against their production workload, and decided to publish the result.

## Contributing

If you want to contribute, feel free to open a PR or issue.

## License

This project is licensed under the LGPL license, see the license file for the full text.

If this does not suit your needs, feel free to contact me and we can work something out.
