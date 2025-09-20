using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkFinanceiro.Models;
using BenchmarkFinanceiro.Repositories;
using Bogus;

namespace BenchmarkFinanceiro.Services;

[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput, launchCount: 1, warmupCount: 1, iterationCount: 1)]
public class BenchmarkPostgresService(IPostgreSQLRepository postgreSQLRepository)
{
    public async Task Insert(List<AggregateValorPostgres> valoresPostgres)
    {
        Stopwatch timer = new();
        timer.Start();
        List<Arquivo> listaArquivos = [..valoresPostgres.Select(x => x.Arquivo).DistinctBy(x => x.NomeArquivo)];
        List<Familia> listaFamilias = [..valoresPostgres.Select(x => x.Familia).DistinctBy(x => x.NomeFamilia)];
        List<Serie> listaSeries = [..valoresPostgres.Select(x => x.Serie).DistinctBy(x => x.NomeSerie)];
        List<Atributo> listaAtributos = [..valoresPostgres.Select(x => x.Atributo).DistinctBy(x => x.NomeAtributo)];

        foreach (Arquivo arquivo in listaArquivos)
        {
            await postgreSQLRepository.InsertArquivoAsync(arquivo);
        }
        foreach (Familia familia in listaFamilias)
        {
            await postgreSQLRepository.InsertFamiliaAsync(familia);
        }
        foreach (Serie serie in listaSeries)
        {
            await postgreSQLRepository.InsertSerieAsync(serie);
        }
        foreach (Atributo atributo in listaAtributos)
        {
            await postgreSQLRepository.InsertAtributoAsync(atributo);
        }
        foreach (AggregateValorPostgres valorPostgres in valoresPostgres)
        {
            await postgreSQLRepository.BatchInsertValoresAsync([valorPostgres.Valor]);
        }

        timer.Stop();
    }

    public async Task<List<ValorPostgres>> GetSerieItensRandomList()
    {
        Stopwatch timer = new();
        timer.Start();

        List<ValorPostgres> resultValoresPostgres = await postgreSQLRepository.GetRandomItemsAsync();

        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Obter random", resultValoresPostgres, elapsedTime);
        return resultValoresPostgres;
    }

    public async Task GetSerieItemSerieAtributoData(List<AggregateValorPostgres> valoresPostgres)
    {
        List<ValorPostgres> resultValoresPostgres = [];
        Stopwatch timer = new();
        timer.Start();

        foreach (AggregateValorPostgres aggregateValorPostgres in valoresPostgres)
        {
            ValorPostgres valorPostgres = aggregateValorPostgres.Valor;
            ValorPostgres? result = await postgreSQLRepository.GetValorAsync(valorPostgres.NomeSerie, valorPostgres.NomeAtributo, valorPostgres.DataInicioVigencia);

            if (result != null)
                resultValoresPostgres.Add(result);
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Obter item serie atributo e data", resultValoresPostgres, elapsedTime);
    }

    public async Task GetSerieAtributoData(List<AggregateValorPostgres> valoresPostgres)
    {
        List<ValorPostgres> resultValoresPostgres = [];
        Stopwatch timer = new();
        timer.Start();

        foreach (AggregateValorPostgres aggregateValorPostgres in valoresPostgres)
        {
            ValorPostgres valorPostgres = aggregateValorPostgres.Valor;
            List<ValorPostgres>? result = await postgreSQLRepository.GetValoresComFiltrosAsync(nomeSerie: valorPostgres.NomeSerie, nomeAtributo: valorPostgres.NomeAtributo, dataInicioVigencia: valorPostgres.DataInicioVigencia);

            if (result != null)
                resultValoresPostgres.AddRange(result);
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Obter serie atributo e data", resultValoresPostgres, elapsedTime);
    }

    public async Task GetSerieData(List<AggregateValorPostgres> valoresPostgres)
    {
        List<ValorPostgres> resultValoresPostgres = [];
        Stopwatch timer = new();
        timer.Start();

        List<string> listaSeries = [.. valoresPostgres.Select(x => x.Valor.NomeSerie).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresPostgres.Select(x => x.Valor.DataInicioVigencia).Distinct()];

        foreach (string series in listaSeries)
        {
            foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
            {
                List<ValorPostgres>? result = await postgreSQLRepository.GetValoresComFiltrosAsync(nomeSerie: series, dataInicioVigencia: dataInicioVigencia);

                if (result != null)
                    resultValoresPostgres.AddRange(result);
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Obter serie e data", resultValoresPostgres, elapsedTime);
    }

    public async Task GetAtributoData(List<AggregateValorPostgres> valoresPostgres)
    {
        List<ValorPostgres> resultValoresPostgres = [];
        Stopwatch timer = new();
        timer.Start();

        List<string> listaAtributo = [.. valoresPostgres.Select(x => x.Valor.NomeAtributo).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresPostgres.Select(x => x.Valor.DataInicioVigencia).Distinct()];

        foreach (string atributo in listaAtributo)
        {
            foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
            {
                List<ValorPostgres>? result = await postgreSQLRepository.GetValoresComFiltrosAsync(nomeAtributo: atributo, dataInicioVigencia: dataInicioVigencia);

                if (result != null)
                    resultValoresPostgres.AddRange(result);
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Obter atributo e data", resultValoresPostgres, elapsedTime);
    }

    public async Task GetFamiliaData(List<AggregateValorPostgres> valoresPostgres)
    {
        List<ValorPostgres> resultValoresPostgres = [];
        Stopwatch timer = new();
        timer.Start();

        List<string> listaFamilia = [.. valoresPostgres.Select(x => x.Valor.NomeFamilia).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresPostgres.Select(x => x.Valor.DataInicioVigencia).Distinct()];

        foreach (string familia in listaFamilia)
        {
            foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
            {
                List<ValorPostgres>? result = await postgreSQLRepository.GetValoresComFiltrosAsync(nomeFamilia: familia, dataInicioVigencia: dataInicioVigencia);

                if (result != null)
                    resultValoresPostgres.AddRange(result);
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Obter familia e data", resultValoresPostgres, elapsedTime);
    }

    public async Task GetFamiliaAtributoData(List<AggregateValorPostgres> valoresPostgres)
    {
        List<ValorPostgres> resultValoresPostgres = [];
        Stopwatch timer = new();
        timer.Start();

        List<string> listaFamilia = [.. valoresPostgres.Select(x => x.Valor.NomeFamilia).Distinct()];
        List<string> listaAtributo = [.. valoresPostgres.Select(x => x.Valor.NomeAtributo).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresPostgres.Select(x => x.Valor.DataInicioVigencia).Distinct()];

        foreach (string familia in listaFamilia)
        {
            foreach (string atributo in listaAtributo)
            {
                foreach (DateOnly dataInicioVigencia in listaDataInicioVigencia)
                {
                    List<ValorPostgres>? result = await postgreSQLRepository.GetValoresComFiltrosAsync(nomeFamilia: familia, nomeAtributo: atributo, dataInicioVigencia: dataInicioVigencia);

                    if (result != null)
                        resultValoresPostgres.AddRange(result);
                }
            }
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Obter familia atributo e data", resultValoresPostgres, elapsedTime);
    }

    public async Task GetFamiliaSerieAtributoData(List<AggregateValorPostgres> valoresPostgres)
    {
        List<ValorPostgres> resultValoresPostgres = [];
        Stopwatch timer = new();
        timer.Start();

        List<string> listaFamilia = [.. valoresPostgres.Select(x => x.Valor.NomeFamilia).Distinct()];
        List<string> listaAtributo = [.. valoresPostgres.Select(x => x.Valor.NomeAtributo).Distinct()];
        List<DateOnly> listaDataInicioVigencia = [.. valoresPostgres.Select(x => x.Valor.DataInicioVigencia).Distinct()];

        foreach (AggregateValorPostgres aggregateValorPostgres in valoresPostgres)
        {
            ValorPostgres valorPostgres = aggregateValorPostgres.Valor;
            List<ValorPostgres>? result = await postgreSQLRepository.GetValoresComFiltrosAsync(nomeFamilia: valorPostgres.NomeFamilia, nomeSerie: valorPostgres.NomeSerie, nomeAtributo: valorPostgres.NomeAtributo, dataInicioVigencia: valorPostgres.DataInicioVigencia);

            if (result != null)
                resultValoresPostgres.AddRange(result);
        }
        timer.Stop();
        TimeSpan ts = timer.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        ImprimirResultado("Obter familia, serie atributo e data", resultValoresPostgres, elapsedTime);
    }

    public static List<AggregateValorPostgres> CriarDados(int qtdFamilia, int qtdSerie, int qtdAtributo, int qtdDias)
    {
        Faker faker = new();
        List<AggregateValorPostgres> aggregateValoresPostrgres = [];

        for (int i = 0; i < qtdFamilia; i++)
        {
            string nomeFamilia = faker.Commerce.Random.String2(5);
            string nomeArquivo = faker.System.FileName();
            Familia familia = new()
            {
                NomeFamilia = nomeFamilia,
                DescricaoFamilia = faker.Commerce.Random.Words(10),
                NomeInternoFamilia = nomeFamilia
            };
            Arquivo arquivo = new()
            {
                NomeArquivo = nomeArquivo,
                DescricaoArquivo = faker.Commerce.Random.Words(10),
                NomeInternoArquivo = nomeArquivo
            };
            for (int j = 0; j < qtdSerie; j++)
            {
                string nomeSerie = faker.Commerce.ProductAdjective();
                Serie serie = new()
                {
                    NomeSerie = nomeSerie,
                    DescricaoSerie = faker.Commerce.Random.Words(10),
                    NomeInternoSerie = nomeSerie
                };

                for (int k = 0; k < qtdAtributo; k++)
                {
                    string nomeAtributo = faker.Commerce.Product();
                    Atributo atributo = new()
                    {
                        NomeAtributo = nomeAtributo,
                        DescricaoAtributo = faker.Commerce.Random.Words(10),
                        NomeInternoAtributo = nomeAtributo
                    };

                    DateOnly dataVigencia = DateOnly.FromDateTime(DateTime.UtcNow);
                    for (int d = 0; d < qtdDias; d++)
                    {
                        dataVigencia = dataVigencia.AddDays(-1);

                        aggregateValoresPostrgres.Add(new AggregateValorPostgres
                        {
                            Familia = familia,
                            Arquivo = arquivo,
                            Serie = serie,
                            Atributo = atributo,
                            Valor = new()
                            {
                                NomeFamilia = familia.NomeFamilia,
                                NomeArquivo = arquivo.NomeArquivo,
                                NomeSerie = serie.NomeSerie,
                                NomeAtributo = atributo.NomeAtributo,
                                ValorAtivo = faker.Random.Double(0, 100),
                                DataInicioVigencia = dataVigencia,
                                DataFimVigencia = dataVigencia,
                                DataAtualizacao = DateTime.UtcNow
                            }
                        });
                    }
                }
            }
        }

        return aggregateValoresPostrgres;
    }

    private static void ImprimirResultado(string title, List<ValorPostgres> resultValoresDynamoDb, string timer)
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
        await postgreSQLRepository.DeleteAllAsync();
    }
}
