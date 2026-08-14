using System.IO;
using System.Runtime.CompilerServices;

namespace ClassSaver2.PredefinedDatatypes
{
    [DefineDatatype(typeof(int))]
    public static class HandleInt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(BinaryWriter binaryWriter, int value)
        {
            binaryWriter.Write(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Read(BinaryReader binaryReader)
        {
            return binaryReader.ReadInt32();
        }
    }
}