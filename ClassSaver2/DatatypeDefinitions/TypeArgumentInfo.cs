using System;

namespace ClassSaver2.DatatypeDefinitions
{
    /// <summary>
    /// Represents possibly a TypeInfo or a type parameter. It can also have both.
    /// </summary>
    public readonly struct TypeArgumentInfo : IEquatable<TypeArgumentInfo>, ITypeInfo
    {
        public string TypeFullName { get; }
        public TypeInfoNoFields TypeInfo { get; }
        public string SymbolName { get; }
        
        public bool HasFixedType => TypeInfo != null;
        public bool HasSymbolName => !string.IsNullOrEmpty(SymbolName);

        public TypeArgumentInfo(string typeFullName, string symbolName)
        {
            TypeFullName = typeFullName;
            SymbolName = symbolName;
            TypeInfo = null;
        }

        public TypeArgumentInfo(string typeFullName, TypeInfoNoFields typeInfo, string symbolName = "")
        {
            TypeFullName = typeFullName;
            TypeInfo = typeInfo;
            SymbolName = symbolName;
        }
        
        #region equality checks
        public bool Equals(TypeArgumentInfo other)
        {
            return other.TypeFullName == TypeFullName && other.SymbolName == SymbolName && Equals(TypeInfo, other.TypeInfo);
        }

        public override bool Equals(object obj)
        {
            return obj is TypeArgumentInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TypeFullName != null ? TypeFullName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TypeInfo != null ? TypeInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SymbolName != null ? SymbolName.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion
    }
}