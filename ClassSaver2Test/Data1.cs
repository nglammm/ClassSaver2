using System;
using System.Collections.Generic;
using System.IO;
using ClassSaver2;
using ClassSaver2.PredefinedDatatypes;
using ClassSaver2.Remake;


namespace MyGame.Entities
{
    [Serialize]
    public class PlayerData
    {
        public string PlayerName;
        public int Health;
        public int nigger;
        public int abc;
        public int Score;

        public List<Vector2<int, int>> Positions;
        public Vector2<int, int> Position;
        public PlayerData2 Data2;
        public int Goddamn;
        public PlayerData playerData;
        public int a;

        // Custom datatype handled by [DefineDatatype]
        // public Math.Vector2 Position;

        // This field MUST be skipped by the generator
        public int TemporarySessionToken = 999;

        public PlayerData()
        {
            playerData = this;
        }
    }
    
    [Serialize]
    public class PlayerData2
    {
        public string PlayerName;
        public int Health;
        public int Score;
        public Vector2<int, int> Position;
        public string shit;

        // Custom datatype handled by [DefineDatatype]
        // public Math.Vector2 Position;

        // This field MUST be skipped by the generator
        public int TemporarySessionToken = 999;
    }
    
    [Serialize]
    public struct Vector2<T1, T2>
    {
        public T1 X;
        public T2 Y;
    }
}

// ============================================================================
// 3. TEST RUNNER
// ============================================================================
namespace ClassSaver2Test
{
    internal class Program
    {
        public static void Main()
        {
        }
    }
}