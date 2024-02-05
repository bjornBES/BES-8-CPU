using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Function
    {
        public string Name { get; set; }
        public Token ReturnType { get; set; }
        public List<Argument> arguments = new();

        public string CallFunc(params Argument[] values)
        {
            string code = "";

            for (int i = 0; i < arguments.Count; i++)
            {
                switch (arguments[i].type.Type)
                {
                    case TokenType.byte_:
                        code += "push #" + values[i].Value + "\n";
                        break;
                    case TokenType.word:
                        code += "push #" + values[i].Value + "\n";
                        break;
                    case TokenType.let:
                        code += "push #" + values[i].Value + "\n";
                        break;
                    default:
                        break;
                }
            }

            if(ReturnType != null)
            {
                code += "pop ZX" + "\n";
            }

            code += "call [_" + Name + "]" + "\n";
            if (arguments.Count != 0)
            {
                code += "add SP #" + arguments.Count + "\n";
            }

            if (ReturnType != null)
            {
                code += "push ZX" + "\n";
            }

            return code;
        }
        public string CallFunc()
        {
            string code = "";

            if (ReturnType != null)
            {
                code += "pop ZX" + "\n";
            }

            code += "call [_" + Name + "]" + "\n";

            if (ReturnType != null)
            {
                code += "push ZX" + "\n";
            }

            return code;
        }
    }
}
