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
    public abstract bool Execute(Board board);
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

    public override bool Execute(Board board)
    {
        Piece piece = board[FromPos];
        bool capture = !board.IsEmpty(ToPos);
        board[ToPos] = piece;
        board[FromPos] = null;
        piece.HasMoved = true;

        return capture || piece.Type == PieceType.Pawn;
    }
} 

public class PawnPromotion : Move{

    public override MoveType Type => MoveType.PawnPromotion;
    public override Position FromPos {get;}
    public override Position ToPos {get;}
    private readonly PieceType NewType;

    public PawnPromotion(Position from, Position to, PieceType newType){
        FromPos = from;
        ToPos = to;
        NewType = newType;
    }

    private Piece createPromotionPiece(Player color){
        return NewType switch{
            PieceType.Knight => new Knight(color),
            PieceType.Bishop => new Bishop(color),
            PieceType.Rook => new Rook(color),
            _ => new Queen(color)
        };
    }

    public override bool Execute(Board board)
    {
        Piece pawn = board[FromPos];
        board[FromPos] = null;

        Piece newPiece = createPromotionPiece(pawn.Color);
        newPiece.HasMoved = true;
        board[ToPos] = newPiece;
        
        return true;
    }

}

public class Castle : Move{
    public override MoveType Type { get; }
    public override Position FromPos { get; }
    public override Position ToPos { get; }

    private readonly Direction kingMoveDir;
    private readonly Position rookFromPos;
    private readonly Position rookToPos;

    public Castle(MoveType type, Position kingPos, Board board){
        Type = type;
        FromPos = kingPos;

        if(Type == MoveType.CastleKS){
            kingMoveDir = Direction.East;
            ToPos = new Position(kingPos.Row, Board.BoardSize - 2);

            int i = kingPos.Column + 1;
            while(i < Board.BoardSize){
                Position pos = new Position(kingPos.Row, i);
                Piece p = board[pos];
                if (p != null && p.Type == PieceType.Rook && p.Color == board[FromPos].Color && !p.HasMoved)
                    break;
                i++;
            }
            rookFromPos = new Position(kingPos.Row, i);
            rookToPos = new Position(kingPos.Row, 5);
        }
        else{
            kingMoveDir = Direction.West;
            ToPos = new Position(kingPos.Row, 2);

            int i = kingPos.Column - 1;
            while (i >= 0)
            {
                Position pos = new Position(kingPos.Row, i);
                Piece p = board[pos];
                if (p != null && p.Type == PieceType.Rook && p.Color == board[FromPos].Color && !p.HasMoved)
                    break;
                i--;
            }
            rookFromPos = new Position(kingPos.Row, i);
            rookToPos = new Position(kingPos.Row, 3);
        }
    }

    public override bool Execute(Board board)
    {
        new NormalMove(FromPos, ToPos).Execute(board);
        new NormalMove(rookFromPos, rookToPos).Execute(board);
        return false;
    }

    public override bool IsLegal(Board board)
    {
        Player player = board[FromPos].Color;
        
        if(board.IsInCheck(player)){
            return false;
        }
        
        Board copy = board.Copy();
        Position kingPos = FromPos;

        for(int i = 0; i<2; i++){
            new NormalMove(kingPos, kingPos + kingMoveDir).Execute(copy);
            kingPos += kingMoveDir;

            if(copy.IsInCheck(player)){ 
                return false;
            }
        }

        return true;
    }

}

public class DoublePawn : Move{
    public override MoveType Type => MoveType.DoublePawn;
    public override Position FromPos { get; }
    public override Position ToPos { get; }

    private readonly Position enPassantSquare;

    public DoublePawn(Position from, Position to){
        FromPos = from;
        ToPos = to;
        enPassantSquare = new Position((from.Row + to.Row)/2 , from.Column);
    }

    public override bool Execute(Board board)
    {
        Player player = board[FromPos].Color;
        board.SetEnPassantSquares(player, enPassantSquare);
        new NormalMove(FromPos, ToPos).Execute(board);
        return true;
    }
}

public class EnPassant : Move{ 
    public override MoveType Type => MoveType.EnPassant;
    public override Position FromPos { get; }
    public override Position ToPos { get; }

    private readonly Position capturePos;

    public EnPassant(Position from, Position to){
        FromPos = from;
        ToPos = to;
        capturePos = new Position(from.Row, to.Column);
    }

    public override bool Execute(Board board)
    {
        new NormalMove(FromPos, ToPos).Execute(board);
        board[capturePos] = null;
        return true;
    }
}