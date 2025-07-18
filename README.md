# Anti-Fraud Service

Microservicio de validación anti-fraude para transacciones financieras implementado en .NET 9 con arquitectura limpia (Clean Architecture).

## 📋 Descripción

Este servicio valida transacciones financieras según reglas anti-fraude y actualiza su estado. Utiliza Kafka para la comunicación asíncrona entre componentes.

### Reglas de Validación Anti-Fraude

- **Rechazo por monto:** Transacciones con valor mayor a $2,000
- **Rechazo por límite diario:** Acumulado diario por cuenta mayor a $20,000
- **Aprobación:** Transacciones que cumplen ambas reglas

### Estados de Transacción

- `pending` - Estado inicial al crear la transacción
- `approved` - Transacción validada y aprobada
- `rejected` - Transacción rechazada por reglas anti-fraude

## 🏗️ Arquitectura

### Estructura del Proyecto

```
AntiFraudService/
├── .github/                    # Configuración de GitHub Actions
├── src/                        # Código fuente
│   ├── AntiFraudService.API/   # Capa de presentación (endpoints)
│   ├── AntiFraudService.Application/ # Lógica de aplicación y casos de uso
│   ├── AntiFraudService.Domain/      # Entidades y lógica de dominio
│   ├── AntiFraudService.Infrastructure/ # Acceso a datos y servicios externos
│   ├── AntiFraudService.Worker/      # Procesamiento asíncrono (Kafka Consumer)
│   └── AntiFraudService.Tests/       # Pruebas unitarias e integración
├── docker-compose.yml          # Configuración de contenedores
├── README.md                   # Documentación
└── .gitignore                 # Archivos ignorados por Git
```

### Tecnologías Utilizadas

- **.NET 9** - Framework principal
- **Entity Framework Core** - ORM para PostgreSQL
- **Kafka** - Mensajería asíncrona
- **PostgreSQL** - Base de datos
- **Docker** - Contenedores para servicios externos
- **Swagger** - Documentación de API

## 🚀 Instalación y Configuración

### Prerrequisitos

- .NET 9 SDK
- Docker Desktop
- PostgreSQL (vía Docker)

### Puertos por Defecto

| Servicio | Puerto | Descripción |
|----------|--------|-------------|
| API HTTP | 5000 | Endpoints de la API |
| API HTTPS | 5001 | Endpoints seguros |
| PostgreSQL | 55432 | Base de datos |
| Kafka | 9092 | Mensajería |
| Swagger UI | 5000 | Documentación |

### Variables de Entorno

#### Development
```bash
# Variables por defecto (no necesarias para desarrollo local)
ASPNETCORE_ENVIRONMENT=Development
DB_PASSWORD=postgres
```

#### Staging
```bash
ASPNETCORE_ENVIRONMENT=Staging
DB_PASSWORD=your_staging_password
KAFKA_BOOTSTRAP_SERVERS=staging-kafka:9092
```

#### Production
```bash
ASPNETCORE_ENVIRONMENT=Production
DB_PASSWORD=your_production_password
KAFKA_BOOTSTRAP_SERVERS=prod-kafka-1:9092,prod-kafka-2:9092,prod-kafka-3:9092
```

### 1. Clonar el repositorio

```bash
git clone <repository-url>
cd AntiFraudService
```

### 2. Levantar servicios externos

```bash
docker-compose up -d
```

Esto levanta:
- **PostgreSQL** en puerto 55432
- **Zookeeper** (requerido por Kafka)
- **Kafka** en puerto 9092

### 3. Aplicar migraciones

```bash
dotnet ef database update --project src/AntiFraudService.Infrastructure/ --startup-project src/AntiFraudService.API/
```

### 4. Ejecutar la aplicación

#### Terminal 1 - API
```bash
# Development (por defecto)
dotnet run --project src/AntiFraudService.API/AntiFraudService.API.csproj

# Staging
ASPNETCORE_ENVIRONMENT=Staging dotnet run --project src/AntiFraudService.API/AntiFraudService.API.csproj

# Production
ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/AntiFraudService.API/AntiFraudService.API.csproj
```

**URLs disponibles:**
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001
- **Swagger UI:** http://localhost:5000 (solo en Development)

#### Terminal 2 - Worker
```bash
# Development (por defecto)
dotnet run --project src/AntiFraudService.Worker/AntiFraudService.Worker.csproj

# Staging
ASPNETCORE_ENVIRONMENT=Staging dotnet run --project src/AntiFraudService.Worker/AntiFraudService.Worker.csproj

# Production
ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/AntiFraudService.Worker/AntiFraudService.Worker.csproj
```

## 📡 API Endpoints

### Crear Transacción
```http
POST http://localhost:5000/api/transactions
Content-Type: application/json

{
  "sourceAccountId": "11111111-1111-1111-1111-111111111111",
  "targetAccountId": "22222222-2222-2222-2222-222222222222",
  "transferTypeId": 1,
  "value": 120
}
```

**Respuesta:**
```json
{
  "id": "guid-de-la-transaccion"
}
```

### Consultar Transacción
```http
GET http://localhost:5000/api/transactions/{id}
```

**Respuesta:**
```json
{
  "id": "guid",
  "sourceAccountId": "11111111-1111-1111-1111-111111111111",
  "targetAccountId": "22222222-2222-2222-2222-222222222222",
  "transferTypeId": 1,
  "value": 120,
  "createdAt": "2024-01-01T00:00:00Z",
  "status": "approved"
}
```

## 🧪 Pruebas

### 1. Acceder a Swagger UI

Abre tu navegador y ve a: **http://localhost:5000**

El navegador se abrirá automáticamente al ejecutar la API (solo en Development).

### 2. Probar casos de uso

#### Caso 1: Transacción Aprobada
```json
{
  "sourceAccountId": "11111111-1111-1111-1111-111111111111",
  "targetAccountId": "22222222-2222-2222-2222-222222222222",
  "transferTypeId": 1,
  "value": 120
}
```
**Resultado esperado:** `status: "approved"`

#### Caso 2: Transacción Rechazada por Monto
```json
{
  "sourceAccountId": "11111111-1111-1111-1111-111111111111",
  "targetAccountId": "22222222-2222-2222-2222-222222222222",
  "transferTypeId": 1,
  "value": 2500
}
```
**Resultado esperado:** `status: "rejected"`

#### Caso 3: Transacción Rechazada por Límite Diario
```json
{
  "sourceAccountId": "11111111-1111-1111-1111-111111111111",
  "targetAccountId": "22222222-2222-2222-2222-222222222222",
  "transferTypeId": 1,
  "value": 21000
}
```
**Resultado esperado:** `status: "rejected"`

### 3. Verificar logs

En la terminal del Worker deberías ver:
```
Starting Worker in Development environment
Worker configured for environment: Development
Worker running at: [timestamp]
Mensaje recibido: [JSON de la transacción]
Transacción [id] actualizada a [status]
```

## 🔄 Flujo de Procesamiento

1. **Cliente envía POST** → API crea transacción con estado "pending"
2. **API publica en Kafka** → Topic "transactions"
3. **Worker consume mensaje** → Valida reglas anti-fraude
4. **Worker actualiza estado** → Base de datos
5. **Worker publica resultado** → Topic "transactions-status"
6. **Cliente consulta GET** → Obtiene estado actualizado

## 📁 Estructura del Proyecto

```
src/
├── AntiFraudService.API/
│   ├── Controllers/           # Endpoints de la API
│   ├── Models/               # Modelos de configuración
│   ├── Program.cs            # Configuración de la aplicación
│   ├── Properties/           # Configuración de puertos
│   └── appsettings.json      # Configuración base
│   ├── appsettings.Development.json  # Configuración Development
│   ├── appsettings.Staging.json      # Configuración Staging
│   └── appsettings.Production.json   # Configuración Production
├── AntiFraudService.Application/
│   ├── DTOs/                 # Objetos de transferencia de datos
│   ├── Interfaces/           # Contratos de servicios
│   └── Services/             # Implementación de servicios
├── AntiFraudService.Domain/
│   └── Entities/             # Entidades de dominio
├── AntiFraudService.Infrastructure/
│   ├── Configuration/        # Configuración de EF Core
│   ├── Messaging/           # Integración con Kafka
│   ├── Persistence/         # Contexto de base de datos
│   └── DependencyInjection.cs # Registro de servicios
└── AntiFraudService.Worker/
    ├── Messaging/           # Consumer de Kafka
    ├── Properties/          # Configuración del Worker
    ├── appsettings.Development.json  # Configuración Development
    ├── appsettings.Staging.json      # Configuración Staging
    ├── appsettings.Production.json   # Configuración Production
    └── Worker.cs            # Lógica de procesamiento
```

## 🔧 Configuración por Ambiente

### Development
- **Swagger:** Habilitado
- **CORS:** Habilitado
- **Rate Limiting:** Deshabilitado
- **Logging:** Detallado
- **Kafka:** Localhost

### Staging
- **Swagger:** Deshabilitado
- **CORS:** Habilitado
- **Rate Limiting:** Habilitado (1000 req/min)
- **Logging:** Información
- **Kafka:** Servidor de staging

### Production
- **Swagger:** Deshabilitado
- **CORS:** Deshabilitado
- **Rate Limiting:** Habilitado (5000 req/min)
- **Logging:** Solo warnings y errores
- **Kafka:** Cluster de producción
- **HTTPS:** Requerido
- **Compresión:** Habilitada

### Variables de Entorno

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
  },
  "Kafka": {
    "BootstrapServers": "${KAFKA_BOOTSTRAP_SERVERS}",
    "ConsumerGroupId": "${KAFKA_CONSUMER_GROUP_ID}",
    "Topics": {
      "Transactions": "${KAFKA_TOPIC_TRANSACTIONS}",
      "TransactionStatus": "${KAFKA_TOPIC_STATUS}"
    }
  },
  "ApiSettings": {
    "Port": "${API_PORT}",
    "HttpsPort": "${API_HTTPS_PORT}",
    "SwaggerEnabled": "${SWAGGER_ENABLED}",
    "CorsEnabled": "${CORS_ENABLED}",
    "RateLimiting": {
      "Enabled": "${RATE_LIMITING_ENABLED}",
      "MaxRequestsPerMinute": "${RATE_LIMITING_MAX_REQUESTS}"
    }
  },
  "WorkerSettings": {
    "ProcessingDelay": "${WORKER_PROCESSING_DELAY}",
    "MaxRetryAttempts": "${WORKER_MAX_RETRY_ATTEMPTS}",
    "BatchSize": "${WORKER_BATCH_SIZE}",
    "EnableDetailedLogging": "${WORKER_DETAILED_LOGGING}"
  }
}
```

### Topics de Kafka

- **`transactions`** - Transacciones pendientes de validación
- **`transactions-status`** - Resultados de validación

## 🐛 Troubleshooting

### Problemas Comunes

1. **Error de conexión a Kafka**
   - Verificar que Docker esté corriendo
   - Ejecutar `docker-compose ps` para verificar servicios
   - Verificar variables de entorno para el ambiente correcto

2. **Error de migraciones**
   - Verificar conexión a PostgreSQL
   - Ejecutar `dotnet ef database update`
   - Verificar variables de entorno de base de datos

3. **Worker no procesa mensajes**
   - Verificar logs del Worker
   - Verificar configuración de Kafka en appsettings.json
   - Verificar consumer group ID por ambiente

4. **Puerto ya en uso**
   - Cambiar puertos en `Properties/launchSettings.json`
   - Verificar que no haya otros servicios usando los puertos

5. **Configuración incorrecta por ambiente**
   - Verificar variable `ASPNETCORE_ENVIRONMENT`
   - Verificar archivo `appsettings.{Environment}.json`
   - Verificar variables de entorno específicas del ambiente

### Comandos Útiles

```bash
# Ver logs de Docker
docker-compose logs

# Ver logs específicos
docker-compose logs kafka
docker-compose logs postgres

# Reiniciar servicios
docker-compose restart

# Limpiar y reconstruir
docker-compose down
docker-compose up -d

# Verificar puertos en uso
netstat -an | findstr :5000
netstat -an | findstr :5001

# Verificar variables de entorno
echo $ASPNETCORE_ENVIRONMENT
echo $DB_PASSWORD

# Ejecutar con ambiente específico
ASPNETCORE_ENVIRONMENT=Staging dotnet run
```

## 📝 Notas de Desarrollo

- El proyecto sigue principios de Clean Architecture
- Usa inyección de dependencias para desacoplar componentes
- Implementa patrones de mensajería asíncrona con Kafka
- Incluye validación de reglas de negocio en el Worker
- Proporciona documentación automática con Swagger
- Configuración de puertos estandarizada
- Navegador se abre automáticamente al ejecutar la API
- Configuración específica por ambiente (Development, Staging, Production)
- Variables de entorno para configuración flexible
- Logging diferenciado por ambiente

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles. 