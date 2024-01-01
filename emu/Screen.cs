﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emu
{
    public class Screen : IExpansionPort
    {
        public MEM MEM { get; set; }
        public ushort DataBus { get; set; }
        public ushort AddrBus { get; set; }
        public bool Read { get; set; }
        public ushort MemBank { get; set; }
        public void TICK()
        {
        }
        public void RESET()
        {
        }

        public void Con(MEM mem)
        {
        }
    }
}
