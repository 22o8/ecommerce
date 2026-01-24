# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything.
# ملاحظة: على Render أحياناً يكون build context هو جذر الريبو، وأحياناً يكون مجلد Ecommerce.Api نفسه.
# لهذا نخلي restore/publish يشتغل بالحالتين.
COPY . .

# Restore
RUN if [ -f "Ecommerce.Api/Ecommerce.Api.csproj" ]; then \
      cd Ecommerce.Api && dotnet restore "Ecommerce.Api.csproj"; \
    elif [ -f "Ecommerce.Api.csproj" ]; then \
      dotnet restore "Ecommerce.Api.csproj"; \
    elif [ -f "Ecommerce.Api.sln" ]; then \
      dotnet restore "Ecommerce.Api.sln"; \
    else \
      echo "No .csproj/.sln found in build context" && ls -la && exit 1; \
    fi

# Publish
RUN if [ -f "Ecommerce.Api/Ecommerce.Api.csproj" ]; then \
      cd Ecommerce.Api && dotnet publish "Ecommerce.Api.csproj" -c Release -o /app/out; \
    else \
      dotnet publish "Ecommerce.Api.csproj" -c Release -o /app/out; \
    fi

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Ecommerce.Api.dll"]
