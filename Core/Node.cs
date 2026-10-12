namespace BreachBigO.Core;

/// <summary>Colors the terminal can print in (Form1 maps these to real colors).</summary>
public enum TermColor { Normal, Player, Warn, Error, Dim, Success }

/// <summary>Result of one typed answer in a multi-step challenge.</summary>
public enum Verdict
{
    Wrong,      // invalid or incorrect: the node already said why; costs trace
    Continue,   // valid step, challenge not finished (the node already gave feedback)
    Solved      // challenge finished: move on
}

/// <summary>Everything a Node is allowed to ask the window to do.</summary>
public interface INodeHost
{
    StepCounter Steps { get; }
    TraceMeter Trace { get; }
    void Print(string text = "", TermColor color = TermColor.Normal);
    void SetNext(string text);          // the always-visible "NEXT:" bar
    void SetBest(int? best);            // the "best" half of "YOUR STEPS / BEST"
    void Record(GameEvent e);           // feeds the final scoreboard
}

/// <summary>
/// One challenge: what to show, how to check the answer, what to say when solved.
/// Single-answer challenges use Validate. Multi-step challenges use Judge.
/// </summary>
public class Question
{
    /// <summary>Prints the challenge. Must be safe to call again (the 'help' command re-shows it).</summary>
    public Action Ask { get; init; } = () => { };

    /// <summary>Single answer: returns null if correct, otherwise the feedback message.</summary>
    public Func<string, string?> Validate { get; init; } = _ => null;

    /// <summary>
    /// Multi-step challenge (used instead of Validate when set). The node prints its own
    /// feedback and returns Wrong, Continue or Solved.
    /// </summary>
    public Func<string, Verdict>? Judge { get; init; }

    /// <summary>Runs once, right after the challenge is solved (show the "run", the aha moment).</summary>
    public Action? Solved { get; init; }

    /// <summary>Fixed hints in order. Each one the player asks for costs trace.</summary>
    public string[] Hints { get; init; } = Array.Empty<string>();

    /// <summary>Dynamic hints (they can use the current state). Return null when out of hints.</summary>
    public Func<int, string?>? HintProvider { get; init; }

    /// <summary>
    /// BRUTE FORCE: prints the answer AND why, and sets the step counter to the cost of
    /// the slow route. Optional: a challenge without it can't be brute-forced.
    /// </summary>
    public Action? Reveal { get; init; }
}

/// <summary>
/// Base class for all 5 nodes. It runs the list of questions; each node only
/// supplies its intro, its questions and its summary.
/// </summary>
public abstract class Node
{
    /// <summary>Trace cost of the BRUTE FORCE button.</summary>
    public const int BruteForceCost = 25;

    protected INodeHost Host { get; }

    private readonly List<Question> _questions = new();
    private int _index;            // which question the player is on
    private int _hintsUsed;        // hints used on the current question (or step)
    private int _bruteForced;      // how many challenges were brute-forced

    public abstract int Number { get; }
    public abstract string Title { get; }
    public abstract string BigO { get; }

    public bool IsComplete { get; private set; }
    public string Prompt => $"root@node{Number}>";

    /// <summary>How many challenges this node has.</summary>
    public int ChallengeCount => _questions.Count;

    /// <summary>True when the current challenge has a Reveal.</summary>
    public bool CanBruteForce =>
        !IsComplete && _index < _questions.Count && _questions[_index].Reveal != null;

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

        Question q = _questions[_index];

        if (q.Judge != null)
        {
            // Multi-step challenge: the node decides what happens.
            Verdict verdict = q.Judge(input);
            if (verdict == Verdict.Wrong) { PenalizeWrong(); return; }
            if (verdict == Verdict.Continue) return;
        }
        else
        {
            string? error = q.Validate(input);
            if (error != null)
            {
                Say(error, TermColor.Error);
                PenalizeWrong();
                return;
            }
        }

        q.Solved?.Invoke();
        MoveToNextQuestion();
    }

    public void ShowHint()
    {
        if (IsComplete)
        {
            Say("No hints needed - node cleared.", TermColor.Dim);
            return;
        }

        Question q = _questions[_index];
        string? hint = q.HintProvider != null
            ? q.HintProvider(_hintsUsed)
            : (_hintsUsed < q.Hints.Length ? q.Hints[_hintsUsed] : null);

        if (hint == null)
        {
            Say("[HINT] No more hints for this challenge.", TermColor.Dim);
            return;
        }

        Say("[HINT] " + hint, TermColor.Warn);
        _hintsUsed++;
        Host.Record(GameEvent.Hint);
        AddTrace(TraceMeter.HintCost);
    }

    /// <summary>
    /// BRUTE FORCE: pays the trace cost, shows the answer and the explanation, and moves on.
    /// Form1 asks the player to confirm before calling this.
    /// </summary>
    public void BruteForce()
    {
        if (!CanBruteForce)
        {
            Say("Brute force isn't available for this challenge.", TermColor.Dim);
            return;
        }

        _bruteForced++;
        Host.Record(GameEvent.BruteForce);
        Say("[BRUTE FORCE] Skipping the thinking...", TermColor.Warn);
        AddTrace(BruteForceCost);
        _questions[_index].Reveal!();
        MoveToNextQuestion();
    }

    /// <summary>Re-shows the current challenge (used by 'help').</summary>
    public void Repeat()
    {
        if (!IsComplete) AskCurrent();
    }

    // ----- Internals -----
    private void MoveToNextQuestion()
    {
        _index++;
        _hintsUsed = 0;

        if (_index >= _questions.Count) Finish();
        else AskCurrent();
    }

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

        if (_bruteForced == 0)
            Say($"Brute-forced challenges: 0/{_questions.Count} - a clean run!", TermColor.Success);
        else
            Say($"Brute-forced challenges: {_bruteForced}/{_questions.Count}", TermColor.Warn);

        Say("Type 'next' to move on.", TermColor.Warn);
        Host.SetNext("type 'next' to move on.");
    }

    private void PenalizeWrong()
    {
        Host.Record(GameEvent.WrongAnswer);
        AddTrace(TraceMeter.WrongAnswerCost);
    }

    // ----- Helpers for the nodes -----
    protected void Say(string text = "", TermColor color = TermColor.Normal) => Host.Print(text, color);

    /// <summary>Raises the trace meter and tells the player why.</summary>
    protected void AddTrace(int amount, string reason = "")
    {
        Say($"[TRACE +{amount}%]" + (reason.Length > 0 ? " " + reason : ""), TermColor.Dim);
        Host.Trace.Add(amount);
    }

    /// <summary>Lets the player use hints again (call it when a multi-step challenge moves on).</summary>
    protected void ResetHints() => _hintsUsed = 0;

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