using System.IO;
using System.Runtime.CompilerServices;
using ClassSaver2.Remake;

namespace ClassSaver2.PredefinedDatatypes
{
    [DefineDatatype(typeof(float))]
    public readonly struct HandleSingle : IDefineDatatype<float>
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