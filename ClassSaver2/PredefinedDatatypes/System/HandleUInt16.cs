using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly struct HandleUInt16 : IDefineDatatype<ushort>
{
    public void Write(BinaryWriter writer, ushort input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref ushort output, ReadContext context)
    {
        output = reader.ReadUInt16();
    }
}