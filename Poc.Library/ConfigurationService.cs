using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Poc.Model.Configuration;
using SourceData.Model;

namespace Poc.Library;

public class ConfigurationService
{
    private IConfiguration? _configuration;

    public void Build(string[] commandLineArguments)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .AddCommandLine(commandLineArguments);

        _configuration = configurationBuilder.Build();
    }

    public void AddConfiguration(IServiceCollection services)
    {
        EnsureConfigurationBuilt();

        services.Configure<MongoOptions>(_configuration.GetSection("Mongo"));
        services.Configure<PostgreOptions>(_configuration.GetSection("Postgre"));
        services.Configure<SourcesOptions>(_configuration.GetSection("Sources"));
        services.Configure<LogicOptions>(_configuration.GetSection("Logic"));
    }

    public void AddLoggingConfiguration(ILoggingBuilder loggingBuilder)
    {
        EnsureConfigurationBuilt();

        loggingBuilder.AddConfiguration(_configuration.GetSection("Logging"));
        loggingBuilder.AddConsole();
    }

    [MemberNotNull(nameof(_configuration))]
    private void EnsureConfigurationBuilt()
    {
        if (_configuration is null)
        {
            throw new InvalidOperationException("Configuration not built");
        }
    }
}