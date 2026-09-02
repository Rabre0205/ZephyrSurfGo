/* Ejecutar una sola vez en SurfDB. Agrega el catálogo de diseños de cada shaper. */
IF OBJECT_ID('dbo.DisenosShaper','U') IS NULL
BEGIN
    CREATE TABLE dbo.DisenosShaper (
        Id INT IDENTITY NOT NULL CONSTRAINT PK_DisenosShaper PRIMARY KEY,
        ShaperId INT NOT NULL,
        Nombre NVARCHAR(120) NOT NULL,
        Descripcion NVARCHAR(600) NOT NULL CONSTRAINT DF_DisenosShaper_Descripcion DEFAULT '',
        ImagenUrl NVARCHAR(500) NULL,
        ZonaAplicacion NVARCHAR(20) NOT NULL CONSTRAINT DF_DisenosShaper_Zona DEFAULT 'Ambos',
        PermiteColoresPersonalizados BIT NOT NULL CONSTRAINT DF_DisenosShaper_Colores DEFAULT 1,
        ColorPrimario NVARCHAR(7) NOT NULL CONSTRAINT DF_DisenosShaper_ColorPrimario DEFAULT '#ffffff',
        ColorSecundario NVARCHAR(7) NOT NULL CONSTRAINT DF_DisenosShaper_ColorSecundario DEFAULT '#111111',
        Recargo DECIMAL(10,2) NOT NULL CONSTRAINT DF_DisenosShaper_Recargo DEFAULT 0,
        Activo BIT NOT NULL CONSTRAINT DF_DisenosShaper_Activo DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_DisenosShaper_Fecha DEFAULT SYSUTCDATETIME(),
        FechaActualizacion DATETIME2 NULL,
        CONSTRAINT FK_DisenosShaper_Usuarios FOREIGN KEY (ShaperId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT CK_DisenosShaper_Zona CHECK (ZonaAplicacion IN ('Deck','Bottom','Ambos')),
        CONSTRAINT CK_DisenosShaper_Recargo CHECK (Recargo BETWEEN 0 AND 100000)
    );
    CREATE INDEX IX_DisenosShaper_ShaperActivo ON dbo.DisenosShaper(ShaperId,Activo,Nombre);
END;
IF OBJECT_ID('dbo.DisenosShaper','U') IS NOT NULL AND COL_LENGTH('dbo.DisenosShaper','ColorPrimario') IS NULL
BEGIN
    ALTER TABLE dbo.DisenosShaper ADD ColorPrimario NVARCHAR(7) NOT NULL CONSTRAINT DF_DisenosShaper_ColorPrimario DEFAULT '#ffffff';
    ALTER TABLE dbo.DisenosShaper ADD ColorSecundario NVARCHAR(7) NOT NULL CONSTRAINT DF_DisenosShaper_ColorSecundario DEFAULT '#111111';
END;
GO
