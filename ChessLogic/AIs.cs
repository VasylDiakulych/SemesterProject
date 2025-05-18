using ChessLogic;
using System.Collections.Generic;
using System.Linq;

namespace ChessLogic;

public abstract class ChessAI{
    public abstract Opponent AIType  { get; } 
    protected GameState game;
    public abstract Move ChooseMove();
    public abstract void HandleMove();
    
    public static ChessAI ReturnAI(Opponent aiType, GameState game) {
        return aiType switch
        {
            Opponent.RandomAI => new RandomAI(game),
            Opponent.MiniMaxAI => new MiniMaxAI(game),
            _ => new RandomAI(game),
        };
        ;
    }
}

public class RandomAI : ChessAI
{
    public override Opponent AIType => Opponent.RandomAI;

    public RandomAI(GameState game){
        this.game = game;
    }

    public override Move ChooseMove(){
        IEnumerable<Move> moves = game.AllLegalMovesFor(game.CurrentPlayer);
        List<Move> movesList = moves.ToList();

        var random = new Random();

        return movesList[random.Next(movesList.Count - 1)];
    }

    public override void HandleMove()
    {
        if(game.Result == null && game.CurrentPlayer == game.OpponentColor){
            game.MakeMove(ChooseMove());
        }
    }
}

public class MiniMaxAI : ChessAI
{
    public override Opponent AIType => Opponent.MiniMaxAI;
    const int whiteWin = 10000;
    const int blackWin = 10000;
    const int draw = 0;

    Dictionary<PieceType, int> PiecePrice = new Dictionary<PieceType, int>{
       { PieceType.Bishop, 330 },
       { PieceType.Knight, 320 },
       { PieceType.Pawn, 100 },
       { PieceType.Queen, 900 },
       { PieceType.Rook, 500 },
       { PieceType.King, 20000 }
    };


    public MiniMaxAI(GameState game)
    {
        this.game = game;
    }

    public override Move ChooseMove()
    {
        var (_, chosenMove) = minimax(game, 4, double.NegativeInfinity, double.PositiveInfinity, true);

        return chosenMove;
    }

    public override void HandleMove()
    {
        if (game.CurrentPlayer == game.OpponentColor)
        {
            game.MakeMove(ChooseMove());
        }
    }

    private double MaterialEval()
    {
        Counting counting = game.Board.CountPieces();
        int whiteScore = 0;
        int blackScore = 0;

        foreach (PieceType piece in Enum.GetValues(typeof(PieceType)))
        {
            whiteScore += counting.White(piece) * PiecePrice[piece];
            blackScore += counting.Black(piece) * PiecePrice[piece];
        }

        return whiteScore - blackScore;
    }

    private double Eval()
    {
        if (game.IsGameOver())
        {
            switch (game.Result.Winner)
            {
                case Player.White:
                    return whiteWin;
                case Player.Black:
                    return blackWin;
                case Player.None:
                    return draw;
                default:
                    return draw;
            }
        }

        return MaterialEval();
    }

    private (double score, Move Bestmove) minimax(GameState currentGame, int depth, double alpha, double beta, bool maximizing)
    {
        if (game.IsGameOver() || depth == 0)
        {
            return (Eval(), null);
        }

        IEnumerable<Move> legalMoves = currentGame.AllLegalMovesFor(currentGame.CurrentPlayer);
        Move best = null;
        double bestScore = maximizing ? double.NegativeInfinity : double.PositiveInfinity;

        foreach (Move move in legalMoves)
        {
            GameState gameCopy = currentGame.Copy();
            gameCopy.MakeMove(move);

            double score = minimax(gameCopy, depth - 1, alpha, beta, !maximizing).score;

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
