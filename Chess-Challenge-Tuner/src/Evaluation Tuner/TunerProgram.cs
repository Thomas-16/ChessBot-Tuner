using System;
using System.IO;

namespace ChessChallenge.Application
{
    public static class TunerProgram
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Chess Engine Evaluation Tuner ===");
            Console.WriteLine("Using Texel's Tuning Method\n");

            // Check command line arguments
            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            string command = args[0].ToLower();

            switch (command)
            {
                case "tune":
                    RunTuning(args);
                    break;
                case "test":
                    TestEngine(args);
                    break;
                case "convert":
                    ConvertEPD(args);
                    break;
                default:
                    ShowUsage();
                    break;
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  TunerProgram.exe tune <epd-file> [options]");
            Console.WriteLine("  TunerProgram.exe test <epd-file> [max-positions]");
            Console.WriteLine("  TunerProgram.exe convert <input-file> <output-file>");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  tune      - Run Texel tuning on the provided EPD file");
            Console.WriteLine("  test      - Test current evaluation on EPD positions");
            Console.WriteLine("  convert   - Convert EPD file to internal format");
            Console.WriteLine();
            Console.WriteLine("Tune Options:");
            Console.WriteLine("  --iterations <n>     Number of tuning iterations (default: 100)");
            Console.WriteLine("  --learning-rate <r>  Initial learning rate (default: 0.01)");
            Console.WriteLine("  --batch-size <n>     Batch size for gradient calculation (default: 1000)");
            Console.WriteLine("  --max-positions <n>  Maximum positions to load (default: 500000)");
            Console.WriteLine("  --output <file>      Output file for tuned parameters");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  TunerProgram.exe tune quiet-labeled.epd --iterations 200 --output tuned_params.cs");
        }

        private static void RunTuning(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: EPD file path required");
                return;
            }

            string epdFile = args[1];
            if (!File.Exists(epdFile))
            {
                Console.WriteLine($"Error: EPD file not found: {epdFile}");
                return;
            }

            // Parse options
            int iterations = 100;
            double learningRate = 10000;
            int batchSize = 10000;
            int maxPositions = 500000;
            string outputFile = "tuned_parameters.cs";

            for (int i = 2; i < args.Length - 1; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--iterations":
                        if (int.TryParse(args[i + 1], out int iter))
                            iterations = iter;
                        i++;
                        break;
                    case "--learning-rate":
                        if (double.TryParse(args[i + 1], out double lr))
                            learningRate = lr;
                        i++;
                        break;
                    case "--batch-size":
                        if (int.TryParse(args[i + 1], out int bs))
                            batchSize = bs;
                        i++;
                        break;
                    case "--max-positions":
                        if (int.TryParse(args[i + 1], out int mp))
                            maxPositions = mp;
                        i++;
                        break;
                    case "--output":
                        outputFile = args[i + 1];
                        i++;
                        break;
                }
            }

            Console.WriteLine($"Starting tuning with parameters:");
            Console.WriteLine($"  EPD file: {epdFile}");
            Console.WriteLine($"  Iterations: {iterations}");
            Console.WriteLine($"  Learning rate: {learningRate}");
            Console.WriteLine($"  Batch size: {batchSize}");
            Console.WriteLine($"  Max positions: {maxPositions}");
            Console.WriteLine($"  Output file: {outputFile}");
            Console.WriteLine();

            var tuner = new TexelTuner();
            
            try
            {
                tuner.LoadPositionsFromEPD(epdFile, maxPositions);
                tuner.PrintStatistics();
                
                Console.WriteLine("\nPress Enter to start tuning, or 'q' to quit...");
                string input = Console.ReadLine();
                if (input?.ToLower() == "q")
                    return;

                tuner.RunTuningCycle(iterations);
                tuner.SaveTunedParameters(outputFile);
                tuner.PrintStatistics();
                
                Console.WriteLine($"\nTuning completed! Results saved to {outputFile}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during tuning: {e.Message}");
                Console.WriteLine($"Stack trace: {e.StackTrace}");
            }
        }

        private static void TestEngine(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: EPD file path required");
                return;
            }

            string epdFile = args[1];
            int maxPositions = 1000;

            if (args.Length > 2 && int.TryParse(args[2], out int max))
            {
                maxPositions = max;
            }

            Console.WriteLine($"Testing engine on {epdFile} (max {maxPositions} positions)...");

            var tuner = new TexelTuner();
            tuner.LoadPositionsFromEPD(epdFile);
            tuner.PrintStatistics();

            // Test a subset of positions
            Console.WriteLine("\nSample evaluations:");
            var positions = tuner.GetSamplePositions(Math.Min(10, maxPositions));
            
            foreach (var pos in positions)
            {
                Console.WriteLine($"FEN: {pos.Fen}");
                Console.WriteLine($"Expected result: {pos.Result} | Engine eval: {pos.Evaluation}");
                Console.WriteLine($"Sigmoid: {1.0 / (1.0 + Math.Exp(-pos.Evaluation / 400.0)):F4}");
                Console.WriteLine();
            }
        }

        private static void ConvertEPD(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Error: Input and output file paths required");
                return;
            }

            string inputFile = args[1];
            string outputFile = args[2];

            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Error: Input file not found: {inputFile}");
                return;
            }

            Console.WriteLine($"Converting {inputFile} to {outputFile}...");

            try
            {
                var lines = File.ReadAllLines(inputFile);
                var convertedLines = new string[lines.Length];
                int processed = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    {
                        convertedLines[i] = line;
                        continue;
                    }

                    // Basic EPD conversion - ensure proper format
                    // This is a placeholder - you can implement specific conversion logic here
                    convertedLines[i] = line;
                    processed++;

                    if (processed % 1000 == 0)
                    {
                        Console.WriteLine($"Processed {processed} lines...");
                    }
                }

                File.WriteAllLines(outputFile, convertedLines);
                Console.WriteLine($"Conversion completed. Processed {processed} positions.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during conversion: {e.Message}");
            }
        }
    }
}

// Extension methods and utilities for the tuner
public static class TunerUtilities
{
    public static void CreateSampleQuietEPD(string outputPath, int numPositions = 1000)
    {
        Console.WriteLine($"Creating sample quiet EPD file with {numPositions} positions...");
        EPDProcessor.CreateSampleEPD(outputPath, numPositions);
    }
}