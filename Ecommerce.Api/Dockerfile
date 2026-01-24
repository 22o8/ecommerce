# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# انسخ csproj فقط حتى يستفيد من cache
COPY Ecommerce.Api.csproj ./
RUN dotnet restore "./Ecommerce.Api.csproj"

# انسخ باقي الملفات
COPY . .
RUN dotnet publish "./Ecommerce.Api.csproj" -c Release -o /app/out

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","Ecommerce.Api.dll"]
