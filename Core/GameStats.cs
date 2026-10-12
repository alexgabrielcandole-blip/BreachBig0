namespace BreachBigO.Core;

/// <summary>Things worth counting for the final scoreboard.</summary>
public enum GameEvent
{
    Hint,           // the player asked for a hint
    WrongAnswer,    // a wrong answer (costs trace)
    BruteForce,     // the player used BRUTE FORCE
    Traced          // trace hit 100% and the node restarted (raised by the window)
}

/// <summary>Everything we count for one node, across all of its attempts.</summary>
public class NodeStats
{
    public int Number { get; init; }
    public string Title { get; init; } = "";
    public int Hints { get; set; }
    public int Mistakes { get; set; }
    public int BruteForces { get; set; }
    public int TimesTraced { get; set; }
    public bool Cleared { get; set; }
    public int FinalTrace { get; set; }     // trace % at the moment the node was cleared
}

/// <summary>Collects the stats for a whole playthrough and turns them into a score and rank.</summary>
public class GameStats
{
    public const int StartScore = 1000;
    public const int BrutePenalty = 100;
    public const int HintPenalty = 10;
    public const int MistakePenalty = 5;
    public const int TracedPenalty = 150;

    private readonly Dictionary<int, NodeStats> _nodes = new();

    public IEnumerable<NodeStats> Nodes => _nodes.Values.OrderBy(n => n.Number);

    public int TotalHints => _nodes.Values.Sum(n => n.Hints);
    public int TotalMistakes => _nodes.Values.Sum(n => n.Mistakes);
    public int TotalBruteForces => _nodes.Values.Sum(n => n.BruteForces);
    public int TotalTraced => _nodes.Values.Sum(n => n.TimesTraced);

    public int Score => Math.Max(0,
        StartScore
        - BrutePenalty * TotalBruteForces
        - HintPenalty * TotalHints
        - MistakePenalty * TotalMistakes
        - TracedPenalty * TotalTraced);

    public (string Name, string Description) Rank => Score switch
    {
        >= 850 => ("GHOST", "The vault never even noticed you."),
        >= 650 => ("PHANTOM", "Barely a flicker in the logs."),
        >= 400 => ("HACKER", "You got in, with a few alarms going off."),
        _ => ("SCRIPT KIDDIE", "You got in, loudly. Review the five classes and try again!")
    };

    /// <summary>Gets (or creates) the stats for one node.</summary>
    public NodeStats For(int number, string title)
    {
        if (!_nodes.TryGetValue(number, out NodeStats? stats))
        {
            stats = new NodeStats { Number = number, Title = title };
            _nodes[number] = stats;
        }
        return stats;
    }

    public void Record(int number, string title, GameEvent e)
    {
        NodeStats stats = For(number, title);
        switch (e)
        {
            case GameEvent.Hint: stats.Hints++; break;
            case GameEvent.WrongAnswer: stats.Mistakes++; break;
            case GameEvent.BruteForce: stats.BruteForces++; break;
            case GameEvent.Traced: stats.TimesTraced++; break;
        }
    }

    public void Reset() => _nodes.Clear();
}