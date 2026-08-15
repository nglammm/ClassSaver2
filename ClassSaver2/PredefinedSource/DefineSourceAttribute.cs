using System;

namespace ClassSaver2.PredefinedSource
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DefineSourceAttribute : Attribute
    {
        public DefineSourceAttribute(Type type) {}
    }
}