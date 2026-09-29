using System.Collections.Generic;
using System.IO;

namespace ClassSaver2.PredefinedDatatypes
{
    [DefineDatatype(typeof(List<>))]
    public readonly partial struct HandleList<T, TElementHandler> : IDefineDatatype<List<T>>
        where TElementHandler : struct, IDefineDatatype<T> where T : new()
    {

        public void Write(BinaryWriter writer, List<T> obj, WriteContext context)
        {
            if (DefineTypeFunctions.CanSkipWrite(writer, obj, context)) return;
            var handler = default(TElementHandler);
            
            writer.Write(obj.Count);
            for (int i = 0; i < obj.Count; i++)
            {
                handler.Write(writer, obj[i], context);
            }
        }

        public void Read(BinaryReader reader, ref List<T> output, ReadContext context)
        {
            if (DefineTypeFunctions.CanSkipRead(reader, ref output, context)) return;
            var handler = default(TElementHandler);
            
            int size = reader.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                var to = new T(); // initialisation
                handler.Read(reader, ref to, context);
                output.Add(to);
            }
        }
    }
}