using System.IO;

namespace ClassSaver2.PredefinedDatatypes
{
    
    /// <typeparam name="T">The type the handler is handling.</typeparam>
    public interface IDefineDatatype<T>
    {
        void Write(BinaryWriter writer, T input, WriteContext context);
        void Read(BinaryReader reader, ref T output, ReadContext context);
    }
}