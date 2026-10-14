using System.Text;
using ClassSaver2.DatatypeDefinitions;

namespace ClassSaver2
{
    /// <summary>
    /// Generates the script with given info.
    /// </summary>
    public static class ScriptGenerator
    {
        private static string _targetTypeDisplayName;
        
        public static string GenerateCode(TypeInfo targetType, StringBuilder outputCode = null)
        {
            if (outputCode == null) outputCode = new StringBuilder(999);
                
             _targetTypeDisplayName = Helper.GetLastName(targetType.TypeFullName);
            
            // initial func + write function
            outputCode.Append($@"
/// Auto-generated script for
/// {targetType.TypeFullName}

using System;
using System.IO;
using ClassSaver2;

namespace ClassSaver2.PredefinedDatatypes
{{
    public readonly partial struct Handle{_targetTypeDisplayName}");
            
            // write the interfaces
            if (targetType.Arity == 0)
            {
                outputCode.Append($@" : IDefineDatatype<{targetType.TypeFullName}>
    {{
        ");
            }
            else
            {
                outputCode.Append($@"<");
                
                StringBuilder whereConditionBuilder = new StringBuilder(47 * targetType.Arity);
                StringBuilder paramBuilder = new StringBuilder(3 * targetType.Arity);
                
                for (int i = 0; i < targetType.Arity; i++)
                {
                    // all type args in TypeInfo has SymbolName.
                    var typeArg = targetType.TypeArguments[i];
                    
                    outputCode.Append($"{typeArg.SymbolName}, THandler_{typeArg.SymbolName}, ");
                    whereConditionBuilder.Append(
                        $"where THandler_{typeArg.SymbolName} : struct, IDefineDatatype<{typeArg.SymbolName}> ");
                    paramBuilder.Append($"{typeArg.SymbolName}, ");
                
                }
                
                outputCode.Remove(outputCode.Length - 2, 2);
                paramBuilder.Remove(paramBuilder.Length - 2, 2);
                outputCode.Append($@"> : IDefineDatatype<{targetType.TypeFullName}> {whereConditionBuilder} {targetType.TypeConstraintsAsString}
    {{
        ");
            }
            
            GenCodeWrite(targetType, outputCode);
            GenCodeRead(targetType, outputCode);

            outputCode.Append($@"
    }}
}}");
            
            return outputCode.ToString();
        }
        
        // type here guaranteed to be handled by ClassSaver.
        private static void GenCodeWrite(TypeInfo targetType, StringBuilder outputCode)
        {
            // starter code:
            // if (context == null) context = new WriteContext();
            // if (DefineTypeFunctions.CanSkipWrite(writer, input, context)) return;
            outputCode.Append($@"public void Write(BinaryWriter writer, {targetType.TypeFullName} input, WriteContext context = null)
        {{
            if (context == null) context = new WriteContext();
            if (DefineTypeFunctions.CanSkipWrite(writer, input, context)) return;
        
            ");
            
            foreach (var fieldData in targetType.Fields)
            {
                GenCodeFieldWrite(fieldData, outputCode);
                outputCode.Append(@"
            ");
            }

            outputCode.Append($@"#line default
        }}
        ");
        }

        private static void GenCodeFieldWrite(FieldInfo fieldInfo, StringBuilder outputCode)
        {
            outputCode.Append(@$"#line {fieldInfo.LineNumber.ToString()} ""{fieldInfo.FileLocation}""
            ");
            
            var fieldType = fieldInfo.FieldType;
            if (!fieldType.HasFixedType)
            {
                outputCode.Append($"default(THandler_{fieldType.SymbolName}).Write(writer, input.{fieldInfo.FieldName}, context);");
                return;
            }
            
            // it has fixed type
            var type = fieldType.TypeInfo;
 
            outputCode.Append($"default(");
            WriteHandlerCallToSB(outputCode, type);
            outputCode.Append($@").Write(writer, input.{fieldInfo.FieldName}, context);");
        }

        private static void GenCodeRead(TypeInfo targetType, StringBuilder outputCode)
        {
            outputCode.Append($@"public void Read(BinaryReader reader, ref {targetType.TypeFullName} input, ReadContext context = null)
        {{
            if (context == null) context = new ReadContext();
            if (DefineTypeFunctions.CanSkipRead(reader, ref input, context, true)) return;
            
            ");
            
            foreach (var fieldData in targetType.Fields)
            {
                GenCodeFieldRead(fieldData, outputCode);
                outputCode.Append(@"
            ");
            }
            
            outputCode.Append($@"#line default
        }}
        ");
        }

        private static void GenCodeFieldRead(FieldInfo fieldInfo, StringBuilder outputCode)
        {
            outputCode.Append(@$"#line {fieldInfo.LineNumber.ToString()} ""{fieldInfo.FileLocation}""
            ");
            
            var fieldType = fieldInfo.FieldType;
            if (!fieldType.HasFixedType)
            {
                outputCode.Append(
                    $"default(THandler_{fieldType.SymbolName}).Read(reader, ref input.{fieldInfo.FieldName}, context);");
                return;
            }
            
            var type = fieldType.TypeInfo;
            outputCode.Append("default(");
            WriteHandlerCallToSB(outputCode, type);
            outputCode.Append($").Read(reader, ref input.{fieldInfo.FieldName}, context);");
        }

        #region handler calls
        private static void WriteHandlerCallToSB(StringBuilder sb, TypeInfoNoFields typeInfo)
        {
            if (typeInfo.IsArrayType)
            {
                sb.Append("HandleArray<");
                WriteHandlerCallToSB(sb, typeInfo.TypeArguments[0], true);
                sb.Append(">");
                return;
            }

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