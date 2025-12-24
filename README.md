# TodoTechnicalTest Backend - Sistema de Gestión de Tareas para la prueba técnica de BEYOND HOSPITALITY

## Índice

1. [Introducción](#introducción)
2. [Guía de Inicio Rápido (Instalación)](#guía-de-inicio-rápido)
3. [URLs de Acceso y Monitorización](#urls-de-acceso-y-monitorización)
4. [Docker y Containerización](#docker-y-containerización)
5. [Arquitectura del Sistema](#arquitectura-del-sistema)
6. [Domain-Driven Design (DDD)](#domain-driven-design-ddd)
7. [Implementación del Dominio siguiendo las indicaciones de la prueba](#implementación-del-dominio-siguiendo-las-indicaciones-de-la-prueba)
8. [Reglas de Negocio](#reglas-de-negocio)
9. [Arquitectura de Microservicios](#arquitectura-de-microservicios)
10. [Behaviours del Pipeline](#behaviours-del-pipeline)
11. [CAP - Event Bus](#cap---event-bus)
12. [API Gateway y Swagger Dinámico](#api-gateway-y-swagger-dinámico)
13. [Proyectos Shared y Utilidades](#proyectos-shared-y-utilidades)
14. [Extensiones del Program.cs](#extensiones-del-programcs)
15. [SocketManagement Microservicio](#socketmanagement-microservicio)
16. [Estrategia de Testing](#estrategia-de-testing)
17. [Configuración y Variables de Entorno](#configuración-y-variables-de-entorno)
18. [Decisiones Técnicas](#decisiones-técnicas)
19. [Posibles Mejoras y Consideraciones Futuras](#posibles-mejoras-y-consideraciones-futuras)
20. [Conclusión](#conclusión)
21. [Autor](#autor)

---

## Introducción
He implementado un sistema de gestión de tareas poniendo todo el cariño en los principios de **Domain-Driven Design (DDD)** y microservicios. Mi idea no fue solo "hacer que funcione", sino crear algo escalable, limpio y mantenible.

### Objetivo del Desafío

El objetivo principal es demostrar maestría técnica en:
- **Domain-Driven Design**: Modelado del dominio con agregados, entidades y value objects
- **Arquitectura de Microservicios**: Separación de responsabilidades y comunicación entre servicios
- **Testing y Calidad**: Implementación de reglas de negocio robustas y validaciones
- **Proactividad**: Implementaciones adicionales.

---

## Guía de Inicio Rápido

Esta sección está diseñada para poner en marcha el sistema desde cero en tu máquina local.

### Requisitos Previos

*   **Docker Desktop**: Necesitas tener Docker instalado y ejecutándose.
    *   [Descargar Docker Desktop para Windows](https://docs.docker.com/desktop/install/windows-install/) (o tu sistema operativo correspondiente).

### Instalación y Compilación

He dockerizado absolutamente todo para que no tengas que instalar ni SQL Server ni SDKs (.NET 9) en tu máquina. Todo corre aislado y feliz en sus contenedores.

1.  **Clonar el repositorio** (si tienes git instalado):
    ```bash
    git clone https://github.com/andrey342/TodoTechnicalTest.git
    cd TodoTechnicalTest
    ```

2.  **Compilar y Levantar el Sistema**:
    Abre una terminal (PowerShell, CMD o Bash) en la raíz del proyecto (donde está el archivo `docker-compose.yml`) y ejecuta:

    ```bash
    docker-compose up -d --build
    ```
    
    > **Explicación**: El flag `--build` fuerza la compilación de las imágenes de Docker asegurando que tengas la última versión del código. El flag `-d` (detached) ejecuta los contenedores en segundo plano.

    También puedes hacerlo desde la interfaz de **Visual Studio**:
    - Establece el proyecto `docker-compose` como **Proyecto de Inicio** (Set as Startup Project).
    - Pulsa **Iniciar** (Start/Run) o `F5`.

3.  **Verificar el estado**:
    Puedes ver si todo ha arrancado correctamente con:
    ```bash
    docker-compose ps
    ```
    Los servicios `sqlserver`, `kafka`, `zookeeper`, `apigateway.ag`, `todomanagement.api` y `socketmanagement.api` deberían estar en estado `Healthy`.

### Ejecución de Tests (Opcional)

Si deseas validar la integridad del código ejecutando la suite de pruebas unitarias:

**Requisito**: Tener .NET 9 SDK instalado localmente.

```bash
# Tests de Dominio (Validan Reglas de negocio puras)
dotnet test src/Microservices/TodoManagement/TodoManagement.Domain.UnitTests

# Tests de API (Validan Validadores, Mappers y Handlers)
dotnet test src/Microservices/TodoManagement/TodoManagement.API.UnitTests
```

También puedes ejecutar las pruebas desde la interfaz de **Visual Studio**:
1. Haz clic derecho sobre la solución o el proyecto de tests.
2. Selecciona **Ejecutar Pruebas** (Run Tests).

---

## URLs de Acceso y Monitorización

Una vez que el sistema esté corriendo mediante Docker Compose, tendrás acceso a las siguientes herramientas y servicios:

### Puntos de Acceso Públicos (Simulado)

*   **API Gateway - Swagger Unificado**
    *   **URL**: [http://localhost:32700/swagger/index.html](http://localhost:32700/swagger/index.html)
    *   **Descripción**: Este es el **único punto de entrada** que debería usar una aplicación Frontend. Agrupa y expone las APIs de todos los microservicios subyacentes.

*   **Kafka UI**
    *   **URL**: [http://localhost:8089/](http://localhost:8089/)
    *   **Descripción**: Panel visual para administrar y monitorizar tu cluster de Kafka.
    *   **Qué ver**: Puedes ir a la sección "Topics" para ver los eventos de integración (ej: `integration.todomanagement.todoitemcreated`) y ver los mensajes en tiempo real.

### Puntos de Acceso Internos (Solo Desarrollo)

Estas URLs acceden directamente a los microservicios, saltándose el API Gateway. Útiles para debugging y ver el estado interno de CAP.

**Microservicio: TodoManagement**
*   **Swagger**: [http://localhost:32701/swagger/index.html](http://localhost:32701/swagger/index.html)
*   **CAP Dashboard**: [http://localhost:32701/cap/index.html#/](http://localhost:32701/cap/index.html#/)
    *   **Descripción**: Panel de control del Event Bus. Muestra los eventos publicados (Published) y recibidos (Received) por este servicio específico, incluyendo reintentos y errores.

**Microservicio: SocketManagement**
*   **Swagger**: [http://localhost:32702/swagger/index.html](http://localhost:32702/swagger/index.html)
    *   **Descripción**: Aparece vacío porque no tiene APIs, solo tiene un socket para la comunicación con el frontend.
*   **CAP Dashboard**: [http://localhost:32702/cap/index.html#/](http://localhost:32702/cap/index.html#/)

---

## Docker y Containerización

Como comentaba, Docker Compose es el director de orquestra aquí. Gestiona todos los servicios del despliegue.

### Arquitectura de Contenedores

El sistema está completamente containerizado y se compone de los siguientes servicios:

#### Servicios de Infraestructura

1. **SQL Server** (`sqlserver`)
   - Imagen: `mcr.microsoft.com/mssql/server:2022-latest`
   - Puerto: `1433` (mapeado al host)
   - Base de datos: `TodoManagementDb`
   - Health check configurado para verificar el estado del servidor

2. **Zookeeper** (`zookeeper`)
   - Imagen: `bitnamilegacy/zookeeper:3.9.3-debian-12-r22`
   - Servicio de coordinación para Kafka
   - Permite login anónimo para desarrollo

3. **Kafka** (`kafka`)
   - Imagen: `bitnamilegacy/kafka:3.3.1-debian-11-r9`
   - Puerto interno: `9092`
   - Configurado para comunicación con Zookeeper
   - Health check para verificar que los topics están disponibles

4. **Kafka UI** (`kafka-ui`)
   - Imagen: `provectuslabs/kafka-ui:latest`
   - Interfaz web para gestión y monitoreo de Kafka
   - Puerto desarrollo: `8089` (configurado en override)
   - Permite visualizar topics, consumidores y mensajes

#### Servicios de Aplicación

5. **API Gateway** (`apigateway.ag`)
   - Construido desde `src/ApiGateways/ApiGateway.AG/Dockerfile`
   - Puerto: `32700` (mapeado desde `8080` interno)
   - Dependencias: Kafka
   - Variables de entorno para autenticación y configuración de CAP

6. **TodoManagement API** (`todomanagement.api`)
   - Construido desde `src/Microservices/TodoManagement/TodoManagement.API/Dockerfile`
   - Puerto desarrollo: `32701` (configurado en override)
   - Dependencias: SQL Server, Kafka, API Gateway
   - Health check para verificar el estado del servicio
   - Política de reinicio: `unless-stopped`

7. **SocketManagement API** (`socketmanagement.api`)
   - Construido desde `src/Microservices/SocketManagement/SocketManagement.API/Dockerfile`
   - Puerto desarrollo: `32702` (configurado en override)
   - Dependencias: Kafka, API Gateway
   - Health check para verificar el estado del servicio
   - Política de reinicio: `unless-stopped`

### Configuración de Docker Compose

#### `docker-compose.yml`

Archivo principal que define todos los servicios y su configuración base:

**Características principales**:
- **Red personalizada**: `todotechnicaltest_backend` (bridge network) para aislar la comunicación entre servicios
- **Health checks**: Configurados para SQL Server, Kafka y los servicios de aplicación
- **Variables de entorno**: Configuración externa mediante variables de entorno
- **Dependencias**: Orden de inicio correcto mediante `depends_on`

**Estructura de servicios**:
- Servicios de infraestructura primero (SQL Server, Zookeeper, Kafka)
- Servicios de aplicación después (API Gateway, TodoManagement API, SocketManagement API)

#### `docker-compose.override.yml`

Archivo de override específico para desarrollo que modifica la configuración base:

**Configuraciones de desarrollo**:
- **Kafka UI**: Expone el puerto `8089` para acceso desde el host
- **TodoManagement API**: Expone el puerto `32701` para acceso directo al servicio, incluyendo:
  - CAP Dashboard (disponible en desarrollo)
  - Endpoints de debugging
  - Swagger UI
  - Health checks
- **SocketManagement API**: Expone el puerto `32702` para acceso directo al servicio, incluyendo:
  - CAP Dashboard
  - Endpoints de debugging
  - Swagger UI
  - Health checks

**Uso**:
Este archivo se carga automáticamente en desarrollo y permite personalizar puertos y configuraciones sin modificar el archivo principal. Para producción, claramente este archivo no debería incluirse o debería tener valores diferentes.

### Variables de Entorno

El sistema utiliza variables de entorno para configurar:

- **SQL Server**: Usuario y contraseña
- **Kafka**: Bootstrap servers y configuración del broker
- **API Gateway**: Configuración de autenticación, authority, audience, etc.
- **Microservicios**: Connection strings, nombres de servicio, URLs base

### Ventajas de la Containerización

1. **Reproducibilidad**: El entorno es idéntico en desarrollo, testing y producción
2. **Aislamiento**: Cada servicio corre en su propio contenedor con dependencias aisladas
3. **Escalabilidad**: Fácil escalado horizontal de servicios individuales
4. **Portabilidad**: Funciona en cualquier sistema que soporte Docker
5. **Desarrollo simplificado**: Un simple `docker-compose up` inicia todo el ecosistema

### Comandos Útiles

```bash
# Iniciar todos los servicios
docker-compose up -d

# Iniciar servicios y reconstruir imágenes
docker-compose up -d --build

# Ver logs de todos los servicios
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f todomanagement.api

# Detener todos los servicios
docker-compose down

# Detener y eliminar volúmenes
docker-compose down -v

# Ver estado de los servicios
docker-compose ps
```

### Health Checks

Todos los servicios críticos incluyen health checks:
- **SQL Server**: Verifica que el proceso `sqlservr` está corriendo
- **Kafka**: Verifica que los topics están disponibles
- **Servicios de aplicación**: Verifican el endpoint `/healthz`

Esto permite que Docker Compose gestione correctamente las dependencias y reinicios.

---

## Arquitectura del Sistema

El sistema está organizado en una arquitectura de microservicios con los siguientes componentes principales:

```
TodoTechnicalTest/
├── src/
│   ├── Microservices/
│   │   ├── TodoManagement/
│   │   │   ├── TodoManagement.API/          # Aplicación y API de Tareas
│   │   │   ├── TodoManagement.Domain/       # Dominio y lógica de negocio
│   │   │   └── TodoManagement.Infrastructure/ # Persistencia y acceso a datos
│   │   └── SocketManagement/
│   │       └── SocketManagement.API/       # Microservicio para SignalR y Tiempo Real
│   ├── ApiGateways/
│   │   └── ApiGateway.AG/                   # API Gateway con YARP (Único punto de entrada)
│   └── Shared/
│       ├── Contracts/                        # Contratos compartidos
│       └── EventBus/                         # Event Bus con CAP
```

### Separación de Capas

1. **Domain Layer**: Contiene las entidades del dominio, agregados, value objects y reglas de negocio
2. **Infrastructure Layer**: Implementa la persistencia (Entity Framework), repositorios y servicios de infraestructura
3. **API Layer**: Expone los endpoints REST, maneja comandos/queries con Mediator, y coordina las operaciones

---

## Domain-Driven Design (DDD)

### Agregados y Entidades

El sistema implementa el patrón de agregados de DDD:

#### **TodoList (Aggregate Root)**
- **Responsabilidad**: Actúa como raíz del agregado y punto de entrada único para todas las operaciones sobre TodoItems
- **Invariantes**: 
  - Gestiona la colección de TodoItems
  - Implementa `ITodoList` para exponer operaciones de negocio

#### **TodoItem (Entity)**
- **Responsabilidad**: Representa una tarea individual con su historial de progreso
- **Propiedades Clave**:
  - `ItemId`: Identificador de negocio (secuencial)
  - `Title`, `Description`, `Category`: Información de la tarea
  - `Progressions`: Colección de registros de progreso
  - `IsCompleted`: Propiedad calculada (progreso total >= 100%)

#### **Progression (Entity)**
- **Responsabilidad**: Representa un registro de avance en un TodoItem
- **Propiedades**:
  - `ActionDate`: Fecha del registro
  - `Percent`: Porcentaje incremental de progreso

### Value Objects y SeedWork

El proyecto incluye clases base reutilizables en `SeedWork`:
- **Entity**: Clase base para todas las entidades con gestión de eventos de dominio
- **ValueObject**: Base para objetos de valor inmutables
- **IAggregateRoot**: Marcador para raíces de agregado
- **IUnitOfWork**: Patrón para gestión de transacciones

---

## Implementación del Dominio siguiendo las indicaciones de la prueba

### Interfaz ITodoList

La interfaz `ITodoList` define el contrato establecido en la prueba:

```csharp
public interface ITodoList 
{
    void AddItem(int id, string title, string description, string category);
    void UpdateItem(int id, string description);
    void RemoveItem(int id);
    void RegisterProgression(int id, DateTime dateTime, decimal percent);
    void PrintItems();
}
```

**Implementación**: La interfaz se implementa directamente en `TodoList`.

### Interfaz ITodoListRepository

Extendida con métodos específicos del dominio:

```csharp
public interface ITodoListRepository : ICommandRepository<TodoList>
{
    int GetNextId();
    List<string> GetAllCategories();
}
```

**Implementación**:
- `GetNextId()`: Retorna el siguiente ID disponible calculando `MAX(ItemId) + 1` de forma global para todos los items
- `GetAllCategories()`: Retorna una lista predefinida de categorías válidas (implementadas como un Master estático en el Dominio para simplificar, ver `CategoryMaster.cs`)

---

## Reglas de Negocio

El sistema implementa las siguientes reglas de negocio críticas:

### 1. Validación de Fechas en Progression

**Regla**: Al añadir una nueva progresión, su fecha debe ser **posterior** a la fecha de la última progresión existente.

**Implementación**: En `TodoItem.AddProgression()`:
```csharp
if (_progressions.Any())
{
    var lastProgression = _progressions.OrderByDescending(p => p.ActionDate).First();
    if (actionDate <= lastProgression.ActionDate)
    {
        throw new ArgumentException(...);
    }
}
```

### 2. Validación de Porcentaje en Progression

**Reglas**:
- El porcentaje debe ser **mayor que 0**
- La suma total de todos los porcentajes no puede superar el **100%**

**Implementación**:
```csharp
if (percent <= 0)
    throw new ArgumentException("El porcentaje debe ser mayor que 0.");

var currentTotal = GetTotalProgress();
if (currentTotal + percent > 100m)
    throw new ArgumentException("El progreso total superaría el 100%.");
```

### 3. Restricción de Modificación

**Regla**: No se permite actualizar o eliminar un TodoItem si su progreso total acumulado **supera el 50%**.

**Implementación**: En `TodoList.UpdateItem()` y `RemoveItem()`:
```csharp
if (!item.CanBeModified())
{
    throw new InvalidOperationException(
        $"No se puede modificar porque el progreso ({item.GetTotalProgress()}%) supera el 50%.");
}
```

### 4. IsCompleted Calculado

**Regla**: Un TodoItem está completado cuando su progreso total acumulado es >= 100%.

**Implementación**: Propiedad calculada en `TodoItem`:
```csharp
public bool IsCompleted => GetTotalProgress() >= 100m;
```

### 5. Validación de Categoría

**Regla**: Solo se permiten categorías predefinidas en el sistema (Master Data).

**Implementación**:
```csharp
if (!Masters.CategoryMaster.IsValidCategory(category))
{
    throw new ArgumentException($"Category '{category}' is invalid...");
}
```

---

## Arquitectura de Microservicios

### Separación CQRS

He separado las lecturas de las escrituras usando **Command Query Responsibility Segregation (CQRS)**. ¿Por qué? Para poder escalar y optimizar cada lado independientemente y mantener el código ordenado.

- **Command Repositories** (`ICommandRepository<T>`): Para operaciones de escritura
- **Query Repositories** (`IQueryRepository<T>`): Para operaciones de lectura
- **Validation Repositories** (`IValidationOnlyRepository<T>`): Para operaciones de solo lectura optimizadas para validaciones
- **Separación Física**: Los queries están en la capa API, mientras que los commands están en Infrastructure

**Ventajas**:
- Escalabilidad independiente de lecturas y escrituras
- Optimización específica para cada tipo de operación
- Separación clara de responsabilidades

#### ICommandRepository<T>

Interfaz para operaciones de escritura en agregados raíz. Hereda de `IBaseRepository<T>` y proporciona:

**Métodos**:
- `IUnitOfWork UnitOfWork { get; }`: Obtiene la unidad de trabajo asociada al repositorio
- `Task AddAsync(T entity, CancellationToken cancellationToken = default)`: Añade una nueva entidad
- `Task UpdateAsync(T entity, CancellationToken cancellationToken = default)`: Actualiza una entidad existente
- `Task DeleteAsync(T entity, CancellationToken cancellationToken = default)`: Elimina una entidad

**Hereda de IBaseRepository**:
- `Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)`: Obtiene una entidad por su ID
- `Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)`: Verifica si existe una entidad

**Uso**: Se utiliza para todas las operaciones que modifican el estado del sistema (crear, actualizar, eliminar).

#### IBaseRepository<T>

Interfaz base que proporciona funcionalidades comunes de lectura y verificación:

**Métodos Principales**:
- `Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)`: Obtención simple por ID.
- `Task<T> GetByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>> includes, CancellationToken cancellationToken = default)`: Permite especificar relaciones (`Includes`) para carga ansiosa (Eager Loading) al obtener por ID.
- `Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)`: Verificación eficiente de existencia.

#### IQueryRepository<T>

Interfaz para operaciones de lectura optimizadas. Hereda de `IBaseRepository<T>` y proporciona:

**Métodos**:
- `Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IQueryable<T>> includes = null, CancellationToken cancellationToken = default)`: Obtiene todas las entidades que coinciden con el filtro
- `Task<PaginatedResult<T>> GetPagedAsync(int pageIndex, int pageSize, Expression<Func<T, object>> orderBy, bool orderByDescending = false, Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IQueryable<T>> includes = null, CancellationToken cancellationToken = default)`: Obtiene resultados paginados
- `Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IQueryable<T>> includes = null, CancellationToken cancellationToken = default)`: Obtiene la primera entidad que coincide con el filtro
- `Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)`: Obtiene entidades usando especificaciones

**Hereda de IBaseRepository**:
- `Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)`: Obtiene una entidad por su ID
- `Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)`: Verifica si existe una entidad

**Uso**: Se utiliza para todas las operaciones de lectura que no modifican el estado del sistema.

#### IValidationOnlyRepository<T>

Interfaz optimizada para operaciones de validación y verificación de existencia. Diseñada específicamente para escenarios de validación:

**Métodos**:
- `ValueTask<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)`: Verifica si existe una entidad por su ID (optimizado con `AsNoTracking`)
- `ValueTask<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)`: Verifica si existe alguna entidad que coincida con el filtro (optimizado con `AsNoTracking`)

**Características**:
- Usa `AsNoTracking()` para mejor rendimiento en operaciones de solo lectura
- Retorna `ValueTask<bool>` para optimización de memoria
- Diseñado exclusivamente para validaciones y lookups

**Uso**: Se utiliza en validadores de FluentValidation para verificar existencia de entidades sin cargar datos completos, mejorando el rendimiento.

**Separación de Interfaces**:
- **Commands**: `ICommandRepository<T>` - Para escritura
- **Queries**: `IQueryRepository<T>` - Para lectura completa
- **Validations**: `IValidationOnlyRepository<T>` - Para validaciones y verificaciones de existencia optimizadas

### Mediator Pattern

Se utiliza **Mediator.Abstractions** para desacoplar los handlers de comandos/queries:

- **Commands**: Operaciones que modifican el estado
- **Queries**: Operaciones que solo leen datos
- **Handlers**: Procesan las solicitudes de forma desacoplada

**Beneficios**:
- Reducción del acoplamiento
- Facilita el testing
- Permite agregar comportamientos transversales (behaviours)

---

## Behaviours del Pipeline

El sistema implementa un pipeline de comportamientos transversales usando el patrón **Chain of Responsibility**:

### 1. LoggingBehavior

**Propósito**: Registra todas las solicitudes y respuestas del sistema.

**Orden**: Más externo (primero en ejecutarse)

**Implementación**: 
- Registra el tipo de request y sus datos antes de procesar
- Registra la respuesta después del procesamiento

**Beneficio**: Trazabilidad completa de las operaciones para debugging y auditoría.

### 2. IdempotencyBehavior

**Propósito**: Garantiza la idempotencia de las operaciones.

**Orden**: **Segundo** en el pipeline

**Implementación**:
- Verifica si la solicitud ya fue procesada usando `IRequestManager`
- Si ya fue procesada, retorna la respuesta almacenada
- Si no, procesa y almacena la respuesta
- Se ejecuta antes de la validación para evitar re-validar solicitudes ya procesadas y exitosas.

**Beneficio**: Previene procesamiento duplicado y optimiza el rendimiento en reintentos.

### 3. ValidationBehavior

**Propósito**: Valida las solicitudes usando **FluentValidation**.

**Orden**: Tercero en el pipeline

**Implementación**:
- Ejecuta todos los validadores en paralelo
- Recolecta todos los errores de validación
- Lanza `ValidationException` si hay errores

**Beneficio**: Validación centralizada y consistente antes de procesar la lógica de negocio.

**BaseValidator**:
Se utiliza una clase base `BaseValidator<T>` que simplifica drásticamente la creación de validadores mediante métodos genéricos predefinidos como:
- `ValidateUniqueness`: Verifica unicidad en BD.
- `ValidateExists`: Verifica existencia de claves foráneas.
- `Require`: Validaciones de obligatoriedad estándar.

### 4. TransactionBehavior

**Propósito**: Gestiona transacciones de base de datos a nivel de aplicación.

**Orden**: Más interno (último antes del handler)

**Implementación**:
- Inicia una transacción antes de procesar el comando
- Si hay éxito, hace commit
- Si hay error, hace rollback

**Diferencia con UnitOfWork**: 
- **UnitOfWork**: Gestiona transacciones dentro de un contexto/repositorio
- **TransactionBehavior**: Gestiona transacciones a nivel de aplicación, incluyendo lógica de negocio y coordinación entre múltiples repositorios

**Beneficio**: Garantiza consistencia transaccional en operaciones complejas.

**Persistencia y SaveEntities**:
Aunque el `TransactionBehavior` realiza el commit de la transacción (lo que persiste los cambios mediante `SaveChangesAsync`), el método `IUnitOfWork.SaveEntitiesAsync` es el encargado de despachar los **Eventos de Dominio**.
- Normalmente el flujo confirma la transacción y guarda cambios.
- Si se requiere asegurar el despacho de eventos de dominio antes del commit o en un punto específico, se puede forzar en el handler llamando a:
  ```csharp
  await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
  ```

---

## CAP - Event Bus

### ¿Qué es CAP?

**CAP (DotNetCore.CAP)** es una librería que proporciona:
- **Event Bus**: Sistema de mensajería para comunicación entre microservicios
- **Outbox Pattern**: Patrón para garantizar consistencia entre base de datos y mensajería
- **Inbox Pattern**: Patrón para procesamiento idempotente de eventos

### Configuración en el Sistema

El sistema utiliza CAP con **Kafka** como broker de mensajería:

```csharp
services.AddCapForMicroserviceStateful<TodoManagementContext>(configuration, env, logger);
```

**Características**:
- **Stateful**: Usa Entity Framework Core como almacenamiento para outbox/inbox
- **Kafka**: Broker de mensajería para distribución de eventos
- **Compresión Automática**: Eventos grandes (>0.9MB) se comprimen automáticamente
- **Retry Policy**: 5 reintentos con expiración de mensajes fallidos después de 7 días

### Uso en el Sistema

1. **Publicación de Eventos**: Los microservicios publican eventos de integración usando `IEventBus`
2. **Consumo de Eventos**: Los servicios suscritos procesan eventos mediante `[CapSubscribe]`
3. **Transacciones Distribuidas**: CAP garantiza que los eventos se publiquen solo si la transacción de BD se completa

**Ejemplo de Publicación**:
```csharp
await _eventBus.PublishAsync(new TodoItemCreatedEvent(...));
```

**Ejemplo de Consumo**:
```csharp
[CapSubscribe("integration.todomanagement.todoitemcreated")]
public void HandleTodoItemCreated(TodoItemCreatedEvent evt) { ... }
```

---

## API Gateway y Swagger Dinámico

### Arquitectura del API Gateway

El sistema implementa un **API Gateway** usando **YARP (Yet Another Reverse Proxy)**:

**Componentes**:
1. **DynamicYarpProvider**: Proveedor dinámico de configuración de rutas
2. **GatewayRoutesConsumer**: Consumidor de eventos de rutas desde Kafka
3. **SwaggerFragmentStore**: Almacén de fragmentos Swagger de cada microservicio
4. **RouteStore**: Almacén de rutas configuradas

### Sistema de Swagger mediante Kafka

**Flujo de Funcionamiento**:

1. **Publicación al Inicio**: Cada microservicio publica sus rutas al iniciar mediante `GatewayRoutesPublisher`
   - Analiza la documentación Swagger
   - Filtra operaciones marcadas con `x-include-in-gateway: true`
   - Publica eventos `GatewayRoutesEvent` a Kafka

2. **Consumo en el Gateway**: El API Gateway consume estos eventos
   - Actualiza el `RouteStore` con las nuevas rutas
   - Actualiza el `SwaggerFragmentStore` con los fragmentos Swagger
   - Reconstruye la configuración de YARP dinámicamente

3. **Swagger Unificado**: El gateway expone un Swagger unificado que combina todos los fragmentos

**Ventajas**:
- **Descubrimiento Automático**: Los microservicios se auto-registran
- **Swagger Centralizado**: Un solo punto de acceso para toda la documentación
- **Escalabilidad**: Nuevos microservicios se integran automáticamente
- **Sin Reinicios**: El gateway se actualiza dinámicamente sin reiniciar

**Control de Publicación (Metadata)**:
Se utiliza el atributo `IncludeInGatewayAttribute` para controlar granularmente la exposición:
- Si no se especifica `Targets`, la API se publica en **TODOS** los gateways configurados en el enum `GatewayTarget`.
- Se puede restringir a un gateway específico (ej: `targets: [ApiGateway]`) como se ha hecho en algunas APIs de ejemplo.
- Permite decidir endpoint por endpoint qué se expone y dónde.

### ¿Por qué un API Gateway (AG) y no exponer cada Microservicio?

En este sistema, el **API Gateway (AG)** actúa como el único punto de entrada hacia la red interna de microservicios.

**Ventajas Técnicas y de Seguridad**:
1.  **Swagger Unificado y NSWAG**: El AG agrupa todos los Swaggers de los microservicios en un único endpoint. Esto permite que el Frontend utilice la librería **NSWAG** para generar automáticamente el `api-client` completo sin tener que gestionar múltiples URLs.
2.  **Seguridad de Red**: Solo el AG es accesible desde el exterior. En un entorno real, los microservicios no tendrían puertos públicos, reduciendo drásticamente la superficie de ataque. 
    > [NOTA!]
    > En este proyecto de desarrollo, se han asignado puertos a los microservicios para facilitar las pruebas, pero en producción estos estarían aislados.
3.  **Simplificación de Mantenimiento**: Centralizamos la configuración de **CORS**, autenticación, rate limiting y logging en un solo sitio, evitando la repetición de código y lógica en cada microservicio.
4.  **Abstracción de Rutas**: El front-end solo conoce la URL del AG. El redireccionamiento interno mediante YARP permite mover o escalar microservicios sin que el cliente tenga que cambiar su configuración.

**Seguridad**:
El API Gateway tiene implementada autenticación JWT, pero **se encuentra desactivada en entorno de Desarrollo** para facilitar el debugging y las pruebas manuales sin necesidad de tokens.

---

## Proyectos Shared y Utilidades

### Global Usings
Para mantener el código limpio y reducir el "ruido" en los archivos, se utilizan archivos `GlobalUsings.cs` en cada proyecto (API, Domain, Infrastructure). Esto centraliza las importaciones comunes y reduce el tamaño de los archivos de código.

### Shared/Contracts

**Propósito**: Contratos compartidos entre microservicios.

**Contenido**:
- **IntegrationEvent**: Clase base para eventos de integración
- **GatewayRoutesEvent**: Evento para comunicación de rutas con el gateway
- **TodoListReportGeneratedIntegrationEvent**: Evento de integración para generar informes de listas de tareas

**Ventaja**: Evita duplicación de código y garantiza compatibilidad entre servicios.

### Shared/EventBus

**Propósito**: Abstracción y implementación del event bus.

**Componentes**:
- **IEventBus**: Interfaz abstracta para publicación de eventos
- **CapEventBus**: Implementación usando CAP
- **IEventSerializer**: Serialización de eventos (JSON)
- **CapExtensions**: Extensiones para configuración de CAP

**Ventaja**: Desacoplamiento del sistema de mensajería, permitiendo cambiar la implementación sin afectar el código de negocio.

---

## Extensiones del Program.cs

El sistema utiliza extensiones para mantener el `Program.cs` del Microservicio **TodoManagement** limpio y organizado:

### DependencyInjectionExtensions

**Método Principal**: `AddCustomServices()`

**Registra**:
1. **DbContext**: Configuración de Entity Framework con SQL Server
2. **Migraciones**: Extensión para aplicar migraciones automáticamente (ver sección inferior)
3. **CAP**: Configuración del event bus
4. **Repositorios**: Registro automático usando Scrutor
5. **Mediator**: Configuración con behaviours
6. **FluentValidation**: Validadores automáticos
7. **Mappers**: Mapeo automático (Riok.Mapperly)
8. **GatewayRoutesPublisher**: Servicio en background para publicar rutas

**Ventaja**: Separación de responsabilidades y código más mantenible.

### Manejo Automático de Migraciones

Se ha implementado una extensión personalizada `MigrateDbContextExtensions.cs` que automatiza el ciclo de vida de la base de datos:

1.  **AddMigration<TContext>**: Registra un `BackgroundService` que se ejecuta al arrancar la aplicación.
2.  **EnsureCreated / Migrate**: El servicio se encarga de verificar si la DB existe, crearla y aplicar las migraciones pendientes antes de que el microservicio empiece a recibir tráfico.
3.  **Seeding**: Permite inyectar un `IDbSeeder` para poblar la base de datos con datos maestros iniciales de forma controlada.

Esto garantiza que cualquier desarrollador pueda clonar el repo y ejecutar `docker-compose up` sin tener que ejecutar comandos manuales de Entity Framework.

---

## SocketManagement Microservicio

Este microservicio se encarga de la comunicación en tiempo real con el exterior (Frontend) utilizando **SignalR**.

### Responsabilidades
- **Gestión de Conexiones**: Mantiene los sockets abiertos con los clientes.
- **Consumo de Eventos**: Escucha eventos de integración del sistema que requieren notificación inmediata.
- **Broadcasting**: Redirige la información de los eventos a los clientes de SignalR correspondientes.

### Implementación Técnica
- **SignalR Hubs**: Implementa `PrintHub` para notificaciones de reportes.
- **Consumidores CAP**: Procesa eventos como `TodoListReportGeneratedIntegrationEvent`.
- **Manejo de Payloads Grandes**: Capacidad para procesar eventos comprimidos (Gzip) cuando el reporte supera el tamaño estándar de mensaje de Kafka.

---

## Estrategia de Testing

El proyecto cuenta con una suite de pruebas robusta que garantiza la calidad y corrección del código tanto a nivel de dominio como de aplicación.

### Tecnologías y Librerías

Se utiliza el siguiente stack tecnológico para las pruebas:

*   **xUnit**: Framework de pruebas unitarias principal.
*   **FluentAssertions**: Librería para escribir aserciones fluidas y legibles (ej: `result.Should().BeTrue()`).
*   **Moq**: Framework de mocking para aislar dependencias/interfaces en los tests.
*   **Coverlet**: Herramienta para medir la cobertura de código.

### Estructura de Pruebas

#### 1. TodoManagement.Domain.UnitTests
Este proyecto se centra en validar la **Lógica Pura del Dominio** sin dependencias externas.
*   **Objetivo**: Asegurar que las reglas de negocio e invariantes se cumplan estrictamente.
*   **Cobertura Principal**:
    *   **Invariantes de Entidad**: Valida que no se pueda crear un estado inválido (ej. progresiones con fechas no secuenciales).
    *   **Lógica de Negocio Compleja**:
        *   Restricción de modificación (Update/Delete) si el progreso > 50%.
        *   Cálculo automático de `IsCompleted`.
        *   Validación de porcentajes acumulados (no exceder 100%).
    *   **Ejemplo**: `TodoListTests.cs` verifica casos como añadir progresiones con fechas pasadas, porcentajes negativos, o intentar borrar tareas avanzadas.

#### 2. TodoManagement.API.UnitTests
Este proyecto valida la **Capa de Aplicación y API**.
*   **Objetivo**: Asegurar que los flujos de entrada, validación y orquestación funcionen correctamente.
*   **Cobertura Principal**:
    *   **Validadores (FluentValidation)**: Tests específicos para asegurar que requests inválidos sean rechazados antes de procesar (ej. `CreateTodoItemCommandValidator`).
    *   **Command Handlers**: Se utiliza **Moq** para simular repositorios y verificar que los handlers orquestan correctamente la lógica (llaman al repositorio, publican eventos, etc.).

---

## Configuración y Variables de Entorno

> [IMPORTANTE!]  
> Se ha incluido el archivo `.env` en el repositorio **únicamente para facilitar la ejecución de esta prueba técnica**.  
> En un proyecto real y profesional, el archivo `.env` **NUNCA** se subiría al control de versiones, sino que se gestionaría mediante secretos (Azure Key Vault, GitHub Secrets, etc.).

---

## Decisiones Técnicas

### 1. ¿Por qué DDD?

**Razón**: El dominio de gestión de tareas tiene reglas de negocio complejas que deben estar encapsuladas y protegidas. DDD permite:
- Un único punto de entrada para todas las operaciones
- Modelar el dominio de forma clara y expresiva
- Proteger las invariantes del dominio
- Facilitar el testing de la lógica de negocio

### 2. ¿Por qué CQRS?

**Razón**: Separar comandos y queries permite:
- Optimizar lecturas y escrituras independientemente
- Escalar cada tipo de operación según necesidad
- Mantener el código más organizado

### 3. ¿Por qué CAP en lugar de otros event buses?

**Razón**: CAP proporciona:
- Integración nativa con Entity Framework (Outbox Pattern)
- Soporte para múltiples brokers (Kafka, RabbitMQ, etc.)
- Manejo automático de reintentos y fallos
- Dashboard para monitoreo
- Reduce el código de implementación, el tiempo de desarrollo y la mantenibilidad

### 4. ¿Por qué YARP para el API Gateway?

**Razón**: YARP es:
- Nativo de .NET (mejor rendimiento)
- Altamente configurable
- Soporta configuración dinámica
- Integración perfecta con ASP.NET Core

### 5. ¿Por qué Mediator Pattern?

**Razón**: Permite:
- Desacoplar handlers de controladores
- Agregar comportamientos transversales fácilmente
- Facilitar el testing
- Mantener el código limpio y organizado

### 6. Implementación de PrintItems

El método `PrintItems()` genera salida formateada en consola con:
- Ordenamiento por `ItemId`
- Formato específico: `{ItemId}) {Title} - {Description} ({Category}) Completed:{IsCompleted}`
- Barras de progreso visuales con porcentaje acumulado
- Formato de fecha: `M/d/yyyy hh:mm:ss tt`

**Razón**: Cumple con el requerimiento específico de la prueba técnica manteniendo.
**Nota**: Cuando se registra una progresión (`RegisterProgression`), el sistema invoca automáticamente `PrintItems()` para mostrar en la consola el estado actualizado de la lista y sus barras de progreso.

### 7. Generación de Archivo de PrintItems

**Desafío**: El método `PrintItems()` de la interfaz `ITodoList` retorna `void` y se requiere una API que genere un archivo con el contenido de `PrintItems` y lo envíe mediante un evento de integración hasta el cliente final.

**Solución**: Se ha completado el flujo utilizando **Domain Events** y un microservicio de Notificaciones/WebSockets (`SocketManagement`) para evitar la modificación de la interfaz ITodoList.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           FLUJO DE EJECUCIÓN COMPLETO                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. API recibe POST /todoList/printItemsFile                                │
│     ▼                                                                       │
│  2. GenerateTodoListReportCommandHandler (TodoManagement.API)               │
│     - Llama a todoList.PrintItems()                                         │
│     ▼                                                                       │
│  3. TodoList.PrintItems() (TodoManagement.Domain)                           │
│     - Genera contenido y emite ItemsPrintedDomainEvent                      │
│     ▼                                                                       │
│  4. ItemsPrintedDomainEventHandler (TodoManagement.API)                     │
│     - Publica TodoListReportGeneratedIntegrationEvent al Event Bus          │
│     ▼                                                                       │
│  5. CAP (Event Bus) distribuye el evento                                    │
│     ▼                                                                       │
│  6. TodoListReportConsumer (SocketManagement.API)                           │
│     - Consume el evento de integración (Raw o Gzipped)                      │
│     ▼                                                                       │
│  7. SignalR Hub (PrintHub)                                                  │
│     - Envía el contenido del archivo (Base64) a los clientes conectados     │
│     ▼                                                                       │
│  8. Frontend (Cliente)                                                      │
│     - Recibe el evento "PrintItems" y muestra/descarga el reporte           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Beneficios de este flujo**:
1. **Desacoplamiento**: El microservicio de tareas no sabe nada de WebSockets.
2. **Escalabilidad**: El manejo de miles de conexiones concurrentes recae sobre un MS especializado (`SocketManagement`).
3. **Resiliencia**: Si el servicio de sockets está caído, CAP reintentará el envío cuando vuelva a estar online.

**Por qué Domain Events**:
1. **No modifica la interfaz `ITodoList`**: La firma sigue siendo `void PrintItems()`
2. **No duplica código**: La lógica de formateo está solo en el dominio
3. **Sincronización automática**: Si `PrintItems` cambia, el archivo generado también
4. **Sigue principios DDD**: Los eventos de dominio son la forma correcta de comunicar efectos secundarios
5. **Transaccionalidad**: El evento se publica dentro de la misma transacción (garantizado por CAP)

---

### 8. ¿Por qué Microservicios en lugar de un Monolito?

**Razón**: Aunque este proyecto técnicamente podría implementarse perfectamente como un monolito, he elegido la arquitectura de microservicios por varias razones estratégicas y personales.

**Demostración de Capacidad Técnica**:
- Los microservicios requieren un entendimiento profundo de arquitectura distribuida, comunicación entre servicios, gestión de transacciones distribuidas y patrones avanzados como Event Sourcing, CQRS y el patrón Outbox
- Permite demostrar competencia en tecnologías modernas como Kafka, API Gateways, service discovery, y gestión de configuración distribuida
- Muestra capacidad para diseñar sistemas escalables desde el inicio, considerando futuras necesidades de crecimiento y distribución geográfica

**Eficiencia y Escalabilidad**:
- Cada microservicio puede escalarse independientemente según sus necesidades específicas (por ejemplo, el servicio de queries puede necesitar más réplicas que el servicio de comandos)
- Permite optimizar cada servicio con tecnologías y estrategias específicas para su dominio
- Facilita el despliegue continuo sin afectar todo el sistema cuando se modifica un servicio específico
- Mejora la resiliencia: un fallo en un servicio no necesariamente afecta a todos los demás

**Pasión por Microservicios y DDD**:
- Los microservicios y Domain-Driven Design están intrínsecamente relacionados: cada microservicio típicamente representa un Bounded Context bien definido
- Esta arquitectura permite aplicar DDD de forma más pura, con agregados claramente definidos y límites de contexto explícitos
- Me apasiona profundamente el diseño de sistemas distribuidos y las buenas prácticas que se requieren: gestión de eventos de dominio, consistencia eventual, idempotencia, versionado de APIs, y observabilidad distribuida

**Buenas Prácticas**:
- Los microservicios fuerzan a pensar en la comunicación entre servicios desde el inicio, lo que lleva a mejores decisiones de diseño
- Implementar patrones como el API Gateway, service mesh, y circuit breakers mejora la robustez del sistema
- La separación clara de responsabilidades facilita el mantenimiento y la evolución del código a largo plazo

**Aclaración Importante**: 
Reconozco que para este desafío técnico específico, un monolito sería completamente válido y más simple de implementar. Sin embargo, dado que el objetivo es demostrar capacidad técnica y maestría, he elegido mostrar mi expertise en arquitecturas más complejas y modernas. Además, este es un trabajo que me recompensa y me motiva: mejorar y perfeccionar mis microservicios a lo largo de mi carrera es algo que disfruto profundamente. Los microservicios no son siempre la solución correcta, pero en este contexto me permiten demostrar un conjunto más amplio de habilidades técnicas y de arquitectura, mientras continúo refinando mi conocimiento y experiencia en este campo que tanto me apasiona.

### 9. Categorías Estáticas (CategoryMaster)

**Decisión**: Uso de una lista estática en código (`CategoryMaster`) en lugar de una tabla en base de datos.
**Razón**:
- **Foco en el Dominio**: Permite concentrar el esfuerzo en reglas de negocio complejas (progresiones, fechas) en lugar de CRUDs básicos.
- **Validación Fuerte**: Las categorías son conocidas en tiempo de compilación y validadas estrictamente por el dominio.

---

### 10. Agrupación de Servicios (TodoManagementServices)

**Decisión**: Uso del patrón "Parameter Object" para agrupar servicios comunes (`IMediator`, `ITodoListQueries`, `TodoListMapper`) en una clase `TodoManagementServices`.

**Razón**: 
- Reduce la complejidad de los constructores y firmas de métodos en la API.
- Facilita la inyección de dependencias transversales en todos los endpoints.
- Mantiene el código de los endpoints limpio y centrado en la lógica de request/response.

### 11. Estrategia de Identificadores (GUID vs ItemId)

**Decisión**: Uso dual de identificadores para satisfacer tanto necesidades de la prueba técnica como de los propios microservicios.

- **Id (GUID)**: Identificador único del sistema (Primary Key). Generado automáticamente por la entidad base (`Entity`). Cumple con las recomendaciones de DDD para identidad global única.
- **ItemId (int)**: Identificador de negocio legible y secuencial.

**Estado Actual**:
- El `ItemId` actúa como un **contador global** único para todos los items del sistema (`_repository.GetNextId()`).

### 12. Domain Events y Unit of Work

**Implementación Actual**:
- ✅ **Domain Events**: El sistema implementa el patrón de Domain Events mediante la clase base `Entity` que mantiene una colección de `IDomainEvent`
- ✅ **Dispatch de Events**: Los eventos de dominio se despachan automáticamente después de guardar los cambios mediante `TodoManagementContext.DispatchDomainEventsAsync()`
- ✅ **Unit of Work**: Implementado mediante `IUnitOfWork` en `TodoManagementContext`, que gestiona transacciones y el dispatch de eventos

**Flujo**:
1. Las entidades agregan eventos de dominio mediante `AddDomainEvent()`
2. Al guardar cambios con `SaveEntitiesAsync()`, se despachan automáticamente los eventos
3. Los eventos se procesan mediante Mediator antes de confirmar la transacción

**Beneficio**: Garantiza consistencia entre el estado persistido y los eventos publicados.

### 13. Uso de Inteligencia Artificial

Este proyecto ha utilizado Inteligencia Artificial (IA) como herramienta de asistencia en las siguientes áreas:

- **Comentarios en Métodos**: Los comentarios XML y documentación de métodos fueron generados con asistencia de IA para mantener consistencia y claridad
- **README.md**: La documentación técnica y arquitectónica fue desarrollada con asistencia de IA para asegurar completitud, estructura profesional y escritura más entendible que si lo hubiera hecho yo :)
- **Tareas Repetitivas**: Para código que sigue patrones similares (como repositorios base, validaciones, etc.), se utilizó IA para acelerar el desarrollo manteniendo la consistencia

**Nota**: Todo el código fue revisado, validado y ajustado manualmente para garantizar calidad y cumplimiento de los requisitos del desafío técnico.

### 14. Separación de Interfaces por Responsabilidad

**Arquitectura Implementada**:
- **ICommandRepository<T>**: Interfaces para operaciones de escritura (commands)
- **IQueryRepository<T>**: Interfaces para operaciones de lectura (queries)
- **IValidationOnlyRepository<T>**: Interfaces para operaciones de solo lectura optimizadas para validaciones

**Beneficio**: Esta separación permite:
- Optimización independiente de cada tipo de operación
- Escalabilidad diferenciada según el tipo de carga
- Claridad en la intención del código
- Mejor rendimiento en validaciones (usando `AsNoTracking`)

---

## Posibles Mejoras y Consideraciones Futuras

### 1. Gestión de IDs en AddItem

**Situación Actual**: El método `AddItem` de `ITodoList` recibe un parámetro `id`, pero la clase base `Entity` ya genera automáticamente un `Guid` único al crear la entidad. Esto se ha implementado para mantener las consideraciones de la prueba técnica.

**Mejora Propuesta**: 
- Eliminar el parámetro `id` de `AddItem` ya que el `Guid` se genera automáticamente
- Eliminar la validación de existencia de `ItemId` duplicado, ya que el sistema de base de datos maneja la unicidad mediante índices
- El `ItemId` (identificador de negocio secuencial) puede seguir siendo gestionado por el repositorio mediante `GetNextId()`, pero el `Guid` (identificador técnico) se genera automáticamente

**Beneficio**: Simplifica la API, reduce código redundante y las operaciones POST/PUT/GET usan la Primary Key (Guid) como deberia ser.

### 2. Entidad Usuario y Multi-tenancy

Actualmente el sistema opera en un contexto global. Una evolución natural sería:
- Introducir la entidad `Usuario` como Aggregate Root.
- Vincular cada `TodoList` a un usuario específico.
- Esto permitiría que cada usuario gestione sus propias listas de forma aislada.

### 3. Microservicio de Identidad y Autenticación (Login)

Implementar un microservicio dedicado (`Identity.API`) encargado exclusivamente de la autenticación.

- **IdentityServer / Duende**: Implementar un Identity Provider (IdP) robusto que emita tokens JWT firmados.
- **Centralización**: El Login no debe ser responsabilidad de cada microservicio. El `Identity.API` centraliza el login, registro, recuperación de contraseñas y gestión de usuarios.
- **Integración con API Gateway**: El Gateway validará la firma de los tokens JWT antes de redirigir la petición a los microservicios (`TodoManagement`, `SocketManagement`).
- **Seguridad**: Almacenamiento seguro de credenciales (hashing), gestión de scopes y claims.

### 4. APIs y Repositorios Genéricos

Dado que Command y Query Repositories comparten patrones base:
- Se podrían implementar **APIs Genéricas** que expongan operaciones CRUD estándar para cualquier entidad.
- Los parámetros de entrada para Queries podrían refactorizarse para aceptar **Objetos JSON** complejos en lugar de múltiples parámetros de query string, permitiendo filtros dinámicos y flexibles.

**Beneficio**: De esta manera cada entidad tienen sus propias APIs y repositorios genéricos automáticamente sin necesidad de repetir código.

### 5. ItemId como Contador por TodoList

Para mejorar la experiencia de usuario:
- Refactorizar la generación de `ItemId` para que sea un contador **local** por cada `TodoList` (ej: Lista A tiene items 1, 2, 3; Lista B tiene items 1, 2).

### 6. Actualizaciones en Tiempo Real (SignalR)

Para una experiencia de usuario moderna y reactiva:
- Implementar **SignalR** para comunicación bidireccional.
- **Caso de Uso**: Cuando un `TodoItem` se marca como completado o su progreso cambia, se envía un evento de integración. Un servicio consumidor notifica vía SignalR al frontend para actualizar la barra de progreso y el estado "Completado" en tiempo real sin recargar la página o forzar un refresco de la lista.

### 7. Otras Mejoras Futuras

- **Caché**: Implementar caché distribuida (Redis) para operaciones de lectura frecuentes.
- **Event Sourcing**: Considerar Event Sourcing para auditoría completa y reconstrucción de estados históricos.

---

## Conclusión

Este proyecto demuestra un enfoque profesional y completo para el desarrollo de software. Es el trabajo de años de experiencia con Microservicios y mejoras constantes de mis conocimientos, implementando:

- **Domain-Driven Design** con agregados bien definidos  
- **Arquitectura de Microservicios** escalable  
- **CQRS** para separación de responsabilidades  
- **Event-Driven Architecture** con CAP y Kafka  
- **API Gateway** dinámico con Swagger unificado  
- **Behaviours** para cross-cutting concerns  
- **Reglas de Negocio** robustas y validadas  
- **Domain Events** con dispatch automático  
- **Unit of Work** para gestión transaccional  
- **Separación de Interfaces** por responsabilidad (Command/Query/Validation)  

El código está diseñado para ser mantenible, escalable y seguir las mejores prácticas de DDD.

---

## Autor

Hecho con cariño y mucho café por Andrey.