/*

using System;
using System.Collections.Generic;
using System.IO;
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
        
        // actions definition + their name constants
        public delegate void WriteClassSaverAction<TWriter, TData, TContext>(TWriter writer, TData data, TContext context) where TWriter : BinaryWriter where TContext : WriteContext;
        private const string WriteClassSaverActionFullName = "global::ClassSaver2.WriteClassSaverAction";
        public delegate void WriteDefaultAction<TWriter, TData>(TWriter writer, TData data) where TWriter : BinaryWriter;
        private const string WriteDefaultActionFullName = "global::ClassSaver2.WriteDefaultAction";
        
        public delegate void ReadClassSaverAction<TReader, TData, TContext>(TReader reader, out TData data, TContext context) where TReader : BinaryReader where TContext : ReadContext;
        private const string ReadClassSaverActionFullName = "global::ClassSaver2.ReadClassSaverAction";
        public delegate TData ReadDefaultAction<TReader, TData>(TReader reader) where TReader : BinaryReader;
        private const string ReadDefaultActionFullName = "global::ClassSaver2.ReadDefaultAction";
        
        private static readonly SymbolDisplayFormat CanonicalTypeFormat = 
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions 
                & ~SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );


        public struct TypeHandler
        {
            public string TypeFullName => Symbol.ToDisplayString(CanonicalTypeFormat);
            public ITypeSymbol Symbol;
            public int typeParameterCount;
            
            public bool IsGenericType => Symbol is INamedTypeSymbol named && named.IsGenericType;
            public INamedTypeSymbol NamedSymbol => Symbol as INamedTypeSymbol;

            public DefinitionType DefinitionType;

            public string GetFunctionWrite(params string[] parameters)
            {
                string parameterString = string.Join(", ", parameters);
                return $"{TypeFullName}.Write({parameterString})";
            }

            public string GetFunctionWriteWithGeneric(string[] genericType, params string[] parameters)
            {
                return $"{TypeFullName}.Write<{string.Join(", ", genericType)}>({string.Join(", ", parameters)})";
            }

            public string GetFunctionRead(params string[] parameters)
            {
                string parameterString = string.Join(", ", parameters);
                return $"{TypeFullName}.Read({parameterString})";
            }

            public string GetFunctionReadWithGeneric(string[] genericType, params string[] parameters)
            {
                return $"{TypeFullName}.Read<{string.Join(", ", genericType)}>({string.Join(", ", parameters)})";
            }

            public TypeHandler(int typeParameterCount, string typeFullName, Compilation compilation, DefinitionType definitionType)
            {
                Symbol = null;
                this.typeParameterCount = typeParameterCount;
                
                if (!string.IsNullOrEmpty(typeFullName)) Symbol = compilation.GetTypeByMetadataName(typeFullName.Replace("global::", string.Empty));
                
                DefinitionType = definitionType;
            }

            public static TypeHandler FromTypeSymbol(int typeParameterCount, ITypeSymbol symbol,  Compilation compilation, DefinitionType definitionType)
            {
                var output = new TypeHandler(typeParameterCount, string.Empty, compilation, definitionType);
                output.Symbol = symbol;
                
                return output;
            }
    }
            
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            Compilation compilationInstance = null;
            context.CompilationProvider.Select((compilation, _) => compilationInstance = compilation);
            
            var predefinedHandlerDictProvider = FetchPredefinedClasses(context, compilationInstance);
            var serializableClassesProvider = FetchSerializableClasses(context);
            
            var handlersProvider = predefinedHandlerDictProvider
                .Combine(serializableClassesProvider.Collect())
                .Select((tuple, _) =>
                {
                    var (predefined, serializableSymbols) = tuple;
            
                    var dict = new Dictionary<string, List<TypeHandler>>(predefined);

                    // any [Serializable] type is handled directly by ClassSaver
                    foreach (var typeSymbol in serializableSymbols)
                    {
                        var typeHandler = TypeHandler.FromTypeSymbol(typeSymbol.Arity, typeSymbol, compilationInstance, DefinitionType.ClassSaverItself);
                        var typeName = typeSymbol.ToDisplayString(CanonicalTypeFormat);
                        
                        if (!dict.TryGetValue(typeName, out var typeHandlers))
                        {
                            typeHandlers = new List<TypeHandler>();
                            dict.Add(typeName, typeHandlers);
                        }
                        typeHandlers.Add(typeHandler);
                    }

                    return dict;
                });
            
            var data = serializableClassesProvider.Combine(handlersProvider);
            
            context.RegisterSourceOutput(data, (scp, values) =>
            {
                // get everything out
                INamedTypeSymbol targetClass = values.Left;
                Dictionary<string, List<TypeHandler>> handlerDict = values.Right;
                
                scp.AddSource($"ClassSaver_{targetClass}.g.cs", GenerateScriptSource(scp, compilationInstance, targetClass, handlerDict));
            });
        }
            
        // <type's full name, handler's full name>
        private static IncrementalValueProvider<Dictionary<string, List<TypeHandler>>> FetchPredefinedClasses(IncrementalGeneratorInitializationContext context, Compilation compilation)
        {
            var data =
                context.SyntaxProvider.ForAttributeWithMetadataName(
                    DefineDatatypeFullName,
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, _) => 
                    {
                        var handlerClassSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
                        var attr = ctx.Attributes[0];
                        var args = attr.ConstructorArguments;
                        var targetTypeSymbol = args.Length > 0 ? args[0].Value as ITypeSymbol : null;
                        var definitionSymbol = args.Length > 1 && args[1].Value is int defVal ? defVal : 0;
                        var numTypeArgs = 0;

                        if (args.Length >= 3 && args[2].Value is int ctorVal)
                        {
                            numTypeArgs = ctorVal;
                        }
                        else
                        {
                            // Roslyn bound the named parameter to named args
                            for (int i = 0; i < attr.NamedArguments.Length; i++)
                            {
                                var arg = attr.NamedArguments[i];
                                if (arg.Key == "numberOfTypeParameters") numTypeArgs = (int)arg.Value.Value;
                            }
                        }

                        return (targetTypeSymbol, TypeHandler.FromTypeSymbol(numTypeArgs, handlerClassSymbol, compilation, (DefinitionType)definitionSymbol));
                    }
                );

            var collected = data.Collect();
            
            var returnData = collected.Select((handlers, _) =>
            {
                var dict = new Dictionary<string, List<TypeHandler>>();
                
                // add in prefills here
                dict["global::System.Int32"] = new List<TypeHandler>
                {
                    new TypeHandler(0, "global::ClassSaver2.PredefinedDatatypes.HandleInt", compilation, DefinitionType.Default)
                };

                dict["global::System.String"] = new List<TypeHandler>
                {
                    new TypeHandler(0, "global::ClassSaver2.PredefinedDatatypes.HandleString", compilation, DefinitionType.Default)
                };

                dict["global::System.Single"] = new List<TypeHandler>
                {
                    new TypeHandler(0, "global::ClassSaver2.PredefinedDatatypes.HandleFloat", compilation, DefinitionType.Default)
                };
                
                foreach (var h in handlers)
                {
                    string toAdd = h.Item1.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (!dict.TryGetValue(toAdd, out var list))
                    {
                        list = new List<TypeHandler>();
                        dict.Add(toAdd, list);
                    }
                    
                    list.Add(h.Item2);
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

        private static bool TryGetHandler(Dictionary<string, List<TypeHandler>> handlerDict, ITypeSymbol type, out TypeHandler typeHandler)
        {
            var typeName = type.ToDisplayString(CanonicalTypeFormat);
            if (handlerDict.TryGetValue(typeName,
                    out var handlers))
            {
                if (!(type is INamedTypeSymbol namedType))
                {
                    typeHandler = handlers[handlers.Count - 1]; // take the last one always
                    return true;
                }

                foreach (var item in handlers)
                {
                    if (item.NamedSymbol.TypeParameters.Length == namedType.TypeParameters.Length)
                    {
                        typeHandler = item;
                        return true;
                    }
                }

                typeHandler = default;
                return false;
            }
            
            // search for all interfaces
            foreach (var interfaceSymbol in type.AllInterfaces)
            {
                if (handlerDict.TryGetValue(interfaceSymbol.ToDisplayString(CanonicalTypeFormat),
                        out var handlerz))
                {
                    if (!(type is INamedTypeSymbol namedType))
                    {
                        typeHandler = handlerz[0];
                        return true;
                    }

                    foreach (var item in handlerz)
                    {
                        if (item.NamedSymbol.TypeParameters.Length == namedType.TypeParameters.Length)
                        {
                            typeHandler = item;
                            return true;
                        }
                    }
                }
            }
            
            typeHandler = default;
            return false;
        }

        private static void HandleGenericTypesWrite(int varId, StringBuilder outputCode, SourceProductionContext context, Dictionary<string, List<TypeHandler>> handlerDict, IFieldSymbol field)
        {
            var fieldType = field.Type;

            if (!(fieldType is INamedTypeSymbol fieldNamedType)) return;
            
            var genericTypes = fieldNamedType.TypeArguments;
            
            if (genericTypes.IsEmpty)
            {
                Location fieldLocation = field.Locations[0];
                            
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.NoTypeGenericsRule, 
                    fieldLocation,
                    field.Name,
                    fieldType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat))
                );
                return;
            }

            bool hasError = false;
            
            TypeHandler[] genericHandlers = new TypeHandler[genericTypes.Length];
            string[] handlerTypeSymbolFullNames = new string[genericTypes.Length];
            
            // error check if types are serializable
            for (int i = 0; i < genericTypes.Length; i++)
            {
                var typeSymbol = genericTypes[i] as ITypeSymbol;
                if (!TryGetHandler(handlerDict, typeSymbol, out var handlerType))
                {
                    Location fieldLocation = field.Locations[0];
                    
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.UnsupportedTypeParameterRule,
                        fieldLocation,
                        field.Name,
                        fieldType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                        typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat))
                    );
                    
                    hasError = true;
                }
                
                handlerTypeSymbolFullNames[i] = typeSymbol.ToDisplayString(CanonicalTypeFormat);
                genericHandlers[i] = handlerType;
            }

            if (hasError) return;
            
            if (!TryGetHandler(handlerDict, fieldType, out TypeHandler fieldTypeHandler))
            {
                Location fieldLocation = field.Locations[0];
                    
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.UnsupportedFieldTypeRule,
                    fieldLocation,
                    field.Name,
                    fieldType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat))
                );

                return;
            }

            
            
            // time to slap some code.
            // get the type of the argument of the shit we are dealing and the appropriate handler.
            for (int i = 0; i < genericHandlers.Length; i++)
            {
                
            }
            
            // write<T1, T2...>(writer, data, writecontext, processT1, processT2, ...)
            outputCode.Append($@"
                {fieldTypeHandler.GetFunctionWriteWithGeneric(handlerTypeSymbolFullNames, "writer", $"data.{field.Name}", "context",
                )}"); // process
        }

        private static string HandleGenericTypesRead(ITypeSymbol baseType, IFieldSymbol field)
        {
            
        }
    }
}
*/