namespace ClassSaver2.DatatypeDefinitions;

/// <summary>
/// The recognizable kinds ClassSaver2 can handle.
/// </summary>
public enum TypeInfoKind : byte
{
    Class,
    Struct,
    Enum,
    Array,
    Unknown
}