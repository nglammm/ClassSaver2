using System;

namespace ClassSaver2.Remake
{
    /// <summary>
    /// Marks this type is serialized by ClassSaver2.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializeAttribute : Attribute
    {
        public SerializeAttribute()
        {
        }
    }
}