USE SurfDB;
GO

IF COL_LENGTH('dbo.FavoritosProductos','PrecioGuardado') IS NULL
BEGIN
    ALTER TABLE dbo.FavoritosProductos ADD PrecioGuardado DECIMAL(10,2) NULL;
    UPDATE f SET PrecioGuardado=p.Precio
    FROM dbo.FavoritosProductos f INNER JOIN dbo.Productos p ON p.Id=f.ProductoId;
    ALTER TABLE dbo.FavoritosProductos ALTER COLUMN PrecioGuardado DECIMAL(10,2) NOT NULL;
END;
GO

IF COL_LENGTH('dbo.DisenosGuardados','FechaActualizacion') IS NULL
    ALTER TABLE dbo.DisenosGuardados ADD FechaActualizacion DATETIME2 NULL;
GO
