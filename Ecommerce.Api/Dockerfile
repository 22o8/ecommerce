# ===== BUILD STAGE =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Ecommerce.Api.csproj ./
RUN dotnet restore Ecommerce.Api.csproj

COPY . ./
RUN dotnet publish -c Release -o /app/out

# ===== RUNTIME STAGE =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Ecommerce.Api.dll"]
