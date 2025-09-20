-- Scripts de otimização para PostgreSQL
-- Execute estes comandos após criar as tabelas para melhorar a performance

-- 1. Configurações de memória e cache
-- Ajuste estes valores conforme a memória disponível do servidor
ALTER SYSTEM SET shared_buffers = '256MB';  -- 25% da RAM total
ALTER SYSTEM SET effective_cache_size = '1GB';  -- 75% da RAM total
ALTER SYSTEM SET work_mem = '4MB';  -- Para operações de ordenação e hash
ALTER SYSTEM SET maintenance_work_mem = '64MB';  -- Para operações de manutenção

-- 2. Configurações de conexão
ALTER SYSTEM SET max_connections = 100;
ALTER SYSTEM SET checkpoint_completion_target = 0.9;
ALTER SYSTEM SET wal_buffers = '16MB';
ALTER SYSTEM SET default_statistics_target = 100;

-- 3. Índices adicionais para otimização
-- Índice composto para consultas frequentes
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_valor_familia_serie_atributo 
ON valor(codigo_familia, codigo_serie, codigo_atributo);

-- Índice para consultas por data com ordenação
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_valor_data_desc 
ON valor(data_atualizacao DESC);

-- Índice parcial para valores recentes (últimos 30 dias)
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_valor_recentes 
ON valor(codigo_familia, codigo_serie, codigo_atributo, codigo_arquivo) 
WHERE data_atualizacao >= CURRENT_DATE - INTERVAL '30 days';

-- 4. Estatísticas das tabelas
ANALYZE provedor;
ANALYZE arquivo;
ANALYZE familia;
ANALYZE serie;
ANALYZE atributo;
ANALYZE valor;

-- 5. Configurações de particionamento (opcional para volumes muito grandes)
-- Particionamento por data para a tabela valor
-- Descomente se necessário para volumes muito grandes (> 100 milhões de registros)

/*
-- Criar tabela particionada
CREATE TABLE valor_partitioned (
    LIKE valor INCLUDING ALL
) PARTITION BY RANGE (data_atualizacao);

-- Criar partições mensais
CREATE TABLE valor_2024_01 PARTITION OF valor_partitioned
    FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');

CREATE TABLE valor_2024_02 PARTITION OF valor_partitioned
    FOR VALUES FROM ('2024-02-01') TO ('2024-03-01');

-- Adicionar mais partições conforme necessário
*/

-- 6. Views materializadas para consultas complexas frequentes
CREATE MATERIALIZED VIEW IF NOT EXISTS mv_valores_por_familia AS
SELECT 
    f.codigo_familia,
    f.nome_familia,
    COUNT(v.*) as total_valores,
    AVG(v.valor_ativo) as valor_medio,
    MAX(v.data_atualizacao) as ultima_atualizacao
FROM familia f
LEFT JOIN valor v ON f.codigo_familia = v.codigo_familia
GROUP BY f.codigo_familia, f.nome_familia;

-- Índice na view materializada
CREATE UNIQUE INDEX IF NOT EXISTS idx_mv_valores_por_familia 
ON mv_valores_por_familia(codigo_familia);

-- 7. Função para refresh automático da view materializada
CREATE OR REPLACE FUNCTION refresh_valores_por_familia()
RETURNS void AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY mv_valores_por_familia;
END;
$$ LANGUAGE plpgsql;

-- 8. Configurações de autovacuum para tabelas grandes
ALTER TABLE valor SET (autovacuum_vacuum_scale_factor = 0.1);
ALTER TABLE valor SET (autovacuum_analyze_scale_factor = 0.05);
ALTER TABLE valor SET (autovacuum_vacuum_cost_delay = 10);

-- 9. Configurações de log para monitoramento
ALTER SYSTEM SET log_min_duration_statement = 1000;  -- Log queries > 1s
ALTER SYSTEM SET log_checkpoints = on;
ALTER SYSTEM SET log_connections = on;
ALTER SYSTEM SET log_disconnections = on;

-- 10. Configurações de backup e WAL
ALTER SYSTEM SET archive_mode = on;
ALTER SYSTEM SET archive_command = 'test ! -f /var/lib/postgresql/archive/%f && cp %p /var/lib/postgresql/archive/%f';
ALTER SYSTEM SET wal_level = replica;

-- 11. Configurações de conexão pool
-- Para uso com PgBouncer ou similar
ALTER SYSTEM SET tcp_keepalives_idle = 600;
ALTER SYSTEM SET tcp_keepalives_interval = 30;
ALTER SYSTEM SET tcp_keepalives_count = 3;

-- 12. Configurações de query planner
ALTER SYSTEM SET random_page_cost = 1.1;  -- Para SSDs
ALTER SYSTEM SET effective_io_concurrency = 200;  -- Para SSDs
ALTER SYSTEM SET seq_page_cost = 1.0;

-- 13. Configurações de lock timeout
ALTER SYSTEM SET lock_timeout = '30s';
ALTER SYSTEM SET statement_timeout = '5min';

-- 14. Configurações de memory para operações grandes
ALTER SYSTEM SET hash_mem_multiplier = 2.0;
ALTER SYSTEM SET enable_hashjoin = on;
ALTER SYSTEM SET enable_mergejoin = on;
ALTER SYSTEM SET enable_nestloop = on;

-- 15. Configurações de parallel query
ALTER SYSTEM SET max_parallel_workers_per_gather = 4;
ALTER SYSTEM SET max_parallel_workers = 8;
ALTER SYSTEM SET parallel_tuple_cost = 0.1;
ALTER SYSTEM SET parallel_setup_cost = 1000.0;

-- Recarregar configurações
SELECT pg_reload_conf();

-- Verificar configurações aplicadas
SELECT name, setting, unit, context 
FROM pg_settings 
WHERE name IN (
    'shared_buffers', 'effective_cache_size', 'work_mem', 
    'maintenance_work_mem', 'max_connections', 'checkpoint_completion_target',
    'wal_buffers', 'default_statistics_target'
);
