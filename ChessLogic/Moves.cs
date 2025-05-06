using System.Reflection.Metadata;

namespace ChessLogic;

public enum MoveType{
    Normal,
    CastleKS,
    CastleQS,
    DoublePawn,
    EnPassant,
    PawnPromotion
}

public abstract class Move{
    public abstract MoveType Type { get; }
    public abstract Position FromPos { get; }
    public abstract Position ToPos { get; }
    public abstract void Execute(Board board);
    public virtual bool IsLegal(Board board){
        Player player = board[FromPos].Color;
        Board copy = board.Copy();
        Execute(copy);
        return !copy.IsInCheck(player);
    }
}

public class NormalMove : Move{
    public override MoveType Type => MoveType.Normal;
    public override Position FromPos{ get; }
    public override Position ToPos { get; }
    public NormalMove(Position from, Position to){
        FromPos = from;
        ToPos = to;
    }

    public override void Execute(Board board)
    {
        Piece piece = board[FromPos];
        board[ToPos] = piece;
        board[FromPos] = null;
        piece.HasMoved = true;
    }
} 