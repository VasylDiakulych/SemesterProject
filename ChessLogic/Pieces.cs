
using System.Security.AccessControl;

namespace ChessLogic;

public enum PieceType{
    Pawn,
    Bishop,
    Knight,
    Rook,
    Queen,
    King
}

public abstract class Piece{
    public abstract PieceType Type { get; }
    public abstract Player Color { get; }
    public bool HasMoved { get; set; } = false;
    public abstract Piece Copy();

    public abstract IEnumerable<Move> GetMoves(Position from, Board board);

    protected IEnumerable<Position> MovePositionsInDir(Position from, Board board, Direction direction){
        for(Position pos = from + direction; Board.IsInside(pos); pos += direction){
            if(board.IsEmpty(pos)){
                yield return pos;
                continue;
            }

            Piece piece = board[pos];

            if(piece.Color != Color){
                yield return pos;
            }

            yield break;
        }
    }

    protected IEnumerable<Position> MovePositionsInDirs(Position from, Board board, Direction[] directions){
        return directions.SelectMany(dir => MovePositionsInDir(from, board, dir));
    }

    public virtual bool CanCaptureKing(Position from, Board board){
        return GetMoves(from, board).Any(move => {
            Piece piece = board[move.ToPos];
            return piece != null && piece.Type == PieceType.King;
            }
        );
    }
}

public class Pawn : Piece{

    public override PieceType Type => PieceType.Pawn;

    public override Player Color {get; }

    private readonly Direction forward;

    public Pawn(Player color){
        Color = color;

        if(color == Player.White){forward = Direction.North;}
        else{forward = Direction.South; }
    }

    public override Piece Copy(){
        Pawn copy = new(Color);
        copy.HasMoved = HasMoved;

        return copy;
    }

    private static bool CanMoveTo(Position pos, Board board){
        return Board.IsInside(pos) && board.IsEmpty(pos);
    }

    private bool CanCaptureAt(Position pos, Board board){
        if(!Board.IsInside(pos) || board.IsEmpty(pos)){
            return false;
        }
        else return board[pos].Color != Color;
    }

    public IEnumerable<Move> PromotionMoves(Position from, Position to){
        yield return new PawnPromotion(from, to, PieceType.Knight);
        yield return new PawnPromotion(from, to, PieceType.Bishop);
        yield return new PawnPromotion(from, to, PieceType.Rook);
        yield return new PawnPromotion(from, to, PieceType.Queen);
    }

    private IEnumerable<Move> ForwardMoves(Position from, Board board){
        Position oneMovePos = from + forward;
        if(CanMoveTo(oneMovePos, board)){

            if(oneMovePos.Row == 0 || oneMovePos.Row == Board.BoardSize - 1){
                foreach (Move move in PromotionMoves(from, oneMovePos)){
                    yield return move;
                }
            }
            else{
                yield return new NormalMove(from, oneMovePos);
            }

            Position twoMovesPos = from + 2*forward;
            if(!HasMoved && CanMoveTo(twoMovesPos, board)){
                yield return new NormalMove(from, twoMovesPos);
            }
        }
    }

    private IEnumerable<Move> DiagonalMoves(Position from, Board board){
        foreach(Direction dir in new Direction[]{Direction.West, Direction.East}){
            Position to = from + forward + dir;

            if(CanCaptureAt(to, board)){
                if(to.Row == 0 || to.Row == Board.BoardSize - 1){
                    foreach (Move move in PromotionMoves(from, to)){
                        yield return move;
                    }
                }
                else{
                    yield return new NormalMove(from, to);
                }
            }
        }
    }

    public override IEnumerable<Move> GetMoves(Position from, Board board){
        return ForwardMoves(from, board).Concat(DiagonalMoves(from, board));
    }

    public override bool CanCaptureKing(Position from, Board board)
    {
        return DiagonalMoves(from, board).Any(move =>{
            Piece piece = board[move.ToPos];
            return piece != null && piece.Type == PieceType.King;
        });
    }
}

public class Bishop : Piece{
    
    public override PieceType Type => PieceType.Bishop;

    public override Player Color {get; }

    private static readonly Direction[] dirs = new Direction[]{
        Direction.NorthWest,
        Direction.NorthEast,
        Direction.SouthWest,
        Direction.SouthEast
    };

    public Bishop(Player color){
        Color = color;
    }

    public override Piece Copy(){
        Bishop copy = new(Color);
        copy.HasMoved = HasMoved;

        return copy;
    }

    public override IEnumerable<Move> GetMoves(Position from, Board board)
    {
        return MovePositionsInDirs(from, board, dirs).Select(to => new NormalMove(from, to));
    }
    
}

public class Knight : Piece{

    public override PieceType Type => PieceType.Knight;

    public override Player Color {get; }

    public Knight(Player color){
        Color = color;
    }

    public override Piece Copy(){
        Knight copy = new(Color);
        copy.HasMoved = HasMoved;

        return copy;
    }

    private static IEnumerable<Position> PotentialToPositions(Position from){
        foreach(Direction vDir in new Direction[] { Direction.North, Direction.South }){
            foreach(Direction hDir in new Direction[] { Direction.West, Direction.East }){
                yield return from + 2*vDir + hDir;
                yield return from + 2*hDir + vDir;
            }
        }
    }

    private IEnumerable<Position> MovePositions(Position from, Board board){
        return PotentialToPositions(from).Where(pos => Board.IsInside(pos) && (board.IsEmpty(pos) || board[pos].Color != Color));
    }

    public override IEnumerable<Move> GetMoves(Position from, Board board)
    {
        return MovePositions(from, board).Select(to => new NormalMove(from, to));
    }
}

public class Rook : Piece{

    public override PieceType Type => PieceType.Rook;

    public override Player Color {get; }

    private static readonly Direction[] dirs = new Direction[]{
        Direction.North,
        Direction.East,
        Direction.South,
        Direction.West
    };

    public Rook(Player color){
        Color = color;
    }

    public override Piece Copy(){
        Rook copy = new(Color);
        copy.HasMoved = HasMoved;

        return copy;
    }

    public override IEnumerable<Move> GetMoves(Position from, Board board)
    {
        return MovePositionsInDirs(from, board, dirs).Select(to => new NormalMove(from, to));
    }
}

public class Queen : Piece{

    public override PieceType Type => PieceType.Queen;

    public override Player Color {get; }

    private static readonly Direction[] dirs = new Direction[]{
        Direction.North,
        Direction.East,
        Direction.South,
        Direction.West,
        Direction.NorthWest,
        Direction.NorthEast,
        Direction.SouthWest,
        Direction.SouthEast
    };

    public Queen(Player color){
        Color = color;
    }

    public override Piece Copy(){
        Queen copy = new(Color);
        copy.HasMoved = HasMoved;

        return copy;
    }

    public override IEnumerable<Move> GetMoves(Position from, Board board)
    {
        return MovePositionsInDirs(from, board, dirs).Select(to => new NormalMove(from, to));
    }

}

public class King : Piece{

    public override PieceType Type => PieceType.King;

    public override Player Color {get; }

    private static readonly Direction[] dirs = new Direction[]{
        Direction.North,
        Direction.East,
        Direction.South,
        Direction.West,
        Direction.NorthWest,
        Direction.NorthEast,
        Direction.SouthWest,
        Direction.SouthEast
    };

    public King(Player color){
        Color = color;
    }

    public override Piece Copy(){
        King copy = new(Color);
        copy.HasMoved = HasMoved;

        return copy;
    }

    private IEnumerable<Position> MovePositions(Position from, Board board){
        foreach (Direction dir in dirs){
            Position to = from + dir;
            if(!Board.IsInside(to)){
                continue;
            }

            if(board.IsEmpty(to) || board[to].Color != Color){
                yield return to;
            }
        }
    }

    public override IEnumerable<Move> GetMoves(Position from, Board board){
        foreach(Position to in MovePositions(from, board)){
            yield return new NormalMove(from, to);
        }
    }

    public override bool CanCaptureKing(Position from, Board board)
    {
        return MovePositions(from, board).Any(to => {
            Piece piece = board[to];
            return piece != null && piece.Type == PieceType.King;
        });
    }

}