using System;
using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly partial struct HandleGuid : IDefineDatatype<Guid>
{
    public void Write(BinaryWriter writer, Guid input, WriteContext context)
    {
        writer.Write(input.ToByteArray());
    }

    public void Read(BinaryReader reader, ref Guid output, ReadContext context)
    {
        output = new Guid(reader.ReadBytes(16));
    }
}