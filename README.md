# Ecommerce Project

This repo contains:
- `Ecommerce.Api` (.NET API)
- `ecommerce-web` (Nuxt app)

## Local development

### 1) Run API
```powershell
cd Ecommerce.Api
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet restore
dotnet run --urls "http://localhost:5000"
```

### 2) Run Nuxt
Copy env:
- `ecommerce-web/.env.example` -> `ecommerce-web/.env.local` (edit values)

```powershell
cd ecommerce-web
npm install
npm run dev
```

## GitHub safety
- Secrets are not committed.
- Use `appsettings.example.json` and `.env.example` as templates.
