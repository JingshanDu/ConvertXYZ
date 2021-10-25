# ConvertXYZ
 Converts standard XYZ files to computem XYZ files, or vice versa.

 ## Background
[The XYZ file](https://en.wikipedia.org/wiki/XYZ_file_format) format is used to describe the atomic coordinates in a chemical, crystal, or any matter. Although a relatively "standard" file format exist, not all programs are following the same file format.

One outstanding outlier is the famous multislice electron microscopy simulation package [_computem_ authored by Dr. Earl J. Kirkland](https://sourceforge.net/projects/computem/), which uses [a slightly different xyz file format](https://prism-em.com/tutorial-classic/#step3) than the standard one generated by most modeling software, for example, the [_VESTA_ package](https://jp-minerals.org/vesta/en/). To maintain compatibility, some other simulation packages, such as [_Prismatic_](https://prism-em.com/), also uses this special xyz format.

The purpose of this program is to convert between standard and _computem_ xyz files. As such, models built in _VESTA_ and other software can be used in simulation packages easily. 

## Usage
```
ConvertXYZ file_name(s) [options]
```

file_name(s): Required. Input one or more xyz file(s). By default, this command converts the standard xyz file(s) into the _computem_ xyz format and places the output file besides the original one in the same folder.

Options:

 -r, --reverse    (Default: false) Reverse processing: computem XYZ to standard XYZ. Default behavior: false (forward
                   processing).

  -o, --outdir     (Default: ) Specify an output directory (for all files processed). Default behavior: output files are
                   placed in the same folder(s) as the input file(s).

  -v, --verbose    (Default: false) Turn on the verbose mode to print every line of the processed XYZ record to the
                   screen. Default behavior: false.

  -t, --thermal    (Default: 0.08) Specify the RMS thermal vibration coefficient in Angstroms (generally, 0.05-0.1).
                   Default behavior; 0.08. Only used in forward processing.

  --help           Display a help screen.

  --version        Display version information.

## Building
Visual Studio 2019, .net Core 3.1

Dependencies: 

CommandLine: https://github.com/commandlineparser/commandline

Bluegrams.Periodica.Data: https://github.com/Bluegrams/periodic-table-data

## Download the Programs
Please visit https://github.com/SequoiaDu/ConvertXYZ/releases

Availablity: Windows (win-x86), Linux (linux-x64), and macOS (osx-x64)

Tested on Windows only but should work in all other environments. Programs are self-contained and have required dependencies included in the single executable file.
