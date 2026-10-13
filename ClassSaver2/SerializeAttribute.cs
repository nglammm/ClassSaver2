using System;

namespace ClassSaver2;

/// <summary>
/// Tells ClassSaver2 the desired class/struct to generate serialize/deserialize source code.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SerializeAttribute : Attribute
{
    public SerializeAttribute()
    {
    }
}