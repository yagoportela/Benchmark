using BenchmarkDotNet.Attributes;
using BenchmarkFinanceiro.Infra.IoC;
using BenchmarkFinanceiro.Models;
using BenchmarkFinanceiro.Repositories;
using BenchmarkFinanceiro.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BenchmarkFinanceiro.Tests;

[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput, launchCount: 5)]
public class BenchmarkPostgresTest
{
    private BenchmarkPostgresService _benchmarkPostgresService = null!;
    private List<AggregateValorPostgres> _valorPostgres = [];

    public void SetupDynamoDb() { }

    [GlobalSetup]
    public async Task SetupGlobal()
    {
        var services = new ServiceCollection();
        InjectionDependeces.AddInjections(services);
        var serviceProvider = services.BuildServiceProvider();

        IPostgreSQLRepository dynamoDBRepository = serviceProvider.GetRequiredService<IPostgreSQLRepository>();
        _benchmarkPostgresService = new(dynamoDBRepository);

        _valorPostgres = BenchmarkPostgresService.CriarDados(1, 3, 5, 10);
        await _benchmarkPostgresService.Insert(_valorPostgres);
    }

    [IterationSetup(Target = nameof(BenchmarkInsert))]
    public void SetupInsert() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkSerieData))]
    public void SetupSerieData() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkAtributoData))]
    public void SetupAtributoData() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFamiliaData))]
    public void SetupFamiliaData() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkSerieAtributoData))]
    public void SetupSerieAtributoData() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFamiliaAtributoData))]
    public void SetupFamiliaAtributoData() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFamiliaSerieAtributoData))]
    public void SetupFamiliaSerieAtributoData() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkDeleteAll))]
    public void SetupDeleteAll() => SetupDynamoDb();

    [Benchmark]
    public async Task BenchmarkInsert()
    {
        var valorDynamoDBItens = BenchmarkPostgresService.CriarDados(1, 3, 5, 10);
        await _benchmarkPostgresService.Insert(valorDynamoDBItens);
    }

    [Benchmark]
    public async Task BenchmarkItem()
    {
        await _benchmarkPostgresService.GetSerieItemSerieAtributoData(_valorPostgres);
    }

    [Benchmark]
    public async Task BenchmarkSerieData()
    {
        await _benchmarkPostgresService.GetSerieData(_valorPostgres);
    }

    [Benchmark]
    public async Task BenchmarkAtributoData()
    {
        await _benchmarkPostgresService.GetAtributoData(_valorPostgres);
    }

    [Benchmark]
    public async Task BenchmarkFamiliaData()
    {
        await _benchmarkPostgresService.GetFamiliaData(_valorPostgres);
    }

    [Benchmark]
    public async Task BenchmarkSerieAtributoData()
    {
        await _benchmarkPostgresService.GetSerieAtributoData(_valorPostgres);
    }

    [Benchmark]
    public async Task BenchmarkFamiliaAtributoData()
    {
        await _benchmarkPostgresService.GetFamiliaAtributoData(_valorPostgres);
    }

    [Benchmark]
    public async Task BenchmarkFamiliaSerieAtributoData()
    {
        await _benchmarkPostgresService.GetFamiliaSerieAtributoData(_valorPostgres);
    }

    public async Task BenchmarkDeleteAll()
    {
        await _benchmarkPostgresService.DeleteAllItens();
    }
}
