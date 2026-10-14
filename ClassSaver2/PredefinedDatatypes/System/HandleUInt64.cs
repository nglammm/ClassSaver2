using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly struct HandleUInt64 : IDefineDatatype<ulong>
{
    public void Write(BinaryWriter writer, ulong input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref ulong output, ReadContext context)
    {
        output = reader.ReadUInt64();
    }
}