using System.IO;

namespace ClassSaver2.PredefinedDatatypes
{
    public static class DefineTypeFunctions
    {
        /// <summary>
        /// Determines if the passed in data can skip writing.
        /// </summary>
        /// <param name="data">The data to check</param>
        /// <param name="writer">The binary writer</param>
        /// <param name="context">WriteContext that contains all the reference of written types</param>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <returns>True means it can be skipped, false means it needs to write</returns>
        public static bool CanSkipWrite<T>(BinaryWriter writer, T data, WriteContext context)
        {
            if (typeof(T).IsValueType) // value types aren't suitable here
            {
                return false;
            }
            
            // 1. if data is null
            if (data == null)
            {
                writer.Write((byte)0);
                return true;
            }
            
            // 2. if the reference of data already written
            if (context.TryGet(data, out int index))
            {
                writer.Write((byte)1);
                writer.Write(index);
                return true;
            }
            
            // assign it an id
            writer.Write((byte)2);
            writer.Write(context.Add(data));
            return false;
        }

        /// <summary>
        /// Determines if we could skip the write process.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="initialise"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool CanSkipRead<T>(BinaryReader reader, ref T initialise, ReadContext context)
        {
            if (typeof(T).IsValueType)
            {
                return false;
            }

            var byteCode = reader.ReadByte();
            switch (byteCode)
            {
                case 0:
                    // null byte
                    return true; // done
                case 1:
                    // id exists
                    initialise = (T)context.Get(reader.ReadInt32());
                    return true;
                case 2:
                    context.Add(reader.ReadInt32(), initialise);
                    return false;
                default:
                    return false;
            }
        }
    }
}