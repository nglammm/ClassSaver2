using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace ClassSaver2.Remake
{
    /// <summary>
    /// Global helper functions.
    /// </summary>
    public static class Helper
    {
        #region string manipulations
        
        public static string GetFullMetadataName(this INamedTypeSymbol symbol)
        {
            if (symbol.ContainingNamespace == null || symbol.ContainingNamespace.IsGlobalNamespace)
                return symbol.MetadataName;

            return $"{symbol.ContainingNamespace.ToDisplayString()}.{symbol.MetadataName}";
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLastName(string s, bool includeTypeArgs = false)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            ReadOnlySpan<char> span = s.AsSpan();

            int firstAngle = span.IndexOf('<');

            ReadOnlySpan<char> typePart = firstAngle >= 0 ? span.Slice(0, firstAngle) : span;

            int lastDot = typePart.LastIndexOf('.');
            int lastColon = typePart.LastIndexOf(':');
            int splitIndex = Math.Max(lastDot, lastColon);

            int nameStart = splitIndex >= 0 ? splitIndex + 1 : 0;

            if (includeTypeArgs)
            {
                return span.Slice(nameStart).ToString();
            }

            return typePart.Slice(nameStart).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StripGenericBrackets(string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName)) return string.Empty;

            int idx = typeFullName.IndexOf('<');
            return idx < 0 ? typeFullName : typeFullName.Substring(0, idx);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MakeUnbounded(string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName)) return string.Empty;

            int firstAngle = typeFullName.IndexOf('<');
            if (firstAngle == -1) return typeFullName;

            var sb = new StringBuilder(typeFullName.Length);
            int layer = 0;

            for (int i = 0; i < typeFullName.Length; i++)
            {
                char c = typeFullName[i];

                switch (c)
                {
                    case '<':
                        if (layer == 0)
                        {
                            sb.Append('<');
                        }
                        layer++;
                        break;

                    case '>':
                        layer--;
                        if (layer == 0)
                        {
                            sb.Append('>');
                        }
                        break;

                    case ',':
                        if (layer == 1)
                        {
                            sb.Append(',');
                        }
                        break;

                    default:
                        if (layer == 0)
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }

            return sb.ToString();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Sanitize(string typeFullName)
        {
            return typeFullName.Replace(":", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty);
            // TODO: FUTURE IMPLEMENT THIS SHIT
            
            /*
            // pattern
            // [Namespace].[EnclosingType].[TargetType][Arity].g.cs
            typeFullName = typeFullName.Replace("global::", string.Empty);

            var output = new StringBuilder(typeFullName);

            int firstAngle = typeFullName.IndexOf('<');
            if (firstAngle == -1) output.Append('');
            */
        }
        
        public static string ToFastDisplayString(this ITypeSymbol type, bool fullyQualified = true)
        {
            var sb = new StringBuilder(128);
            AppendType(sb, type, fullyQualified);
            return sb.ToString();
        }
        
        static void AppendType(StringBuilder sb, ITypeSymbol symbol, bool fullyQualified)
        {
            switch (symbol)
            {
                case IArrayTypeSymbol array:
                    AppendType(sb, array.ElementType, fullyQualified);
                    sb.Append('[').Append(',', array.Rank - 1).Append(']');
                    break;

                case IPointerTypeSymbol pointer:
                    AppendType(sb, pointer.PointedAtType, fullyQualified);
                    sb.Append('*');
                    break;

                case INamedTypeSymbol named:
                    if (named.ContainingType != null)
                    {
                        AppendType(sb, named.ContainingType, fullyQualified);
                        sb.Append('.');
                    }
                    else if (!named.ContainingNamespace.IsGlobalNamespace)
                    {
                        if (fullyQualified) sb.Append("global::");
                        sb.Append(named.ContainingNamespace.ToDisplayString()).Append('.');
                    }
                    else if (fullyQualified)
                    {
                        sb.Append("global::");
                    }

                    sb.Append(named.Name);

                    if (named.TypeArguments.Length > 0)
                    {
                        sb.Append('<');
                        for (int i = 0; i < named.TypeArguments.Length; i++)
                        {
                            if (i > 0) sb.Append(", ");
                            AppendType(sb, named.TypeArguments[i], fullyQualified);
                        }
                        sb.Append('>');
                    }
                    break;

                default:
                    sb.Append(symbol.Name);
                    break;
            }
        }

        public static void ToConstraintString(this ITypeParameterSymbol param, StringBuilder builder)
        {
            int startLength = builder.Length;
            builder.Append($"where {param.Name} : ");
            int clauseStartLength = builder.Length;

            if (param.HasUnmanagedTypeConstraint)
                builder.Append("unmanaged, ");
            else if (param.HasValueTypeConstraint)
                builder.Append("struct, ");
            else if (param.HasReferenceTypeConstraint)
                builder.Append(param.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "class?, " : "class, ");
            else if (param.HasNotNullConstraint)
                builder.Append("notnull, ");

            foreach (var constraintType in param.ConstraintTypes)
            {
                builder.Append(constraintType.ToFastDisplayString(true));
                builder.Append(", ");
            }

            if (param.HasConstructorConstraint)
            {
                builder.Append("new(), ");
            }

            if (builder.Length > clauseStartLength)
            {
                builder.Length -= 2;
            }
            else
            {
                builder.Length = startLength;
            }
        }
        
        public static bool HasParameterlessConstructor(this INamedTypeSymbol typeSymbol)
        {
            // Structs always have an implicit parameterless constructor
            if (typeSymbol.IsValueType)
            {
                return true;
            }

            // Abstract classes cannot be constructed directly
            if (typeSymbol.IsAbstract)
            {
                return false;
            }

            // For classes, look for an explicit instance constructor with 0 parameters
            return typeSymbol.InstanceConstructors.Length == 0 || typeSymbol.InstanceConstructors.Any(c => 
                c.Parameters.IsEmpty && 
                c.DeclaredAccessibility != Accessibility.Private);
        }

        #endregion
        
        #region equality and hashcode
        public static bool Equals<T>(IList<T> a, IList<T> b, EqualityComparer<T> comparer)
        {
            if (a == null || b == null) return a == b;
            if (a.Count != b.Count) return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (!comparer.Equals(a[i], b[i])) return false;
            }

            return true;
        }

        public static bool Equals<T>(T[] a, T[] b, EqualityComparer<T> comparer)
        {
            if (a == null || b == null) return a == b;
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (!comparer.Equals(a[i], b[i])) return false;
            }

            return true;
        }

        public static bool Equals<TKey, TValue>(IDictionary<TKey, TValue> a, IDictionary<TKey, TValue> b, EqualityComparer<TValue> valueComparer, Func<TValue, TValue, bool> checkBeforeComparison = null)
        {
            if (a == null || b == null) return a == b;
            if (a.Count != b.Count) return false;

            foreach (var pair in a)
            {
                var aValue = pair.Value;
                if (b.TryGetValue(pair.Key, out var bValue))
                {
                    if (checkBeforeComparison != null && !checkBeforeComparison.Invoke(aValue, bValue)) return false;
                    if (!valueComparer.Equals(aValue, bValue)) return false;
                }
                else return false;
            }

            return true;
        }

        public static int GetHashCode<T>(IList<T> list)
        {
            if (list == null) return 0;
            
            unchecked
            {
                int hash = 17;
                foreach (var item in list)
                {
                    hash = (hash * 31) ^ (item != null ? item.GetHashCode() : 0);
                }
                return hash;
            }
        }

        public static int GetHashCode<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return 0;

            unchecked
            {
                int hash = 67;
                foreach (var pair in dictionary)
                {
                    int pairHash = pair.Key.GetHashCode();
                    if (pair.Value != null) pairHash = (pairHash * 31) ^ pair.Value.GetHashCode();
                    
                    hash = (hash * 31) ^ pairHash;
                }

                return hash;
            }
        }
        #endregion
    }
}