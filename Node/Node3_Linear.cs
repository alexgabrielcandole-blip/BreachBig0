using BreachBigO.Core;

namespace BreachBigO.Nodes;

/// <summary>
/// NODE 3 - O(n) linear time.
/// Covers the PDF's 3 examples: print every number (for loop), find a name (foreach),
/// calculate the total (foreach). Plus growth and worst-case questions.
/// </summary>
public class Node3_Linear : Node
{
    public override int Number => 3;
    public override string Title => "LOG SCAN";
    public override string BigO => "O(n)";

    private static readonly string[] NamePool =
        { "Ana", "Ben", "Carlo", "David", "Elena", "Felix", "Gina", "Hugo", "Iris", "Jon", "Kara", "Leo" };
    private static readonly int[] PrintSizes = { 5, 50, 5000 };
    private const int WorstCaseSize = 1000;
    private const int ListLength = 6;

    private readonly Random _rng = new();

    // --- Challenge 2 (growing print loop) ---
    private int _printRound;

    // --- Challenge 3 (name search) ---
    private readonly string[] _names;
    private readonly int _nameIndex;                // position of the name to find (1-5)
    private string Target => _names[_nameIndex];

    // --- Challenge 5 (total) ---
    private readonly int[] _nums;
    private readonly int _sum;
    private int _totalPhase;    // 0 = work out the total, 1 = predict additions for 1,000 entries

    public Node3_Linear(INodeHost host) : base(host)
    {
        _names = NamePool.OrderBy(_ => _rng.Next()).Take(ListLength).ToArray();
        _nameIndex = _rng.Next(1, ListLength);
        _nums = Enumerable.Range(0, ListLength).Select(_ => _rng.Next(3, 25)).ToArray();
        _sum = _nums.Sum();
    }

    protected override void Intro()
    {
        Say("=== NODE 3/5: LOG SCAN   [ O(n) - linear time ] ===", TermColor.Success);
        Say();
        Say("The server keeps its logs in a list. To find or total something you");
        Say("must look at the entries ONE BY ONE. In O(n), the work grows in step");
        Say("with the data: double the entries, double the steps.");
        Say();
        Say("WORKED EXAMPLE", TermColor.Warn);
        Say("  int[] hits = { 4, 9, 2 };");
        Say("  int total = 0;");
        Say("  foreach (int h in hits) { total = total + h; }");
        Say("3 entries = 3 steps (one addition each). 30 entries would be 30 steps.");
        Say();
        Say("Heads-up: here scanning one by one IS the real algorithm, so BRUTE FORCE");
        Say("can reveal answers but has no faster route to compare against.");
    }

    protected override IEnumerable<Question> BuildQuestions()
    {
        // 1) Print every number (PDF example 1): fill in the loop bound.
        yield return new Question
        {
            Ask = AskLoop,
            Validate = ValidateLoop,
            Solved = LoopSolved,
            Reveal = RevealLoop,
            Hints = new[]
            {
                "Arrays know their own size through a property.",
                "The property starts with a capital L and means 'number of elements'."
            }
        };

        // 2) How the print loop grows with the data.
        yield return new Question
        {
            Ask = AskPrint,
            Judge = JudgePrint,
            HintProvider = PrintHint,
            Solved = PrintSolved,
            Reveal = RevealPrint
        };

        // 3) Find a name (PDF example 2): count the checks.
        yield return new Question
        {
            Ask = AskName,
            Validate = ValidateName,
            Solved = NameSolved,
            Reveal = RevealName,
            Hints = new[]
            {
                "Names are checked in order from the first one, and the loop stops when it finds the target.",
                "Count each name from the start, up to and including the target."
            }
        };

        // 4) Worst case: why Big-O talks about the longest the search can take.
        yield return new Question
        {
            Ask = AskWorst,
            Validate = ValidateWorst,
            Solved = WorstSolved,
            Reveal = RevealWorst,
            Hints = new[]
            {
                "Worst case = the name is last, or isn't there at all. What does the loop do then?",
                "Think of the longest the search could possibly take."
            }
        };

        // 5) Calculate the total (PDF example 3): hand calculation, then growth.
        yield return new Question
        {
            Ask = AskTotal,
            Judge = JudgeTotal,
            HintProvider = TotalHint,
            Solved = TotalSolved,
            Reveal = RevealTotal
        };

        // 6) Name the Big-O.
        yield return new Question
        {
            Ask = AskBigO,
            Validate = ValidateBigO,
            Solved = () => Say("Correct: O(n) = linear time.", TermColor.Success),
            Reveal = RevealBigO,
            Hints = new[]
            {
                "If you double the entries, what happens to the work?",
                "The work grows in step with n. Put n inside O( )."
            }
        };
    }

    // ================= Challenge 1: Print Log =================

    private void AskLoop()
    {
        Host.Steps.Reset();
        Host.SetBest(null);
        Say("Print Log:");
        Say("  int[] numbers = { 10, 20, 30, 40, 50 };");
        Say("  for (int i = 0; i < numbers.___; i++)");
        Say("  {");
        Say("      Console.WriteLine(numbers[i]);");
        Say("  }");
        Say("Fill the blank so the loop prints EVERY number. Type what replaces ___ .");
        Host.SetNext("type what replaces ___ (the array's size).");
    }

    private string? ValidateLoop(string input)
    {
        string s = Norm(input);
        if (s == "Length" || s == "numbers.Length") return null;

        string lower = s.ToLowerInvariant();
        if (lower == "length" || lower == "numbers.length")
            return "Close, but C# is case-sensitive. It's spelled Length.";
        if (lower == "count" || lower == "numbers.count")
            return "Count belongs to Lists. An array's size property is called Length.";
        if (lower.Replace("numbers.", "").StartsWith("length-1"))
            return "That stops one early: i < 4 never reaches the last number.";
        if (int.TryParse(s, out _))
            return "A fixed number works today but breaks if the array changes. Use the array's own size property.";
        return "Not quite. Arrays have a property that says how many elements they hold.";
    }

    private void LoopSolved()
    {
        Run(5);
        Say("> 10  20  30  40  50   [5 steps]", TermColor.Success);
        Say("numbers.Length is 5, so i runs 0 to 4: one print per element = 5 steps.");
    }

    private void RevealLoop()
    {
        Say("ANSWER: Length", TermColor.Success);
        Say("numbers.Length is the number of elements. The loop runs while i < Length,");
        Say("so every element gets visited exactly once.");
        LoopSolved();
    }

    // ================= Challenge 2: Growing Log =================

    private void AskPrint()
    {
        int n = PrintSizes[_printRound];
        if (_printRound == 0)
        {
            Host.Steps.Reset();
            Host.SetBest(null);     // the best here IS the answer, so don't show it yet
        }
        Say("Growing Log:");
        Say("  for (int i = 0; i < numbers.Length; i++)");
        Say("  {");
        Say("      Console.WriteLine(numbers[i]);");
        Say("  }");
        Say($"Now numbers holds {n:N0} entries. How many times does Console.WriteLine run? (type a number)");
        Host.SetNext($"type how many prints happen for {n:N0} entries.");
    }

    private Verdict JudgePrint(string input)
    {
        int n = PrintSizes[_printRound];

        if (!TryGetNumber(input, out int guess))
        {
            Say("Type just a number.", TermColor.Error);
            return Verdict.Wrong;
        }

        if (guess != n)
        {
            if (guess == 1)
                Say("That's O(1) thinking: a jump to one entry. This loop visits EVERY entry.", TermColor.Error);
            else if (guess == n - 1 || guess == n + 1)
                Say("Off by one. i runs from 0 up to n - 1, which is exactly n passes.", TermColor.Error);
            else
                Say("Not quite. The loop prints once per entry.", TermColor.Error);
            return Verdict.Wrong;
        }

        Run(n);
        Host.SetBest(n);
        Say($"> {n:N0} entries  ->  {n:N0} steps", TermColor.Success);

        _printRound++;
        ResetHints();
        if (_printRound >= PrintSizes.Length) return Verdict.Solved;

        Say();
        AskPrint();                 // next size
        return Verdict.Continue;
    }

    private string? PrintHint(int i) => i switch
    {
        0 => "Does the loop visit every entry, or jump straight to one?",
        1 => $"It prints once for each entry. How many entries are there? ({PrintSizes[_printRound]:N0})",
        _ => null
    };

    private void PrintSolved()
    {
        int first = PrintSizes[0];
        int last = PrintSizes[^1];
        Say($"The data grew {last / first:N0}x ({first} to {last:N0}) and the steps grew {last / first:N0}x too. That's linear.",
            TermColor.Success);
    }

    // BRUTE FORCE: all answers and the pattern behind them.
    private void RevealPrint()
    {
        Say("ANSWERS:", TermColor.Success);
        foreach (int n in PrintSizes)
            Say($"  {n,5:N0} entries  ->  {n,5:N0} steps", TermColor.Success);
        Say("One print per entry, so steps = n. Doubling the data doubles the work.");

        int last = PrintSizes[^1];
        Run(last);
        Host.SetBest(last);
        PrintSolved();
    }

    // ================= Challenge 3: Name Search =================

    private void AskName()
    {
        Host.Steps.Reset();
        Host.SetBest(null);
        Say("Name Search:");
        Say("  string[] names = { " + string.Join(", ", _names.Select(n => $"\"{n}\"")) + " };");
        Say("  foreach (string name in names)");
        Say("  {");
        Say($"      if (name == \"{Target}\")");
        Say("      {");
        Say("          Console.WriteLine(\"Found!\");");
        Say("          break;");
        Say("      }");
        Say("  }");
        Say($"How many names does the loop check before it prints Found!? (count \"{Target}\" too)");
        Host.SetNext($"type how many names get checked, up to and including {Target}.");
    }

    private string? ValidateName(string input)
    {
        int answer = _nameIndex + 1;

        if (!TryGetNumber(input, out int n)) return "Type just a number.";
        if (n == answer) return null;
        if (n == _nameIndex)
            return $"That counts only the names BEFORE {Target}. {Target} itself gets checked too.";
        if (n == _names.Length)
            return "That's checking every name. The break stops the loop as soon as the name is found.";
        return $"Not quite. Count the names from the start, up to and including {Target}.";
    }

    private void NameSolved()
    {
        int answer = _nameIndex + 1;
        Run(answer);
        Say($"> checked: {string.Join(", ", _names.Take(answer))}  ->  Found!   [{answer} steps]", TermColor.Success);
        Say($"Lucky or unlucky: {Target} could have been first (1 step) or last ({_names.Length} steps).");
    }

    private void RevealName()
    {
        Say($"ANSWER: {_nameIndex + 1}", TermColor.Success);
        Say($"foreach checks the names in order and stops at \"{Target}\" (that's the break):");
        NameSolved();
    }

    // ================= Challenge 4: Worst Case =================

    private void AskWorst()
    {
        Host.Steps.Reset();
        Host.SetBest(null);
        Say("Worst Case:");
        Say($"  The same search, but the log now holds {WorstCaseSize:N0} names.");
        Say("  In the WORST case (the name is last, or isn't there at all),");
        Say("  how many names does the loop check? (type a number)");
        Host.SetNext("type the worst-case number of names checked.");
    }

    private string? ValidateWorst(string input)
    {
        if (!TryGetNumber(input, out int n)) return "Type just a number.";
        if (n == WorstCaseSize) return null;
        if (n == WorstCaseSize / 2) return "That's the average case. Big-O describes the worst case.";
        if (n == 1) return "That's the best case (the name is first). We want the worst.";
        if (n > WorstCaseSize) return $"The log only has {WorstCaseSize:N0} names. The loop can't check more than that.";
        return "Not quite. Think of the longest the search could possibly take.";
    }

    private void WorstSolved()
    {
        Run(WorstCaseSize);
        Host.SetBest(WorstCaseSize);
        Say($"> worst case on {WorstCaseSize:N0} names  ->  {WorstCaseSize:N0} steps", TermColor.Success);
        Say($"Best case 1, average about {WorstCaseSize / 2:N0}, worst {WorstCaseSize:N0}. Big-O uses the worst case.");
        Say("Without 'break', the loop would ALWAYS check every name.");
    }

    private void RevealWorst()
    {
        Say($"ANSWER: {WorstCaseSize:N0}", TermColor.Success);
        Say("Worst case: the name is last or missing, so EVERY name gets checked.");
        WorstSolved();
    }

    // ================= Challenge 5: Rune Sum =================

    private void AskTotal()
    {
        if (_totalPhase == 0)
        {
            Host.Steps.Reset();
            Host.SetBest(null);
            Say("Rune Sum:");
            Say($"  int[] numbers = {{ {string.Join(", ", _nums)} }};");
            Say("  int total = 0;");
            Say("  foreach (int number in numbers)");
            Say("  {");
            Say("      total = total + number;");
            Say("  }");
            Say("Work out total by hand and type it.");
            Host.SetNext("type the final value of total.");
        }
        else
        {
            Say($"Now numbers holds {WorstCaseSize:N0} entries.");
            Say("How many times does total = total + number run? (type a number)");
            Host.SetNext($"type how many additions happen for {WorstCaseSize:N0} entries.");
        }
    }

    private Verdict JudgeTotal(string input)
    {
        if (!TryGetNumber(input, out int n))
        {
            Say("Type just a number.", TermColor.Error);
            return Verdict.Wrong;
        }

        if (_totalPhase == 0)
        {
            if (n != _sum)
            {
                Say("Not quite. Add them one at a time, like the loop: total = total + number.", TermColor.Error);
                return Verdict.Wrong;
            }

            Run(_nums.Length);
            Say($"> total = {_sum}   [{_nums.Length} steps]", TermColor.Success);

            _totalPhase = 1;
            ResetHints();
            Say();
            AskTotal();             // now the 1,000-entry question
            return Verdict.Continue;
        }

        if (n != WorstCaseSize)
        {
            if (n == 1)
                Say("That's O(1) thinking. The loop adds once for EVERY entry.", TermColor.Error);
            else if (n == _nums.Length)
                Say($"That's for the small array. Now there are {WorstCaseSize:N0} entries.", TermColor.Error);
            else
                Say($"Not quite. The loop adds once per entry, and there are {WorstCaseSize:N0} entries.", TermColor.Error);
            return Verdict.Wrong;
        }

        Run(WorstCaseSize);
        Host.SetBest(WorstCaseSize);
        Say($"> {WorstCaseSize:N0} entries  ->  {WorstCaseSize:N0} additions", TermColor.Success);
        return Verdict.Solved;
    }

    private string? TotalHint(int i) => (_totalPhase, i) switch
    {
        (0, 0) => "Add the numbers left to right, like the loop does.",
        (0, 1) => "Keep a running total: first + second, then add the third, and so on.",
        (1, 0) => "The loop adds once for every entry.",
        (1, 1) => $"How many entries are there now? ({WorstCaseSize:N0})",
        _ => null
    };

    private void TotalSolved()
    {
        Say("Every entry is visited exactly once, so 2x the entries = 2x the work.", TermColor.Success);
    }

    private void RevealTotal()
    {
        Say($"ANSWERS: total = {_sum}, then {WorstCaseSize:N0} additions.", TermColor.Success);
        Say($"The loop adds each entry once: {string.Join(" + ", _nums)} = {_sum} ({_nums.Length} additions).");
        Say($"With {WorstCaseSize:N0} entries it adds {WorstCaseSize:N0} times: one addition per entry.");
        Run(WorstCaseSize);
        Host.SetBest(WorstCaseSize);
        TotalSolved();
    }

    // ================= Challenge 6: Big-O =================

    private void AskBigO()
    {
        Say("Last check. The work grows in step with the number of entries.");
        Say("Type the Big-O of scanning a list one entry at a time.");
        Host.SetNext("type the Big-O of scanning a list one by one (written like O(1) or O(log n)).");
    }

    private string? ValidateBigO(string input)
    {
        string s = Norm(input).ToUpperInvariant();
        if (s == "O(N)") return null;
        if (s == "O(1)") return "O(1) is a direct jump. A scan visits every entry.";
        if (s == "O(LOGN)" || s == "O(LOG2N)") return "O(log n) halves the data each step. A scan looks at every entry.";
        if (s.StartsWith("O(N") && s.Contains('2'))
            return "n squared needs a loop INSIDE a loop. Here there's only one loop.";
        if (s.StartsWith("O(")) return "Not quite. Double the data: what happens to the work?";
        return "Write it like O(...), for example O(1) or O(log n).";
    }

    private void RevealBigO()
    {
        Say("ANSWER: O(n)", TermColor.Success);
        Say("Linear time: the work grows in step with n. 10 entries = 10 steps,");
        Say("1,000 entries = 1,000 steps. Here the slow route IS the algorithm, so");
        Say("brute force showed the same steps as solving it: only your trace paid.");
    }

    // ================= Summary =================

    protected override void Summary()
    {
        Say();
        Say("=== NODE 3 CLEARED ===", TermColor.Success);
        Say("Steps needed as the data grows (the 3 classes so far):");
        Say("n".PadLeft(12) + "O(1)".PadLeft(8) + "O(log n)".PadLeft(12) + "O(n)".PadLeft(14));
        foreach (int n in new[] { 10, 100, 1000, 1000000 })
        {
            int log = (int)Math.Floor(Math.Log2(n)) + 1;
            Say(n.ToString("N0").PadLeft(12) + "1".PadLeft(8) + log.ToString().PadLeft(12) + n.ToString("N0").PadLeft(14));
        }
        Say("Double the data, double the work. A million entries = a million steps.");
        Say($"Final trace: {Host.Trace.Level}%");
    }

    // ================= Input helpers =================

    private static bool TryGetNumber(string input, out int n) =>
        int.TryParse(Norm(input).Replace(",", ""), out n);
}