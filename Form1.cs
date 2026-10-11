using BreachBigO.Core;
using BreachBigO.Nodes;

namespace BreachBigO;

public partial class Form1 : Form, INodeHost
{
    private const int TotalNodes = 5;

    private static readonly Color Green = Color.FromArgb(0, 255, 120);
    private static readonly Color Amber = Color.FromArgb(255, 193, 7);
    private static readonly Color Red = Color.FromArgb(255, 80, 80);
    private static readonly Color White = Color.FromArgb(230, 230, 230);
    private static readonly Color Dim = Color.FromArgb(90, 160, 120);
    private static readonly Color Cyan = Color.FromArgb(90, 200, 255);

    private readonly StepCounter _steps = new();
    private readonly TraceMeter _trace = new();

    // One entry per node. Nodes 3-5 get added here in later rounds.
    private readonly List<Func<INodeHost, Node>> _nodeFactories = new()
    {
        host => new Node1_Constant(host),
        host => new Node2_Logarithmic(host),
    };

    private Node? _node;        // the node being played
    private int _nodeIndex;     // its position in _nodeFactories
    private int? _best;         // "best" value shown in the side panel

    public Form1()
    {
        InitializeComponent();
        _steps.Changed += UpdatePanel;
        _trace.Changed += UpdatePanel;
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        PrintLine("BREACH v0.2  //  Big-O Terminal", Green);
        PrintLine("Break into 5 systems. Each one runs on a different Big-O class.", White);
        PrintLine("Type answers at the prompt. Type 'help' any time.", White);
        StartNode(0);
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

    private void HandleCommand(string raw)
    {
        string cmd = raw.Trim();
        if (cmd.Length == 0 || _node == null) return;

        PrintLine($"{_node.Prompt} {cmd}", White);   // echo what the player typed

        switch (cmd.ToLowerInvariant())
        {
            case "help":
                ShowHelp();
                return;

            case "hint":
                _node.ShowHint();
                break;

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
                break;
        }

        if (_trace.IsFull) Traced();
    }

    private void ShowHelp()
    {
        PrintLine("Commands:", Green);
        PrintLine("  hint     get a clue (+5% trace)", Green);
        PrintLine("  next     go to the next node (after clearing this one)", Green);
        PrintLine("  restart  start this node over", Green);
        PrintLine("  clear    wipe the terminal", Green);
        PrintLine("Anything else you type is treated as your answer.", Green);
        _node?.Repeat();                          // re-show the current challenge
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
            PrintLine("That's every node built so far. The next one arrives next round.", Amber);
            return;
        }

        StartNode(_nodeIndex + 1);
    }

    // ---------- Node flow ----------

    private void StartNode(int index)
    {
        _nodeIndex = index;
        _steps.Reset();
        _trace.Reset();
        _best = null;

        _node = _nodeFactories[index](this);

        lblNode.Text = $"NODE {_node.Number}/{TotalNodes} — {_node.Title}";
        lblBigO.Text = $"[ {_node.BigO} ]";
        // keep the Big-O tag hugging the right edge, whatever its width
        lblBigO.Left = pnlHeader.ClientSize.Width - lblBigO.GetPreferredSize(Size.Empty).Width - 16;
        lblPrompt.Text = _node.Prompt;

        PrintLine("");
        _node.Start();
        UpdatePanel();
    }

    // Fail state: trace is full -> node restarts with fresh values.
    private void Traced()
    {
        PrintLine("");
        PrintLine("!! TRACE COMPLETE - you've been detected !!", Red);
        PrintLine("Locked out. Restarting the node...", Amber);
        StartNode(_nodeIndex);
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

    // ---------- UI helpers ----------

    private void UpdatePanel()
    {
        lblSteps.Text = _steps.Count.ToString();
        prgTrace.Value = _trace.Level;
        lblTracePct.Text = _trace.Level + "%";
        lblCompare.Text = $"{_steps.Count} / {(_best?.ToString() ?? "--")}";
    }

    // Updates the always-visible "what do I do now" bar.
    private void SetNext(string text) => lblNext.Text = "NEXT: " + text;

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