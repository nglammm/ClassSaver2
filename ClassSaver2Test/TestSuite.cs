using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClassSaver2;
using ClassSaver2.PredefinedDatatypes;
using MyGame.Entities;

namespace MyGame.Entities
{
    public interface IRandomizable : IEquatable<IRandomizable>
    {
        void RandomData(Random random);
    }

    [Serialize]
    public struct Vector2<T1, T2> : IRandomizable, IEquatable<Vector2<T1, T2>> where T1 : struct
    {
        public T1 X;
        public T2 Y;

        public void RandomData(Random random)
        {
            if (typeof(T1) == typeof(int)) X = (T1)(object)random.Next(-1000, 1000);
            if (typeof(T2) == typeof(int)) Y = (T2)(object)random.Next(-1000, 1000);
        }

        public override string ToString() => $"({X}, {Y})";

        public bool Equals(Vector2<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(X, other.X) &&
                   EqualityComparer<T2>.Default.Equals(Y, other.Y);
        }

        public bool Equals(IRandomizable other) => other is Vector2<T1, T2> vec && Equals(vec);
        public override bool Equals(object obj) => obj is Vector2<T1, T2> vec && Equals(vec);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + EqualityComparer<T1>.Default.GetHashCode(X);
                hash = hash * 31 + (Y != null ? EqualityComparer<T2>.Default.GetHashCode(Y) : 0);
                return hash;
            }
        }

        public static bool operator ==(Vector2<T1, T2> left, Vector2<T1, T2> right) => left.Equals(right);
        public static bool operator !=(Vector2<T1, T2> left, Vector2<T1, T2> right) => !left.Equals(right);
    }

    [Serialize]
    public class PlayerData2 : IRandomizable, IEquatable<PlayerData2>
    {
        public string PlayerName;
        public int Health;
        public int Score;
        public Vector2<int, int> Position;
        public string Tag;
        public int TemporarySessionToken = 999;

        public void RandomData(Random random)
        {
            PlayerName = "Player_" + random.Next(1000, 9999);
            Health = random.Next(1, 101);
            Score = random.Next(0, 50000);

            Position = new Vector2<int, int>();
            Position.RandomData(random);

            Tag = "Tag_" + random.Next(10, 99);
        }

        public bool Equals(PlayerData2 other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(PlayerName, other.PlayerName, StringComparison.Ordinal) &&
                   Health == other.Health &&
                   Score == other.Score &&
                   Position.Equals(other.Position) &&
                   string.Equals(Tag, other.Tag, StringComparison.Ordinal);
        }

        public bool Equals(IRandomizable other) => other is PlayerData2 data && Equals(data);
        public override bool Equals(object obj) => Equals(obj as PlayerData2);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (PlayerName != null ? StringComparer.Ordinal.GetHashCode(PlayerName) : 0);
                hash = hash * 31 + Health.GetHashCode();
                hash = hash * 31 + Score.GetHashCode();
                hash = hash * 31 + Position.GetHashCode();
                hash = hash * 31 + (Tag != null ? StringComparer.Ordinal.GetHashCode(Tag) : 0);
                return hash;
            }
        }
    }

    [Serialize]
    public class PlayerData : IRandomizable, IEquatable<PlayerData>
    {
        public string PlayerName;
        public int Health;
        public int Stat1;
        public int Stat2;
        public int Score;
        public int LevelOffset;

        public int[] Testing;
        public List<List<List<Vector2<int, int>>>> Positions;
        public Vector2<int, int> Position;
        public PlayerData2 Data2;

        [NonSerialized]
        public object IgnoredData;

        [NonSerialized]
        public PlayerData playerData;

        public PlayerData()
        {
            playerData = this;
            Testing = new int[3];
        }

        public void RandomData(Random random)
        {
            PlayerName = "Hero_" + random.Next(100, 999);
            Health = random.Next(1, 100);
            Stat1 = random.Next(0, 10);
            Stat2 = random.Next(100, 200);
            Score = random.Next(1000, 99999);
            LevelOffset = random.Next(-50, 50);

            Testing = new int[random.Next(2, 6)];
            for (int i = 0; i < Testing.Length; i++)
            {
                Testing[i] = random.Next(-500, 500);
            }

            Position = new Vector2<int, int>();
            Position.RandomData(random);

            int d1 = random.Next(1, 3);
            Positions = new List<List<List<Vector2<int, int>>>>(d1);
            for (int i = 0; i < d1; i++)
            {
                int d2 = random.Next(1, 3);
                var subList2 = new List<List<Vector2<int, int>>>(d2);
                for (int j = 0; j < d2; j++)
                {
                    int d3 = random.Next(1, 4);
                    var subList3 = new List<Vector2<int, int>>(d3);
                    for (int k = 0; k < d3; k++)
                    {
                        var vec = new Vector2<int, int>();
                        vec.RandomData(random);
                        subList3.Add(vec);
                    }
                    subList2.Add(subList3);
                }
                Positions.Add(subList2);
            }

            Data2 = new PlayerData2();
            Data2.RandomData(random);

            playerData = this;
        }

        public bool Equals(PlayerData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(PlayerName, other.PlayerName, StringComparison.Ordinal) &&
                   Health == other.Health &&
                   Stat1 == other.Stat1 &&
                   Stat2 == other.Stat2 &&
                   Score == other.Score &&
                   LevelOffset == other.LevelOffset &&
                   Position.Equals(other.Position) &&
                   Equals(Data2, other.Data2) &&
                   AreArraysEqual(Testing, other.Testing) &&
                   AreNestedPositionsEqual(Positions, other.Positions);
        }

        private static bool AreArraysEqual(int[] a, int[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        private static bool AreNestedPositionsEqual(
            List<List<List<Vector2<int, int>>>> a,
            List<List<List<Vector2<int, int>>>> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Count != b.Count) return false;

            for (int i = 0; i < a.Count; i++)
            {
                var subA = a[i];
                var subB = b[i];

                if (ReferenceEquals(subA, subB)) continue;
                if (subA == null || subB == null || subA.Count != subB.Count) return false;

                for (int j = 0; j < subA.Count; j++)
                {
                    var listA = subA[j];
                    var listB = subB[j];

                    if (ReferenceEquals(listA, listB)) continue;
                    if (listA == null || listB == null || listA.Count != listB.Count) return false;

                    for (int k = 0; k < listA.Count; k++)
                    {
                        if (!listA[k].Equals(listB[k])) return false;
                    }
                }
            }

            return true;
        }

        public bool Equals(IRandomizable other) => other is PlayerData data && Equals(data);
        public override bool Equals(object obj) => Equals(obj as PlayerData);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (PlayerName != null ? StringComparer.Ordinal.GetHashCode(PlayerName) : 0);
                hash = hash * 31 + Health.GetHashCode();
                hash = hash * 31 + Score.GetHashCode();
                hash = hash * 31 + Position.GetHashCode();
                return hash;
            }
        }
    }

    [Serialize]
    public class AllPredefinedTypesData : IRandomizable, IEquatable<AllPredefinedTypesData>
    {
        // System Primitives
        public byte ByteVal;
        public char CharVal;
        public short Int16Val;
        public int Int32Val;
        public long Int64Val;
        public ushort UInt16Val;
        public uint UInt32Val;
        public ulong UInt64Val;
        public float SingleVal;
        public double DoubleVal;
        public string StringVal;

        // System Collections
        public byte[] ByteArray;
        public List<int> IntList;
        public Dictionary<string, double> StringToDoubleDict;
        public Dictionary<int, List<string>> NestedDictWithList;

        public void RandomData(Random random)
        {
            var buf = new byte[1];
            random.NextBytes(buf);
            ByteVal = buf[0];

            CharVal = (char)random.Next('A', 'z' + 1);
            Int16Val = (short)random.Next(short.MinValue, short.MaxValue);
            Int32Val = random.Next();
            Int64Val = ((long)random.Next() << 32) | (long)random.Next();
            UInt16Val = (ushort)random.Next(0, ushort.MaxValue + 1);
            UInt32Val = (uint)random.Next();
            UInt64Val = ((ulong)(uint)random.Next() << 32) | (ulong)(uint)random.Next();
            SingleVal = (float)(random.NextDouble() * 2000.0 - 1000.0);
            DoubleVal = random.NextDouble() * 100000.0 - 50000.0;
            StringVal = "Str_" + random.Next(1000, 9999);

            int arrayLen = random.Next(2, 8);
            ByteArray = new byte[arrayLen];
            random.NextBytes(ByteArray);

            int listLen = random.Next(2, 8);
            IntList = new List<int>(listLen);
            for (int i = 0; i < listLen; i++) IntList.Add(random.Next(-5000, 5000));

            int dictLen = random.Next(2, 5);
            StringToDoubleDict = new Dictionary<string, double>(dictLen);
            for (int i = 0; i < dictLen; i++)
            {
                StringToDoubleDict[$"Key_{i}_{random.Next(100, 999)}"] = random.NextDouble() * 100.0;
            }

            int nestedDictLen = random.Next(2, 4);
            NestedDictWithList = new Dictionary<int, List<string>>(nestedDictLen);
            for (int i = 0; i < nestedDictLen; i++)
            {
                NestedDictWithList[random.Next(1, 10000)] = new List<string> { $"ItemA_{i}", $"ItemB_{i}" };
            }
        }

        public bool Equals(AllPredefinedTypesData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return ByteVal == other.ByteVal &&
                   CharVal == other.CharVal &&
                   Int16Val == other.Int16Val &&
                   Int32Val == other.Int32Val &&
                   Int64Val == other.Int64Val &&
                   UInt16Val == other.UInt16Val &&
                   UInt32Val == other.UInt32Val &&
                   UInt64Val == other.UInt64Val &&
                   Math.Abs(SingleVal - other.SingleVal) < 0.0001f &&
                   Math.Abs(DoubleVal - other.DoubleVal) < 0.000001 &&
                   string.Equals(StringVal, other.StringVal, StringComparison.Ordinal) &&
                   AreArraysEqual(ByteArray, other.ByteArray) &&
                   AreListsEqual(IntList, other.IntList) &&
                   AreDictsEqual(StringToDoubleDict, other.StringToDoubleDict) &&
                   AreNestedDictsEqual(NestedDictWithList, other.NestedDictWithList);
        }

        private static bool AreArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        private static bool AreListsEqual<T>(List<T> a, List<T> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Count != b.Count) return false;
            return a.SequenceEqual(b);
        }

        private static bool AreDictsEqual(Dictionary<string, double> a, Dictionary<string, double> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Count != b.Count) return false;
            foreach (var kvp in a)
            {
                if (!b.TryGetValue(kvp.Key, out var bVal) || Math.Abs(kvp.Value - bVal) > 0.000001)
                    return false;
            }
            return true;
        }

        private static bool AreNestedDictsEqual(Dictionary<int, List<string>> a, Dictionary<int, List<string>> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Count != b.Count) return false;
            foreach (var kvp in a)
            {
                if (!b.TryGetValue(kvp.Key, out var bList) || !AreListsEqual(kvp.Value, bList))
                    return false;
            }
            return true;
        }

        public bool Equals(IRandomizable other) => other is AllPredefinedTypesData data && Equals(data);
        public override bool Equals(object obj) => Equals(obj as AllPredefinedTypesData);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ByteVal.GetHashCode();
                hash = hash * 31 + CharVal.GetHashCode();
                hash = hash * 31 + Int32Val.GetHashCode();
                hash = hash * 31 + Int64Val.GetHashCode();
                hash = hash * 31 + DoubleVal.GetHashCode();
                return hash;
            }
        }
    }
}

namespace ClassSaver2Test
{
    internal class Program
    {
        public static void Main()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("    ClassSaver2 Serialization Suite     ");
            Console.WriteLine("========================================\n");

            // --- 1. PREDEFINED DATATYPES TESTS ---
            RunPredefinedTest("Types 01: All Types & Collections (Randomized)", () =>
            {
                var data = new AllPredefinedTypesData();
                data.RandomData(new Random(777));
                return data;
            });

            RunPredefinedTest("Types 02: Numeric Extreme Boundary Limits", () => new AllPredefinedTypesData
            {
                ByteVal = byte.MaxValue,
                CharVal = char.MaxValue,
                Int16Val = short.MinValue,
                Int32Val = int.MinValue,
                Int64Val = long.MinValue,
                UInt16Val = ushort.MaxValue,
                UInt32Val = uint.MaxValue,
                UInt64Val = ulong.MaxValue,
                SingleVal = float.MaxValue,
                DoubleVal = double.MinValue,
                StringVal = "Unicode \u2603 \uD83D\uDE80 Symbol #Test",
                ByteArray = new byte[] { byte.MinValue, 127, byte.MaxValue },
                IntList = new List<int> { int.MinValue, 0, int.MaxValue },
                StringToDoubleDict = new Dictionary<string, double>
                {
                    { "min", double.MinValue },
                    { "max", double.MaxValue },
                    { "epsilon", double.Epsilon }
                },
                NestedDictWithList = new Dictionary<int, List<string>>
                {
                    { int.MinValue, new List<string> { "min_key_entry" } },
                    { int.MaxValue, new List<string> { "max_key_entry" } }
                }
            });

            RunPredefinedTest("Types 03: Empty & Null Collections", () => new AllPredefinedTypesData
            {
                StringVal = null,
                ByteArray = null,
                IntList = new List<int>(),
                StringToDoubleDict = new Dictionary<string, double>(),
                NestedDictWithList = null
            });

            // --- 2. PLAYER DATA & ARRAY TESTS ---
            RunPlayerTest("Player 01: Full Random Hierarchy", () =>
            {
                var p = new PlayerData();
                p.RandomData(new Random(42));
                return p;
            });

            RunPlayerTest("Player 02: Array - Null Reference", () =>
            {
                var p = CreateBasePlayer();
                p.Testing = null;
                return p;
            });

            RunPlayerTest("Player 03: Array - Zero Length (Empty)", () =>
            {
                var p = CreateBasePlayer();
                p.Testing = Array.Empty<int>();
                return p;
            });

            RunPlayerTest("Player 04: Nested List - Completely Empty", () =>
            {
                var p = CreateBasePlayer();
                p.Positions = new List<List<List<Vector2<int, int>>>>();
                return p;
            });

            RunPlayerTest("Player 05: Data2 - Null Object", () =>
            {
                var p = CreateBasePlayer();
                p.Data2 = null;
                return p;
            });

            // --- 3. STRESS TEST ---
            RunStressTest(iterations: 50);
        }

        private static PlayerData CreateBasePlayer()
        {
            var p = new PlayerData();
            p.RandomData(new Random(101));
            return p;
        }

        private static void RunPredefinedTest(string name, Func<AllPredefinedTypesData> factory)
        {
            Console.Write($"[{name}] ... ");
            var original = factory();

            using var stream = new MemoryStream();
            var writeContext = new WriteContext();

            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                default(HandleAllPredefinedTypesData).Write(writer, original, writeContext);
            }

            stream.Position = 0;
            var deserialized = new AllPredefinedTypesData();
            var readContext = new ReadContext();

            using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                default(HandleAllPredefinedTypesData).Read(reader, ref deserialized, readContext);
            }

            PrintResult(original.Equals(deserialized));
        }

        private static void RunPlayerTest(string name, Func<PlayerData> factory)
        {
            Console.Write($"[{name}] ... ");
            var original = factory();

            using var stream = new MemoryStream();
            var writeContext = new WriteContext();

            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                default(HandlePlayerData).Write(writer, original, writeContext);
            }

            stream.Position = 0;
            var deserialized = new PlayerData();
            var readContext = new ReadContext();

            using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                default(HandlePlayerData).Read(reader, ref deserialized, readContext);
            }

            deserialized.playerData = deserialized;
            PrintResult(original.Equals(deserialized));
        }

        private static void RunStressTest(int iterations)
        {
            Console.Write($"\n[Stress Test ({iterations} randomized cycles)] ... ");

            for (int i = 0; i < iterations; i++)
            {
                var p = new PlayerData();
                p.RandomData(new Random(1000 + i));

                using var stream = new MemoryStream();
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
                {
                    default(HandlePlayerData).Write(writer, p, new WriteContext());
                }

                stream.Position = 0;
                var res = new PlayerData();
                using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
                {
                    default(HandlePlayerData).Read(reader, ref res, new ReadContext());
                }

                res.playerData = res;
                if (!p.Equals(res))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"FAILED on seed {1000 + i}");
                    Console.ResetColor();
                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"PASSED (All {iterations} completed)\n");
            Console.ResetColor();
        }

        private static void PrintResult(bool success)
        {
            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("PASSED");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FAILED");
            }
            Console.ResetColor();
        }
    }
}