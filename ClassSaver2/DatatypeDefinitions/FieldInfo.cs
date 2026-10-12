using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ClassSaver2.DatatypeDefinitions
{
    public readonly struct FieldInfo : IEquatable<FieldInfo>, ITypeInfo
    {
        public string FieldName { get; }
        public string FileLocation { get; }
        public int LineNumber { get; }
        public TypeArgumentInfo FieldType { get; }
        public string TypeFullName => FieldType.TypeFullName;

        public FieldInfo(IFieldSymbol symbol)
        {
            FieldName = symbol.Name;

            if (symbol.Type is ITypeParameterSymbol typeParameter)
            {
                FieldType = new TypeArgumentInfo(symbol.Type.ToFastDisplayString(), typeParameter.Name);
            }
            else
            {
                var typeInfo = new TypeInfoNoFields(symbol.Type);
                FieldType = new TypeArgumentInfo(typeInfo.TypeFullName, typeInfo);
            }
            
            var loc = symbol.Locations.FirstOrDefault(l => l.IsInSource);
            if (loc != null)
            {
                var lineSpan = loc.GetLineSpan();
                FileLocation = lineSpan.Path.Replace(@"\", @"\\");;
                LineNumber = lineSpan.StartLinePosition.Line + 1; // 0-based
            }
            else
            {
                FileLocation = string.Empty;
                LineNumber = 0;
            }
        }
            
        public bool Equals(FieldInfo other)
        {
            return FieldName == other.FieldName && FieldType.Equals(other.FieldType);
        }

        public override bool Equals(object obj)
        {
            return obj is FieldInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FieldName != null ? FieldName.GetHashCode() : 0) * 397) ^ FieldType.GetHashCode();
            }
        }
        
        public static bool operator ==(FieldInfo left, FieldInfo right) => left.Equals(right);
        public static bool operator !=(FieldInfo left, FieldInfo right) => !left.Equals(right);
    }
}