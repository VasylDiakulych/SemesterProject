using ChessLogic;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ChessLogic;


// General class for chess bots 
public abstract class ChessAI
{
    public abstract Opponent AIType { get; }
    protected GameState game;
    public abstract Move ChooseMove();
    public abstract void HandleMove();

    public static ChessAI ReturnAI(Opponent aiType, GameState game)
    {
        return aiType switch
        {
            Opponent.RandomAI => new RandomAI(game),
            Opponent.MiniMaxAI => new MiniMaxAI(game),
            Opponent.MiniMaxAIOld1 => new MiniMaxAIOld1(game),
            Opponent.MiniMaxAIOld2 => new MiniMaxAIOld2(game),
            _ => new RandomAI(game),
        };
    }
}

// Most basic chess bot, which returns random move from all legal moves
public class RandomAI : ChessAI
{
    public override Opponent AIType => Opponent.RandomAI;

    public RandomAI(GameState game)
    {
        this.game = game;
    }

    public override Move ChooseMove()
    {
        IEnumerable<Move> moves = game.AllLegalMovesFor(game.CurrentPlayer);
        List<Move> movesList = moves.ToList();

        var random = new Random();

        return movesList[random.Next(movesList.Count - 1)];
    }

    public override void HandleMove()
    {
        if (game.Result == null && game.CurrentPlayer == game.OpponentColor)
        {
            game.MakeMove(ChooseMove());
        }
    }
}

// First version of chess bot which uses miniMax algorithm with alpha-beta pruning to choose move
// Evaluation of the position is basically total material difference between players
public class MiniMaxAIOld1 : ChessAI
{
    public override Opponent AIType => Opponent.MiniMaxAIOld2;
    const int whiteWin = 10000;
    const int blackWin = -10000;
    const int draw = 0;

    readonly Dictionary<PieceType, int> PiecePrice = new Dictionary<PieceType, int>{
       { PieceType.Bishop, 330 },
       { PieceType.Knight, 320 },
       { PieceType.Pawn, 100 },
       { PieceType.Queen, 900 },
       { PieceType.Rook, 500 },
       { PieceType.King, 20000 }

    };

    public MiniMaxAIOld1(GameState game)
    {
        this.game = game;
    }

    public override Move ChooseMove()
    {
        var (_, chosenMove) = Minimax(game, 5, double.NegativeInfinity, double.PositiveInfinity);

        return chosenMove;
    }

    public override void HandleMove()
    {
        if (game.CurrentPlayer == game.OpponentColor)
        {
            game.MakeMove(ChooseMove());
        }
    }

    // Function which puts capture moves first, to optimize minimax
    private IEnumerable<Move> OrderMoves(GameState state, IEnumerable<Move> moves)
    {
        return moves.OrderByDescending(move =>
        {
            if (move.IsCapture(state.Board))
            {
                Piece capturedPiece = state.Board[move.ToPos];
                Piece piece = state.Board[move.FromPos];

                return PiecePrice[capturedPiece.Type] - PiecePrice[piece.Type] / 10;
            }

            return -1;
        });
    }

    private double MaterialEval(GameState state)
    {
        Counting counting = state.Board.CountPieces();
        int whiteScore = 0;
        int blackScore = 0;

        foreach (PieceType piece in Enum.GetValues(typeof(PieceType)))
        {
            int piecePrice = PiecePrice[piece];
            whiteScore += counting.White(piece) * piecePrice;
            blackScore += counting.Black(piece) * piecePrice;
        }

        return whiteScore - blackScore;
    }

    private double Eval(GameState state)
    {
        if (state.IsGameOver())
        {
            return state.Result.Winner switch
            {
                Player.White => whiteWin,
                Player.Black => blackWin,
                _ => draw
            };
        }

        double totalEvaluation = 0;

        totalEvaluation += MaterialEval(state);
        return totalEvaluation;
    }

    private (double score, Move Bestmove) Minimax(GameState currentGame, int depth, double alpha, double beta)
    {
        string positionString = StateString.SimpleStateString(currentGame.CurrentPlayer, currentGame.Board);

        if (currentGame.IsGameOver() || depth == 0)
        {
            double eval = Eval(currentGame);
            return (eval, null);
        }

        IEnumerable<Move> legalMoves = currentGame.AllLegalMovesFor(currentGame.CurrentPlayer);
        legalMoves = OrderMoves(currentGame, legalMoves);

        Move best = null;
        bool maximizing = currentGame.CurrentPlayer == Player.White;
        double bestScore = maximizing ? double.NegativeInfinity : double.PositiveInfinity;

        foreach (Move move in legalMoves)
        {
            GameState gameCopy = currentGame.Copy();
            gameCopy.MakeMove(move);

            if (gameCopy.Result == Result.Win(currentGame.CurrentPlayer))
            {
                return (bestScore, move);
            }

            double score = Minimax(gameCopy, depth - 1, alpha, beta).score;

            if (maximizing)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    best = move;
                }
                alpha = Math.Max(alpha, bestScore);
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    best = move;
                }
                beta = Math.Min(beta, bestScore);
            }

            if (beta <= alpha)
            {
                break;
            }
        }
        return (bestScore, best);
    }
}

// Second version of chess bot with minimax
// Evaluation of the position is based on material difference and number of possible moves for each player
// Also it implements an optimization technique, by keeping track of positions visited in minimax
public class MiniMaxAIOld2 : ChessAI
{
    public override Opponent AIType => Opponent.MiniMaxAIOld1;
    const int whiteWin = 10000;
    const int blackWin = -10000;
    const int draw = 0;

    readonly Dictionary<PieceType, int> PiecePrice = new Dictionary<PieceType, int>{
       { PieceType.Bishop, 330 },
       { PieceType.Knight, 320 },
       { PieceType.Pawn, 100 },
       { PieceType.Queen, 900 },
       { PieceType.Rook, 500 },
       { PieceType.King, 20000 }

    };

    readonly double MobilityPrice = 10;

    private readonly Dictionary<string, (double score, int depth)> transpositionTable = [];

    public MiniMaxAIOld2(GameState game)
    {
        this.game = game;
    }

    public override Move ChooseMove()
    {
        var (_, chosenMove) = Minimax(game, 4, double.NegativeInfinity, double.PositiveInfinity);

        return chosenMove;
    }

    public override void HandleMove()
    {
        if (game.CurrentPlayer == game.OpponentColor)
        {
            game.MakeMove(ChooseMove());
        }
    }

    private double MobilityEval(GameState state, IEnumerable<Move> whiteMoves, IEnumerable<Move> blackMoves)
    {

        return MobilityPrice * (whiteMoves.Count() - blackMoves.Count());
    }

    private IEnumerable<Move> OrderMoves(GameState state, IEnumerable<Move> moves)
    {
        return moves.OrderByDescending(move =>
        {
            if (move.IsCapture(state.Board))
            {
                Piece capturedPiece = state.Board[move.ToPos];
                Piece piece = state.Board[move.FromPos];

                return PiecePrice[capturedPiece.Type] - PiecePrice[piece.Type] / 10;
            }

            return -1;
        });
    }

    private double MaterialEval(GameState state)
    {
        Counting counting = state.Board.CountPieces();
        int whiteScore = 0;
        int blackScore = 0;

        foreach (PieceType piece in Enum.GetValues(typeof(PieceType)))
        {
            int piecePrice = PiecePrice[piece];
            whiteScore += counting.White(piece) * piecePrice;
            blackScore += counting.Black(piece) * piecePrice;
        }

        return whiteScore - blackScore;
    }

    private double Eval(GameState state)
    {
        if (state.IsGameOver())
        {
            return state.Result.Winner switch
            {
                Player.White => whiteWin,
                Player.Black => blackWin,
                _ => draw
            };
        }

        double totalEvaluation = 0;
        var whiteMoves = state.AllLegalMovesFor(Player.White);
        var blackMoves = state.AllLegalMovesFor(Player.Black);

        totalEvaluation += MaterialEval(state);
        totalEvaluation += MobilityEval(state, whiteMoves, blackMoves);

        return totalEvaluation;
    }

    private (double score, Move Bestmove) Minimax(GameState currentGame, int depth, double alpha, double beta)
    {
        // created simplified version of the state string to use in transposition table
        string positionString = StateString.SimpleStateString(currentGame.CurrentPlayer, currentGame.Board);

        // if position already evaluated on higher depth, we just take cached result
        if (transpositionTable.TryGetValue(positionString, out var CachedResult) && CachedResult.depth >= depth)
        {
            return (CachedResult.score, null);
        }

        if (currentGame.IsGameOver() || depth == 0)
        {
            double eval = Eval(currentGame);
            transpositionTable[positionString] = (eval, depth);
            return (eval, null);
        }

        IEnumerable<Move> legalMoves = currentGame.AllLegalMovesFor(currentGame.CurrentPlayer);
        legalMoves = OrderMoves(currentGame, legalMoves);

        Move best = null;
        bool maximizing = currentGame.CurrentPlayer == Player.White;
        double bestScore = maximizing ? double.NegativeInfinity : double.PositiveInfinity;

        foreach (Move move in legalMoves)
        {
            GameState gameCopy = currentGame.Copy();
            gameCopy.MakeMove(move);

            double score = Minimax(gameCopy, depth - 1, alpha, beta).score;

            if (maximizing)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    best = move;
                }
                alpha = Math.Max(alpha, bestScore);
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    best = move;
                }
                beta = Math.Min(beta, bestScore);
            }

            if (beta <= alpha)
            {
                break;
            }
        }

        transpositionTable[positionString] = (bestScore, depth);
        return (bestScore, best);
    }
}

// Third version of chess bot with minimax
// Evaluation implements previous techniques + position evaluation based on precalculated
// optimal squares for each piece(piece position tables) + current phase of the game(to be improved in newer versions)
// + aditional optimization, with using Zobrist hashing instead of simplified statestring for transposition table
public class MiniMaxAI : ChessAI
{


    public override Opponent AIType => Opponent.MiniMaxAI;
    const int whiteWin = 10000;
    const int blackWin = -10000;
    const int draw = 0;
    int TotalPhase = 4000;
    int startingDepth = 4;
    GameStage stage;

    readonly Dictionary<PieceType, int> PiecePrice = new Dictionary<PieceType, int>{
       { PieceType.Bishop, 330 },
       { PieceType.Knight, 320 },
       { PieceType.Pawn, 100 },
       { PieceType.Queen, 900 },
       { PieceType.Rook, 500 },
       { PieceType.King, 20000 }

    };

    readonly double MobilityPrice = 10;

    private readonly Dictionary<ulong, (double score, int depth)> transpositionTable = [];

    public MiniMaxAI(GameState game)
    {
        this.game = game;
    }

    public override Move ChooseMove()
    {
        var (score, chosenMove) = Minimax(game, startingDepth, double.NegativeInfinity, double.PositiveInfinity);

        // debugging line
        Console.WriteLine("The heuristic evaluation of this move is: " + score);

        return chosenMove;
    }

    public override void HandleMove()
    {
        if (game.CurrentPlayer == game.OpponentColor)
        {
            game.MakeMove(ChooseMove());
        }
    }

    private double CalculatePhase(GameState state)
    {
        int totPhase = TotalPhase;
        foreach (PieceType piece in Enum.GetValues(typeof(PieceType)))
        {
            if (piece == PieceType.King) { continue; }
            totPhase -= state.Board.CountPieces().White(piece) * PiecePrice[piece];
            totPhase -= state.Board.CountPieces().Black(piece) * PiecePrice[piece];
        }
        return (double)totPhase / TotalPhase;
    }

    private GameStage GetStage(GameState state)
    {
        double phase = CalculatePhase(state);
        if (phase > 0.8) { return GameStage.Opening; }
        else if (phase < 0.2) { return GameStage.Endgame; }
        else return GameStage.Middlegame;
    }

    private double PositionEval(GameState state)
    {
        int eval = 0;

        foreach (Position pos in state.Board.PiecePositions())
        {
            Piece piece = state.Board[pos];
            int index = pos.Row * 8 + pos.Column;
            bool isWhite = piece.Color == Player.White;

            if (isWhite)
            {
                eval += piece.Type switch
                {
                    PieceType.Pawn => PieceTables.PawnTable[index],
                    PieceType.Bishop => PieceTables.BishopTable[index],
                    PieceType.Knight => PieceTables.KnightTable[index],
                    PieceType.Rook => PieceTables.RookTable[index],
                    PieceType.Queen => PieceTables.QueenTable[index],
                    PieceType.King =>
                        stage != GameStage.Endgame ?
                        PieceTables.KingOpeningTable[index] :
                        PieceTables.KingEndgameTable[index],
                    _ => 0
                };
            }
            else
            {
                eval -= piece.Type switch
                {
                    PieceType.Pawn => PieceTables.PawnTable[63 - index],
                    PieceType.Bishop => PieceTables.BishopTable[63 - index],
                    PieceType.Knight => PieceTables.KnightTable[63 - index],
                    PieceType.Rook => PieceTables.RookTable[63 - index],
                    PieceType.Queen => PieceTables.QueenTable[63 - index],
                    PieceType.King =>
                        stage != GameStage.Endgame ?
                        PieceTables.KingOpeningTable[63 - index] :
                        PieceTables.KingEndgameTable[63 - index],
                    _ => 0
                };
            }
        }

        return eval;
    }

    private double MobilityEval(GameState state, IEnumerable<Move> whiteMoves, IEnumerable<Move> blackMoves)
    {

        return MobilityPrice * (whiteMoves.Count() - blackMoves.Count());
    }

    private IEnumerable<Move> OrderMoves(GameState state, IEnumerable<Move> moves)
    {
        return moves.OrderByDescending(move =>
        {   
            // prioritize castling moves
            if (move.Type == MoveType.CastleKS || move.Type == MoveType.CastleQS)
            {
                return 1000;
            }
            // then captures
            else if (move.IsCapture(state.Board))
            {
                Piece capturedPiece = state.Board[move.ToPos];
                Piece piece = state.Board[move.FromPos];

                return 10 * PiecePrice[capturedPiece.Type] - PiecePrice[piece.Type];
            }

            return -1;
        });
    }

    private double MaterialEval(GameState state)
    {
        Counting counting = state.Board.CountPieces();
        int whiteScore = 0;
        int blackScore = 0;

        foreach (PieceType piece in Enum.GetValues(typeof(PieceType)))
        {
            int piecePrice = PiecePrice[piece];
            whiteScore += counting.White(piece) * piecePrice;
            blackScore += counting.Black(piece) * piecePrice;
        }

        return whiteScore - blackScore;
    }

    private double Eval(GameState state, IEnumerable<Move> whiteMoves, IEnumerable<Move> blackMoves)
    {
        if (state.IsGameOver())
        {
            if (state.Result.Reason == EndReason.Checkmate)
            {
                return state.Result.Winner switch
                {
                    Player.White => whiteWin,
                    _ => blackWin
                };
            }
            else
            {
                return 0;
            }
        }

        double totalEvaluation = 0;

        stage = GetStage(state);

        totalEvaluation += MaterialEval(state);
        totalEvaluation += MobilityEval(state, whiteMoves, blackMoves);
        totalEvaluation += PositionEval(state);

        return totalEvaluation;
    }

    private (double score, Move Bestmove) Minimax(GameState currentGame, int depth, double alpha, double beta)
    {
        ulong hash = currentGame.Board.ComputeZobristHash(currentGame.CurrentPlayer);

        if (transpositionTable.TryGetValue(hash, out var CachedResult) && CachedResult.depth >= depth && depth != startingDepth)
        {
            return (CachedResult.score, null);
        }

        var whiteMoves = currentGame.AllLegalMovesFor(Player.White);
        var blackMoves = currentGame.AllLegalMovesFor(Player.Black);

        if (currentGame.IsGameOver() || depth == 0)
        {
            double eval = Eval(currentGame, whiteMoves, blackMoves);
            transpositionTable[hash] = (eval, depth);
            return (eval, null);
        }

        IEnumerable<Move> legalMoves = currentGame.CurrentPlayer == Player.White ? whiteMoves : blackMoves;
        legalMoves = OrderMoves(currentGame, legalMoves);

        Move best = null;
        bool maximizing = currentGame.CurrentPlayer == Player.White;
        double bestScore = maximizing ? double.NegativeInfinity : double.PositiveInfinity;

        foreach (Move move in legalMoves)
        {
            GameState gameCopy = currentGame.Copy();
            gameCopy.MakeMove(move);

            double score = Minimax(gameCopy, depth - 1, alpha, beta).score;

            if (maximizing)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    best = move;
                }
                alpha = Math.Max(alpha, bestScore);
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    best = move;
                }
                beta = Math.Min(beta, bestScore);
            }

            if (beta <= alpha)
            {
                break;
            }
        }

        transpositionTable[hash] = (bestScore, depth);
        return (bestScore, best);
    }
}