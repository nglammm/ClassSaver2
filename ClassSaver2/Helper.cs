using System.Text;

namespace ClassSaver2
{
    public static class Helper
    {
        public static string Varify(string input)
        {
            StringBuilder output = new StringBuilder();
            foreach (char c in input)
            {
                if (c == '.')
                {
                    output.Append('_');
                }
                else output.Append(c);
            }
            
            return output.ToString();
        }
    }
}