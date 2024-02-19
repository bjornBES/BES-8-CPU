namespace assembler
{
    public partial class Assembler
    {
        public class AssemblerLables
        {
            public static uint UseLable(string name, bool WriteTokens = false)
            {
                for (int i = 0; i < AssemblerLists.lables.Count; i++)
                {
                    if (name.TrimEnd(':') == AssemblerLists.lables[i].Name)
                    {
                        if (WriteTokens)
                            AssemblerLists.Tokens.Add(Convert.ToString(AssemblerLists.lables[i].Addr, 16).PadLeft(5, '0') + " | " + "Lable " + name + " ");
                        return (uint)AssemblerLists.lables[i].Addr;
                    }
                }
                for (int i = 0; i < AssemblerLists.GlobalLables.Count; i++)
                {
                    if (name.TrimEnd(':') == AssemblerLists.GlobalLables[i].Name)
                    {
                        if (WriteTokens)
                            AssemblerLists.Tokens.Add(Convert.ToString(AssemblerLists.GlobalLables[i].Addr, 16).PadLeft(5, '0') + " | " + "Global Lable " + name + " ");
                        return (uint)AssemblerLists.GlobalLables[i].Addr;
                    }
                }
                return 0x8000;
            }
        }
    }
}
