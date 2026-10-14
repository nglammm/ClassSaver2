# ClassSaver 2

A simple Roslyn code generator at compile-time that generates serializer/deserializer code that serialize/deserialize C# objects into/from binary format.

What it does is it takes all the public fields (ignoring the ones with `[System.NonSerialized]` attribute) and generate a code to serialize all of its field type.

This is the newer version of the old [ClassSaver](https://github.com/nglammm/ClassSaver).

# Quick start

More details are in the [project's wiki](https://github.com/nglammm/ClassSaver2/wiki).

## Installation

Available installation methods are:
- [nuget](https://www.nuget.org/packages/ClassSaver2)
- from source

Installing via nuget is straight forward, as you follow the instructions provided by nuget ([link to get](https://www.nuget.org/packages/ClassSaver2)).

If you wanted to install from source, [clone your desired version](https://github.com/nglammm/ClassSaver2/releases) and reference `ClassSaver2` in your project:

`.csproj`:
```
<ItemGroup>
  <ProjectReference Include="{PATH}/ClassSaver2.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
...
```

*Replace `PATH` with the path where you cloned `ClassSaver2`.*


## Serializing (Writing)

You need to use the `[ClassSaver2.Serialize]` attribute to your desired class/struct so that *ClassSaver* knows which type to write the serialization/deserialization code to.

```csharp
[ClassSaver2.Serialize] // use the attribute
public struct Vector3
{
  public int x;
  public int y;
  public int z;

  public Vector3(int x, int y, int z)
  {
    this.x = x;
    this.y = y;
    this.z = z;
  }
}
```

To call for serialize, you follow the pattern:
```
default(ClassSaver2.Handle{CLASS_NAME}).Write(BinaryWriter, {CLASS_TYPE})
``` 

Where `CLASS_TYPE` is the type keyword of your serializing class/struct and `CLASS_NAME` is `nameof(CLASS_TYPE)` (aka name of `CLASS_TYPE`):

```csharp
  var serialize = new Vector3(1, 2, 3);
  default(ClassSaver2.HandleVector3).Write(writer, serialize);
```

### Handling generic types

Generics types follow a similar pattern compared to non-generics but you need to supply extra information to the write function.

Given an example class with one type generic:

```csharp
using ClassSaver2;

[Serialize]
public class Number<T>
{
  public T value;
  public Number(T val)
  {
    value = val;
  }
}
```

The caller pattern follows:

```csharp
  var serialize = new Number<int>(67);
  default(ClassSaver2.HandleNumber<int, HandleInt32>).Write(writer, serialize)
```

For types with two type generics or above:

```csharp
using ClassSaver2;

[Serialize]
public class Numbers<T1, T2>
{
  public T1 value1;
  public T2 value2;

  public Numbers(T1 val1, T2 val2)
  {
    value1 = val1;
    value2 = val2;
  }
}
```

It follows this pattern:

```csharp
  var serialize = new Numbers<int, float>(3, 6.7f);
  default(ClassSaver2.HandleNumbers<int, HandleInt32, float, HandleSingle>).Write(writer, serialize);
```

Similarly for more type generics it follows like that:

```csharp
default(ClassSaver2.Handle{CLASS_NAME}<T1, HandleT1, T2, HandleT2, T3, HandleT3, ...>).Write(writer, data);
```

More information and constriants in the [wiki](https://github.com/nglammm/ClassSaver2/wiki).

## Deserializing (Reading)

> Make sure the desired class/struct has the [ClassSaver2.Serialize] attribute.

The read function follows the same pattern as the write function:

```csharp
default(ClassSaver2.Handle{CLASS_NAME}).Read(BinaryReader, {CLASS_TYPE})
```

And same way of handling type generics from the write method.

