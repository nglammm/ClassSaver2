using System.IO;
using System.Runtime.CompilerServices;

namespace ClassSaver2.PredefinedDatatypes
{
    [DefineDatatype(typeof(float))]
    public static class HandleFloat
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(BinaryWriter writer, float value)
        {
            writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Read(BinaryReader binaryReader)
        {
            return binaryReader.ReadSingle();
        }
    }
}