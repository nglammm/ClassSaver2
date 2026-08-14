using System;
using System.IO;
using ClassSaver2;


namespace MyGame.Entities
{
    [Serializable]
    public class PlayerData
    {
        public string PlayerName;
        public int Health;
        public int Score;
        
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Velocity2;
        public PlayerData2 Data2;

        // Custom datatype handled by [DefineDatatype]
        // public Math.Vector2 Position;

        // This field MUST be skipped by the generator
        [NonSerialized]
        public int TemporarySessionToken = 999;
    }
    
    [Serializable]
    public class PlayerData2
    {
        public string PlayerName;
        public int Health;
        public int Score;

        // Custom datatype handled by [DefineDatatype]
        // public Math.Vector2 Position;

        // This field MUST be skipped by the generator
        [NonSerialized]
        public int TemporarySessionToken = 999;
    }
    
    [Serializable]
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
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
            Console.WriteLine("===========================================");
            Console.WriteLine("    CLASSSAVER2 SOURCE GENERATOR TEST      ");
            Console.WriteLine("===========================================\n");

            // 1. Construct original data
            var originalPlayer = new MyGame.Entities.PlayerData
            {
                PlayerName = "ShadowKnight",
                Health = 85,
                Score = 14200,
                Position = new MyGame.Entities.Vector2(67, 69),
                TemporarySessionToken = 12345 // Should NOT be serialized
            };

            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);
            var reader = new BinaryReader(memoryStream);

            // 2. Test Write (Serializing)
            Console.WriteLine("[1/3] Serializing PlayerData using generated ClassSaver.Write()...");
            ClassSaver.Write(writer, originalPlayer);

            Console.WriteLine($"      -> Stream Size: {memoryStream.Length} bytes\n");

            // Rewind stream position back to beginning for reading
            memoryStream.Position = 0;

            // 3. Test Read (Deserializing)
            Console.WriteLine("[2/3] Deserializing PlayerData using generated ClassSaver.Read()...");
            ClassSaver.Read(reader, out MyGame.Entities.PlayerData loadedPlayer);

            // 4. Assert Results
            Console.WriteLine("\n[3/3] Validating Deserialized Data:");
            bool allPassed = true;

            allPassed &= AssertEquals("PlayerName", originalPlayer.PlayerName, loadedPlayer.PlayerName);
            allPassed &= AssertEquals("Health", originalPlayer.Health, loadedPlayer.Health);
            allPassed &= AssertEquals("Score", originalPlayer.Score, loadedPlayer.Score);
            allPassed &= AssertEquals("Position.X", originalPlayer.Position.X, loadedPlayer.Position.X);
            allPassed &= AssertEquals("Position.Y", originalPlayer.Position.Y, loadedPlayer.Position.Y);

            // Check if [NonSerialized] was ignored (loadedPlayer should have default int value 0)
            if (loadedPlayer.TemporarySessionToken == 999)
            {
                Console.WriteLine("  [PASS] [NonSerialized] field 'TemporarySessionToken' was correctly ignored.");
            }
            else
            {
                Console.WriteLine($"  [FAIL] [NonSerialized] field was incorrectly saved! Value = {loadedPlayer.TemporarySessionToken}");
                allPassed = false;
            }

            // Summary
            Console.WriteLine("\n===========================================");
            if (allPassed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  🎉 SUCCESS: Generator emitted 100% working code!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ❌ FAILURE: Mismatch detected in serialized data.");
            }
            Console.ResetColor();
            Console.WriteLine("===========================================");
        }

        private static bool AssertEquals<T>(string propertyName, T expected, T actual)
        {
            if (Equals(expected, actual))
            {
                Console.WriteLine($"  [PASS] {propertyName,-12} = '{actual}'");
                return true;
            }

            Console.WriteLine($"  [FAIL] {propertyName,-12} Expected: '{expected}', Got: '{actual}'");
            return false;
        }
    }
}