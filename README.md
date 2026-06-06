# Med9Dump

Med9Dump is a small command-line tool for disassembling and scanning Bosch MED9.1 ECU firmware images (commonly used in VAG group vehicles). It was created to extract function pointers and RAM variable mappings used when building custom measuring blocks for data logging and to assist in locating map data.

## Features

- Disassembles PowerPC (32-bit, big-endian) code from an ECU image
- Scans for known function patterns and locates a function pointer table
- Prints a human-readable disassembly and writes the disassembly to `output.txt`
- Optionally loads a CSV mapping (`TKMWL-RAM-Zellen.csv`) to annotate pointer table entries with channel names and descriptions
- Command-line search/filter for mnemonics or operands

## Requirements

- Windows
- .NET Framework 4.8
- Visual Studio 2022 (recommended) to build from source

## Building

1. Open the `Med9Dump` solution in Visual Studio 2022.
2. Target `.NET Framework 4.8` and build the project (Debug or Release).

## Usage

```
Med9Dump.exe [file] [search-term] [options]
```

- `file` — Path to ROM image (default: `file.bin`)
- `search-term` — Optional case-insensitive filter applied to disassembly lines

### Options

- `--print`               Print the disassembly to the console
- `--garbage`             Include known Capstone garbage instruction (`fnmadd.`)
- `--show-allpointers`    Show all pointer table entries (including `UNKNOWN` / zero entries)
- `--output-console`      Print the disassembly output to the console instead of writing `output.txt` (if available)

### Examples

- `Med9Dump.exe file.bin --print`
- `Med9Dump.exe file.bin "lwz r3"`

## CSV mapping: `TKMWL-RAM-Zellen.csv`

If present in the working directory, the tool attempts to load `TKMWL-RAM-Zellen.csv` to annotate pointer entries. The CSV must have a header and rows with the following columns:

```
Channel,VariableName,Description
0,SomeVar,Main engine speed
1,AnotherVar,Temperature
```

`Channel` is an integer index that maps to the pointer table entry. `VariableName` is used for human-readable output.

## Output

- `output.txt` — disassembly lines (unless `--output-console` is used)
- Console output shows summary information and pointer table entries when available

## Safety, Legality, and Disclaimers

Working with ECU firmware and modifying vehicle control systems can be dangerous and may be illegal depending on your jurisdiction or how the results are used. Use this tool only for lawful and safe research, development, or diagnostic purposes. The author and contributors are not responsible for damages or legal consequences resulting from misuse.

## Contributing

See `CONTRIBUTING.md` for contribution guidelines and coding standards. Pull requests, bug reports, and feature requests are welcome.

## License

This project is licensed under the MIT License. See the `LICENSE` file for more details.

---