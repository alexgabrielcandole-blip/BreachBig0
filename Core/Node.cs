namespace BreachBigO.Core;

/// <summary>Colors the terminal can print in (Form1 maps these to real colors).</summary>
public enum TermColor { Normal, Player, Warn, Error, Dim, Success }

/// <summary>Everything a Node is allowed to ask the window to do.</summary>
public interface INodeHost
{
    StepCounter Steps { get; }
    TraceMeter Trace { get; }
    void Print(string text = "", TermColor color = TermColor.Normal);
    void SetNext(string text);      // the always-visible "NEXT:" bar
    void SetBest(int? best);        // the "best" half of "YOUR STEPS / BEST"
}

/// <summary>
/// One challenge: what to show, how to check the answer, what to say when solved.
/// </summary>
public class Question
{
    /// <summary>Prints the challenge. Must be safe to call again (the 'help' command re-shows it).</summary>
    public Action Ask { get; init; } = () => { };

    /// <summary>Returns null if the answer is correct, otherwise the feedback message.</summary>
    public Func<string, string?> Validate { get; init; } = _ => null;

    /// <summary>Runs once, right after a correct answer (show the "run", the aha moment).</summary>
    public Action? Solved { get; init; }

    /// <summary>Hints in order. Each one the player asks for costs trace.</summary>
    public string[] Hints { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Base class for all 5 nodes. It runs the list of questions; each node only
/// supplies its intro, its questions and its summary.
/// </summary>
public abstract class Node
{
    protected INodeHost Host { get; }

    private readonly List<Question> _questions = new();
    private int _index;        // which question the player is on
    private int _hintsUsed;    // hints used on the current question

    public abstract int Number { get; }
    public abstract string Title { get; }
    public abstract string BigO { get; }

    public bool IsComplete { get; private set; }
    public string Prompt => $"root@node{Number}>";

    protected Node(INodeHost host)
    {
        Host = host;
    }

    // ----- Each node fills these in -----
    protected abstract void Intro();
    protected abstract IEnumerable<Question> BuildQuestions();
    protected abstract void Summary();

    // ----- Called by Form1 -----
    public void Start()
    {
        Intro();
        _questions.AddRange(BuildQuestions());
        _index = 0;
        AskCurrent();
    }

    public void HandleInput(string input)
    {
        if (IsComplete)
        {
            Say("Node cleared. Type 'next' to continue.", TermColor.Warn);
            return;
        }

        string? error = _questions[_index].Validate(input);
        if (error != null)
        {
            Say(error, TermColor.Error);
            Say($"[TRACE +{TraceMeter.WrongAnswerCost}%]", TermColor.Dim);
            Host.Trace.Add(TraceMeter.WrongAnswerCost);
            return;
        }

        _questions[_index].Solved?.Invoke();
        _index++;
        _hintsUsed = 0;

        if (_index >= _questions.Count) Finish();
        else AskCurrent();
    }

    public void ShowHint()
    {
        if (IsComplete)
        {
            Say("No hints needed - node cleared.", TermColor.Dim);
            return;
        }

        string[] hints = _questions[_index].Hints;
        if (_hintsUsed >= hints.Length)
        {
            Say("[HINT] No more hints for this challenge.", TermColor.Dim);
            return;
        }

        Say("[HINT] " + hints[_hintsUsed], TermColor.Warn);
        Say($"[TRACE +{TraceMeter.HintCost}%]", TermColor.Dim);
        _hintsUsed++;
        Host.Trace.Add(TraceMeter.HintCost);
    }

    /// <summary>Re-shows the current challenge (used by 'help').</summary>
    public void Repeat()
    {
        if (!IsComplete) AskCurrent();
    }

    // ----- Internals -----
    private void AskCurrent()
    {
        Say();
        Say($"--- CHALLENGE {_index + 1}/{_questions.Count} ---", TermColor.Warn);
        _questions[_index].Ask();
    }

    private void Finish()
    {
        IsComplete = true;
        Summary();
        Say();
        Say("Type 'next' to move on.", TermColor.Warn);
        Host.SetNext("type 'next' to move on.");
    }

    // ----- Helpers for the nodes -----
    protected void Say(string text = "", TermColor color = TermColor.Normal) => Host.Print(text, color);

    /// <summary>Simulates running code: the step counter shows what that run cost.</summary>
    protected void Run(int steps)
    {
        Host.Steps.Reset();
        Host.Steps.Add(steps);
    }

    /// <summary>Removes spaces and a trailing semicolon so "numbers[ 2 ];" equals "numbers[2]".</summary>
    protected static string Norm(string s) =>
        new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray()).TrimEnd(';');

    /// <summary>
    /// True if the player typed the expression, assigned it (x = expr) or printed it
    /// (Console.WriteLine(expr)).
    /// </summary>
    protected static bool MatchesCode(string input, string expr)
    {
        string s = Norm(input);
        expr = Norm(expr);
        return s == expr || s.EndsWith("=" + expr) || s == $"Console.WriteLine({expr})";
    }

    /// <summary>
    /// Checks "arrayName[target]" and gives specific feedback for common mistakes.
    /// Returns null when correct.
    /// </summary>
    protected static string? CheckIndexAnswer(string input, string arrayName, int target, string[] values)
    {
        if (MatchesCode(input, $"{arrayName}[{target}]")) return null;

        string s = Norm(input);

        // Typed the value instead of the code?
        if (s.Length > 0 && (char.IsDigit(s[0]) || s[0] == '"'))
            return $"That's a value. I need the C# that fetches it, like {arrayName}[position].";

        // Right name, wrong capitals? (C# is case-sensitive)
        if (s.StartsWith(arrayName, StringComparison.OrdinalIgnoreCase) &&
            !s.StartsWith(arrayName, StringComparison.Ordinal))
            return $"C# is case-sensitive. The array is called {arrayName}.";

        // arrayName[k] with the wrong k?
        int open = s.IndexOf('[');
        int close = s.IndexOf(']');
        if (s.StartsWith(arrayName, StringComparison.Ordinal) && open > 0 && close > open + 1 &&
            int.TryParse(s[(open + 1)..close], out int k))
        {
            if (k == target + 1) return "Off by one. Positions start counting at 0, not 1.";
            if (k < 0 || k >= values.Length)
                return $"Position {k} doesn't exist (valid: 0 to {values.Length - 1}). That would crash!";
            return $"{arrayName}[{k}] gives {values[k]}, not the target.";
        }

        return $"Not quite. Use the form {arrayName}[position].";
    }
}