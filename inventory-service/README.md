# inventory-service

Standalone Inventory microservice extracted from the OrderManager monolith. Owns stock levels,
warehouse locations and reorder thresholds in its own SQLite database (`inventory.db`); the
monolith calls it over HTTP via `InventoryHttpClient`.

## Layout

| Path | Purpose |
|------|---------|
| `src/InventoryService.Api` | .NET 8 Web API (EF Core + SQLite, Swagger, `/health`, Prometheus `/metrics`) |
| `tests/InventoryService.Api.Tests` | xUnit unit + in-process API tests |
| `client-app` | Angular 17 UI (inventory list, low-stock view, restock, add item) built into `wwwroot` |
| `docker/Dockerfile` | Multi-stage build (Angular → .NET publish → `aspnet:8.0-alpine`, port 8080) |
| `helm/inventory-service` | Deployment, Service, Ingress, NetworkPolicy, ServiceMonitor, HPA, PVC + `values-dev/staging.yaml` |
| `argocd` | ArgoCD `Application` manifests for `decomposition-dev` / `decomposition-staging` |
| `../.github/workflows/inventory-service.yaml` | CI/CD: build, test, helm lint, push to ECR, ArgoCD sync |

## API

| Method | Route | Notes |
|--------|-------|-------|
| GET | `/api/inventory` | all items |
| GET | `/api/inventory/{id}` | by record id |
| GET | `/api/inventory/product/{productId}` | by product |
| GET | `/api/inventory/low-stock` | `QuantityOnHand <= ReorderLevel` |
| POST | `/api/inventory` | create record (409 on duplicate product) |
| PUT | `/api/inventory/product/{productId}` | update name/sku/reorder level/location |
| POST | `/api/inventory/product/{productId}/restock` | `{ "quantity": n }` |
| POST | `/api/inventory/product/{productId}/deduct` | `{ "quantity": n }`, 409 on insufficient stock |
| POST | `/api/inventory/check` | `{ "items": [{ "productId", "quantity" }] }` → availability per line |
| DELETE | `/api/inventory/product/{productId}` | |

## Run locally

```bash
cd client-app && npm ci && npm run build && cd ..
dotnet test InventoryService.sln
ASPNETCORE_URLS=http://localhost:8080 dotnet run --project src/InventoryService.Api
```

Docker: `docker build -f docker/Dockerfile -t inventory-service:local . && docker run -p 8080:8080 inventory-service:local`
