using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly partial struct HandleChar : IDefineDatatype<char>
{
    public void Write(BinaryWriter writer, char input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref char output, ReadContext context)
    {
        output = reader.ReadChar();
    }
}