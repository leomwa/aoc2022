const string inputFileName = "strategy-guide-input.txt";
var fileReader = File.OpenText(inputFileName);

Game game = new();

while (true)
{
    var lineItem = await fileReader.ReadLineAsync();
    if (lineItem is null)
    {
        break;
    }

    var hands = lineItem.Split(' ');
    string opponentHand = hands[0];
    string desiredOutcome = hands[1];

    Round round = game.NewRound();
    round.OpponentPlays(opponentHand);
    int score = round.GetScoreForDesiredOutcome(desiredOutcome);

    game.RecordScore(score);
}

game.End();
int myTotalScore = game.GetMyTotalScore();

Console.WriteLine($"If everything goes to plan my total score will be: {myTotalScore}");

Console.ReadLine();


internal class Hand
{
    internal Shape Shape { get; }

    private Hand(Shape shape)
    {
        Shape = shape;
    }

    internal static readonly Hand RockInstance = new(Rock.Shape);
    internal static readonly Hand PaperInstance = new(Paper.Shape);
    internal static readonly Hand ScissorsInstance = new(Scissors.Shape);
    internal static readonly Hand NoneInstance = new(None.Shape);
}

internal class Shape
{
    internal int Points;
}

internal class None : Shape
{
    private None()
    {
        Points = 0;
    }

    internal static None Shape { get; } = new();
}

internal class Rock : Shape
{
    private Rock()
    {
        Points = 1;
    }

    internal static Rock Shape { get; } = new();
}

internal class Paper : Shape
{
    private Paper()
    {
        Points = 2;
    }

    internal static Paper Shape { get; } = new();
}

internal class Scissors : Shape
{
    private Scissors()
    {
        Points = 3;
    }

    internal static Scissors Shape { get; } = new();
}

internal static class ScoreChart
{
    private static readonly Dictionary<Play, Hand> ScoreMap = new()
    {
        [Rock.Shape.Versus(Scissors.Shape)] = Hand.RockInstance,
        [Scissors.Shape.Versus(Rock.Shape)] = Hand.RockInstance,

        [Scissors.Shape.Versus(Paper.Shape)] = Hand.ScissorsInstance,
        [Paper.Shape.Versus(Scissors.Shape)] = Hand.ScissorsInstance,

        [Paper.Shape.Versus(Rock.Shape)] = Hand.PaperInstance,
        [Rock.Shape.Versus(Paper.Shape)] = Hand.PaperInstance,

        [Rock.Shape.Versus(Rock.Shape)] = Hand.NoneInstance,
        [Paper.Shape.Versus(Paper.Shape)] = Hand.NoneInstance,
        [Scissors.Shape.Versus(Scissors.Shape)] = Hand.NoneInstance
    };

    internal static Hand GetScore(Play play)
    {
        return ScoreMap[play];
    }
}

internal record struct Play(Shape Player1, Shape Player2);

internal static class Extensions
{
    internal static Play Versus(this Shape a, Shape b)
    {
        return new Play(a, b);
    }
}

internal record Round
{
    private enum Outcome
    {
        Lose,
        Draw,
        Win
    }

    private static readonly Dictionary<string, Shape> InputShapeMap = new()
    {
        ["A"] = Rock.Shape,
        ["B"] = Paper.Shape,
        ["C"] = Scissors.Shape
    };

    private static readonly Dictionary<string, Outcome> InputStrategyMap = new()
    {
        ["X"] = Outcome.Lose,
        ["Y"] = Outcome.Draw,
        ["Z"] = Outcome.Win
    };

    private Shape OpponentHand { get; set; } = Hand.NoneInstance.Shape;

    public void OpponentPlays(string hand)
    {
        OpponentHand = InputShapeMap[hand];
    }

    public int GetScoreForDesiredOutcome(string desiredOutcome)
    {
        int myScore = 0;

        Outcome outcome = InputStrategyMap[desiredOutcome];
        switch (outcome)
        {
            case Outcome.Lose:
                myScore = GetScoreByDesiredOutcome(OpponentHand, 0);
                break;

            case Outcome.Draw: // just copy his points for his selected hand and add 3 for a tie
                myScore = 3 + OpponentHand.Points;
                break;

            case Outcome.Win:
                myScore = GetScoreByDesiredOutcome(None.Shape, 6);
                break;
        }

        return myScore;
    }

    private int GetScoreByDesiredOutcome(Shape compare, int outcomePoints)
    {
        int score = outcomePoints;
        foreach (Hand myHand in new[] { Hand.RockInstance, Hand.PaperInstance, Hand.ScissorsInstance, })
        {
            var play = OpponentHand.Versus(myHand.Shape);
            var scoreResult = ScoreChart.GetScore(play);
            if (scoreResult.Shape != (compare is None ? myHand.Shape : compare)) continue;
            score = outcomePoints + myHand.Shape.Points;
            break;
        }

        return score;
    }
}

internal class Game
{
    private int _mySoreTally;
    private int _rounds;

    internal Round NewRound()
    {
        ++_rounds;
        Console.WriteLine($"Starting new round #{_rounds}");
        return new Round();
    }

    public void RecordScore(int score)
    {
        Console.WriteLine($"New score: {score} being added to current: {_mySoreTally}");
        _mySoreTally += score;
    }

    public void End()
    {
        Console.WriteLine("The game has ended");
    }

    public int GetMyTotalScore()
    {
        return _mySoreTally;
    }
}