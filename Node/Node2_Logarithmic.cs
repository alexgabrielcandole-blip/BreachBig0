using BreachBigO.Core;

namespace BreachBigO.Nodes;

/// <summary>
/// NODE 2 - O(log n) logarithmic time.
/// Covers the PDF's 3 examples: binary search, keep dividing by 2, and the 1-100
/// guessing game (now with a real hidden target, so it actually searches).
/// </summary>
public class Node2_Logarithmic : Node
{
    public override int Number => 2;
    public override string Title => "PIN LOCK";
    public override string BigO => "O(log n)";

    private const int ListSize = 15;
    private const int BestSearch = 4;       // most checks needed for 15 sorted entries
    private const int GuessMax = 100;
    private const int BestGuess = 7;        // most guesses needed for 1..100 by halving

    private readonly Random _rng = new();

    // --- Challenge 1 (Sorted Gate) ---
    private readonly int[] _pins = new int[ListSize];   // sorted, hidden from the player
    private readonly int _targetValue;
    private int _left = 0;
    private int _right = ListSize - 1;
    private int _checks;

    // --- Challenge 2 (Halving Hall) ---
    private static readonly int[] HalvingSizes = { 16, 64, 1024 };
    private int _round;

    // --- Challenge 3 (Number Sphinx) ---
    private readonly int _secret;
    private int _low = 1;
    private int _high = GuessMax;
    private int _guesses;

    public Node2_Logarithmic(INodeHost host) : base(host)
    {
        // A fresh sorted list every run: each entry is bigger than the one before.
        int value = _rng.Next(5, 15);
        for (int i = 0; i < ListSize; i++)
        {
            _pins[i] = value;
            value += _rng.Next(3, 10);
        }
        _targetValue = _pins[_rng.Next(0, ListSize)];
        _secret = _rng.Next(1, GuessMax + 1);
    }

    protected override void Intro()
    {
        Say("=== NODE 2/5: PIN LOCK   [ O(log n) - logarithmic time ] ===", TermColor.Success);
        Say();
        Say("The lock hides a PIN in a SORTED list. Checking every entry is slow.");
        Say("In O(log n) you throw away HALF of the data with every check.");
        Say();
        Say("WORKED EXAMPLE (looking for 50)", TermColor.Warn);
        Say("  int[] pins = { 10, 20, 30, 40, 50, 60, 70 };");
        Say("  left 0, right 6 -> middle = (0 + 6) / 2 = 3 -> pins[3] = 40, too low");
        Say("  left 4, right 6 -> middle = (4 + 6) / 2 = 5 -> pins[5] = 60, too high");
        Say("  left 4, right 4 -> middle = (4 + 4) / 2 = 4 -> pins[4] = 50, FOUND");
        Say("3 checks instead of 5. With 1,000,000 entries it would still take");
        Say("only about 20. C# drops decimals in whole-number division: 7 / 2 = 3.");
        Say("Binary search only works when the data is SORTED.");
    }

    protected override IEnumerable<Question> BuildQuestions()
    {
        // 1) Binary search (PDF example 1): the player works out each middle by hand.
        yield return new Question
        {
            Ask = AskGate,
            Judge = JudgeGate,
            HintProvider = GateHint,
            Solved = GateSolved
        };

        // 2) Keep dividing by 2 (PDF example 2): predict the loop count for 3 sizes.
        yield return new Question
        {
            Ask = AskHalving,
            Judge = JudgeHalving,
            HintProvider = HalvingHint,
            Solved = HalvingSolved
        };

        // 3) Guessing 1-100 (PDF example 3, fixed: it now has a real target).
        yield return new Question
        {
            Ask = AskSphinx,
            Judge = JudgeSphinx,
            HintProvider = SphinxHint,
            Solved = SphinxSolved
        };

        // 4) Name the Big-O.
        yield return new Question
        {
            Ask = AskBigO,
            Validate = input =>
            {
                string s = Norm(input).ToUpperInvariant();
                if (s == "O(LOGN)" || s == "O(LOG2N)") return null;
                if (s == "O(N)") return "O(n) means checking one by one. Halving every time is much faster.";
                if (s == "O(1)") return "O(1) would be one step always. Here the steps grow, but very slowly.";
                if (s.StartsWith("O(")) return "Not quite. Think: what do you do to the data with every check?";
                return "Write it like O(...), for example O(n) or O(1).";
            },
            Solved = () => Say("Correct: O(log n) = logarithmic time.", TermColor.Success),
            Hints = new[]
            {
                "Halving again and again is the opposite of doubling: it's a logarithm.",
                "A logarithm is written log. Put n inside it: O(log n)."
            }
        };
    }

    // ================= Challenge 1: Sorted Gate =================

    private void AskGate()
    {
        if (_checks == 0) Host.Steps.Reset();
        Say("Sorted Gate:");
        Say($"  The lock holds {ListSize} sorted PIN entries at positions 0 to {ListSize - 1}.");
        Say($"  Find the entry that equals {_targetValue}.");
        Say("  Type the position you want to check and the lock reveals what's there.");
        Say($"  Range: left = {_left}, right = {_right}   ({_right - _left + 1} entries still possible)");
        Host.SetBest(BestSearch);
        Host.SetNext("type the middle position of the range: (left + right) / 2.");
    }

    private Verdict JudgeGate(string input)
    {
        if (!TryGetIndex(input, out int pos))
        {
            Say("Type a position as a whole number, for example 7 or pins[7].", TermColor.Error);
            return Verdict.Wrong;
        }

        if (pos < _left || pos > _right)
        {
            Say($"Position {pos} is outside the range ({_left} to {_right}). That part is already thrown away.",
                TermColor.Error);
            return Verdict.Wrong;
        }

        int middle = (_left + _right) / 2;
        if (pos != middle)
        {
            Say("Valid position, but not the middle. Work out (left + right) / 2.", TermColor.Error);
            return Verdict.Wrong;
        }

        // A valid check costs 1 step.
        _checks++;
        Host.Steps.Add(1);
        int value = _pins[middle];

        if (value == _targetValue)
        {
            Say($"> pins[{middle}] = {value}   FOUND!", TermColor.Success);
            return Verdict.Solved;
        }

        if (value < _targetValue)
        {
            Say($"> pins[{middle}] = {value}   too LOW. Throw away the left half.");
            _left = middle + 1;
        }
        else
        {
            Say($"> pins[{middle}] = {value}   too HIGH. Throw away the right half.");
            _right = middle - 1;
        }

        Say($"  Range: left = {_left}, right = {_right}   ({_right - _left + 1} entries still possible)");
        ResetHints();
        Host.SetNext("type the middle position of the new range: (left + right) / 2.");
        return Verdict.Continue;
    }

    private string? GateHint(int i) => i switch
    {
        0 => "The middle position is (left + right) / 2. C# drops decimals: 7 / 2 = 3.",
        1 => $"Right now left = {_left} and right = {_right}: ({_left} + {_right}) / 2 = {(_left + _right) / 2}.",
        _ => null
    };

    private void GateSolved()
    {
        Say($"Found in {_checks} checks. A scan could have needed up to {ListSize}.");
        Say("Each check threw away half of what was left.");
    }

    // ================= Challenge 2: Halving Hall =================

    private void AskHalving()
    {
        int n = HalvingSizes[_round];
        if (_round == 0)
        {
            Host.Steps.Reset();
            Host.SetBest(null);     // the best here IS the answer, so don't show it yet
        }
        Say("Halving Hall:");
        Say($"  int n = {n};");
        Say("  while (n > 1)");
        Say("  {");
        Say("      n = n / 2;");
        Say("  }");
        Say($"How many times does the loop run when n = {n:N0}? (type a number)");
        Host.SetNext($"type how many times the loop runs for n = {n:N0}.");
    }

    private Verdict JudgeHalving(string input)
    {
        int n = HalvingSizes[_round];
        int answer = Halvings(n);

        if (!TryGetNumber(input, out int guess))
        {
            Say("Type just a number.", TermColor.Error);
            return Verdict.Wrong;
        }

        if (guess != answer)
        {
            if (guess == n)
                Say("That's n itself. I need how many halvings it takes to reach 1.", TermColor.Error);
            else if (guess == n / 2)
                Say("That's the value after ONE halving. Count the halvings until n reaches 1.", TermColor.Error);
            else
                Say("Not quite. Halve n again and again until it reaches 1, and count.", TermColor.Error);
            return Verdict.Wrong;
        }

        // Correct: run it and show the chain.
        Run(answer);
        Host.SetBest(answer);
        Say($"> {HalvingChain(n)}   [{answer} steps]", TermColor.Success);

        _round++;
        ResetHints();
        if (_round >= HalvingSizes.Length) return Verdict.Solved;

        Say();
        AskHalving();               // next size
        return Verdict.Continue;
    }

    private string? HalvingHint(int i)
    {
        int n = HalvingSizes[_round];
        return i switch
        {
            0 => "Halve n again and again until you reach 1. Count the halvings.",
            1 => $"{n} -> {n / 2} -> {n / 4} -> ... keep halving until you reach 1.",
            _ => null
        };
    }

    private void HalvingSolved()
    {
        int first = HalvingSizes[0];
        int last = HalvingSizes[^1];
        Say($"n grew {last / first}x ({first} to {last:N0}), but the steps only went from {Halvings(first)} to {Halvings(last)}.",
            TermColor.Success);
    }

    private static int Halvings(int n)
    {
        int count = 0;
        while (n > 1)
        {
            n /= 2;
            count++;
        }
        return count;
    }

    private static string HalvingChain(int n)
    {
        var parts = new List<string> { n.ToString("N0") };
        while (n > 1)
        {
            n /= 2;
            parts.Add(n.ToString("N0"));
        }
        return string.Join(" -> ", parts);
    }

    // ================= Challenge 3: Number Sphinx =================

    private void AskSphinx()
    {
        if (_guesses == 0) Host.Steps.Reset();
        Say("Number Sphinx:");
        Say($"  The Sphinx is thinking of a PIN between 1 and {GuessMax}.");
        Say("  Type a guess. It answers HIGHER or LOWER.");
        Say($"  Every guess beyond {BestGuess} raises your trace.");
        Say($"  Still possible: {_low} to {_high}   ({_high - _low + 1} numbers)");
        Host.SetBest(BestGuess);
        Host.SetNext($"type your guess ({_low} to {_high} are still possible).");
    }

    private Verdict JudgeSphinx(string input)
    {
        if (!TryGetNumber(input, out int g) || g < 1 || g > GuessMax)
        {
            Say($"Type a whole number from 1 to {GuessMax}.", TermColor.Error);
            return Verdict.Wrong;
        }

        // Every guess is a step, even a wasted one.
        _guesses++;
        Host.Steps.Add(1);
        bool wasted = g < _low || g > _high;

        if (_guesses > BestGuess)
            AddTrace(TraceMeter.WrongAnswerCost, $"guess #{_guesses} is beyond the best of {BestGuess}.");

        if (g == _secret)
        {
            Say($"> {g}  ->  CORRECT!", TermColor.Success);
            return Verdict.Solved;
        }

        string note = wasted ? "   (wasted: you already knew that)" : "";
        if (g < _secret)
        {
            Say($"> {g}  ->  HIGHER{note}");
            _low = Math.Max(_low, g + 1);
        }
        else
        {
            Say($"> {g}  ->  LOWER{note}");
            _high = Math.Min(_high, g - 1);
        }

        Say($"  Still possible: {_low} to {_high}   ({_high - _low + 1} numbers)");
        ResetHints();
        Host.SetNext($"type your next guess ({_low} to {_high} are still possible).");
        return Verdict.Continue;
    }

    private string? SphinxHint(int i) => i switch
    {
        0 => "Don't guess 1, 2, 3... Guess the MIDDLE of the numbers still possible.",
        1 => $"Still possible: {_low} to {_high}. The middle is ({_low} + {_high}) / 2 = {(_low + _high) / 2}.",
        _ => null
    };

    private void SphinxSolved()
    {
        Say($"PIN {_secret} found in {_guesses} guesses (best possible: {BestGuess}).");
        Say("Halving the range each time would have gone:");
        Say("  " + HalvingPath(_secret));
        Say("That's the PDF's guessing loop, now with a real target:");
        Say("  if (middle == target) break;");
        Say("  else if (middle < target) low = middle + 1;");
        Say("  else high = middle - 1;");
    }

    private static string HalvingPath(int target)
    {
        int low = 1;
        int high = GuessMax;
        var path = new List<string>();

        while (low <= high)
        {
            int middle = (low + high) / 2;
            path.Add(middle.ToString());
            if (middle == target) break;
            if (middle < target) low = middle + 1;
            else high = middle - 1;
        }

        return string.Join(" -> ", path) + $"   ({path.Count} guesses)";
    }

    // ================= Challenge 4: Big-O =================

    private void AskBigO()
    {
        Say("Last check. Each check throws away HALF of the data.");
        Say("Type the Big-O of binary search.");
        Host.SetNext("type the Big-O of binary search (written like O(n) or O(1)).");
    }

    // ================= Summary =================

    protected override void Summary()
    {
        Say();
        Say("=== NODE 2 CLEARED ===", TermColor.Success);
        Say("Checks needed to find one entry as the data grows:");
        Say("n".PadLeft(14) + "binary O(log n)".PadLeft(18) + "scan O(n)".PadLeft(16));
        foreach (int n in new[] { 10, 100, 1000, 1000000, 1000000000 })
        {
            int binary = (int)Math.Floor(Math.Log2(n)) + 1;
            Say(n.ToString("N0").PadLeft(14) + binary.ToString().PadLeft(18) + n.ToString("N0").PadLeft(16));
        }
        Say("A billion entries need only 30 checks. Halving beats scanning.");
        Say($"Final trace: {Host.Trace.Level}%");
    }

    // ================= Input helpers =================

    // Accepts "7" or "pins[7]".
    private static bool TryGetIndex(string input, out int index)
    {
        string s = Norm(input);
        if (s.StartsWith("pins[", StringComparison.Ordinal) && s.EndsWith(']'))
            s = s[5..^1];
        return int.TryParse(s, out index);
    }

    private static bool TryGetNumber(string input, out int n) =>
        int.TryParse(Norm(input).Replace(",", ""), out n);
}