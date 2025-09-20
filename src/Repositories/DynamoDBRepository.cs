using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BenchmarkFinanceiro.Configuration;
using BenchmarkFinanceiro.Models;
using Microsoft.Extensions.Options;

namespace BenchmarkFinanceiro.Repositories;

public class DynamoDBRepository : IDynamoDBRepository
{
    private readonly IAmazonDynamoDB _dynamoDBClient;
    private readonly DynamoDBSettings _settings;
    private readonly string _tableName;

    public DynamoDBRepository(IAmazonDynamoDB dynamoDBClient, IOptions<AppSettings> settings)
    {
        _dynamoDBClient = dynamoDBClient;
        _settings = settings.Value.DynamoDB;
        _tableName = _settings.TableName;
    }

    public async Task InsertValorAsync(ValorDynamoDB valor)
    {
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = ConvertToDynamoDBItem(valor)
        };
        await _dynamoDBClient.PutItemAsync(request);
    }

    public async Task DeleteAllItemsAsync()
    {
        Dictionary<string, AttributeValue>? lastEvaluatedKey = null;
        int deletedCount = 0;

        do
        {
            var scanRequest = new ScanRequest
            {
                TableName = _tableName,
                ExclusiveStartKey = lastEvaluatedKey
            };

            var scanResponse = await _dynamoDBClient.ScanAsync(scanRequest);
            lastEvaluatedKey = scanResponse.LastEvaluatedKey;

            foreach (var item in scanResponse.Items)
            {
                ValorDynamoDB valorDynamoDB = ConvertFromDynamoDBItem(item);
                Dictionary<string, AttributeValue> atributoDynamoDb = ConvertToDynamoDBItem(valorDynamoDB);
                var deleteItemRequest = new DeleteItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue> {
                        { "NomeSerie", item["NomeSerie"] },
                        { "NomeAtributoDataInicioVigencia", item["NomeAtributoDataInicioVigencia"] }}
                };

                await _dynamoDBClient.DeleteItemAsync(deleteItemRequest);
                deletedCount++;
            }

        } while (lastEvaluatedKey.Count > 0);

        Console.WriteLine($"Concluído. Total de itens deletados: {deletedCount}");
    }

    public async Task<List<ValorDynamoDB>> CaptureBatchAsync(int maxItens, int limit)
    {
        Dictionary<string, AttributeValue>? lastEvaluatedKey = null;
        List<ValorDynamoDB> listaValorDynamoDb = [];

        while (true)
        {
            var scanRequest = new ScanRequest
            {
                TableName = _tableName,
                ExclusiveStartKey = lastEvaluatedKey,
                Limit = 500
            };

            var scanResponse = await _dynamoDBClient.ScanAsync(scanRequest);
            lastEvaluatedKey = scanResponse.LastEvaluatedKey;

            foreach (var item in scanResponse.Items)
            {
                ValorDynamoDB valorDynamoDB = ConvertFromDynamoDBItem(item);
                listaValorDynamoDb.Add(valorDynamoDB);
            }

            if (listaValorDynamoDb.Count >= limit || lastEvaluatedKey == null || lastEvaluatedKey.Count == 0)
            {
                break;
            }
        }

        return listaValorDynamoDb;
    }

    public async Task BatchInsertValoresAsync(List<ValorDynamoDB> valores)
    {
        var request = new BatchWriteItemRequest
        {
            RequestItems = new Dictionary<string, List<WriteRequest>>
            {
                [_tableName] = [.. valores.Select(v => new WriteRequest
                {
                    PutRequest = new PutRequest { Item = ConvertToDynamoDBItem(v) }
                })]
            }
        };
        await _dynamoDBClient.BatchWriteItemAsync(request);
    }

    public async Task<ValorDynamoDB> GetValorKeyItemAsync(string NomeSerie, string NomeAtributoDataInicioVigencia)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["NomeSerie"] = new AttributeValue { S = NomeSerie },
                ["NomeAtributoDataInicioVigencia"] = new AttributeValue { S = NomeAtributoDataInicioVigencia }
            }
        };

        var response = await _dynamoDBClient.GetItemAsync(request);
        return ConvertFromDynamoDBItem(response.Item);
    }

    public async Task<List<ValorDynamoDB>> GetValorFamiliaDataAsync(string nomeFamilia, DateOnly dataInicioVigencia)
    {
        QueryRequest queryRequest = new()
        {
            TableName = _tableName,
            IndexName = "GsiBuscaFamilia",
            KeyConditionExpression = "#dataInicioVigencia = :dataInicioVigencia and #nomeFamilia = :nomeFamilia",
            ExpressionAttributeNames = new Dictionary<String, String> {
                {"#dataInicioVigencia", "DataInicioVigencia"},
                {"#nomeFamilia", "NomeFamilia"}
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":dataInicioVigencia", new AttributeValue { S =  dataInicioVigencia.ToString("yyyyMMdd") }},
                {":nomeFamilia", new AttributeValue { S =  nomeFamilia }}
            },
            ScanIndexForward = true
        };

        QueryResponse response = await _dynamoDBClient.QueryAsync(queryRequest);
        List<ValorDynamoDB> listaValorDynamoDb = [.. response.Items.Select(x => ConvertFromDynamoDBItem(x))];
        return listaValorDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> GetValorFamiliaDataAsync(string nomeFamilia)
    {
        QueryRequest queryRequest = new()
        {
            TableName = _tableName,
            IndexName = "GsiBuscaFamilia",
            KeyConditionExpression = "#nomeFamilia = :nomeFamilia",
            ExpressionAttributeNames = new Dictionary<String, String> {
                {"#nomeFamilia", "NomeFamilia"}
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":nomeFamilia", new AttributeValue { S =  nomeFamilia }}
            },
            ScanIndexForward = true
        };

        QueryResponse response = await _dynamoDBClient.QueryAsync(queryRequest);
        List<ValorDynamoDB> listaValorDynamoDb = [.. response.Items.Select(x => ConvertFromDynamoDBItem(x))];
        return listaValorDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> GetValorSerieFamiliaAsync(string nomeSerie, string nomeFamilia, DateOnly dataInicioVigencia)
    {
        QueryRequest queryRequest = new()
        {
            TableName = _tableName,
            IndexName = "BuscarFamilia",
            KeyConditionExpression = "#NomeFamilia = :nomeFamilia and #NomeSerie = :nomeSerie",
            FilterExpression = "#dataInicioVigencia = :dataInicioVigencia",
            ExpressionAttributeNames = new Dictionary<string, string> {
                {"#NomeFamilia", "NomeFamilia"},
                {"#NomeSerie", "NomeSerie"},
                {"#dataInicioVigencia", "DataInicioVigencia"},
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":nomeFamilia", new AttributeValue { S =  nomeFamilia }},
                {":nomeSerie", new AttributeValue { S =  nomeSerie }},
                {":dataInicioVigencia", new AttributeValue { S =  dataInicioVigencia.ToString("yyyyMMdd") }}
            },
            ScanIndexForward = true
        };

        QueryResponse response = await _dynamoDBClient.QueryAsync(queryRequest);
        List<ValorDynamoDB> listaValorDynamoDb = [.. response.Items.Select(x => ConvertFromDynamoDBItem(x))];
        return listaValorDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> GetValorSerieFamiliaAsync(string nomeSerie, string nomeFamilia)
    {
        QueryRequest queryRequest = new()
        {
            TableName = _tableName,
            IndexName = "BuscarFamilia",
            KeyConditionExpression = "#NomeFamilia = :nomeFamilia and #NomeSerie = :nomeSerie",
            ExpressionAttributeNames = new Dictionary<string, string> {
                {"#NomeFamilia", "NomeFamilia"},
                {"#NomeSerie", "NomeSerie"}
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":nomeFamilia", new AttributeValue { S =  nomeFamilia }},
                {":nomeSerie", new AttributeValue { S =  nomeSerie }}
            },
            ScanIndexForward = true
        };

        QueryResponse response = await _dynamoDBClient.QueryAsync(queryRequest);
        List<ValorDynamoDB> listaValorDynamoDb = [.. response.Items.Select(x => ConvertFromDynamoDBItem(x))];
        return listaValorDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> GetValorSerieAtributoDataAsync(string nomeSerie, string nomeAtributo, DateOnly dataInicioVigencia)
    {
        QueryRequest queryRequest = new()
        {
            TableName = _tableName,
            IndexName = "BuscarAtributo",
            KeyConditionExpression = "#nomeSerie = :nomeSerie and #nomeAtributo = :nomeAtributo",
            FilterExpression = "#dataInicioVigencia = :dataInicioVigencia",
            ExpressionAttributeNames = new Dictionary<string, string> {
                {"#nomeAtributo", "NomeAtributo"},
                {"#nomeSerie", "NomeSerie"},
                {"#dataInicioVigencia", "DataInicioVigencia"}
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":nomeAtributo", new AttributeValue { S =  nomeAtributo }},
                {":nomeSerie", new AttributeValue { S =  nomeSerie }},
                {":dataInicioVigencia", new AttributeValue { S =  dataInicioVigencia.ToString("yyyyMMdd") }}
            },
            ScanIndexForward = true
        };

        QueryResponse response = await _dynamoDBClient.QueryAsync(queryRequest);
        List<ValorDynamoDB> listaValorDynamoDb = [.. response.Items.Select(x => ConvertFromDynamoDBItem(x))];
        return listaValorDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> GetValorSerieAtributoAsync(string nomeSerie, string nomeAtributo)
    {
        QueryRequest queryRequest = new()
        {
            TableName = _tableName,
            IndexName = "BuscarAtributo",
            KeyConditionExpression = "#nomeSerie = :nomeSerie and #nomeAtributo = :nomeAtributo ",
            ExpressionAttributeNames = new Dictionary<string, string> {
                {"#nomeAtributo", "NomeAtributo"},
                {"#nomeSerie", "NomeSerie"}
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":nomeAtributo", new AttributeValue { S = nomeAtributo }},
                {":nomeSerie", new AttributeValue { S =  nomeSerie }}
            },
            ScanIndexForward = true
        };

        QueryResponse response = await _dynamoDBClient.QueryAsync(queryRequest);
        List<ValorDynamoDB> listaValorDynamoDb = [.. response.Items.Select(x => ConvertFromDynamoDBItem(x))];
        return listaValorDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> GetItemSerieAtributoAsync(string nomeSerie, string nomeAtributoDataInicioVigencia)
    {
        QueryRequest queryRequest = new()
        {
            TableName = _tableName,
            KeyConditionExpression = "#nomeSerie = :nomeSerie and #nomeAtributoDataInicioVigencia = :nomeAtributoDataInicioVigencia",
            ExpressionAttributeNames = new Dictionary<string, string> {
                {"#nomeAtributoDataInicioVigencia", "NomeAtributoDataInicioVigencia"},
                {"#nomeSerie", "NomeSerie"}
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":nomeAtributoDataInicioVigencia", new AttributeValue { S =  nomeAtributoDataInicioVigencia }},
                {":nomeSerie", new AttributeValue { S =  nomeSerie }}
            },
            ScanIndexForward = true
        };

        QueryResponse response = await _dynamoDBClient.QueryAsync(queryRequest);
        List<ValorDynamoDB> listaValorDynamoDb = [.. response.Items.Select(x => ConvertFromDynamoDBItem(x))];
        return listaValorDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> GetValorSerieDataAsync(string nomeSerie, DateOnly dataInicioVigencia)
    {
        QueryRequest queryRequest = new()
        {
            TableName = _tableName,
            IndexName = "BuscarDataInicioVigencia",
            KeyConditionExpression = "#dataInicioVigencia = :dataInicioVigencia and #nomeSerie = :nomeSerie",
            ExpressionAttributeNames = new Dictionary<string, string> {
                {"#dataInicioVigencia", "DataInicioVigencia"},
                {"#nomeSerie", "NomeSerie"}
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":dataInicioVigencia", new AttributeValue { S =  dataInicioVigencia.ToString("yyyyMMdd") }},
                {":nomeSerie", new AttributeValue { S =  nomeSerie }}
            },
            ScanIndexForward = true
        };

        QueryResponse response = await _dynamoDBClient.QueryAsync(queryRequest);
        List<ValorDynamoDB> listaValorDynamoDb = [.. response.Items.Select(x => ConvertFromDynamoDBItem(x))];
        return listaValorDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> GetValorSerieDataAsync(string nomeSerie)
    {
        QueryRequest queryRequest = new()
        {
            TableName = _tableName,
            IndexName = "BuscarDataInicioVigencia",
            KeyConditionExpression = "#nomeSerie = :nomeSerie",
            ExpressionAttributeNames = new Dictionary<string, string> {
                {"#nomeSerie", "NomeSerie"}
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":nomeSerie", new AttributeValue { S =  nomeSerie }}
            },
            ScanIndexForward = true
        };

        QueryResponse response = await _dynamoDBClient.QueryAsync(queryRequest);
        List<ValorDynamoDB> listaValorDynamoDb = [.. response.Items.Select(x => ConvertFromDynamoDBItem(x))];
        return listaValorDynamoDb;
    }

    public async Task<List<ValorDynamoDB>> GetValoresComFiltrosAsync(string? nomeFamilia = null, string? nomeSerie = null, string? nomeAtributo = null, string? nomeAtributoDataInicioVigencia = null, DateOnly? dataInicioVigencia = null, DateOnly? dataFimVigencia = null)
    {
        var request = new ScanRequest
        {
            TableName = _tableName,
            FilterExpression = BuildFilterExpression(nomeFamilia, nomeSerie, nomeAtributo, nomeAtributoDataInicioVigencia, dataInicioVigencia, dataFimVigencia),
            ExpressionAttributeValues = BuildExpressionAttributeValues(nomeFamilia, nomeSerie, nomeAtributo, nomeAtributoDataInicioVigencia, dataInicioVigencia, dataFimVigencia)
        };

        var response = await _dynamoDBClient.ScanAsync(request);
        return [.. response.Items.Select(ConvertFromDynamoDBItem)];
    }

    private static Dictionary<string, AttributeValue> ConvertToDynamoDBItem(ValorDynamoDB valor)
    {
        return new Dictionary<string, AttributeValue>
        {
            ["NomeFamilia"] = new AttributeValue { S = valor.NomeFamilia },
            ["NomeSerie"] = new AttributeValue { S = valor.NomeSerie },
            ["NomeAtributoDataInicioVigencia"] = new AttributeValue { S = $"{valor.NomeAtributo}#{valor.DataInicioVigencia:yyyyMMdd}" },
            ["NomeAtributo"] = new AttributeValue { S = valor.NomeAtributo },
            ["NomeArquivo"] = new AttributeValue { S = valor.NomeArquivo },
            ["ValorAtivo"] = new AttributeValue { N = valor.ValorAtivo.ToString(CultureInfo.InvariantCulture) },
            ["DataInicioVigencia"] = new AttributeValue { S = valor.DataInicioVigencia.ToString("yyyyMMdd") },
            ["DataFimVigencia"] = new AttributeValue { S = valor.DataFimVigencia.ToString("yyyyMMdd") },
            ["DataAtualizacao"] = new AttributeValue { S = valor.DataAtualizacao.ToString("yyyy-MM-ddTHH:mm:ss") }
        };
    }

    private ValorDynamoDB ConvertFromDynamoDBItem(Dictionary<string, AttributeValue> item)
    {
        return new ValorDynamoDB
        {
            NomeFamilia = item["NomeFamilia"].S,
            NomeSerie = item["NomeSerie"].S,
            NomeAtributo = item["NomeAtributo"].S,
            NomeArquivo = item["NomeArquivo"].S,
            ValorAtivo = decimal.Parse(item["ValorAtivo"].N),
            DataInicioVigencia = DateOnly.ParseExact(item["DataInicioVigencia"].S, "yyyyMMdd", CultureInfo.InvariantCulture),
            DataFimVigencia = DateOnly.ParseExact(item["DataFimVigencia"].S, "yyyyMMdd", CultureInfo.InvariantCulture),
            DataAtualizacao = DateTime.Parse(item["DataAtualizacao"].S)
        };
    }

    private static string BuildFilterExpression(string? nomeFamilia, string? nomeSerie, string? nomeAtributo, string? nomeAtributoDataInicioVigencia, DateOnly? dataInicioVigencia = null, DateOnly? dataFimVigencia = null)
    {
        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(nomeFamilia)) conditions.Add("NomeFamilia = :nomeFamilia");
        if (!string.IsNullOrWhiteSpace(nomeAtributoDataInicioVigencia)) conditions.Add("NomeAtributoDataInicioVigencia = :nomeAtributoDataInicioVigencia");
        if (!string.IsNullOrWhiteSpace(nomeSerie)) conditions.Add("NomeSerie = :nomeSerie");
        if (!string.IsNullOrWhiteSpace(nomeAtributo)) conditions.Add("NomeAtributo = :nomeAtributo");
        if (dataInicioVigencia.HasValue) conditions.Add("DataInicioVigencia = :dataInicioVigencia");
        if (dataFimVigencia.HasValue) conditions.Add("DataFimVigencia = :dataFimVigencia");

        return conditions.Count > 0 ? string.Join(" AND ", conditions) : "";
    }

    private static Dictionary<string, AttributeValue> BuildExpressionAttributeValues(string? nomeFamilia, string? nomeSerie, string? nomeAtributo, string? nomeAtributoDataInicioVigencia, DateOnly? dataInicioVigencia = null, DateOnly? dataFimVigencia = null)
    {
        var values = new Dictionary<string, AttributeValue>();

        if (!string.IsNullOrWhiteSpace(nomeFamilia)) values[":nomeFamilia"] = new AttributeValue { S = nomeFamilia };
        if (!string.IsNullOrWhiteSpace(nomeAtributoDataInicioVigencia)) values[":nomeAtributoDataInicioVigencia"] = new AttributeValue { S = nomeAtributoDataInicioVigencia };
        if (!string.IsNullOrWhiteSpace(nomeSerie)) values[":nomeSerie"] = new AttributeValue { S = nomeSerie };
        if (!string.IsNullOrWhiteSpace(nomeAtributo)) values[":nomeAtributo"] = new AttributeValue { S = nomeAtributo };
        if (dataInicioVigencia.HasValue) values[":dataInicioVigencia"] = new AttributeValue { S = dataInicioVigencia.Value.ToString("yyyyMMdd") };
        if (dataFimVigencia.HasValue) values[":dataFimVigencia"] = new AttributeValue { S = dataFimVigencia.Value.ToString("yyyyMMdd") };

        return values;
    }
}
