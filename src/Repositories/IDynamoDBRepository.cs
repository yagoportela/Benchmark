using BenchmarkFinanceiro.Models;

namespace BenchmarkFinanceiro.Repositories;

public interface IDynamoDBRepository
{
    Task<List<ValorDynamoDB>> CaptureBatchAsync(int maxItens, int limit);
    Task InsertValorAsync(ValorDynamoDB valor);
    Task DeleteAllItemsAsync();
    Task BatchInsertValoresAsync(List<ValorDynamoDB> valores);
    Task<ValorDynamoDB> GetValorKeyItemAsync(string NomeSerie, string NomeAtributoDataInicioVigencia);
    Task<List<ValorDynamoDB>> GetValorFamiliaDataAsync(string nomeFamilia, DateOnly dataInicioVigencia);
    Task<List<ValorDynamoDB>> GetValorSerieFamiliaAsync(string nomeSerie, string nomeFamilia, DateOnly dataInicioVigencia);
    Task<List<ValorDynamoDB>> GetValorSerieAtributoDataAsync(string nomeSerie, string nomeAtributo, DateOnly dataInicioVigencia);
    Task<List<ValorDynamoDB>> GetValorSerieDataAsync(string nomeSerie, DateOnly dataInicioVigencia);
    Task<List<ValorDynamoDB>> GetValoresComFiltrosAsync(string? nomeFamilia = null, string? nomeSerie = null, string? nomeAtributo = null, string? nomeAtributoDataInicioVigencia = null, DateOnly? dataInicioVigencia = null, DateOnly? dataFimVigencia = null);
}
