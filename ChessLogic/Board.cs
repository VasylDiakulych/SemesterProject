
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
}