using System;
using System.Collections.Generic;
using System.IO;
using ClassSaver2.Remake;
using Microsoft.CodeAnalysis;

namespace ClassSaver2.PredefinedDatatypes
{
    public static class DefineTypeFunctions
    {
        private const string SerializeAttributeFullName = "ClassSaver2.Remake.SerializeAttribute";

        private static readonly HashSet<string> PredefinedMetadataNames = new HashSet<string>
        {
            // primitives
            "System.Int32",
            "System.Int64",
            "System.Double",
            "System.Single",
            "System.Boolean",
            "System.Byte",
            "System.SByte",
            "System.Char",
            "System.String",
            // collections
            "System.Collections.Generic.List`1",
        };

        internal static bool HasHandler(Compilation compilation, ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null) return false;
            if (!(typeSymbol is INamedTypeSymbol namedTypeSymbol))
            {
                return typeSymbol is ITypeParameterSymbol;
            }
            
            namedTypeSymbol = namedTypeSymbol.OriginalDefinition;
            
            if (PredefinedMetadataNames.Contains(namedTypeSymbol.GetFullMetadataName())) return true;
            
            // scan for type symbol attr
            foreach (var attr in namedTypeSymbol.GetAttributes())
            {
                if (attr.AttributeClass.ToFastDisplayString(false) == SerializeAttributeFullName) return true;
            }
            
            // scan for compilation
            var typeData = compilation.GetTypeByMetadataName("ClassSaver2.PredefinedDatatypes.Handle" + Helper.GetLastName(namedTypeSymbol.MetadataName));
            if (typeData == null) return false;

            foreach (var iFace in namedTypeSymbol.Interfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(iFace.TypeArguments[0] as INamedTypeSymbol, namedTypeSymbol)) return true;
            }
            
            return false;
        }
        
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
        public static bool CanSkipRead<T>(BinaryReader reader, ref T initialise, ReadContext context, bool createNewInitialiseIfNull) where T : new()
        {
            if (typeof(T).IsValueType)
            {
                if (createNewInitialiseIfNull) initialise = new T();
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
                    if (createNewInitialiseIfNull && initialise == null) initialise = new T();
                    context.Add(reader.ReadInt32(), initialise);
                    return false;
                default:
                    return false;
            }
        }

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