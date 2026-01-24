# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

# ابحث عن اول csproj واستعمله بالـ restore/publish
RUN set -e; \
    csproj="$(find . -maxdepth 6 -name '*.csproj' | head -n 1)"; \
    echo "Using csproj: $csproj"; \
    dotnet restore "$csproj"; \
    dotnet publish "$csproj" -c Release -o /app/out --no-restore

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# (اسم dll حسب مشروعك)
ENTRYPOINT ["dotnet", "Ecommerce.Api.dll"]
