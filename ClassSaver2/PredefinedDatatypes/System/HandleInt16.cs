using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly struct HandleInt16 : IDefineDatatype<short>
{
    public void Write(BinaryWriter writer, short input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref short output, ReadContext context)
    {
        output = reader.ReadInt16();
    }
}