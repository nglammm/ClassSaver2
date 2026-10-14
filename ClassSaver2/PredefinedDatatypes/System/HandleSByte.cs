using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly partial struct HandleSByte : IDefineDatatype<sbyte>
{
    public void Write(BinaryWriter writer, sbyte input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref sbyte output, ReadContext context)
    {
        output = reader.ReadSByte();
    }
}