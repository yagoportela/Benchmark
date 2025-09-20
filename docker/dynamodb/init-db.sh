#!/bin/sh
# Aguarda um pouco mais para garantir que está totalmente inicializado
sleep 5

for json_file in /tmp/tables/*.json; do
  table_name=$(jq -r '.TableName' "$json_file")
  echo "Criando a tabela: $table_name ..."
  aws dynamodb create-table \
    --endpoint-url http://dynamodb-valorativo:8000 \
    --cli-input-json file://"$json_file"

  echo "Aguardando tabela $table_name ser criada..."
  aws dynamodb wait table-exists \
    --endpoint-url http://dynamodb-valorativo:8000 \
    --table-name "$table_name"

  echo "Tabela $table_name criada com sucesso!"
done

# Insere dados dos serviços AWS
echo "Inserindo dados dos serviços AWS..."

for json_file in /tmp/items/*.json; do
  echo "Processando arquivo: $json_file"
  
  # Extrai o nome da tabela do arquivo JSON
  table_name=$(jq -r '.TableName' "$json_file")
  
  # Extrai o array 'Itens' e passa para o loop
  jq -c '.Itens[]' "$json_file" | while read -r item_json; do    
    # Executa o comando aws dynamodb put-item
    aws dynamodb put-item \
      --endpoint-url http://dynamodb-valorativo:8000 \
      --table-name "$table_name" \
      --item "$item_json"
  done
done

echo "Inicialização do banco de dados concluída!"