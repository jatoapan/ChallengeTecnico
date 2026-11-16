# Etapa 1: compilar y publicar la API (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero el .csproj: Docker reutiliza el restore mientras no cambien los paquetes.
COPY InventarioProductos.API/InventarioProductos.API.csproj InventarioProductos.API/
RUN dotnet restore InventarioProductos.API/InventarioProductos.API.csproj

COPY InventarioProductos.API/ InventarioProductos.API/
RUN dotnet publish InventarioProductos.API/InventarioProductos.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: imagen ligera de ejecución, sin SDK
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Carpeta de logs de Serilog, con permisos para el usuario sin privilegios de la imagen.
RUN mkdir -p /app/logs && chown -R $APP_UID /app/logs

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

# No corre como root.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "InventarioProductos.API.dll"]
