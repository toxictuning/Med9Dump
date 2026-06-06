# Med9Dump

A command-line tool for disassembling and analyzing Bosch MED9.1 ECU firmware images from VAG group vehicles. Originally developed for extracting function pointers and RAM variable mappings for custom measuring blocks, data logging, and firmware research.

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Building](#building)
- [Usage](#usage)
- [CSV Mapping](#csv-mapping-tkmwl-ram-zellencsv)
- [Output](#output)
- [Technical Details](#technical-details)
- [Third-Party Libraries](#third-party-libraries)
- [License](#license)
- [Legal Disclaimer](#legal-disclaimer)

## Features

- **PowerPC Disassembly**: Disassembles 32-bit big-endian PowerPC instructions from ECU firmware
- **Function Scanning**: Automatically locates known function patterns and extracts function pointer tables
- **RAM Mapping**: Loads optional CSV files to annotate variables with channel names and descriptions
- **Flexible Output**: Console display, file output, or both with command-line control
- **Search Filtering**: Filter disassembly results with case-insensitive search terms

## Requirements

- Windows 7 or later
- .NET Framework 4.8 Runtime
- Visual Studio 2022 (for building from source)

## Building

### From Visual Studio

1. Clone the repository:
   ```bash
   git clone https://github.com/toxictuning/Med9Dump.git
   cd Med9Dump
   ```

2. Open `Med9Dump.sln` in Visual Studio 2022

3. Ensure target framework is set to `.NET Framework 4.8` (Project Properties > Application > Target framework)

4. Build the solution:
   - **Debug**: `Ctrl+Shift+B`
   - **Release**: Build > Build Solution, then select Release configuration

5. Output binary: `bin/Debug/Med9Dump.exe` or `bin/Release/Med9Dump.exe`

### From Command Line

```bash
cd Med9Dump
msbuild Med9Dump.sln /p:Configuration=Release /p:Platform=AnyCPU
```

## Usage

### Basic Syntax

```
Med9Dump.exe [file] [options]
```

### Arguments

| Argument | Description | Default |
|----------|-------------|---------|
| `file` | Path to ECU ROM image (binary file) | `file.bin` |

### Options

| Option | Description |
|--------|-------------|
| `--print` | Print disassembly to the console window |
| `--garbage` | Include known Capstone garbage instructions (e.g., `fnmadd.` double-decode) |
| `--show--allpointers` | Display all pointer table entries, including `UNKNOWN` and zero entries |
| `--output-console` | Print output to console instead of writing `output.txt` |

### Examples

Basic disassembly with file output only:

```bash
Med9Dump.exe firmware.bin
```

Print disassembly to console:

```bash
Med9Dump.exe firmware.bin --print
```

Show all pointers including unknown entries:

```bash
Med9Dump.exe firmware.bin --print --show--allpointers
```

Output to console only (no `output.txt` file):

```bash
Med9Dump.exe firmware.bin --output-console
```

## CSV Mapping: `TKMWL-RAM-Zellen.csv`

The tool supports optional CSV files to annotate pointer table entries with human-readable names.

### File Format

Place `TKMWL-RAM-Zellen.csv` in the same directory as `Med9Dump.exe`.

**CSV Structure:**
```csv
Channel,VariableName,Description
0,EngineSpeed_RPM,Main engine speed in revolutions per minute
1,CoolantTemp_C,Engine coolant temperature in Celsius
2,ThrottlePosition_Percent,Throttle position percentage
```

**Column Definitions:**
- **Channel** (integer): Index in the pointer table (0-based)
- **VariableName** (string): Display name for the variable/function (max 15 characters in output)
- **Description** (string): Human-readable description or notes

### Loading Behavior

- If `TKMWL-RAM-Zellen.csv` exists, it is automatically loaded at startup
- Missing or malformed CSV entries default to `UNKNOWN`
- Entries marked `UNKNOWN` or containing `FREIE` (German: "free/unused") are hidden unless `--show--allpointers` is used
- Entries with pointer value `0x000393D0` are hidden unless `--show--allpointers` is used

## Output

### Files Generated

- **`output.txt`** — Full disassembly listing (written unless `--output-console` is used)
  - Format: `0xADDRESS:  MNEMONIC  OPERAND`
  - Example: `0x00020000:  lis       r3, 0x4000`

### Console Output

The tool prints to console:
- MED9.1 header with file path and region information
- Disassembly (if `--print` is used)
- Function discovery results
- Pointer table entries (if found)
- Summary statistics

### Example Output

```
; MED9.1 disassembly : (c) Wayne M
; File   : firmware.bin
; Region : 0x00020000 - 0x001C0000

== Disassembling ==

0x00020000:  lis       r3, 0x4000
0x00020004:  addi      r3, r3, 0x1000

; ---- function @ 0x00020010 ----

0x00020010:  mflr      r0
0x00020014:  stwu      r1, -0x10(r1)

== Searching for functions ==

; TKMWL Addr: 0x001A0000
; KFMWNTK Size: 512
; Pointer Table Addr: 0x001A5000

== Printing pointers ==

   0 EngineSpeed      0x00020100
   1 CoolantTemp      0x00020200
   2 ThrottlePos      0x00020300
   ...
```

## Technical Details

### Supported Architecture

- **Processor**: PowerPC (32-bit)
- **Byte Order**: Big-endian
- **Instruction Size**: 4 bytes (fixed-width)
- **Disassembler**: Capstone Engine via C# wrapper

### Region Mapping

The tool disassembles a fixed region of the firmware:

```
Code Offset:  0x00020000
Code Length:  0x001A0000 (1.625 MB)
Code End:     0x001C0000
```

These values are hardcoded for MED9.1 ECUs; adjust in source if targeting different firmware versions.

### Function Detection

Functions are identified by recognizing common PowerPC prologue patterns:

- `mflr r0` — Move Link Register to R0 (non-leaf function setup)
- `stwu r1, -N(r1)` — Store with Update to R1 (stack frame allocation)
- `stmw` — Store Multiple Words (multi-register save)

## Third-Party Libraries

This project uses the following third-party components:

### Capstone Engine

- **Purpose**: PowerPC disassembly engine
- **Homepage**: https://www.capstone-engine.org
- **License**: BSD 3-Clause License
- **Copyright**: (c) 2013-2024 Nguyen Anh Quynh and Capstone contributors
- **License File**: [`licenses/CAPSTONE-LICENSE.txt`](licenses/CAPSTONE-LICENSE.txt)

### Gee.External.Capstone

- **Purpose**: .NET/C# wrapper for Capstone Engine
- **NuGet Package**: https://www.nuget.org/packages/Gee.External.Capstone/
- **GitHub**: https://github.com/Gee.External.Capstone
- **License**: MIT License
- **Copyright**: (c) Geehe and contributors
- **License File**: [`licenses/GEE-EXTERNAL-CAPSTONE-LICENSE.txt`](licenses/GEE-EXTERNAL-CAPSTONE-LICENSE.txt)

**Important**: All third-party licenses are included in the `licenses/` directory and must be retained in any redistribution of this project or its binaries.

## License

This project is licensed under the **MIT License**.

### Summary

- ? **Permitted**: Use, modify, and distribute this software for any purpose
- ? **Required**: Include the original MIT license and copyright notice
- ? **Not permitted**: Hold the author liable for damages or consequences

See [`LICENSE`](LICENSE) for the complete MIT License text.

## Legal Disclaimer

**?? IMPORTANT: This tool is provided for educational and research purposes only.**

### Risks and Warnings

1. **Vehicle Safety**: Modifying ECU firmware can cause:
   - Engine malfunction or failure
   - Unpredictable vehicle behavior
   - Loss of safety systems (ABS, airbags, stability control)
   - Rendering the vehicle undriveable or dangerous

2. **Legal Consequences**: Firmware modification may violate:
   - **Emissions Regulations**: EPA (USA), EURO standards (EU), and regional air quality laws
   - **Warranty**: Manufacturer warranty typically becomes void
   - **Anti-Circumvention Laws**: DMCA (USA), similar laws in your jurisdiction
   - **Vehicle Registration**: May fail inspections or be considered non-compliant
   - **Criminal Law**: Depending on jurisdiction and method of modification

3. **Environmental Impact**: Bypassing emissions controls contributes to air pollution and environmental harm

### No Liability

The author and all contributors are **explicitly NOT responsible** for:
- Vehicle damage, malfunction, or failure
- Personal injury or death
- Environmental damage
- Legal fines, penalties, or criminal prosecution
- Loss of vehicle warranty or insurance coverage
- Any direct or indirect consequences of using this tool

### Permitted Use Only

This tool may be used only for:
- Educational study of firmware structure and PowerPC assembly language
- Legitimate vehicle diagnostics by qualified technicians
- Research and development in controlled, lawful environments
- Understanding VAG ECU architecture (educational, non-commercial)

### Prohibited Use

Do **NOT** use this tool for:
- Disabling or circumventing emissions controls
- Bypassing security, anti-tamper, or DRM mechanisms
- Illegal vehicle modifications
- Violating manufacturer terms of service or warranty
- Circumventing copyrights or digital rights management
- Any use that violates local, state, or federal law

### User Responsibility

**By downloading, compiling, or using this tool, you acknowledge and accept:**
- Full responsibility for any legal consequences
- Full responsibility for vehicle safety and performance
- Obligation to comply with all applicable local, state, and federal laws
- Obligation to understand the environmental impact of any modifications
- That the author and contributors are NOT liable for any damages or consequences

---

**If you are unsure about the legality of your intended use, consult a qualified attorney in your jurisdiction BEFORE proceeding.**

## Contributing

Contributions are welcome! Please see [`CONTRIBUTING.md`](CONTRIBUTING.md) for guidelines and standards.

### Quick Start

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes
4. Commit with clear messages: `git commit -m "Describe your changes"`
5. Push to your fork: `git push origin feature/your-feature-name`
6. Open a Pull Request with a detailed description

---

**Author**: Wayne M  
**Repository**: https://github.com/toxictuning/Med9Dump  
**Issues & Feedback**: https://github.com/toxictuning/Med9Dump/issues