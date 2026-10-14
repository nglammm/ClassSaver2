using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public struct HandleUInt32 : IDefineDatatype<uint>
{
    public void Write(BinaryWriter writer, uint input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref uint output, ReadContext context)
    {
        output = reader.ReadUInt32();
    }
}