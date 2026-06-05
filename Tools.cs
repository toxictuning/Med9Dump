using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Med9Dump
{
    internal class Tools
    {
        public static string FindPattern(
            List<Objs.Instruction> instructions,
            string[] pattern)
        {
            for (int i = 0; i <= instructions.Count - pattern.Length; i++)
            {
                bool match = true;

                for (int j = 0; j < pattern.Length; j++)
                {
                    if (instructions[i + j].Mnemonic.Trim() != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return instructions[i].Address;
            }

            return null;
        }
    }
}
