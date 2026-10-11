namespace BreachBigO.Core;

/// <summary>
/// The fail-state meter. Wrong answers and hints raise it; at 100% the player is "traced".
/// </summary>
public class TraceMeter
{
    public const int Max = 100;
    public const int WrongAnswerCost = 10;
    public const int HintCost = 5;          // matches the text on the HINT button

    public int Level { get; private set; }
    public bool IsFull => Level >= Max;

    public event Action? Changed;

    public void Add(int amount)
    {
        Level = Math.Min(Level + amount, Max);
        Changed?.Invoke();
    }

    public void Reset()
    {
        Level = 0;
        Changed?.Invoke();
    }
}