# Etapa 1: compilar e publicar
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ ./src/
RUN dotnet publish src/CryptoDashboard/CryptoDashboard.csproj -c Release -o /app/publish

# Etapa 2: imagem final, apenas com o runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Pasta do banco SQLite. O usuário 'app' é o padrão da imagem a partir do .NET 8.
RUN mkdir /data && chown app:app /data

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CryptoDashboard.dll"]
