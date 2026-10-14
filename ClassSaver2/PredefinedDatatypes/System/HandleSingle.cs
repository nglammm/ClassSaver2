using System.IO;
using System.Runtime.CompilerServices;

namespace ClassSaver2.PredefinedDatatypes
{
    public readonly partial struct HandleSingle : IDefineDatatype<float>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, float value, WriteContext context)
        {
            writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(BinaryReader binaryReader, ref float output, ReadContext context)
        {
            output = binaryReader.ReadSingle();
        }
    }
}