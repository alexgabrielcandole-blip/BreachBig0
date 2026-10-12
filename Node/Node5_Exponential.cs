using BreachBigO.Core;

namespace BreachBigO.Nodes;

/// <summary>
/// NODE 5 - O(2^n) exponential time.
/// The PDF's light switches / coin toss / password examples only multiplied by 2 in a
/// loop (n steps), so here the work is GENERATING the combinations, which really is 2^n.
/// Also uses the PDF's recursive Choose(n, result) method (two calls per call).
/// </summary>
public class Node5_Exponential : Node
{
    public override int Number => 5;
    public override string Title => "VAULT CRACKER";
    public override string BigO => "O(2\u207F)";

    private static readonly int[] SwitchSizes = { 3, 4, 10 };
    private static readonly int[] CoinSizes = { 5, 6 };
    private const int PasswordLength = 3;
    private const int Calls = (1 << (PasswordLength + 1)) - 1;     // 1 + 2 + 4 + 8 = 15

    // Accepted answers for the loop bound (after spaces are removed).
    private static readonly HashSet<string> AcceptedBounds = new()
    {
        "1<<n", "(1<<n)", "Math.Pow(2,n)", "(int)Math.Pow(2,n)", "(int)(Math.Pow(2,n))"
    };

    private readonly Random _rng = new();

    // --- Challenge 1 (Light Switches) ---
    private int _switchStep;        // 0..2 = predict the patterns, 3 = fill in the loop bound

    // --- Challenge 2 (Coin Toss) ---
    private int _coinRound;

    // --- Challenge 3 (Password Choose) ---
    private readonly List<string> _passwords = new();   // what Choose(3, "") prints, in order
    private readonly int _nth;                          // which printed line to name (3-7)
    private int _passPhase;                             // 0 = name the line, 1 = count the calls

    public Node5_Exponential(INodeHost host) : base(host)
    {
        Generate(PasswordLength, "", _passwords);
        _nth = _rng.Next(3, 8);
    }

    protected override void Intro()
    {
        Say("=== NODE 5/5: VAULT CRACKER   [ O(2\u207F) - exponential time ] ===", TermColor.Success);
        Say();
        Say("The final vault is locked with ON/OFF choices. Every extra choice DOUBLES");
        Say("the number of combinations, and brute force means trying them ALL.");
        Say("So the work is 2 x 2 x 2 ... (n times): O(2\u207F).");
        Say();
        Say("WORKED EXAMPLE", TermColor.Warn);
        Say("  2 switches -> 4 patterns:  00  01  10  11");
        Say("  3 switches -> 8 patterns:  000 001 010 011 100 101 110 111");
        Say("Each new switch doubles the list: 2, 4, 8, 16, 32...");
        Say();
        Say("Careful: multiplying by 2 n times is cheap (n steps). The cost explodes");
        Say("when you must LIST or TRY every combination. That is the O(2\u207F) work here.");
    }

    protected override IEnumerable<Question> BuildQuestions()
    {
        // 1) Light switches (PDF example 1, fixed): predict, then generate with a real 2^n loop.
        yield return new Question
        {
            Ask = AskSwitches,
            Judge = JudgeSwitches,
            HintProvider = SwitchHint,
            Solved = SwitchesSolved,
            Reveal = RevealSwitches
        };

        // 2) Coin toss (PDF example 2, fixed): predict, then watch the counter explode.
        yield return new Question
        {
            Ask = AskCoins,
            Judge = JudgeCoins,
            HintProvider = CoinHint,
            Solved = CoinsSolved,
            Reveal = RevealCoins
        };

        // 3) Password Choose (the PDF's recursive method): name a line, count the calls.
        yield return new Question
        {
            Ask = AskPassword,
            Judge = JudgePassword,
            HintProvider = PasswordHint,
            Solved = PasswordSolved,
            Reveal = RevealPassword
        };

        // 4) Name the Big-O.
        yield return new Question
        {
            Ask = AskBigO,
            Validate = ValidateBigO,
            Solved = () => Say("Correct: O(2\u207F) = exponential time.", TermColor.Success),
            Reveal = RevealBigO,
            Hints = new[]
            {
                "Each extra item DOUBLES the work: 2 x 2 x 2 ... (n times).",
                "n is in the EXPONENT: 2 to the power n. Write it inside O( )."
            }
        };
    }

    // ================= Challenge 1: Light Switches =================

    private void AskSwitches()
    {
        if (_switchStep == 0)
        {
            Host.Steps.Reset();
            Host.SetBest(null);
        }

        if (_switchStep < SwitchSizes.Length)
        {
            int n = SwitchSizes[_switchStep];
            Say("Light Switches:");
            Say("  Each switch is OFF (0) or ON (1).");
            Say($"With {n} switches, how many different ON/OFF patterns are there? (type a number)");
            Host.SetNext($"type how many patterns {n} switches can make.");
        }
        else
        {
            Say("Light Switches: now GENERATE every pattern with a loop.");
            Say("  int n = 3;                         // number of switches");
            Say("  for (int mask = 0; mask < ___; mask++)");
            Say("  {");
            Say("      // every mask from 0 upward is one ON/OFF pattern");
            Say("  }");
            Say("Type what replaces ___ so the loop runs once per pattern (use n).");
            Host.SetNext("type what replaces ___ (the number of patterns, written with n).");
        }
    }

    private Verdict JudgeSwitches(string input)
    {
        // Steps 0..2: predict the number of patterns.
        if (_switchStep < SwitchSizes.Length)
        {
            int n = SwitchSizes[_switchStep];
            int answer = 1 << n;

            if (!TryGetNumber(input, out int guess))
            {
                Say("Type just a number.", TermColor.Error);
                return Verdict.Wrong;
            }

            if (guess != answer)
            {
                if (guess == 2 * n)
                    Say("That's 2 x n. Every extra switch DOUBLES the patterns: 2 x 2 x 2... (n times).", TermColor.Error);
                else if (guess == n * n)
                    Say("That's n squared. Each switch has 2 options and the choices multiply: 2 x 2 x 2...", TermColor.Error);
                else if (guess == n)
                    Say("That's the number of switches, not the number of patterns.", TermColor.Error);
                else
                    Say("Not quite. Each switch has 2 options, and every extra switch doubles the patterns.", TermColor.Error);
                return Verdict.Wrong;
            }

            Run(answer);
            Say($"> {n} switches  ->  {answer:N0} patterns", TermColor.Success);

            _switchStep++;
            ResetHints();
            Say();
            AskSwitches();          // next size, then the loop-bound step
            return Verdict.Continue;
        }

        // Step 3: fill in the loop bound.
        string? error = ValidateBound(input);
        if (error != null)
        {
            Say(error, TermColor.Error);
            return Verdict.Wrong;
        }
        return Verdict.Solved;
    }

    private string? ValidateBound(string input)
    {
        string s = Norm(input);
        if (AcceptedBounds.Contains(s)) return null;

        if (s.Contains('^'))
            return "In C#, ^ means XOR, not 'power'! Use Math.Pow(2, n) or a bit shift.";
        if (s == "2*n" || s == "n*2")
            return "2 * n is just double n (linear). We need 2 multiplied by ITSELF n times.";
        if (s == "n*n")
            return "n * n is n squared (Node 4). We need 2 multiplied by itself n times.";
        if (s == "n")
            return "n passes isn't enough: we need one pass per PATTERN, and there are far more patterns than n.";
        if (int.TryParse(s, out _))
            return "A fixed number only works for one n. Write it with n so it works for any number of switches.";
        return "Not quite. The loop must run once per pattern, and there are 2 x 2 x ... x 2 (n times) patterns.";
    }

    private string? SwitchHint(int i)
    {
        if (_switchStep < SwitchSizes.Length)
        {
            return i switch
            {
                0 => "Each switch has 2 options. Two switches: 00, 01, 10, 11 = 4 patterns.",
                1 => "Every extra switch doubles the list: 2, 4, 8, 16...",
                _ => null
            };
        }

        return i switch
        {
            0 => "The loop must run once per pattern. How many patterns do n switches make?",
            1 => "It's 2 multiplied by itself n times. C# can do that with Math.Pow or a bit shift (<<).",
            _ => null
        };
    }

    private void SwitchesSolved()
    {
        Say("> for (int mask = 0; mask < (1 << n); mask++)   with n = 3:", TermColor.Success);
        string patterns = string.Join("  ",
            Enumerable.Range(0, 1 << 3).Select(m => Convert.ToString(m, 2).PadLeft(3, '0')));
        Say("  " + patterns, TermColor.Success);
        Run(1 << 3);
        Say("The loop really ran 2^3 = 8 times: ONE pass per pattern.");
        Say("(The PDF's loop only multiplied by 2 n times: that COMPUTES 2^n cheaply but never");
        Say("generates a single pattern. Generating them is the real O(2\u207F) work.)");
    }

    // BRUTE FORCE: all answers, the loop bound, and the generated patterns.
    private void RevealSwitches()
    {
        Say("ANSWERS:", TermColor.Success);
        foreach (int n in SwitchSizes)
            Say($"  {n,2} switches  ->  {1 << n,5:N0} patterns", TermColor.Success);
        Say("Each switch doubles the list. The loop bound is 1 << n (or Math.Pow(2, n)).");
        Say("Note: in C#, 2^n is XOR, not 'power'.");
        SwitchesSolved();
    }

    // ================= Challenge 2: Coin Toss =================

    private void AskCoins()
    {
        int n = CoinSizes[_coinRound];
        if (_coinRound == 0)
        {
            Host.Steps.Reset();
            Host.SetBest(null);
        }
        Say("Coin Toss:");
        Say("  Each toss lands on Heads or Tails.");
        Say($"  Tossing a coin {n} times, how many different sequences of results exist?");
        Say("  (like HHTHT: every different order counts)");
        Say("Type a number.");
        Host.SetNext($"type how many different results {n} tosses can give.");
    }

    private Verdict JudgeCoins(string input)
    {
        int n = CoinSizes[_coinRound];
        int answer = 1 << n;

        if (!TryGetNumber(input, out int guess))
        {
            Say("Type just a number.", TermColor.Error);
            return Verdict.Wrong;
        }

        if (guess != answer)
        {
            if (guess == 2 * n)
                Say("That's 2 x n. Each extra toss DOUBLES the number of results: 2 x 2 x 2... (n times).",
                    TermColor.Error);
            else if (guess == n * n)
                Say("That's n squared. Each toss has 2 outcomes and the choices multiply: 2 x 2 x 2...",
                    TermColor.Error);
            else if (guess == n)
                Say("That's the number of tosses, not the number of results.", TermColor.Error);
            else if (_coinRound > 0 && guess == 1 << CoinSizes[_coinRound - 1])
                Say("That's the answer for one toss fewer. One more coin DOUBLES it.", TermColor.Error);
            else
                Say("Not quite. Each toss has 2 outcomes, and every extra toss doubles the total.", TermColor.Error);
            return Verdict.Wrong;
        }

        Run(answer);
        Say($"> {n} tosses  ->  {answer:N0} sequences", TermColor.Success);

        _coinRound++;
        ResetHints();
        if (_coinRound >= CoinSizes.Length) return Verdict.Solved;

        Say();
        AskCoins();                 // next size
        return Verdict.Continue;
    }

    private string? CoinHint(int i) => i switch
    {
        0 => "Each toss has 2 outcomes. Two tosses: HH, HT, TH, TT = 4 sequences.",
        1 => _coinRound > 0
            ? "One more toss than before: the total doubles."
            : "Every extra toss doubles the total: 2, 4, 8, 16...",
        _ => null
    };

    private void CoinsSolved()
    {
        Say("Now watch it explode. Generating EVERY sequence costs one step each:", TermColor.Warn);
        foreach (int n in new[] { 10, 20, 30 })
        {
            long steps = 1L << n;
            Run((int)steps);
            Say($"  {n,2} tosses  ->  {steps,15:N0} steps", TermColor.Success);
        }
        Say($"40 tosses would be {1L << 40:N0} steps: over a TRILLION.");
        Say("Every extra coin DOUBLES the work. That is O(2\u207F).");
    }

    // BRUTE FORCE: both answers, then the explosion.
    private void RevealCoins()
    {
        Say("ANSWERS:", TermColor.Success);
        foreach (int n in CoinSizes)
            Say($"  {n} tosses  ->  {1 << n} sequences", TermColor.Success);
        Say("Each toss has 2 outcomes and the choices multiply: 2 x 2 x ... (n times).");
        CoinsSolved();
    }

    // ================= Challenge 3: Password Choose =================

    private static void Generate(int n, string result, List<string> output)
    {
        if (n == 0)
        {
            output.Add(result);
            return;
        }
        Generate(n - 1, result + "A", output);
        Generate(n - 1, result + "B", output);
    }

    private void AskPassword()
    {
        if (_passPhase == 0)
        {
            Host.Steps.Reset();
            Host.SetBest(null);
            Say("Password Choose:");
            Say("  static void Choose(int n, string result)");
            Say("  {");
            Say("      if (n == 0)");
            Say("      {");
            Say("          Console.WriteLine(result);");
            Say("          return;");
            Say("      }");
            Say("      Choose(n - 1, result + \"A\");");
            Say("      Choose(n - 1, result + \"B\");");
            Say("  }");
            Say("  Choose(3, \"\");      // prints every 3-letter password made of A and B");
            Say($"The lines print in order, with AAA as line 1. What does line {_nth} say?");
            Host.SetNext($"type the password printed on line {_nth} (3 letters, A or B).");
        }
        else
        {
            Say("Next: count the CALLS of Choose for Choose(3, \"\").");
            Say("Count every call, including the first one and the ones that just print.");
            Say("How many times does Choose run in total? (type a number)");
            Host.SetNext("type the total number of Choose calls.");
        }
    }

    private Verdict JudgePassword(string input)
    {
        if (_passPhase == 0)
        {
            string s = Norm(input).ToUpperInvariant();
            string expected = _passwords[_nth - 1];

            if (s == expected)
            {
                Say($"> line {_nth}: {expected}", TermColor.Success);
                _passPhase = 1;
                ResetHints();
                Say();
                AskPassword();      // now the call-counting question
                return Verdict.Continue;
            }

            if (s.Length != PasswordLength || s.Any(c => c != 'A' && c != 'B'))
                Say($"A password here is exactly {PasswordLength} letters, each A or B.", TermColor.Error);
            else if (_nth < _passwords.Count && s == _passwords[_nth])
                Say("Off by one: AAA is line 1, not line 0.", TermColor.Error);
            else if (_nth >= 2 && s == _passwords[_nth - 2])
                Say("That's the line BEFORE. AAA is line 1.", TermColor.Error);
            else
                Say("Not quite. Each call tries A before B, so the lines come out in order. The right-most letter changes fastest.",
                    TermColor.Error);
            return Verdict.Wrong;
        }

        // Phase 1: total number of calls.
        if (!TryGetNumber(input, out int n))
        {
            Say("Type just a number.", TermColor.Error);
            return Verdict.Wrong;
        }

        if (n != Calls)
        {
            if (n == 1 << PasswordLength)
                Say("That's only the calls that print (n = 0). Count the calls on the way down too.", TermColor.Error);
            else if (n == (1 << PasswordLength) - 1)
                Say("That misses the last level: the 8 calls with n = 0 are calls too.", TermColor.Error);
            else
                Say("Not quite. Draw the tree: 1 call at the top, each call makes 2 more, until n reaches 0.",
                    TermColor.Error);
            return Verdict.Wrong;
        }

        Run(Calls);
        Say($"> 1 + 2 + 4 + 8 = {Calls} calls", TermColor.Success);
        return Verdict.Solved;
    }

    private string? PasswordHint(int i) => (_passPhase, i) switch
    {
        (0, 0) => "Each call tries A before B, so the lines come out in order, starting with AAA as line 1.",
        (0, 1) => "It's counting in binary with A = 0 and B = 1. Line 1 is 000.",
        (1, 0) => "Draw it: 1 call at the top, then 2 calls, then 4...",
        (1, 1) => "The levels hold 1, 2, 4 and 8 calls. Add them up.",
        _ => null
    };

    private void PasswordSolved()
    {
        Say();
        Say("Now the hacker's problem. Brute-forcing a password means trying EVERY one.", TermColor.Warn);
        Say("At 1,000 guesses per second, a password with n A/B positions takes:");
        Say("positions".PadLeft(10) + "guesses".PadLeft(24) + "time".PadLeft(16));
        foreach (int n in new[] { 10, 20, 30, 40, 50 })
        {
            long guesses = 1L << n;
            Say(n.ToString().PadLeft(10) + guesses.ToString("N0").PadLeft(24) +
                HumanTime(guesses / 1000.0).PadLeft(16), TermColor.Success);
        }
        Say("Each extra position doubles the time. That's why long passwords are safe.");
    }

    // BRUTE FORCE: the line, the full output, and the call tree.
    private void RevealPassword()
    {
        Say($"ANSWER (line {_nth}): {_passwords[_nth - 1]}", TermColor.Success);
        Say("Choose prints, in order:  " + string.Join("  ", _passwords));
        Say("Each call tries A first, then B: counting in binary with A = 0 and B = 1.");
        Say($"CALLS: 1 + 2 + 4 + 8 = {Calls} (every call makes 2 more until n reaches 0).", TermColor.Success);
        Run(Calls);
        PasswordSolved();
    }

    private static string HumanTime(double seconds)
    {
        if (seconds < 60) return $"{seconds:0.#} sec";
        if (seconds < 3600) return $"{seconds / 60:0.#} min";
        if (seconds < 86400) return $"{seconds / 3600:0.#} hours";

        double days = seconds / 86400;
        if (days < 365.25) return $"{days:0.#} days";

        double years = days / 365.25;
        return years < 1000 ? $"{years:0.#} years" : $"{years:N0} years";
    }

    // ================= Challenge 4: Big-O =================

    private void AskBigO()
    {
        Say("Last check. Every extra item DOUBLES the work: 2 x 2 x 2 ... (n times).");
        Say("Type the Big-O of trying every combination.");
        Host.SetNext("type the Big-O of trying every combination (written like O(n) or O(1)).");
    }

    private string? ValidateBigO(string input)
    {
        string s = Norm(input).ToUpperInvariant();

        // Accepted spellings: O(2^n) and O(2ⁿ)
        if (s == "O(2^N)" || s == "O(2\u207F)") return null;

        if (s == "O(2N)")
            return "O(2n) is just double n: still linear. 2^n means 2 multiplied by ITSELF n times.";
        if (s == "O(N^2)" || s == "O(N2)" || s == "O(N\u00B2)")
            return "n squared grows much slower. In 2^n the n is in the EXPONENT: 2 x 2 x ... (n times).";
        if (s == "O(N)") return "O(n) is a single pass. Trying every combination is far more work.";
        if (s == "O(1)") return "O(1) is a direct jump. Trying every combination is the opposite.";
        if (s == "O(LOGN)" || s == "O(LOG2N)") return "O(log n) halves the work. Here the work DOUBLES.";
        if (s.StartsWith("O(")) return "Not quite. Each extra item doubles the work: what grows like that?";
        return "Write it like O(...), for example O(n) or O(1).";
    }

    private void RevealBigO()
    {
        Say("ANSWER: O(2^n)  (also written O(2\u207F))", TermColor.Success);
        Say("Each extra item doubles the work: 2 x 2 x ... (n times). 20 items is about a");
        Say("million steps, 30 items is over a billion.");
    }

    // ================= Summary =================

    protected override void Summary()
    {
        Say();
        Say("=== NODE 5 CLEARED ===", TermColor.Success);
        Say("ALL FIVE CLASSES side by side (steps for n items):");
        Say("n".PadLeft(6) + "O(1)".PadLeft(6) + "O(log n)".PadLeft(10) + "O(n)".PadLeft(6) +
            "O(n\u00B2)".PadLeft(8) + "O(2\u207F)".PadLeft(16));
        foreach (int n in new[] { 10, 20, 30 })
        {
            int log = (int)Math.Floor(Math.Log2(n)) + 1;
            Say(n.ToString().PadLeft(6) + "1".PadLeft(6) + log.ToString().PadLeft(10) +
                n.ToString().PadLeft(6) + (n * n).ToString("N0").PadLeft(8) +
                (1L << n).ToString("N0").PadLeft(16));
        }
        Say("At n = 30: O(n\u00B2) is 900 steps, but O(2\u207F) is over a BILLION.");
        Say($"Final trace: {Host.Trace.Level}%");
    }

    // ================= Input helpers =================

    private static bool TryGetNumber(string input, out int n) =>
        int.TryParse(Norm(input).Replace(",", ""), out n);
}