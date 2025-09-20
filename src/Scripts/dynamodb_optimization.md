# Otimizações para DynamoDB

## Configurações de Capacidade

### On-Demand vs Provisioned
Para o volume de dados especificado (2M inserts, 2M updates, 15M consultas/dia), recomenda-se:

- **On-Demand**: Para cargas variáveis e desenvolvimento
- **Provisioned**: Para cargas previsíveis e otimização de custos

### Configurações Recomendadas

```json
{
  "ReadCapacityUnits": 1000,
  "WriteCapacityUnits": 1000,
  "AutoScaling": {
    "ReadCapacity": {
      "MinCapacity": 500,
      "MaxCapacity": 2000,
      "TargetUtilization": 70
    },
    "WriteCapacity": {
      "MinCapacity": 500,
      "MaxCapacity": 2000,
      "TargetUtilization": 70
    }
  }
}
```

## Estrutura de Índices

### Global Secondary Indexes (GSI)

1. **GSI1**: Para consultas por família
   - PK: FAMILIA#{codigoFamilia}
   - SK: SERIE#{codigoSerie}#ATRIBUTO#{codigoAtributo}

2. **GSI2**: Para consultas por arquivo
   - PK: ARQUIVO#{codigoArquivo}
   - SK: VALOR#{codigoFamilia}#{codigoSerie}#{codigoAtributo}

3. **GSI3**: Para consultas por data
   - PK: VALOR
   - SK: {dataAtualizacao:yyyy-MM-dd}#{codigoFamilia}#{codigoSerie}#{codigoAtributo}

## Otimizações de Consulta

### 1. Batch Operations
- Use `BatchWriteItem` para operações em lote
- Máximo 25 itens por batch
- Implemente retry logic para itens não processados

### 2. Query Optimization
- Use `Query` em vez de `Scan` sempre que possível
- Implemente paginação com `ExclusiveStartKey`
- Use `FilterExpression` apenas quando necessário

### 3. Caching
- Implemente cache local para dados de domínio (família, série, atributo)
- Use DynamoDB Accelerator (DAX) para consultas frequentes

## Configurações de Rede

### VPC Endpoint
```json
{
  "VpcEndpointType": "Gateway",
  "PolicyDocument": {
    "Version": "2012-10-17",
    "Statement": [
      {
        "Effect": "Allow",
        "Principal": "*",
        "Action": [
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem",
          "dynamodb:Query",
          "dynamodb:Scan",
          "dynamodb:BatchGetItem",
          "dynamodb:BatchWriteItem"
        ],
        "Resource": "arn:aws:dynamodb:region:account:table/table-name"
      }
    ]
  }
}
```

## Monitoramento

### CloudWatch Metrics
- `ConsumedReadCapacityUnits`
- `ConsumedWriteCapacityUnits`
- `ThrottledRequests`
- `UserErrors`
- `SystemErrors`

### Alarms
```json
{
  "ThrottledRequests": {
    "Threshold": 10,
    "Period": 300,
    "EvaluationPeriods": 2
  },
  "HighReadUtilization": {
    "Threshold": 80,
    "Period": 300,
    "EvaluationPeriods": 2
  }
}
```

## Configurações de Segurança

### IAM Policy
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "dynamodb:GetItem",
        "dynamodb:PutItem",
        "dynamodb:UpdateItem",
        "dynamodb:DeleteItem",
        "dynamodb:Query",
        "dynamodb:Scan",
        "dynamodb:BatchGetItem",
        "dynamodb:BatchWriteItem"
      ],
      "Resource": [
        "arn:aws:dynamodb:region:account:table/table-name",
        "arn:aws:dynamodb:region:account:table/table-name/index/*"
      ]
    }
  ]
}
```

## Configurações de Backup

### Point-in-Time Recovery
```json
{
  "PointInTimeRecoverySpecification": {
    "PointInTimeRecoveryEnabled": true
  }
}
```

### Backup Automático
```json
{
  "BackupPolicy": {
    "BackupRetentionPeriod": 7,
    "BackupSchedule": "cron(0 2 * * ? *)"
  }
}
```

## Configurações de TTL

Para limpeza automática de dados antigos:
```json
{
  "TimeToLiveSpecification": {
    "AttributeName": "ttl",
    "Enabled": true
  }
}
```

## Configurações de Streams

Para processamento em tempo real:
```json
{
  "StreamSpecification": {
    "StreamEnabled": true,
    "StreamViewType": "NEW_AND_OLD_IMAGES"
  }
}
```

## Configurações de Global Tables

Para replicação multi-região:
```json
{
  "GlobalTableSpecification": {
    "Regions": ["us-east-1", "us-west-2", "eu-west-1"]
  }
}
```

## Configurações de DAX

Para cache de consultas:
```json
{
  "ClusterName": "benchmark-dax-cluster",
  "NodeType": "dax.r4.large",
  "ReplicationFactor": 3,
  "SubnetGroupName": "dax-subnet-group",
  "SecurityGroupIds": ["sg-xxxxxxxxx"]
}
```

## Configurações de Lambda Triggers

Para processamento assíncrono:
```json
{
  "LambdaTriggers": [
    {
      "FunctionName": "process-valor-updates",
      "EventSourceArn": "arn:aws:dynamodb:region:account:table/table-name/stream/stream-id"
    }
  ]
}
```

## Configurações de CloudFormation

Template completo para infraestrutura:
```yaml
Resources:
  BenchmarkTable:
    Type: AWS::DynamoDB::Table
    Properties:
      TableName: BenchmarkFinanceiro
      BillingMode: PROVISIONED
      ProvisionedThroughput:
        ReadCapacityUnits: 1000
        WriteCapacityUnits: 1000
      AttributeDefinitions:
        - AttributeName: PK
          AttributeType: S
        - AttributeName: SK
          AttributeType: S
        - AttributeName: GSI1PK
          AttributeType: S
        - AttributeName: GSI1SK
          AttributeType: S
        - AttributeName: GSI2PK
          AttributeType: S
        - AttributeName: GSI2SK
          AttributeType: S
        - AttributeName: GSI3PK
          AttributeType: S
        - AttributeName: GSI3SK
          AttributeType: S
      KeySchema:
        - AttributeName: PK
          KeyType: HASH
        - AttributeName: SK
          KeyType: RANGE
      GlobalSecondaryIndexes:
        - IndexName: GSI1
          KeySchema:
            - AttributeName: GSI1PK
              KeyType: HASH
            - AttributeName: GSI1SK
              KeyType: RANGE
          Projection:
            ProjectionType: ALL
          ProvisionedThroughput:
            ReadCapacityUnits: 1000
            WriteCapacityUnits: 1000
        - IndexName: GSI2
          KeySchema:
            - AttributeName: GSI2PK
              KeyType: HASH
            - AttributeName: GSI2SK
              KeyType: RANGE
          Projection:
            ProjectionType: ALL
          ProvisionedThroughput:
            ReadCapacityUnits: 1000
            WriteCapacityUnits: 1000
        - IndexName: GSI3
          KeySchema:
            - AttributeName: GSI3PK
              KeyType: HASH
            - AttributeName: GSI3SK
              KeyType: RANGE
          Projection:
            ProjectionType: ALL
          ProvisionedThroughput:
            ReadCapacityUnits: 1000
            WriteCapacityUnits: 1000
      PointInTimeRecoverySpecification:
        PointInTimeRecoveryEnabled: true
      StreamSpecification:
        StreamEnabled: true
        StreamViewType: NEW_AND_OLD_IMAGES
      TimeToLiveSpecification:
        AttributeName: ttl
        Enabled: true
      Tags:
        - Key: Environment
          Value: Production
        - Key: Application
          Value: BenchmarkFinanceiro
```

## Configurações de Auto Scaling

```yaml
Resources:
  ReadCapacityAutoScaling:
    Type: AWS::ApplicationAutoScaling::ScalableTarget
    Properties:
      MaxCapacity: 2000
      MinCapacity: 500
      ResourceId: !Sub "table/${BenchmarkTable}"
      RoleARN: !Sub "arn:aws:iam::${AWS::AccountId}:role/aws-service-role/dynamodb.application-autoscaling.amazonaws.com/AWSServiceRoleForApplicationAutoScaling_DynamoDBTable"
      ScalableDimension: dynamodb:table:ReadCapacityUnits
      ServiceNamespace: dynamodb

  WriteCapacityAutoScaling:
    Type: AWS::ApplicationAutoScaling::ScalableTarget
    Properties:
      MaxCapacity: 2000
      MinCapacity: 500
      ResourceId: !Sub "table/${BenchmarkTable}"
      RoleARN: !Sub "arn:aws:iam::${AWS::AccountId}:role/aws-service-role/dynamodb.application-autoscaling.amazonaws.com/AWSServiceRoleForApplicationAutoScaling_DynamoDBTable"
      ScalableDimension: dynamodb:table:WriteCapacityUnits
      ServiceNamespace: dynamodb
```

## Configurações de Monitoramento

```yaml
Resources:
  ThrottledRequestsAlarm:
    Type: AWS::CloudWatch::Alarm
    Properties:
      AlarmName: DynamoDB-ThrottledRequests
      AlarmDescription: DynamoDB throttled requests
      MetricName: ThrottledRequests
      Namespace: AWS/DynamoDB
      Statistic: Sum
      Period: 300
      EvaluationPeriods: 2
      Threshold: 10
      ComparisonOperator: GreaterThanThreshold
      Dimensions:
        - Name: TableName
          Value: !Ref BenchmarkTable

  HighReadUtilizationAlarm:
    Type: AWS::CloudWatch::Alarm
    Properties:
      AlarmName: DynamoDB-HighReadUtilization
      AlarmDescription: DynamoDB high read utilization
      MetricName: ConsumedReadCapacityUnits
      Namespace: AWS/DynamoDB
      Statistic: Average
      Period: 300
      EvaluationPeriods: 2
      Threshold: 800
      ComparisonOperator: GreaterThanThreshold
      Dimensions:
        - Name: TableName
          Value: !Ref BenchmarkTable
```

## Configurações de Backup

```yaml
Resources:
  BackupPlan:
    Type: AWS::Backup::BackupPlan
    Properties:
      BackupPlan:
        BackupPlanName: DynamoDB-Backup-Plan
        BackupPlanRule:
          - RuleName: DailyBackup
            TargetBackupVault: !Ref BackupVault
            ScheduleExpression: "cron(0 2 * * ? *)"
            StartWindowMinutes: 60
            CompletionWindowMinutes: 120
            Lifecycle:
              DeleteAfterDays: 30
              MoveToColdStorageAfterDays: 7

  BackupVault:
    Type: AWS::Backup::BackupVault
    Properties:
      BackupVaultName: DynamoDB-Backup-Vault
      BackupVaultTags:
        Environment: Production
        Application: BenchmarkFinanceiro
```

## Configurações de Segurança

```yaml
Resources:
  DynamoDBRole:
    Type: AWS::IAM::Role
    Properties:
      RoleName: DynamoDB-Benchmark-Role
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: ec2.amazonaws.com
            Action: sts:AssumeRole
      Policies:
        - PolicyName: DynamoDBBenchmarkPolicy
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - dynamodb:GetItem
                  - dynamodb:PutItem
                  - dynamodb:UpdateItem
                  - dynamodb:DeleteItem
                  - dynamodb:Query
                  - dynamodb:Scan
                  - dynamodb:BatchGetItem
                  - dynamodb:BatchWriteItem
                Resource:
                  - !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}"
                  - !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}/index/*"
```

## Configurações de VPC Endpoint

```yaml
Resources:
  DynamoDBVPCEndpoint:
    Type: AWS::EC2::VPCEndpoint
    Properties:
      VpcId: !Ref VPC
      ServiceName: !Sub "com.amazonaws.${AWS::Region}.dynamodb"
      VpcEndpointType: Gateway
      RouteTableIds:
        - !Ref PrivateRouteTable
      PolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal: '*'
            Action:
              - dynamodb:GetItem
              - dynamodb:PutItem
              - dynamodb:UpdateItem
              - dynamodb:DeleteItem
              - dynamodb:Query
              - dynamodb:Scan
              - dynamodb:BatchGetItem
              - dynamodb:BatchWriteItem
            Resource: !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}"
```

## Configurações de DAX

```yaml
Resources:
  DAXSubnetGroup:
    Type: AWS::DAX::SubnetGroup
    Properties:
      SubnetGroupName: benchmark-dax-subnet-group
      Description: Subnet group for DAX cluster
      SubnetIds:
        - !Ref PrivateSubnet1
        - !Ref PrivateSubnet2

  DAXSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupName: benchmark-dax-sg
      GroupDescription: Security group for DAX cluster
      VpcId: !Ref VPC
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 8111
          ToPort: 8111
          SourceSecurityGroupId: !Ref ApplicationSecurityGroup

  DAXCluster:
    Type: AWS::DAX::Cluster
    Properties:
      ClusterName: benchmark-dax-cluster
      Description: DAX cluster for benchmark
      NodeType: dax.r4.large
      ReplicationFactor: 3
      SubnetGroupName: !Ref DAXSubnetGroup
      SecurityGroupIds:
        - !Ref DAXSecurityGroup
      IAMRoleARN: !GetAtt DAXRole.Arn
      ParameterGroupName: default.dax1.0
      Tags:
        - Key: Environment
          Value: Production
        - Key: Application
          Value: BenchmarkFinanceiro

  DAXRole:
    Type: AWS::IAM::Role
    Properties:
      RoleName: DAX-Benchmark-Role
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: dax.amazonaws.com
            Action: sts:AssumeRole
      Policies:
        - PolicyName: DAXBenchmarkPolicy
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - dynamodb:GetItem
                  - dynamodb:PutItem
                  - dynamodb:UpdateItem
                  - dynamodb:DeleteItem
                  - dynamodb:Query
                  - dynamodb:Scan
                  - dynamodb:BatchGetItem
                  - dynamodb:BatchWriteItem
                Resource:
                  - !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}"
                  - !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}/index/*"
```

## Configurações de Lambda Triggers

```yaml
Resources:
  ProcessValorUpdatesFunction:
    Type: AWS::Lambda::Function
    Properties:
      FunctionName: process-valor-updates
      Runtime: dotnet8
      Handler: ProcessValorUpdates::ProcessValorUpdates.Function::FunctionHandler
      Code:
        ZipFile: |
          using Amazon.Lambda.Core;
          using Amazon.Lambda.DynamoDBEvents;
          using System.Text.Json;

          [assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

          namespace ProcessValorUpdates
          {
              public class Function
              {
                  public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
                  {
                      foreach (var record in dynamoEvent.Records)
                      {
                          if (record.EventName == "INSERT" || record.EventName == "MODIFY")
                          {
                              // Processar atualização de valor
                              var valor = JsonSerializer.Deserialize<Valor>(record.Dynamodb.NewImage);
                              await ProcessValorUpdate(valor);
                          }
                      }
                  }

                  private async Task ProcessValorUpdate(Valor valor)
                  {
                      // Implementar lógica de processamento
                  }
              }
          }
      Role: !GetAtt LambdaExecutionRole.Arn
      Timeout: 30
      MemorySize: 256
      Environment:
        Variables:
          TABLE_NAME: !Ref BenchmarkTable

  LambdaExecutionRole:
    Type: AWS::IAM::Role
    Properties:
      RoleName: Lambda-Benchmark-Role
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: lambda.amazonaws.com
            Action: sts:AssumeRole
      ManagedPolicyArns:
        - arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole
      Policies:
        - PolicyName: LambdaBenchmarkPolicy
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - dynamodb:GetItem
                  - dynamodb:PutItem
                  - dynamodb:UpdateItem
                  - dynamodb:DeleteItem
                  - dynamodb:Query
                  - dynamodb:Scan
                Resource:
                  - !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}"
                  - !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}/index/*"

  DynamoDBStreamEventSourceMapping:
    Type: AWS::Lambda::EventSourceMapping
    Properties:
      EventSourceArn: !GetAtt BenchmarkTable.StreamArn
      FunctionName: !Ref ProcessValorUpdatesFunction
      StartingPosition: LATEST
      BatchSize: 10
      MaximumBatchingWindowInSeconds: 5
```

## Configurações de Global Tables

```yaml
Resources:
  GlobalTable:
    Type: AWS::DynamoDB::GlobalTable
    Properties:
      TableName: BenchmarkFinanceiro
      AttributeDefinitions:
        - AttributeName: PK
          AttributeType: S
        - AttributeName: SK
          AttributeType: S
        - AttributeName: GSI1PK
          AttributeType: S
        - AttributeName: GSI1SK
          AttributeType: S
        - AttributeName: GSI2PK
          AttributeType: S
        - AttributeName: GSI2SK
          AttributeType: S
        - AttributeName: GSI3PK
          AttributeType: S
        - AttributeName: GSI3SK
          AttributeType: S
      KeySchema:
        - AttributeName: PK
          KeyType: HASH
        - AttributeName: SK
          KeyType: RANGE
      GlobalSecondaryIndexes:
        - IndexName: GSI1
          KeySchema:
            - AttributeName: GSI1PK
              KeyType: HASH
            - AttributeName: GSI1SK
              KeyType: RANGE
          Projection:
            ProjectionType: ALL
          ProvisionedThroughput:
            ReadCapacityUnits: 1000
            WriteCapacityUnits: 1000
        - IndexName: GSI2
          KeySchema:
            - AttributeName: GSI2PK
              KeyType: HASH
            - AttributeName: GSI2SK
              KeyType: RANGE
          Projection:
            ProjectionType: ALL
          ProvisionedThroughput:
            ReadCapacityUnits: 1000
            WriteCapacityUnits: 1000
        - IndexName: GSI3
          KeySchema:
            - AttributeName: GSI3PK
              KeyType: HASH
            - AttributeName: GSI3SK
              KeyType: RANGE
          Projection:
            ProjectionType: ALL
          ProvisionedThroughput:
            ReadCapacityUnits: 1000
            WriteCapacityUnits: 1000
      Replicas:
        - Region: us-east-1
          GlobalSecondaryIndexes:
            - IndexName: GSI1
              ProvisionedThroughput:
                ReadCapacityUnits: 1000
                WriteCapacityUnits: 1000
            - IndexName: GSI2
              ProvisionedThroughput:
                ReadCapacityUnits: 1000
                WriteCapacityUnits: 1000
            - IndexName: GSI3
              ProvisionedThroughput:
                ReadCapacityUnits: 1000
                WriteCapacityUnits: 1000
        - Region: us-west-2
          GlobalSecondaryIndexes:
            - IndexName: GSI1
              ProvisionedThroughput:
                ReadCapacityUnits: 1000
                WriteCapacityUnits: 1000
            - IndexName: GSI2
              ProvisionedThroughput:
                ReadCapacityUnits: 1000
                WriteCapacityUnits: 1000
            - IndexName: GSI3
              ProvisionedThroughput:
                ReadCapacityUnits: 1000
                WriteCapacityUnits: 1000
        - Region: eu-west-1
          GlobalSecondaryIndexes:
            - IndexName: GSI1
              ProvisionedThroughput:
                ReadCapacityUnits: 1000
                WriteCapacityUnits: 1000
            - IndexName: GSI2
              ProvisionedThroughput:
                ReadCapacityUnits: 1000
                WriteCapacityUnits: 1000
            - IndexName: GSI3
              ProvisionedThroughput:
                ReadCapacityUnits: 1000
                WriteCapacityUnits: 1000
      StreamSpecification:
        StreamEnabled: true
        StreamViewType: NEW_AND_OLD_IMAGES
      TimeToLiveSpecification:
        AttributeName: ttl
        Enabled: true
      PointInTimeRecoverySpecification:
        PointInTimeRecoveryEnabled: true
```

## Configurações de Monitoramento Avançado

```yaml
Resources:
  CloudWatchDashboard:
    Type: AWS::CloudWatch::Dashboard
    Properties:
      DashboardName: DynamoDB-Benchmark-Dashboard
      DashboardBody: !Sub |
        {
          "widgets": [
            {
              "type": "metric",
              "x": 0,
              "y": 0,
              "width": 12,
              "height": 6,
              "properties": {
                "metrics": [
                  [ "AWS/DynamoDB", "ConsumedReadCapacityUnits", "TableName", "${BenchmarkTable}" ],
                  [ "AWS/DynamoDB", "ConsumedWriteCapacityUnits", "TableName", "${BenchmarkTable}" ]
                ],
                "view": "timeSeries",
                "stacked": false,
                "region": "${AWS::Region}",
                "title": "Consumed Capacity Units",
                "period": 300
              }
            },
            {
              "type": "metric",
              "x": 12,
              "y": 0,
              "width": 12,
              "height": 6,
              "properties": {
                "metrics": [
                  [ "AWS/DynamoDB", "ThrottledRequests", "TableName", "${BenchmarkTable}" ],
                  [ "AWS/DynamoDB", "UserErrors", "TableName", "${BenchmarkTable}" ],
                  [ "AWS/DynamoDB", "SystemErrors", "TableName", "${BenchmarkTable}" ]
                ],
                "view": "timeSeries",
                "stacked": false,
                "region": "${AWS::Region}",
                "title": "Errors and Throttling",
                "period": 300
              }
            },
            {
              "type": "metric",
              "x": 0,
              "y": 6,
              "width": 12,
              "height": 6,
              "properties": {
                "metrics": [
                  [ "AWS/DynamoDB", "SuccessfulRequestLatency", "TableName", "${BenchmarkTable}", "Operation", "GetItem" ],
                  [ "AWS/DynamoDB", "SuccessfulRequestLatency", "TableName", "${BenchmarkTable}", "Operation", "PutItem" ],
                  [ "AWS/DynamoDB", "SuccessfulRequestLatency", "TableName", "${BenchmarkTable}", "Operation", "Query" ],
                  [ "AWS/DynamoDB", "SuccessfulRequestLatency", "TableName", "${BenchmarkTable}", "Operation", "Scan" ]
                ],
                "view": "timeSeries",
                "stacked": false,
                "region": "${AWS::Region}",
                "title": "Request Latency",
                "period": 300
              }
            },
            {
              "type": "metric",
              "x": 12,
              "y": 6,
              "width": 12,
              "height": 6,
              "properties": {
                "metrics": [
                  [ "AWS/DynamoDB", "ItemCount", "TableName", "${BenchmarkTable}" ],
                  [ "AWS/DynamoDB", "TableSizeBytes", "TableName", "${BenchmarkTable}" ]
                ],
                "view": "timeSeries",
                "stacked": false,
                "region": "${AWS::Region}",
                "title": "Table Size and Item Count",
                "period": 300
              }
            }
          ]
        }

  CloudWatchLogGroup:
    Type: AWS::Logs::LogGroup
    Properties:
      LogGroupName: /aws/dynamodb/benchmark
      RetentionInDays: 30

  CloudWatchLogStream:
    Type: AWS::Logs::LogStream
    Properties:
      LogGroupName: !Ref CloudWatchLogGroup
      LogStreamName: benchmark-stream
```

## Configurações de Backup Avançado

```yaml
Resources:
  BackupVault:
    Type: AWS::Backup::BackupVault
    Properties:
      BackupVaultName: DynamoDB-Backup-Vault
      BackupVaultTags:
        Environment: Production
        Application: BenchmarkFinanceiro
      EncryptionKeyArn: !Ref BackupKMSKey

  BackupKMSKey:
    Type: AWS::KMS::Key
    Properties:
      Description: KMS key for DynamoDB backup encryption
      KeyPolicy:
        Version: '2012-10-17'
        Statement:
          - Sid: Enable IAM User Permissions
            Effect: Allow
            Principal:
              AWS: !Sub "arn:aws:iam::${AWS::AccountId}:root"
            Action: kms:*
            Resource: '*'
          - Sid: Allow Backup Service
            Effect: Allow
            Principal:
              Service: backup.amazonaws.com
            Action:
              - kms:Decrypt
              - kms:DescribeKey
              - kms:Encrypt
              - kms:GenerateDataKey*
              - kms:ReEncrypt*
            Resource: '*'
            Condition:
              StringEquals:
                kms:ViaService: !Sub "backup.${AWS::Region}.amazonaws.com"

  BackupPlan:
    Type: AWS::Backup::BackupPlan
    Properties:
      BackupPlan:
        BackupPlanName: DynamoDB-Backup-Plan
        BackupPlanRule:
          - RuleName: DailyBackup
            TargetBackupVault: !Ref BackupVault
            ScheduleExpression: "cron(0 2 * * ? *)"
            StartWindowMinutes: 60
            CompletionWindowMinutes: 120
            Lifecycle:
              DeleteAfterDays: 30
              MoveToColdStorageAfterDays: 7
          - RuleName: WeeklyBackup
            TargetBackupVault: !Ref BackupVault
            ScheduleExpression: "cron(0 3 ? * SUN *)"
            StartWindowMinutes: 60
            CompletionWindowMinutes: 120
            Lifecycle:
              DeleteAfterDays: 90
              MoveToColdStorageAfterDays: 30
          - RuleName: MonthlyBackup
            TargetBackupVault: !Ref BackupVault
            ScheduleExpression: "cron(0 4 1 * ? *)"
            StartWindowMinutes: 60
            CompletionWindowMinutes: 120
            Lifecycle:
              DeleteAfterDays: 365
              MoveToColdStorageAfterDays: 90

  BackupSelection:
    Type: AWS::Backup::BackupSelection
    Properties:
      BackupPlanId: !Ref BackupPlan
      BackupSelection:
        SelectionName: DynamoDB-Backup-Selection
        IamRoleArn: !GetAtt BackupRole.Arn
        Resources:
          - !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}"
        Conditions:
          StringEquals:
            - Key: aws:ResourceTag/Environment
              Value: Production

  BackupRole:
    Type: AWS::IAM::Role
    Properties:
      RoleName: Backup-Benchmark-Role
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: backup.amazonaws.com
            Action: sts:AssumeRole
      ManagedPolicyArns:
        - arn:aws:iam::aws:policy/service-role/AWSBackupServiceRolePolicyForBackup
      Policies:
        - PolicyName: BackupBenchmarkPolicy
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - dynamodb:DescribeTable
                  - dynamodb:ListTagsOfResource
                Resource: !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}"
              - Effect: Allow
                Action:
                  - kms:Decrypt
                  - kms:DescribeKey
                  - kms:Encrypt
                  - kms:GenerateDataKey*
                  - kms:ReEncrypt*
                Resource: !GetAtt BackupKMSKey.Arn
```

## Configurações de Segurança Avançada

```yaml
Resources:
  DynamoDBEncryptionKey:
    Type: AWS::KMS::Key
    Properties:
      Description: KMS key for DynamoDB table encryption
      KeyPolicy:
        Version: '2012-10-17'
        Statement:
          - Sid: Enable IAM User Permissions
            Effect: Allow
            Principal:
              AWS: !Sub "arn:aws:iam::${AWS::AccountId}:root"
            Action: kms:*
            Resource: '*'
          - Sid: Allow DynamoDB Service
            Effect: Allow
            Principal:
              Service: dynamodb.amazonaws.com
            Action:
              - kms:Decrypt
              - kms:DescribeKey
              - kms:Encrypt
              - kms:GenerateDataKey*
              - kms:ReEncrypt*
            Resource: '*'
            Condition:
              StringEquals:
                kms:ViaService: !Sub "dynamodb.${AWS::Region}.amazonaws.com"

  DynamoDBEncryptionKeyAlias:
    Type: AWS::KMS::Alias
    Properties:
      AliasName: alias/dynamodb-benchmark
      TargetKeyId: !Ref DynamoDBEncryptionKey

  DynamoDBTable:
    Type: AWS::DynamoDB::Table
    Properties:
      TableName: BenchmarkFinanceiro
      BillingMode: PROVISIONED
      ProvisionedThroughput:
        ReadCapacityUnits: 1000
        WriteCapacityUnits: 1000
      AttributeDefinitions:
        - AttributeName: PK
          AttributeType: S
        - AttributeName: SK
          AttributeType: S
        - AttributeName: GSI1PK
          AttributeType: S
        - AttributeName: GSI1SK
          AttributeType: S
        - AttributeName: GSI2PK
          AttributeType: S
        - AttributeName: GSI2SK
          AttributeType: S
        - AttributeName: GSI3PK
          AttributeType: S
        - AttributeName: GSI3SK
          AttributeType: S
      KeySchema:
        - AttributeName: PK
          KeyType: HASH
        - AttributeName: SK
          KeyType: RANGE
      GlobalSecondaryIndexes:
        - IndexName: GSI1
          KeySchema:
            - AttributeName: GSI1PK
              KeyType: HASH
            - AttributeName: GSI1SK
              KeyType: RANGE
          Projection:
            ProjectionType: ALL
          ProvisionedThroughput:
            ReadCapacityUnits: 1000
            WriteCapacityUnits: 1000
        - IndexName: GSI2
          KeySchema:
            - AttributeName: GSI2PK
              KeyType: HASH
            - AttributeName: GSI2SK
              KeyType: RANGE
          Projection:
            ProjectionType: ALL
          ProvisionedThroughput:
            ReadCapacityUnits: 1000
            WriteCapacityUnits: 1000
        - IndexName: GSI3
          KeySchema:
            - AttributeName: GSI3PK
              KeyType: HASH
            - AttributeName: GSI3SK
              KeyType: RANGE
          Projection:
            ProjectionType: ALL
          ProvisionedThroughput:
            ReadCapacityUnits: 1000
            WriteCapacityUnits: 1000
      SSESpecification:
        SSEEnabled: true
        KMSMasterKeyId: !Ref DynamoDBEncryptionKey
      PointInTimeRecoverySpecification:
        PointInTimeRecoveryEnabled: true
      StreamSpecification:
        StreamEnabled: true
        StreamViewType: NEW_AND_OLD_IMAGES
      TimeToLiveSpecification:
        AttributeName: ttl
        Enabled: true
      Tags:
        - Key: Environment
          Value: Production
        - Key: Application
          Value: BenchmarkFinanceiro
        - Key: DataClassification
          Value: Confidential
        - Key: Compliance
          Value: SOX
```

## Configurações de Compliance

```yaml
Resources:
  ConfigRule:
    Type: AWS::Config::ConfigRule
    Properties:
      ConfigRuleName: DynamoDB-Benchmark-Compliance
      Description: Ensure DynamoDB table compliance
      Source:
        Owner: AWS
        SourceIdentifier: DYNAMODB_ENCRYPTION_ENABLED
      Scope:
        ComplianceResourceTypes:
          - AWS::DynamoDB::Table
      InputParameters: '{}'

  ConfigRule2:
    Type: AWS::Config::ConfigRule
    Properties:
      ConfigRuleName: DynamoDB-Backup-Compliance
      Description: Ensure DynamoDB backup compliance
      Source:
        Owner: AWS
        SourceIdentifier: DYNAMODB_BACKUP_ENABLED
      Scope:
        ComplianceResourceTypes:
          - AWS::DynamoDB::Table
      InputParameters: '{}'

  ConfigRule3:
    Type: AWS::Config::ConfigRule
    Properties:
      ConfigRuleName: DynamoDB-PointInTimeRecovery-Compliance
      Description: Ensure DynamoDB point-in-time recovery compliance
      Source:
        Owner: AWS
        SourceIdentifier: DYNAMODB_PITR_ENABLED
      Scope:
        ComplianceResourceTypes:
          - AWS::DynamoDB::Table
      InputParameters: '{}'
```

## Configurações de Auditoria

```yaml
Resources:
  CloudTrail:
    Type: AWS::CloudTrail::Trail
    Properties:
      TrailName: DynamoDB-Benchmark-Trail
      S3BucketName: !Ref CloudTrailBucket
      S3KeyPrefix: dynamodb-benchmark/
      IncludeGlobalServiceEvents: true
      IsMultiRegionTrail: true
      EventSelectors:
        - ReadWriteType: All
          IncludeManagementEvents: true
          DataResources:
            - Type: AWS::DynamoDB::Table
              Values:
                - !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}"
      CloudWatchLogsLogGroupArn: !GetAtt CloudTrailLogGroup.Arn
      CloudWatchLogsRoleArn: !GetAtt CloudTrailRole.Arn
      Tags:
        - Key: Environment
          Value: Production
        - Key: Application
          Value: BenchmarkFinanceiro

  CloudTrailBucket:
    Type: AWS::S3::Bucket
    Properties:
      BucketName: !Sub "dynamodb-benchmark-cloudtrail-${AWS::AccountId}"
      VersioningConfiguration:
        Status: Enabled
      BucketEncryption:
        ServerSideEncryptionConfiguration:
          - ServerSideEncryptionByDefault:
              SSEAlgorithm: AES256
      PublicAccessBlockConfiguration:
        BlockPublicAcls: true
        BlockPublicPolicy: true
        IgnorePublicAcls: true
        RestrictPublicBuckets: true
      LifecycleConfiguration:
        Rules:
          - Id: DeleteOldLogs
            Status: Enabled
            ExpirationInDays: 90
            NoncurrentVersionExpirationInDays: 30

  CloudTrailLogGroup:
    Type: AWS::Logs::LogGroup
    Properties:
      LogGroupName: /aws/cloudtrail/dynamodb-benchmark
      RetentionInDays: 30

  CloudTrailRole:
    Type: AWS::IAM::Role
    Properties:
      RoleName: CloudTrail-Benchmark-Role
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: cloudtrail.amazonaws.com
            Action: sts:AssumeRole
      Policies:
        - PolicyName: CloudTrailBenchmarkPolicy
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - logs:CreateLogGroup
                  - logs:CreateLogStream
                  - logs:PutLogEvents
                Resource: !GetAtt CloudTrailLogGroup.Arn
              - Effect: Allow
                Action:
                  - s3:GetBucketAcl
                  - s3:PutObject
                Resource:
                  - !GetAtt CloudTrailBucket.Arn
                  - !Sub "${CloudTrailBucket}/*"
```

## Configurações de Disaster Recovery

```yaml
Resources:
  CrossRegionReplicationRole:
    Type: AWS::IAM::Role
    Properties:
      RoleName: CrossRegionReplication-Benchmark-Role
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: dynamodb.amazonaws.com
            Action: sts:AssumeRole
      Policies:
        - PolicyName: CrossRegionReplicationBenchmarkPolicy
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - dynamodb:CreateTable
                  - dynamodb:DescribeTable
                  - dynamodb:PutItem
                  - dynamodb:UpdateItem
                  - dynamodb:DeleteItem
                  - dynamodb:GetItem
                  - dynamodb:Query
                  - dynamodb:Scan
                Resource:
                  - !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}"
                  - !Sub "arn:aws:dynamodb:${AWS::Region}:${AWS::AccountId}:table/${BenchmarkTable}/index/*"

  DisasterRecoveryPlan:
    Type: AWS::SSM::Document
    Properties:
      DocumentType: Command
      DocumentFormat: YAML
      Name: DynamoDB-Benchmark-DR-Plan
      Content: !Sub |
        schemaVersion: '2.2'
        description: 'Disaster Recovery Plan for DynamoDB Benchmark'
        parameters:
          SourceRegion:
            type: String
            description: 'Source AWS Region'
            default: '${AWS::Region}'
          TargetRegion:
            type: String
            description: 'Target AWS Region for DR'
            default: 'us-west-2'
          TableName:
            type: String
            description: 'DynamoDB Table Name'
            default: '${BenchmarkTable}'
        mainSteps:
          - action: aws:runShellScript
            name: CheckSourceTable
            inputs:
              runCommand:
                - 'aws dynamodb describe-table --table-name {{TableName}} --region {{SourceRegion}}'
          - action: aws:runShellScript
            name: CreateTargetTable
            inputs:
              runCommand:
                - 'aws dynamodb create-table --table-name {{TableName}} --region {{TargetRegion}} --attribute-definitions AttributeName=PK,AttributeType=S AttributeName=SK,AttributeType=S --key-schema AttributeName=PK,KeyType=HASH AttributeName=SK,KeyType=RANGE --provisioned-throughput ReadCapacityUnits=1000,WriteCapacityUnits=1000'
          - action: aws:runShellScript
            name: WaitForTableCreation
            inputs:
              runCommand:
                - 'aws dynamodb wait table-exists --table-name {{TableName}} --region {{TargetRegion}}'
          - action: aws:runShellScript
            name: RestoreFromBackup
            inputs:
                - 'aws dynamodb restore-table-from-backup --target-table-name {{TableName}} --backup-arn {{BackupArn}} --region {{TargetRegion}}'
