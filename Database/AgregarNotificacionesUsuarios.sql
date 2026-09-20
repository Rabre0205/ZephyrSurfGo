USE SurfDB;
GO
IF OBJECT_ID(N'dbo.NotificacionesUsuarios',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.NotificacionesUsuarios(
  Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  UsuarioId INT NOT NULL,
  Titulo NVARCHAR(160) NOT NULL,
  Mensaje NVARCHAR(1000) NOT NULL,
  Url NVARCHAR(500) NULL,
  Tipo NVARCHAR(40) NOT NULL CONSTRAINT DF_NotificacionesUsuarios_Tipo DEFAULT N'General',
  Leida BIT NOT NULL CONSTRAINT DF_NotificacionesUsuarios_Leida DEFAULT 0,
  FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_NotificacionesUsuarios_Fecha DEFAULT SYSUTCDATETIME(),
  FechaLectura DATETIME2 NULL,
  CONSTRAINT FK_NotificacionesUsuarios_Usuarios FOREIGN KEY(UsuarioId) REFERENCES dbo.Usuarios(Id) ON DELETE CASCADE
 );
 CREATE INDEX IX_NotificacionesUsuarios_Bandeja ON dbo.NotificacionesUsuarios(UsuarioId,Leida,FechaCreacion DESC);
END
GO
