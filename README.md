# Zephyr Surf Go

Aplicación ASP.NET para conectar clientes con shapers, publicar productos, personalizar tablas y gestionar pedidos.

## Inicio rápido en Windows

### Requisitos

- .NET SDK 10
- SQL Server Express o Developer
- `sqlcmd` (SQL Server Command Line Utilities)
- Git

### Instalación automática

```powershell
git clone https://github.com/Rabre0205/ZephyrSurfGo.git
cd ZephyrSurfGo
git checkout codex/mejoras-panel-shaper-filtros
dotnet dev-certs https --trust
.\setup-dev.ps1
dotnet run --project .\WebApplication2\WebApplication2.csproj --launch-profile https
```

Abrir `https://localhost:7165`.

Si PowerShell bloquea el script, se puede ejecutar una vez con:

```powershell
powershell -ExecutionPolicy Bypass -File .\setup-dev.ps1
```

Para otra instancia de SQL Server:

```powershell
.\setup-dev.ps1 -SqlServer "(localdb)\MSSQLLocalDB"
```

El instalador restaura paquetes, crea `SurfDB` cuando no existe, agrega datos ficticios idempotentes, crea el `.env` local y configura valores de desarrollo para que Google no impida iniciar la aplicación.

## Usuarios de prueba

| Rol | Usuario | Contraseña |
|---|---|---|
| Administrador | `admin@zephyrsurfgo.com` | `Admin123` |
| Cliente | `cliente@demo.com` | `Cliente123` |
| Shaper | `shaper@demo.com` | `Shaper123` |

Estas cuentas son solamente para desarrollo local. No deben utilizarse en producción.

## Integraciones opcionales

El proyecto funciona localmente con login manual sin configurar servicios externos. Para probar funciones reales hay que reemplazar los valores de ejemplo:

- Google: `Authentication:Google:ClientId` y `Authentication:Google:ClientSecret` mediante `dotnet user-secrets`. Si no están configurados, el botón de Google se oculta y el acceso manual continúa disponible.
- Gmail SMTP: `Correo:SmtpUsuario` y `Correo:SmtpContrasena` mediante `dotnet user-secrets`.
- Cloudinary y Mercado Pago: completar `WebApplication2/.env` tomando como referencia `WebApplication2/.env.example`.

Nunca se deben subir contraseñas, tokens, cadenas privadas ni el archivo `.env`.

## Pruebas

```powershell
dotnet test --configuration Release
```

## Instalación manual de la base

Si no se dispone de `sqlcmd`, ejecutar desde SQL Server Management Studio, en este orden:

1. `Database/CreacionSurfDB.sql`
2. `Database/DatosPrueba.sql`

El primer archivo se usa sobre una instalación nueva. El segundo puede ejecutarse nuevamente porque evita duplicar los datos ficticios.

Si la base ya existía antes de las mejoras de favoritos y diseños guardados, ejecutar una sola vez:

```text
Database/MejorarFavoritosYDisenosGuardados.sql
```

Para una base existente, las mejoras de catálogo, reseñas, solicitudes de shapers y recuperación de contraseña se aplican automáticamente con `setup-dev.ps1`. Manualmente se puede ejecutar:

```text
Database/AgregarCatalogoResenasSolicitudesYRecuperacion.sql
```

La recuperación de contraseña y las notificaciones por correo requieren `Correo:SmtpUsuario` y `Correo:SmtpContrasena`. Los enlaces vencen a los 30 minutos y son de un solo uso.

El esquema principal ya incluye también el marketplace de cursos. En una base existente, `setup-dev.ps1` ejecuta automáticamente `Database/AgregarCursosShaper.sql` junto con el resto de las migraciones.
