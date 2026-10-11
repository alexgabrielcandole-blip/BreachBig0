namespace BreachBigO.Core;

/// <summary>
/// Counts the operations a piece of code "costs". The side panel shows this live.
/// </summary>
public class StepCounter
{
    public int Count { get; private set; }

    /// <summary>Raised whenever the count changes (the window listens to refresh the panel).</summary>
    public event Action? Changed;

    public void Add(int amount = 1)
    {
        Count += amount;
        Changed?.Invoke();
    }

    public void Reset()
    {
        Count = 0;
        Changed?.Invoke();
    }
}