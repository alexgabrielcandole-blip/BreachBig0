using BreachBigO.Core;
using BreachBigO.Nodes;

namespace BreachBigO;

public partial class Form1 : Form, INodeHost
{
    private const int TotalNodes = 5;
    private const string GameVersion = "v1.0";

    private static readonly Color Green = Color.FromArgb(0, 255, 120);
    private static readonly Color Amber = Color.FromArgb(255, 193, 7);
    private static readonly Color Red = Color.FromArgb(255, 80, 80);
    private static readonly Color White = Color.FromArgb(230, 230, 230);
    private static readonly Color Dim = Color.FromArgb(90, 160, 120);
    private static readonly Color Cyan = Color.FromArgb(90, 200, 255);

    // Block letters for the title banner ('#' is drawn as a solid block).
    private static readonly string[][] BannerLetters =
    {
        new[] { "#### ", "#   #", "#### ", "#   #", "#### " },   // B
        new[] { "#### ", "#   #", "#### ", "#  # ", "#   #" },   // R
        new[] { "#####", "#    ", "#### ", "#    ", "#####" },   // E
        new[] { " ### ", "#   #", "#####", "#   #", "#   #" },   // A
        new[] { " ####", "#    ", "#    ", "#    ", " ####" },   // C
        new[] { "#   #", "#   #", "#####", "#   #", "#   #" },   // H
    };

    private enum GameScreen { Title, Playing, Finished }

    private readonly StepCounter _steps = new();
    private readonly TraceMeter _trace = new();
    private readonly GameStats _stats = new();

    // One entry per node, in play order.
    private readonly List<Func<INodeHost, Node>> _nodeFactories = new()
    {
        host => new Node1_Constant(host),
        host => new Node2_Logarithmic(host),
        host => new Node3_Linear(host),
        host => new Node4_Quadratic(host),
        host => new Node5_Exponential(host),
    };

    private GameScreen _screen = GameScreen.Title;
    private Node? _node;                // the node being played
    private int _nodeIndex;             // its position in _nodeFactories
    private int? _best;                 // "best" value shown in the side panel
    private string _nextText = "";      // text of the NEXT bar (so we can restore it)
    private bool _awaitingBrute;        // true while we wait for the player to type 'yes'
    private bool _clearHandled;         // true once the current node's clear was recorded

    public Form1()
    {
        InitializeComponent();
        _steps.Changed += UpdatePanel;
        _trace.Changed += UpdatePanel;
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        ShowTitle();
        txtInput.Focus();
    }

    // ---------- Input ----------

    // Enter key in the input box = submit.
    private void txtInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true;          // stops the Windows "ding"
        string input = txtInput.Text;
        txtInput.Clear();
        HandleCommand(input);
    }

    // The HINT button is the same as typing "hint".
    private void btnHint_Click(object? sender, EventArgs e)
    {
        HandleCommand("hint");
        txtInput.Focus();
    }

    // The BRUTE FORCE button is the same as typing "bruteforce".
    private void btnBrute_Click(object? sender, EventArgs e)
    {
        HandleCommand("bruteforce");
        txtInput.Focus();
    }

    private void HandleCommand(string raw)
    {
        string cmd = raw.Trim();
        if (cmd.Length == 0) return;

        PrintLine($"{lblPrompt.Text} {cmd}", White);   // echo what the player typed

        if (_screen == GameScreen.Title) { HandleTitleCommand(cmd); return; }
        if (_screen == GameScreen.Finished) { HandleFinishedCommand(cmd); return; }
        if (_node == null) return;

        // Waiting for the brute force confirmation?
        if (_awaitingBrute)
        {
            _awaitingBrute = false;
            ShowNextBar();
            string reply = cmd.ToLowerInvariant();

            if (reply == "yes" || reply == "y")
            {
                _node.BruteForce();
                AfterNodeAction();
                return;
            }

            PrintLine("Brute force cancelled.", Amber);
            if (reply == "no" || reply == "n") return;
            // anything else: carry on and treat it as a normal command/answer
        }

        switch (cmd.ToLowerInvariant())
        {
            case "help":
                ShowHelp();
                return;

            case "score":
                PrintScoreboard();
                return;

            case "hint":
                _node.ShowHint();
                AfterNodeAction();
                return;

            case "bruteforce":
            case "brute":
                AskBruteForce();
                return;

            case "next":
                GoNext();
                return;

            case "restart":
                PrintLine("Restarting node...", Amber);
                StartNode(_nodeIndex);
                return;

            case "clear":
                rtbTerminal.Clear();
                return;

            default:
                _node.HandleInput(cmd);          // anything else is an answer
                AfterNodeAction();
                return;
        }
    }

    private void HandleTitleCommand(string cmd)
    {
        switch (cmd.ToLowerInvariant())
        {
            case "start":
                StartGame();
                break;

            case "help":
                PrintLine("Commands on this screen:", Green);
                PrintLine("  start   begin the game", Green);
                PrintLine("  quit    close the game", Green);
                break;

            case "quit":
            case "exit":
                Close();
                break;

            case "hint":
            case "bruteforce":
            case "brute":
                PrintLine("Those only work inside a system. Type 'start' first.", Amber);
                break;

            default:
                PrintLine("Type 'start' to begin.", Amber);
                break;
        }
    }

    private void HandleFinishedCommand(string cmd)
    {
        switch (cmd.ToLowerInvariant())
        {
            case "play":
                ShowTitle();
                break;

            case "score":
                PrintScoreboard();
                break;

            case "help":
                PrintLine("Commands on this screen:", Green);
                PrintLine("  play    back to the title screen to play again", Green);
                PrintLine("  score   show the scoreboard again", Green);
                PrintLine("  quit    close the game", Green);
                break;

            case "quit":
            case "exit":
                Close();
                break;

            default:
                PrintLine("Type 'play' to play again, or 'quit' to exit.", Amber);
                break;
        }
    }

    private void ShowHelp()
    {
        PrintLine("Commands:", Green);
        PrintLine("  hint        get a clue (+5% trace)", Green);
        PrintLine("  bruteforce  reveal the answer + why (+25% trace, asks first)", Green);
        PrintLine("  next        go to the next node (after clearing this one)", Green);
        PrintLine("  score       show your stats so far", Green);
        PrintLine("  restart     start this node over", Green);
        PrintLine("  clear       wipe the terminal", Green);
        PrintLine("Anything else you type is treated as your answer.", Green);
        _node?.Repeat();                          // re-show the current challenge
    }

    // Brute force step 1: safety checks, then ask the player to confirm.
    private void AskBruteForce()
    {
        if (_node == null) return;

        if (_node.IsComplete)
        {
            PrintLine("Nothing to brute-force: this node is cleared. Type 'next'.", Amber);
            return;
        }

        if (!_node.CanBruteForce)
        {
            PrintLine("Brute force isn't available for this challenge.", Amber);
            return;
        }

        if (_trace.Level + Node.BruteForceCost >= TraceMeter.Max)
        {
            PrintLine($"Too risky: brute force costs +{Node.BruteForceCost}% and you're at {_trace.Level}%.", Red);
            PrintLine("It would get you traced. Solve this one yourself, or 'restart' the node.", Amber);
            return;
        }

        PrintLine("BRUTE FORCE reveals the answer and explains why, but it costs", Amber);
        PrintLine($"+{Node.BruteForceCost}% trace ({_trace.Level}% -> {_trace.Level + Node.BruteForceCost}%).", Amber);
        PrintLine("Type 'yes' to confirm, or anything else to cancel.", Amber);
        lblNext.Text = "NEXT: type 'yes' to confirm brute force (anything else cancels).";
        _awaitingBrute = true;
    }

    private void GoNext()
    {
        if (_node == null) return;

        if (!_node.IsComplete)
        {
            PrintLine("Finish this node first. Type 'hint' if you're stuck.", Amber);
            return;
        }

        if (_nodeIndex + 1 >= _nodeFactories.Count)
        {
            ShowResults();
            return;
        }

        StartNode(_nodeIndex + 1);
    }

    // ---------- Game flow ----------

    // After anything that can change the node: trace-out first, then check for a clear.
    private void AfterNodeAction()
    {
        if (_trace.IsFull) Traced();
        else CheckNodeCleared();
    }

    // Records the clear (once) so the scoreboard knows the final trace for the node.
    private void CheckNodeCleared()
    {
        if (_node == null || !_node.IsComplete || _clearHandled) return;
        _clearHandled = true;

        NodeStats stats = _stats.For(_node.Number, _node.Title);
        stats.Cleared = true;
        stats.FinalTrace = _trace.Level;

        if (_nodeIndex + 1 >= _nodeFactories.Count)
        {
            PrintLine("");
            PrintLine("That was the last system! Type 'next' to see your results.", Amber);
            SetNext("type 'next' to see your results.");
        }
    }

    private void ShowTitle()
    {
        _screen = GameScreen.Title;
        _node = null;
        _awaitingBrute = false;
        _steps.Reset();
        _trace.Reset();
        _best = null;
        rtbTerminal.Clear();

        lblNode.Text = "BREACH — BIG-O TERMINAL";
        lblBigO.Text = "[ TITLE ]";
        PositionBigOTag();
        lblPrompt.Text = "root@breach>";
        SetNext("type 'start' to begin.");
        UpdatePanel();

        PrintBanner();
        PrintLine("");
        PrintLine($"  THE  BIG-O  TERMINAL  GAME   {GameVersion}", Cyan);
        PrintLine("");
        PrintLine("You're a hacker. Five systems stand between you and the vault, and each", White);
        PrintLine("one is guarded by a different Big-O class:", White);
        PrintLine("");
        PrintLine("  1  DIRECT ACCESS    O(1)       constant time", Green);
        PrintLine("  2  PIN LOCK         O(log n)   logarithmic time", Green);
        PrintLine("  3  LOG SCAN         O(n)       linear time", Green);
        PrintLine("  4  RECORD MATCH     O(n\u00B2)      quadratic time", Green);
        PrintLine("  5  VAULT CRACKER    O(2\u207F)      exponential time", Green);
        PrintLine("");
        PrintLine("HOW IT WORKS", Amber);
        PrintLine("  - Work each answer out yourself and TYPE it at the prompt.", White);
        PrintLine("  - Wrong answers and hints raise your TRACE. At 100% you're caught.", White);
        PrintLine("  - The STEP COUNTER shows what your code costs.", White);
        PrintLine("  - Stuck? BRUTE FORCE reveals the answer and why, for +25% trace.", White);
        PrintLine("");
        PrintLine("Type 'start' to begin, or 'help' for the commands.", Amber);
    }

    private void PrintBanner()
    {
        for (int row = 0; row < 5; row++)
        {
            string line = string.Join("  ", BannerLetters.Select(letter => letter[row]));
            PrintLine("  " + line.Replace('#', '\u2588'), Green);
        }
    }

    private void StartGame()
    {
        _stats.Reset();
        _screen = GameScreen.Playing;
        rtbTerminal.Clear();
        PrintLine($"BREACH {GameVersion}  //  connection established", Green);
        PrintLine("Five systems. Five Big-O classes. Type 'help' any time.", White);
        StartNode(0);
    }

    private void StartNode(int index)
    {
        _nodeIndex = index;
        _awaitingBrute = false;
        _clearHandled = false;
        _steps.Reset();
        _trace.Reset();
        _best = null;

        _node = _nodeFactories[index](this);

        lblNode.Text = $"NODE {_node.Number}/{TotalNodes} — {_node.Title}";
        lblBigO.Text = $"[ {_node.BigO} ]";
        PositionBigOTag();
        lblPrompt.Text = _node.Prompt;

        PrintLine("");
        _node.Start();
        UpdatePanel();
    }

    // Fail state: trace is full -> node restarts with fresh values.
    private void Traced()
    {
        if (_node != null) _stats.Record(_node.Number, _node.Title, GameEvent.Traced);

        PrintLine("");
        PrintLine("!! TRACE COMPLETE - you've been detected !!", Red);
        PrintLine("Locked out. Restarting the node...", Amber);
        StartNode(_nodeIndex);
    }

    private void ShowResults()
    {
        _screen = GameScreen.Finished;
        _node = null;
        _awaitingBrute = false;
        _steps.Reset();
        _trace.Reset();
        _best = null;
        rtbTerminal.Clear();

        lblNode.Text = "ACCESS GRANTED — VAULT BREACHED";
        lblBigO.Text = "[ DONE ]";
        PositionBigOTag();
        lblPrompt.Text = "root@vault>";
        SetNext("type 'play' to play again, or 'quit' to exit.");
        UpdatePanel();

        string message = "ACCESS GRANTED - ALL 5 SYSTEMS BREACHED";
        string bar = "+" + new string('-', message.Length + 6) + "+";
        PrintLine(bar, Cyan);
        PrintLine("|   " + message + "   |", Cyan);
        PrintLine(bar, Cyan);
        PrintLine("");

        PrintLine("WHAT YOU CRACKED", Amber);
        PrintLine("  O(1)      constant     the same work however big the data", White);
        PrintLine("  O(log n)  logarithmic  halve the data with every step", White);
        PrintLine("  O(n)      linear       look at every item once", White);
        PrintLine("  O(n\u00B2)     quadratic    a loop inside a loop", White);
        PrintLine("  O(2\u207F)     exponential  every extra item doubles the work", White);
        PrintLine("");

        PrintScoreboard();
        PrintLine("");

        int score = _stats.Score;
        var (rankName, rankText) = _stats.Rank;
        PrintLine($"SCORE: {score} / {GameStats.StartScore}", Cyan);
        PrintLine($"RANK:  {rankName} - {rankText}", Cyan);
        PrintLine($"Score = {GameStats.StartScore} - {GameStats.BrutePenalty} per brute force - {GameStats.HintPenalty} per hint", Dim);
        PrintLine($"        - {GameStats.MistakePenalty} per mistake - {GameStats.TracedPenalty} per time traced", Dim);
        PrintLine("");
        PrintLine("Type 'play' to play again, or 'quit' to exit.", Amber);
    }

    private void PrintScoreboard()
    {
        PrintLine("SCOREBOARD", Cyan);
        PrintLine($"{"NODE",4}  {"SYSTEM",-15}{"TRACE",6}{"HINTS",7}{"BRUTE",7}{"MISTAKES",10}{"TRACED",8}", Dim);

        foreach (NodeStats s in _stats.Nodes)
        {
            string trace = s.Cleared ? s.FinalTrace + "%" : "-";
            PrintLine($"{s.Number,4}  {s.Title,-15}{trace,6}{s.Hints,7}{s.BruteForces,7}{s.Mistakes,10}{s.TimesTraced,8}", White);
        }

        PrintLine(new string('-', 59), Dim);
        PrintLine($"{"ALL",4}  {"",-15}{"",6}{_stats.TotalHints,7}{_stats.TotalBruteForces,7}{_stats.TotalMistakes,10}{_stats.TotalTraced,8}", White);
    }

    // ---------- INodeHost (what the nodes can ask this window to do) ----------

    StepCounter INodeHost.Steps => _steps;
    TraceMeter INodeHost.Trace => _trace;
    void INodeHost.Print(string text, TermColor color) => PrintLine(text, ToColor(color));
    void INodeHost.SetNext(string text) => SetNext(text);
    void INodeHost.SetBest(int? best)
    {
        _best = best;
        UpdatePanel();
    }
    void INodeHost.Record(GameEvent e)
    {
        if (_node != null) _stats.Record(_node.Number, _node.Title, e);
    }

    // ---------- UI helpers ----------

    private void UpdatePanel()
    {
        lblSteps.Text = _steps.Count.ToString();
        prgTrace.Value = _trace.Level;
        lblTracePct.Text = _trace.Level + "%";
        lblCompare.Text = $"{_steps.Count} / {(_best?.ToString() ?? "--")}";
    }

    // Keeps the Big-O tag hugging the right edge, whatever its width.
    private void PositionBigOTag() =>
        lblBigO.Left = pnlHeader.ClientSize.Width - lblBigO.GetPreferredSize(Size.Empty).Width - 16;

    // Updates the always-visible "what do I do now" bar.
    private void SetNext(string text)
    {
        _nextText = text;
        ShowNextBar();
    }

    private void ShowNextBar() => lblNext.Text = "NEXT: " + _nextText;

    private static Color ToColor(TermColor c) => c switch
    {
        TermColor.Player => White,
        TermColor.Warn => Amber,
        TermColor.Error => Red,
        TermColor.Dim => Dim,
        TermColor.Success => Cyan,
        _ => Green,
    };

    // Appends one colored line to the terminal and scrolls down.
    private void PrintLine(string text, Color? color = null)
    {
        rtbTerminal.SelectionStart = rtbTerminal.TextLength;
        rtbTerminal.SelectionLength = 0;
        rtbTerminal.SelectionColor = color ?? Green;
        rtbTerminal.AppendText(text + Environment.NewLine);
        rtbTerminal.SelectionColor = rtbTerminal.ForeColor;
        rtbTerminal.ScrollToCaret();
    }
}