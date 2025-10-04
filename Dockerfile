# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copiar csproj y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar todo y publicar en modo Release
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa final: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# Nombre de tu aplicación principal (busca el .dll dentro de /out)
ENV APP_NET_CORE ExamenParcial.dll

# Render usa la variable $PORT automáticamente
CMD ASPNETCORE_URLS=http://+:${PORT} dotnet $APP_NET_CORE
