
using System.Drawing;

namespace ChessLogic;

public class Board{
    public static int BoardSize { get; set; }
    private readonly Piece[,] pieces = new Piece[BoardSize, BoardSize];

    private readonly Dictionary<Player, Position> EnPassantSquares = new Dictionary<Player, Position>{
        {Player.White, null},
        {Player.Black, null}
    };

    public Piece this[int row, int col]{

        get{ return pieces[row, col]; }
        set{ pieces[row, col] = value; }
    }

    public Piece this[Position pos]{

        get{ return pieces[pos.Row, pos.Column]; }
        set{ pieces[pos.Row, pos.Column] = value; }

    }

    public Position GetEnPassantSquares(Player player){
        return EnPassantSquares[player];
    }

    public void SetEnPassantSquares(Player player, Position position){
        EnPassantSquares[player] = position;
    }

    public static Board Initial(string startingPos, int boardSize = 8 ){
        BoardSize = boardSize;
        Board board = new Board();
        board.AddStartPieces(startingPos);
        return board;
    }

    //Adds pieces to the board as starting position. 
    //As input takes path to .txt file, which contains the custom starting position 
    private void AddStartPieces(string startingPos){
        var lines = File.ReadAllLines(startingPos);
        for(int i = 0; i < BoardSize; i++){
            string line = lines[i];
            char[] chars = line.ToCharArray();
            for(int j = 0; j < BoardSize; j++){
                
                Player color;

                if(char.IsLetter(chars[j])){
                    color = char.IsLower(chars[j]) ? Player.Black : Player.White;
                }
                else{ continue; }

                switch(char.ToLower(chars[j])){
                    case 'p':
                        pieces[i, j] = new Pawn(color);
                        break;
                    case 'b':
                        pieces[i, j] = new Bishop(color);
                        break;
                    case 'n':
                        pieces[i, j] = new Knight(color);
                        break;
                    case 'r':
                        pieces[i, j] = new Rook(color);
                        break;
                    case 'q':
                        pieces[i, j] = new Queen(color);
                        break;
                    case 'k':
                        pieces[i, j] = new King(color);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public static bool IsInside(Position pos){
        return pos.Row >= 0 && pos.Row < BoardSize && pos.Column >= 0 && pos.Column < BoardSize;
    }

    public bool IsEmpty(Position pos){
        return this[pos] == null;
    }

    public IEnumerable<Position> PiecePositions(){
        for(int r = 0; r < BoardSize; r++){
            for(int c = 0; c < BoardSize; c++){
                Position pos = new Position(r, c);
                if(this[pos] != null){
                    yield return pos;
                }
            }
        }
    }

    public IEnumerable<Position> PiecePosCol (Player color){
        IEnumerable<Position> positions = PiecePositions();
        return positions.Where(pos => this[pos].Color == color);
    }

    public bool IsInCheck(Player player){
        return PiecePosCol(player.Opponent()).Any(pos =>{
            Piece piece = this[pos];
            return piece.CanCaptureKing(pos, this);
        });
    }

    public Board Copy(){
        Board copy = new Board();
        foreach (Position pos in PiecePositions()){
            copy[pos] = this[pos].Copy();
        }
        return copy;
    }

    public Counting CountPieces(){
        Counting counting = new();

        foreach (Position pos in PiecePositions()){
            Piece piece = this[pos];
            counting.Increment(piece.Color, piece.Type);
        }

        return counting;
    }

    public bool InsufficientMaterial(){
        Counting counting = CountPieces();

        return IsKingVKing(counting) || IsKingBishopVKing(counting) || 
               IsKingBishopVKingBishop(counting) || IsKingKnightVKing(counting);
    } 

    private static bool IsKingVKing(Counting counting){
        return counting.TotalCount == 2;
    }

    private static bool IsKingBishopVKing(Counting counting){
        return counting.TotalCount == 3 && (counting.White(PieceType.Bishop) == 1 || counting.Black(PieceType.Bishop) == 1);
    }

    private static bool IsKingKnightVKing(Counting counting){
        return counting.TotalCount == 3 && (counting.White(PieceType.Knight) == 1 || counting.Black(PieceType.Knight) == 1);
    }

    private bool IsKingBishopVKingBishop(Counting counting){
        if(counting.TotalCount != 4){
            return false;
        }

        if(counting.White(PieceType.Bishop) != 1 || counting.Black(PieceType.Bishop) != 1){
            return false;
        }

        Position wBishopPos = FindPiece(Player.White, PieceType.Bishop);
        Position bBishopPos = FindPiece(Player.Black, PieceType.Bishop);

        return wBishopPos.SquareColor() == bBishopPos.SquareColor();
    }

    private Position FindPiece(Player color, PieceType type){
        return PiecePosCol(color).First(pos => this[pos].Type == type);
    }
    
    private bool IsUnmovedKingAndRook(Position kingPos, Position rookPos){
        if(IsEmpty(kingPos) || IsEmpty(rookPos)){
            return false;
        }

        Piece king = this[kingPos];
        Piece rook = this[rookPos];
        return (king.Type == PieceType.King && rook.Type == PieceType.Rook) && !king.HasMoved && !rook.HasMoved;
    }

    private Position findRook(Position from, Direction dir){
        Player color = this[from].Color;
        while(this[from] == null || this[from].Type != PieceType.Rook || this[from].Color != color){
            from += dir;

            if(!IsInside(from)){
                return null;
            }
        }
        return from;
    }

    public bool CastleRightKS(Player player){
        Position kingPos = FindPiece(player, PieceType.King);
        Position? rookPos = findRook(kingPos, Direction.East);
        if(rookPos == null){return false;}

        if(IsUnmovedKingAndRook(kingPos, rookPos)){
            return true;
        }
        else{
            return false;
        }
    }

    public bool CastleRightQS(Player player){
        Position kingPos = FindPiece(player, PieceType.King);
        Position? rookPos = findRook(kingPos, Direction.West);
        if(rookPos == null){return false;}

        if(IsUnmovedKingAndRook(kingPos, rookPos)){
            return true;
        }
        else{
            return false;
        }
    }

    private bool HasPawnInPosition(Player player, Position[] pawnPositions, Position enPassantSquare){
        foreach(Position pos in pawnPositions.Where(IsInside)){
            Piece piece = this[pos];
            if(piece == null || piece.Color == player.Opponent() || piece.Type != PieceType.Pawn){
                continue;
            }

            EnPassant move = new EnPassant(pos, enPassantSquare);
            if(move.IsLegal(this)){
                return true;
            }
        }
        return false;
    }

    public bool CanCaptureEnPassant(Player player){
        Position enPassantSquare = GetEnPassantSquares(player.Opponent());
        if(enPassantSquare == null){
            return false;
        }
        
        Position[] pawnPositions = player switch{
            Player.White => [enPassantSquare + Direction.SouthWest, enPassantSquare + Direction.SouthEast],
            Player.Black => [enPassantSquare + Direction.NorthWest, enPassantSquare + Direction.NorthEast],
            _ => Array.Empty<Position>()
        };

        return HasPawnInPosition(player, pawnPositions, enPassantSquare);
    }
}
