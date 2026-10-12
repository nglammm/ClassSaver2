using System.IO;
using System.Runtime.CompilerServices;

namespace ClassSaver2.PredefinedDatatypes
{
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