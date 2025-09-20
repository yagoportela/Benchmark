namespace BenchmarkFinanceiro.Models;

public class ValorDynamoDB
{
    public string NomeFamilia { get; set; } = null!;
    public string NomeSerie { get; set; } = null!;
    public string NomeAtributo { get; set; } = null!;
    public string? NomeArquivo { get; set; }
    public decimal ValorAtivo { get; set; }
    public DateOnly DataInicioVigencia { get; set; }
    public DateOnly DataFimVigencia { get; set; }
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
}
