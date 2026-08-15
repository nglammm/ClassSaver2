using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ClassSaver2.Debug;

namespace ClassSaver2
{
    [Generator]
    public class Main : IIncrementalGenerator
    {
        // name constants
        private const string DefineDatatypeFullName = "ClassSaver2.PredefinedDatatypes.DefineDatatypeAttribute";
        private const string SerializableAttributeFullName = "global::System.SerializableAttribute";
        private const string SerializeMethodName = "System.SerializableAttribute";
        private const string NonSerializedAttributeFullName = "global::System.NonSerializedAttribute";
        
        private const string WriteContextFullName = "global::ClassSaver2.WriteContext";
        private const string ReadContextFullName = "global::ClassSaver2.ReadContext";
        
        private static readonly SymbolDisplayFormat CanonicalTypeFormat = 
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions 
                & ~SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

        private struct TypeHandler
        {
            public string TypeFullName;
            
            public FuncReadType FunctionReadType;
            public enum FuncReadType
            {
                /// <summary>
                /// This option is the ClassSaver.Read(BinaryReader, out Type, ReadContext) func where we pass the context in too
                /// </summary>
                ClassSaverItself,
                
                /// <summary>
                /// This is the Type Read(BinaryReader) that returns some value.
                /// </summary>
                ReturnType,
            }
            
            public FuncWriteType FunctionWriteType;
            public enum FuncWriteType
            {
                /// <summary>
                /// Inside ClassSaver.Write(BinaryWriter, Type, WriteContext)
                /// </summary>
                ClassSaverItself,
                
                /// <summary>
                /// External Write functions that has Write(BinaryWriter, Type)
                /// </summary>
                External
            }

            public TypeHandler(string typeFullName, FuncReadType functionReadType, FuncWriteType functionWriteType)
            {
                TypeFullName = typeFullName;
                FunctionWriteType = functionWriteType;
                FunctionReadType = functionReadType;
            }
        }
            
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif
            
            var predefinedHandlerDictProvider = FetchPredefinedClasses(context);
            var serializableClassesProvider = FetchSerializableClasses(context);
            
            // now merge the serialized classes into the dict
            var serializableTypeNamesProvider = serializableClassesProvider
                .Select((symbol, _) => symbol.ToDisplayString(CanonicalTypeFormat))
                .Collect();
            
            var handlersProvider = predefinedHandlerDictProvider
                .Combine(serializableTypeNamesProvider)
                .Select((tuple, _) =>
                {
                    var (predefined, serializableNames) = tuple;
            
                    var dict = new Dictionary<string, TypeHandler>(predefined);

                    // any [Serializable] type is handled directly by ClassSaver
                    foreach (var typeName in serializableNames)
                    {
                        dict[typeName] = new TypeHandler("global::ClassSaver2.ClassSaver", TypeHandler.FuncReadType.ClassSaverItself, TypeHandler.FuncWriteType.ClassSaverItself);
                    }

                    return dict;
                });
            
            var data = serializableClassesProvider.Combine(handlersProvider);
            
            context.RegisterSourceOutput(data, (scp, values) =>
            {
                // get everything out
                INamedTypeSymbol targetClass = values.Left;
                Dictionary<string, TypeHandler> handlerDict = values.Right;
                
                scp.AddSource($"ClassSaver_{targetClass}.g.cs", GenerateScriptSource(scp, targetClass, handlerDict));
            });
        }

        private static string GenerateScriptSource(SourceProductionContext context, INamedTypeSymbol classSymbol,
            Dictionary<string, TypeHandler> handlerDict)
        {
            // check if there are 0 initial constructors or not
            bool valid = classSymbol.IsValueType; // structs always have an overload for 0 constructor
            
            if (!valid)
            {
                foreach (var constructor in classSymbol.InstanceConstructors)
                {
                    if (constructor.Parameters.IsEmpty
                        && (constructor.DeclaredAccessibility == Accessibility.Public ||
                            constructor.DeclaredAccessibility == Accessibility.Internal))
                    {
                        valid = true;
                    }
                }
            }
            
            if (classSymbol.IsAbstract) valid = false;

            if (!valid)
            {
                Location classLocation = classSymbol.Locations[0];
        
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.MissingParameterlessConstructorRule,
                    classLocation,
                    classSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
                ));

                return string.Empty;
            }
            
            StringBuilder outputString = new StringBuilder();
            var classString = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            
            // first start segment
            outputString.Append($@"
/// auto generated script, any changes will be overwritten.
/// generated by ClassSaver2

using System;
using System.IO;
using System.Collections.Generic;

namespace ClassSaver2
{{
    public static partial class ClassSaver
    {{
        public static void Write(BinaryWriter writer, {classString} data, {WriteContextFullName} context = null)
        {{
            ");

            if (classSymbol.IsReferenceType)
            {
                outputString.Append($@"
            if (context == null)
            {{
                context = new {WriteContextFullName}();
            }}

            if (data is null)
            {{
                writer.Write((byte)0);
                return;
            }}

            if (context.TryGet(data, out int dataIndex))
            {{
                writer.Write((byte)1);
                writer.Write(dataIndex);
                return;
            }}
            
            writer.Write((byte)2);
            writer.Write(context.Add(data));

            ");
            }
            
            // write function
            Queue<(string VarName, string VarTypeFullName, TypeHandler Handler)> varOrder = new Queue<(string VarName, string VarTypeFullName, TypeHandler Handler)>();

            bool hasError = false;
            GetSerializableFields(classSymbol, (fieldSymbol) =>
            {
                // for every field, fetch type's name
                var varTypeName = fieldSymbol.Type.ToDisplayString(CanonicalTypeFormat);
                if (!handlerDict.TryGetValue(varTypeName, out var handlerData))
                {
                    Location fieldLocation = fieldSymbol.Locations[0];

                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.UnsupportedFieldTypeRule,
                        fieldLocation,
                        fieldSymbol.Name,
                        classSymbol.Name,
                        fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
                    ));

                    hasError = true;
                }

                if (hasError) return;
                
                // save the variable order
                varOrder.Enqueue((fieldSymbol.Name, varTypeName, new TypeHandler(handlerData.TypeFullName, handlerData.FunctionReadType, handlerData.FunctionWriteType)));

                if (handlerData.FunctionWriteType == TypeHandler.FuncWriteType.ClassSaverItself)
                {
                    outputString.Append($@"{handlerData.TypeFullName}.Write(writer, data.{fieldSymbol.Name}, context);
            ");
                }
                else
                {
                    // FuncWriteType.External
                    outputString.Append($@"{handlerData.TypeFullName}.Write(writer, data.{fieldSymbol.Name});
            ");
                }
            });

            if (hasError)
            {
                return string.Empty;
            }
            
            // read function
            outputString.Append($@"
        }}
        
        public static void Read(BinaryReader reader, out {classString} output, {ReadContextFullName} context = null)
        {{
            ");

            if (classSymbol.IsReferenceType)
            {
                outputString.Append($@"if (context == null)
            {{
                context = new {ReadContextFullName}();
            }}
            
            byte byteCode = reader.ReadByte();
            if (byteCode == (byte)0)
            {{
                output = null;
                return;
            }}
            
            if (byteCode == (byte)1)
            {{
                output = context.Get<{classString}>(reader.ReadInt32());
                return;
            }}

            int code = reader.ReadInt32();
            ");
            }

            outputString.Append($@"output = new {classString}();
            ");
            
            if (classSymbol.IsReferenceType)
            {
                outputString.Append($@"
            context.Add(code, output);
            ");
            }

            while (varOrder.Count > 0)
            {
                var varData = varOrder.Dequeue();

                if (varData.Handler.FunctionReadType == TypeHandler.FuncReadType.ReturnType)
                {
                    outputString.Append($@"output.{varData.VarName} = {varData.Handler.TypeFullName}.Read(reader);
            ");
                }
                else
                {
                    // TypeHandler.FuncReadType.ClassSaverItself
                    
                    var count = varOrder.Count; // this is used to name the variable
                    outputString.Append($@"{varData.Handler.TypeFullName}.Read(reader, out {varData.VarTypeFullName} temp_{count}, context);
            output.{varData.VarName} = temp_{count};
            ");
                }
            }
            
            // end
            outputString.Append($@"
        }}
    }}
}}");
            
            return outputString.ToString();
        }
            
        // <type's full name, handler's full name>
        private static IncrementalValueProvider<Dictionary<string, TypeHandler>> FetchPredefinedClasses(IncrementalGeneratorInitializationContext context)
        {
            var data =
                context.SyntaxProvider.ForAttributeWithMetadataName(
                    DefineDatatypeFullName,
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, _) => 
                    {
                        var handlerClassSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
                        var attr = ctx.Attributes[0];
                        var targetTypeSymbol = (ITypeSymbol)attr.ConstructorArguments[0].Value;
            
                        return (TargetType: targetTypeSymbol, HandlerClass: handlerClassSymbol);
                    }
                );

            var collected = data.Collect();
            
            var returnData = collected.Select((handlers, _) =>
            {
                var dict = new Dictionary<string, TypeHandler>();
                
                // add in prefills too
                dict["global::System.Int32"] = new TypeHandler("global::ClassSaver2.PredefinedDatatypes.HandleInt", TypeHandler.FuncReadType.ReturnType, TypeHandler.FuncWriteType.External);
                dict["global::System.String"] = new TypeHandler("global::ClassSaver2.PredefinedDatatypes.HandleString", TypeHandler.FuncReadType.ReturnType, TypeHandler.FuncWriteType.External);
                dict["global::System.Single"] = new TypeHandler("global::ClassSaver2.PredefinedDatatypes.HandleFloat", TypeHandler.FuncReadType.ReturnType,  TypeHandler.FuncWriteType.External);
                
                foreach (var h in handlers)
                {
                    dict[h.TargetType.ToDisplayString(CanonicalTypeFormat)] = new TypeHandler(h.HandlerClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), TypeHandler.FuncReadType.ReturnType,  TypeHandler.FuncWriteType.External);
                }

                return dict;
            });
            
            return returnData;
        }

        private static IncrementalValuesProvider<INamedTypeSymbol> FetchSerializableClasses(
            IncrementalGeneratorInitializationContext context)
        {
            return 
                context.SyntaxProvider.ForAttributeWithMetadataName(
                    SerializeMethodName,
                    predicate: (node, _) => node is ClassDeclarationSyntax || node is StructDeclarationSyntax,
                    transform: (ctx, _) => 
                    {
                        
                        var handlerClassSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
            
                        return handlerClassSymbol;
                    }
                );
        }

        private static void GetSerializableFields(INamedTypeSymbol classSymbol, Action<IFieldSymbol> setter)
        {
            foreach (ISymbol member in classSymbol.GetMembers())
            {
                if (!(member is IFieldSymbol field)) continue; // only do this for fields

                if (field.IsStatic || field.IsConst || field.IsImplicitlyDeclared)
                    continue;

                bool serializable = field.DeclaredAccessibility == Accessibility.Public;
                var attributes = field.GetAttributes();

                // scan through all of attributes name
                foreach (var attr in attributes)
                {
                    if (attr.AttributeClass == null) continue;
                    
                    var attrName = attr.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (attrName == NonSerializedAttributeFullName)
                    {
                        serializable = false;
                        break;
                    }

                    if (attrName != SerializableAttributeFullName) continue;

                    serializable = true;
                    break;
                }

                if (serializable)
                {
                    setter.Invoke(field);
                }
            }
        }
    }
}