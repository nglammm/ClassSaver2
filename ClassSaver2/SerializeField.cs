using System;

namespace ClassSaver2
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializeFieldAttribute : Attribute
    {
        public SerializeFieldAttribute()
        {
            
        }
    }
}