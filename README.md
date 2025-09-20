# Benchmark de Performance - Mercado Financeiro

Compara a performance entre **DynamoDB** e **PostgreSQL** para operações de ativos financeiros.

## O que faz

- Executa benchmarks de inserção, consulta e atualização
- Compara latência e throughput entre os dois bancos
- Testa diferentes cenários de consulta (por família, série, atributo, data)
- Gera relatórios de performance detalhados

## Como executar

### 1. Configurar bancos de dados

```bash
# PostgreSQL
docker compose -f docker/docker-postgres.yaml up -d

# DynamoDB Local
docker compose -f docker/docker-dynamodb.yaml up -d
```

### 2. Configurar aplicação

Edite `src/Configuration/appsettings.json` com suas credenciais e configurações.

### 3. Executar benchmark

```bash
cd src
dotnet run
```

### 4. Ver resultados

- **Release**: Executa benchmark completo com BenchmarkDotNet
- **Debug**: Executa testes manuais para desenvolvimento

## Estrutura

- `Models/`: Classes de domínio (Valor, Familia, Serie, etc.)
- `Repositories/`: Implementações para PostgreSQL e DynamoDB
- `Services/`: Lógica de benchmark
- `Tests/`: Classes de teste de performance
- `docker/`: Configurações Docker para bancos locais

## Dependências

- .NET 9.0
- PostgreSQL ou DynamoDB Local
- AWSSDK.DynamoDBv2
- Npgsql
