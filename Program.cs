using System;
using System.IO;
using System.Collections.Generic;
using CommandLine; //https://github.com/commandlineparser/commandline
using Bluegrams.Periodica.Data; //https://github.com/Bluegrams/periodic-table-data

namespace ConvertXYZ
{
    class Program
    {
        public class Options
        {
            [Value(0, 
                Required = true,
                HelpText ="Input one or more xyz file(s).")]
            public IEnumerable<string> InputFiles { get; set; }

            [Option('r', "reverse", 
                Required = false, 
                Default = false,
                HelpText = "Reverse processing: computem XYZ to standard XYZ. Default behavior: false (forward processing).")]
            public bool IsReverse { get; set; }

            [Option('o', "outdir",
                Required = false,
                Default = "",
                HelpText = "Specify an output directory (for all files processed). Default behavior: output files are placed in the same folder(s) as the input file(s).")]
            public string OutputDirectory { get; set; }
            
            [Option('v', "verbose",
                Required = false,
                Default = false,
                HelpText = "Turn on the verbose mode to print every line of the processed XYZ record to the screen. Default behavior: false.")]
            public Boolean IsVerbose { get; set; }

            [Option('t', "thermal",
                Required = false,
                Default = (Single)0.08, 
                HelpText = "Specify the RMS thermal vibration coefficient in Angstroms (generally, 0.05-0.1). Default behavior; 0.08. Only used in forward processing.")]
            // Default RMS thermal vibration taken from https://prism-em.com/tutorial-classic/#step3
            public Single ThermalVibration { get; set; }
        }



        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
        }
        
        private static void Run(Options option)
        {
            // Global error indicator; if any file failed
            bool IsGlobalError = false;
            try
            {
                // Load the periodic table
                var objPeriodicTable = PeriodicTable.Load();

                foreach (string strInputFilePath in option.InputFiles)
                {
                    // Print some explanary information
                    string strMode = "forward";
                    string strExplainMode = "standard to computem xyz.";
                    if (option.IsReverse)
                    {
                        strMode = "reverse";
                        strExplainMode = "computem to standard xyz.";
                    }
                    string strVersionNum = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    Console.WriteLine("ConvertXYZ " + strVersionNum + " operating in the " + strMode + " mode, converting from " + strExplainMode);

                    // Error state; if one line fails, stop processing this file and move to the next
                    bool IsFileError = false;

                    // Set up the input file
                    Console.WriteLine("Processing file: " + strInputFilePath);
                    var txtXYZ = new List<string>(File.ReadAllLines(strInputFilePath));

                    // Set up the output file
                    string strOutputDirectory = String.IsNullOrEmpty(option.OutputDirectory) ? Path.GetDirectoryName(strInputFilePath) : option.OutputDirectory;
                    string strOutputAppend = option.IsReverse ? "-std.xyz" : "-computem.xyz";
                    string strOutputFilePath = Path.Combine(strOutputDirectory, Path.GetFileNameWithoutExtension(strInputFilePath) + strOutputAppend);
                    if (File.Exists(strOutputFilePath))
                    {
                        File.Delete(strOutputFilePath);
                    }
                    var txtXYZNew = new List<string>();


                    if (option.IsReverse)
                    {
                        // Reverse: computem XYZ to standard XYZ.
                        // Number of atoms line (will fill in after counting)
                        txtXYZNew.Add("");
                        int intTotalAtoms = 0;
                        // Comment line
                        txtXYZNew.Add(txtXYZ[0]);
                        // Remove the first two lines from the input
                        txtXYZ.RemoveRange(0, 2);
                        // Parse and process each XYZ record
                        foreach (string line in txtXYZ)
                        {
                            // A line "-1" should be treated as the end of file in computem
                            if (line.Equals("-1")) break;
                            // Split the line into segments based on spaces
                            string[] atom = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            if (string.IsNullOrEmpty(line)) continue;
                            // If the format is wrong, print the failed line and stop processing this file
                            if (atom.Length != 6)
                            {
                                StopLine(line);
                                IsFileError = true;
                                IsGlobalError = true;
                                break;
                            }
                            atom[0] = objPeriodicTable[Int32.Parse(atom[0])].Symbol;
                            // Remove occupancy and thermal vibration and add this line to the output
                            var newline = string.Join("   ", atom[0], atom[1], atom[2], atom[3]);
                            txtXYZNew.Add(newline);
                            if (option.IsVerbose) Console.WriteLine(newline);
                            intTotalAtoms++;
                        }
                        if (IsFileError) break;
                        txtXYZNew[0] = intTotalAtoms.ToString();
                    }
                    else
                    {
                        // Forward: standard XYZ to compputem XYZ.
                        // Comment line
                        txtXYZNew.Add(string.Join(", ", txtXYZ[0], txtXYZ[1]));
                        // Cell size line (will fill in after determining the boundaries)
                        txtXYZNew.Add("");
                        // Remove the first two lines from the input
                        txtXYZ.RemoveRange(0, 2);
                        // Make a list of x y z values (to determine the cell size for the second line)
                        var x = new List<Single>();
                        var y = new List<Single>();
                        var z = new List<Single>();
                        // Parse and process each XYZ record
                        foreach (string line in txtXYZ)
                        {
                            // Split the line into segments based on spaces
                            string[] atom = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            if (string.IsNullOrEmpty(line)) continue;
                            // If the format is wrong, print the failed line and stop processing this file
                            if (atom.Length != 4)
                            {
                                StopLine(line);
                                IsFileError = true;
                                IsGlobalError = true;
                                break;
                            }
                            atom[0] = objPeriodicTable[atom[0]].AtomicNumber.ToString();
                            // Add occupancy (1) and thermal vibration coeff and add this line to the output
                            var newline = string.Join("   ", string.Join("   ", atom), '1', option.ThermalVibration.ToString());
                            txtXYZNew.Add(newline);
                            if (option.IsVerbose) Console.WriteLine(newline);
                            // insert x y z values to the lists
                            x.Add(Convert.ToSingle(atom[1]));
                            y.Add(Convert.ToSingle(atom[2]));
                            z.Add(Convert.ToSingle(atom[3]));
                        }
                        if (IsFileError) break;
                        // Get the x y z boundaries as the cell size for the second line of the output
                        x.Sort();
                        y.Sort();
                        z.Sort();
                        var xmax = x[x.Count - 1];
                        var ymax = y[y.Count - 1];
                        var zmax = z[z.Count - 1];
                        txtXYZNew[1] = string.Join("   ", "  ", xmax, ymax, zmax);
                        // Add "-1" to the end of the file --computem file format
                        txtXYZNew.Add("-1");
                    }
                    Console.WriteLine("Writing file to: " + strOutputFilePath);
                    File.WriteAllLines(strOutputFilePath, txtXYZNew);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                IsGlobalError = true;
            }
            Console.WriteLine("All tasks completed.");
            if(IsGlobalError)
            {
                Console.WriteLine("There were error(s) during the conversion.");
            }
            else
            {
                Console.WriteLine("No error detected.");
            }
        }

        private static void StopLine(string strLine)
        {
            Console.WriteLine("File format is wrong. Cannot process this line: " + strLine);
            Console.WriteLine("This file is skipped.");
        }
    }
}
