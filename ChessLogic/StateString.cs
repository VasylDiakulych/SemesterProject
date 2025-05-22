using System.Text;

namespace ChessLogic;

public class StateString{
    private readonly StringBuilder sb = new StringBuilder();

    public StateString(Player player, Board board){
        AddPiecePlacement(board, sb);
        sb.Append(' ');
        AddCurrentPlayer(player, sb);
        sb.Append(' ');
        AddCastlingRights(board, sb);
        sb.Append(' ');
        AddEnPassant(board, player, sb);
    }

    public static string SimpleStateString(Player player, Board board)
    {
        StringBuilder ssb = new();
        AddPiecePlacement(board, ssb);
        ssb.Append(' ');
        AddCurrentPlayer(player, ssb);
        return ssb.ToString();
    }

    public override string ToString()
    {
        return sb.ToString();
    }

    private static char PieceChar(Piece piece){
        char c = piece.Type switch{
            PieceType.Pawn => 'p',
            PieceType.Knight => 'n',
            PieceType.Bishop => 'b',
            PieceType.Rook => 'r',
            PieceType.Queen => 'q',
            PieceType.King => 'k',
            _ => ' '
        };

        if(piece.Color == Player.White){
            return char.ToUpper(c);
        }

        return c;
    }

    private static void AddRowData(Board board, int row, StringBuilder sb){
        int empty = 0;
        for(int i = 0; i < Board.BoardSize; i++){
            if(board[row, i] == null){
                empty++;
                continue;
            } 

            if(empty > 0){
                sb.Append(empty);
                empty = 0;
            }

            sb.Append(PieceChar(board[row, i]));
        }

        if(empty > 0){
            sb.Append(empty);
        }
    }

    private static void AddPiecePlacement(Board board, StringBuilder sb){
        for(int i = 0; i<Board.BoardSize; i++){
            if( i != 0){
                sb.Append('/');
            }
            AddRowData(board, i, sb);
        }
    }

    private static void AddCurrentPlayer(Player currentPlayer, StringBuilder sb){
        if(currentPlayer == Player.White){
            sb.Append('w');
        }
        else{
            sb.Append('b');
        }
    }

    private void AddCastlingRights(Board board, StringBuilder sb){

        bool castleWKS = board.CastleRightKS(Player.White);
        bool castleBKS = board.CastleRightKS(Player.Black);
        bool castleWQS = board.CastleRightQS(Player.White);
        bool castleBQS = board.CastleRightQS(Player.Black);

        if(!(castleBKS || castleBQS || castleWKS || castleWQS)){
            sb.Append('-');
            return;
        }

        if(castleWKS){
            sb.Append('K');
        }

        if(castleWQS){
            sb.Append('Q');
        }

        if(castleBKS){
            sb.Append('k');
        }

        if(castleBQS){
            sb.Append('q');
        }

    }

    private void AddEnPassant(Board board, Player player, StringBuilder bs){
        if(!board.CanCaptureEnPassant(player)){
            sb.Append('-');
            return;
        }

        Position pos = board.GetEnPassantSquares(player.Opponent());
        char file = (char)('a' + pos.Column);
        int rank = Board.BoardSize - pos.Row;
        sb.Append(file);
        sb.Append(rank);

    }
}
