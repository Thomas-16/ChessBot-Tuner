# C# Chess Engine Evaluation Tuner

A chess engine evaluation tuner I wrote to improve my chess bot's evaluation function using Texel's tuning method.

## Framework

This project is built on top of [Sebastian Lague's Chess Challenge](https://github.com/SebLague/Chess-Challenge) framework. I've adapted and extended the original chess engine framework to create a comprehensive evaluation tuner while maintaining the excellent chess API and board representation from the original project.

## Features

- **Texel Tuning Implementation**: Complete implementation with momentum, adaptive learning rates, and gradient amplification
- **500+ Tunable Parameters**: Material values, piece-square tables, pawn structure, king safety, and more (converted to floats for fine-grained tuning)
- **Multi-Format EPD Support**: Handles quiet-labeled v7, standard EPD, and PGN-style formats
- **Smart Optimization**: Random position shuffling, periodic K-parameter re-optimization, and local minima escape
- **Real-Time Monitoring**: Live progress tracking with parameter change visualization

## How It Works

The tuner uses Texel's method to optimize evaluation parameters:

1. **Load Training Data**: Parses EPD file with labeled positions (win/loss/draw outcomes)
2. **Parameter Extraction**: Identifies all tunable parameters using reflection and attributes
3. **K-Parameter Optimization**: Finds optimal scaling factor for evaluation-to-probability conversion
4. **Gradient Descent**: Iteratively adjusts parameters using finite difference gradients with amplification
5. **Momentum & Adaptive Learning**: Advanced optimization techniques for faster convergence

### Mathematical Foundation

The objective function minimizes mean squared error between predicted and actual results:

```
Error = Σ(sigmoid(eval_i * K) - result_i)² / N
sigmoid(x) = 1 / (1 + e^(-x/400))
```

Where:
- `eval_i` is the engine's evaluation of position i
- `result_i` is the actual game result (0.0/0.5/1.0)  
- `K` is the scaling parameter
- `N` is the number of positions

## Quick Start

### Prerequisites
- .NET 6.0 or later
- EPD training data file (quiet-labeled positions recommended)

### Basic Usage

```bash
# Build the project
dotnet build

# Run basic tuning (uses 500k positions by default)
./Chess-Challenge-Tuner.exe tune your-training-data.epd

# Advanced tuning with custom parameters
./Chess-Challenge-Tuner.exe tune quiet-labeled.epd \
  --iterations 200 \
  --learning-rate 1000 \
  --batch-size 10000 \
  --max-positions 100000 \
  --output optimized_params.cs
```

### Command Line Options

- `--iterations <n>` - Number of tuning iterations (default: 100)
- `--learning-rate <r>` - Initial learning rate (default: 1000) 
- `--batch-size <n>` - Batch size for gradient calculation (default: 10000)
- `--max-positions <n>` - Maximum positions to load (default: 500000)
- `--output <file>` - Output file for tuned parameters (default: tuned_parameters.cs)

## Example Output

```
=== Tuning Statistics ===
Total positions: 500000
White wins: 187234, Draws: 125432, Black wins: 187334
Total tunable parameters: 547
Current error: 0.201234
Best error achieved: 0.198765
K parameter: 0.52

Starting Texel tuning with 500000 positions...
Learning rate: 1000, Batch size: 10000

Iteration 1/400
Calculating gradients...
    Param PawnValueMidgame: orig=100, step=20, gradient=0.00463341
    Param KnightValueMidgame: orig=320, step=32, gradient=0.00125208
    Updated PawnValueMidgame: 100 -> 95.33 (gradient: 0.00463341, velocity: -4.67)
Parameters: 515/547 changed, avg gradient magnitude: 0.00019609
Error: 0.195432 (Best: 0.201234)
New best error: 0.195432
```

## Key Features Implemented

- **Float precision parameters** for fine-grained tuning
- **Gradient amplification** to prevent tiny gradients from being lost
- **Random batch selection** every 3rd iteration to escape local minima  
- **Periodic K re-optimization** every 5-10 iterations
- **Large step sizes** (20+ minimum) for better gradient detection
- **Adaptive learning rate** with momentum
- **Parameter bounds** to prevent unrealistic values

## Training Data

Use quiet position datasets for best results:
- 100k-500k positions recommended
- Balanced win/loss/draw outcomes
- Middle/endgame positions preferred
- No immediate tactics or checks

## Acknowledgments

- **Sebastian Lague** - For the excellent [Chess Challenge](https://github.com/SebLague/Chess-Challenge) framework that this project builds upon
- **Texel Chess Engine** - Original tuning methodology  
- **Chess Programming Community** - Extensive evaluation knowledge and techniques

---