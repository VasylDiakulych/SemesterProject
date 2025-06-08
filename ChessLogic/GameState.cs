
using System.Diagnostics;
using System.Dynamic;
using System.Linq;

namespace ChessLogic;

// class which contains all information about game
public class GameState
{

    public Board Board { get; }
    public Player CurrentPlayer { get; private set; }
    public Opponent Opponent { get; }
    public ChessAI Ai;
    public Player OpponentColor { get; }
    public Result Result { get; private set; } = null;
    public int FiftyMoveCounter = 0;
    private string stateString;
    private Dictionary<string, int> stateHistory = new Dictionary<string, int>();

    public GameState(Player player, Board board, Opponent opponent, Player startingPlayer = Player.White)
    {
        CurrentPlayer = startingPlayer;
        Board = board;
        Opponent = opponent;
        Ai = ChessAI.ReturnAI(opponent, this);
        OpponentColor = player.Opponent();

        stateString = new StateString(CurrentPlayer, board).ToString();

        stateHistory[stateString] = 1;
    }

    public IEnumerable<Move> LegalMovesForPiece(Position pos)
    {
        if (Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer)
        {
            return []; //empty
        }

        Piece piece = Board[pos];
        IEnumerable<Move> MoveCandidates = piece.GetMoves(pos, Board);
        return MoveCandidates.Where(move => move.IsLegal(Board));
    }

    public void MakeMove(Move move)
    {
        Board.SetEnPassantSquares(CurrentPlayer, null);
        bool captureOrPawn = move.Execute(Board);
        if (captureOrPawn)
        {
            FiftyMoveCounter = 0;
            stateHistory.Clear();
        }
        else
        {
            FiftyMoveCounter++;
        }
        CurrentPlayer = CurrentPlayer.Opponent();
        updateStateString();
        CheckForGameOver();
    }

    public IEnumerable<Move> AllLegalMovesFor(Player player)
    {
        IEnumerable<Move> moveCandidates = Board.PiecePosCol(player).SelectMany(pos =>
        {
            Piece piece = Board[pos];
            return piece.GetMoves(pos, Board);
        });

        return moveCandidates.Where(move => move.IsLegal(Board));
    }

    private void CheckForGameOver()
    {
        if (!AllLegalMovesFor(CurrentPlayer).Any())
        {
            if (Board.IsInCheck(CurrentPlayer))
            {
                Result = Result.Win(CurrentPlayer.Opponent());
            }
            else
            {
                Result = Result.Draw(EndReason.Stalemate);
            }
        }
        else if (Board.InsufficientMaterial())
        {
            Result = Result.Draw(EndReason.InsufficientMaterial);
        }
        else if (FiftyMoveRule())
        {
            Result = Result.Draw(EndReason.FiftyMoveRule);
        }
        else if (ThreefoldRepetition())
        {
            Result = Result.Draw(EndReason.ThreefoldRepetition);
        }
        else return;
    }

    // creates full copy of the game
    public GameState Copy()
    {
        GameState copy = new GameState(CurrentPlayer, Board.Copy(), Opponent, CurrentPlayer)
        {
            FiftyMoveCounter = FiftyMoveCounter,
            stateHistory = new Dictionary<string, int>(this.stateHistory),
            stateString = new string(stateString)
        };

        return copy;
    }

    public bool IsGameOver()
    {
        return Result != null;
    }

    public bool FiftyMoveRule()
    {
        int fullMoves = FiftyMoveCounter / 2;
        return fullMoves >= 50;
    }

    public void updateStateString()
    {
        stateString = new StateString(CurrentPlayer, Board).ToString();

        if (!stateHistory.ContainsKey(stateString))
        {
            stateHistory[stateString] = 1;
        }
        else
        {
            stateHistory[stateString]++;
        }
    }

    public bool ThreefoldRepetition()
    {
        return stateHistory[stateString] >= 3;
    }

    // debugging instrument, to check if gamelogic generates all possible positions starting at current position
    public int MoveGenerationTest(int depth, bool isRoot = true)
    {
        if (depth == 0)
        {
            return 1;
        }

        List<Move> moves = AllLegalMovesFor(CurrentPlayer).ToList();
        int numPosition = 0;

        foreach (Move move in moves)
        {
            Board copy = Board.Copy();
            GameState gameCopy = new GameState(CurrentPlayer, copy, Opponent.HumanPlayer, CurrentPlayer);
            gameCopy.MakeMove(move);

            int childMoves = gameCopy.MoveGenerationTest(depth - 1, false);
            numPosition += childMoves;

            if (isRoot)
            {
                Console.WriteLine($"{move}: {childMoves}");
            }
        }

        return numPosition;
    }

}