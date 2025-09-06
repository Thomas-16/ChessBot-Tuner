using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

public class TexelTuner
{
    // Tuning data structure
    public struct TuningPosition
    {
        public Board Board;
        public double Result; // 0.0 = black wins, 0.5 = draw, 1.0 = white wins
        public string Fen;
        public float Evaluation; // Current evaluation from engine
    }
    
    // Parameter metadata
    public struct TunableParam
    {
        public string Name;
        public FieldInfo Field;
        public float Value;
        public float MinValue;
        public float MaxValue;
        public double Gradient;
    }

    private List<TuningPosition> positions = new();
    private List<TunableParam> parameters = new();
    private double K = 0.5; // Scaling factor for sigmoid
    private double learningRate = 10000;
    private int batchSize = 10000;
    private double bestError = double.MaxValue;
    private Dictionary<string, float> bestParameters = new();
    
    // Texel tuning configuration
    private bool useAdaptiveLearningRate = true;
    private double minLearningRate = 0.0001;
    private double maxLearningRate = 100000;
    private double momentum = 0.9;
    private Dictionary<string, double> velocities = new();
    
    public TexelTuner()
    {
        InitializeParameters();
    }

    private void InitializeParameters()
    {
        Type paramType = typeof(TunableParameters);
        var fields = paramType.GetFields(BindingFlags.Public | BindingFlags.Static);
        
        foreach (var field in fields)
        {
            if (field.GetCustomAttribute<TunableAttribute>() != null)
            {
                var param = new TunableParam
                {
                    Name = field.Name,
                    Field = field,
                    Value = (float)field.GetValue(null),
                    MinValue = GetMinValueForParameter(field.Name),
                    MaxValue = GetMaxValueForParameter(field.Name),
                    Gradient = 0.0
                };
                
                parameters.Add(param);
                velocities[param.Name] = 0.0;
            }
        }
        
        Console.WriteLine($"Initialized {parameters.Count} tunable parameters");
    }

    private float GetMinValueForParameter(string paramName)
    {
        // Set reasonable bounds for different parameter types
        if (paramName.Contains("Value"))
        {
            if (paramName.Contains("Pawn")) return 50;
            if (paramName.Contains("Knight") || paramName.Contains("Bishop")) return 200;
            if (paramName.Contains("Rook")) return 300;
            if (paramName.Contains("Queen")) return 600;
        }
        
        if (paramName.Contains("PST"))
        {
            return -100; // Piece square table values
        }
        
        if (paramName.Contains("Bonus"))
        {
            return 0;
        }
        
        if (paramName.Contains("Penalty"))
        {
            return -200;
        }
        
        return -100; // Default minimum
    }

    private float GetMaxValueForParameter(string paramName)
    {
        // Set reasonable bounds for different parameter types
        if (paramName.Contains("Value"))
        {
            if (paramName.Contains("Pawn")) return 150;
            if (paramName.Contains("Knight") || paramName.Contains("Bishop")) return 500;
            if (paramName.Contains("Rook")) return 800;
            if (paramName.Contains("Queen")) return 1200;
        }
        
        if (paramName.Contains("PST"))
        {
            return 100; // Piece square table values
        }
        
        if (paramName.Contains("Bonus"))
        {
            return 200;
        }
        
        if (paramName.Contains("Penalty"))
        {
            return 0;
        }
        
        return 100; // Default maximum
    }

    public void LoadPositionsFromEPD(string epdFilePath, int maxPositions = 500000)
    {
        Console.WriteLine($"Loading positions from {epdFilePath} (max: {maxPositions})...");
        
        var epdPositions = EPDProcessor.ParseEPDFile(epdFilePath);
        int loaded = 0;
        int skipped = 0;

        foreach (var epdPos in epdPositions)
        {
            // Stop loading if we've reached the maximum
            if (loaded >= maxPositions)
            {
                Console.WriteLine($"Reached maximum position limit: {maxPositions}");
                break;
            }

            try
            {
                if (epdPos.GameResult.HasValue)
                {
                    var position = new TuningPosition
                    {
                        Board = Board.CreateBoardFromFEN(epdPos.Fen),
                        Result = epdPos.GameResult.Value,
                        Fen = epdPos.Fen,
                        Evaluation = 0
                    };
                    
                    positions.Add(position);
                    loaded++;
                }
                else
                {
                    skipped++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading position {epdPos.Fen}: {e.Message}");
                skipped++;
            }

            if (loaded % 10000 == 0 && loaded > 0)
            {
                Console.WriteLine($"Loaded {loaded} positions...");
            }
        }

        Console.WriteLine($"Loaded {loaded} positions, skipped {skipped}");
        
        // Evaluate all positions with current parameters
        EvaluateAllPositions();
    }

    // Method to get sample positions for testing
    public List<TuningPosition> GetSamplePositions(int count)
    {
        var random = new Random();
        return positions.OrderBy(x => random.Next()).Take(count).ToList();
    }

    private void EvaluateAllPositions()
    {
        Console.WriteLine("Evaluating all positions...");
        
        TunableParameters.UpdateArrays(); // Make sure arrays are updated
        
        var tasks = new List<Task>();
        int batchSize = Math.Max(1, positions.Count / Environment.ProcessorCount);
        
        for (int i = 0; i < positions.Count; i += batchSize)
        {
            int start = i;
            int end = Math.Min(i + batchSize, positions.Count);
            
            tasks.Add(Task.Run(() =>
            {
                for (int j = start; j < end; j++)
                {
                    var pos = positions[j];
                    pos.Evaluation = Evaluation.Evaluate(pos.Board);
                    positions[j] = pos;
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine("Finished evaluating positions");
    }

    public void OptimizeK()
    {
        Console.WriteLine("Optimizing K parameter...");
        
        double bestK = K;
        double bestKError = CalculateError();
        
        // Try different K values
        for (double k = 0.005; k <= 0.02; k += 0.001)
        {
            K = k;
            double error = CalculateError();
            Console.WriteLine($"K = {k:F2}: Error = {error:F6}");
            
            if (error < bestKError)
            {
                bestKError = error;
                bestK = k;
            }
        }
        
        K = bestK;
        Console.WriteLine($"Optimal K = {K:F2}, Error = {bestKError:F6}");
    }

    private double CalculateError()
    {
        if (positions.Count == 0)
            return double.MaxValue;

        double totalError = 0.0;
        
        foreach (var pos in positions)
        {
            double sigmoid = Sigmoid(pos.Evaluation);
            if (double.IsNaN(sigmoid) || double.IsInfinity(sigmoid))
            {
                Console.WriteLine($"Warning: Invalid sigmoid value {sigmoid} for evaluation {pos.Evaluation}");
                continue;
            }
            
            double diff = sigmoid - pos.Result;
            totalError += diff * diff;
        }
        
        double error = totalError / positions.Count;
        return double.IsNaN(error) || double.IsInfinity(error) ? double.MaxValue : error;
    }

    private double Sigmoid(float evaluation)
    {
        return 1.0 / (1.0 + Math.Exp(-K * evaluation / 400.0));
    }

    public void RunTuningCycle(int maxIterations = 100)
    {
        if (positions.Count == 0)
        {
            Console.WriteLine("Error: No positions loaded for tuning.");
            return;
        }

        if (parameters.Count == 0)
        {
            Console.WriteLine("Error: No tunable parameters found.");
            return;
        }

        Console.WriteLine($"Starting Texel tuning with {positions.Count} positions...");
        Console.WriteLine($"Learning rate: {learningRate}, Batch size: {batchSize}");
        
        OptimizeK(); // First optimize K
        bestError = CalculateError();
        SaveBestParameters();
        
        Console.WriteLine($"Initial error: {bestError:F6}");
        
        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            Console.WriteLine($"\nIteration {iteration + 1}/{maxIterations}");
            
            // Re-optimize K every 5 iterations
            if (iteration > 0 && iteration % 5 == 0)
            {
                Console.WriteLine("Re-optimizing K parameter...");
                OptimizeK();
            }
            
            // Calculate gradients
            CalculateGradients(iteration);
            
            // Update parameters with momentum
            UpdateParameters();
            
            // Re-evaluate positions with new parameters
            EvaluateAllPositions();
            
            // Calculate new error
            double currentError = CalculateError();
            Console.WriteLine($"Error: {currentError:F6} (Best: {bestError:F6})");
            
            if (currentError < bestError)
            {
                bestError = currentError;
                SaveBestParameters();
                Console.WriteLine($"New best error: {bestError:F6}");
                
                // Adaptive learning rate - increase if we're improving
                if (useAdaptiveLearningRate)
                {
                    learningRate = Math.Min(learningRate * 1.05, maxLearningRate);
                }
            }
            else
            {
                // If error increased, restore best parameters and reduce learning rate
                RestoreBestParameters();
                EvaluateAllPositions();
                
                if (useAdaptiveLearningRate)
                {
                    learningRate = Math.Max(learningRate * 0.9, minLearningRate);
                    Console.WriteLine($"Learning rate adjusted to: {learningRate:F6}");
                }
            }
            
            // Early stopping if learning rate gets too small
            if (learningRate < minLearningRate * 1.1)
            {
                Console.WriteLine("Learning rate too small, stopping early");
                break;
            }
        }
        
        RestoreBestParameters();
        Console.WriteLine($"\nTuning completed. Final error: {bestError:F6}");
        PrintParameterSummary();
    }

    private void CalculateGradients(int iteration = 0)
    {
        Console.WriteLine("Calculating gradients...");
        
        // Reset gradients
        for (int i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];
            param.Gradient = 0.0;
            parameters[i] = param;
        }
        
        // Alternate between focused and random batches to escape local minima
        var batchPositions = new List<TuningPosition>();
        if (iteration % 3 == 0) // Every 3rd iteration, use random positions
        {
            var random = new Random();
            batchPositions = positions.OrderBy(x => random.Next()).Take(batchSize).ToList();
        }
        else // Use positions closest to 0.5 (most informative)
        {
            batchPositions = positions
                .OrderBy(p => Math.Abs(p.Result - 0.5))
                .Take(batchSize)
                .ToList();
        }
        
        for (int paramIndex = 0; paramIndex < parameters.Count; paramIndex++)
        {
            var param = parameters[paramIndex];
            double gradient = 0.0;
            
            // Calculate finite difference gradient
            float originalValue = param.Value;
            
            // Use much larger step size for better gradient detection
            float stepSize = Math.Max(5.0f, Math.Abs(originalValue) * 0.1f); // 10% of current value, minimum 5.0
            stepSize = Math.Max(stepSize, 20.0f); // But at least 20.0 for strong signal
            
            // Positive perturbation
            SetParameter(param.Name, originalValue + stepSize);
            TunableParameters.UpdateArrays();
            double errorPlus = CalculateErrorForBatch(batchPositions);
            
            // Negative perturbation
            SetParameter(param.Name, originalValue - stepSize);
            TunableParameters.UpdateArrays();
            double errorMinus = CalculateErrorForBatch(batchPositions);
            
            // Debug: Check if evaluations actually change
            if (paramIndex < 3) // Only for first few params to avoid spam
            {
                // Test evaluation of first position
                var testPos = batchPositions[0];
                float evalWithChange = Evaluation.Evaluate(testPos.Board);
                SetParameter(param.Name, originalValue);
                TunableParameters.UpdateArrays();
                float evalOriginal = Evaluation.Evaluate(testPos.Board);
                Console.WriteLine($"      Eval test: original={evalOriginal}, with change={evalWithChange}, diff={Math.Abs(evalWithChange - evalOriginal)}");
                // Don't restore here, we'll do it below
            }
            
            // Restore original value
            SetParameter(param.Name, originalValue);
            TunableParameters.UpdateArrays();
            
            // Calculate gradient (divide by 2*stepSize for correct scaling)
            gradient = (errorPlus - errorMinus) / (2.0 * stepSize);
            
            // Amplify very small gradients to prevent them from being lost
            if (Math.Abs(gradient) < 0.00001)
            {
                gradient *= 1000.0; // Amplify tiny gradients
            }
            else if (Math.Abs(gradient) < 0.0001)
            {
                gradient *= 100.0; // Amplify small gradients
            }
            
            // Debug output for first few parameters
            if (paramIndex < 5)
            {
                Console.WriteLine($"    Param {param.Name}: orig={originalValue}, step={stepSize}, errorPlus={errorPlus:F8}, errorMinus={errorMinus:F8}, gradient={gradient:F8}");
            }
            
            // Update parameter gradient
            var updatedParam = parameters[paramIndex];
            updatedParam.Gradient = gradient;
            parameters[paramIndex] = updatedParam;
        }
    }

    private double CalculateErrorForBatch(List<TuningPosition> batchPositions)
    {
        double totalError = 0.0;
        
        foreach (var pos in batchPositions)
        {
            float evaluation = Evaluation.Evaluate(pos.Board);
            double sigmoid = Sigmoid(evaluation);
            double diff = sigmoid - pos.Result;
            totalError += diff * diff;
        }
        
        return totalError / batchPositions.Count;
    }

    private void UpdateParameters()
    {
        int updatedCount = 0;
        double totalGradientMagnitude = 0.0;
        
        for (int i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];
            totalGradientMagnitude += Math.Abs(param.Gradient);
            
            float originalValue = param.Value;
            
            // Apply momentum
            double velocity = momentum * velocities[param.Name] - learningRate * param.Gradient;
            velocities[param.Name] = velocity;
            
            // Update parameter value
            float newValue = param.Value + (float)velocity;
            
            // Clamp to bounds
            newValue = Math.Max(param.MinValue, Math.Min(param.MaxValue, newValue));
            
            SetParameter(param.Name, newValue);
            
            param.Value = newValue;
            parameters[i] = param;
            
            if (newValue != originalValue)
            {
                updatedCount++;
                if (updatedCount <= 5) // Debug first 5 parameter changes
                {
                    Console.WriteLine($"    Updated {param.Name}: {originalValue} -> {newValue} (gradient: {param.Gradient:F8}, velocity: {velocity:F8})");
                }
            }
        }
        
        TunableParameters.UpdateArrays();
        Console.WriteLine($"Parameters: {updatedCount}/{parameters.Count} changed, avg gradient magnitude: {totalGradientMagnitude / Math.Max(1, parameters.Count):F8}");
    }

    private void SetParameter(string name, float value)
    {
        var param = parameters.FirstOrDefault(p => p.Name == name);
        if (param.Field != null)
        {
            param.Field.SetValue(null, value);
        }
    }

    private void SaveBestParameters()
    {
        bestParameters.Clear();
        foreach (var param in parameters)
        {
            bestParameters[param.Name] = param.Value;
        }
    }

    private void RestoreBestParameters()
    {
        foreach (var kvp in bestParameters)
        {
            SetParameter(kvp.Key, kvp.Value);
        }
        TunableParameters.UpdateArrays();
        
        // Update parameter list
        for (int i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];
            if (bestParameters.ContainsKey(param.Name))
            {
                param.Value = bestParameters[param.Name];
                parameters[i] = param;
            }
        }
    }

    private void PrintParameterSummary()
    {
        Console.WriteLine("\n=== Parameter Summary ===");
        
        var sortedParams = parameters.OrderBy(p => Math.Abs(p.Gradient)).Reverse().ToList();
        
        foreach (var param in sortedParams.Take(20)) // Show top 20 most significant
        {
            Console.WriteLine($"{param.Name}: {param.Value} (gradient: {param.Gradient:F6})");
        }
    }

    public void SaveTunedParameters(string filePath)
    {
        Console.WriteLine($"Saving tuned parameters to {filePath}...");
        
        var lines = new List<string>();
        lines.Add("// Tuned parameters generated by Texel tuning");
        lines.Add($"// Final error: {bestError:F6}");
        lines.Add($"// K parameter: {K:F2}");
        lines.Add("");
        
        foreach (var param in parameters.OrderBy(p => p.Name))
        {
            lines.Add($"public static float {param.Name} = {param.Value}f;");
        }
        
        File.WriteAllLines(filePath, lines);
        Console.WriteLine("Parameters saved successfully");
    }

    public void PrintStatistics()
    {
        Console.WriteLine("\n=== Tuning Statistics ===");
        Console.WriteLine($"Total positions: {positions.Count}");
        Console.WriteLine($"White wins: {positions.Count(p => p.Result == 1.0)}");
        Console.WriteLine($"Draws: {positions.Count(p => p.Result == 0.5)}");
        Console.WriteLine($"Black wins: {positions.Count(p => p.Result == 0.0)}");
        Console.WriteLine($"Total tunable parameters: {parameters.Count}");
        Console.WriteLine($"Current error: {CalculateError():F6}");
        Console.WriteLine($"Best error achieved: {bestError:F6}");
        Console.WriteLine($"K parameter: {K:F2}");
    }
}