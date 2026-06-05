using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med9Dump
{
    internal class Objs
    {
        public class Instruction
        {
            public string Address { get; set; }
            public string Mnemonic { get; set; }
            public string Operand { get; set; }
        }

        public class RamCell
        {
            public int Channel { get; set; }
            public string VariableName { get; set; }
            public string Description { get; set; }
        }
    }
}
