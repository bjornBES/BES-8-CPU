namespace assembler
{
    public partial class Assembler
    {
        public class AssemblerVariables
        {
            public static void AddVariable(string Name, uint Value, bool IsPointer, bool IsImm)
            {
                if(IsPointer)
                    AssemblerLists.PointerVariables.Add(new Variables() { Name = Name, Value = Value });
                else if (IsImm)
                    AssemblerLists.ImmVariables.Add(new Variables() { Name= Name, Value = Value });
                else
                    AssemblerLists.Variables.Add(new Variables() { Name = Name, Value= Value });
            }
        }
        public static uint UseVariables(string Name, out string AddrMode)
        {
            Name = Name.TrimStart('%');
            for (int i = 0; i < AssemblerLists.Variables.Count; i++)
            {
                if (Name == AssemblerLists.Variables[i].Name)
                {
                    string HexString = Convert.ToString(AssemblerLists.Variables[i].Value, 16).PadLeft(5, '0').Remove(0, 1);
                    AddrMode = "1";
                    return Convert.ToUInt32(HexString, 16);
                }
            }
            for (int i = 0; i < AssemblerLists.PointerVariables.Count; i++)
            {
                if (Name == AssemblerLists.PointerVariables[i].Name)
                {
                    string HexString = Convert.ToString(AssemblerLists.PointerVariables[i].Value, 16).PadLeft(5, '0').Remove(0, 1);
                    AddrMode = "1";
                    return Convert.ToUInt32(HexString, 16);
                }
            }
            for (int i = 0; i < AssemblerLists.ImmVariables.Count; i++)
            {
                if (Name == AssemblerLists.ImmVariables[i].Name)
                {
                    string HexString = Convert.ToString(AssemblerLists.ImmVariables[i].Value, 16).PadLeft(5, '0').Remove(0, 1);
                    AddrMode = "0";
                    return Convert.ToUInt32(HexString, 16);
                }
            }
            AssemblerErrors.ErrorVariableNotFound(Name);
            AddrMode = "F";
            return 0;
        }
    }
}
