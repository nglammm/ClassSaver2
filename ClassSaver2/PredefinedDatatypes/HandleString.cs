using System.IO;
using System.Runtime.CompilerServices;

namespace ClassSaver2.PredefinedDatatypes
{
    [DefineDatatype(typeof(string))]
    public static class HandleString
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(BinaryWriter binaryWriter, string value)
        {
            binaryWriter.Write(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Read(BinaryReader binaryReader)
        {
            return binaryReader.ReadString();
        }
    }
}