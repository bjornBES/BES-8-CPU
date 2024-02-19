namespace Compiler
{
    public enum DataTypes
    {
        _ubyte  = 7,     // 7-bit
        _byte   = 8,     // 8-bit
        _short  = 15,    // 16-but signed
        _ushort = 16,    // 16-bit unsigned
        _int    = 20     // 20-bit unsigned
    }
    public class Type
    {
        DataTypes DataTypes;
        static int[] Bit_Limits =
        {
            7,
            8,
            15,
            16,
            20
        };
        public int BitLimit
        {
            get;
            private set;
        }
        public Type(int bitLimit_Index)
        {
            BitLimit = Bit_Limits[bitLimit_Index];
            switch (bitLimit_Index)
            {
                case 0:
                    DataTypes = DataTypes._ubyte;
                    break;
                case 1:
                    DataTypes = DataTypes._byte;
                    break;
                case 2:
                    DataTypes = DataTypes._short;
                    break;
                case 3:
                    DataTypes = DataTypes._ushort;
                    break;
                case 4:
                    DataTypes = DataTypes._int;
                    break;
                default:
                    break;
            }
        }
    }
}
