using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm
{
    public class AssemblerMarcos : AssemblerConst
    {
        public string ConvTo(int Base, string value, int FromBase)
        {
            return Convert.ToString(Convert.ToUInt16(value, FromBase), Base).PadLeft(5, '0');
        }
        public string ConvTo32(int Base, string value, int FromBase)
        {
            return Convert.ToString(Convert.ToUInt32(value, FromBase), Base).PadLeft(5, '0');
        }
        public string ConvFrom(ref string value)
        {
            if (value.Last() == 'h')
            {
                value = value.TrimEnd('h').PadLeft(5, '0');
                return value;
            }
            else if (value.Last() == 'b')
            {
                value = value.TrimEnd('b');
                value = ConvTo32(16, value, 2).PadLeft(5, '0');
                return value;
            }
            else if (!char.IsDigit(value[0]))
            {
                if (value.StartsWith('['))
                {
                    value = value.Replace("[", "");
                    value = value.Replace("]", "");
                    return "LABLE: " + value;
                }
                else
                {
                    ErrorSyntax("ARGS CONVFROM");
                    value = "Null";
                    return "Null";
                }
            }
            else
            {
                value = ConvTo32(16, value, 10).PadLeft(5, '0');
                return value;
            }
        }
        public string[] Split32bitNumber(string Value, char EndChar)
        {
            string LOW;
            string HIGH;
            if (EndChar == 'h')
            {
                Value = Value.PadLeft(8, '0');

                HIGH = Value.Substring(0, 4);
                LOW = Value.Substring(4, 4);
            }
            else if (EndChar == 'b')
            {
                Value = Value.PadLeft(32, '0');

                HIGH = Value.Substring(0, 16);
                LOW = Value.Substring(16, 16);
            }
            else
            {
                Value = Value.PadLeft(10, '0');

                HIGH = Value.Substring(0, 5);
                LOW = Value.Substring(5, 5);
            }

            return new string[] { HIGH, LOW };
        }
    }
}
