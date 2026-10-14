using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly partial struct HandleDecimal : IDefineDatatype<decimal>
{
    public void Write(BinaryWriter writer, decimal input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref decimal output, ReadContext context)
    {
        output = reader.ReadSByte();
    }
}