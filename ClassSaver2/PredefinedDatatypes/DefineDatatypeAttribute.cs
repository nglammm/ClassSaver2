using System;

namespace ClassSaver2.PredefinedDatatypes
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public class DefineDatatypeAttribute : Attribute
    {
        public DefineDatatypeAttribute(Type type)
        {
        }
    }
}