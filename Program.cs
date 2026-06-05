using Gee.External.Capstone;
using Gee.External.Capstone.PowerPc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using static Med9Dump.Objs;

namespace Med9Dump
{
    internal class Program
    {
        static readonly List<Instruction> InstructionSet = new List<Instruction>();
        const int CodeOffset = 0x20000;
        const int CodeLength = 0x1A0000;

        static bool _garbage = false;
        static bool _print = false;
        static bool _allpointers = false;
        static bool _outputConsole = false;
        const string RamZellenCsv = "TKMWL-RAM-Zellen.csv";
        static string filePath = "file.bin";
        static List<RamCell> ramZellen = new List<RamCell>();
        static bool HasArg(string[] args, string arg)
        {
            return args.Any(x => x.Equals(arg, StringComparison.OrdinalIgnoreCase));
        }

        static int Main(string[] args)
        {

            _print = HasArg(args, "--print");
            _garbage = HasArg(args, "--garbage");
            _allpointers = HasArg(args, "--show--allpointers");
            _outputConsole = HasArg(args,"--output--console");
            filePath = args.Length > 0 ? args[0] : "file.bin";
            PrintUsage();
            string searchTerm = args.Length > 1 ? args[1].ToLowerInvariant() : null;
            string ln = string.Empty;
            if (!File.Exists(filePath))
            {
               
                Console.Error.WriteLine("Error: file not found: " + filePath);
                return 1;
            }

            if (File.Exists(RamZellenCsv))
            {
                ramZellen = File.ReadLines("TKMWL-RAM-Zellen.csv")
                    .Skip(1)
                    .Select(line =>
                    {
                        string[] c = line.Split(',');

                        return new RamCell
                        {
                            Channel = int.Parse(c[0]),
                            VariableName = c[1],
                            Description = c[2]
                        };
                    })
                    .ToList();
            }

            byte[] rom = File.ReadAllBytes(filePath);

            if (rom.Length < CodeOffset + CodeLength)
            {
                Console.Error.WriteLine(
                    "Error: file too small. Need 0x" + (CodeOffset + CodeLength).ToString("X") +
                    " bytes, got 0x" + rom.Length.ToString("X"));
                return 1;
            }

            List<string> output = new List<string>();
            byte[] codeSection = new byte[CodeLength];
            Array.Copy(rom, CodeOffset, codeSection, 0, CodeLength);

            using (CapstonePowerPcDisassembler disassembler = CapstoneDisassembler.CreatePowerPcDisassembler(
                PowerPcDisassembleMode.Bit32 | PowerPcDisassembleMode.BigEndian))
            {
                disassembler.EnableInstructionDetails = true;
                //disassembler.NullTerminateInstructionMnemonic = true;

                Console.WriteLine("\n== Disassembling ==");
                int count = 0;
                int offset = 0;

                while (offset < codeSection.Length)
                {
                    // Feed 4 bytes at a time — avoids wrapper swallowing the whole buffer silently
                    int remaining = codeSection.Length - offset;
                    int chunkSize = Math.Min(4, remaining);

                    byte[] chunk = new byte[chunkSize];
                    Array.Copy(codeSection, offset, chunk, 0, chunkSize);

                    long address = CodeOffset + offset;
                    var result = disassembler.Disassemble(chunk, address);


                        if (result == null || result.Length == 0)
                        {
                            // Undecodable — emit a .word and advance 4 bytes
                            if (searchTerm == null)
                            {
                            uint raw = 0;
                            for (int b = 0; b < chunkSize; b++)
                                    raw = (raw << 8) | codeSection[offset + b];
                                if (_print)
                                    Console.WriteLine("0x" + address.ToString("X8") + ":  .word       0x" + raw.ToString("X8"));
                            
                            InstructionSet.Add(new Instruction
                            {
                                Address = "0x" + address.ToString("X8"),
                                Mnemonic = ".word".Trim(),
                                Operand = raw.ToString("X8")
                            });
                        }
                            offset += 4;
                            continue;
                        }

                    //Print Functions
                    foreach (var item in result.Select((ins, index) => new { ins, index }))
                    {

                            if (item.ins.Mnemonic == "fnmadd." && !_garbage)
                                continue; // Capstone bug: fnmadd is decoded as 2 instructions, the second is garbage

                            if (IsFunctionStart(item.ins) && _print)
                                Console.WriteLine("\n; ---- function @ 0x" + item.ins.Address.ToString("X8") + " ----");

                            string line = "0x" + item.ins.Address.ToString("X8") + ":  " +
                                          item.ins.Mnemonic.PadRight(12) + " " + item.ins.Operand;

                            if ((searchTerm == null || line.ToLowerInvariant().Contains(searchTerm))&&_print)
                                Console.WriteLine(line);

                            output.Add(line);
                            count++;


                        InstructionSet.Add(new Instruction
                        {
                            Address = item.ins.Address.ToString("X8"),
                            Mnemonic = item.ins.Mnemonic.PadRight(12).Trim(),
                            Operand = item.ins.Operand
                        });

                    }

                    offset += 4;
                }

                Console.WriteLine();
                Console.WriteLine("; Instructions found: " + count);
                //Console.WriteLine("; Total dictionary: " + InstructionSet.Count);
                Console.WriteLine("\n== Searching for functions ==");
                string Addr_TKMWL = Tools.FindPattern(InstructionSet, Functions.TKMWL);

                if (Addr_TKMWL != null)
                {
                    Console.WriteLine($"; TKMWL Addr: 0x{Addr_TKMWL}");

                    int instrIndex = InstructionSet.FindIndex(x =>
                        x.Address.Equals(Addr_TKMWL, StringComparison.OrdinalIgnoreCase));

                    int KFMWNTK_Size = Convert.ToInt32(
                        InstructionSet[instrIndex + 4]
                            .Operand
                            .Split(',')[1]
                            .Trim()
                            .Replace("0x", ""),
                        16);

                    Console.WriteLine($"; KFMWNTK Size: {KFMWNTK_Size}");

                    string Addr_Pointers_Factor = InstructionSet[instrIndex + 6]
                     .Operand
                     .Split(',')
                     .Last()
                     .Trim();
                    //Console.WriteLine($"; Pointers Factor Addr: {Addr_Pointers_Factor}");

                    string Addr_Pointers = InstructionSet[instrIndex + 7]
                        .Operand
                        .Split(',')
                        .Last()
                        .Trim();

                    //Console.WriteLine($"; Pointers Addr: {Addr_Pointers}");

                    uint high = Convert.ToUInt32(
                        InstructionSet[instrIndex + 6].Operand.Split(',')[1].Trim().Replace("0x", ""),
                        16);

                    uint low = Convert.ToUInt32(
                        InstructionSet[instrIndex + 7].Operand.Split(',')[2].Trim().Replace("0x", ""),
                        16);

                    uint pointerAddr = (high << 16) | low;

                    Console.WriteLine($"; Pointer Table Addr: 0x{pointerAddr:X}");

                    byte[] bytes = File.ReadAllBytes(filePath);
                   
                    Console.WriteLine("\n== Printing pointers ==");
                    for (int i = 0; i < KFMWNTK_Size; i++)
                    {
                        int addr = (int)pointerAddr + (i * 4);

                        uint funcPtr =
                            ((uint)bytes[addr] << 24) |
                            ((uint)bytes[addr + 1] << 16) |
                            ((uint)bytes[addr + 2] << 8) |
                            bytes[addr + 3];

                        string name = ramZellen.FirstOrDefault(x => x.Channel == i)?.VariableName ?? "UNKNOWN";

                        if (!_allpointers)
                        {
                            if (name.ToUpper() == "UNKNOWN" || funcPtr == 0x000393D0 || name.ToUpper().Contains("FREIE"))
                                continue;
                        }

                        if (name.Length > 15)
                            name = name.Substring(0, 15);

                        ln = $"{i,4} {name,-15} 0x{funcPtr:X8}";
                        Console.WriteLine(ln);
                        output.Add(ln);
                    }
                    ln = "\n== End for pointers ==";
                    Console.WriteLine(ln);
                    output.Add(ln);
                }

            }
            if (_outputConsole)
                File.WriteAllLines("output.txt", output);
            return 0;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("; MED9.1 disassembly : (c) Wayne M - TOXIC TUNING");
            Console.WriteLine("; File   : " + filePath);
            Console.WriteLine("; Region : 0x" + CodeOffset.ToString("X8") + " - 0x" + (CodeOffset + CodeLength).ToString("X8"));
            Console.WriteLine();
            Console.WriteLine("Usage: Med9Dump.exe [file] [options]");
            Console.WriteLine("  file          Path to ROM image (default: file.bin)");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --print               Print disassembly to the console");
            Console.WriteLine("  --garbage             Include known Capstone garbage instruction (fnmadd.)");
            Console.WriteLine("  --show--allpointers   Show all pointer table entries (including UNKNOWN/zero)");
            Console.WriteLine("  --output--console     Saves the output to a text file.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  Med9Dump.exe file.bin --print");
            Console.WriteLine();
        }

        static bool IsFunctionStart(PowerPcInstruction ins)
        {
            string mnemonic = ins.Mnemonic != null ? ins.Mnemonic.ToLowerInvariant() : "";

            // mflr r0 — standard non-leaf function prologue
            if (mnemonic == "mflr")
                return true;

            // stwu r1, -N(r1) — stack frame allocation
            if (mnemonic == "stwu" && ins.Operand != null && ins.Operand.Contains("r1"))
                return true;

            // stmw — save multiple regs, always a prologue
            if (mnemonic == "stmw")
                return true;

            return false;
        }
    }
}