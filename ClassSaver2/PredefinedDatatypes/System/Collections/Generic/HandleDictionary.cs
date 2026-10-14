using System.Collections.Generic;
using System.IO;

namespace ClassSaver2.PredefinedDatatypes;

public readonly struct HandleDictionary<TKey, TKey_Handler, TValue, TValue_Handler> : IDefineDatatype<Dictionary<TKey, TValue>>
    where TKey_Handler : struct, IDefineDatatype<TKey> where TValue_Handler : struct, IDefineDatatype<TValue>
{
    public void Write(BinaryWriter writer, Dictionary<TKey, TValue> input, WriteContext context)
    {
        if (DefineTypeFunctions.CanSkipWrite(writer, input, context)) return;
        
        var keyHandler = default(TKey_Handler);
        var valueHandler = default(TValue_Handler);
        
        writer.Write(input.Count);
        foreach (var item in input)
        {
            keyHandler.Write(writer, item.Key, context);
            valueHandler.Write(writer, item.Value, context);
        }
    }

    public void Read(BinaryReader reader, ref Dictionary<TKey, TValue> output, ReadContext context)
    {
        if (DefineTypeFunctions.CanSkipReadNoInitialise(reader, ref output, context)) return;
        
        int id = reader.ReadInt32(); // reference id
        
        int length = reader.ReadInt32();
        if (output == null) output = new Dictionary<TKey, TValue>(length);
        
        context.Add(id, output);
        
        var keyHandler = default(TKey_Handler);
        var valueHandler = default(TValue_Handler);

        for (int i = 0; i < length; i++)
        {
            var key = default(TKey);
            keyHandler.Read(reader, ref key, context);
            var value = default(TValue);
            valueHandler.Read(reader, ref value, context);

            output[key] = value;
        }
    }
}