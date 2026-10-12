using BreachBigO.Core;

namespace BreachBigO.Nodes;

/// <summary>
/// NODE 4 - O(n²) quadratic time.
/// Covers the PDF's examples, de-duplicated: the multiplication grid (ex. 1, 2 and 4),
/// compare students (ex. 3, fixed so nobody is compared with themselves), plus a
/// nested-vs-one-after-another challenge.
/// </summary>
public class Node4_Quadratic : Node
{
    public override int Number => 4;
    public override string Title => "RECORD MATCH";
    public override string BigO => "O(n²)";

    private static readonly string[] NamePool =
        { "Ana", "Ben", "Carlo", "David", "Elena", "Felix", "Gina", "Hugo" };
    private static readonly int[] GridSizes = { 5, 10, 100 };
    private static readonly int[] StudentCounts = { 4, 10 };
    private const int NestN = 100;

    private readonly Random _rng = new();

    // --- Challenge 1 (one cell of the grid) ---
    private readonly int _cellI;
    private readonly int _cellJ;

    // --- Challenge 2 (growing grid) ---
    private int _gridRound;

    // --- Challenge 3 (student pairs) ---
    private readonly string[] _students;
    private int _pairRound;

    // --- Challenge 4 (two loop layouts) ---
    private int _nestPhase;     // 0 = pick A or B, 1 = steps of A, 2 = steps of B

    public Node4_Quadratic(INodeHost host) : base(host)
    {
        _cellI = _rng.Next(2, 6);       // 2..5, inside the 1..5 grid
        _cellJ = _rng.Next(2, 6);
        _students = NamePool.OrderBy(_ => _rng.Next()).Take(4).ToArray();
    }

    protected override void Intro()
    {
        Say("=== NODE 4/5: RECORD MATCH   [ O(n²) - quadratic time ] ===", TermColor.Success);
        Say();
        Say("The vault must compare its records with EACH OTHER. For every record");
        Say("you look at, you loop over the records again: a loop INSIDE a loop.");
        Say("So n records cost about n x n steps: double the data, FOUR times the work.");
        Say();
        Say("WORKED EXAMPLE", TermColor.Warn);
        Say("  int[] ids = { 1, 2, 3 };");
        Say("  for (int i = 0; i < ids.Length; i++)");
        Say("      for (int j = 0; j < ids.Length; j++)");
        Say("          Console.WriteLine(ids[i] + \" \" + ids[j]);");
        Say("3 records x 3 = 9 lines (1 1, 1 2, 1 3, 2 1, ...). 30 records would be 900.");
    }

    protected override IEnumerable<Question> BuildQuestions()
    {
        // 1) Multiplication grid (PDF example 4): one cell by hand.
        yield return new Question
        {
            Ask = AskCell,
            Validate = ValidateCell,
            Solved = CellSolved,
            Reveal = RevealCell,
            Hints = new[]
            {
                "The line prints i x j = and then the result of multiplying i by j.",
                "Multiply the two numbers the question gives you."
            }
        };

        // 2) How the grid grows (PDF examples 1 and 2 merged).
        yield return new Question
        {
            Ask = AskGrid,
            Judge = JudgeGrid,
            HintProvider = GridHint,
            Solved = GridSolved,
            Reveal = RevealGrid
        };

        // 3) Compare students (PDF example 3, fixed): pairs only.
        yield return new Question
        {
            Ask = AskPairs,
            Judge = JudgePairs,
            HintProvider = PairsHint,
            Solved = PairsSolved,
            Reveal = RevealPairs
        };

        // 4) Nested loops multiply, loops one after another only add.
        yield return new Question
        {
            Ask = AskNest,
            Judge = JudgeNest,
            HintProvider = NestHint,
            Solved = NestSolved,
            Reveal = RevealNest
        };

        // 5) Name the Big-O.
        yield return new Question
        {
            Ask = AskBigO,
            Validate = ValidateBigO,
            Solved = () => Say("Correct: O(n²) = quadratic time.", TermColor.Success),
            Reveal = RevealBigO,
            Hints = new[]
            {
                "A loop inside a loop: n passes, each doing n steps.",
                "n x n is n squared. Write it as O( ) with n and the square inside."
            }
        };
    }

    // ================= Challenge 1: Multiplication Grid =================

    private void AskCell()
    {
        Host.Steps.Reset();
        Host.SetBest(null);
        Say("Multiplication Grid:");
        Say("  for (int i = 1; i <= 5; i++)");
        Say("  {");
        Say("      for (int j = 1; j <= 5; j++)");
        Say("      {");
        Say("          Console.WriteLine(i + \" x \" + j + \" = \" + (i * j));");
        Say("      }");
        Say("  }");
        Say($"What does the line print when i = {_cellI} and j = {_cellJ}? (type the result of i x j)");
        Host.SetNext($"type the result of {_cellI} x {_cellJ}.");
    }

    private string? ValidateCell(string input)
    {
        // Accept "12" or the whole line "3 x 4 = 12".
        string s = Norm(input);
        int eq = s.LastIndexOf('=');
        if (eq >= 0) s = s[(eq + 1)..];

        if (!int.TryParse(s, out int n)) return "Type the result of i x j (just the number).";
        if (n == _cellI * _cellJ) return null;
        if (n == _cellI + _cellJ) return "That's i + j. The line multiplies: i * j.";
        return "Not quite. Multiply i by j.";
    }

    private void CellSolved()
    {
        Run(1);
        Say($"> {_cellI} x {_cellJ} = {_cellI * _cellJ}   [1 step]", TermColor.Success);
        Say("One cell is 1 step. The real question is how many cells the grid has.");
    }

    private void RevealCell()
    {
        Say($"ANSWER: {_cellI * _cellJ}", TermColor.Success);
        Say($"The line prints {_cellI} x {_cellJ} = {_cellI * _cellJ}.");
        CellSolved();
    }

    // ================= Challenge 2: Growing Grid =================

    private void AskGrid()
    {
        int n = GridSizes[_gridRound];
        if (_gridRound == 0)
        {
            Host.Steps.Reset();
            Host.SetBest(null);     // the best here IS the answer, so don't show it yet
        }
        Say("Growing Grid:");
        Say("  for (int i = 1; i <= n; i++)");
        Say("      for (int j = 1; j <= n; j++)");
        Say("          Console.WriteLine(i * j);");
        Say($"With n = {n:N0}, how many times does Console.WriteLine run? (type a number)");
        Host.SetNext($"type how many prints happen when n = {n:N0}.");
    }

    private Verdict JudgeGrid(string input)
    {
        int n = GridSizes[_gridRound];
        int answer = n * n;

        if (!TryGetNumber(input, out int guess))
        {
            Say("Type just a number.", TermColor.Error);
            return Verdict.Wrong;
        }

        if (guess != answer)
        {
            if (guess == n)
                Say("That's one pass of the outer loop. The inner loop runs fully for EACH pass: n x n.",
                    TermColor.Error);
            else if (guess == 2 * n)
                Say("That's n + n (two loops one after the other). These loops are NESTED: n x n.",
                    TermColor.Error);
            else
                Say("Not quite. For each of the n values of i, the inner loop runs n times.", TermColor.Error);
            return Verdict.Wrong;
        }

        Run(answer);
        Host.SetBest(answer);
        Say($"> n = {n:N0}  ->  {answer:N0} steps", TermColor.Success);

        _gridRound++;
        ResetHints();
        if (_gridRound >= GridSizes.Length) return Verdict.Solved;

        Say();
        AskGrid();                  // next size
        return Verdict.Continue;
    }

    private string? GridHint(int i)
    {
        int n = GridSizes[_gridRound];
        return i switch
        {
            0 => "For every value of i, how many times does the inner loop run?",
            1 => $"The outer loop makes {n:N0} passes, and each pass runs the inner loop {n:N0} times.",
            _ => null
        };
    }

    private void GridSolved()
    {
        int first = GridSizes[0];
        int last = GridSizes[^1];
        Say($"n grew {last / first}x ({first} to {last}) but the steps grew {last * last / (first * first):N0}x " +
            $"({first * first} to {last * last:N0}). Double n and the work quadruples.", TermColor.Success);
    }

    // BRUTE FORCE: all answers and the pattern behind them.
    private void RevealGrid()
    {
        Say("ANSWERS:", TermColor.Success);
        foreach (int n in GridSizes)
            Say($"  n = {n,3}  ->  {n * n,6:N0} steps", TermColor.Success);
        Say("The outer loop runs n times, and each pass runs the inner loop n times: n x n.");
        Say("Doubling n (5 -> 10) quadrupled the work (25 -> 100).");

        int last = GridSizes[^1];
        Run(last * last);
        Host.SetBest(last * last);
        GridSolved();
    }

    // ================= Challenge 3: Student Match =================

    private static int Pairs(int n) => n * (n - 1) / 2;

    // "3 + 2 + 1" for n = 4: the checks each student makes, added up.
    private static string PairSum(int n) =>
        string.Join(" + ", Enumerable.Range(1, n - 1).Reverse());

    private void AskPairs()
    {
        int n = StudentCounts[_pairRound];
        if (_pairRound == 0)
        {
            Host.Steps.Reset();
            Host.SetBest(null);
        }
        Say("Student Match:");
        if (_pairRound == 0)
            Say("  string[] students = { " + string.Join(", ", _students.Select(s => $"\"{s}\"")) + " };");
        else
            Say($"  The students array now holds {n} students.");
        Say("  for (int i = 0; i < students.Length; i++)");
        Say("  {");
        Say("      for (int j = i + 1; j < students.Length; j++)");
        Say("      {");
        Say("          Console.WriteLine(students[i] + \" checks \" + students[j]);");
        Say("      }");
        Say("  }");
        Say($"With {n} students, how many 'checks' lines print? (type a number)");
        Host.SetNext($"type how many checks happen for {n} students.");
    }

    private Verdict JudgePairs(string input)
    {
        int n = StudentCounts[_pairRound];
        int answer = Pairs(n);

        if (!TryGetNumber(input, out int guess))
        {
            Say("Type just a number.", TermColor.Error);
            return Verdict.Wrong;
        }

        if (guess != answer)
        {
            if (guess == n * n)
                Say("That's every student vs every student, themselves included. This loop starts j at i + 1, so each pair is checked only once.",
                    TermColor.Error);
            else if (guess == n * (n - 1))
                Say("That counts each pair twice (A checks B AND B checks A). j starts at i + 1, so each pair is checked once.",
                    TermColor.Error);
            else if (guess == n - 1)
                Say("That's only the first student's checks. Add up every pass: (n-1) + (n-2) + ... + 1.",
                    TermColor.Error);
            else
                Say("Not quite. Count the checks for each i, then add them up.", TermColor.Error);
            return Verdict.Wrong;
        }

        Run(answer);
        Host.SetBest(answer);
        if (_pairRound == 0) ShowPairLines();
        Say($"> {n} students  ->  {PairSum(n)} = {answer} checks", TermColor.Success);

        _pairRound++;
        ResetHints();
        if (_pairRound >= StudentCounts.Length) return Verdict.Solved;

        Say();
        AskPairs();                 // next size
        return Verdict.Continue;
    }

    private string? PairsHint(int i)
    {
        int n = StudentCounts[_pairRound];
        return i switch
        {
            0 => "The first student is checked against everyone after them, the next one against everyone after THEM, and so on.",
            1 => $"With {n} students the passes add up like this: {PairSum(n)}.",
            _ => null
        };
    }

    private void ShowPairLines()
    {
        for (int i = 0; i < _students.Length; i++)
            for (int j = i + 1; j < _students.Length; j++)
                Say($"    {_students[i]} checks {_students[j]}");
    }

    private void PairsSolved()
    {
        Say("n x (n - 1) / 2 checks: about HALF of n x n. Big-O ignores constants like", TermColor.Success);
        Say($"the 1/2, so this is STILL O(n²). 100 students would need {Pairs(100):N0} checks.",
            TermColor.Success);
    }

    // BRUTE FORCE: both answers with the sums behind them.
    private void RevealPairs()
    {
        Say("ANSWERS:", TermColor.Success);
        foreach (int n in StudentCounts)
            Say($"  {n,2} students:  {PairSum(n)} = {Pairs(n)}", TermColor.Success);
        Say("Each student is checked against the ones AFTER them, so every pair counts once.");

        int last = Pairs(StudentCounts[^1]);
        Run(last);
        Host.SetBest(last);
        PairsSolved();
    }

    // ================= Challenge 4: Two Loop Layouts =================

    private void AskNest()
    {
        if (_nestPhase == 0)
        {
            Host.Steps.Reset();
            Host.SetBest(null);
            Say("Two Loop Layouts:");
            Say("  A)  for (int i = 0; i < n; i++) { Console.WriteLine(i); }");
            Say("      for (int j = 0; j < n; j++) { Console.WriteLine(j); }");
            Say();
            Say("  B)  for (int i = 0; i < n; i++)");
            Say("      {");
            Say("          for (int j = 0; j < n; j++) { Console.WriteLine(i + j); }");
            Say("      }");
            Say("Which layout is O(n²)? Type A or B.");
            Host.SetNext("type A or B: which layout is O(n²)?");
        }
        else if (_nestPhase == 1)
        {
            Say("Layout A = two loops, one AFTER the other.");
            Say($"With n = {NestN}, how many prints does layout A make? (type a number)");
            Host.SetNext($"type how many prints layout A makes when n = {NestN}.");
        }
        else
        {
            Say("Layout B = a loop INSIDE a loop.");
            Say($"With n = {NestN}, how many prints does layout B make? (type a number)");
            Host.SetNext($"type how many prints layout B makes when n = {NestN}.");
        }
    }

    private Verdict JudgeNest(string input)
    {
        if (_nestPhase == 0)
        {
            string s = Norm(input).ToUpperInvariant();
            if (s != "A" && s != "B")
            {
                Say("Type A or B.", TermColor.Error);
                return Verdict.Wrong;
            }
            if (s == "A")
            {
                Say("A's loops run one AFTER the other: n + n = 2n steps, which is just O(n). Look for a loop INSIDE a loop.",
                    TermColor.Error);
                return Verdict.Wrong;
            }

            Say("> B: the inner loop runs fully for every pass of the outer loop.", TermColor.Success);
            _nestPhase = 1;
            ResetHints();
            Say();
            AskNest();
            return Verdict.Continue;
        }

        if (!TryGetNumber(input, out int n))
        {
            Say("Type just a number.", TermColor.Error);
            return Verdict.Wrong;
        }

        if (_nestPhase == 1)
        {
            int answerA = 2 * NestN;
            if (n != answerA)
            {
                if (n == NestN)
                    Say("That's just one of A's loops. A has TWO loops one after the other: their steps add.",
                        TermColor.Error);
                else if (n == NestN * NestN)
                    Say("That's layout B. In A the loops are separate, so the steps ADD (n + n), not multiply.",
                        TermColor.Error);
                else
                    Say("Not quite. Two separate loops of n steps each.", TermColor.Error);
                return Verdict.Wrong;
            }

            Run(answerA);
            Say($"> layout A, n = {NestN}:  {NestN} + {NestN} = {answerA} steps", TermColor.Success);
            _nestPhase = 2;
            ResetHints();
            Say();
            AskNest();
            return Verdict.Continue;
        }

        int answerB = NestN * NestN;
        if (n != answerB)
        {
            if (n == 2 * NestN)
                Say("That's layout A. In B, EVERY pass of the outer loop runs the whole inner loop: n x n.",
                    TermColor.Error);
            else if (n == NestN)
                Say("That's a single pass of the inner loop. The outer loop repeats it n times.",
                    TermColor.Error);
            else
                Say("Not quite. n passes of the outer loop x n steps of the inner loop.", TermColor.Error);
            return Verdict.Wrong;
        }

        Run(answerB);
        Host.SetBest(answerB);
        Say($"> layout B, n = {NestN}:  {NestN} x {NestN} = {answerB:N0} steps", TermColor.Success);
        return Verdict.Solved;
    }

    private string? NestHint(int i) => (_nestPhase, i) switch
    {
        (0, 0) => "Look for a loop INSIDE another loop.",
        (0, 1) => "One layout runs its loops one after the other. The other puts one loop inside the other.",
        (1, 0) => "Layout A is two loops of n steps each, one after the other.",
        (1, 1) => "Loops one after another ADD their steps: n + n.",
        (2, 0) => "The outer loop runs n times, and each time the inner loop runs n times.",
        (2, 1) => "Nested loops MULTIPLY their steps.",
        _ => null
    };

    private void NestSolved()
    {
        Say($"2n vs n²: for n = {NestN} that's {2 * NestN} vs {NestN * NestN:N0}. Loops one after another ADD", TermColor.Success);
        Say("(still O(n)). Loops inside each other MULTIPLY (O(n²)).", TermColor.Success);
    }

    // BRUTE FORCE: the layout and both step counts.
    private void RevealNest()
    {
        Say("ANSWER: B is O(n²).", TermColor.Success);
        Say($"A: two separate loops, {NestN} + {NestN} = {2 * NestN} steps (that's O(n)).");
        Say($"B: a loop inside a loop, {NestN} x {NestN} = {NestN * NestN:N0} steps.");
        Run(NestN * NestN);
        Host.SetBest(NestN * NestN);
        NestSolved();
    }

    // ================= Challenge 5: Big-O =================

    private void AskBigO()
    {
        Say("Last check. n records, each compared with n records.");
        Say("Type the Big-O of a loop inside a loop.");
        Host.SetNext("type the Big-O of nested loops (written like O(n) or O(log n)).");
    }

    private string? ValidateBigO(string input)
    {
        string s = Norm(input).ToUpperInvariant();

        // Accepted spellings: O(n^2), O(n2), O(n²), O(n*n)
        if (s == "O(N^2)" || s == "O(N2)" || s == "O(N\u00B2)" || s == "O(N*N)") return null;

        if (s == "O(2N)")
            return "O(2n) is two loops one after the other: still linear. Nested loops MULTIPLY: n x n.";
        if (s == "O(2^N)" || s == "O(2\u207F)")
            return "2^n is exponential (that's Node 5). Quadratic means n SQUARED: n x n.";
        if (s == "O(N)") return "O(n) is a single loop. Here one loop sits INSIDE another.";
        if (s == "O(1)") return "O(1) is a direct jump. Nested loops do much more work.";
        if (s == "O(LOGN)" || s == "O(LOG2N)") return "O(log n) halves the data. Nested loops do n x n steps.";
        if (s.StartsWith("O(")) return "Not quite. n records, each compared with n records: how many steps?";
        return "Write it like O(...), for example O(n) or O(log n).";
    }

    private void RevealBigO()
    {
        Say("ANSWER: O(n²)", TermColor.Success);
        Say("Quadratic time: n passes x n steps = n x n. Double the data, four times the");
        Say("work. You can write it as O(n^2) or O(n²).");
    }

    // ================= Summary =================

    protected override void Summary()
    {
        Say();
        Say("=== NODE 4 CLEARED ===", TermColor.Success);
        Say("Steps needed as the data grows (the 4 classes so far):");
        Say("n".PadLeft(10) + "O(1)".PadLeft(7) + "O(log n)".PadLeft(10) + "O(n)".PadLeft(10) + "O(n²)".PadLeft(16));
        foreach (int n in new[] { 10, 100, 1000, 10000 })
        {
            int log = (int)Math.Floor(Math.Log2(n)) + 1;
            long square = (long)n * n;
            Say(n.ToString("N0").PadLeft(10) + "1".PadLeft(7) + log.ToString().PadLeft(10) +
                n.ToString("N0").PadLeft(10) + square.ToString("N0").PadLeft(16));
        }
        Say("10,000 records already need 100,000,000 steps. Nested loops get costly FAST.");
        Say($"Final trace: {Host.Trace.Level}%");
    }

    // ================= Input helpers =================

    private static bool TryGetNumber(string input, out int n) =>
        int.TryParse(Norm(input).Replace(",", ""), out n);
}