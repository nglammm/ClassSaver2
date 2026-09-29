using System;

namespace ClassSaver2.Remake
{
    /// <summary>
    /// Forces ClassSaver2 to serialize a non-public field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializeFieldAttribute : Attribute
    {
        public SerializeFieldAttribute()
        {
            
        }
    }
}