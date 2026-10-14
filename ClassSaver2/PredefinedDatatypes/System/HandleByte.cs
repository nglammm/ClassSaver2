using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public struct HandleByte : IDefineDatatype<byte>
{
    public void Write(BinaryWriter writer, byte input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref byte output, ReadContext context)
    {
        output = reader.ReadByte();
    }
}