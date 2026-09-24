using GdsSharp.Lib.Binary;

namespace GdsSharp.Lib.Terminals.Abstractions;

public abstract class GenericGdsRecord<T> : IGdsSimpleRead, IGdsSimpleWrite
{
    public virtual T Value { get; set; } = default!;

    public abstract ushort Code { get; }

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        Value = typeof(T) switch
        {
            _ when typeof(T) == typeof(short) => (T)(object)reader.ReadInt16(),
            _ when typeof(T) == typeof(int) => (T)(object)reader.ReadInt32(),
            _ when typeof(T) == typeof(double) => (T)(object)reader.ReadDouble(),
            _ when typeof(T) == typeof(string) => (T)(object)reader.ReadAsciiString(header.NumToRead),
            _ => throw new ArgumentOutOfRangeException(nameof(T), typeof(T), $"Cannot read type '{typeof(T)}'")
        };
    }

    public void Write(GdsBinaryWriter writer)
    {
        switch (Value)
        {
            case short s:
                writer.Write(s);
                break;
            case int i:
                writer.Write(i);
                break;
            case double d:
                writer.Write(d);
                break;
            case string s:
                writer.Write(s);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(T), typeof(T), $"Cannot write type '{typeof(T)}'");
        }
    }

    public virtual ushort GetLength()
    {
        return Value switch
        {
            double => 8,
            ushort => 2,
            short => 2,
            uint => 4,
            int => 4,
            ulong => 8,
            long => 8,
            string s => (ushort)(s.Length % 2 == 0 ? s.Length : s.Length + 1),
            _ => throw new ArgumentOutOfRangeException(nameof(T), $"Cannot get size of type '{typeof(T)}'")
        };
    }
}
