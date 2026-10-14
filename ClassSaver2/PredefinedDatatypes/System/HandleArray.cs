using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

/// <summary>
/// Fixed implementation for handling arrays T[].
/// </summary>
public readonly struct HandleArray<T, TElementHandler> where TElementHandler : struct, IDefineDatatype<T>
{
    public void Write(BinaryWriter writer, T[] value, WriteContext context)
    {
        if (DefineTypeFunctions.CanSkipWrite(writer, value, context)) return;
        
        writer.Write(value.Length);
        var handler = default(TElementHandler);
        foreach (var item in value)
        {
            handler.Write(writer, item, context);
        }
    }

    public void Read(BinaryReader reader, ref T[] value, ReadContext context)
    {
        if (DefineTypeFunctions.CanSkipReadNoInitialise(reader, ref value, context)) return;
        var id = reader.ReadInt32(); // reference id
        
        var handler = default(TElementHandler);
        var length = reader.ReadInt32();
        
        if (value == null || value.Length != length) value = new T[length];
        context.Add(id, value);

        for (int i = 0; i < length; i++)
        {
            handler.Read(reader, ref value[i], context);
        }
    }
}