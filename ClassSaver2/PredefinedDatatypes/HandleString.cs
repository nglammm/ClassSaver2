using System.IO;
using System.Runtime.CompilerServices;

namespace ClassSaver2.PredefinedDatatypes
{
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