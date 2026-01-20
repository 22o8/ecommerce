# ===== BUILD STAGE =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات المشروع فقط أولاً
COPY Ecommerce.Api/Ecommerce.Api.csproj Ecommerce.Api/
RUN dotnet restore Ecommerce.Api/Ecommerce.Api.csproj

# نسخ باقي الملفات
COPY . .
WORKDIR /src/Ecommerce.Api

RUN dotnet publish -c Release -o /app/out

# ===== RUNTIME STAGE =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Ecommerce.Api.dll"]
