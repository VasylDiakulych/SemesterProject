using ChessLogic;
using System.Collections.Generic;
using System.Linq;

namespace ChessLogic;

public abstract class ChessAI{
    public abstract Opponent AIType  { get; } 
    protected GameState game;
    public abstract Move ChooseMove();
    public abstract void HandleMove();
    
    public static ChessAI ReturnAI(Opponent aiType, GameState game) {
        return aiType switch
        {
            Opponent.RandomAI => new RandomAI(game),
            Opponent.MiniMaxAI => new MiniMaxAI(game),
            _ => new RandomAI(game),
        };
        ;
    }
}

public class RandomAI : ChessAI
{
    public override Opponent AIType => Opponent.RandomAI;

    public RandomAI(GameState game){
        this.game = game;
    }

    public override Move ChooseMove(){
        IEnumerable<Move> moves = game.AllLegalMovesFor(game.CurrentPlayer);
        List<Move> movesList = moves.ToList();

        var random = new Random();

        return movesList[random.Next(movesList.Count - 1)];
    }

    public override void HandleMove()
    {
        if(game.Result == null && game.CurrentPlayer == game.OpponentColor){
            game.MakeMove(ChooseMove());
        }
    }
}

public class MiniMaxAI : ChessAI
{
    public override Opponent AIType => Opponent.MiniMaxAI;

    public MiniMaxAI(GameState game){
        this.game = game;
    }

    public override Move ChooseMove(){
        IEnumerable<Move> moves = game.AllLegalMovesFor(game.CurrentPlayer);
        List<Move> movesList = moves.ToList();

        var random = new Random();

        return movesList[random.Next(movesList.Count - 1)];
    }

    public override void HandleMove()
    {
        if(game.CurrentPlayer == game.OpponentColor){
            game.MakeMove(ChooseMove());
        }
    }
}
