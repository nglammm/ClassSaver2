using Microsoft.CodeAnalysis;

namespace ClassSaver2.Debug
{
    public static class Diagnostics
    {
        // Rule 1: Class lacks a valid parameterless constructor or is abstract
        public static readonly DiagnosticDescriptor MissingParameterlessConstructorRule = new DiagnosticDescriptor(
            id: "CLASSSAVER0001",
            title: "Missing parameterless constructor",
            messageFormat: "The serializable type '{0}' must be a non-abstract class with a public or internal parameterless constructor (or a struct)",
            category: "ClassSaver",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        // Rule 2: Field type has no registered [DefineDatatype] handler
        public static readonly DiagnosticDescriptor UnsupportedFieldTypeRule = new DiagnosticDescriptor(
            id: "CLASSSAVER0002",
            title: "Unserializable field serialization type",
            messageFormat: "The field '{0}' on type '{1}' has an unsupported serialization type '{2}'. You should implement your own class to serialize.",
            category: "ClassSaver",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
        
        // Rule 3: No generic types inside a class
        public static readonly DiagnosticDescriptor NoTypeGenericsRule = new DiagnosticDescriptor(
            id: "CLASSSAVER0003",
            title: "No generic types given",
            messageFormat: "The field '{0}'s type '{1}' is a generic type but has no generic types given, which the serializer couldn't serialize",
            category: "ClassSaver",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
        
        // Rule 4: Unserializable type parameter as type argument
        public static readonly DiagnosticDescriptor UnsupportedTypeParameterRule = new DiagnosticDescriptor(
            id: "CLASSSAVER0004",
            title: "Unserializable type parameter",
            messageFormat: "The field '{0}'s type '{1}' has an unsupported type parameter '{2}'. You should implement your own class to serialize.",
            category: "ClassSaver",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }
}