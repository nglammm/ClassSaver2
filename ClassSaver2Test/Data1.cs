using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ClassSaver2;
using MyGame.Entities;
using ClassSaver2.PredefinedDatatypes;

namespace MyGame.Entities
{
    public interface IRandomizable : IEquatable<IRandomizable>
    {
        void RandomData(Random random);
    }

    [Serialize]
    public struct Vector2<T1, T2> : IRandomizable, IEquatable<Vector2<T1, T2>> where T1 : struct
    {
        [SerializeField]
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
        public string shit;

        public int TemporarySessionToken = 999;

    public void RandomData(Random random)
        {
            PlayerName = "Player_" + random.Next(1000, 9999);
            Health = random.Next(1, 101);
            Score = random.Next(0, 50000);

            Position = new Vector2<int, int>();
            Position.RandomData(random);

            shit = "Tag_" + random.Next(10, 99);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("PlayerData2 {");
            sb.AppendLine($"  PlayerName: \"{PlayerName}\"");
            sb.AppendLine($"  Health: {Health}");
            sb.AppendLine($"  Score: {Score}");
            sb.AppendLine($"  Position: {Position}");
            sb.AppendLine($"  shit: \"{shit}\"");
            sb.Append("}");
            return sb.ToString();
        }

        public bool Equals(PlayerData2 other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(PlayerName, other.PlayerName, StringComparison.Ordinal) &&
                   Health == other.Health &&
                   Score == other.Score &&
                   Position.Equals(other.Position) &&
                   string.Equals(shit, other.shit, StringComparison.Ordinal);
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
                hash = hash * 31 + (shit != null ? StringComparer.Ordinal.GetHashCode(shit) : 0);
                return hash;
            }
        }

        public static bool operator ==(PlayerData2 left, PlayerData2 right) => Equals(left, right);
        public static bool operator !=(PlayerData2 left, PlayerData2 right) => !Equals(left, right);
    }

    [Serialize]
    public class PlayerData : IRandomizable, IEquatable<PlayerData>
    {
        public string PlayerName;
        public int Health;
        public int nigger;
        public int abc;
        public int Score;

        public List<List<List<Vector2<int, int>>>> Positions;
        
        [NonSerialized]
        public List Shit;
        
        public Vector2<int, int> Position;
        public PlayerData2 Data2;
        public int Goddamn;
        public PlayerData playerData;

        private int TemporarySessionToken = 999;

        public PlayerData()
        { 
            playerData = this;
        }

        public void RandomData(Random random)
        {
            PlayerName = "Hero_" + random.Next(100, 999);
            Health = random.Next(1, 100);
            nigger = random.Next(0, 10);
            abc = random.Next(100, 200);
            Score = random.Next(1000, 99999);
            Goddamn = random.Next(-50, 50);

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

            Shit = new List();

            Data2 = new PlayerData2();
            Data2.RandomData(random);

            playerData = this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("PlayerData {");
            sb.AppendLine($"  PlayerName: \"{PlayerName}\"");
            sb.AppendLine($"  Health: {Health}");
            sb.AppendLine($"  nigger: {nigger}");
            sb.AppendLine($"  abc: {abc}");
            sb.AppendLine($"  Score: {Score}");
            sb.AppendLine($"  Goddamn: {Goddamn}");
            sb.AppendLine($"  Position: {Position}");
            
            // Nested 3D List formatting
            sb.AppendLine("  Positions: " + FormatNestedPositions(Positions));
            
            // Sub-object formatting
            if (Data2 != null)
            {
                sb.AppendLine("  Data2: " + Data2.ToString().Replace("\n", "\n  "));
            }
            else
            {
                sb.AppendLine("  Data2: null");
            }

            // Self-referential loop detection
            if (ReferenceEquals(playerData, this))
            {
                sb.AppendLine("  playerData: <this>");
            }
            else if (playerData != null)
            {
                sb.AppendLine($"  playerData: <Ref: {playerData.PlayerName}>");
            }
            else
            {
                sb.AppendLine("  playerData: null");
            }

            sb.Append("}");
            return sb.ToString();
        }

        private static string FormatNestedPositions(List<List<List<Vector2<int, int>>>> list)
        {
            if (list == null) return "null";
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var l2 = list[i];
                if (l2 == null) { sb.Append("null"); continue; }

                sb.Append("[");
                for (int j = 0; j < l2.Count; j++)
                {
                    if (j > 0) sb.Append(", ");
                    var l3 = l2[j];
                    if (l3 == null) { sb.Append("null"); continue; }

                    sb.Append("[");
                    for (int k = 0; k < l3.Count; k++)
                    {
                        if (k > 0) sb.Append(", ");
                        sb.Append(l3[k].ToString());
                    }
                    sb.Append("]");
                }
                sb.Append("]");
            }
            sb.Append("]");
            return sb.ToString();
        }

        public bool Equals(PlayerData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (!string.Equals(PlayerName, other.PlayerName, StringComparison.Ordinal) ||
                Health != other.Health ||
                nigger != other.nigger ||
                abc != other.abc ||
                Score != other.Score ||
                Goddamn != other.Goddamn ||
                !Position.Equals(other.Position) ||
                !Equals(Data2, other.Data2) ||
                !AreNestedPositionsEqual(Positions, other.Positions))
            {
                return false;
            }

            bool thisRefersSelf = ReferenceEquals(playerData, this);
            bool otherRefersSelf = ReferenceEquals(other.playerData, other);

            if (thisRefersSelf || otherRefersSelf)
            {
                return thisRefersSelf == otherRefersSelf;
            }

            return ReferenceEquals(playerData, other.playerData);
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
                hash = hash * 31 + nigger.GetHashCode();
                hash = hash * 31 + abc.GetHashCode();
                hash = hash * 31 + Score.GetHashCode();
                hash = hash * 31 + Goddamn.GetHashCode();
                hash = hash * 31 + Position.GetHashCode();
                hash = hash * 31 + (Data2 != null ? Data2.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(PlayerData left, PlayerData right) => Equals(left, right);
        public static bool operator !=(PlayerData left, PlayerData right) => !Equals(left, right);

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
    }

    public class List
    {
    }
}

namespace ClassSaver2Test
{
    internal class Program
    {
        public static void Main()
        {
            var rng = new Random(42);

            var player = new PlayerData();
            player.RandomData(rng);

            using var stream = new MemoryStream();

            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                default(HandlePlayerData).Write(writer, player);
            }

            stream.Position = 0;

            var playerEq = new PlayerData();
            using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                default(HandlePlayerData).Read(reader, ref playerEq);
            }

            bool isMatch = playerEq.Equals(player);
            Console.WriteLine($"Equals: {isMatch}\n");

            if (!isMatch)
            {
                Console.WriteLine("=== ORIGINAL ===");
                Console.WriteLine(player);
                Console.WriteLine("\n=== DESERIALIZED ===");
                Console.WriteLine(playerEq);
            }
        }
    }
}