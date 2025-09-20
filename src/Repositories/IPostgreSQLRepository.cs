using BenchmarkFinanceiro.Models;

namespace BenchmarkFinanceiro.Repositories;

public interface IPostgreSQLRepository
{
    Task InsertArquivoAsync(Arquivo arquivo);
    Task InsertFamiliaAsync(Familia familia);
    Task InsertSerieAsync(Serie serie);
    Task InsertAtributoAsync(Atributo atributo);
    Task BatchInsertValoresAsync(List<ValorPostgres> valores);
    Task<List<ValorPostgres>> GetRandomItemsAsync();
    Task<ValorPostgres?> GetValorAsync(string nomeSerie, string nomeAtributo, DateOnly dataInicioVigencia);
    Task DeleteAllAsync();
    Task<List<ValorPostgres>> GetValoresComFiltrosAsync(string? nomeFamilia = null, string? nomeSerie = null, string? nomeAtributo = null, DateOnly? dataInicioVigencia = null);
    Task<List<ValorPostgres>> GetValoresComJoinsAsync(int? codigoFamilia = null, int? codigoSerie = null, int? codigoAtributo = null, int? codigoArquivo = null);
}
