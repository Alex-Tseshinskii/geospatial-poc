using Poc.Model;
using Polygon.Poc.Batch;

const int rerunCount = 5;

foreach (var option in Enum.GetValues<RunOption>().Where(ro => ro != RunOption.Mongo))
{
    OneRunService oneRun;

    Console.WriteLine($"{option} run started, no index");
    oneRun = new OneRunService(option, false);
    await oneRun.RunAsync(rerunCount);

    Console.WriteLine($"{option} run started, with index");
    oneRun = new OneRunService(option, true);
    await oneRun.RunAsync(rerunCount);
}