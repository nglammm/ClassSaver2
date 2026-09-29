using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ClassSaver2.DatatypeDefinitions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeInfo = ClassSaver2.DatatypeDefinitions.TypeInfo;

namespace ClassSaver2.Remake
{
    // TODO: THE FUNCTION ToDisplayString() IS SLOW, OPTIMIZE IF BOTTLENECK (future)
    [Generator]
    public class Fetcher : IIncrementalGenerator
    {
        private const string SerializeAttributeName = "ClassSaver2.Remake.SerializeAttribute";
        private const string NonSerializedAttributeName = "System.NonSerializedAttribute";
        private const string SerializableFieldAttributeName = "ClassSaver2.Remake.SerializableFieldAttribute";
        
        private static readonly SymbolDisplayFormat CanonicalTypeFormat = 
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions 
                & ~SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var serializable = FetchSerializable(context.SyntaxProvider).Collect();
            
            context.RegisterSourceOutput(serializable, (scp, values) =>
            {
                foreach (var value in values)
                {
                    var serializeCode = ScriptGenerator.GenerateCode(value);
                    scp.AddSource($"{Helper.Sanitize(value.TypeFullName)}.g.cs", serializeCode);
                }
            });
        }

        private IncrementalValuesProvider<TypeInfo> FetchSerializable(SyntaxValueProvider provider)
        {
            return provider.ForAttributeWithMetadataName(
                SerializeAttributeName,
                predicate: (node, _) => node is ClassDeclarationSyntax || node is StructDeclarationSyntax,
                transform: (ctx, _) =>
                {
                    // TODO: check if this symbol is a good symbol as it needs 0 constructors
                    
                    return new TypeInfo(ctx.TargetSymbol as ITypeSymbol, SerializableFieldAttributeName, NonSerializedAttributeName);
                });
        }
    }
}