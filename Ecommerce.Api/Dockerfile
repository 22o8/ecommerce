# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything (Render build context may be the repo root OR the Ecommerce.Api folder)
COPY . .

# Restore/Publish with a project path that works in both layouts:
# 1) build context = repo root  -> ./Ecommerce.Api/Ecommerce.Api.csproj
# 2) build context = Ecommerce.Api folder -> ./Ecommerce.Api.csproj
RUN if [ -f "./Ecommerce.Api.csproj" ]; then \
      dotnet restore ./Ecommerce.Api.csproj; \
      dotnet publish ./Ecommerce.Api.csproj -c Release -o /app/out; \
    elif [ -f "./Ecommerce.Api/Ecommerce.Api.csproj" ]; then \
      dotnet restore ./Ecommerce.Api/Ecommerce.Api.csproj; \
      dotnet publish ./Ecommerce.Api/Ecommerce.Api.csproj -c Release -o /app/out; \
    else \
      echo "Could not find Ecommerce.Api.csproj" && ls -la && exit 1; \
    fi

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Ecommerce.Api.dll"]
