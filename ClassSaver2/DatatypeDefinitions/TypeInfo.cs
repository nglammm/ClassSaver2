using System;
using System.Collections.Generic;
using ClassSaver2.Remake;
using Microsoft.CodeAnalysis;

namespace ClassSaver2.DatatypeDefinitions
{
    /// <summary>
    /// Type info and has fields information.
    /// </summary>
    public class TypeInfo : TypeInfoNoFields, IEquatable<TypeInfo>
    {
        public List<FieldInfo> Fields { get; }

        public TypeInfo(ITypeSymbol symbol, string serializeFieldAttributeFullName, string nonSerializeAttributeFullName) : base(symbol)
        {
            // generate fields for me
            Fields = new List<FieldInfo>(2);
            foreach (var field in symbol.OriginalDefinition.GetMembers())
            {
                // 1. is it a valid field
                if (!(field is IFieldSymbol fieldSymbol) || fieldSymbol.IsImplicitlyDeclared) continue;
                if (fieldSymbol.AssociatedSymbol is IPropertySymbol) continue;
                if (fieldSymbol.IsConst || fieldSymbol.IsStatic) continue;
                
                // 2. is public or has the SerializeField tag
                bool serializeField = fieldSymbol.DeclaredAccessibility == Accessibility.Public;
                foreach (var attribute in fieldSymbol.GetAttributes())
                {
                    var attr = attribute.AttributeClass;
                    if (attr == null) continue;

                    var attrName = attr.ToFastDisplayString(false);

                    if (attrName == serializeFieldAttributeFullName)
                    {
                        serializeField = true;
                        continue;
                    }

                    if (attrName == nonSerializeAttributeFullName)
                    {
                        serializeField = false;
                    }
                }

                if (!serializeField) continue;
                Fields.Add(new FieldInfo(fieldSymbol));
            }
        }

        public bool Equals(TypeInfo other)
        {
            if (other == null) return false;
            
            return TypeFullName == other.TypeFullName 
                   && Helper.Equals(TypeArguments, other.TypeArguments, EqualityComparer<TypeArgumentInfo>.Default)
                   && Helper.Equals(Fields, other.Fields, EqualityComparer<FieldInfo>.Default);
        }

        public override bool Equals(object obj)
        {
            return obj is TypeInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TypeFullName != null ? TypeFullName.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ Helper.GetHashCode(TypeArguments);
                hashCode = (hashCode * 397) ^ Helper.GetHashCode(Fields);
                return hashCode;
            }
        }
    }
}