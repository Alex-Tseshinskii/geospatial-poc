using Circle.Poc.Batch;
using Poc.Model;

const int rerunCount = 5;

foreach (var option in Enum.GetValues<RunOption>())
{
    OneRunService oneRun;
    try
    {
        Console.WriteLine($"{option} run started, no index");
        oneRun = new OneRunService(option, false);
        await oneRun.RunAsync(rerunCount);
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error running w/o index: {e.Message}");
    }

    Console.WriteLine($"{option} run started, with index");
    oneRun = new OneRunService(option, true);
    await oneRun.RunAsync(rerunCount);
}