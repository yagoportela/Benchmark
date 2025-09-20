using System.ComponentModel.DataAnnotations;

namespace BenchmarkFinanceiro.Models;

public class ValorPostgres
{
    [Key]
    public string NomeArquivo { get; set; } = null!;
    [Key]
    public string NomeFamilia { get; set; } = null!;
    [Key]
    public string NomeSerie { get; set; } = null!;
    [Key]
    public string NomeAtributo { get; set; } = null!;
    public double ValorAtivo { get; set; }
    [Key]
    public DateOnly DataInicioVigencia { get; set; }
    [Key]
    public DateOnly DataFimVigencia { get; set; }
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
}
