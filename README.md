# Anti-Fraud Service

Microservicio de validación anti-fraude para transacciones financieras implementado en **.NET 8** con arquitectura limpia (Clean Architecture).

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
├── docker-compose.yml          # Configuración de contenedores
├── README.md                   # Documentación
└── .gitignore                 # Archivos ignorados por Git
```

### Tecnologías Utilizadas

- **.NET 8** - Framework principal
- **Entity Framework Core** - ORM para PostgreSQL
- **Kafka** - Mensajería asíncrona
- **PostgreSQL** - Base de datos
- **Docker** - Contenedores para servicios externos
- **Swagger** - Documentación de API

## 🚀 Instalación y Configuración

### Prerrequisitos

- .NET 8 SDK
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
