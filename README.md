# JobsMarketplace

## 1. Overview

JobsMarketplace is a simple RESTful ASP.NET Core Web API for customers who post jobs and contractors who submit offers. The solution is intentionally kept simple but structured. Controllers handle HTTP requests, services contain business rules, and repositories isolate data access. This keeps the API easier to test and allows the persistence layer to be changed later without rewriting the application logic.

## 1.1 Assessment Coverage

- Seed data: sample `Customers` and `Contractors` are inserted automatically on startup in [SeedData.cs]JobsMarketplace\JobsMarketplace.Api\Data\SeedData.cs
- OOP and SOLID: responsibilities are separated across controllers, services, repositories, DTOs, and entities
- Input validation: request DTO validation and service-level business-rule validation are both applied
- Efficient data structures: indexed columns, composite indexes, paged queries, cursor-based pagination, and `AsNoTracking()` are used on read-heavy paths
- Design patterns: repository pattern, service layer pattern, and DTO pattern are used throughout the API
- Unit testing: xUnit tests cover the main business rules, search behavior, and validation failures
- README: this file documents setup, endpoints, architecture, validation, performance, and testing
- Bonus caching: customer and contractor lookups by ID are cached in memory and Redis to reduce repeated reads for hot accounts across instances

## 2. Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker Compose
- Redis
- xUnit
- IMemoryCache and Redis distributed cache

## 3. How To Run

1. Make sure .NET 8 SDK is available.
2. Start PostgreSQL with Docker:

```powershell
docker compose up -d
```

3. From the repository root, run:

```powershell
dotnet restore
dotnet run --project .\JobsMarketplace.Api\JobsMarketplace.Api.csproj
```

4. If you need a clean local reset for the Dockerized services:

```powershell
docker compose down -v
docker compose up -d
```

## 4. How To Run Tests

```powershell
dotnet test
```

Optional k6 smoke test:

```powershell
docker run --rm -i -v "${PWD}\performance:/scripts" grafana/k6 run -e BASE_URL=https://host.docker.internal:36060 /scripts/k6-smoke.js
```

Optional Postman testing:

- Import [JobsMarketplace.postman_collection.json]JobsMarketplace\JobsMarketplace.postman_collection.json)
- Set `baseUrl` to `https://localhost:36060`
- After creating a job or offer, update the `jobId` and `offerId` collection variables before running the follow-up requests

## 5. Database And Seed Data

- The API uses PostgreSQL with Docker:
  - Host: `localhost`
  - Port: `5432`
  - Database: `jobsmarketplace`
  - Username: `postgres`
  - Password: `postgres`
- Redis is also included for distributed caching:
  - Host: `localhost`
  - Port: `6379`
- PostgreSQL and Redis are started with `docker compose up -d`.
- The database is created and updated automatically on startup with EF Core migrations.
- Sample seed data is added for customers and contractors.

Seeded customers:
- `1 - John Smith`
- `2 - Maria Santos`
- `3 - Alice Brown`
- `4 - David Miller`

Seeded contractors:
- `1 - BuildRight Services`
- `2 - Northwind Plumbing`
- `3 - Skyline Electric`
- `4 - Crafted Interiors`

## 6. API Endpoints

Customers:
- `GET /customers/search?query=smi&page=1&pageSize=20&cursor=...`
- `GET /customers/{id}`

Contractors:
- `GET /contractors/search?query=build&page=1&pageSize=20&cursor=...`
- `GET /contractors/{id}`

Jobs:
- `GET /jobs?page=1&pageSize=20&cursor=...`
- `GET /jobs/{id}`
- `POST /customers/{customerId}/jobs`
- `PUT /jobs/{id}`
- `DELETE /jobs/{id}`

Job offers:
- `GET /jobs/{jobId}/offers`
- `GET /joboffers/{id}`
- `POST /jobs/{jobId}/offers`
- `PUT /joboffers/{id}`
- `DELETE /joboffers/{id}`
- `POST /jobs/{jobId}/offers/{offerId}/accept`

Example request for accepting an offer:

```json
{
  "customerId": 1
}
```

## 7. Validation Rules

Jobs:
- customer must exist before a job can be created
- description is required
- budget must be greater than zero
- due date must be after start date
- only open jobs can be updated or deleted

Job offers:
- job must exist
- contractor must exist
- price must be greater than zero
- offers can only be created for open jobs
- a contractor cannot submit more than one offer for the same job
- only pending offers on open jobs can be updated or deleted

Accepting an offer:
- the job must exist
- the offer must belong to the job
- only the owning customer can accept the offer
- only open jobs can accept offers
- only pending offers can be accepted
- accepted offer is marked `Accepted`
- remaining offers are marked `Rejected`

Search requests:
- `page` must be greater than zero
- `pageSize` must be between `1` and `100`
- `query` must not exceed `100` characters

## 8. Architecture Decisions

- Controllers are thin and delegate work to services.
- Services contain business rules and workflow validation.
- Repositories isolate EF Core queries and persistence access.
- DTOs separate API contracts from entity models.
- PostgreSQL was chosen to better match the assessment requirement around scale, indexing, and concurrent requests, while Docker keeps local setup predictable.
- Jobs use optimistic concurrency so competing writes fail safely instead of silently overwriting state.

## 9. Search And Performance Notes

- Searches use EF Core LINQ queries rather than loading records into memory.
- Searchable fields are indexed:
  - `Customers.LastName, FirstName`
  - `Contractors.Name, Id`
- Relationship and lookup fields are also indexed:
  - `Jobs.CustomerId, Id`
  - `Jobs.Status, Id`
  - `JobOffers.JobId, Status, CreatedAt`
  - `JobOffers.ContractorId`
- The customer search index is aligned with the actual query pattern: filter by last name prefix and order by last name then first name.
- The job offer listing index is aligned with the actual query pattern: filter by job and order by creation time.
- Search endpoints support paging through `page` and `pageSize`.
- Cursor-based pagination is also supported for customer search, contractor search, and job listing to avoid deep offset scans on large datasets.
- Prefix matching is used for partial searches to stay efficient and predictable.
- `AsNoTracking()` is used on read-heavy queries to reduce EF Core tracking overhead.
- PostgreSQL is a better fit than SQLite for higher write concurrency and larger production-style workloads.
- PostgreSQL connection pooling is configured in the connection string to better handle concurrent requests.

This implementation includes practical scaling-oriented decisions: PostgreSQL, composite indexes aligned to query patterns, cursor-capable pagination, `AsNoTracking()` on read-heavy queries, Redis-backed distributed caching, and optimistic concurrency handling. These choices provide a strong foundation for larger-scale workloads, while true production operation at 10 million customers would further require horizontal scaling, production monitoring, database tuning, and workload-based optimization.

## 10. API Security And Error Handling

- Search and lookup queries use EF Core LINQ rather than raw SQL string concatenation.
- Paging and search inputs are validated before queries are executed.
- HTTPS redirection is enabled for the API.
- A global exception middleware returns consistent JSON error responses.
- Validation and not-found errors return controlled messages.
- Unexpected exceptions return a generic `500` response without exposing internal stack traces to API clients.
- Database uniqueness conflicts are translated into safe conflict responses.
- Write conflicts caused by concurrent updates are translated into `409 Conflict` responses.

## 11. Caching Notes

- Frequently requested customer and contractor lookups by ID use a two-level cache:
- `IMemoryCache` for fast in-process hot reads
- Redis for cross-instance distributed reuse
- The caches use size limits and expiration windows to reduce repeated database reads.
- This satisfies the assessment's caching bonus while still being simple to explain and run.

## 12. Observability And Operations

- `/health/live` provides a liveness endpoint.
- `/health/ready` verifies PostgreSQL and Redis readiness.
- HTTP request logging is enabled.
- A request-timing middleware logs slow requests for troubleshooting.
- A reproducible k6 smoke test is included in [k6-smoke.js]JobsMarketplace/performance/k6-smoke.js.

## 13. Future Improvements

- add integration tests for HTTP endpoints
- add authentication and authorization if ownership needs to come from identity instead of request data
- add stronger concurrency control for offer acceptance in high-write scenarios
- move startup-applied migrations to a deployment pipeline step for stricter production control
- refine indexes further using production query plans and workload statistics
- add read replicas and traffic routing for heavier read-mostly workloads
