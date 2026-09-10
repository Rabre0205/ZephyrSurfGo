-- Ejecutar en SurfDB existente. No elimina datos.
IF OBJECT_ID('dbo.SolicitudesPersonalizadas','U') IS NULL
    THROW 50001, 'Primero creá la tabla SolicitudesPersonalizadas.', 1;
IF COL_LENGTH('dbo.SolicitudesPersonalizadas','EntregaEstimada') IS NULL
    ALTER TABLE dbo.SolicitudesPersonalizadas ADD EntregaEstimada DATE NULL;
IF COL_LENGTH('dbo.SolicitudesPersonalizadas','EntregaOriginal') IS NULL
    ALTER TABLE dbo.SolicitudesPersonalizadas ADD EntregaOriginal DATE NULL;
IF COL_LENGTH('dbo.SolicitudesPersonalizadas','AvanceShaper') IS NULL
    ALTER TABLE dbo.SolicitudesPersonalizadas ADD AvanceShaper NVARCHAR(1000) NOT NULL DEFAULT '';
IF COL_LENGTH('dbo.SolicitudesPersonalizadas','FechaSeguimiento') IS NULL
    ALTER TABLE dbo.SolicitudesPersonalizadas ADD FechaSeguimiento DATETIME2 NULL;
GO
