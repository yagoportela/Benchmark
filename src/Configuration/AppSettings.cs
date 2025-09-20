namespace BenchmarkFinanceiro.Configuration;

public class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public DynamoDBSettings DynamoDB { get; set; } = new();
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxPoolSize { get; set; } = 100;
    public int CommandTimeout { get; set; } = 30;
}

public class DynamoDBSettings
{
    public string TableName { get; set; } = "BenchmarkFinanceiro";
    public string ServiceUrl { get; set; } = null!;
}

