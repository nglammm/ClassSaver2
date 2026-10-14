using System.IO;
using System.Runtime.CompilerServices;

namespace ClassSaver2.PredefinedDatatypes
{
    public readonly partial struct HandleString : IDefineDatatype<string>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter binaryWriter, string value, WriteContext context)
        {
            if (value == null)
            {
                binaryWriter.Write((byte)0);
                return;
            }
            
            binaryWriter.Write((byte)1);
            binaryWriter.Write(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(BinaryReader binaryReader, ref string output, ReadContext context)
        {
            var nullByte = binaryReader.ReadByte();
            if (nullByte == 0)
            {
                output = null;
                return;
            }
            output = binaryReader.ReadString();
        }
    }
}