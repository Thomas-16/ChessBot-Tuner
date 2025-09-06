using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public static class EPDProcessor
{
    private static int debugLineCount = 0;

    public struct EPDPosition
    {
        public string Fen;
        public Dictionary<string, string> Operations;
        public double? GameResult; // 0.0 = black wins, 0.5 = draw, 1.0 = white wins
        public string Comment;
    }

    /// <summary>
    /// Parse an EPD (Extended Position Description) file
    /// EPD format: [fen] [operation1 value1; operation2 value2; ...]
    /// </summary>
    public static List<EPDPosition> ParseEPDFile(string filePath)
    {
        var positions = new List<EPDPosition>();
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"EPD file not found: {filePath}");
            return positions;
        }

        string[] lines = File.ReadAllLines(filePath);
        Console.WriteLine($"Processing {lines.Length} lines from EPD file...");

        int processed = 0;
        int errors = 0;

        foreach (string line in lines)
        {
            try
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                var epdPosition = ParseEPDLine(trimmed);
                if (epdPosition.HasValue)
                {
                    positions.Add(epdPosition.Value);
                    processed++;
                }
                else
                {
                    errors++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error parsing line: {line}");
                Console.WriteLine($"Exception: {e.Message}");
                errors++;
            }

            if ((processed + errors) % 5000 == 0)
            {
                Console.WriteLine($"Processed {processed} positions, {errors} errors...");
            }
        }

        Console.WriteLine($"EPD parsing completed: {processed} positions loaded, {errors} errors");
        return positions;
    }

    private static EPDPosition? ParseEPDLine(string line)
    {
        var position = new EPDPosition
        {
            Operations = new Dictionary<string, string>(),
            GameResult = null,
            Comment = ""
        };

        // For this format, the entire line contains FEN + operations mixed together
        // Split into tokens first
        string[] allTokens = line.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (allTokens.Length < 4)
            return null;

        // Find where the FEN ends by looking for 'c9' token
        int fenEndIndex = 4; // Try minimum FEN tokens first
        for (int i = 4; i < Math.Min(8, allTokens.Length); i++)
        {
            if (allTokens[i] == "c9")
            {
                fenEndIndex = i;
                break;
            }
        }

        position.Fen = string.Join(" ", allTokens.Take(fenEndIndex));

        // Validate FEN by trying to create a board
        try
        {
            Board.CreateBoardFromFEN(position.Fen);
        }
        catch
        {
            return null; // Invalid FEN
        }

        // Parse operations from remaining tokens after FEN
        if (allTokens.Length > fenEndIndex)
        {
            string operationsString = string.Join(" ", allTokens.Skip(fenEndIndex));
            ParseEPDOperations(operationsString, ref position);
        }

        // Debug: Print first few lines to understand format
        if (debugLineCount < 5)
        {
            Console.WriteLine($"DEBUG Line {debugLineCount + 1}: {line}");
            Console.WriteLine($"  All tokens: [{string.Join(", ", allTokens)}]");
            Console.WriteLine($"  FEN end index: {fenEndIndex}");
            Console.WriteLine($"  FEN: {position.Fen}");
            Console.WriteLine($"  Operations: {(allTokens.Length > fenEndIndex ? string.Join(" ", allTokens.Skip(fenEndIndex)) : "none")}");
            Console.WriteLine($"  GameResult: {position.GameResult?.ToString() ?? "NULL"}");
            Console.WriteLine($"  Has GameResult: {position.GameResult.HasValue}");
            Console.WriteLine();
            debugLineCount++;
        }

        return position;
    }

    private static void ParseEPDOperations(string operationsString, ref EPDPosition position)
    {
        if (string.IsNullOrEmpty(operationsString))
            return;
        

        // Regular expression to match EPD operations: opcode operand;
        var regex = new Regex(@"(\w+)\s+([^;]+);?", RegexOptions.IgnoreCase);
        var matches = regex.Matches(operationsString);
        
        foreach (Match match in matches)
        {
            string opcode = match.Groups[1].Value.Trim();
            string operand = match.Groups[2].Value.Trim();

            position.Operations[opcode] = operand;

            // Handle common opcodes
            switch (opcode.ToLower())
            {
                case "c0": // Black wins
                    position.GameResult = 0.0;
                    break;
                case "c1": // White wins  
                    position.GameResult = 1.0;
                    break;
                case "c2": // Draw
                    position.GameResult = 0.5;
                    break;
                case "c9": // Comment or result in quotes
                    position.Comment = operand.Trim('"');
                    // Parse the result from c9 comment
                    string cleanOperand = operand.Trim('"');
                    if (cleanOperand == "0-1")
                        position.GameResult = 0.0;
                    else if (cleanOperand == "1-0")
                        position.GameResult = 1.0;
                    else if (cleanOperand == "1/2-1/2")
                        position.GameResult = 0.5;
                    break;
            }
        }

        // Also handle simple format without opcodes (just result values)
        string[] tokens = operationsString.Split(new char[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string token in tokens)
        {
            switch (token.Trim())
            {
                case "c0":
                    position.GameResult = 0.0;
                    break;
                case "c1":
                    position.GameResult = 1.0;
                    break;
                case "c2":
                    position.GameResult = 0.5;
                    break;
            }
        }

        // Parse PGN-style result tags
        if (operationsString.Contains("\"1-0\""))
            position.GameResult = 1.0;
        else if (operationsString.Contains("\"0-1\""))
            position.GameResult = 0.0;
        else if (operationsString.Contains("\"1/2-1/2\""))
            position.GameResult = 0.5;

        // Parse quiet-labeled v7 format patterns
        // Look for numeric results: 0.0, 0.5, 1.0
        var numericResultRegex = new Regex(@"\b(0\.0|0\.5|1\.0)\b");
        var numericMatch = numericResultRegex.Match(operationsString);
        if (numericMatch.Success)
        {
            if (double.TryParse(numericMatch.Value, out double result))
            {
                position.GameResult = result;
            }
        }

        // Also check for integer results: 0, 1 (for draws might be represented differently)
        if (!position.GameResult.HasValue)
        {
            if (operationsString.Contains(" 1 ") || operationsString.EndsWith(" 1"))
                position.GameResult = 1.0;
            else if (operationsString.Contains(" 0 ") || operationsString.EndsWith(" 0"))
                position.GameResult = 0.0;
        }
    }

    /// <summary>
    /// Convert quiet-labeled v7 format to standard EPD
    /// </summary>
    public static void ConvertQuietLabeledToEPD(string inputFile, string outputFile)
    {
        Console.WriteLine($"Converting {inputFile} to standard EPD format...");
        
        var lines = File.ReadAllLines(inputFile);
        var convertedLines = new List<string>();
        
        int processed = 0;
        int converted = 0;

        foreach (string line in lines)
        {
            processed++;
            string trimmed = line.Trim();
            
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
            {
                convertedLines.Add(line);
                continue;
            }

            try
            {
                string convertedLine = ConvertQuietLabeledLine(trimmed);
                if (!string.IsNullOrEmpty(convertedLine))
                {
                    convertedLines.Add(convertedLine);
                    converted++;
                }
            }
            catch
            {
                // Keep original line if conversion fails
                convertedLines.Add(line);
            }

            if (processed % 1000 == 0)
            {
                Console.WriteLine($"Processed {processed} lines, converted {converted}...");
            }
        }

        File.WriteAllLines(outputFile, convertedLines);
        Console.WriteLine($"Conversion completed: {converted} lines converted out of {processed}");
    }

    private static string ConvertQuietLabeledLine(string line)
    {
        // Quiet-labeled format typically has: FEN + result value
        // Convert to proper EPD format with result operations
        
        string[] parts = line.Split(' ');
        if (parts.Length < 6)
            return line; // Not a valid format

        // Extract FEN (first 6 parts)
        string fen = string.Join(" ", parts.Take(6));
        
        // Look for result indicators in remaining parts
        var remainingParts = parts.Skip(6).ToList();
        string result = "";
        
        // Check for numeric results (0, 0.5, 1) or string results
        foreach (string part in remainingParts)
        {
            if (part == "0" || part == "0.0")
                result = "c0";
            else if (part == "1" || part == "1.0")
                result = "c1";
            else if (part == "0.5")
                result = "c2";
            else if (part == "c0" || part == "c1" || part == "c2")
                result = part;
        }

        // Return FEN with proper EPD result operation
        if (!string.IsNullOrEmpty(result))
            return $"{fen} {result};";
        else
            return line; // Return original if no result found
    }

    /// <summary>
    /// Filter positions to keep only quiet positions (no captures, checks, or tactical shots)
    /// </summary>
    public static List<EPDPosition> FilterQuietPositions(List<EPDPosition> positions)
    {
        var quietPositions = new List<EPDPosition>();
        
        Console.WriteLine($"Filtering {positions.Count} positions for quiet positions...");
        
        foreach (var pos in positions)
        {
            try
            {
                Board board = Board.CreateBoardFromFEN(pos.Fen);
                
                if (IsQuietPosition(board))
                {
                    quietPositions.Add(pos);
                }
            }
            catch
            {
                // Skip invalid positions
                continue;
            }
        }
        
        Console.WriteLine($"Filtered to {quietPositions.Count} quiet positions");
        return quietPositions;
    }

    private static bool IsQuietPosition(Board board)
    {
        // Check if position is "quiet" (no immediate tactics)
        
        // 1. Not in check
        if (board.IsInCheck())
            return false;
        
        // 2. No captures available that win material
        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            if (move.IsCapture)
            {
                // Check if this capture wins material significantly
                PieceType capturedPiece = move.CapturePieceType;
                PieceType movingPiece = move.MovePieceType;
                
                // If capturing piece is worth less than captured piece, might not be quiet
                int[] pieceValues = { 0, 100, 300, 300, 500, 900, 0 };
                int captureValue = pieceValues[(int)capturedPiece] - pieceValues[(int)movingPiece];
                
                if (captureValue > 100) // Significant material gain
                    return false;
            }
            
            // 3. No immediate mate threats
            if (move.IsPromotion)
                return false;
        }
        
        // 4. Position should be in middle/endgame (not early opening)
        if (board.PlyCount < 16) // First 8 moves for each side
            return false;
        
        return true;
    }

    /// <summary>
    /// Create a sample EPD file for testing
    /// </summary>
    public static void CreateSampleEPD(string outputFile, int numPositions = 1000)
    {
        Console.WriteLine($"Creating sample EPD file with {numPositions} positions...");
        
        var lines = new List<string>();
        var random = new Random();
        
        // Sample FENs with results - these would normally come from a game database
        string[] sampleFens = {
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Starting position
            "r1bqkb1r/pppp1ppp/2n2n2/1B2p3/4P3/3P1N2/PPP2PPP/RNBQK2R b KQkq - 0 4",
            "r1bqk2r/pppp1ppp/2n2n2/2b1p3/2B1P3/3P1N2/PPP2PPP/RNBQK2R w KQkq - 2 5",
            "r1bq1rk1/pppp1ppp/2n2n2/2b1p3/2B1P3/3P1N2/PPP2PPP/RNBQ1RK1 b - - 5 6",
            "8/8/8/8/8/8/8/K7 w - - 0 1", // King vs King
            "8/8/8/8/8/8/P7/K6k b - - 0 1", // King and pawn vs King
        };
        
        for (int i = 0; i < numPositions; i++)
        {
            string fen = sampleFens[random.Next(sampleFens.Length)];
            
            // Assign random result (weighted towards draws in middle game)
            double rand = random.NextDouble();
            string result;
            if (rand < 0.1) result = "c0"; // Black wins
            else if (rand < 0.2) result = "c1"; // White wins  
            else result = "c2"; // Draw
            
            lines.Add($"{fen} {result};");
        }
        
        File.WriteAllLines(outputFile, lines);
        Console.WriteLine($"Sample EPD file created: {outputFile}");
    }
}