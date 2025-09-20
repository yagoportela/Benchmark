namespace BenchmarkFinanceiro.Models;

public class AggregateValorPostgres
{
    public Arquivo Arquivo { get; set; } = null!;
    public Familia Familia { get; set; } = null!;
    public Serie Serie { get; set; } = null!;
    public Atributo Atributo { get; set; } = null!;
    public ValorPostgres Valor { get; set; } = null!;
}
