using Microsoft.CodeAnalysis;

namespace ClassSaver2.Generator
{
    public static class Diagnostics
    {
        // Rule 1: Class lacks a valid parameterless constructor or is abstract
        public static readonly DiagnosticDescriptor MissingParameterlessConstructorRule = new DiagnosticDescriptor(
            id: "CS2001",
            title: "Missing parameterless constructor",
            messageFormat: "The serializable type '{0}' must be a non-abstract class or struct with a public or internal parameterless constructor",
            category: "ClassSaver",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        // Rule 2: Field type has no registered [DefineDatatype] handler
        public static readonly DiagnosticDescriptor UnsupportedFieldTypeRule = new DiagnosticDescriptor(
            id: "CS2002",
            title: "Unsupported field serialization type",
            messageFormat: "The field '{0}' on type '{1}' has an unsupported serialization type '{2}'. Implement your own class to do so.",
            category: "ClassSaver",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }
}