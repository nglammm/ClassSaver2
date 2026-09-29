using System.IO;
using System.Runtime.CompilerServices;
using ClassSaver2.Remake;

namespace ClassSaver2.PredefinedDatatypes
{
    [DefineDatatype(typeof(string))]
    public readonly struct HandleString : IDefineDatatype<string>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter binaryWriter, string value, WriteContext context)
        {
            if (DefineTypeFunctions.CanSkipWrite(binaryWriter, value, context)) return;
            
            binaryWriter.Write(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(BinaryReader binaryReader, ref string output, ReadContext context)
        {
            if (DefineTypeFunctions.CanSkipRead(binaryReader, ref output, context)) return;
            
            output = binaryReader.ReadString();
        }
    }
}