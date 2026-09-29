using System;
using System.Collections.Generic;
using ClassSaver2.Remake;
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
        
        /// <summary>
        /// If a type has handler, it will be handled by ClassSaver2.PredefinedDatatypes.Handler{type display name},
        /// else it is handled by ClassSaver2.{type display name}
        /// </summary>
        public bool HasHandler { get; }
        public int Arity => TypeArguments?.Length ?? 0;

        
        public TypeInfoNoFields(ITypeSymbol symbol)
        {
            TypeFullName = symbol.ToFastDisplayString(true);
    
            if (!(symbol is INamedTypeSymbol namedTypeSymbol))
            {
                TypeArguments = Array.Empty<TypeArgumentInfo>();
                return;
            }

            HasHandler = namedTypeSymbol.HasHandler();
   
            if (namedTypeSymbol.Arity == 0)
            {
                TypeArguments = Array.Empty<TypeArgumentInfo>();
                return;
            }
    
            var typeArgs = new TypeArgumentInfo[namedTypeSymbol.Arity];
            TypeArguments = typeArgs;
    
            for (int i = 0; i < namedTypeSymbol.Arity; i++)
            {
                var typeArgument = namedTypeSymbol.TypeArguments[i];
                if (typeArgument.TypeKind == TypeKind.TypeParameter)
                {
                    typeArgs[i] = new TypeArgumentInfo(TypeFullName, typeArgument.Name);
                }
                else
                {
                    typeArgs[i] = new TypeArgumentInfo(TypeFullName, new TypeInfoNoFields(typeArgument), typeArgument.Name);
                }
            }
        }
        
        public bool Equals(TypeInfoNoFields other)
        {
            if (other == null) return false;
            return TypeFullName == other.TypeFullName && Helper.Equals(TypeArguments, other.TypeArguments, EqualityComparer<TypeArgumentInfo>.Default) && HasHandler == other.HasHandler;
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
                hashCode = (hashCode * 397) ^ Helper.GetHashCode(TypeArguments);
                hashCode = (hashCode * 397) ^ HasHandler.GetHashCode();
                return hashCode;
            }
        }
    }
}