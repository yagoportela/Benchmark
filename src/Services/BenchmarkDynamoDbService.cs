using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkFinanceiro.Models;
using BenchmarkFinanceiro.Repositories;
using Bogus;

namespace BenchmarkFinanceiro.Services;

[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput, launchCount: 1, warmupCount: 1, iterationCount: 1)]
public class BenchmarkDynamoDbService(IDynamoDBRepository dynamoDBRepository)
{
    public async Task<List<ValorDynamoDB>> GetBatch(int maxItens, int limit)
    {
        var valoresDynamoDb = await dynamoDBRepository.CaptureBatchAsync(maxItens, limit);
        return valoresDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> Insert(List<ValorDynamoDB> valoresDynamoDb)
    {
        Stopwatch timer = new();
        timer.Start();
        foreach (ValorDynamoDB valorDynamoDB in valoresDynamoDb)
        {
            await dynamoDBRepository.InsertValorAsync(valorDynamoDB);
        }

        timer.Stop();
        return valoresDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> InsertBatch(List<ValorDynamoDB> valoresDynamoDb)
    {
        valoresDynamoDb = [.. valoresDynamoDb.DistinctBy(x => new { x.NomeSerie, x.NomeAtributo, x.DataInicioVigencia })];
        var batches = valoresDynamoDb.Chunk(25).ToList();

        Stopwatch timer = new();
        timer.Start();
        foreach (var batch in batches)
        {
            await dynamoDBRepository.BatchInsertValoresAsync([.. batch]);
        }
        timer.Stop();
        return valoresDynamoDb;
    }

    public async Task GetItemData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        Stopwatch timer = new();
        timer.Start();

        foreach (ValorDynamoDB valorDynamoDB in valoresDynamoDb)
        {
            resultValoresDynamoDb.Add(await dynamoDBRepository.GetValorKeyItemAsync(valorDynamoDB.NomeSerie, $"{valorDynamoDB.NomeAtributo}#{valorDynamoDB.DataInicioVigencia:yyyyMMdd}"));
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Item por PK e SK", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFamiliaData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaFamilia = [.. valoresDynamoDb.Select(x => x.NomeFamilia).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresDynamoDb.Select(x => x.DataInicioVigencia).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (string familia in listaFamilia)
        {
            foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
            {
                resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValorFamiliaDataAsync(familia, dataInicioVigencia));
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Querie por familia e data", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltroFamiliaData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaFamilia = [.. valoresDynamoDb.Select(x => x.NomeFamilia).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresDynamoDb.Select(x => x.DataInicioVigencia).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (string familia in listaFamilia)
        {
            foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
            {
                resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeFamilia: familia, dataInicioVigencia: dataInicioVigencia));
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan por familia e data", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFamilia(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaFamilia = [.. valoresDynamoDb.Select(x => x.NomeFamilia).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (string familia in listaFamilia)
        {
            resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValorFamiliaDataAsync(familia));
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Querir por familia", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetSerieFamiliaData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaFamilia = [.. valoresDynamoDb.Select(x => x.NomeFamilia).Distinct()];
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresDynamoDb.Select(x => x.DataInicioVigencia).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
        {
            foreach (string familia in listaFamilia)
            {
                foreach (string serie in listaSerie)
                {
                    resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValorSerieFamiliaAsync(serie, familia, dataInicioVigencia));
                }
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Querie por serie, familia e data", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltroSerieFamiliaData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaFamilia = [.. valoresDynamoDb.Select(x => x.NomeFamilia).Distinct()];
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresDynamoDb.Select(x => x.DataInicioVigencia).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
        {
            foreach (string familia in listaFamilia)
            {
                foreach (string serie in listaSerie)
                {
                    resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeSerie: serie, nomeFamilia: familia, dataInicioVigencia: dataInicioVigencia));
                }
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan por serie, familia e data", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetSerieFamilia(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaFamilia = [.. valoresDynamoDb.Select(x => x.NomeFamilia).Distinct()];
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (string familia in listaFamilia)
        {
            foreach (string serie in listaSerie)
            {
                resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValorSerieFamiliaAsync(serie, familia));
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Querie por serie e familia", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltroSerieFamilia(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaFamilia = [.. valoresDynamoDb.Select(x => x.NomeFamilia).Distinct()];
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (string familia in listaFamilia)
        {
            foreach (string serie in listaSerie)
            {
                resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeSerie: serie, nomeFamilia: familia));
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan por serie e familia", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetSerieAtributoData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];

        Stopwatch timer = new();
        timer.Start();
        foreach (ValorDynamoDB valorDynamoDB in valoresDynamoDb)
        {
            resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValorSerieAtributoDataAsync(valorDynamoDB.NomeSerie, valorDynamoDB.NomeAtributo, valorDynamoDB.DataInicioVigencia));
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Querie por serie, atributo e data", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetItemSerieAtributo(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];

        Stopwatch timer = new();
        timer.Start();
        foreach (ValorDynamoDB valorDynamoDB in valoresDynamoDb)
        {
            resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetItemSerieAtributoAsync(valorDynamoDB.NomeSerie, $"{valorDynamoDB.NomeAtributo}#{valorDynamoDB.DataInicioVigencia:yyyyMMdd}"));
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Querie item por serie e atributo", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetSerieAtributo(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];

        Stopwatch timer = new();
        timer.Start();
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];
        List<string> listaAtributos = [.. valoresDynamoDb.Select(x => x.NomeAtributo).Distinct()];

        foreach (string serie in listaSerie)
        {
            foreach (string atributo in listaAtributos)
            {
                resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValorSerieAtributoAsync(serie, atributo));
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Querie por serie e atributo", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetSerieData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<DateOnly> listaDataInicioVigencia = [.. valoresDynamoDb.Select(x => x.DataInicioVigencia).Distinct()];
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
        {
            foreach (string serie in listaSerie)
            {
                resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValorSerieDataAsync(serie, dataInicioVigencia));
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Querie por serie e data", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetSerie(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (string serie in listaSerie)
        {
            resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValorSerieDataAsync(serie));
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Query Por serie", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltoFamilia(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaFamilia = [.. valoresDynamoDb.Select(x => x.NomeFamilia).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresDynamoDb.Select(x => x.DataInicioVigencia).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
        {
            foreach (string familia in listaFamilia)
            {
                resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeFamilia: familia, dataInicioVigencia: dataInicioVigencia));
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan por familia e data", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltoSerie(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresDynamoDb.Select(x => x.DataInicioVigencia).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
        {
            foreach (string serie in listaSerie)
            {
                resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeSerie: serie, dataInicioVigencia: dataInicioVigencia));
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan por serie e data", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltoSerieAtributoData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresDynamoDb.Select(x => x.DataInicioVigencia).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (ValorDynamoDB valorDynamoDb in valoresDynamoDb)
        {
            resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeSerie: valorDynamoDb.NomeSerie, nomeAtributo: valorDynamoDb.NomeAtributo, dataInicioVigencia: valorDynamoDb.DataInicioVigencia));
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan por serie, atributo e data apenas PK", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltoSerieAtributo(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];
        List<string> listaAtributos = [.. valoresDynamoDb.Select(x => x.NomeAtributo).Distinct()];

        Stopwatch timer = new();
        timer.Start();

        foreach (string serie in listaSerie)
        {
            foreach (string atributo in listaAtributos)
            {
                resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeSerie: serie, nomeAtributo: atributo));
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan por serie e atributo", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltroItemFamiliaAtributoData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];

        Stopwatch timer = new();
        timer.Start();
        foreach (ValorDynamoDB valorDynamoDb in valoresDynamoDb)
        {
            resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeFamilia: valorDynamoDb.NomeFamilia, nomeAtributoDataInicioVigencia: $"{valorDynamoDb.NomeAtributo}#{valorDynamoDb.DataInicioVigencia:yyyyMMdd}"));
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan item por familia, atributo e data", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltroFamiliaAtributoData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];

        Stopwatch timer = new();
        timer.Start();
        foreach (ValorDynamoDB valorDynamoDb in valoresDynamoDb)
        {
            resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeFamilia: valorDynamoDb.NomeFamilia, nomeAtributo: valorDynamoDb.NomeAtributo, dataInicioVigencia: valorDynamoDb.DataInicioVigencia));
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan por familia, atributo e data", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltroFamiliaAtributo(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];

        Stopwatch timer = new();
        timer.Start();
        foreach (ValorDynamoDB valorDynamoDb in valoresDynamoDb)
        {
            resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeFamilia: valorDynamoDb.NomeFamilia, nomeAtributo: valorDynamoDb.NomeAtributo));
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan por familia e atributo", resultValoresDynamoDb, elapsedTime);
    }

    public async Task GetFiltoSerieData(List<ValorDynamoDB> valoresDynamoDb)
    {
        List<ValorDynamoDB> resultValoresDynamoDb = [];
        List<string> listaSerie = [.. valoresDynamoDb.Select(x => x.NomeSerie).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresDynamoDb.Select(x => x.DataInicioVigencia).Distinct()];

        Stopwatch timer = new();
        timer.Start();
        foreach (ValorDynamoDB valorDynamoDb in valoresDynamoDb)
        {
            resultValoresDynamoDb.AddRange(await dynamoDBRepository.GetValoresComFiltrosAsync(nomeSerie: valorDynamoDb.NomeSerie, nomeAtributoDataInicioVigencia: $"{valorDynamoDb.NomeAtributo}#{valorDynamoDb.DataInicioVigencia:yyyyMMdd}"));
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Scan por serie e data PK e SK", resultValoresDynamoDb, elapsedTime);
    }

    public static List<ValorDynamoDB> CriarDados(int qtdFamilia, int qtdSerie, int qtdAtributo, int qtdDias)
    {
        Faker faker = new();
        List<ValorDynamoDB> valoresDynamoDb = [];

        for (int i = 0; i < qtdFamilia; i++)
        {
            string nomeFamilia = faker.Commerce.Random.String2(5);
            string nomeArquivo = faker.System.FileName();
            for (int j = 0; j < qtdSerie; j++)
            {
                string nomeSerie = faker.Commerce.ProductAdjective();
                for (int k = 0; k < qtdAtributo; k++)
                {
                    DateOnly dataVigencia = DateOnly.FromDateTime(DateTime.UtcNow);
                    for (int d = 0; d < qtdDias; d++)
                    {
                        dataVigencia = dataVigencia.AddDays(-1);
                        valoresDynamoDb.Add(new ValorDynamoDB
                        {
                            NomeFamilia = nomeFamilia,
                            NomeSerie = nomeSerie,
                            NomeAtributo = faker.Commerce.Product(),
                            NomeArquivo = nomeArquivo,
                            ValorAtivo = faker.Random.Decimal(0, 100),
                            DataInicioVigencia = dataVigencia,
                            DataFimVigencia = dataVigencia,
                            DataAtualizacao = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        return valoresDynamoDb;
    }

    private static void ImprimirResultado(string title, List<ValorDynamoDB> resultValoresDynamoDb, string timer)
    {
        Console.WriteLine();
        Console.WriteLine($"*******************{title}*******************");
        if (resultValoresDynamoDb.Count == 0)
        {
            Console.WriteLine($"Erro GetItemData: {resultValoresDynamoDb.Count}");
        }
        else
        {
            Console.WriteLine($"Sucesso GetItemData: {resultValoresDynamoDb.Count}");
        }

        Console.WriteLine($"Tempo de execucao: {timer}");
        Console.WriteLine();
    }

    public async Task DeleteAllItens()
    {
        await dynamoDBRepository.DeleteAllItemsAsync();
    }
}
