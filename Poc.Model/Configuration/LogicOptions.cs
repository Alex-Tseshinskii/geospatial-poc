namespace Poc.Model.Configuration;

public enum MatchRule
{
    State = 1,
    County = 2,
}

public class LogicOptions
{
    public required MatchRule MatchRule { get; set; }

    public bool MongoCacheMaxRadius { get; set; } = false;
}