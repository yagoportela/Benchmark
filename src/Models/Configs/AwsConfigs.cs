namespace BenchmarkFinanceiro.Models.Configs;

public class AwsConfigs
{
    public DynamoDBAwsConfigs DynamoDB{ get; set; } = null!;
}

public class DynamoDBAwsConfigs
{
    public string ServiceUrl { get; set; } = null!;
}

