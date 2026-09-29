using System;
using System.Collections.Generic;
using System.Text;
using ClassSaver2.DatatypeDefinitions;

namespace ClassSaver2.Remake
{
    /// <summary>
    /// Generates the script with given info.
    /// </summary>
    public static class ScriptGenerator
    {
        private const string WriteContextFullName = "global::ClassSaver2.WriteContext";
        private const string ReadContextFullName = "global::ClassSaver2.ReadContext";
        private const string SerializableClassFullName = "global::ClassSaver2.Internal.SerializableMap";
        private const string TypeInfoClassFullName = "global::ClassSaver2.Remake.TypeInfo";
        
        private static StringBuilder _outputCode;

        private static string _targetTypeDisplayName;

        private static void Initialise()
        {
            _outputCode = new StringBuilder(999);
            _targetTypeDisplayName = string.Empty;
        }

        public static string GenerateCode(TypeInfo targetType)
        {
            Initialise();
            
            _targetTypeDisplayName = Helper.GetLastName(targetType.TypeFullName);
            
            // initial func + write function
            _outputCode.Append($@"
/// Auto-generated script for
/// {targetType.TypeFullName}

using System;
using System.IO;
using ClassSaver2;
using ClassSaver2.Remake;
using ClassSaver2.PredefinedDatatypes;

namespace ClassSaver2.Serializable
{{
    public static partial class {_targetTypeDisplayName}
    {{
        ");
            
            GenCodeWrite(targetType);
            //GenCodeRead(targetType);

            _outputCode.Append($@"
    }}
}}");
            
            return _outputCode.ToString();
        }
        
        // type here guaranteed to be handled by ClassSaver.
        private static void GenCodeWrite(TypeInfo targetType)
        {
            if (targetType.Arity == 0)
            {
                // for no generic
                _outputCode.Append($@"
        public static void Write(BinaryWriter writer, {targetType.TypeFullName} input, WriteContext context = null)
        {{
            if (context == null) context = new WriteContext();
            if (DefineTypeFunctions.CanSkipWrite(writer, input, context)) return;
            
            ");
            }
            else
            {
                // starter code
                _outputCode.Append($@"
        public static void Write<");
                
                // scan for type arguments
                StringBuilder whereConditionBuilder = null;

                bool executed = false;
                
                for (int i = 0; i < targetType.Arity; i++)
                {
                    var typeArg = targetType.TypeArguments[i];
                    if (typeArg.HasFixedType) continue; // skip

                    if (!executed)
                    {
                        whereConditionBuilder = new StringBuilder(58);
                        executed = true;
                    }
                    
                    _outputCode.Append($"{typeArg.SymbolName}, THandler_{typeArg.SymbolName}, ");
                    whereConditionBuilder.Append(
                        $"where THandler_{typeArg.SymbolName} : struct, IDefineDatatype<{typeArg.SymbolName}> where {typeArg.SymbolName} : new() ");
                
                }

                if (!executed)
                {
                    _outputCode.Append($@"
        public static void Write(BinaryWriter writer, {targetType.TypeFullName} input, WriteContext context = null)
        {{
            if (context == null) context = new WriteContext();
            if (DefineTypeFunctions.CanSkipWrite(writer, input, context)) return;
            
            ");
                }

                else
                {
                    _outputCode.Remove(_outputCode.Length - 2, 2);
                    
                    _outputCode.Append(
                        $@">(BinaryWriter writer, {targetType.TypeFullName} input, WriteContext context = null) {whereConditionBuilder}
        {{
            if (context == null) context = new WriteContext();
            if (DefineTypeFunctions.CanSkipWrite(writer, input, context)) return;
            
            ");
                }
            }
            
            foreach (var fieldData in targetType.Fields)
            {
                GenCodeFieldWrite(fieldData);
                _outputCode.Append(@"
            ");
            }

            _outputCode.Append($@"
        }}
        ");
        }

        private static void GenCodeFieldWrite(FieldInfo fieldInfo)
        {
            var fieldType = fieldInfo.FieldType;
            if (!fieldType.HasFixedType)
            {
                _outputCode.Append($"default(THandler_{fieldType.SymbolName}).Write(writer, input.{fieldInfo.FieldName}, context);");
                return;
            }
            
            // it has fixed type
            var type = fieldType.TypeInfo;
            if (type.HasHandler)
            {
                _outputCode.Append($"default(");
                WriteHandlerCallToSB(_outputCode, type);
                _outputCode.Append($@").Write(writer, input.{fieldInfo.FieldName}, context);");
                return;
            }
            
            // it doesn't have a handler, meaning
            // handled by classsaver2.
            if (type.Arity == 0)
            {
                _outputCode.Append($"{Helper.GetLastName(type.TypeFullName)}.Write(writer, input.{fieldInfo.FieldName}, context);");
                return;
            }
            
            _outputCode.Append($"{Helper.GetLastName(type.TypeFullName)}.Write<");
            for (int i = 0; i < type.Arity; i++)
            {
                var typeArg = type.TypeArguments[i];
                if (i > 0)
                {
                    _outputCode.Append(", ");
                }
                if (typeArg.HasFixedType)
                {
                    WriteHandlerCallToSB(_outputCode, typeArg, true);
                }
                else
                {
                    _outputCode.Append($"{typeArg.SymbolName}, THandler_{typeArg.SymbolName}");
                }
            }

            _outputCode.Append($">(writer, input.{fieldInfo.FieldName}, context);");
        }

        #region handler calls
        private static void WriteHandlerCallToSB(StringBuilder sb, TypeInfoNoFields typeInfo)
        {
            if (typeInfo.Arity == 0)
            {
                sb.Append($"Handle{Helper.GetLastName(typeInfo.TypeFullName)}");
                return;
            }
            
            sb.Append($"Handle{Helper.GetLastName(typeInfo.TypeFullName)}<");
            
            for (int i = 0; i < typeInfo.Arity; i++)
            {
                var typeArg = typeInfo.TypeArguments[i];
                if (i > 0)
                    sb.Append(", ");
                WriteHandlerCallToSB(sb, typeArg, true);
            }
            
            sb.Append(">");
        }

        private static void WriteHandlerCallToSB(StringBuilder sb, TypeArgumentInfo targetType, bool inScope)
        {
            if (!targetType.HasFixedType)
            {
                if (inScope) sb.Append($"{targetType.SymbolName}, ");
                sb.Append($"THandler_{targetType.SymbolName}");
                return;
            }

            var type = targetType.TypeInfo;
            if (inScope) sb.Append($"{type.TypeFullName}, ");
                
            if (type.Arity == 0)
            {
                sb.Append($"Handle{Helper.GetLastName(type.TypeFullName)}");
                return;
            }
            
            sb.Append($"Handle{Helper.GetLastName(type.TypeFullName)}<");
            for (int i = 0; i < type.Arity; i++)
            {
                var typeArg = type.TypeArguments[i];
                if (i > 0)
                    sb.Append(", ");
                
                WriteHandlerCallToSB(sb, typeArg, true);
            }
            
            sb.Append(">");
        }
        #endregion
    }
}