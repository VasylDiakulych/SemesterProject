
using Gdk;
using Gtk;
using Cairo;
using ChessLogic;

public class MainMenuWindow : Gtk.Window {
    public MainMenuWindow() : base("Chess")
    {
        SetDefaultSize(500, 500);
        SetPosition(WindowPosition.Center);
        string iconPath = System.IO.Path.Combine("Assets", "icon.png");
        Icon = new Pixbuf(iconPath);
        DeleteEvent += (o, args) => Application.Quit();

        Box box = new Box(Orientation.Vertical, 10){
            BorderWidth = 20
        };

        Button newGameButton = new Button("New Game");
        newGameButton.Clicked += (sender, e) =>
        {
            MainWindow gameWindow = new MainWindow();
            gameWindow.ShowAll();
            this.Hide();
        };

        Button quitButton = new Button("Quit");
        quitButton.Clicked += (sender, e) => Application.Quit();
        
        Button loadPosition = new Button("New game with custom position");
        loadPosition.Clicked += (sender, e) => OnLoadPositionClicked ();

        box.PackStart(newGameButton, true, true, 10);
        box.PackStart(loadPosition, true, true, 10);
        box.PackStart(quitButton, true, true, 10);

        Add(box);
        ShowAll();
    }

    public void OnLoadPositionClicked(){
        FileChooserDialog fileChooser = new FileChooserDialog(
            "Choose position file",
            this,
            FileChooserAction.Open,
            "Cancel", ResponseType.Cancel,
            "Open", ResponseType.Accept
        );

        FileFilter filter = new FileFilter();
        filter.Name = "Valid position files (*.txt)";
        filter.AddPattern("*.txt");
        fileChooser.AddFilter(filter);

        if (fileChooser.Run() == (int)ResponseType.Accept)
        {
            string filePath = fileChooser.Filename;

            try{
                MainWindow customGameWindow = new MainWindow(filePath);
                customGameWindow.ShowAll();
                this.Hide();
            }
            catch (Exception ex){
                Console.WriteLine("Error loading position: " + ex.Message);
                MessageDialog errorDialog = new MessageDialog(
                    this,
                    DialogFlags.Modal,
                    MessageType.Error,
                    ButtonsType.Close,
                    "Failed to load position file."
                );
                errorDialog.Run();
                errorDialog.Destroy();
            }
        }
        fileChooser.Destroy();
    }
}

public class GameEndWindow : Gtk.Window{
    public GameEndWindow(ChessLogic.Result result) : base("Chess"){
        SetDefaultSize(500, 500);
        SetPosition(WindowPosition.Center);
        Icon = new Pixbuf("Assets/icon.png");

        string endString = "";
        if(result.Reason == EndReason.Checkmate){
            endString += result.Winner.ToString() + " won!";
        }
        else{
            endString += "Draw, " + result.Reason.ToString();
        }

        Label endLabel = new Label(endString);
        
        Button quitButton = new Button("Quit");
        quitButton.Clicked += (sender, e) => Application.Quit();

        Button newGameButton = new Button("New Game");
        newGameButton.Clicked += (sender, e) =>
        {
          MainWindow gameWindow = new MainWindow();
          gameWindow.ShowAll();
          this.Hide();
        };

        Button mainMenuButton = new Button("Main Menu");
        mainMenuButton.Clicked += (sender, e) =>
        {
            MainMenuWindow mainMenu = new MainMenuWindow();
            mainMenu.ShowAll();
            this.Hide();
        };

        Box box = new Box(Orientation.Vertical, 20);
        
        box.PackStart(endLabel, true, true, 10);
        box.PackStart(newGameButton, true, true, 10);
        box.PackStart(mainMenuButton, true, true, 10);
        box.PackStart(quitButton, true, true, 10);
        
        Add(box);

        DeleteEvent += (o, args) => Application.Quit();
    }
}

class MainWindow : Gtk.Window {
    private Pixbuf backgroundImage;
    private DrawingArea drawingArea;
    private const int squareSize = 72;
    GameState game;

    private Position selectedPiece;
    private readonly Dictionary<Position, Move> CachedMoves = new();

    private Board board = new();

    public MainWindow(string startingPositionPath = "ChessLogic\\standardPosition.txt") : base("Chess")
    {
        SetDefaultSize(594, 594);
        SetPosition(WindowPosition.Center);
        Icon = new Pixbuf(System.IO.Path.Combine("Assets", "icon.png"));

        board = Board.Initial(startingPositionPath);
        game = new(Player.White, board); 

        drawingArea = new DrawingArea();
        Add(drawingArea);

        string path = System.IO.Path.Combine("Assets", "Board.png");
        backgroundImage = new Pixbuf(path);
        drawingArea.Drawn += OnDrawingAreaDrawn;
        drawingArea.ButtonPressEvent += OnButtonPressEvent;
        drawingArea.AddEvents((int)EventMask.ButtonPressMask);

        ShowAll();
    }

    private void OnDrawingAreaDrawn(object sender, DrawnArgs args){
        Context cr = args.Cr;
        Gdk.CairoHelper.SetSourcePixbuf(cr, backgroundImage, 0, 0);
        cr.Paint();
        DrawPieces(cr);
        DrawHighlights(cr);
    }

    private void DrawPieces(Context cr)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece piece = board[row, col];
                if (piece != null)
                {
                    string filename = $"{piece.Type}{(piece.Color == Player.White? "W" : "B")}.png";
                    string path = System.IO.Path.Combine("Assets", filename);
                    Pixbuf pieceImage = new Pixbuf(path);

                    Gdk.CairoHelper.SetSourcePixbuf(cr, pieceImage, col * squareSize + 10, row * squareSize + 10);
                    cr.Paint();
                }
            }
        }
    }

    private void DrawHighlights(Context cr){
        string path = System.IO.Path.Combine("Assets", "highlight.png");
        Pixbuf pieceImage = new Pixbuf(path);

        foreach (Position to in CachedMoves.Keys){
            Gdk.CairoHelper.SetSourcePixbuf(cr, pieceImage, to.Column * squareSize + 9, to.Row * squareSize + 9);
            cr.Paint();
        }
    }

    private void OnButtonPressEvent(object sender, ButtonPressEventArgs args)
    {
        int x = (int)args.Event.X - 10;
        int y = (int)args.Event.Y - 10;

        int row = y / squareSize;
        int col = x / squareSize;

        if(row >= Board.BoardSize || col >= Board.BoardSize){return;}
        Position pos = new Position(row, col);

        if(selectedPiece == null){
            OnFromPosSel(pos);
        }
        else{
            OnToPosSel(pos);
        }
    }

    private void OnFromPosSel(Position pos){
        IEnumerable<Move> moves = game.LegalMovesForPiece(pos);

        if(moves.Any()){
            selectedPiece = pos;
            CacheMoves(moves);
            QueueDraw();
        }
    }

    private void OnToPosSel(Position pos){
        selectedPiece = null;

        if (CachedMoves.TryGetValue(pos, out Move move)){

            if(move.Type == MoveType.PawnPromotion){
                HandlePromotion(move.FromPos, move.ToPos);
            }
            else{ 
                HandleMove(move);
            }
        }

        ClearHighlights();
    }

    private void HandleMove(Move move){
        game.MakeMove(move);
        if(game.Result != null){
            GameEndWindow endWindow = new(game.Result);
            this.Hide();
            endWindow.ShowAll();
            Console.WriteLine($"The result is: {game.Result.Reason}");
        }
        QueueDraw();
    }

    private void HandlePromotion(Position from, Position to){
        PromotionWindow window = new PromotionWindow(game.CurrentPlayer);
        window.ShowAll();
        window.PieceSelected += type => {
            window.Hide();
            Move promMove = new PawnPromotion(from, to, type);
            HandleMove(promMove);
        };
    }

    private void ClearHighlights()
    {
        CachedMoves.Clear();
        QueueDraw();
    }

    private void CacheMoves(IEnumerable<Move> moves){
        CachedMoves.Clear();

        foreach (Move move in moves){
            CachedMoves[move.ToPos] = move;
        }
    }

    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}

class PromotionWindow : Gtk.Window {
    public event Action<PieceType> PieceSelected;

    public PromotionWindow(Player color) : base("Promotion"){
        SetDefaultSize(72, 72*4 + 36);
        SetPosition(WindowPosition.Mouse);
        Resizable = false;
        Decorated = false;
        DeleteEvent += (o, args) => this.Hide();

        DrawingArea drawingArea = new DrawingArea();
        drawingArea.SetSizeRequest(20, 72*4 + 36);
        drawingArea.Drawn += (o, args) => DrawPieces(args.Cr, color);
        drawingArea.ButtonPressEvent += OnButtonPress;
        drawingArea.Events |= Gdk.EventMask.ButtonPressMask;

        Add(drawingArea);
        ShowAll();
    }

    private void DrawPieces(Cairo.Context cr, Player color)
    {
        PieceType[] promPieces = [PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight];

        for (int i = 0; i < promPieces.Length; i++)
        {
            PieceType piece = promPieces[i];
            string filename = piece + (color == Player.White ? "W" : "B") + ".png";
            string path = System.IO.Path.Combine("Assets", filename);

            Pixbuf pixbuf = new Pixbuf(path);
            Gdk.CairoHelper.SetSourcePixbuf(cr, pixbuf, 36, i * 72);
            cr.Paint();
        }
        string pathButton = System.IO.Path.Combine("Assets", "BACK.png");
        Pixbuf pixbufButton = new Pixbuf(pathButton);
        Gdk.CairoHelper.SetSourcePixbuf(cr, pixbufButton, 36, promPieces.Length * 72);
        cr.Paint();

    }

    private void OnButtonPress(object o, ButtonPressEventArgs args)
    {
        double x = args.Event.X;
        double y = args.Event.Y;

        int index = (int) y/72;
        if(index >= 0 && index <= 3){
            switch(index){
                case 0:
                    QueenSelected(o, args);
                    break;
                case 1:
                    RookSelected(o, args);
                    break;
                case 2:
                    BishopSelected(o, args);
                    break;
                default:
                    KnightSelected(o, args);
                    break;
            };
        }
        else{
            this.Hide();
        }
    }

    public void QueenSelected(object o, ButtonPressEventArgs args){
        PieceSelected?.Invoke(PieceType.Queen);
    }

    public void KnightSelected(object o, ButtonPressEventArgs args){
        PieceSelected?.Invoke(PieceType.Knight);
    }

    public void BishopSelected(object o, ButtonPressEventArgs args){
        PieceSelected?.Invoke(PieceType.Bishop);
    }

    public void RookSelected(object o, ButtonPressEventArgs args){
        PieceSelected?.Invoke(PieceType.Rook);
    }
}

class Chess {
    static void Main() {
        Application.Init();
        MainMenuWindow w = new();
        w.ShowAll();
        Application.Run();
    }
}