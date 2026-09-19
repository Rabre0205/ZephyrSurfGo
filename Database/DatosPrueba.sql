USE SurfDB;
GO

/* Opciones requeridas por SQL Server para modificar tablas que tienen
   índices filtrados o índices sobre expresiones. sqlcmd no siempre las
   habilita con los mismos valores que SSMS. */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO

/* Datos ficticios e idempotentes para desarrollo local.
   Cuentas:
   admin@zephyrsurfgo.com / Admin123
   cliente@demo.com         / Cliente123
   shaper@demo.com          / Shaper123
*/

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = N'admin@zephyrsurfgo.com')
    INSERT INTO Usuarios (Email, Contrasenia, Nombre, PaisId, TipoDeUsuarioId, Activo)
    VALUES (N'admin@zephyrsurfgo.com', N'$2a$11$UFl/gBu1mJmCzKYkUgvAyO6dEphrzHG24eviZcIsLlX3QphWJJz9O', N'Administrador Demo', 0, 2, 1);

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = N'cliente@demo.com')
    INSERT INTO Usuarios (Email, Contrasenia, Nombre, PaisId, TipoDeUsuarioId, Activo)
    VALUES (N'cliente@demo.com', N'$2a$11$./YUsRryUIrRVcDpSZIp3ec/dvxIcBnIFcdaP3dxxm5/64ObjJkaW', N'Cliente Demo', 0, 1, 1);

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = N'shaper@demo.com')
    INSERT INTO Usuarios (Email, Contrasenia, Nombre, PaisId, TipoDeUsuarioId, NombreDeNegosio, Contacto, LogoUrl, Activo)
    VALUES (N'shaper@demo.com', N'$2a$11$2E/tHdgqJvtdTMw6vEl5xubVQPPWq9XFbi1LUv54TBIEP81YRQSe.', N'Shaper Demo', 0, 0, N'Taller Demo', N'099 000 000', N'/img/LogoDeMarca.png', 1);
GO

DECLARE @ShaperId INT = (SELECT Id FROM Usuarios WHERE Email = N'shaper@demo.com');

IF NOT EXISTS (SELECT 1 FROM Productos WHERE ShaperId = @ShaperId AND Titulo = N'Tabla Demo')
BEGIN
    INSERT INTO Productos (Titulo, Subtitulo, Precio, Descripcion, ImagenUrl, ShaperId, TipoProducto)
    VALUES (N'Tabla Demo', N'Shortboard versátil', 650, N'Tabla ficticia para comprobar catálogo, carrito, favoritos y reseñas.', N'/img/tabla.png', @ShaperId, N'Tabla');
    DECLARE @TablaId INT = SCOPE_IDENTITY();
    INSERT INTO Tablas (ProductoId, Altura, Ancho, Volumen, SistemaDeEncajeId, TipoDeOlaId, EstiloDeSurfId, PesoMinimo, PesoMaximo, ExperienciaId, ImagenAtrasUrl, Disponible)
    VALUES (@TablaId, N'6''0"', 19, 30.00, 0, 1, 2, 55, 90, 2, N'/img/tabla.png', 1);
END;

IF NOT EXISTS (SELECT 1 FROM Productos WHERE ShaperId = @ShaperId AND Titulo = N'Quillas Demo')
BEGIN
    INSERT INTO Productos (Titulo, Subtitulo, Precio, Descripcion, ImagenUrl, ShaperId, TipoProducto)
    VALUES (N'Quillas Demo', N'Juego de tres', 95, N'Accesorio ficticio para probar stock y carrito.', N'/img/boards/thruster.png', @ShaperId, N'Quilla');
    DECLARE @QuillaId INT = SCOPE_IDENTITY();
    INSERT INTO Quillas (ProductoId, SistemaDeEncajeId, Stock) VALUES (@QuillaId, 0, 10);
END;

IF NOT EXISTS (SELECT 1 FROM PuntosRetiro WHERE ShaperId = @ShaperId AND Nombre = N'Punto Demo')
    INSERT INTO PuntosRetiro (ShaperId, Nombre, Direccion, Ciudad, Horario, Indicaciones, Latitud, Longitud, Activo)
    VALUES (@ShaperId, N'Punto Demo', N'Rambla de prueba 123', N'Montevideo', N'Lunes a viernes de 10 a 18', N'Coordinar antes de retirar.', -34.901100, -56.164500, 1);
GO
