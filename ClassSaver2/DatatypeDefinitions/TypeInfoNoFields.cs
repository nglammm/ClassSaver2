using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace ClassSaver2.DatatypeDefinitions
{
    /// <summary>
    /// Represents a <b>serializable</b> type info with no information on fields.
    /// </summary>
    public class TypeInfoNoFields : ITypeInfo, IEquatable<TypeInfoNoFields>
    {
        public string TypeFullName { get; }
        public TypeArgumentInfo[] TypeArguments { get; }
        public string TypeConstraintsAsString { get; }
        
        public TypeInfoKind Kind { get; }
        
        /// <summary>
        /// The underlying enum type, is an empty string if this is not an enum.
        /// </summary>
        public string UnderlyingEnumTypeFullName { get; }
        
        public bool IsArrayType => Kind == TypeInfoKind.Array;
        
        public int Arity => IsArrayType ? 1 : (TypeArguments?.Length ?? 0);

        
        public TypeInfoNoFields(ITypeSymbol symbol)
        {
            UnderlyingEnumTypeFullName = string.Empty;
            
            // handle other types
            if (!(symbol is INamedTypeSymbol namedTypeSymbol))
            {
                // 1. array type symbol
                if (symbol is IArrayTypeSymbol arrayTypeSymbol)
                {
                    var typeInfo = new TypeInfoNoFields(arrayTypeSymbol.ElementType);
                    TypeFullName = typeInfo.TypeFullName;
                    Kind = TypeInfoKind.Array;
                    TypeArguments = [new TypeArgumentInfo(typeInfo)];
                    return;
                }
                
                // 2. unknown
                TypeArguments = Array.Empty<TypeArgumentInfo>();
                TypeConstraintsAsString = string.Empty;
                Kind = TypeInfoKind.Unknown;
                return;
            }
            
            TypeFullName = symbol.ToFastDisplayString(true);
            
            // handle enums
            if (namedTypeSymbol.TypeKind == TypeKind.Enum)
            {
                UnderlyingEnumTypeFullName = namedTypeSymbol.EnumUnderlyingType.ToFastDisplayString(true);
                TypeArguments = Array.Empty<TypeArgumentInfo>();
                TypeConstraintsAsString = string.Empty;
                Kind = TypeInfoKind.Enum;
                return;
            }
            
            Kind = namedTypeSymbol.IsReferenceType ? TypeInfoKind.Class : TypeInfoKind.Struct;
            
            if (namedTypeSymbol.Arity == 0)
            {
                TypeArguments = Array.Empty<TypeArgumentInfo>();
                TypeConstraintsAsString = string.Empty;
                return;
            }
    
            TypeArguments = new TypeArgumentInfo[namedTypeSymbol.Arity];
            StringBuilder typeConstraintsBuilder = new StringBuilder(namedTypeSymbol.Arity * 3);
    
            for (int i = 0; i < namedTypeSymbol.Arity; i++)
            {
                var typeArgument = namedTypeSymbol.TypeArguments[i];
                if (typeArgument.TypeKind == TypeKind.TypeParameter)
                {
                    TypeArguments[i] = new TypeArgumentInfo(TypeFullName, typeArgument.Name);
                }
                else
                {
                    TypeArguments[i] = new TypeArgumentInfo(new TypeInfoNoFields(typeArgument));
                }
                
                // type parameter
                var typeParameter = namedTypeSymbol.TypeParameters[i];
                typeParameter.ToConstraintString(typeConstraintsBuilder);
                typeConstraintsBuilder.Append(' ');
            }
            
            TypeConstraintsAsString = typeConstraintsBuilder.ToString();
        }
        

        public override bool Equals(object obj)
        {
            return Equals(obj as TypeInfoNoFields);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TypeFullName != null ? TypeFullName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TypeArguments != null ? TypeArguments.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TypeConstraintsAsString != null ? TypeConstraintsAsString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Kind;
                hashCode = (hashCode * 397) ^ (UnderlyingEnumTypeFullName != null ? UnderlyingEnumTypeFullName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public bool Equals(TypeInfoNoFields other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return TypeFullName == other.TypeFullName && Equals(TypeArguments, other.TypeArguments) && TypeConstraintsAsString == other.TypeConstraintsAsString && Kind == other.Kind && UnderlyingEnumTypeFullName == other.UnderlyingEnumTypeFullName;
        }
    }
}