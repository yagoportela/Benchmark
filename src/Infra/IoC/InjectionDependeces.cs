using Amazon.DynamoDBv2;
using BenchmarkFinanceiro.Configuration;
using BenchmarkFinanceiro.Repositories;
using BenchmarkFinanceiro.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BenchmarkFinanceiro.Infra.IoC;

public static class InjectionDependeces
{
    public static void AddInjections(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Configuration/appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    
        AddConfigs(services, configuration);
        Inject(services, configuration);
    }

    private static void Inject(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAmazonDynamoDB>(provider =>
        {
            IOptions<AppSettings> awsConfigs = provider.GetRequiredService<IOptions<AppSettings>>();
            var dynamoDbConfig = new AmazonDynamoDBConfig
            {
                ServiceURL = awsConfigs.Value.DynamoDB.ServiceUrl
            };
            return new AmazonDynamoDBClient(dynamoDbConfig);
        });

        services.AddScoped<IPostgreSQLRepository, PostgreSQLRepository>();
        services.AddScoped<IDynamoDBRepository, DynamoDBRepository>();
        services.AddScoped<BenchmarkDynamoDbService>();
    }

    private static void AddConfigs(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration);
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }
}