using ChessChallenge.API;
using System;

public static class Evaluation
{
    // Use tunable parameters instead of static values
    public static float[] PieceValues => TunableParameters.PieceValues;
    public static float[] PieceValuesEndGame => TunableParameters.PieceValuesEndGame;

    static readonly ulong centerSquares = ((ulong)1 << new Square("e4").Index)
                          | ((ulong)1 << new Square("d4").Index)
                          | ((ulong)1 << new Square("e5").Index)
                          | ((ulong)1 << new Square("d5").Index)
                          | ((ulong)1 << new Square("c3").Index)
                          | ((ulong)1 << new Square("f3").Index)
                          | ((ulong)1 << new Square("c6").Index)
                          | ((ulong)1 << new Square("f6").Index)
                          | ((ulong)1 << new Square("c4").Index)
                          | ((ulong)1 << new Square("f4").Index)
                          | ((ulong)1 << new Square("c5").Index)
                          | ((ulong)1 << new Square("f5").Index)
                          | ((ulong)1 << new Square("d3").Index)
                          | ((ulong)1 << new Square("e3").Index)
                          | ((ulong)1 << new Square("d6").Index)
                          | ((ulong)1 << new Square("e6").Index);
    
    static ulong[] fileMasks = {
        0x0101010101010101, // File A
        0x0202020202020202, // File B
        0x0404040404040404, // File C
        0x0808080808080808, // File D
        0x1010101010101010, // File E
        0x2020202020202020, // File F
        0x4040404040404040, // File G
        0x8080808080808080  // File H
    };
    
    // Use tunable parameters
    static float[] passedPawnBonuses => TunableParameters.PassedPawnBonuses;
    static float[] isolatedPawnPenaltyByCount => TunableParameters.IsolatedPawnPenaltyByCount;
    static float[] kingPawnShieldScores => TunableParameters.KingPawnShieldScores;
    
    static float pawnCenterControlScore => TunableParameters.PawnCenterControlScore;
    static float pieceCenterControlScore => TunableParameters.PieceCenterControlScore;
    static float maxUnCastledKingPenalty => TunableParameters.MaxUncastledKingPenalty;
    static float semiOpenKingFilePenalty => TunableParameters.SemiOpenKingFilePenalty;
    static float semiOpenNonKingFilePenalty => TunableParameters.SemiOpenNonKingFilePenalty;
    static float fullyOpenKingFilePenalty => TunableParameters.FullyOpenKingFilePenalty;
    static float fullyOpenNonKingFilePenalty => TunableParameters.FullyOpenNonKingFilePenalty;
    static float mopUpKingDistanceMultiplier => TunableParameters.MopUpKingDistanceMultiplier;
    static float mopUpOpponentKingDistanceToCenterMultiplier => TunableParameters.MopUpOpponentKingDistanceToCenterMultiplier;

    
    // Performs static evaluation of the current position.
    // The position is assumed to be 'quiet', i.e. no captures are available that could drastically affect the evaluation.
    // The score that's returned is given from the perspective of whoever's turn it is to move.
    // So a positive score means the player whose turn it is to move has an advantage, while a negative score indicates a disadvantage.
    public static float Evaluate(Board board)
    {
        if (board.IsInCheckmate())
        {
            // Return from the perspective of the side to move
            return -MainBot.immediateMateScore;  // Being checkmated is bad
        }
        
        if (board.IsDraw())
        {
            return 0;
        }
        
        float evaluation = 0;
        
        // Calculate game phase (0 = opening, 1 = endgame)
        // Based on remaining material
        float endGameWeight = CalculateGamePhase(board);
        float openingWeight = 1f - endGameWeight;
        
        // Get all piece bitboards once for efficiency
        ulong whitePawns = board.GetPieceBitboard(PieceType.Pawn, true);
        ulong blackPawns = board.GetPieceBitboard(PieceType.Pawn, false);
        ulong whiteKnights = board.GetPieceBitboard(PieceType.Knight, true);
        ulong blackKnights = board.GetPieceBitboard(PieceType.Knight, false);
        ulong whiteBishops = board.GetPieceBitboard(PieceType.Bishop, true);
        ulong blackBishops = board.GetPieceBitboard(PieceType.Bishop, false);
        ulong whiteRooks = board.GetPieceBitboard(PieceType.Rook, true);
        ulong blackRooks = board.GetPieceBitboard(PieceType.Rook, false);
        ulong whiteQueens = board.GetPieceBitboard(PieceType.Queen, true);
        ulong blackQueens = board.GetPieceBitboard(PieceType.Queen, false);
        
        // Evaluate material
        float whiteMaterialEval = EvaluateMaterial(whitePawns, PieceType.Pawn, openingWeight, endGameWeight) +
                                  EvaluateMaterial(whiteKnights, PieceType.Knight, openingWeight, endGameWeight) +
                                  EvaluateMaterial(whiteBishops, PieceType.Bishop, openingWeight, endGameWeight) +
                                  EvaluateMaterial(whiteRooks, PieceType.Rook, openingWeight, endGameWeight) +
                                  EvaluateMaterial(whiteQueens, PieceType.Queen, openingWeight, endGameWeight);
        float blackMaterialEval = EvaluateMaterial(blackPawns, PieceType.Pawn, openingWeight, endGameWeight) +
                                  EvaluateMaterial(blackKnights, PieceType.Knight, openingWeight, endGameWeight) +
                                  EvaluateMaterial(blackBishops, PieceType.Bishop, openingWeight, endGameWeight) +
                                  EvaluateMaterial(blackRooks, PieceType.Rook, openingWeight, endGameWeight) +
                                  EvaluateMaterial(blackQueens, PieceType.Queen, openingWeight, endGameWeight);
        evaluation += whiteMaterialEval - blackMaterialEval;
        
        // Evaluate piece square table
        float whitePieceSquareTableEval = EvaluatePieceSquareTable(whitePawns, true, PieceType.Pawn, openingWeight, endGameWeight) +
                                          EvaluatePieceSquareTable(whiteKnights, true, PieceType.Knight, openingWeight, endGameWeight) +
                                          EvaluatePieceSquareTable(whiteBishops, true, PieceType.Bishop, openingWeight, endGameWeight) +
                                          EvaluatePieceSquareTable(whiteRooks, true, PieceType.Rook, openingWeight, endGameWeight) +
                                          EvaluatePieceSquareTable(whiteQueens, true, PieceType.Queen, openingWeight, endGameWeight);
        float blackPieceSquareTableEval = EvaluatePieceSquareTable(blackPawns, false, PieceType.Pawn, openingWeight, endGameWeight) +
                                          EvaluatePieceSquareTable(blackKnights, false, PieceType.Knight, openingWeight, endGameWeight) +
                                          EvaluatePieceSquareTable(blackBishops, false, PieceType.Bishop, openingWeight, endGameWeight) +
                                          EvaluatePieceSquareTable(blackRooks, false, PieceType.Rook, openingWeight, endGameWeight) +
                                          EvaluatePieceSquareTable(blackQueens, false, PieceType.Queen, openingWeight, endGameWeight);
        evaluation += whitePieceSquareTableEval - blackPieceSquareTableEval;
        
        // King evaluation - get squares and evaluate normally
        Square whiteKingSquare = board.GetKingSquare(true);
        Square blackKingSquare = board.GetKingSquare(false);
        
        float whiteKingValue = GetPieceSquareValue(PieceType.King, GetSquareIndexSided(whiteKingSquare, true), openingWeight, endGameWeight);
        float blackKingValue = GetPieceSquareValue(PieceType.King, GetSquareIndexSided(blackKingSquare, false), openingWeight, endGameWeight);
        evaluation += whiteKingValue - blackKingValue;

        ulong whitePiecesWithoutKing = board.WhitePiecesBitboard & ~board.GetPieceBitboard(PieceType.King, true);
        ulong blackPiecesWithoutKing = board.BlackPiecesBitboard & ~board.GetPieceBitboard(PieceType.King, false);
        
        // Mop-up eval for endgame
        float whiteMopUpEval = EvaluateMopUp(whiteMaterialEval, blackMaterialEval, whiteKingSquare, blackKingSquare, endGameWeight);
        float blackMopUpEval = EvaluateMopUp(blackMaterialEval, whiteMaterialEval, blackKingSquare, whiteKingSquare, endGameWeight);
        evaluation += whiteMopUpEval - blackMopUpEval;

        // Center control
        float whiteCenterControl = BitboardHelper.GetNumberOfSetBits((whitePiecesWithoutKing & ~whitePawns) & centerSquares) * pieceCenterControlScore
                                   + BitboardHelper.GetNumberOfSetBits(whitePawns & centerSquares) * pawnCenterControlScore;
        float blackCenterControl = BitboardHelper.GetNumberOfSetBits((blackPiecesWithoutKing & ~blackPawns) & centerSquares) * pieceCenterControlScore
                                   + BitboardHelper.GetNumberOfSetBits(blackPawns & centerSquares) * pawnCenterControlScore;
        
        // Apply with opening weight (important early, less important in endgame)
        evaluation += (whiteCenterControl - blackCenterControl) * openingWeight;
        
        // Passed pawns and isolated pawns
        float whitePawnScore = EvaluatePassedAndIsolatedPawns(whitePawns, blackPawns, true);
        float blackPawnScore = EvaluatePassedAndIsolatedPawns(blackPawns, whitePawns, false);
        evaluation += whitePawnScore - blackPawnScore;
        
        // King safety
        int whiteNumRooks = BitboardHelper.GetNumberOfSetBits(whiteRooks);
        int whiteNumQueens = BitboardHelper.GetNumberOfSetBits(whiteQueens);
        int blackNumRooks = BitboardHelper.GetNumberOfSetBits(blackRooks);
        int blackNumQueens = BitboardHelper.GetNumberOfSetBits(blackQueens);
        float whiteKingSafetyEval = EvaluateKingSafety(true, whiteKingSquare.Index, whitePawns, blackPawns, blackPieceSquareTableEval, blackNumRooks, blackNumQueens, openingWeight);
        float blackKingSafetyEval = EvaluateKingSafety(false, blackKingSquare.Index, blackPawns, whitePawns, whitePieceSquareTableEval, whiteNumRooks, whiteNumQueens, openingWeight);
        evaluation += whiteKingSafetyEval - blackKingSafetyEval;
        
        
        return board.IsWhiteToMove ? evaluation : -evaluation;
    }

    private static float EvaluateMaterial(ulong pieces, PieceType pieceType, float openingWeight, float endGameWeight)
    {
        // Get piece values for both game phases
        float pieceValueOpening = PieceValues[(int)pieceType];
        float pieceValueEndgame = PieceValuesEndGame[(int)pieceType];
    
        // Blend the piece value based on game phase
        float blendedPieceValue = pieceValueOpening * openingWeight + pieceValueEndgame * endGameWeight;
    
        int pieceCount = BitboardHelper.GetNumberOfSetBits(pieces);
        return blendedPieceValue * pieceCount;
    }

    private static float EvaluatePieceSquareTable(ulong pieces, bool isWhite, PieceType pieceType, float openingWeight, float endGameWeight)
    {
        float evaluation = 0;
        
        ulong piecesCopy = pieces;
        while (piecesCopy != 0)
        {
            int squareIndex = BitboardHelper.ClearAndGetIndexOfLSB(ref piecesCopy);
            int adjustedSquareIndex = GetSquareIndexSided(squareIndex, isWhite);
            
            // Get piece-square value with game phase blending
            float pieceSquareValue = GetPieceSquareValue(pieceType, adjustedSquareIndex, openingWeight, endGameWeight);
            evaluation += pieceSquareValue;
        }
        
        return evaluation;
    }
    
    private static float EvaluatePassedAndIsolatedPawns(ulong ownPawns, ulong opponentPawns, bool isWhite)
    {
        float evaluation = 0;
        int numIsolatedPawns = 0;
        
        ulong pawnsCopy = ownPawns;
        while (pawnsCopy != 0)
        {
            int squareIndex = BitboardHelper.ClearAndGetIndexOfLSB(ref pawnsCopy);
            ulong passedPawnMask = isWhite ? PrecomputedEvalData.WhitePassedPawnMask[squareIndex] : PrecomputedEvalData.BlackPassedPawnMask[squareIndex];
            
            // If no enemy pawns can block or capture this pawn, it's passed
            if ((passedPawnMask & opponentPawns) == 0)
            {
                // Bonus based on how advanced the pawn is
                int rank = squareIndex / 8;
                float bonus = isWhite ? passedPawnBonuses[7 - rank] : passedPawnBonuses[rank];
                evaluation += bonus;
            }
            
            // Is isolated pawn
            if ((ownPawns & PrecomputedEvalData.AdjacentFilesMask[FileIndex(squareIndex)]) == 0)
            {
                numIsolatedPawns++;
            }
        }
        evaluation += isolatedPawnPenaltyByCount[numIsolatedPawns];
        
        return evaluation;
    }

    private static float EvaluateKingSafety(bool isWhite, int kingSquare, ulong ownPawns, ulong enemyPawns, float enemyPieceSquareScore, int enemyNumRooks, int enemyNumQueens, float openingWeight)
    {
        int kingFile = kingSquare % 8;
        
        float penalty = 0;
        float uncastledKingPenalty = 0;
        
        // King is likely castled
        if (kingFile <= 2 || kingFile >= 5)
        {
            int[] pawnShieldSquares = isWhite ? PrecomputedEvalData.PawnShieldSquaresWhite[kingSquare] : PrecomputedEvalData.PawnShieldSquaresBlack[kingSquare];
            
            // Check the immediate shield pawns - 1 rank ahead
            for (int i = 0; i < pawnShieldSquares.Length / 2; i++)
            {
                int shieldSquareIndex = pawnShieldSquares[i];
                if (!BitBoardContainsSquare(ownPawns, shieldSquareIndex))
                {
                    // The immediate shield pawn is missing but a pawn exists 1 rank forward
                    if (pawnShieldSquares.Length > 3 && BitBoardContainsSquare(ownPawns, pawnShieldSquares[i + 3]))
                    {
                        // Apply a reduced penalty
                        penalty += kingPawnShieldScores[i + 3];
                    }
                    // The immediate pawn is missing and there's no pawn 1 rank forward either
                    else
                    {
                        penalty += kingPawnShieldScores[i];
                    }
                }
            }
            penalty *= penalty; // square the penalty
        }
        // King is likely not castled
        else
        {
            // Scales penalty from 0-50 based on how developed the opponent's pieces are
            // Logic: an uncastled king is more vulnerable when the opponent has active pieces
            float enemyDevelopmentScore = Math.Clamp((enemyPieceSquareScore + 10) / 130f, 0, 1);
            uncastledKingPenalty = maxUnCastledKingPenalty * enemyDevelopmentScore;
        }
        
        // Evaluate open file attacks
        float openFileAgainstKingPenalty = 0;
        
        // If enemy has 2+ rooks or rook+queen combo
        if (enemyNumRooks > 1 || (enemyNumRooks > 0 && enemyNumQueens > 0))
        {
            int clampedKingFile = Math.Clamp(kingFile, 1, 6);
            
            // Check open files near the king:
            for (int attackFile = clampedKingFile - 1; attackFile <= clampedKingFile + 1; attackFile++)
            {
                ulong fileMask = fileMasks[attackFile];
                bool isKingFile = attackFile == kingFile;
                
                // Semi-open file - no enemy pawns
                if ((enemyPawns & fileMask) == 0)
                {
                    openFileAgainstKingPenalty += isKingFile ? semiOpenKingFilePenalty : semiOpenNonKingFilePenalty;
                    
                    // Fully open file - no pawns at all
                    if ((ownPawns & fileMask) == 0)
                    {
                        openFileAgainstKingPenalty += isKingFile ? fullyOpenKingFilePenalty : fullyOpenNonKingFilePenalty;
                    }
                }

            }
        }

        float enemyQueenMultiplier = 1f;
        // If enemy queen is off the board, penalty is reduced to 67%
        if (enemyNumQueens == 0)
        {
            enemyQueenMultiplier *= 0.67f; // SIX SEVEN - not tuned just funny number
        }

        return (-penalty - uncastledKingPenalty - openFileAgainstKingPenalty) * openingWeight * enemyQueenMultiplier;
    }
    
    public static float CalculateGamePhase(Board board)
    {
        // Game phase calculation based on remaining material
        // Start at 256 (opening) and decrease as pieces are captured
        int phase = 256;
        
        PieceList[] allPieceLists = board.GetAllPieceLists();
    
        for (int i = 0; i < allPieceLists.Length; i++)
        {
            PieceList pieceList = allPieceLists[i];
            PieceType pieceType = pieceList.TypeOfPieceInList;
            
            // Subtract phase value for each piece
            // Knights and Bishops reduce phase by 16
            // Rooks reduce phase by 32
            // Queens reduce phase by 64
            // Pawns don't affect phase (encourage pawn endgames)
            // Kings don't affect phase
            
            switch (pieceType)
            {
                case PieceType.Knight:
                case PieceType.Bishop:
                    phase -= pieceList.Count * 16;
                    break;
                case PieceType.Rook:
                    phase -= pieceList.Count * 32;
                    break;
                case PieceType.Queen:
                    phase -= pieceList.Count * 64;
                    break;
            }
        }
        
        // Clamp to valid range [0, 256]
        return Math.Max(0, Math.Min(256, phase)) / 256f;
    }
    
    public static int GetSquareIndexSided(Square square, bool isWhite)
    {
        // For white, use normal index (a1=0, h8=63)
        // For black, flip the board vertically (a8=0, h1=63)
        int index = square.Index;
        
        if (!isWhite)
        {
            // Flip rank for black pieces
            int rank = index / 8;
            int file = index % 8;
            int flippedRank = 7 - rank;
            index = flippedRank * 8 + file;
        }
        
        return index;
    }
    
    public static int GetSquareIndexSided(int squareIndex, bool isWhite)
    {
        if (!isWhite)
        {
            // Flip rank for black pieces
            int rank = squareIndex / 8;
            int file = squareIndex % 8;
            int flippedRank = 7 - rank;
            return flippedRank * 8 + file;
        }
        
        return squareIndex;
    }
    
    public static float GetPieceSquareValue(PieceType pieceType, int squareIndex, float openingWeight, float endGameWeight)
    {
        float openingValue = 0;
        float endgameValue = 0;
        
        switch (pieceType)
        {
            case PieceType.Pawn:
                openingValue = TunableParameters.GetPawnPST()[squareIndex];
                endgameValue = TunableParameters.GetPawnPSTEnd()[squareIndex];
                break;
            case PieceType.Knight:
                openingValue = TunableParameters.GetKnightPST()[squareIndex];
                endgameValue = TunableParameters.GetKnightPST()[squareIndex]; // Knights use same table for both phases
                break;
            case PieceType.Bishop:
                openingValue = TunableParameters.GetBishopPST()[squareIndex];
                endgameValue = TunableParameters.GetBishopPST()[squareIndex]; // Bishops use same table for both phases
                break;
            case PieceType.Rook:
                openingValue = TunableParameters.GetRookPST()[squareIndex];
                endgameValue = TunableParameters.GetRookPST()[squareIndex]; // Rooks use same table for both phases
                break;
            case PieceType.Queen:
                openingValue = TunableParameters.GetQueenPST()[squareIndex];
                endgameValue = TunableParameters.GetQueenPST()[squareIndex]; // Queens use same table for both phases
                break;
            case PieceType.King:
                openingValue = TunableParameters.GetKingPSTStart()[squareIndex];
                endgameValue = TunableParameters.GetKingPSTEnd()[squareIndex];
                break;
            default:
                return 0;
        }
        
        // Blend between opening and endgame values
        return openingValue * openingWeight + endgameValue * endGameWeight;
    }

    private static float EvaluateMopUp(float myMaterial, float enemyMaterial, Square selfKingSquare, Square enemyKingSquare, float endGameWeight)
    {
        if (myMaterial > enemyMaterial + PieceValues[(int)PieceType.Pawn] * 2 && endGameWeight > 0)
        {
            float mopUpScore = 0;

            int friendlyKingSquare = selfKingSquare.Index;
            int opponentKingSquare = enemyKingSquare.Index;
            
            // Encourage moving king closer to opponent king
            mopUpScore += (14 - PrecomputedEvalData.OrthogonalDistance[friendlyKingSquare, opponentKingSquare]) * mopUpKingDistanceMultiplier;
            
            // Encourage pushing opponent king to edge of board
            mopUpScore += PrecomputedEvalData.CentreManhattanDistance[opponentKingSquare] * mopUpOpponentKingDistanceToCenterMultiplier;
            return mopUpScore * endGameWeight;
        }

        return 0;
    }
    
    // Helpers
    private static int FileIndex(int squareIndex)
    {
        return squareIndex & 0b000111;
    }

    private static bool BitBoardContainsSquare(ulong bitboard, int squareIndex)
    {
        return (bitboard & (1UL << squareIndex)) != 0;
    }
}