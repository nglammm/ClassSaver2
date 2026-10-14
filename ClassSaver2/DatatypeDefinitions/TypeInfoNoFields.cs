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
        
        public bool IsArrayType { get; }
        
        public int Arity => IsArrayType ? 1 : (TypeArguments?.Length ?? 0);

        
        public TypeInfoNoFields(ITypeSymbol symbol)
        {
            if (!(symbol is INamedTypeSymbol namedTypeSymbol))
            {
                if (!(symbol is IArrayTypeSymbol arrayTypeSymbol))
                {
                    TypeArguments = Array.Empty<TypeArgumentInfo>();
                    TypeConstraintsAsString = string.Empty;
                    return;
                }

                var typeInfo = new TypeInfoNoFields(arrayTypeSymbol.ElementType);
                TypeFullName = typeInfo.TypeFullName;
                IsArrayType = true;
                TypeArguments = [new TypeArgumentInfo(typeInfo)];
                return;
            }

            TypeFullName = symbol.ToFastDisplayString(true);
            
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
        
        public bool Equals(TypeInfoNoFields other)
        {
            if (other == null) return false;
            return TypeFullName == other.TypeFullName && Helper.Equals(TypeArguments, other.TypeArguments, EqualityComparer<TypeArgumentInfo>.Default);
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
                hashCode = (hashCode * 397) ^ Helper.GetHashCode(TypeArguments); ;
                return hashCode;
            }
        }
    }
}