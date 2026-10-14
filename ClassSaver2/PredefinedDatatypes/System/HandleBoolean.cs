using System.IO;
using ClassSaver2;

namespace ClassSaver2.PredefinedDatatypes;

public readonly partial struct HandleBoolean : IDefineDatatype<bool>
{
    public void Write(BinaryWriter writer, bool input, WriteContext context)
    {
        writer.Write(input);
    }

    public void Read(BinaryReader reader, ref bool output, ReadContext context)
    {
        output = reader.ReadBoolean();
    }
}