using System;
using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly partial struct HandleDatetime : IDefineDatatype<DateTime>
{
    public void Write(BinaryWriter writer, DateTime input, WriteContext context)
    {
        writer.Write(input.ToBinary());
    }

    public void Read(BinaryReader reader, ref DateTime output, ReadContext context)
    {
        output = DateTime.FromBinary(reader.ReadInt64());
    }
}