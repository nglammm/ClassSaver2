namespace ClassSaver2
{
    /// <summary>
    /// Power of 4 means generic type.
    /// </summary>
    public enum DefinitionType : int
    {
        /// <summary>
        /// Inside ClassSaver.Write(BinaryWriter, Type, WriteContext)
        /// </summary>
        ClassSaverItself = 0,

        /// <summary>
        /// External Write functions that has Write(BinaryWriter, Type)
        /// </summary>
        Default = 1,

        /// <summary>
        /// Writes with classes with generic types
        /// </summary>
        GenericType = 2
    }
}