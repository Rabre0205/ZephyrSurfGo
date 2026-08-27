SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.PuntosRetiro','U') IS NULL
BEGIN
    CREATE TABLE dbo.PuntosRetiro (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ShaperId INT NOT NULL,
        Nombre NVARCHAR(150) NOT NULL,
        Direccion NVARCHAR(250) NOT NULL,
        Ciudad NVARCHAR(120) NOT NULL,
        Horario NVARCHAR(250) NOT NULL DEFAULT '',
        Indicaciones NVARCHAR(500) NOT NULL DEFAULT '',
        Latitud DECIMAL(9,6) NOT NULL,
        Longitud DECIMAL(9,6) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        FechaActualizacion DATETIME2 NULL,
        CONSTRAINT FK_PuntosRetiro_Usuarios FOREIGN KEY (ShaperId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT CK_PuntosRetiro_Latitud CHECK (Latitud BETWEEN -90 AND 90),
        CONSTRAINT CK_PuntosRetiro_Longitud CHECK (Longitud BETWEEN -180 AND 180)
    );
    CREATE INDEX IX_PuntosRetiro_ShaperActivo ON dbo.PuntosRetiro(ShaperId,Activo);
END;

IF OBJECT_ID('dbo.SolicitudesSoporte','U') IS NULL
BEGIN
    CREATE TABLE dbo.SolicitudesSoporte (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ShaperId INT NOT NULL,
        Asunto NVARCHAR(150) NOT NULL,
        Mensaje NVARCHAR(2000) NOT NULL,
        Respuesta NVARCHAR(2000) NULL,
        Estado TINYINT NOT NULL DEFAULT 0,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        FechaRespuesta DATETIME2 NULL,
        CONSTRAINT FK_SolicitudesSoporte_Usuarios FOREIGN KEY (ShaperId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT CK_SolicitudesSoporte_Estado CHECK (Estado BETWEEN 0 AND 2)
    );
    CREATE INDEX IX_SolicitudesSoporte_EstadoFecha ON dbo.SolicitudesSoporte(Estado,FechaCreacion DESC);
END;

COMMIT TRANSACTION;
