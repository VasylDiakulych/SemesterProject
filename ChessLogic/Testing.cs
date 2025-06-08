using System.Diagnostics;

namespace ChessLogic;

// testing of time needed for a move
public class AITesting
{
    public static void AItest(Opponent ai)
    {
        Board board = new Board();
        board = Board.Initial("C:\\Users\\Diakjulych Vasyl\\Desktop\\SemesterProject\\ChessUI\\ChessLogic\\testPosition.txt");
        GameState game = new GameState(Player.White, board, ai, Player.White);

        Stopwatch sw = new();
        sw.Start();

        for (int i = 0; i < 10; i++)
        {

            ChessAI AI = ChessAI.ReturnAI(ai, game);
            Move move = AI.ChooseMove();
            Console.WriteLine($"Move chosen: {move}, Time: {sw.ElapsedMilliseconds}ms");
            sw.Restart();
        }
    }
}