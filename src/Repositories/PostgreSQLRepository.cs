using BenchmarkFinanceiro.Configuration;
using BenchmarkFinanceiro.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BenchmarkFinanceiro.Repositories;

public class PostgreSQLRepository : IPostgreSQLRepository
{
    private readonly string _connectionString;
    private readonly DatabaseSettings _settings;

    public PostgreSQLRepository(IOptions<AppSettings> settings)
    {
        _settings = settings.Value.Database;
        _connectionString = _settings.ConnectionString;
    }

    public async Task InsertArquivoAsync(Arquivo arquivo)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "INSERT INTO \"Arquivo\" (\"NomeArquivo\", \"DescricaoArquivo\", \"NomeInternoArquivo\") VALUES (@nomearquivo, @descricaoarquivo, @nomeinternoarquivo) ON CONFLICT (\"NomeArquivo\") DO NOTHING";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nomearquivo", arquivo.NomeArquivo);
        command.Parameters.AddWithValue("@descricaoarquivo", arquivo.DescricaoArquivo);
        command.Parameters.AddWithValue("@nomeinternoarquivo", arquivo.NomeInternoArquivo);
        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertFamiliaAsync(Familia familia)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "INSERT INTO \"Familia\" (\"NomeFamilia\", \"DescricaoFamilia\", \"NomeInternoFamilia\") VALUES (@nomeFamilia, @descricaofamilia, @nomeinternofamilia) ON CONFLICT (\"NomeFamilia\") DO NOTHING";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nomefamilia", familia.NomeFamilia);
        command.Parameters.AddWithValue("@descricaofamilia", familia.DescricaoFamilia);
        command.Parameters.AddWithValue("@nomeinternofamilia", familia.NomeInternoFamilia);
        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertSerieAsync(Serie serie)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "INSERT INTO \"Serie\" (\"NomeSerie\", \"DescricaoSerie\", \"NomeInternoSerie\") VALUES (@nomeserie, @descricaoserie, @nomeinternoserie) ON CONFLICT (\"NomeSerie\") DO NOTHING";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nomeserie", serie.NomeSerie);
        command.Parameters.AddWithValue("@descricaoserie", serie.DescricaoSerie);
        command.Parameters.AddWithValue("@nomeinternoserie", serie.NomeInternoSerie);
        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertAtributoAsync(Atributo atributo)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "INSERT INTO \"Atributo\" (\"NomeAtributo\", \"DescricaoAtributo\", \"NomeInternoAtributo\") VALUES (@nomeatributo, @descricaoatributo, @nomeinternoatributo) ON CONFLICT (\"NomeAtributo\") DO NOTHING";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nomeatributo", atributo.NomeAtributo);
        command.Parameters.AddWithValue("@descricaoatributo", atributo.DescricaoAtributo);
        command.Parameters.AddWithValue("@nomeinternoatributo", atributo.NomeInternoAtributo);
        await command.ExecuteNonQueryAsync();
    }

public async Task BatchInsertValoresAsync(List<ValorPostgres> valores)
{
    using var connection = new NpgsqlConnection(_connectionString);
    await connection.OpenAsync();

    // A capitalização de EXCLUDED."Valor" e EXCLUDED."DataAtualizacao" foi corrigida
    var sql = "INSERT INTO \"Valor\" (\"NomeFamilia\", \"NomeSerie\", \"NomeAtributo\", \"NomeArquivo\", \"Valor\", \"DataInicioVigencia\", \"DataFimVigencia\", \"DataAtualizacao\") VALUES (@nomefamilia, @nomeserie, @nomeatributo, @nomearquivo, @valor, @datainiciovigencia, @datafimvigencia, @dataatualizacao) ON CONFLICT (\"NomeSerie\", \"NomeAtributo\", \"DataAtualizacao\") DO UPDATE SET \"Valor\" = EXCLUDED.\"Valor\", \"DataAtualizacao\" = EXCLUDED.\"DataAtualizacao\"";

    using var command = new NpgsqlCommand(sql, connection);
    command.Parameters.Add("@nomefamilia", NpgsqlTypes.NpgsqlDbType.Varchar);
    command.Parameters.Add("@nomeserie", NpgsqlTypes.NpgsqlDbType.Varchar);
    command.Parameters.Add("@nomeatributo", NpgsqlTypes.NpgsqlDbType.Varchar);
    command.Parameters.Add("@nomearquivo", NpgsqlTypes.NpgsqlDbType.Varchar);
    command.Parameters.Add("@valor", NpgsqlTypes.NpgsqlDbType.Numeric);
    command.Parameters.Add("@datainiciovigencia", NpgsqlTypes.NpgsqlDbType.Date);
    command.Parameters.Add("@datafimvigencia", NpgsqlTypes.NpgsqlDbType.Date);
    command.Parameters.Add("@dataatualizacao", NpgsqlTypes.NpgsqlDbType.TimestampTz);

    foreach (var valor in valores)
    {
        command.Parameters["@nomefamilia"].Value = valor.NomeFamilia;
        command.Parameters["@nomeserie"].Value = valor.NomeSerie;
        command.Parameters["@nomeatributo"].Value = valor.NomeAtributo;
        command.Parameters["@nomearquivo"].Value = valor.NomeArquivo;
        command.Parameters["@valor"].Value = valor.ValorAtivo;
        command.Parameters["@datainiciovigencia"].Value = valor.DataInicioVigencia;
        command.Parameters["@datafimvigencia"].Value = valor.DataFimVigencia;
        command.Parameters["@dataatualizacao"].Value = valor.DataAtualizacao;
        await command.ExecuteNonQueryAsync();
    }
}

    public async Task<List<ValorPostgres>> GetRandomItemsAsync()
    {
        string sql = "SELECT * FROM \"Valor\" ORDER BY RANDOM() LIMIT 50";

        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        var valores = new List<ValorPostgres>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            valores.Add(new ValorPostgres
            {
                NomeFamilia = reader.GetString(reader.GetOrdinal("NomeFamilia")),
                NomeSerie = reader.GetString(reader.GetOrdinal("NomeSerie")),
                NomeAtributo = reader.GetString(reader.GetOrdinal("NomeAtributo")),
                NomeArquivo = reader.GetString(reader.GetOrdinal("NomeArquivo")),
                DataInicioVigencia = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataInicioVigencia"))),
                DataFimVigencia = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataFimVigencia"))),
                ValorAtivo = reader.GetDouble(reader.GetOrdinal("Valor")),
                DataAtualizacao = reader.GetDateTime(reader.GetOrdinal("DataAtualizacao"))
            });
        }
        return valores;
    }

    public async Task<ValorPostgres?> GetValorAsync(string nomeSerie, string nomeAtributo, DateOnly dataInicioVigencia)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT \"NomeFamilia\", \"NomeSerie\", \"NomeAtributo\", \"NomeArquivo\", \"Valor\", \"DataInicioVigencia\", \"DataFimVigencia\", \"DataAtualizacao\" FROM \"Valor\" WHERE \"NomeSerie\" = @nomeSerie AND \"NomeAtributo\" = @nomeAtributo AND \"DataInicioVigencia\" = @dataInicioVigencia";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nomeSerie", nomeSerie);
        command.Parameters.AddWithValue("@nomeAtributo", nomeAtributo);
        command.Parameters.AddWithValue("@dataInicioVigencia", dataInicioVigencia);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ValorPostgres
            {
                NomeFamilia = reader.GetString(reader.GetOrdinal("NomeFamilia")),
                NomeSerie = reader.GetString(reader.GetOrdinal("NomeSerie")),
                NomeAtributo = reader.GetString(reader.GetOrdinal("NomeAtributo")),
                NomeArquivo = reader.GetString(reader.GetOrdinal("NomeArquivo")),
                DataInicioVigencia = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataInicioVigencia"))),
                DataFimVigencia = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataFimVigencia"))),
                ValorAtivo = reader.GetDouble(reader.GetOrdinal("Valor")),
                DataAtualizacao = reader.GetDateTime(reader.GetOrdinal("DataAtualizacao"))
            };
        }
        return null;
    }

    public async Task DeleteAllAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"TRUNCATE TABLE valor";

        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public async Task<List<ValorPostgres>> GetValoresComFiltrosAsync(string? nomeFamilia = null, string? nomeSerie = null, string? nomeAtributo = null, DateOnly? dataInicioVigencia = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var conditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        if (!string.IsNullOrWhiteSpace(nomeFamilia))
        {
            conditions.Add("\"NomeFamilia\" = @nomeFamilia");
            parameters.Add(new NpgsqlParameter("@nomeFamilia", nomeFamilia));
        }
        if (!string.IsNullOrWhiteSpace(nomeSerie))
        {
            conditions.Add("\"NomeSerie\" = @nomeSerie");
            parameters.Add(new NpgsqlParameter("@nomeSerie", nomeSerie));
        }
        if (!string.IsNullOrWhiteSpace(nomeAtributo))
        {
            conditions.Add("\"NomeAtributo\" = @nomeAtributo");
            parameters.Add(new NpgsqlParameter("@nomeAtributo", nomeAtributo));
        }
        if (dataInicioVigencia.HasValue)
        {
            conditions.Add("\"DataInicioVigencia\" = @dataInicioVigencia");
            parameters.Add(new NpgsqlParameter("@dataInicioVigencia", dataInicioVigencia.Value));
        }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
        var sql = $"SELECT \"NomeFamilia\", \"NomeSerie\", \"NomeAtributo\", \"NomeArquivo\", \"Valor\", \"DataInicioVigencia\", \"DataFimVigencia\", \"DataAtualizacao\" FROM \"Valor\" {whereClause}";

        using var command = new NpgsqlCommand(sql, connection);
        foreach (var param in parameters)
        {
            command.Parameters.Add(param);
        }

        var valores = new List<ValorPostgres>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            valores.Add(new ValorPostgres
            {
                NomeFamilia = reader.GetString(reader.GetOrdinal("NomeFamilia")),
                NomeSerie = reader.GetString(reader.GetOrdinal("NomeSerie")),
                NomeAtributo = reader.GetString(reader.GetOrdinal("NomeAtributo")),
                NomeArquivo = reader.GetString(reader.GetOrdinal("NomeArquivo")),
                DataInicioVigencia = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataInicioVigencia"))),
                DataFimVigencia = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataFimVigencia"))),
                ValorAtivo = reader.GetDouble(reader.GetOrdinal("Valor")),
                DataAtualizacao = reader.GetDateTime(reader.GetOrdinal("DataAtualizacao"))
            });
        }
        return valores;
    }

    public async Task<List<ValorPostgres>> GetValoresComJoinsAsync(int? codigoFamilia = null, int? codigoSerie = null, int? codigoAtributo = null, int? codigoArquivo = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var conditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        if (codigoFamilia.HasValue)
        {
            conditions.Add("v.NomeFamilia = @familia");
            parameters.Add(new NpgsqlParameter("@familia", codigoFamilia.Value));
        }
        if (codigoSerie.HasValue)
        {
            conditions.Add("v.NomeSerie = @serie");
            parameters.Add(new NpgsqlParameter("@serie", codigoSerie.Value));
        }
        if (codigoAtributo.HasValue)
        {
            conditions.Add("v.NomeAtributo = @atributo");
            parameters.Add(new NpgsqlParameter("@atributo", codigoAtributo.Value));
        }
        if (codigoArquivo.HasValue)
        {
            conditions.Add("v.NomeArquivo = @arquivo");
            parameters.Add(new NpgsqlParameter("@arquivo", codigoArquivo.Value));
        }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
        var sql = $@"
            SELECT v.NomeFamilia, v.NomeSerie, v.NomeAtributo, v.NomeArquivo, v.Valor, v.DataAtualizacao,
                   f.nome_familia, s.nome_serie, a.nome_atributo, ar.nome_arquivo, p.nome_provedor
            FROM Valor v
            INNER JOIN familia f ON v.NomeFamilia = f.NomeFamilia
            INNER JOIN serie s ON v.NomeSerie = s.NomeSerie
            INNER JOIN atributo a ON v.NomeAtributo = a.NomeAtributo
            INNER JOIN arquivo ar ON v.NomeArquivo = ar.NomeArquivo
            INNER JOIN provedor p ON ar.codigo_provedor = p.codigo_provedor
            {whereClause}";

        using var command = new NpgsqlCommand(sql, connection);
        foreach (var param in parameters)
        {
            command.Parameters.Add(param);
        }

        var valores = new List<ValorPostgres>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            valores.Add(new ValorPostgres
            {
                NomeFamilia = reader.GetString(reader.GetOrdinal("NomeFamilia")),
                NomeSerie = reader.GetString(reader.GetOrdinal("NomeSerie")),
                NomeAtributo = reader.GetString(reader.GetOrdinal("NomeAtributo")),
                NomeArquivo = reader.GetString(reader.GetOrdinal("NomeArquivo")),
                DataInicioVigencia = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataInicioVigencia"))),
                DataFimVigencia = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DataFimVigencia"))),
                ValorAtivo = reader.GetDouble(reader.GetOrdinal("Valor")),
                DataAtualizacao = reader.GetDateTime(reader.GetOrdinal("DataAtualizacao"))
            });
        }
        return valores;
    }
}
