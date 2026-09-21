/* Ejecutar una vez sobre SurfDB. Es seguro volver a ejecutarlo. */
IF COL_LENGTH('ResenasProductos','RespuestaShaper') IS NULL ALTER TABLE ResenasProductos ADD RespuestaShaper NVARCHAR(1000) NULL;
IF COL_LENGTH('ResenasProductos','FechaRespuesta') IS NULL ALTER TABLE ResenasProductos ADD FechaRespuesta DATETIME2 NULL;
IF COL_LENGTH('ResenasProductos','FechaEdicion') IS NULL ALTER TABLE ResenasProductos ADD FechaEdicion DATETIME2 NULL;
IF COL_LENGTH('ResenasProductos','Moderada') IS NULL ALTER TABLE ResenasProductos ADD Moderada BIT NOT NULL CONSTRAINT DF_ResenasProductos_Moderada DEFAULT 0;
IF COL_LENGTH('ResenasProductos','MotivoModeracion') IS NULL ALTER TABLE ResenasProductos ADD MotivoModeracion NVARCHAR(300) NULL;
GO
IF OBJECT_ID('SolicitudesShapers','U') IS NULL CREATE TABLE SolicitudesShapers(
 Id INT IDENTITY PRIMARY KEY,Nombre NVARCHAR(100) NOT NULL,Email NVARCHAR(150) NOT NULL,Marca NVARCHAR(150) NOT NULL,
 Ubicacion NVARCHAR(120) NOT NULL,Celular NVARCHAR(40) NOT NULL,Instagram NVARCHAR(100) NOT NULL,AniosExperiencia INT NOT NULL,
 RealizaPersonalizadas BIT NOT NULL,RealizaEnvios BIT NOT NULL,Presentacion NVARCHAR(1500) NOT NULL,
 Estado NVARCHAR(30) NOT NULL CONSTRAINT DF_SolicitudesShapers_Estado DEFAULT N'Pendiente',NotasAdmin NVARCHAR(1000) NULL,
 FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_SolicitudesShapers_Fecha DEFAULT SYSUTCDATETIME(),FechaActualizacion DATETIME2 NULL,
 CONSTRAINT CK_SolicitudesShapers_Estado CHECK(Estado IN(N'Pendiente',N'En revisión',N'Aprobada',N'Rechazada')));
GO
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_SolicitudesShapers_EstadoFecha') CREATE INDEX IX_SolicitudesShapers_EstadoFecha ON SolicitudesShapers(Estado,FechaCreacion DESC);
GO
IF OBJECT_ID('RecuperacionesContrasenia','U') IS NULL CREATE TABLE RecuperacionesContrasenia(
 Id BIGINT IDENTITY PRIMARY KEY,UsuarioId INT NOT NULL REFERENCES Usuarios(Id),TokenHash CHAR(64) NOT NULL UNIQUE,
 FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),FechaExpiracion DATETIME2 NOT NULL,FechaUso DATETIME2 NULL,
 SolicitadaDesde NVARCHAR(64) NULL);
GO
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Recuperaciones_UsuarioExpiracion') CREATE INDEX IX_Recuperaciones_UsuarioExpiracion ON RecuperacionesContrasenia(UsuarioId,FechaExpiracion DESC);
GO
