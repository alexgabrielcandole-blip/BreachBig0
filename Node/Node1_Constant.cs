using BreachBigO.Core;

namespace BreachBigO.Nodes;

/// <summary>
/// NODE 1 - O(1) constant time.
/// Covers the PDF's 3 examples: array index, add two numbers, first item.
/// </summary>
public class Node1_Constant : Node
{
    public override int Number => 1;
    public override string Title => "DIRECT ACCESS";
    public override string BigO => "O(1)";

    private static readonly int[] Shelf = { 10, 20, 30, 40, 50 };
    private static readonly string[] Fruits = { "Apple", "Banana", "Mango" };

    // Rolled once per run so 'help' can re-show the same challenge.
    private readonly int _shelfIndex;       // position the player must fetch (1-4)
    private readonly int _runeA;
    private readonly int _runeB;

    public Node1_Constant(INodeHost host) : base(host)
    {
        var rng = new Random();
        _shelfIndex = rng.Next(1, 5);
        _runeA = rng.Next(12, 49);
        _runeB = rng.Next(12, 49);
    }

    protected override void Intro()
    {
        Host.SetBest(1);    // in O(1) the best (and only) cost is always 1 step

        Say("=== NODE 1/5: DIRECT ACCESS   [ O(1) - constant time ] ===", TermColor.Success);
        Say();
        Say("The vault keeps its data in arrays. In O(1), the work stays the");
        Say("SAME no matter how much data there is.");
        Say();
        Say("Watch the STEP COUNTER on the right: it counts what your code costs.");
        Say("Wrong answers raise your TRACE. At 100% you're caught and the node");
        Say("restarts. 'hint' helps, but it raises trace too.");
        Say();
        Say("WORKED EXAMPLE", TermColor.Warn);
        Say("  int[] codes = { 7, 14, 21, 28 };");
        Say("  int x = codes[1];        // x is 14");
        Say("Arrays count positions from 0. The computer jumps straight to");
        Say("position 1: 1 step, even if the array held a million items.");
    }

    protected override IEnumerable<Question> BuildQuestions()
    {
        int target = Shelf[_shelfIndex];
        string[] shelfText = Shelf.Select(v => v.ToString()).ToArray();
        int sum = _runeA + _runeB;

        // 1) Array index (PDF example 1) - type the real C#
        yield return new Question
        {
            Ask = () =>
            {
                Say("Treasure Shelf:");
                Say("  int[] numbers = { 10, 20, 30, 40, 50 };");
                Say($"Type the C# that fetches the value {target}.");
                Host.SetNext($"type the C# that fetches the value {target}.");
            },
            Validate = input => CheckIndexAnswer(input, "numbers", _shelfIndex, shelfText),
            Solved = () =>
            {
                Run(1);
                Say($"> numbers[{_shelfIndex}]  =  {target}      [1 step]", TermColor.Success);
                Say($"A scan from the start would have taken {_shelfIndex + 1} steps.");
            },
            Hints = new[]
            {
                "Arrays are read as name[position]. Positions start at 0.",
                "Count from 0: 10 is at position 0, 20 at position 1, 30 at 2..."
            }
        };

        // 2) Predict the steps on a huge shelf
        yield return new Question
        {
            Ask = () =>
            {
                Say("Now the shelf holds 1,000,000 items.");
                Say($"How many steps does numbers[{_shelfIndex}] take now? (type a number)");
                Host.SetNext("type how many steps the lookup takes on 1,000,000 items.");
            },
            Validate = input =>
            {
                if (!int.TryParse(Norm(input).Replace(",", ""), out int n))
                    return "Type just a number of steps.";
                if (n == 1) return null;
                if (n > 1)
                    return "That's what a scan would cost. A direct lookup jumps straight to the position.";
                return "It still takes 1 step to do the lookup, not zero.";
            },
            Solved = () =>
            {
                Run(1);
                Say("> 1,000,000 items  ->  1 step", TermColor.Success);
                Say("Same cost as 5 items. The size doesn't matter.");
            },
            Hints = new[]
            {
                "Does the computer walk past the other items, or jump straight to the position?",
                "A jump is 1 step. The size of the shelf doesn't change that."
            }
        };

        // 3) Add two numbers (PDF example 2) - hand calculation
        yield return new Question
        {
            Ask = () =>
            {
                Say("Rune Lock:");
                Say($"  int a = {_runeA};");
                Say($"  int b = {_runeB};");
                Say("  int sum = a + b;");
                Say("Work out sum by hand and type it.");
                Host.SetNext("type the value of sum.");
            },
            Validate = input =>
            {
                if (!int.TryParse(Norm(input), out int n)) return "Type just the number.";
                return n == sum ? null : "Not quite. Add them again.";
            },
            Solved = () =>
            {
                Run(1);
                Say($"> sum = {sum}      [1 step]", TermColor.Success);
                Say("One addition is 1 step, however big the numbers are.");
            },
            Hints = new[]
            {
                "Just add a and b by hand.",
                "Add the tens first, then the ones."
            }
        };

        // 4) First item (PDF example 3) - type the real C#, then watch the data grow
        yield return new Question
        {
            Ask = () =>
            {
                Say("Fruit Crate:");
                Say("  string[] fruits = { \"Apple\", \"Banana\", \"Mango\" };");
                Say("Type the C# that gets the FIRST item.");
                Host.SetNext("type the C# that gets the first fruit.");
            },
            Validate = input => CheckIndexAnswer(input, "fruits", 0, Fruits),
            Solved = () =>
            {
                Run(1);
                Say("> fruits[0]  =  \"Apple\"      [1 step]", TermColor.Success);
                Say("Now the crate keeps growing:");
                foreach (int n in new[] { 3, 30, 300, 3000 })
                {
                    Run(1);
                    Say($"  crate with {n,4} items  ->  1 step", TermColor.Success);
                }
                Say("The crate grew 1000x. The steps didn't.");
            },
            Hints = new[]
            {
                "You want the item at the very start. Which position is that?",
                "The first position is 0. Use the form fruits[position]."
            }
        };

        // 5) Name the Big-O
        yield return new Question
        {
            Ask = () =>
            {
                Say("Last check. Getting an item by its position, whatever the size.");
                Say("Type its Big-O (written like O(n) or O(log n)).");
                Host.SetNext("type the Big-O of getting an item by its position.");
            },
            Validate = input =>
            {
                string s = Norm(input).ToUpperInvariant();
                if (s == "O(1)") return null;
                if (s == "O(N)") return "O(n) means more items = more work. Here the work never grows.";
                if (s.StartsWith("O(")) return "Not quite. Does the work grow when the data grows?";
                return "Write it like O(...), for example O(n) or O(log n).";
            },
            Solved = () => Say("Correct: O(1) = constant time.", TermColor.Success),
            Hints = new[]
            {
                "Does the work grow when the shelf grows?",
                "Constant means a fixed number of steps. What number goes inside O( )?"
            }
        };
    }

    protected override void Summary()
    {
        Say();
        Say("=== NODE 1 CLEARED ===", TermColor.Success);
        Say("Steps to fetch one item as the data grows:");
        Say("         n    direct O(1)    scan O(n)");
        foreach (int n in new[] { 10, 100, 1000, 1000000 })
            Say($"{n,10:N0}{1,15}{n,13:N0}");
        Say("Direct access never grows. A scan grows with n.");
        Say($"Final trace: {Host.Trace.Level}%");
    }
}