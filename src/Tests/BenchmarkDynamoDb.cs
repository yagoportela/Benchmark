using BenchmarkDotNet.Attributes;
using BenchmarkFinanceiro.Infra.IoC;
using BenchmarkFinanceiro.Models;
using BenchmarkFinanceiro.Repositories;
using BenchmarkFinanceiro.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BenchmarkFinanceiro.Tests;

// [SimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput, launchCount: 5)]
public class BenchmarkDynamoDbTest
{
    private BenchmarkDynamoDbService _benchmarkDynamoDb = null!;
    private List<ValorDynamoDB> _valorDynamoDB = [];

    public void SetupDynamoDb(){}

    [GlobalSetup]
    public async Task SetupGlobal()
    {
        var services = new ServiceCollection();
        InjectionDependeces.AddInjections(services);
        var serviceProvider = services.BuildServiceProvider();

        IDynamoDBRepository dynamoDBRepository = serviceProvider.GetRequiredService<IDynamoDBRepository>();
        _benchmarkDynamoDb = new(dynamoDBRepository);

        var valorDynamoDBItens = await _benchmarkDynamoDb.GetBatch(10000, 51);

        if (valorDynamoDBItens.Count < 50)
        {
            var novosValorDynamoDBItens = BenchmarkDynamoDbService.CriarDados(1, 3, 5, 10);
            await _benchmarkDynamoDb.Insert(novosValorDynamoDBItens);
            valorDynamoDBItens.AddRange(novosValorDynamoDBItens);
        }
        
        _valorDynamoDB.AddRange(valorDynamoDBItens);
    }

    [IterationSetup(Target = nameof(BenchmarkInsertDynamoDb))]
    public void SetupInsertDynamoDb() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkInsertBatchDynamoDb))]
    public void SetupInsertBatchDynamoDb() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkItemDataDynamoDb))]
    public void SetupItemDataDynamoDb() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFamiliaDataDynamoDb))]
    public void SetupFamiliaDataDynamoDb() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFiltroFamiliaDataDynamoDb_limitado))]
    public void SetupFiltroFamiliaDataDynamoDb() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkSerieFamiliaDataDynamoDb))]
    public void SetupSerieFamiliaDataDynamoDb() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFiltroSerieFamiliaDataDynamoDb_limitado))]
    public void SetupFiltroSerieFamiliaDataDynamoDb() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkSerieAtributoDataDynamoDb))]
    public void SetupSerieAtributoDataDynamoDb() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFiltoSerieAtributoData_limitado))]
    public void SetupFiltoSerieAtributoData() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFiltroItemFamiliaAtributoData_limitado))]
    public void SetupFiltroItemFamiliaAtributoData() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFiltroFamiliaAtributoData_limitado))]
    public void SetupFiltroFamiliaAtributoData() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkSerieDataDynamoDb))]
    public void SetupSerieDataDynamoDb() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFiltoSerieData_limitado))]
    public void SetupFiltoSerieData() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkFiltoSerieDynamoDb))]
    public void SetupFiltoSerieDynamoDb() => SetupDynamoDb();
    [IterationSetup(Target = nameof(BenchmarkDeleteAllDynamoDb))]
    public void SetupDeleteAllDynamoDb() => SetupDynamoDb();

    [Benchmark]
    public async Task BenchmarkInsertDynamoDb()
    {
        var valorDynamoDBItens = BenchmarkDynamoDbService.CriarDados(1, 3, 5, 10);
        await _benchmarkDynamoDb.Insert(valorDynamoDBItens);
    }

    [Benchmark]
    public async Task BenchmarkInsertBatchDynamoDb()
    {
        var valorDynamoDBBatch = BenchmarkDynamoDbService.CriarDados(1, 3, 5, 10);
        await _benchmarkDynamoDb.InsertBatch(valorDynamoDBBatch);
    }

    [Benchmark]
    public async Task BenchmarkItemDataDynamoDb()
    {
        await _benchmarkDynamoDb.GetItemData(_valorDynamoDB);
    }

    [Benchmark]
    public async Task BenchmarkSerieAtributoDataDynamoDb()
    {
        await _benchmarkDynamoDb.GetSerieAtributoData(_valorDynamoDB);
    }

    [Benchmark]
    public async Task BenchmarkFiltoSerieAtributoData_limitado()
    {
        await _benchmarkDynamoDb.GetFiltoSerieAtributoData(_valorDynamoDB, 10);
    }

    [Benchmark]
    public async Task BenchmarkFiltroItemFamiliaAtributoData_limitado()
    {
        await _benchmarkDynamoDb.GetFiltroItemFamiliaAtributoData(_valorDynamoDB, 2);
    }

    [Benchmark]
    public async Task BenchmarkFiltroFamiliaAtributoData_limitado()
    {
        await _benchmarkDynamoDb.GetFiltroFamiliaAtributoData(_valorDynamoDB, 1, 1);
    }

    [Benchmark]
    public async Task BenchmarkFamiliaDataDynamoDb()
    {
        await _benchmarkDynamoDb.GetFamiliaData(_valorDynamoDB);
    }

    [Benchmark]
    public async Task BenchmarkFiltroFamiliaDataDynamoDb_limitado()
    {
        await _benchmarkDynamoDb.GetFiltroFamiliaData(_valorDynamoDB, 2);
    }

    [Benchmark]
    public async Task BenchmarkSerieDataDynamoDb()
    {
        await _benchmarkDynamoDb.GetSerieData(_valorDynamoDB);
    }

    [Benchmark]
    public async Task BenchmarkFiltoSerieData_limitado()
    {
        await _benchmarkDynamoDb.GetFiltoSerieData(_valorDynamoDB, 1);
    }

    [Benchmark]
    public async Task BenchmarkFiltoSerieDynamoDb()
    {
        await _benchmarkDynamoDb.GetFiltoSerie(_valorDynamoDB);
    }

    [Benchmark]
    public async Task BenchmarkSerieFamiliaDataDynamoDb()
    {
        await _benchmarkDynamoDb.GetSerieFamiliaData(_valorDynamoDB);
    }

    [Benchmark]
    public async Task BenchmarkFiltroSerieFamiliaDataDynamoDb_limitado()
    {
        await _benchmarkDynamoDb.GetFiltroSerieFamiliaData(_valorDynamoDB, 1, 1);
    }

    public async Task BenchmarkDeleteAllDynamoDb()
    {
        await _benchmarkDynamoDb.DeleteAllItens();
    }
}
