using System.Collections;
using System.Text;

namespace ClassSaver2.PredefinedSource
{
    [DefineSource(typeof(IList))]
    public static class HandleIList
    {
        public static string Write(string fullVarName, Main.TypeHandler typeHandler, int indent)
        {
            string INDENT = "    ";
            for (int i = 0; i < indent; i++)
            {
                INDENT += "    ";
            }
            
            StringBuilder outputCode = new StringBuilder();
            outputCode.Append($@"
{INDENT}writer.Write({fullVarName}.Count);
{INDENT}for (int i = 0; i < {fullVarName}.Count; i++)
{INDENT}{{
{INDENT}    ");

            if (typeHandler.FunctionWriteType == Main.TypeHandler.FuncWriteType.ClassSaverItself)
            {
                outputCode.Append($@"
{INDENT}    {typeHandler.GetFunctionWrite("reader", $"{fullVarName}[i]", "context")};
{INDENT}    ");
            }
            else
            {
                // FuncWriteType.External
                outputCode.Append($@"
{INDENT}    {typeHandler.GetFunctionWrite("writer", $"{fullVarName}[i]")};
{INDENT}    ");
            }
            
            // end scope
            outputCode.Append($@"
{INDENT}}}");
            
            return outputCode.ToString();
        }

        public static string Read(string fullVarType, Main.TypeHandler typeHandler, int indent, int id)
        {
            string INDENT = "    ";
            for (int i = 0; i < indent; i++)
            {
                INDENT += "    ";
            }
            
            StringBuilder outputCode = new StringBuilder();
            outputCode.Append($@"
{INDENT}var count_{id} = reader.ReadInt32();
{INDENT}var iList_{id} = new {fullVarType}();
{INDENT}for (int i = 0; i < count_{id}; i++)
{INDENT}{{
{INDENT}    ");

            if (typeHandler.FunctionReadType == Main.TypeHandler.FuncReadType.ClassSaverItself)
            {
                outputCode.Append($@"
{INDENT}    {typeHandler.GetFunctionRead("reader", $"out var data", "context")};
{INDENT}    ");
            }
            else
            {
                // FuncReadType.ReturnType
                outputCode.Append($@"
{INDENT}    var data = {typeHandler.GetFunctionRead("writer")};
{INDENT}    ");
            }

            outputCode.Append($@"iList_{id}.Add(data);
{INDENT}}}");
            
            return outputCode.ToString();
        }
    }
}