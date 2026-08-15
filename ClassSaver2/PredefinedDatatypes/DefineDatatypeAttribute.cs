using System;

namespace ClassSaver2.PredefinedDatatypes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DefineDatatypeAttribute : Attribute
    {
        public DefineDatatypeAttribute(Type type)
        {
        }
    }
}