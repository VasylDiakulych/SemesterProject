
namespace ChessLogic;

public class GameState{

    public Board Board { get; }
    public Player CurrentPlayer { get; private set; }
    public Result Result { get; private set; } = null;

    public GameState(Player player, Board board){
        CurrentPlayer = player;
        Board = board;
    }

    public IEnumerable<Move> LegalMovesForPiece(Position pos){
        if(Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer){
            return []; //empty
        }

        Piece piece = Board[pos];
        IEnumerable<Move> MoveCandidates = piece.GetMoves(pos, Board);
        return MoveCandidates.Where(move => move.IsLegal(Board));
    }

    public void MakeMove(Move move){
        move.Execute(Board);
        CurrentPlayer = CurrentPlayer.Opponent();
        CheckForGameOver();
    }

    public IEnumerable<Move> AllLegalMovesFor(Player player){
        IEnumerable<Move> moveCandidates = Board.PiecePosCol(player).SelectMany(pos => {
            Piece piece = Board[pos];
            return piece.GetMoves(pos, Board);
        });

        return moveCandidates.Where(move => move.IsLegal(Board));
    }

    private void CheckForGameOver(){
        if(!AllLegalMovesFor(CurrentPlayer).Any()){
            if(Board.IsInCheck(CurrentPlayer)){
                Result = Result.Win(CurrentPlayer.Opponent());
            }
            else{
                Result = Result.Draw(EndReason.Stalemate);
            }
        }
    }

    public bool IsGameOver(){
        return Result != null;
    }
}