# Imagen de la UI (Blazor Server). La cadena de conexion real se pasa al contenedor con la variable
# ConnectionStrings__Default (docker-compose.yml lo hace con el perfil "app"); la imagen solo lleva
# el valor publico de desarrollo que trae appsettings.json.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Primero solo los csproj: la capa del restore se reusa mientras no cambien las dependencias.
COPY src/WorldCupPlanner.Domain/WorldCupPlanner.Domain.csproj src/WorldCupPlanner.Domain/
COPY src/WorldCupPlanner.DataAccess/WorldCupPlanner.DataAccess.csproj src/WorldCupPlanner.DataAccess/
COPY src/WorldCupPlanner.Services/WorldCupPlanner.Services.csproj src/WorldCupPlanner.Services/
COPY src/WorldCupPlanner.UI/WorldCupPlanner.UI.csproj src/WorldCupPlanner.UI/
RUN dotnet restore src/WorldCupPlanner.UI/WorldCupPlanner.UI.csproj

# El publish incluye flags-master (Content con CopyToPublishDirectory en el csproj de la UI).
COPY src/ src/
RUN dotnet publish src/WorldCupPlanner.UI/WorldCupPlanner.UI.csproj \
    --configuration Release --no-restore --output /app/publish -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# La imagen base no trae cliente HTTP y el HEALTHCHECK necesita uno.
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Usuario sin privilegios "app" que define la imagen aspnet; escucha en 8080 (ASPNETCORE_HTTP_PORTS).
USER $APP_UID
EXPOSE 8080

# El arranque migra la base y crea las cuentas de demo antes de escuchar: de ahi el start-period.
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl --fail --silent --show-error http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "WorldCupPlanner.UI.dll"]
