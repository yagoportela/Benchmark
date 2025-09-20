using BenchmarkDotNet.Running;
using BenchmarkFinanceiro.Tests;

Console.WriteLine("Iniciando Benchmark...");

#if !DEBUG
    BenchmarkRunner.Run<BenchmarkDynamoDbTest>();
    BenchmarkRunner.Run<BenchmarkPostgresTest>();
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

    //Serie Atributo
    await benchmarkDynamoDbTest.BenchmarkSerieAtributoDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltoSerieAtributoData_limitado();

    //Familia
    await benchmarkDynamoDbTest.BenchmarkFamiliaDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltroFamiliaDataDynamoDb_limitado();

    //Familia Atributo
    await benchmarkDynamoDbTest.BenchmarkFiltroItemFamiliaAtributoData_limitado();
    await benchmarkDynamoDbTest.BenchmarkFiltroFamiliaAtributoData_limitado();

    //Serie
    await benchmarkDynamoDbTest.BenchmarkSerieDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltoSerieData_limitado();
    await benchmarkDynamoDbTest.BenchmarkFiltoSerieDynamoDb();

    //Serie Familia
    await benchmarkDynamoDbTest.BenchmarkSerieFamiliaDataDynamoDb();
    await benchmarkDynamoDbTest.BenchmarkFiltroSerieFamiliaDataDynamoDb_limitado();

    // await benchmarkDynamoDbTest.BenchmarkDeleteAllDynamoDb();
}
