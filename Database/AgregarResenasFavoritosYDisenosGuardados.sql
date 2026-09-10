USE SurfDB;
GO

IF OBJECT_ID(N'dbo.ResenasProductos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ResenasProductos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductoId INT NOT NULL REFERENCES dbo.Productos(Id),
        ClienteId INT NOT NULL REFERENCES dbo.Usuarios(Id),
        PedidoId INT NOT NULL REFERENCES dbo.Pedidos(Id),
        Estrellas TINYINT NOT NULL,
        Comentario NVARCHAR(1000) NOT NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_ResenasProductos_Estrellas CHECK (Estrellas BETWEEN 1 AND 5),
        CONSTRAINT UQ_ResenasProductos_ClienteProducto UNIQUE (ClienteId, ProductoId)
    );
    CREATE INDEX IX_ResenasProductos_ProductoFecha ON dbo.ResenasProductos(ProductoId, FechaCreacion DESC);
END;
GO

IF OBJECT_ID(N'dbo.FavoritosProductos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FavoritosProductos (
        ClienteId INT NOT NULL REFERENCES dbo.Usuarios(Id),
        ProductoId INT NOT NULL REFERENCES dbo.Productos(Id),
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_FavoritosProductos PRIMARY KEY (ClienteId, ProductoId)
    );
END;
GO

IF OBJECT_ID(N'dbo.DisenosGuardados', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DisenosGuardados (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ClienteId INT NOT NULL REFERENCES dbo.Usuarios(Id),
        ShaperId INT NOT NULL REFERENCES dbo.Usuarios(Id),
        Nombre NVARCHAR(100) NOT NULL,
        ConfiguracionJson NVARCHAR(MAX) NOT NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_DisenosGuardados_Json CHECK (ISJSON(ConfiguracionJson)=1)
    );
    CREATE INDEX IX_DisenosGuardados_ClienteFecha ON dbo.DisenosGuardados(ClienteId, FechaCreacion DESC);
END;
GO
