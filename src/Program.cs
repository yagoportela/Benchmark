using BenchmarkDotNet.Running;
using BenchmarkFinanceiro.Tests;

Console.WriteLine("Iniciando Benchmark...");

#if !DEBUG
    var resultado = BenchmarkRunner.Run<BenchmarkDynamoDbTest>();
#else
await CallPostgres();
await CallDynamoDb();
#endif

Console.WriteLine("Benchmark finalizado.");

static async Task CallPostgres()
{
    BenchmarkPostgresTest benchmarkPostgresTest = new();
    await benchmarkPostgresTest.SetupGlobal();
    await benchmarkPostgresTest.BenchmarkInsert();
    await benchmarkPostgresTest.BenchmarkItem();
    await benchmarkPostgresTest.BenchmarkSerieData();
    await benchmarkPostgresTest.BenchmarkAtributoData();
    await benchmarkPostgresTest.BenchmarkFamiliaData();
    await benchmarkPostgresTest.BenchmarkSerieAtributoData();
    await benchmarkPostgresTest.BenchmarkFamiliaAtributoData();
    await benchmarkPostgresTest.BenchmarkFamiliaSerieAtributoData();
    await benchmarkPostgresTest.BenchmarkDeleteAll();
}

static async Task CallDynamoDb()
{
    BenchmarkDynamoDbTest benchmarkDynamoDbTest = new();
    await benchmarkDynamoDbTest.SetupGlobal();

    //Insert
    await benchmarkDynamoDbTest.BenchmarkInsertDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkInsertBatchDynamoDb();

    //Item
    await benchmarkDynamoDbTest.BenchmarkItemDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkItemSerieAtributo();

    //Serie Atributo
    await benchmarkDynamoDbTest.BenchmarkSerieAtributoDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltoSerieAtributoData();
    await benchmarkDynamoDbTest.BenchmarkSerieAtributo();
    await benchmarkDynamoDbTest.BenchmarkFiltoSerieAtributo();

    //Familia
    await benchmarkDynamoDbTest.BenchmarkFamiliaDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltroFamiliaDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFamiliaDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltoFamiliaDynamoDb();

    //Familia Atributo
    await benchmarkDynamoDbTest.BenchmarkFiltroItemFamiliaAtributoData();
    await benchmarkDynamoDbTest.BenchmarkFiltroFamiliaAtributoData();
    await benchmarkDynamoDbTest.BenchmarkFiltroFamiliaAtributo();

    //Serie
    await benchmarkDynamoDbTest.BenchmarkSerieDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltoSerieData();
    await benchmarkDynamoDbTest.BenchmarkSerieDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltoSerieDynamoDb();

    //Serie Familia
    await benchmarkDynamoDbTest.BenchmarkSerieFamiliaDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltroSerieFamiliaDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkSerieFamiliaDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltroSerieFamiliaDynamoDb();

    // await benchmarkDynamoDbTest.BenchmarkDeleteAllDynamoDb();
}
