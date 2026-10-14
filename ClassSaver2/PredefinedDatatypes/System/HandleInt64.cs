using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly partial struct HandleInt64 : IDefineDatatype<long>
{
    public void Write(BinaryWriter writer, long input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref long output, ReadContext context)
    {
        output = reader.ReadInt64();
    }
}