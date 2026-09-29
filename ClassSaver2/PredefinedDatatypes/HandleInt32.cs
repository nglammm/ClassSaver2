using System.IO;
using System.Runtime.CompilerServices;
using ClassSaver2.Remake;

namespace ClassSaver2.PredefinedDatatypes
{
    [DefineDatatype(typeof(int))]
    public readonly struct HandleInt32 : IDefineDatatype<int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter binaryWriter, int value, WriteContext context)
        {
            binaryWriter.Write(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(BinaryReader binaryReader, ref int output, ReadContext context)
        {
            output = binaryReader.ReadInt32();
        }
    }
}