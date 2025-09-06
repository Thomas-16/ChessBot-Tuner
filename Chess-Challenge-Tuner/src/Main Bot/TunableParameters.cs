using System;
using System.Reflection;

public static class TunableParameters
{
    // Material values
    [Tunable] public static float PawnValueMidgame = 100;
    [Tunable] public static float KnightValueMidgame = 320;
    [Tunable] public static float BishopValueMidgame = 330;
    [Tunable] public static float RookValueMidgame = 500;
    [Tunable] public static float QueenValueMidgame = 900;
    
    [Tunable] public static float PawnValueEndgame = 115;
    [Tunable] public static float KnightValueEndgame = 305;
    [Tunable] public static float BishopValueEndgame = 340;
    [Tunable] public static float RookValueEndgame = 510;
    [Tunable] public static float QueenValueEndgame = 950;
    
    // Piece-square table values for pawns (opening)
    [Tunable] public static float PawnPST_A7 = 0; [Tunable] public static float PawnPST_B7 = 0; [Tunable] public static float PawnPST_C7 = 0; [Tunable] public static float PawnPST_D7 = 0; [Tunable] public static float PawnPST_E7 = 0; [Tunable] public static float PawnPST_F7 = 0; [Tunable] public static float PawnPST_G7 = 0; [Tunable] public static float PawnPST_H7 = 0;
    [Tunable] public static float PawnPST_A6 = 50; [Tunable] public static float PawnPST_B6 = 50; [Tunable] public static float PawnPST_C6 = 50; [Tunable] public static float PawnPST_D6 = 50; [Tunable] public static float PawnPST_E6 = 50; [Tunable] public static float PawnPST_F6 = 50; [Tunable] public static float PawnPST_G6 = 50; [Tunable] public static float PawnPST_H6 = 50;
    [Tunable] public static float PawnPST_A5 = 10; [Tunable] public static float PawnPST_B5 = 10; [Tunable] public static float PawnPST_C5 = 20; [Tunable] public static float PawnPST_D5 = 30; [Tunable] public static float PawnPST_E5 = 30; [Tunable] public static float PawnPST_F5 = 20; [Tunable] public static float PawnPST_G5 = 10; [Tunable] public static float PawnPST_H5 = 10;
    [Tunable] public static float PawnPST_A4 = 5; [Tunable] public static float PawnPST_B4 = 5; [Tunable] public static float PawnPST_C4 = 10; [Tunable] public static float PawnPST_D4 = 40; [Tunable] public static float PawnPST_E4 = 40; [Tunable] public static float PawnPST_F4 = 10; [Tunable] public static float PawnPST_G4 = 5; [Tunable] public static float PawnPST_H4 = 5;
    [Tunable] public static float PawnPST_A3 = 0; [Tunable] public static float PawnPST_B3 = 0; [Tunable] public static float PawnPST_C3 = 0; [Tunable] public static float PawnPST_D3 = 40; [Tunable] public static float PawnPST_E3 = 40; [Tunable] public static float PawnPST_F3 = 0; [Tunable] public static float PawnPST_G3 = 0; [Tunable] public static float PawnPST_H3 = 0;
    [Tunable] public static float PawnPST_A2 = 5; [Tunable] public static float PawnPST_B2 = -5; [Tunable] public static float PawnPST_C2 = -10; [Tunable] public static float PawnPST_D2 = 0; [Tunable] public static float PawnPST_E2 = 0; [Tunable] public static float PawnPST_F2 = -10; [Tunable] public static float PawnPST_G2 = -5; [Tunable] public static float PawnPST_H2 = 5;
    [Tunable] public static float PawnPST_A1 = 5; [Tunable] public static float PawnPST_B1 = 10; [Tunable] public static float PawnPST_C1 = 10; [Tunable] public static float PawnPST_D1 = -20; [Tunable] public static float PawnPST_E1 = -20; [Tunable] public static float PawnPST_F1 = 10; [Tunable] public static float PawnPST_G1 = 10; [Tunable] public static float PawnPST_H1 = 5;
    [Tunable] public static float PawnPST_A0 = 0; [Tunable] public static float PawnPST_B0 = 0; [Tunable] public static float PawnPST_C0 = 0; [Tunable] public static float PawnPST_D0 = 0; [Tunable] public static float PawnPST_E0 = 0; [Tunable] public static float PawnPST_F0 = 0; [Tunable] public static float PawnPST_G0 = 0; [Tunable] public static float PawnPST_H0 = 0;

    // Piece-square table values for pawns (endgame)
    [Tunable] public static float PawnPSTEnd_A7 = 0; [Tunable] public static float PawnPSTEnd_B7 = 0; [Tunable] public static float PawnPSTEnd_C7 = 0; [Tunable] public static float PawnPSTEnd_D7 = 0; [Tunable] public static float PawnPSTEnd_E7 = 0; [Tunable] public static float PawnPSTEnd_F7 = 0; [Tunable] public static float PawnPSTEnd_G7 = 0; [Tunable] public static float PawnPSTEnd_H7 = 0;
    [Tunable] public static float PawnPSTEnd_A6 = 80; [Tunable] public static float PawnPSTEnd_B6 = 80; [Tunable] public static float PawnPSTEnd_C6 = 80; [Tunable] public static float PawnPSTEnd_D6 = 80; [Tunable] public static float PawnPSTEnd_E6 = 80; [Tunable] public static float PawnPSTEnd_F6 = 80; [Tunable] public static float PawnPSTEnd_G6 = 80; [Tunable] public static float PawnPSTEnd_H6 = 80;
    [Tunable] public static float PawnPSTEnd_A5 = 50; [Tunable] public static float PawnPSTEnd_B5 = 50; [Tunable] public static float PawnPSTEnd_C5 = 50; [Tunable] public static float PawnPSTEnd_D5 = 50; [Tunable] public static float PawnPSTEnd_E5 = 50; [Tunable] public static float PawnPSTEnd_F5 = 50; [Tunable] public static float PawnPSTEnd_G5 = 50; [Tunable] public static float PawnPSTEnd_H5 = 50;
    [Tunable] public static float PawnPSTEnd_A4 = 30; [Tunable] public static float PawnPSTEnd_B4 = 30; [Tunable] public static float PawnPSTEnd_C4 = 30; [Tunable] public static float PawnPSTEnd_D4 = 30; [Tunable] public static float PawnPSTEnd_E4 = 30; [Tunable] public static float PawnPSTEnd_F4 = 30; [Tunable] public static float PawnPSTEnd_G4 = 30; [Tunable] public static float PawnPSTEnd_H4 = 30;
    [Tunable] public static float PawnPSTEnd_A3 = 20; [Tunable] public static float PawnPSTEnd_B3 = 20; [Tunable] public static float PawnPSTEnd_C3 = 20; [Tunable] public static float PawnPSTEnd_D3 = 20; [Tunable] public static float PawnPSTEnd_E3 = 20; [Tunable] public static float PawnPSTEnd_F3 = 20; [Tunable] public static float PawnPSTEnd_G3 = 20; [Tunable] public static float PawnPSTEnd_H3 = 20;
    [Tunable] public static float PawnPSTEnd_A2 = 10; [Tunable] public static float PawnPSTEnd_B2 = 10; [Tunable] public static float PawnPSTEnd_C2 = 10; [Tunable] public static float PawnPSTEnd_D2 = 10; [Tunable] public static float PawnPSTEnd_E2 = 10; [Tunable] public static float PawnPSTEnd_F2 = 10; [Tunable] public static float PawnPSTEnd_G2 = 10; [Tunable] public static float PawnPSTEnd_H2 = 10;
    [Tunable] public static float PawnPSTEnd_A1 = 10; [Tunable] public static float PawnPSTEnd_B1 = 10; [Tunable] public static float PawnPSTEnd_C1 = 10; [Tunable] public static float PawnPSTEnd_D1 = 10; [Tunable] public static float PawnPSTEnd_E1 = 10; [Tunable] public static float PawnPSTEnd_F1 = 10; [Tunable] public static float PawnPSTEnd_G1 = 10; [Tunable] public static float PawnPSTEnd_H1 = 10;
    [Tunable] public static float PawnPSTEnd_A0 = 0; [Tunable] public static float PawnPSTEnd_B0 = 0; [Tunable] public static float PawnPSTEnd_C0 = 0; [Tunable] public static float PawnPSTEnd_D0 = 0; [Tunable] public static float PawnPSTEnd_E0 = 0; [Tunable] public static float PawnPSTEnd_F0 = 0; [Tunable] public static float PawnPSTEnd_G0 = 0; [Tunable] public static float PawnPSTEnd_H0 = 0;

    // Knight PST (opening and endgame)
    [Tunable] public static float KnightPST_A8 = -50; [Tunable] public static float KnightPST_B8 = -40; [Tunable] public static float KnightPST_C8 = -30; [Tunable] public static float KnightPST_D8 = -30; [Tunable] public static float KnightPST_E8 = -30; [Tunable] public static float KnightPST_F8 = -30; [Tunable] public static float KnightPST_G8 = -40; [Tunable] public static float KnightPST_H8 = -50;
    [Tunable] public static float KnightPST_A7 = -40; [Tunable] public static float KnightPST_B7 = -20; [Tunable] public static float KnightPST_C7 = 0; [Tunable] public static float KnightPST_D7 = 0; [Tunable] public static float KnightPST_E7 = 0; [Tunable] public static float KnightPST_F7 = 0; [Tunable] public static float KnightPST_G7 = -20; [Tunable] public static float KnightPST_H7 = -40;
    [Tunable] public static float KnightPST_A6 = -30; [Tunable] public static float KnightPST_B6 = 0; [Tunable] public static float KnightPST_C6 = 10; [Tunable] public static float KnightPST_D6 = 15; [Tunable] public static float KnightPST_E6 = 15; [Tunable] public static float KnightPST_F6 = 10; [Tunable] public static float KnightPST_G6 = 0; [Tunable] public static float KnightPST_H6 = -30;
    [Tunable] public static float KnightPST_A5 = -30; [Tunable] public static float KnightPST_B5 = 5; [Tunable] public static float KnightPST_C5 = 15; [Tunable] public static float KnightPST_D5 = 20; [Tunable] public static float KnightPST_E5 = 20; [Tunable] public static float KnightPST_F5 = 15; [Tunable] public static float KnightPST_G5 = 5; [Tunable] public static float KnightPST_H5 = -30;
    [Tunable] public static float KnightPST_A4 = -30; [Tunable] public static float KnightPST_B4 = 0; [Tunable] public static float KnightPST_C4 = 15; [Tunable] public static float KnightPST_D4 = 20; [Tunable] public static float KnightPST_E4 = 20; [Tunable] public static float KnightPST_F4 = 15; [Tunable] public static float KnightPST_G4 = 0; [Tunable] public static float KnightPST_H4 = -30;
    [Tunable] public static float KnightPST_A3 = -30; [Tunable] public static float KnightPST_B3 = 5; [Tunable] public static float KnightPST_C3 = 10; [Tunable] public static float KnightPST_D3 = 15; [Tunable] public static float KnightPST_E3 = 15; [Tunable] public static float KnightPST_F3 = 10; [Tunable] public static float KnightPST_G3 = 5; [Tunable] public static float KnightPST_H3 = -30;
    [Tunable] public static float KnightPST_A2 = -40; [Tunable] public static float KnightPST_B2 = -20; [Tunable] public static float KnightPST_C2 = 0; [Tunable] public static float KnightPST_D2 = 5; [Tunable] public static float KnightPST_E2 = 5; [Tunable] public static float KnightPST_F2 = 0; [Tunable] public static float KnightPST_G2 = -20; [Tunable] public static float KnightPST_H2 = -40;
    [Tunable] public static float KnightPST_A1 = -50; [Tunable] public static float KnightPST_B1 = -40; [Tunable] public static float KnightPST_C1 = -30; [Tunable] public static float KnightPST_D1 = -30; [Tunable] public static float KnightPST_E1 = -30; [Tunable] public static float KnightPST_F1 = -30; [Tunable] public static float KnightPST_G1 = -40; [Tunable] public static float KnightPST_H1 = -50;

    // Bishop PST (opening and endgame)
    [Tunable] public static float BishopPST_A8 = -20; [Tunable] public static float BishopPST_B8 = -10; [Tunable] public static float BishopPST_C8 = -10; [Tunable] public static float BishopPST_D8 = -10; [Tunable] public static float BishopPST_E8 = -10; [Tunable] public static float BishopPST_F8 = -10; [Tunable] public static float BishopPST_G8 = -10; [Tunable] public static float BishopPST_H8 = -20;
    [Tunable] public static float BishopPST_A7 = -10; [Tunable] public static float BishopPST_B7 = 0; [Tunable] public static float BishopPST_C7 = 0; [Tunable] public static float BishopPST_D7 = 0; [Tunable] public static float BishopPST_E7 = 0; [Tunable] public static float BishopPST_F7 = 0; [Tunable] public static float BishopPST_G7 = 0; [Tunable] public static float BishopPST_H7 = -10;
    [Tunable] public static float BishopPST_A6 = -10; [Tunable] public static float BishopPST_B6 = 0; [Tunable] public static float BishopPST_C6 = 5; [Tunable] public static float BishopPST_D6 = 10; [Tunable] public static float BishopPST_E6 = 10; [Tunable] public static float BishopPST_F6 = 5; [Tunable] public static float BishopPST_G6 = 0; [Tunable] public static float BishopPST_H6 = -10;
    [Tunable] public static float BishopPST_A5 = -10; [Tunable] public static float BishopPST_B5 = 5; [Tunable] public static float BishopPST_C5 = 5; [Tunable] public static float BishopPST_D5 = 10; [Tunable] public static float BishopPST_E5 = 10; [Tunable] public static float BishopPST_F5 = 5; [Tunable] public static float BishopPST_G5 = 5; [Tunable] public static float BishopPST_H5 = -10;
    [Tunable] public static float BishopPST_A4 = -10; [Tunable] public static float BishopPST_B4 = 0; [Tunable] public static float BishopPST_C4 = 10; [Tunable] public static float BishopPST_D4 = 10; [Tunable] public static float BishopPST_E4 = 10; [Tunable] public static float BishopPST_F4 = 10; [Tunable] public static float BishopPST_G4 = 0; [Tunable] public static float BishopPST_H4 = -10;
    [Tunable] public static float BishopPST_A3 = -10; [Tunable] public static float BishopPST_B3 = 10; [Tunable] public static float BishopPST_C3 = 10; [Tunable] public static float BishopPST_D3 = 10; [Tunable] public static float BishopPST_E3 = 10; [Tunable] public static float BishopPST_F3 = 10; [Tunable] public static float BishopPST_G3 = 10; [Tunable] public static float BishopPST_H3 = -10;
    [Tunable] public static float BishopPST_A2 = -10; [Tunable] public static float BishopPST_B2 = 5; [Tunable] public static float BishopPST_C2 = 0; [Tunable] public static float BishopPST_D2 = 0; [Tunable] public static float BishopPST_E2 = 0; [Tunable] public static float BishopPST_F2 = 0; [Tunable] public static float BishopPST_G2 = 5; [Tunable] public static float BishopPST_H2 = -10;
    [Tunable] public static float BishopPST_A1 = -20; [Tunable] public static float BishopPST_B1 = -10; [Tunable] public static float BishopPST_C1 = -10; [Tunable] public static float BishopPST_D1 = -10; [Tunable] public static float BishopPST_E1 = -10; [Tunable] public static float BishopPST_F1 = -10; [Tunable] public static float BishopPST_G1 = -10; [Tunable] public static float BishopPST_H1 = -20;

    // Rook PST (opening and endgame)
    [Tunable] public static float RookPST_A8 = 0; [Tunable] public static float RookPST_B8 = 0; [Tunable] public static float RookPST_C8 = 0; [Tunable] public static float RookPST_D8 = 0; [Tunable] public static float RookPST_E8 = 0; [Tunable] public static float RookPST_F8 = 0; [Tunable] public static float RookPST_G8 = 0; [Tunable] public static float RookPST_H8 = 0;
    [Tunable] public static float RookPST_A7 = 5; [Tunable] public static float RookPST_B7 = 10; [Tunable] public static float RookPST_C7 = 10; [Tunable] public static float RookPST_D7 = 10; [Tunable] public static float RookPST_E7 = 10; [Tunable] public static float RookPST_F7 = 10; [Tunable] public static float RookPST_G7 = 10; [Tunable] public static float RookPST_H7 = 5;
    [Tunable] public static float RookPST_A6 = -5; [Tunable] public static float RookPST_B6 = 0; [Tunable] public static float RookPST_C6 = 0; [Tunable] public static float RookPST_D6 = 0; [Tunable] public static float RookPST_E6 = 0; [Tunable] public static float RookPST_F6 = 0; [Tunable] public static float RookPST_G6 = 0; [Tunable] public static float RookPST_H6 = -5;
    [Tunable] public static float RookPST_A5 = -5; [Tunable] public static float RookPST_B5 = 0; [Tunable] public static float RookPST_C5 = 0; [Tunable] public static float RookPST_D5 = 0; [Tunable] public static float RookPST_E5 = 0; [Tunable] public static float RookPST_F5 = 0; [Tunable] public static float RookPST_G5 = 0; [Tunable] public static float RookPST_H5 = -5;
    [Tunable] public static float RookPST_A4 = -5; [Tunable] public static float RookPST_B4 = 0; [Tunable] public static float RookPST_C4 = 0; [Tunable] public static float RookPST_D4 = 0; [Tunable] public static float RookPST_E4 = 0; [Tunable] public static float RookPST_F4 = 0; [Tunable] public static float RookPST_G4 = 0; [Tunable] public static float RookPST_H4 = -5;
    [Tunable] public static float RookPST_A3 = -5; [Tunable] public static float RookPST_B3 = 0; [Tunable] public static float RookPST_C3 = 0; [Tunable] public static float RookPST_D3 = 0; [Tunable] public static float RookPST_E3 = 0; [Tunable] public static float RookPST_F3 = 0; [Tunable] public static float RookPST_G3 = 0; [Tunable] public static float RookPST_H3 = -5;
    [Tunable] public static float RookPST_A2 = -5; [Tunable] public static float RookPST_B2 = 0; [Tunable] public static float RookPST_C2 = 0; [Tunable] public static float RookPST_D2 = 0; [Tunable] public static float RookPST_E2 = 0; [Tunable] public static float RookPST_F2 = 0; [Tunable] public static float RookPST_G2 = 0; [Tunable] public static float RookPST_H2 = -5;
    [Tunable] public static float RookPST_A1 = 0; [Tunable] public static float RookPST_B1 = 0; [Tunable] public static float RookPST_C1 = 6; [Tunable] public static float RookPST_D1 = 8; [Tunable] public static float RookPST_E1 = 8; [Tunable] public static float RookPST_F1 = 6; [Tunable] public static float RookPST_G1 = 0; [Tunable] public static float RookPST_H1 = 0;

    // Queen PST (opening and endgame)
    [Tunable] public static float QueenPST_A8 = -20; [Tunable] public static float QueenPST_B8 = -10; [Tunable] public static float QueenPST_C8 = -10; [Tunable] public static float QueenPST_D8 = -5; [Tunable] public static float QueenPST_E8 = -5; [Tunable] public static float QueenPST_F8 = -10; [Tunable] public static float QueenPST_G8 = -10; [Tunable] public static float QueenPST_H8 = -20;
    [Tunable] public static float QueenPST_A7 = -10; [Tunable] public static float QueenPST_B7 = 0; [Tunable] public static float QueenPST_C7 = 0; [Tunable] public static float QueenPST_D7 = 0; [Tunable] public static float QueenPST_E7 = 0; [Tunable] public static float QueenPST_F7 = 0; [Tunable] public static float QueenPST_G7 = 0; [Tunable] public static float QueenPST_H7 = -10;
    [Tunable] public static float QueenPST_A6 = -10; [Tunable] public static float QueenPST_B6 = 0; [Tunable] public static float QueenPST_C6 = 5; [Tunable] public static float QueenPST_D6 = 5; [Tunable] public static float QueenPST_E6 = 5; [Tunable] public static float QueenPST_F6 = 5; [Tunable] public static float QueenPST_G6 = 0; [Tunable] public static float QueenPST_H6 = -10;
    [Tunable] public static float QueenPST_A5 = -5; [Tunable] public static float QueenPST_B5 = 0; [Tunable] public static float QueenPST_C5 = 5; [Tunable] public static float QueenPST_D5 = 5; [Tunable] public static float QueenPST_E5 = 5; [Tunable] public static float QueenPST_F5 = 5; [Tunable] public static float QueenPST_G5 = 0; [Tunable] public static float QueenPST_H5 = -5;
    [Tunable] public static float QueenPST_A4 = 0; [Tunable] public static float QueenPST_B4 = 0; [Tunable] public static float QueenPST_C4 = 5; [Tunable] public static float QueenPST_D4 = 5; [Tunable] public static float QueenPST_E4 = 5; [Tunable] public static float QueenPST_F4 = 5; [Tunable] public static float QueenPST_G4 = 0; [Tunable] public static float QueenPST_H4 = -5;
    [Tunable] public static float QueenPST_A3 = -10; [Tunable] public static float QueenPST_B3 = 5; [Tunable] public static float QueenPST_C3 = 5; [Tunable] public static float QueenPST_D3 = 5; [Tunable] public static float QueenPST_E3 = 5; [Tunable] public static float QueenPST_F3 = 5; [Tunable] public static float QueenPST_G3 = 0; [Tunable] public static float QueenPST_H3 = -10;
    [Tunable] public static float QueenPST_A2 = -10; [Tunable] public static float QueenPST_B2 = 0; [Tunable] public static float QueenPST_C2 = 5; [Tunable] public static float QueenPST_D2 = 0; [Tunable] public static float QueenPST_E2 = 0; [Tunable] public static float QueenPST_F2 = 0; [Tunable] public static float QueenPST_G2 = 0; [Tunable] public static float QueenPST_H2 = -10;
    [Tunable] public static float QueenPST_A1 = -20; [Tunable] public static float QueenPST_B1 = -10; [Tunable] public static float QueenPST_C1 = -10; [Tunable] public static float QueenPST_D1 = -5; [Tunable] public static float QueenPST_E1 = -5; [Tunable] public static float QueenPST_F1 = -10; [Tunable] public static float QueenPST_G1 = -10; [Tunable] public static float QueenPST_H1 = -20;

    // King PST opening
    [Tunable] public static float KingPSTStart_A8 = -80; [Tunable] public static float KingPSTStart_B8 = -70; [Tunable] public static float KingPSTStart_C8 = -70; [Tunable] public static float KingPSTStart_D8 = -70; [Tunable] public static float KingPSTStart_E8 = -70; [Tunable] public static float KingPSTStart_F8 = -70; [Tunable] public static float KingPSTStart_G8 = -70; [Tunable] public static float KingPSTStart_H8 = -80;
    [Tunable] public static float KingPSTStart_A7 = -60; [Tunable] public static float KingPSTStart_B7 = -60; [Tunable] public static float KingPSTStart_C7 = -60; [Tunable] public static float KingPSTStart_D7 = -60; [Tunable] public static float KingPSTStart_E7 = -60; [Tunable] public static float KingPSTStart_F7 = -60; [Tunable] public static float KingPSTStart_G7 = -60; [Tunable] public static float KingPSTStart_H7 = -60;
    [Tunable] public static float KingPSTStart_A6 = -40; [Tunable] public static float KingPSTStart_B6 = -50; [Tunable] public static float KingPSTStart_C6 = -50; [Tunable] public static float KingPSTStart_D6 = -60; [Tunable] public static float KingPSTStart_E6 = -60; [Tunable] public static float KingPSTStart_F6 = -50; [Tunable] public static float KingPSTStart_G6 = -50; [Tunable] public static float KingPSTStart_H6 = -40;
    [Tunable] public static float KingPSTStart_A5 = -30; [Tunable] public static float KingPSTStart_B5 = -40; [Tunable] public static float KingPSTStart_C5 = -40; [Tunable] public static float KingPSTStart_D5 = -50; [Tunable] public static float KingPSTStart_E5 = -50; [Tunable] public static float KingPSTStart_F5 = -40; [Tunable] public static float KingPSTStart_G5 = -40; [Tunable] public static float KingPSTStart_H5 = -30;
    [Tunable] public static float KingPSTStart_A4 = -20; [Tunable] public static float KingPSTStart_B4 = -30; [Tunable] public static float KingPSTStart_C4 = -30; [Tunable] public static float KingPSTStart_D4 = -40; [Tunable] public static float KingPSTStart_E4 = -40; [Tunable] public static float KingPSTStart_F4 = -30; [Tunable] public static float KingPSTStart_G4 = -30; [Tunable] public static float KingPSTStart_H4 = -20;
    [Tunable] public static float KingPSTStart_A3 = -10; [Tunable] public static float KingPSTStart_B3 = -20; [Tunable] public static float KingPSTStart_C3 = -20; [Tunable] public static float KingPSTStart_D3 = -20; [Tunable] public static float KingPSTStart_E3 = -20; [Tunable] public static float KingPSTStart_F3 = -20; [Tunable] public static float KingPSTStart_G3 = -20; [Tunable] public static float KingPSTStart_H3 = -10;
    [Tunable] public static float KingPSTStart_A2 = 20; [Tunable] public static float KingPSTStart_B2 = 20; [Tunable] public static float KingPSTStart_C2 = -5; [Tunable] public static float KingPSTStart_D2 = -5; [Tunable] public static float KingPSTStart_E2 = -5; [Tunable] public static float KingPSTStart_F2 = -5; [Tunable] public static float KingPSTStart_G2 = 20; [Tunable] public static float KingPSTStart_H2 = 20;
    [Tunable] public static float KingPSTStart_A1 = 20; [Tunable] public static float KingPSTStart_B1 = 30; [Tunable] public static float KingPSTStart_C1 = 20; [Tunable] public static float KingPSTStart_D1 = 0; [Tunable] public static float KingPSTStart_E1 = 0; [Tunable] public static float KingPSTStart_F1 = 10; [Tunable] public static float KingPSTStart_G1 = 40; [Tunable] public static float KingPSTStart_H1 = 20;

    // King PST endgame
    [Tunable] public static float KingPSTEnd_A8 = -20; [Tunable] public static float KingPSTEnd_B8 = -10; [Tunable] public static float KingPSTEnd_C8 = -10; [Tunable] public static float KingPSTEnd_D8 = -10; [Tunable] public static float KingPSTEnd_E8 = -10; [Tunable] public static float KingPSTEnd_F8 = -10; [Tunable] public static float KingPSTEnd_G8 = -10; [Tunable] public static float KingPSTEnd_H8 = -20;
    [Tunable] public static float KingPSTEnd_A7 = -5; [Tunable] public static float KingPSTEnd_B7 = 0; [Tunable] public static float KingPSTEnd_C7 = 5; [Tunable] public static float KingPSTEnd_D7 = 5; [Tunable] public static float KingPSTEnd_E7 = 5; [Tunable] public static float KingPSTEnd_F7 = 5; [Tunable] public static float KingPSTEnd_G7 = 0; [Tunable] public static float KingPSTEnd_H7 = -5;
    [Tunable] public static float KingPSTEnd_A6 = -10; [Tunable] public static float KingPSTEnd_B6 = -5; [Tunable] public static float KingPSTEnd_C6 = 20; [Tunable] public static float KingPSTEnd_D6 = 30; [Tunable] public static float KingPSTEnd_E6 = 30; [Tunable] public static float KingPSTEnd_F6 = 20; [Tunable] public static float KingPSTEnd_G6 = -5; [Tunable] public static float KingPSTEnd_H6 = -10;
    [Tunable] public static float KingPSTEnd_A5 = -15; [Tunable] public static float KingPSTEnd_B5 = -10; [Tunable] public static float KingPSTEnd_C5 = 35; [Tunable] public static float KingPSTEnd_D5 = 45; [Tunable] public static float KingPSTEnd_E5 = 45; [Tunable] public static float KingPSTEnd_F5 = 35; [Tunable] public static float KingPSTEnd_G5 = -10; [Tunable] public static float KingPSTEnd_H5 = -15;
    [Tunable] public static float KingPSTEnd_A4 = -20; [Tunable] public static float KingPSTEnd_B4 = -15; [Tunable] public static float KingPSTEnd_C4 = 30; [Tunable] public static float KingPSTEnd_D4 = 40; [Tunable] public static float KingPSTEnd_E4 = 40; [Tunable] public static float KingPSTEnd_F4 = 30; [Tunable] public static float KingPSTEnd_G4 = -15; [Tunable] public static float KingPSTEnd_H4 = -20;
    [Tunable] public static float KingPSTEnd_A3 = -25; [Tunable] public static float KingPSTEnd_B3 = -20; [Tunable] public static float KingPSTEnd_C3 = 20; [Tunable] public static float KingPSTEnd_D3 = 25; [Tunable] public static float KingPSTEnd_E3 = 25; [Tunable] public static float KingPSTEnd_F3 = 20; [Tunable] public static float KingPSTEnd_G3 = -20; [Tunable] public static float KingPSTEnd_H3 = -25;
    [Tunable] public static float KingPSTEnd_A2 = -30; [Tunable] public static float KingPSTEnd_B2 = -25; [Tunable] public static float KingPSTEnd_C2 = 0; [Tunable] public static float KingPSTEnd_D2 = 0; [Tunable] public static float KingPSTEnd_E2 = 0; [Tunable] public static float KingPSTEnd_F2 = 0; [Tunable] public static float KingPSTEnd_G2 = -25; [Tunable] public static float KingPSTEnd_H2 = -30;
    [Tunable] public static float KingPSTEnd_A1 = -50; [Tunable] public static float KingPSTEnd_B1 = -30; [Tunable] public static float KingPSTEnd_C1 = -30; [Tunable] public static float KingPSTEnd_D1 = -30; [Tunable] public static float KingPSTEnd_E1 = -30; [Tunable] public static float KingPSTEnd_F1 = -30; [Tunable] public static float KingPSTEnd_G1 = -30; [Tunable] public static float KingPSTEnd_H1 = -50;

    // Pawn evaluation
    [Tunable] public static float PassedPawnBonus_Rank6 = 120;
    [Tunable] public static float PassedPawnBonus_Rank5 = 80;
    [Tunable] public static float PassedPawnBonus_Rank4 = 50;
    [Tunable] public static float PassedPawnBonus_Rank3 = 30;
    [Tunable] public static float PassedPawnBonus_Rank2 = 15;
    [Tunable] public static float PassedPawnBonus_Rank1 = 15;

    [Tunable] public static float IsolatedPawnPenalty_1 = -10;
    [Tunable] public static float IsolatedPawnPenalty_2 = -25;
    [Tunable] public static float IsolatedPawnPenalty_3 = -50;
    [Tunable] public static float IsolatedPawnPenalty_4 = -75;

    // Center control
    [Tunable] public static float PawnCenterControlScore = 12;
    [Tunable] public static float PieceCenterControlScore = 10;

    // King safety
    [Tunable] public static float MaxUncastledKingPenalty = 50;
    [Tunable] public static float SemiOpenKingFilePenalty = 25;
    [Tunable] public static float SemiOpenNonKingFilePenalty = 15;
    [Tunable] public static float FullyOpenKingFilePenalty = 15;
    [Tunable] public static float FullyOpenNonKingFilePenalty = 10;

    [Tunable] public static float KingPawnShieldScore_0 = 4;
    [Tunable] public static float KingPawnShieldScore_1 = 7;
    [Tunable] public static float KingPawnShieldScore_2 = 4;
    [Tunable] public static float KingPawnShieldScore_3 = 3;
    [Tunable] public static float KingPawnShieldScore_4 = 6;
    [Tunable] public static float KingPawnShieldScore_5 = 3;

    // Endgame
    [Tunable] public static float MopUpKingDistanceMultiplier = 6;
    [Tunable] public static float MopUpOpponentKingDistanceToCenterMultiplier = 10;

    // Dynamic arrays for evaluation
    public static float[] PieceValues = new float[] { 0, PawnValueMidgame, KnightValueMidgame, BishopValueMidgame, RookValueMidgame, QueenValueMidgame, 0 };
    public static float[] PieceValuesEndGame = new float[] { 0, PawnValueEndgame, KnightValueEndgame, BishopValueEndgame, RookValueEndgame, QueenValueEndgame, 0 };
    
    public static float[] PassedPawnBonuses = new float[] { 0, PassedPawnBonus_Rank6, PassedPawnBonus_Rank5, PassedPawnBonus_Rank4, PassedPawnBonus_Rank3, PassedPawnBonus_Rank2, PassedPawnBonus_Rank1 };
    public static float[] IsolatedPawnPenaltyByCount = new float[] { 0, IsolatedPawnPenalty_1, IsolatedPawnPenalty_2, IsolatedPawnPenalty_3, IsolatedPawnPenalty_4, -75, -75, -75, -75 };
    public static float[] KingPawnShieldScores = new float[] { KingPawnShieldScore_0, KingPawnShieldScore_1, KingPawnShieldScore_2, KingPawnShieldScore_3, KingPawnShieldScore_4, KingPawnShieldScore_5 };

    // Get PST arrays dynamically
    public static float[] GetPawnPST()
    {
        return new float[]
        {
            PawnPST_A7, PawnPST_B7, PawnPST_C7, PawnPST_D7, PawnPST_E7, PawnPST_F7, PawnPST_G7, PawnPST_H7,
            PawnPST_A6, PawnPST_B6, PawnPST_C6, PawnPST_D6, PawnPST_E6, PawnPST_F6, PawnPST_G6, PawnPST_H6,
            PawnPST_A5, PawnPST_B5, PawnPST_C5, PawnPST_D5, PawnPST_E5, PawnPST_F5, PawnPST_G5, PawnPST_H5,
            PawnPST_A4, PawnPST_B4, PawnPST_C4, PawnPST_D4, PawnPST_E4, PawnPST_F4, PawnPST_G4, PawnPST_H4,
            PawnPST_A3, PawnPST_B3, PawnPST_C3, PawnPST_D3, PawnPST_E3, PawnPST_F3, PawnPST_G3, PawnPST_H3,
            PawnPST_A2, PawnPST_B2, PawnPST_C2, PawnPST_D2, PawnPST_E2, PawnPST_F2, PawnPST_G2, PawnPST_H2,
            PawnPST_A1, PawnPST_B1, PawnPST_C1, PawnPST_D1, PawnPST_E1, PawnPST_F1, PawnPST_G1, PawnPST_H1,
            PawnPST_A0, PawnPST_B0, PawnPST_C0, PawnPST_D0, PawnPST_E0, PawnPST_F0, PawnPST_G0, PawnPST_H0
        };
    }

    public static float[] GetPawnPSTEnd()
    {
        return new float[]
        {
            PawnPSTEnd_A7, PawnPSTEnd_B7, PawnPSTEnd_C7, PawnPSTEnd_D7, PawnPSTEnd_E7, PawnPSTEnd_F7, PawnPSTEnd_G7, PawnPSTEnd_H7,
            PawnPSTEnd_A6, PawnPSTEnd_B6, PawnPSTEnd_C6, PawnPSTEnd_D6, PawnPSTEnd_E6, PawnPSTEnd_F6, PawnPSTEnd_G6, PawnPSTEnd_H6,
            PawnPSTEnd_A5, PawnPSTEnd_B5, PawnPSTEnd_C5, PawnPSTEnd_D5, PawnPSTEnd_E5, PawnPSTEnd_F5, PawnPSTEnd_G5, PawnPSTEnd_H5,
            PawnPSTEnd_A4, PawnPSTEnd_B4, PawnPSTEnd_C4, PawnPSTEnd_D4, PawnPSTEnd_E4, PawnPSTEnd_F4, PawnPSTEnd_G4, PawnPSTEnd_H4,
            PawnPSTEnd_A3, PawnPSTEnd_B3, PawnPSTEnd_C3, PawnPSTEnd_D3, PawnPSTEnd_E3, PawnPSTEnd_F3, PawnPSTEnd_G3, PawnPSTEnd_H3,
            PawnPSTEnd_A2, PawnPSTEnd_B2, PawnPSTEnd_C2, PawnPSTEnd_D2, PawnPSTEnd_E2, PawnPSTEnd_F2, PawnPSTEnd_G2, PawnPSTEnd_H2,
            PawnPSTEnd_A1, PawnPSTEnd_B1, PawnPSTEnd_C1, PawnPSTEnd_D1, PawnPSTEnd_E1, PawnPSTEnd_F1, PawnPSTEnd_G1, PawnPSTEnd_H1,
            PawnPSTEnd_A0, PawnPSTEnd_B0, PawnPSTEnd_C0, PawnPSTEnd_D0, PawnPSTEnd_E0, PawnPSTEnd_F0, PawnPSTEnd_G0, PawnPSTEnd_H0
        };
    }

    public static float[] GetKnightPST()
    {
        return new float[]
        {
            KnightPST_A8, KnightPST_B8, KnightPST_C8, KnightPST_D8, KnightPST_E8, KnightPST_F8, KnightPST_G8, KnightPST_H8,
            KnightPST_A7, KnightPST_B7, KnightPST_C7, KnightPST_D7, KnightPST_E7, KnightPST_F7, KnightPST_G7, KnightPST_H7,
            KnightPST_A6, KnightPST_B6, KnightPST_C6, KnightPST_D6, KnightPST_E6, KnightPST_F6, KnightPST_G6, KnightPST_H6,
            KnightPST_A5, KnightPST_B5, KnightPST_C5, KnightPST_D5, KnightPST_E5, KnightPST_F5, KnightPST_G5, KnightPST_H5,
            KnightPST_A4, KnightPST_B4, KnightPST_C4, KnightPST_D4, KnightPST_E4, KnightPST_F4, KnightPST_G4, KnightPST_H4,
            KnightPST_A3, KnightPST_B3, KnightPST_C3, KnightPST_D3, KnightPST_E3, KnightPST_F3, KnightPST_G3, KnightPST_H3,
            KnightPST_A2, KnightPST_B2, KnightPST_C2, KnightPST_D2, KnightPST_E2, KnightPST_F2, KnightPST_G2, KnightPST_H2,
            KnightPST_A1, KnightPST_B1, KnightPST_C1, KnightPST_D1, KnightPST_E1, KnightPST_F1, KnightPST_G1, KnightPST_H1
        };
    }

    public static float[] GetBishopPST()
    {
        return new float[]
        {
            BishopPST_A8, BishopPST_B8, BishopPST_C8, BishopPST_D8, BishopPST_E8, BishopPST_F8, BishopPST_G8, BishopPST_H8,
            BishopPST_A7, BishopPST_B7, BishopPST_C7, BishopPST_D7, BishopPST_E7, BishopPST_F7, BishopPST_G7, BishopPST_H7,
            BishopPST_A6, BishopPST_B6, BishopPST_C6, BishopPST_D6, BishopPST_E6, BishopPST_F6, BishopPST_G6, BishopPST_H6,
            BishopPST_A5, BishopPST_B5, BishopPST_C5, BishopPST_D5, BishopPST_E5, BishopPST_F5, BishopPST_G5, BishopPST_H5,
            BishopPST_A4, BishopPST_B4, BishopPST_C4, BishopPST_D4, BishopPST_E4, BishopPST_F4, BishopPST_G4, BishopPST_H4,
            BishopPST_A3, BishopPST_B3, BishopPST_C3, BishopPST_D3, BishopPST_E3, BishopPST_F3, BishopPST_G3, BishopPST_H3,
            BishopPST_A2, BishopPST_B2, BishopPST_C2, BishopPST_D2, BishopPST_E2, BishopPST_F2, BishopPST_G2, BishopPST_H2,
            BishopPST_A1, BishopPST_B1, BishopPST_C1, BishopPST_D1, BishopPST_E1, BishopPST_F1, BishopPST_G1, BishopPST_H1
        };
    }

    public static float[] GetRookPST()
    {
        return new float[]
        {
            RookPST_A8, RookPST_B8, RookPST_C8, RookPST_D8, RookPST_E8, RookPST_F8, RookPST_G8, RookPST_H8,
            RookPST_A7, RookPST_B7, RookPST_C7, RookPST_D7, RookPST_E7, RookPST_F7, RookPST_G7, RookPST_H7,
            RookPST_A6, RookPST_B6, RookPST_C6, RookPST_D6, RookPST_E6, RookPST_F6, RookPST_G6, RookPST_H6,
            RookPST_A5, RookPST_B5, RookPST_C5, RookPST_D5, RookPST_E5, RookPST_F5, RookPST_G5, RookPST_H5,
            RookPST_A4, RookPST_B4, RookPST_C4, RookPST_D4, RookPST_E4, RookPST_F4, RookPST_G4, RookPST_H4,
            RookPST_A3, RookPST_B3, RookPST_C3, RookPST_D3, RookPST_E3, RookPST_F3, RookPST_G3, RookPST_H3,
            RookPST_A2, RookPST_B2, RookPST_C2, RookPST_D2, RookPST_E2, RookPST_F2, RookPST_G2, RookPST_H2,
            RookPST_A1, RookPST_B1, RookPST_C1, RookPST_D1, RookPST_E1, RookPST_F1, RookPST_G1, RookPST_H1
        };
    }

    public static float[] GetQueenPST()
    {
        return new float[]
        {
            QueenPST_A8, QueenPST_B8, QueenPST_C8, QueenPST_D8, QueenPST_E8, QueenPST_F8, QueenPST_G8, QueenPST_H8,
            QueenPST_A7, QueenPST_B7, QueenPST_C7, QueenPST_D7, QueenPST_E7, QueenPST_F7, QueenPST_G7, QueenPST_H7,
            QueenPST_A6, QueenPST_B6, QueenPST_C6, QueenPST_D6, QueenPST_E6, QueenPST_F6, QueenPST_G6, QueenPST_H6,
            QueenPST_A5, QueenPST_B5, QueenPST_C5, QueenPST_D5, QueenPST_E5, QueenPST_F5, QueenPST_G5, QueenPST_H5,
            QueenPST_A4, QueenPST_B4, QueenPST_C4, QueenPST_D4, QueenPST_E4, QueenPST_F4, QueenPST_G4, QueenPST_H4,
            QueenPST_A3, QueenPST_B3, QueenPST_C3, QueenPST_D3, QueenPST_E3, QueenPST_F3, QueenPST_G3, QueenPST_H3,
            QueenPST_A2, QueenPST_B2, QueenPST_C2, QueenPST_D2, QueenPST_E2, QueenPST_F2, QueenPST_G2, QueenPST_H2,
            QueenPST_A1, QueenPST_B1, QueenPST_C1, QueenPST_D1, QueenPST_E1, QueenPST_F1, QueenPST_G1, QueenPST_H1
        };
    }

    public static float[] GetKingPSTStart()
    {
        return new float[]
        {
            KingPSTStart_A8, KingPSTStart_B8, KingPSTStart_C8, KingPSTStart_D8, KingPSTStart_E8, KingPSTStart_F8, KingPSTStart_G8, KingPSTStart_H8,
            KingPSTStart_A7, KingPSTStart_B7, KingPSTStart_C7, KingPSTStart_D7, KingPSTStart_E7, KingPSTStart_F7, KingPSTStart_G7, KingPSTStart_H7,
            KingPSTStart_A6, KingPSTStart_B6, KingPSTStart_C6, KingPSTStart_D6, KingPSTStart_E6, KingPSTStart_F6, KingPSTStart_G6, KingPSTStart_H6,
            KingPSTStart_A5, KingPSTStart_B5, KingPSTStart_C5, KingPSTStart_D5, KingPSTStart_E5, KingPSTStart_F5, KingPSTStart_G5, KingPSTStart_H5,
            KingPSTStart_A4, KingPSTStart_B4, KingPSTStart_C4, KingPSTStart_D4, KingPSTStart_E4, KingPSTStart_F4, KingPSTStart_G4, KingPSTStart_H4,
            KingPSTStart_A3, KingPSTStart_B3, KingPSTStart_C3, KingPSTStart_D3, KingPSTStart_E3, KingPSTStart_F3, KingPSTStart_G3, KingPSTStart_H3,
            KingPSTStart_A2, KingPSTStart_B2, KingPSTStart_C2, KingPSTStart_D2, KingPSTStart_E2, KingPSTStart_F2, KingPSTStart_G2, KingPSTStart_H2,
            KingPSTStart_A1, KingPSTStart_B1, KingPSTStart_C1, KingPSTStart_D1, KingPSTStart_E1, KingPSTStart_F1, KingPSTStart_G1, KingPSTStart_H1
        };
    }

    public static float[] GetKingPSTEnd()
    {
        return new float[]
        {
            KingPSTEnd_A8, KingPSTEnd_B8, KingPSTEnd_C8, KingPSTEnd_D8, KingPSTEnd_E8, KingPSTEnd_F8, KingPSTEnd_G8, KingPSTEnd_H8,
            KingPSTEnd_A7, KingPSTEnd_B7, KingPSTEnd_C7, KingPSTEnd_D7, KingPSTEnd_E7, KingPSTEnd_F7, KingPSTEnd_G7, KingPSTEnd_H7,
            KingPSTEnd_A6, KingPSTEnd_B6, KingPSTEnd_C6, KingPSTEnd_D6, KingPSTEnd_E6, KingPSTEnd_F6, KingPSTEnd_G6, KingPSTEnd_H6,
            KingPSTEnd_A5, KingPSTEnd_B5, KingPSTEnd_C5, KingPSTEnd_D5, KingPSTEnd_E5, KingPSTEnd_F5, KingPSTEnd_G5, KingPSTEnd_H5,
            KingPSTEnd_A4, KingPSTEnd_B4, KingPSTEnd_C4, KingPSTEnd_D4, KingPSTEnd_E4, KingPSTEnd_F4, KingPSTEnd_G4, KingPSTEnd_H4,
            KingPSTEnd_A3, KingPSTEnd_B3, KingPSTEnd_C3, KingPSTEnd_D3, KingPSTEnd_E3, KingPSTEnd_F3, KingPSTEnd_G3, KingPSTEnd_H3,
            KingPSTEnd_A2, KingPSTEnd_B2, KingPSTEnd_C2, KingPSTEnd_D2, KingPSTEnd_E2, KingPSTEnd_F2, KingPSTEnd_G2, KingPSTEnd_H2,
            KingPSTEnd_A1, KingPSTEnd_B1, KingPSTEnd_C1, KingPSTEnd_D1, KingPSTEnd_E1, KingPSTEnd_F1, KingPSTEnd_G1, KingPSTEnd_H1
        };
    }

    // Update dynamic arrays (call this after tuning parameters)
    public static void UpdateArrays()
    {
        PieceValues[1] = PawnValueMidgame;
        PieceValues[2] = KnightValueMidgame;
        PieceValues[3] = BishopValueMidgame;
        PieceValues[4] = RookValueMidgame;
        PieceValues[5] = QueenValueMidgame;

        PieceValuesEndGame[1] = PawnValueEndgame;
        PieceValuesEndGame[2] = KnightValueEndgame;
        PieceValuesEndGame[3] = BishopValueEndgame;
        PieceValuesEndGame[4] = RookValueEndgame;
        PieceValuesEndGame[5] = QueenValueEndgame;

        PassedPawnBonuses[1] = PassedPawnBonus_Rank6;
        PassedPawnBonuses[2] = PassedPawnBonus_Rank5;
        PassedPawnBonuses[3] = PassedPawnBonus_Rank4;
        PassedPawnBonuses[4] = PassedPawnBonus_Rank3;
        PassedPawnBonuses[5] = PassedPawnBonus_Rank2;
        PassedPawnBonuses[6] = PassedPawnBonus_Rank1;

        IsolatedPawnPenaltyByCount[1] = IsolatedPawnPenalty_1;
        IsolatedPawnPenaltyByCount[2] = IsolatedPawnPenalty_2;
        IsolatedPawnPenaltyByCount[3] = IsolatedPawnPenalty_3;
        IsolatedPawnPenaltyByCount[4] = IsolatedPawnPenalty_4;

        KingPawnShieldScores[0] = KingPawnShieldScore_0;
        KingPawnShieldScores[1] = KingPawnShieldScore_1;
        KingPawnShieldScores[2] = KingPawnShieldScore_2;
        KingPawnShieldScores[3] = KingPawnShieldScore_3;
        KingPawnShieldScores[4] = KingPawnShieldScore_4;
        KingPawnShieldScores[5] = KingPawnShieldScore_5;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class TunableAttribute : Attribute
{
}