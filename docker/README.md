# Docker - Bancos de Dados Locais

Configuração Docker para executar PostgreSQL e DynamoDB Local para desenvolvimento.

## Como usar

### PostgreSQL
```bash
docker compose -f docker/docker-postgres.yaml up -d
```
- Banco: `localhost:5432`
- Usuário: `root`
- Senha: `root`
- Database: `valorativo_db`
- Admin UI: `http://localhost:8002`
- Usuário: `postgresteste@post.com.br`
- Senha: `root`

### DynamoDB Local
```bash
docker compose -f docker/docker-dynamodb.yaml up -d
```
- DynamoDB: `http://localhost:8000`
- Admin UI: `http://localhost:8001`

## Arquivos importantes

- `docker-postgres.yaml`: Configuração do PostgreSQL
- `docker-dynamodb.yaml`: Configuração do DynamoDB Local
- `postgres/valorativo.sql`: Script de criação das tabelas
- `dynamodb/tables/ValorAtivo.json`: Definição da tabela DynamoDB
