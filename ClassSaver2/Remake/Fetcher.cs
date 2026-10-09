using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeInfo = ClassSaver2.DatatypeDefinitions.TypeInfo;

namespace ClassSaver2.Remake
{
    [Generator]
    public class Fetcher : IIncrementalGenerator
    {
        private const string SerializeAttributeName = "ClassSaver2.Remake.SerializeAttribute";
        private const string NonSerializedAttributeName = "System.NonSerializedAttribute";
        private const string SerializableFieldAttributeName = "ClassSaver2.Remake.SerializeFieldAttribute";
        
        private static readonly SymbolDisplayFormat CanonicalTypeFormat = 
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions 
                & ~SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var serializable = FetchSerializable(context.SyntaxProvider);
            
            context.RegisterSourceOutput(serializable, (scp, value) =>
            {
                var serializeCode = ScriptGenerator.GenerateCode(value);
                scp.AddSource($"{Helper.Sanitize(value.TypeFullName)}.g.cs", serializeCode);
            });
        }

        private IncrementalValuesProvider<TypeInfo> FetchSerializable(SyntaxValueProvider provider)
        {
            return provider.ForAttributeWithMetadataName(
                SerializeAttributeName,
                predicate: (node, _) => node is ClassDeclarationSyntax || node is StructDeclarationSyntax,
                transform: (ctx, _) =>
                {
                    if (!(ctx.TargetSymbol is INamedTypeSymbol typeSymbol)) return null;
                    if (!typeSymbol.HasParameterlessConstructor()) return null;
                    
                    return new TypeInfo(typeSymbol, ctx.SemanticModel.Compilation, NonSerializedAttributeName);
                }).Where(value => value != null);
        }
    }
}