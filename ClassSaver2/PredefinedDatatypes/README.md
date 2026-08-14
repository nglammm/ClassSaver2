## Hello,

If you plan to make support for more datatypes, feel free to but be sure
that your class **must** follow the following implementation rules:

The class must:
- Be static,
- Be public,
- Has attribute `[DefineDatatype(Type)]`,
- Has strictly 2 **public** functions as follows:
  - `void Write(BinaryWriter, Type)`,
  - `Type Read(BinaryReader)`.

After the class implementation, add class to **Line 186** in `Main.cs`.

Formally, a class support for type `T` should look like this:

```csharp
using ClassSaver2.PredefinedDatatypes;
using System.IO;

[DefineDatatype(typeof(T))]
public static class HandleT
{
    public static void Write(BinaryWriter writer, T data)
    {
        // write logic
    }
    
    public static T Read(BinaryReader reader)
    {
        // read logic
    }
}
```

---

Example valid implementation for serializing *Int32* (`HandleInt.cs`):
```csharp
using System.IO;
using System.Runtime.CompilerServices;

namespace ClassSaver2.PredefinedDatatypes
{
    [DefineDatatype(typeof(int))]
    public static class HandleInt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(BinaryWriter binaryWriter, int value)
        {
            binaryWriter.Write(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Read(BinaryReader binaryReader)
        {
            return binaryReader.ReadInt32();
        }
    }
}
```

