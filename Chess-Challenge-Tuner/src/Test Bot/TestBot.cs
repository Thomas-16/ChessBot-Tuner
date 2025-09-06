﻿using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using static System.Math;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class TestBot : IChessBot
    {
        const bool useTimer = false;
        const int maxTimePerMove = 200;
        const bool printDebug = false;
        const bool bookMoves = true;
        const bool determinedFirstTwoMoves = false;
        const bool useMaxThinkTime = true;
        const int maxThinkTimeMs = 5000;

        static readonly int[] pieceValues = { 0, 100, 320, 330, 500, 900, 20000 };
        static readonly float[] capturePieceValues = { 0, 100f, 320f, 330f, 525f, 1000f, 3200f };
        static readonly PieceType[] pieceTypes = { PieceType.None, PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen, PieceType.King };
        static readonly ulong whiteTerritoryMask = 0x00000000FFFFFFFF; // The bottom 4 ranks (1-4)
        static readonly ulong blackTerritoryMask = 0xFFFFFFFF00000000; // The top 4 ranks (5-8)
        static readonly ulong notAFile = 0xFEFEFEFEFEFEFEFE;
        static readonly ulong notHFile = 0x7F7F7F7F7F7F7F7F;
        static readonly int[] passedPawnBonuses = { 0, 600, 400, 250, 150, 75, 75 };
        static readonly int[] isolatedPawnPenaltyByCount = { 0, -50, -125, -250, -375, -375, -375, -375, -375 };
        static readonly int[] doubledPawnPenaltyByCount = { 0, -60, -140, -269, -375, -375, -375, -375, -375 };

        // Move Ordering constants
        const int million = 1000000;
        const int hashMoveScore = 100 * million;
        const int winningCaptureBias = 8 * million;
        const int promoteBias = 6 * million;
        const int killerBias = 4 * million;
        const int losingCaptureBias = 2 * million;

        Entry[] _transpositions = new Entry[16777216];
        static int transCount;
        float _budgetCounter = 121, budget;
        bool searchCancelled;
        Timer timer;
        Board board;
        HashSet<Move> _killerMoves = new();
        int[,,] _history = new int[2, 64, 64];

        static readonly string openingBookPath = Path.Combine(ChessChallenge.Application.FileHelper.GetResourcePath(), "Book.txt");
        OpeningBook openingBook = new(File.ReadAllText(openingBookPath));

        // Square values can be calculated by bit shifting
        ulong centerSquares = ((ulong)1 << new Square("e4").Index)
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
        Square[] whiteCastledKingSquares = {
            new Square("a1"),
            new Square("b1"),
            new Square("c1"),
            new Square("g1"),
            new Square("h1")
        };
        Square[] blackCastledKingSquares = {
            new Square("a8"),
            new Square("b8"),
            new Square("c8"),
            new Square("g8"),
            new Square("h8")
        };
        //ulong whiteKingMobilitySquares =
        //                      ((ulong)1 << new Square("a1").Index)
        //                    | ((ulong)1 << new Square("b1").Index)
        //                    | ((ulong)1 << new Square("c1").Index)
        //                    | ((ulong)1 << new Square("a2").Index)
        //                    | ((ulong)1 << new Square("b2").Index)
        //                    | ((ulong)1 << new Square("c2").Index)
        //                    | ((ulong)1 << new Square("f1").Index)
        //                    | ((ulong)1 << new Square("g1").Index)
        //                    | ((ulong)1 << new Square("h1").Index)
        //                    | ((ulong)1 << new Square("g2").Index)
        //                    | ((ulong)1 << new Square("h2").Index);
        
        //ulong blackKingMobilitySquares =
        //                      ((ulong)1 << new Square("a8").Index)
        //                    | ((ulong)1 << new Square("b8").Index)
        //                    | ((ulong)1 << new Square("c8").Index)
        //                    | ((ulong)1 << new Square("a7").Index)
        //                    | ((ulong)1 << new Square("b7").Index)
        //                    | ((ulong)1 << new Square("c7").Index)
        //                    | ((ulong)1 << new Square("f8").Index)
        //                    | ((ulong)1 << new Square("g8").Index)
        //                    | ((ulong)1 << new Square("h8").Index)
        //                    | ((ulong)1 << new Square("g7").Index)
        //                    | ((ulong)1 << new Square("h7").Index);

        // Masks for each file on the chessboard
        static ulong[] files = {
            0x0101010101010101, // File A
            0x0202020202020202, // File B
            0x0404040404040404, // File C
            0x0808080808080808, // File D
            0x1010101010101010, // File E
            0x2020202020202020, // File F
            0x4040404040404040, // File G
            0x8080808080808080  // File H
        };
        public class OpeningBook
        {
            readonly Dictionary<string, BookMove[]> movesByPosition;
            readonly Random rng;

            public OpeningBook(string file) {
                rng = new Random();
                Span<string> entries = file.Trim(new char[] { ' ', '\n' }).Split("pos").AsSpan(1);
                movesByPosition = new Dictionary<string, BookMove[]>(entries.Length);

                for (int i = 0; i < entries.Length; i++) {
                    string[] entryData = entries[i].Trim('\n').Split('\n');
                    string positionFen = entryData[0].Trim();
                    Span<string> allMoveData = entryData.AsSpan(1);

                    BookMove[] bookMoves = new BookMove[allMoveData.Length];

                    for (int moveIndex = 0; moveIndex < bookMoves.Length; moveIndex++) {
                        string[] moveData = allMoveData[moveIndex].Split(' ');
                        bookMoves[moveIndex] = new BookMove(moveData[0], int.Parse(moveData[1]));
                    }

                    movesByPosition.Add(positionFen, bookMoves);
                }
            }

            public bool HasBookMove(string positionFen) {
                return movesByPosition.ContainsKey(RemoveMoveCountersFromFEN(positionFen));
            }

            // WeightPow is a value between 0 and 1.
            // 0 means all moves are picked with equal probablity, 1 means moves are weighted by num times played.
            public bool TryGetBookMove(Board board, out string moveString, double weightPow = 0.5) {
                string positionFen = board.GetFenString();
                weightPow = Math.Clamp(weightPow, 0, 1);
                if (movesByPosition.TryGetValue(RemoveMoveCountersFromFEN(positionFen), out var moves)) {
                    int totalPlayCount = 0;
                    foreach (BookMove move in moves) {
                        totalPlayCount += WeightedPlayCount(move.numTimesPlayed);
                    }

                    double[] weights = new double[moves.Length];
                    double weightSum = 0;
                    for (int i = 0; i < moves.Length; i++) {
                        double weight = WeightedPlayCount(moves[i].numTimesPlayed) / (double)totalPlayCount;
                        weightSum += weight;
                        weights[i] = weight;
                    }

                    double[] probCumul = new double[moves.Length];
                    for (int i = 0; i < weights.Length; i++) {
                        double prob = weights[i] / weightSum;
                        probCumul[i] = probCumul[Math.Max(0, i - 1)] + prob;
                        string debugString = $"{moves[i].moveString}: {prob * 100:0.00}% (cumul = {probCumul[i]})";
                        if (printDebug) {
                            Console.WriteLine(debugString);
                        }
                    }


                    double random = rng.NextDouble();
                    for (int i = 0; i < moves.Length; i++) {

                        if (random <= probCumul[i]) {
                            moveString = moves[i].moveString;
                            return true;
                        }
                    }
                }

                moveString = "Null";
                return false;

                int WeightedPlayCount(int playCount) => (int)Math.Ceiling(Math.Pow(playCount, weightPow));
            }

            string RemoveMoveCountersFromFEN(string fen) {
                string fenA = fen[..fen.LastIndexOf(' ')];
                return fenA[..fenA.LastIndexOf(' ')];
            }


            public readonly struct BookMove
            {
                public readonly string moveString;
                public readonly int numTimesPlayed;

                public BookMove(string moveString, int numTimesPlayed) {
                    this.moveString = moveString;
                    this.numTimesPlayed = numTimesPlayed;
                }
            }
        }
        public static string GetTranspositionPercentage() {
            return $"{Math.Round(transCount / 16777216.0 * 100.0, 2)}%";
        }
        public void ResetTrans() {
            transCount = 0;
            //_transpositions = new Entry[16777216];
        }
        float OneMinusEndgameT(Board board, bool white) {
            int endgameWeightSum = 0;
            foreach (var pl in board.GetAllPieceLists())
                if (pl.IsWhitePieceList == white)
                    endgameWeightSum += (0x942200 >> 4 * (int)pl.TypeOfPieceInList & 0xf) * pl.Count;

            return Min(1, endgameWeightSum * 0.04f);
        }
        int EvaluateBoard() {
            float ownOneMinusEndgameT = OneMinusEndgameT(board, false);
            float otherOneMinusEndgameT = OneMinusEndgameT(board, true);
            float score = 0.0f;
            float whiteMatScore = 0f;
            float blackMatScore = 0f;

            //foreach (var pl in board.GetAllPieceLists())
            //    score += 0b1000010 >> (int)pl.TypeOfPieceInList != 0
            //        ? (pl.IsWhitePieceList ? ownOneMinusEndgameT : -otherOneMinusEndgameT) * EvaluatePieceSquareTable(Starts, pl)
            //          + (pl.IsWhitePieceList ? 1.0f - ownOneMinusEndgameT : otherOneMinusEndgameT - 1.0f) * EvaluatePieceSquareTable(Ends, pl)
            //        : EvaluatePieceSquareTable(Starts, pl);

            foreach (PieceList pl in board.GetAllPieceLists()) {
                float pieceListScore;

                if (0b1000010 >> (int)pl.TypeOfPieceInList != 0) {
                    float pieceScoreStart = EvaluatePieceSquareTable(Starts, pl);
                    float pieceScoreEnd = EvaluatePieceSquareTable(Ends, pl);

                    if (pl.IsWhitePieceList) {
                        pieceListScore = ownOneMinusEndgameT * pieceScoreStart + (1.0f - ownOneMinusEndgameT) * pieceScoreEnd;
                    }
                    else {
                        pieceListScore = -otherOneMinusEndgameT * pieceScoreStart + (otherOneMinusEndgameT - 1.0f) * pieceScoreEnd;
                    }
                }
                else {
                    pieceListScore = EvaluatePieceSquareTable(Starts, pl);
                }

                if (pl.IsWhitePieceList) {
                    whiteMatScore += pieceListScore;
                }
                else {
                    blackMatScore += pieceListScore;
                }
                score += pieceListScore;
            }

            ulong whitePieces = board.WhitePiecesBitboard;
            ulong blackPieces = board.BlackPiecesBitboard;

            int totalPieces = CountPiecesOnBoard(board);

            bool isOpening = totalPieces > 25;
            bool isMidgame = totalPieces <= 25 && totalPieces > 13;
            bool isEndEndgame = totalPieces < 13;
            bool isWhiteToMove = board.IsWhiteToMove;

            Square whiteKingSquare = board.GetKingSquare(true);
            Square blackKingSquare = board.GetKingSquare(false);


            int whiteCenterControlScore = BitboardHelper.GetNumberOfSetBits(whitePieces & centerSquares);
            int blackCenterControlScore = BitboardHelper.GetNumberOfSetBits(blackPieces & centerSquares);


            // In the opening and midgame, add score for center control and subtract for king safety
            if (isOpening || isMidgame) {
                float openingScalingFactor = Clamp(totalPieces / 20f, 0f, 1f);

                score += (whiteCenterControlScore - blackCenterControlScore) * 16f * openingScalingFactor;
            }

            ulong whitePawns = board.GetPieceBitboard(PieceType.Pawn, true);
            ulong blackPawns = board.GetPieceBitboard(PieceType.Pawn, false);
            ulong allPawns = whitePawns | blackPawns;

            // Doubled Pawns
            int whiteDoubledPawnsPenalty = doubledPawnPenaltyByCount[CountDoubledPawns(whitePawns)];
            int blackDoubledPawnsPenalty = doubledPawnPenaltyByCount[CountDoubledPawns(blackPawns)];
            score += whiteDoubledPawnsPenalty - blackDoubledPawnsPenalty;

            // Isolated Pawns
            int whiteIsolatedPawnPenalty = isolatedPawnPenaltyByCount[CountIsolatedPawns(whitePawns)];
            int blackIsolatedPawnsPenalty = isolatedPawnPenaltyByCount[CountIsolatedPawns(blackPawns)];
            score += whiteIsolatedPawnPenalty - blackIsolatedPawnsPenalty;

            // Passed Pawns
            int whitePassedPawnsBonus = CountPassedPawnsBonuses(whitePawns, blackPawns, true);
            int blackPassedPawnsBonus = CountPassedPawnsBonuses(blackPawns, whitePawns, false);
            score += whitePassedPawnsBonus - blackPassedPawnsBonus;

            // Rooks on Open Files
            int whiteRooksOnOpenFiles = CountRooksOnOpenFiles(board.GetPieceBitboard(PieceType.Rook, true), allPawns);
            int blackRooksOnOpenFiles = CountRooksOnOpenFiles(board.GetPieceBitboard(PieceType.Rook, false), allPawns);
            score += (whiteRooksOnOpenFiles - blackRooksOnOpenFiles) * 50f;

            // Bishop Pair
            bool whiteHasBishopPair = BitCount(board.GetPieceBitboard(PieceType.Bishop, true)) >= 2;
            bool blackHasBishopPair = BitCount(board.GetPieceBitboard(PieceType.Bishop, false)) >= 2;
            score += (whiteHasBishopPair ? 40 : 0) - (blackHasBishopPair ? 40 : 0);

            // Pawn Shield
            float pawnShieldMultiplier = CalculateMultiplier(totalPieces, 18, 22);
            if (pawnShieldMultiplier != 0) {
                float whitePawnShieldMissingCount = EvaluateMissingPawnShieldCount(board, whiteKingSquare, true);
                float blackPawnShieldMissingCount = EvaluateMissingPawnShieldCount(board, blackKingSquare, false);

                score += ((whitePawnShieldMissingCount * -25f) - (blackPawnShieldMissingCount * -25f)) * pawnShieldMultiplier;
            }

            //// Virtual King Mobility *UNUSED*
            //float virtualKingMobilityMultiplier = CalculateMultiplier(totalPieces, 23, 29);
            //if (virtualKingMobilityMultiplier != 0) {
            //    int whiteKingVirtualMobilityCount = CountKingVirtualMobility(whiteKingSquare, board.AllPiecesBitboard, true);
            //    int blackKingVirtualMobilityCount = CountKingVirtualMobility(blackKingSquare, board.AllPiecesBitboard, false);
            //    score += ((whiteKingVirtualMobilityCount * -12f) - (blackKingVirtualMobilityCount * -12f)) * virtualKingMobilityMultiplier;
            //}

            //// Mop-up Evaluation *UNUSED*
            //float mopUpMultiplier = 1 - CalculateMultiplier(totalPieces, 3, 13);
            //score += MopUpEval(allPawns, whiteMatScore, blackMatScore, mopUpMultiplier, whiteKingSquare, blackKingSquare) -
            //    MopUpEval(allPawns, blackMatScore, whiteMatScore, mopUpMultiplier, blackKingSquare, whiteKingSquare);

            //// Space (How to win at Chess P.112) and King Safety *UNUSED*
            //float spaceFactor = 1 - CalculateMultiplier(totalPieces, 14, 20);
            //if (spaceFactor <= 0f) {
            //    ulong whitePiecesViewInEnemyTerritory = 0;
            //    ulong blackPiecesViewInEnemyTerritory = 0;

            //    ulong blackMask = blackTerritoryMask;
            //    ulong whiteMask = whiteTerritoryMask;

            //    foreach (PieceType pieceType in pieceTypes) {
            //        if (pieceType == PieceType.None || pieceType == PieceType.King || pieceType == PieceType.Knight || pieceType == PieceType.Pawn) continue;

            //        ulong whitePiecesBB = board.GetPieceBitboard(pieceType, true);
            //        ulong blackPiecesBB = board.GetPieceBitboard(pieceType, false);

            //        while (whitePiecesBB != 0) {
            //            Square square = new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref whitePiecesBB));
            //            ulong attacks = BitboardHelper.GetPieceAttacks(pieceType, square, board, true);
            //            whitePiecesViewInEnemyTerritory |= attacks & blackMask;
            //            whitePiecesViewInEnemyTerritory &= ~whitePieces;
            //        }

            //        while (blackPiecesBB != 0) {
            //            Square square = new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref blackPiecesBB));
            //            ulong attacks = BitboardHelper.GetPieceAttacks(pieceType, square, board, false);
            //            blackPiecesViewInEnemyTerritory |= attacks & whiteMask;
            //            blackPiecesViewInEnemyTerritory &= ~blackPieces;
            //        }
            //    }

            //    score += (BitboardHelper.GetNumberOfSetBits(whitePiecesViewInEnemyTerritory) - BitboardHelper.GetNumberOfSetBits(blackPiecesViewInEnemyTerritory)) * 20f * spaceFactor;

            //}

            return (int) Round(isWhiteToMove ? score : -score);
        }

        #region Evaluation Functions
        float MopUpEval(ulong allPawns, float ownMaterial, float enemyMaterial, float mopUpMultiplier, Square ownKingSquare, Square enemyKingSquare) {
            if (mopUpMultiplier != 0 && BitCount(allPawns) <= 5 && ownMaterial >= enemyMaterial + 1500) {
                float mopUpScore = 0f;

                mopUpScore += (14 - ChessChallenge.Chess.PrecomputedMoveData.OrthogonalDistance[ownKingSquare.Index, enemyKingSquare.Index]) * 17;

                mopUpScore += ChessChallenge.Chess.PrecomputedMoveData.CentreManhattanDistance[enemyKingSquare.Index] * 37f;

                return mopUpScore * mopUpMultiplier;
            }
            return 0;
        }
        int CountKingVirtualMobility(Square kingSquare, ulong blockers, bool isWhite) {
            return BitCount(BitboardHelper.GetPieceAttacks(PieceType.Queen, kingSquare, blockers, isWhite));
        }
        int EvaluateMissingPawnShieldCount(Board board, Square kingSquare, bool isWhite) {
            // Directly check if the king is on a castling square
            bool hasCastled = isWhite ? 
                (kingSquare.Index == 6 || kingSquare.Index == 2 || kingSquare.Index == 7 || kingSquare.Index == 1 || kingSquare.Index == 0) :
                (kingSquare.Index == 62 || kingSquare.Index == 58 || kingSquare.Index == 59 || kingSquare.Index == 57 || kingSquare.Index == 56);
            if (!hasCastled) {
                return 0;
            }

            ulong pawnBitboard = board.GetPieceBitboard(PieceType.Pawn, isWhite);
            ulong shieldSquares = GetPawnShieldSquares(kingSquare, isWhite);

            int missingShieldPawns = BitboardHelper.GetNumberOfSetBits(shieldSquares & ~pawnBitboard);

            return missingShieldPawns;
        }
        ulong GetPawnShieldSquares(Square kingSquare, bool isWhite) {
            return isWhite ? PrecomputedEvaluationData.whitePawnShieldSquares[kingSquare.Index] :
                PrecomputedEvaluationData.blackPawnShieldSquares[kingSquare.Index];
        }
        ulong GetKingSurroundingSquares(bool isWhite) {
            // Masks for A and H files
            ulong notAFile = 0xFEFEFEFEFEFEFEFE;
            ulong notHFile = 0x7F7F7F7F7F7F7F7F;

            // Get the king's square
            Square kingSquare = board.GetKingSquare(isWhite);

            // Convert the king's square to a bitboard using the Index property
            ulong kingBitboard = 1UL << kingSquare.Index;

            // Calculate the surrounding squares
            ulong surroundingSquares = kingBitboard
                | (kingBitboard << 8) | (kingBitboard >> 8) // squares above and below the king
                | ((kingBitboard & notHFile) << 1) // squares to the right of the king
                | ((kingBitboard & notHFile) << 9) // square above and to the right of the king
                | ((kingBitboard & notHFile) >> 7) // square below and to the right of the king
                | ((kingBitboard & notAFile) >> 1) // squares to the left of the king
                | ((kingBitboard & notAFile) >> 9) // square above and to the left of the king
                | ((kingBitboard & notAFile) << 7); // square below and to the left of the king

            return surroundingSquares;
        }
        ulong BlendBitboards(ulong bb1, ulong bb2, float weight) {
            if (weight < 0f) return bb1;
            if (weight > 1f) return bb2;

            int bitsFromBB1 = (int)(weight * 64); // 64 bits in a ulong
            ulong result = 0;

            for (int i = 0; i < 32; i++) {
                // Check the higher bit (from the outside)
                if ((bb1 & (1UL << (63 - i))) != 0 && bitsFromBB1 > 0) {
                    result |= (1UL << (63 - i));
                    bitsFromBB1--;
                }
                else if ((bb2 & (1UL << (63 - i))) != 0) {
                    result |= (1UL << (63 - i));
                }

                // Check the lower bit (from the outside)
                if ((bb1 & (1UL << i)) != 0 && bitsFromBB1 > 0) {
                    result |= (1UL << i);
                    bitsFromBB1--;
                }
                else if ((bb2 & (1UL << i)) != 0) {
                    result |= 1UL << i;
                }
            }

            return result;
        }

        // Calculate the scaling factor based on the number of total pieces
        float CalculateMultiplier(int totalPieces, int min, int max) {
            // Ensure totalPieces is within the [min, max] range
            totalPieces = totalPieces < min ? min : (totalPieces > max ? max : totalPieces);

            // Directly return the scaling factor without extra variables
            return (float)(totalPieces - min) / (max - min);
        }


        float EvaluatePieceSquareTable(ulong[][] table, PieceList pl) {
            float value = 0;
            foreach (Piece p in pl) {
                Square sq = p.Square;
                float pieceActivityMultiplier = 1f;

                //if (p.PieceType == PieceType.Bishop || p.PieceType == PieceType.Rook || p.PieceType == PieceType.Queen) {
                //    int pieceVision = BitCount(BitboardHelper.GetPieceAttacks(p.PieceType, sq, board, p.IsWhite) & ~(p.IsWhite ? board.WhitePiecesBitboard : board.BlackPiecesBitboard));
                //    pieceActivityMultiplier = Clamp(pieceVision / 3f + .22f, 0f, 1f);
                //}

                value += (table[(int)p.PieceType][sq.File >= 4 ? 7 - sq.File : sq.File] << 8 * (pl.IsWhitePieceList ? 7 - sq.Rank : sq.Rank) >> 56) * pieceActivityMultiplier;
            }
            return 25 * value;
        }
        int CountMaterial(Board board, bool white) {
            int material = 0;

            material += BitCount(board.GetPieceBitboard(PieceType.Pawn, white)) * pieceValues[1];
            material += BitCount(board.GetPieceBitboard(PieceType.Knight, white)) * pieceValues[2];
            material += BitCount(board.GetPieceBitboard(PieceType.Bishop, white)) * pieceValues[3];
            material += BitCount(board.GetPieceBitboard(PieceType.Rook, white)) * pieceValues[4];
            material += BitCount(board.GetPieceBitboard(PieceType.Queen, white)) * pieceValues[5];

            return material;
        }

        static int CountDoubledPawns(ulong bitboard) {
            int doubledCount = 0;

            foreach (ulong file in files) {
                // Count the number of pawns on this file
                int pawnsOnFile = BitCount(bitboard & file);

                // If there's more than one pawn on this file, increment the doubled count
                if (pawnsOnFile > 1) {
                    doubledCount += pawnsOnFile - 1; // Add (pawnsOnFile - 1) to the count for each additional pawn on the same file
                }
            }

            return doubledCount;
        }
        static int CountIsolatedPawns(ulong bitboard) {
            int isolatedCount = 0;

            for (int i = 0; i < 8; i++) {
                ulong currentFile = files[i];
                ulong pawnsOnCurrentFile = bitboard & currentFile;

                if (pawnsOnCurrentFile == 0)
                    continue; // No pawns on this file, skip to the next file

                ulong adjacentPawns = 0;

                // Check left adjacent file if not on file A
                if (i > 0)
                    adjacentPawns |= bitboard & files[i - 1];

                // Check right adjacent file if not on file H
                if (i < 7)
                    adjacentPawns |= bitboard & files[i + 1];

                // If no pawns on adjacent files, then all pawns on the current file are isolated
                if (adjacentPawns == 0)
                    isolatedCount += BitCount(pawnsOnCurrentFile);
            }

            return isolatedCount;
        }
        public static int CountPassedPawnsBonuses(ulong ownPawns, ulong opponentPawns, bool isWhite) {
            int passedBonus = 0;

            while (ownPawns != 0) {
                // Get the least significant bit's position
                ulong lsb = ownPawns & (~ownPawns + 1);

                ulong blockingRegion = CalculatePassedPawnBlockedRegion(lsb, isWhite);

                if ((blockingRegion & opponentPawns) == 0) {
                    int distance = CalculatePassedPawnDistance(lsb, isWhite);
                    passedBonus += passedPawnBonuses[distance];
                }

                ownPawns &= ownPawns - 1;
            }

            return passedBonus;
        }
        static ulong CalculatePassedPawnBlockedRegion(ulong lsb, bool isWhite) {
            return isWhite ? PrecomputedEvaluationData.whitePassedPawnBlockedRegion[GetSquareIndex(lsb)] :
                PrecomputedEvaluationData.blackPassedPawnBlockedRegion[GetSquareIndex(lsb)];
        }
        static int CalculatePassedPawnDistance(ulong lsb, bool isWhite) {
            Square square = new Square(GetSquareIndex(lsb));
            return isWhite ? 7 - square.Rank : square.Rank;
        }

        static int CountRooksOnOpenFiles(ulong rooks, ulong allPawns) {
            int openFileRooks = 0;

            while (rooks != 0) {
                // Get the least significant bit's position (the position of one of the rooks)
                ulong lsb = rooks & (~rooks + 1);

                // Create a mask for the file this rook is on
                ulong fileMask = NorthFill(lsb) | SouthFill(lsb);

                // Check if there are no pawns on this file
                if ((fileMask & allPawns) == 0)
                    openFileRooks++;

                // Clear the least significant bit (move to the next rook)
                rooks ^= lsb;
            }

            return openFileRooks;
        }

        #endregion

        #region Evaluation Helpers
        public static int GetSquareIndex(ulong bitboard) {
            // Ensure that there is exactly one bit set in the bitboard
            if (bitboard == 0 || (bitboard & (bitboard - 1)) != 0) {
                throw new ArgumentException("Bitboard must have exactly one bit set.");
            }

            return BitOperations.TrailingZeroCount(bitboard);
        }
        ulong SquareToBitboard(Square square) {
            return 1UL << (square.Index);
        }
        private static ulong NorthOne(ulong bb) => bb << 8;
        private static ulong SouthOne(ulong bb) => bb >> 8;
        private static ulong EastOne(ulong bb) => (bb & 0xFEFEFEFEFEFEFEFE) << 1;
        private static ulong WestOne(ulong bb) => (bb & 0x7F7F7F7F7F7F7F7F) >> 1;

        private static ulong NorthFill(ulong bb) {
            bb |= bb << 8;
            bb |= bb << 16;
            bb |= bb << 32;
            return bb;
        }

        private static ulong SouthFill(ulong bb) {
            bb |= bb >> 8;
            bb |= bb >> 16;
            bb |= bb >> 32;
            return bb;
        }
        #endregion

        SearchReturn Search(int depthLeft, int checkExtensionsLeft, bool isCaptureOnly, int alpha = -32200, int beta = 32200) {
            if (board.IsInCheckmate())
                return new SearchReturn(-32100, default, true);

            if (board.IsDraw())
                return new SearchReturn(0, default, board.IsInStalemate() || board.IsInsufficientMaterial());

            if (depthLeft == 0) {
                ++depthLeft;
                if (board.IsInCheck() && checkExtensionsLeft > 0)
                    --checkExtensionsLeft;
                else if (!isCaptureOnly && checkExtensionsLeft == 4)
                    return Search(8, checkExtensionsLeft, true, alpha, beta);
                else
                    return new SearchReturn(EvaluateBoard(), default, true);
            }

            ulong key = board.ZobristKey;
            Entry trans = _transpositions[key % 16777216];
            int bestScore = -32150, score;
            Move best = default;
            if (trans.Key == key && Abs(trans.Depth) >= depthLeft) {
                board.MakeMove(trans.Move);
                bool toDraw = board.IsDraw();
                board.UndoMove(trans.Move);

                if (toDraw)
                    trans = default;
                else {
                    alpha = Max(alpha, bestScore = trans.Score);
                    best = trans.Move;
                    if (beta < alpha || trans.Depth >= 0)
                        return new SearchReturn(trans.Score, trans.Move, true);
                }
            }

            if (isCaptureOnly && (score = EvaluateBoard()) > bestScore && beta < (alpha = Max(alpha, bestScore = score)))
                return new SearchReturn(score, default, true);

            Span<Move> legal = stackalloc Move[218];
            board.GetLegalMovesNonAlloc(ref legal, isCaptureOnly);

            Span<ScoredMove> prioritizedMoves = stackalloc ScoredMove[legal.Length];
            int loopvar = 0;

            OrderMoves(board, ref legal, ref prioritizedMoves, trans);

            bool canUseTranspositions = true, approximate = false, canUse;
            loopvar = 0;
            foreach (ScoredMove scoredMove in prioritizedMoves) {
                searchCancelled = timer.MillisecondsElapsedThisTurn >= budget && !searchCancelled;

                if (searchCancelled)
                    return new SearchReturn(bestScore, best, canUseTranspositions);

                Move move = scoredMove.Move;
                board.MakeMove(move);
                try {
                    if (depthLeft >= 3 && ++loopvar >= 4 && !move.IsCapture) {
                        score = -Search(depthLeft - 2, checkExtensionsLeft, isCaptureOnly, -beta, -alpha).Score;

                        if (searchCancelled)
                            break;

                        if (score < bestScore)
                            continue;
                    }
                    SearchReturn searchReturn = Search(depthLeft - 1, checkExtensionsLeft, isCaptureOnly, -beta, -alpha);
                    score = searchReturn.Score;
                    canUse = searchReturn.CanUseTranspositions;

                    if (searchCancelled)
                        break;

                    score = -score + (Abs(score) >= 30000 ? Sign(score) : 0);

                    if (score <= bestScore)
                        continue;

                    bestScore = score;
                    best = move;
                    alpha = Max(alpha, score);
                    canUseTranspositions = canUse;

                    if (approximate = beta < alpha) {
                        _killerMoves.Add(move);

                        int moveColourIndex = board.IsWhiteToMove ? 0 : 1;
                        _history[moveColourIndex, move.StartSquare.Index, move.TargetSquare.Index] += depthLeft * depthLeft;
                        break;
                    }
                }
                finally {
                    board.UndoMove(move);
                }
            }
            if (!searchCancelled && !isCaptureOnly && canUseTranspositions && bestScore != 0) {
                _transpositions[key % 16777216] = new Entry { Key = key, Depth = (short)(approximate ? -depthLeft : depthLeft), Score = (short)bestScore, Move = best };
                transCount++;
            }

            return new SearchReturn(bestScore, best, canUseTranspositions);
        }
        void OrderMoves(Board board, ref Span<Move> legals, ref Span<ScoredMove> prioritizedMoves, Entry trans) {
            int loopvar = 0;
            int totalPieces = CountPiecesOnBoard(board);

            bool isOpening = totalPieces > 25;
            bool isMidgame = totalPieces <= 25 && totalPieces > 15;
            float endgameT = totalPieces < 25 ? (25f - totalPieces) / 23f : 0f;

            ulong zobristKey = board.ZobristKey;

            foreach (var lmove in legals) {
                float moveScore = 0f;
                Piece movePiece = board.GetPiece(lmove.StartSquare);
                Piece capturePiece = board.GetPiece(lmove.TargetSquare);
                PieceType movePieceType = movePiece.PieceType;
                PieceType capturePieceType = capturePiece.PieceType;
                Square targetSquare = lmove.TargetSquare;

                // Transposition table move
                if (zobristKey == trans.Key && lmove == trans.Move) {
                    prioritizedMoves[loopvar++] = new ScoredMove(hashMoveScore, lmove);
                    continue;
                }
                // Killer moves
                else if (_killerMoves.Contains(lmove)) {
                    moveScore += killerBias;
                }

                // Promotion
                if (lmove.IsPromotion) {
                    moveScore += promoteBias;
                }

                // Encourage central control
                if ((isOpening || isMidgame) && targetSquare.File >= 3 && targetSquare.File <= 4 && targetSquare.Rank >= 3 && targetSquare.Rank <= 4) {
                    moveScore += 75f * (1 - endgameT);
                }

                // Prioritize pawn advances with endgameT scaling
                if (movePieceType == PieceType.Pawn && !isOpening) {
                    moveScore += (movePiece.IsWhite ? targetSquare.Rank : 7 - targetSquare.Rank) * 50f * endgameT;
                }

                // Castling
                if ((isOpening || isMidgame) && lmove.IsCastles) {
                    moveScore += 75f;
                }

                // Capture evaluation
                if (lmove.IsCapture) {
                    // Order moves to try capturing the most valuable opponent piece with least valuable of own pieces first
                    float captureMaterialDelta = capturePieceValues[(int)capturePieceType] - capturePieceValues[(int)movePieceType];
                    bool opponentCanRecapture = board.SquareIsAttackedByOpponent(targetSquare);
                    if (opponentCanRecapture) {
                        moveScore += (captureMaterialDelta >= 0 ? winningCaptureBias : losingCaptureBias) + captureMaterialDelta;
                    }
                    else {
                        moveScore += winningCaptureBias + captureMaterialDelta;
                    }
                }
                // History Heuristic
                else {
                    int moveColourIndex = board.IsWhiteToMove ? 0 : 1;
                    moveScore += _history[moveColourIndex, lmove.StartSquare.Index, lmove.TargetSquare.Index];
                }

                //moveScore += ((0x0953310 >> 4 * (int)lmove.CapturePieceType) & 0xf) * 100;

                //prioritizedMoves[loopvar++] = (moveScore, lmove);
                prioritizedMoves[loopvar++] = new ScoredMove(moveScore, lmove);
            }
            // Hybrid Sorting
            if (prioritizedMoves.Length <= 5) {
                InsertionSort(ref prioritizedMoves);
            }
            else {
                QuickSort(ref prioritizedMoves, 0, prioritizedMoves.Length - 1);
            }
        }
        #region Sorting Algorithms
        void InsertionSort(ref Span<ScoredMove> moves) {
            for (int i = 1; i < moves.Length; i++) {
                var key = moves[i];
                int j = i - 1;

                while (j >= 0 && moves[j].Score < key.Score) {
                    moves[j + 1] = moves[j];
                    j--;
                }
                moves[j + 1] = key;
            }
        }
        void QuickSort(ref Span<ScoredMove> data, int left, int right) {
            if (left < right) {
                int pivotIndex = Partition(ref data, left, right);
                QuickSort(ref data, left, pivotIndex - 1);
                QuickSort(ref data, pivotIndex + 1, right);
            }
        }

        int Partition(ref Span<ScoredMove> data, int left, int right) {
            ScoredMove pivot = data[right];
            int i = left - 1;

            for (int j = left; j < right; j++) {
                if (data[j].Score > pivot.Score) { // Descending order
                    i++;
                    Swap(ref data[i], ref data[j]);
                }
            }

            Swap(ref data[i + 1], ref data[right]);
            return i + 1;
        }

        void Swap(ref ScoredMove a, ref ScoredMove b) {
            ScoredMove temp = a;
            a = b;
            b = temp;
        }
        #endregion
        public Move Think(Board b, Timer t) {
            board = b;
            timer = t;

            //List<string> toWrite = new List<string>();
            //for (int i = 0; i <= 63; i++) {
            //    ulong passedPawnBlockedRegion = CalculatePassedPawnBlockedRegion(IndexToBitboard(i), false);

            //    string toAdd = ", " + passedPawnBlockedRegion.ToString();
            //    toWrite.Add(toAdd);
            //}
            //string path = Path.Combine("E:\\Github\\ChessAI-Project\\Chess-Challenge-main\\Chess-Challenge\\resources", "Temp.txt");

            //File.WriteAllLines(path, toWrite.ToArray());

            //Console.WriteLine("done");

            // Book moves
            if (bookMoves && board.PlyCount <= 10) {
                if (determinedFirstTwoMoves) {
                    if (board.PlyCount == 0) {

                        Move dMove = new Move("d2d4", board);
                        Move eMove = new Move("e2e4", board);

                        Random rand = new Random();

                        Move selectedMove;

                        if (rand.Next(2) == 0)  // Randomly generates 0 or 1
                        {
                            selectedMove = dMove;
                        }
                        else {
                            selectedMove = eMove;
                        }

                        return selectedMove;
                    }

                    if (board.PlyCount == 1) {
                        Move lastMove = board.GameMoveHistory[^1]; // Gets the last move
                        Move nextMove;
                        Random rand = new Random();

                        if (lastMove.ToString() == "d2d4") {
                            return new Move("d7d5", board);
                        }
                        else if (lastMove.ToString() == "e2e4") {
                            if (rand.Next(4) == 0)  // Randomly generates 0 or 1
                            {
                                nextMove = new Move("e7e5", board);
                            }
                            else {
                                nextMove = new Move("c7c5", board);
                            }
                            return nextMove;
                        }
                    }
                }
                if (openingBook.TryGetBookMove(board, out string moveString, 0.8)) {
                    return new Move(moveString, board);
                }
            }

            Stopwatch totalSW = new();
            totalSW.Start();

            if (useTimer) {
                int myTimeRemainingMs = timer.MillisecondsRemaining;
                int myIncrementMs = timer.IncrementMilliseconds;
                // Get a fraction of remaining time to use for current move
                double thinkTimeMs = myTimeRemainingMs / 40.0;
                // Clamp think time if a maximum limit is imposed
                if (useMaxThinkTime) {
                    thinkTimeMs = Min(maxThinkTimeMs, thinkTimeMs);
                }
                // Add increment
                if (myTimeRemainingMs > myIncrementMs * 2) {
                    thinkTimeMs += myIncrementMs * 0.8;
                }

                double minThinkTime = Min(50, myTimeRemainingMs * 0.25);
                budget = (float) Ceiling(Max(minThinkTime, thinkTimeMs));
            }
            else {
                budget = maxTimePerMove;
            }
            searchCancelled = false;

            _killerMoves.Clear();
            _history = new int[2, 64, 64];

            Move bestMove = default, move;

            int depth = 0;
            while (++depth <= 33 && !searchCancelled)
                if ((move = Search(depth, 4, false).Move) != default)
                    bestMove = move;

            if (bestMove == default) {
                if(printDebug)
                    Console.WriteLine("First in list");

                bestMove = board.GetLegalMoves()[0];
            }

            board.MakeMove(bestMove);
            //Console.Clear();
            if (printDebug) {
                Console.WriteLine($"Ply: {board.PlyCount}, Depth: {depth - 1}, Best Move: {bestMove}" +
                    $", Elapsed time: {Round((double)totalSW.ElapsedMilliseconds, 2)}");
            }
            board.UndoMove(bestMove);

            totalSW.Stop();

            return bestMove;
        }

        #region Unused
        Move[] GetOrderedMoves(ref Span<Move> moves, Board board, Entry trans, bool onlyCaptures = false) {
            // Retrieve all legal moves
            board.GetLegalMovesNonAlloc(ref moves, onlyCaptures);
            int moveCount = moves.Length;

            // Use a tuple to hold both move and score, enabling sort of both with Array.Sort()
            (Move, int)[] moveScores = new (Move, int)[moveCount];
            for (int i = 0; i < moveCount; i++) {
                moveScores[i] = (moves[i], EvaluateMoveHeuristic(moves[i], board, trans));
            }

            // Sort moveScores array by score (in descending order) using built-in Array.Sort
            Array.Sort(moveScores, (x, y) => y.Item2.CompareTo(x.Item2));

            // Extract sorted moves into a new array
            Move[] sortedMoves = new Move[moveCount];
            for (int i = 0; i < moveCount; i++) {
                sortedMoves[i] = moveScores[i].Item1;
            }

            return sortedMoves;
        }

        private int EvaluateMoveHeuristic(Move move, Board board, Entry trans) {
            int score = 0;
            Piece movePiece = board.GetPiece(move.StartSquare);
            Piece capturePiece = board.GetPiece(move.TargetSquare);
            PieceType movePieceType = movePiece.PieceType;
            Square targetSquare = move.TargetSquare;
            Square startSquare = move.StartSquare;
            bool isWhite = movePiece.IsWhite;
            bool isEndGame = IsEndgame(board);

            if (move.IsPromotion) {
                score += 1000;
            }

            if (trans.Key == board.ZobristKey) {
                score += 5000;
            }
            else if (_killerMoves.Contains(move)) {
                score += 1000;
            }

            if (move.PromotionPieceType == PieceType.Queen) {
                score += 500;
            }

            if (move.IsCapture) {
                int whiteMaterial = CountMaterial(board, true) / 100;
                int blackMaterial = CountMaterial(board, false) / 100;
                int materialDifference = whiteMaterial - blackMaterial;

                // Encourage equal captures when up in material
                if (materialDifference > 0 && movePieceType == capturePiece.PieceType) {
                    score += 300;
                }
                else if (materialDifference < 0 && movePieceType == capturePiece.PieceType) {
                    score -= 500;
                }

                if (board.SquareIsAttackedByOpponent(targetSquare) && (int)capturePiece.PieceType < (int)movePieceType) {
                    score -= pieceValues[(int)movePieceType];
                }
                else {
                    score += 2 * ((int)capturePiece.PieceType - (int)movePieceType);
                }
            }

            // Encourage central control
            if (targetSquare.File >= 3 && targetSquare.File <= 4 && targetSquare.Rank >= 3 && targetSquare.Rank <= 4) {
                score += 125;
            }

            // Prioritize pawn advances
            if (movePiece.IsPawn) {
                score += (isWhite ? targetSquare.Rank : 7 - targetSquare.Rank) * 20;
            }

            // Encourage moves that keep or place the king in safety.
            if (movePiece.IsKing && !isEndGame) {
                if (BitCount(BitboardHelper.GetKingAttacks(targetSquare)) <
                    BitCount(BitboardHelper.GetKingAttacks(startSquare))) {
                    score += 200;
                }
            }

            return score;
        }
        #endregion
        static bool IsEndgame(Board board) {
            ulong whiteQueens = board.GetPieceBitboard(PieceType.Queen, true);
            ulong blackQueens = board.GetPieceBitboard(PieceType.Queen, false);

            bool whiteHasQueen = whiteQueens != 0;
            bool blackHasQueen = blackQueens != 0;

            // Condition 1: Both sides have no queens
            if (!whiteHasQueen && !blackHasQueen) {
                return true;
            }

            // Condition 2: Every side which has a queen has additionally no other pieces or one minorpiece maximum.
            ulong whiteMinorPieces = board.GetPieceBitboard(PieceType.Knight, true) | board.GetPieceBitboard(PieceType.Bishop, true);
            ulong blackMinorPieces = board.GetPieceBitboard(PieceType.Knight, false) | board.GetPieceBitboard(PieceType.Bishop, false);

            bool whiteQueenCondition = whiteHasQueen && BitCount(whiteMinorPieces) <= 1 && BitCount(board.WhitePiecesBitboard & ~whiteQueens & ~whiteMinorPieces) == 0;
            bool blackQueenCondition = blackHasQueen && BitCount(blackMinorPieces) <= 1 && BitCount(board.BlackPiecesBitboard & ~blackQueens & ~blackMinorPieces) == 0;

            return whiteQueenCondition || blackQueenCondition;
        }
        static int BitCount(ulong b) {
            int count = 0;
            while (b != 0) {
                b &= b - 1;
                count++;
            }
            return count;
        }
        static int CountPiecesOnBoard(Board board) => BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard);
        

        static readonly ulong[] Knights = { 0x3234363636363432ul, 0x34383c3d3c3d3834ul, 0x363c3e3f3f3e3c36ul, 0x363c3f40403f3d36ul },
                                       Bishops = { 0x3c3e3e3e3e3e3e3cul, 0x3e4040414042413eul, 0x3e4041414242403eul, 0x3e4042424242403eul },
                                       Rooks = { 0x6465636363636364ul, 0x6466646464646464ul, 0x6466646464646464ul, 0x6466646464646465ul },
                                       Queens = { 0xb0b2b2b3b4b2b2b0ul, 0xb2b4b4b4b4b5b4b2ul, 0xb2b4b5b5b5b5b5b2ul, 0xb3b4b5b5b5b5b4b3ul };
        ulong[][] Starts = { null, new[] { 0x141e161514151514ul, 0x141e161514131614ul, 0x141e181614121614ul, 0x141e1a1918141014ul }, Knights, Bishops, Rooks,
                                   Queens, new[] { 0x0004080a0c0e1414ul, 0x020406080a0c1416ul, 0x020406080a0c0f12ul, 0x02040406080c0f10ul } },
                         Ends = { null, new[] { 0x14241e1a18161614ul, 0x14241e1a18161614ul, 0x14241e1a18161614ul, 0x14241e1a18161614ul }, Knights, Bishops, Rooks,
                                   Queens, new[] { 0x0c0f0e0d0c0b0a06ul, 0x0e100f0e0d0c0b0aul, 0x0e1114171614100aul, 0x0e1116191815100aul } };

        struct Entry
        {
            public ulong Key;
            public short Score, Depth;
            public Move Move;
        }
        struct ScoredMove
        {
            public float Score;
            public Move Move;

            public ScoredMove(float score, Move move) {
                Score = score;
                Move = move;
            }
        }
        struct SearchReturn
        {
            public int Score;
            public Move Move;
            public bool CanUseTranspositions;

            public SearchReturn(int score, Move move, bool canUseTranspositions) {
                Score = score;
                Move = move;
                CanUseTranspositions = canUseTranspositions;
            }
        }
        public static class PrecomputedEvaluationData 
        {
            public static ulong[] whitePawnShieldSquares = {
                768
                , 1792
                , 3584
                , 7168
                , 14336
                , 28672
                , 57344
                , 49152
                , 196608
                , 458752
                , 917504
                , 1835008
                , 3670016
                , 7340032
                , 14680064
                , 12582912
                , 50331648
                , 117440512
                , 234881024
                , 469762048
                , 939524096
                , 1879048192
                , 3758096384
                , 3221225472
                , 12884901888
                , 30064771072
                , 60129542144
                , 120259084288
                , 240518168576
                , 481036337152
                , 962072674304
                , 824633720832
                , 3298534883328
                , 7696581394432
                , 15393162788864
                , 30786325577728
                , 61572651155456
                , 123145302310912
                , 246290604621824
                , 211106232532992
                , 844424930131968
                , 1970324836974592
                , 3940649673949184
                , 7881299347898368
                , 15762598695796736
                , 31525197391593472
                , 63050394783186944
                , 54043195528445952
                , 216172782113783808
                , 504403158265495552
                , 1008806316530991104
                , 2017612633061982208
                , 4035225266123964416
                , 8070450532247928832
                , 16140901064495857664
                , 13835058055282163712
                , 3
                , 7
                , 14
                , 28
                , 56
                , 112
                , 224
                , 192
            };

            public static ulong[] blackPawnShieldSquares = {
                216172782113783808
                , 504403158265495552
                , 1008806316530991104
                , 2017612633061982208
                , 4035225266123964416
                , 8070450532247928832
                , 16140901064495857664
                , 13835058055282163712
                , 3
                , 7
                , 14
                , 28
                , 56
                , 112
                , 224
                , 192
                , 768
                , 1792
                , 3584
                , 7168
                , 14336
                , 28672
                , 57344
                , 49152
                , 196608
                , 458752
                , 917504
                , 1835008
                , 3670016
                , 7340032
                , 14680064
                , 12582912
                , 50331648
                , 117440512
                , 234881024
                , 469762048
                , 939524096
                , 1879048192
                , 3758096384
                , 3221225472
                , 12884901888
                , 30064771072
                , 60129542144
                , 120259084288
                , 240518168576
                , 481036337152
                , 962072674304
                , 824633720832
                , 3298534883328
                , 7696581394432
                , 15393162788864
                , 30786325577728
                , 61572651155456
                , 123145302310912
                , 246290604621824
                , 211106232532992
                , 844424930131968
                , 1970324836974592
                , 3940649673949184
                , 7881299347898368
                , 15762598695796736
                , 31525197391593472
                , 63050394783186944
                , 54043195528445952
            };

            public static ulong[] whitePassedPawnBlockedRegion = {
                217020518514230017
                , 506381209866536706
                , 1012762419733073412
                , 2025524839466146824
                , 4051049678932293648
                , 8102099357864587296
                , 16204198715729174592
                , 13889313184910721152
                , 217020518514229504
                , 506381209866535424
                , 1012762419733070848
                , 2025524839466141696
                , 4051049678932283392
                , 8102099357864566784
                , 16204198715729133568
                , 13889313184910704640
                , 217020518514098176
                , 506381209866207232
                , 1012762419732414464
                , 2025524839464828928
                , 4051049678929657856
                , 8102099357859315712
                , 16204198715718631424
                , 13889313184906477568
                , 217020518480478208
                , 506381209782190080
                , 1012762419564380160
                , 2025524839128760320
                , 4051049678257520640
                , 8102099356515041280
                , 16204198713030082560
                , 13889313183824347136
                , 217020509873766400
                , 506381188273799168
                , 1012762376547598336
                , 2025524753095196672
                , 4051049506190393344
                , 8102099012380786688
                , 16204198024761573376
                , 13889312906798956544
                , 217018306555543552
                , 506375682125725696
                , 1012751364251451392
                , 2025502728502902784
                , 4051005457005805568
                , 8102010914011611136
                , 16204021828023222272
                , 13889241988298964992
                , 216454257090494464
                , 504966108218916864
                , 1009932216437833728
                , 2019864432875667456
                , 4039728865751334912
                , 8079457731502669824
                , 16158915463005339648
                , 13871086852301127680
                , 72057594037927936
                , 144115188075855872
                , 288230376151711744
                , 576460752303423488
                , 1152921504606846976
                , 2305843009213693952
                , 4611686018427387904
                , 9223372036854775808
            };

            public static ulong[] blackPassedPawnBlockedRegion = {
                1
                , 2
                , 4
                , 8
                , 16
                , 32
                , 64
                , 128
                , 259
                , 519
                , 1038
                , 2076
                , 4152
                , 8304
                , 16608
                , 32960
                , 66307
                , 132871
                , 265742
                , 531484
                , 1062968
                , 2125936
                , 4251872
                , 8437952
                , 16974595
                , 34014983
                , 68029966
                , 136059932
                , 272119864
                , 544239728
                , 1088479456
                , 2160115904
                , 4345496323
                , 8707835655
                , 17415671310
                , 34831342620
                , 69662685240
                , 139325370480
                , 278650740960
                , 552989671616
                , 1112447058691
                , 2229205927687
                , 4458411855374
                , 8916823710748
                , 17833647421496
                , 35667294842992
                , 71334589685984
                , 141565355933888
                , 284786447024899
                , 570676717487879
                , 1141353434975758
                , 2282706869951516
                , 4565413739903032
                , 9130827479806064
                , 18261654959612128
                , 36240731119075520
                , 72905330438374147
                , 146093239676897031
                , 292186479353794062
                , 584372958707588124
                , 1168745917415176248
                , 2337491834830352496
                , 4674983669660704992
                , 9277627166483333312
            };

        }
    }
}