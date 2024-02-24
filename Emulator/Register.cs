using System.Globalization;
using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;

namespace emulator
{
    [Serializable]
    [ComVisible(true)]
    public struct Register
    {
        public uint m_value;

        public Register()
        {
            m_value = 0;
        }

        public Register(uint value)
        {
            m_value = value;
        }
        public Register(int value)
        {
            m_value = (uint)value;
        }

        public Register this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return GetLowByte();
                }
                else return GetHighByte();
            }

            set
            {
                if (index == 0)
                {
                    SetLowByte(value.m_value);
                }
                else SetHighByte(value.m_value);
            }
        }

        public Register GetHighByte()
        {
            return new Register((m_value & 0x000FF800));
        }
        public Register GetLowByte()
        {
            return new Register((m_value & 0x000007FF));
        }
        public void SetHighByte(uint value)
        {
            m_value = (m_value & 0x000FF800) | ((uint)value << 8);
        }
        public void SetLowByte(uint value)
        {
            m_value = (m_value & 0x00008FF) | value;
        }

        public void SetHighByte(ushort value)
        {
            m_value = (m_value & 0x00FF) | ((uint)value << 8);
        }
        public void SetLowByte(ushort value)
        {
            m_value = (m_value & 0xFF00) | value;
        }

        public static bool operator ==(Register left, Register right)
        {
            return left.m_value == right.m_value;
        }

        public static bool operator !=(Register left, Register right)
        {
            return left.m_value != right.m_value;
        }

        public static bool operator <(Register left, Register right)
        {
            return left.m_value < right.m_value;
        }

        public static bool operator <=(Register left, Register right)
        {
            return left.m_value <= right.m_value;
        }

        public static bool operator >(Register left, Register right)
        {
            return left.m_value > right.m_value;
        }

        public static bool operator >=(Register left, Register right) => left.m_value >= right.m_value;

        public static bool operator ==(Register left, int right)
        {
            return left.m_value == right;
        }

        public static bool operator !=(Register left, int right)
        {
            return left.m_value != right;
        }

        public static bool operator <(Register left, int right)
        {
            return left.m_value < right;
        }

        public static bool operator <=(Register left, int right)
        {
            return left.m_value <= right;
        }

        public static bool operator >(Register left, int right)
        {
            return left.m_value > right;
        }

        public static bool operator >=(Register left, int right)
        {
            return left.m_value >= right;
        }

        public static Register operator ++(Register r)
        {
            r.m_value += 1;
            return r;
        }

        public static Register operator +(Register left, Register right)
        {
            return left + right;
        }

        public static Register operator --(Register r)
        {
            r.m_value -= 1;
            return r;
        }
        public static Register operator <<(Register register, int shift)
        {
            return new Register(register.m_value << shift);
        }

        public static Register operator >>(Register register, int shift)
        {
            return new Register(register.m_value >> shift);
        }

        public static implicit operator Register(int v)
        {
            return new Register(v);
        }
        public static implicit operator Register(uint v)
        {
            return new Register(v);
        }
        public static implicit operator Register(ushort v)
        {
            return new Register(v);
        }

        public static explicit operator int(Register v)
        {
            return (int)v.m_value;
        }
        public static explicit operator uint(Register v)
        {
            return v.m_value;
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
