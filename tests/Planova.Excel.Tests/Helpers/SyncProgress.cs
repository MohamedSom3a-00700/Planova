namespace Planova.Excel.Tests.Helpers;

public class SyncProgress : IProgress<int>
{
    private readonly Action<int> _handler;
    public SyncProgress(Action<int> handler) => _handler = handler;
    public void Report(int value) => _handler(value);
}
