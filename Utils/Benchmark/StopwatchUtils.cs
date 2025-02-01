using System.Diagnostics;

namespace Utils.Benchmark;

public static class StopwatchUtils
{
    public static async Task<(T, TimeSpan)> ExecuteAndMeasureAsync<T>(Func<Task<T>> action)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await action();
        stopwatch.Stop();

        return (result, stopwatch.Elapsed);
    }
}