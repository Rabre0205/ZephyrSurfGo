param(
    [string]$SqlServer = "localhost\SQLEXPRESS"
)

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot
$envFile = Join-Path $projectRoot "WebApplication2\.env"
$schemaFile = Join-Path $projectRoot "Database\CreacionSurfDB.sql"
$seedFile = Join-Path $projectRoot "Database\DatosPrueba.sql"
$improvementsFile = Join-Path $projectRoot "Database\AgregarCatalogoResenasSolicitudesYRecuperacion.sql"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "No se encontró .NET. Instalá el SDK 10 antes de continuar."
}
if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "No se encontró sqlcmd. Instalá SQL Server Command Line Utilities o ejecutá los SQL desde SSMS."
}

Write-Host "Restaurando dependencias..."
dotnet restore (Join-Path $projectRoot "Surf.slnx")
if ($LASTEXITCODE -ne 0) { throw "No se pudieron restaurar las dependencias." }

$databaseExists = sqlcmd -S $SqlServer -E -C -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID('SurfDB') IS NULL THEN 0 ELSE 1 END"
if ($LASTEXITCODE -ne 0) { throw "No se pudo conectar con SQL Server en '$SqlServer'." }
if (($databaseExists | Out-String).Trim() -eq "0") {
    Write-Host "Creando SurfDB..."
    sqlcmd -S $SqlServer -E -C -b -i $schemaFile
    if ($LASTEXITCODE -ne 0) { throw "No se pudo crear SurfDB." }
} else {
    Write-Host "SurfDB ya existe; no se vuelve a crear."
}

Write-Host "Aplicando mejoras de catálogo, reseñas, solicitudes y recuperación..."
sqlcmd -S $SqlServer -E -C -b -d SurfDB -i $improvementsFile
if ($LASTEXITCODE -ne 0) { throw "No se pudieron aplicar las mejoras recientes de la base." }

Write-Host "Agregando datos ficticios..."
sqlcmd -S $SqlServer -E -C -b -i $seedFile
if ($LASTEXITCODE -ne 0) { throw "No se pudieron agregar los datos ficticios." }

if (-not (Test-Path $envFile)) {
    $connectionString = "Server=$SqlServer;Database=SurfDB;Trusted_Connection=True;TrustServerCertificate=True;"
    @(
        "SURFDB_CONNECTION_STRING=$connectionString"
        "CLOUDINARY_URL=cloudinary://API_KEY:API_SECRET@CLOUD_NAME"
        "MP_CLIENT_ID=CONFIGURAR_PARA_PAGOS"
        "MP_CLIENT_SECRET=CONFIGURAR_PARA_PAGOS"
        "MP_REDIRECT_URI=https://localhost:7165/MercadoPago/Callback"
        "MP_COMISION_PLATAFORMA=0.10"
    ) | Set-Content -Path $envFile -Encoding utf8
    Write-Host "Se creó WebApplication2\.env con valores locales."
} else {
    Write-Host "El archivo .env existente se conservó sin cambios."
}

$webProject = Join-Path $projectRoot "WebApplication2\WebApplication2.csproj"
dotnet user-secrets set "Authentication:Google:ClientId" "DESARROLLO_LOCAL_SIN_GOOGLE" --project $webProject | Out-Null
dotnet user-secrets set "Authentication:Google:ClientSecret" "DESARROLLO_LOCAL_SIN_GOOGLE" --project $webProject | Out-Null

Write-Host ""
Write-Host "Configuración terminada. Ejecutá:"
Write-Host "dotnet run --project .\WebApplication2\WebApplication2.csproj --launch-profile https"
Write-Host ""
Write-Host "Usuarios demo: admin@zephyrsurfgo.com / Admin123 | cliente@demo.com / Cliente123 | shaper@demo.com / Shaper123"
