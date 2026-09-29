using System.IO;

namespace ClassSaver2.Remake
{
    public delegate void WriteAction<in T>(BinaryWriter writer, T value, WriteContext context = null);
    public delegate T ReadFunction<out T>(BinaryReader reader, ReadContext context = null);
}