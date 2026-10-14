using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly partial struct HandleDouble : IDefineDatatype<double>
{
    public void Write(BinaryWriter writer, double input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref double output, ReadContext context)
    {
        output = reader.ReadDouble();
    }
}