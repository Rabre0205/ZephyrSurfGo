/* Ejecutar una sola vez en SurfDB. Es idempotente y no altera pedidos ni pagos. */
IF OBJECT_ID('dbo.SolicitudesPersonalizadas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SolicitudesPersonalizadas (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SolicitudesPersonalizadas PRIMARY KEY,
        ClienteId INT NOT NULL,
        ShaperId INT NOT NULL,
        ProductoBaseId INT NULL,
        ModeloSnapshot NVARCHAR(150) NOT NULL,
        PrecioEstimado DECIMAL(10,2) NOT NULL,
        Largo NVARCHAR(30) NOT NULL,
        Ancho NVARCHAR(30) NOT NULL,
        Grosor NVARCHAR(30) NOT NULL,
        Volumen NVARCHAR(30) NOT NULL,
        Construccion NVARCHAR(100) NOT NULL,
        Tail NVARCHAR(80) NOT NULL,
        SistemaQuillas NVARCHAR(80) NOT NULL,
        ConfiguracionQuillas NVARCHAR(100) NOT NULL,
        Laminado NVARCHAR(100) NOT NULL,
        ParcheCarbono NVARCHAR(100) NOT NULL,
        Diseno NVARCHAR(100) NOT NULL,
        ColorPrimario NVARCHAR(30) NOT NULL,
        ColorSecundario NVARCHAR(30) NOT NULL,
        DetallesAdicionales NVARCHAR(500) NOT NULL CONSTRAINT DF_Solicitudes_Detalles DEFAULT '',
        AccesoriosJson NVARCHAR(MAX) NOT NULL CONSTRAINT DF_Solicitudes_Accesorios DEFAULT '[]',
        Notas NVARCHAR(1000) NOT NULL CONSTRAINT DF_Solicitudes_Notas DEFAULT '',
        Estado TINYINT NOT NULL CONSTRAINT DF_Solicitudes_Estado DEFAULT 0,
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Solicitudes_Fecha DEFAULT SYSUTCDATETIME(),
        FechaActualizacion DATETIME2 NULL,
        FechaRespuestaCliente DATETIME2 NULL,
        CONSTRAINT FK_Solicitudes_Cliente FOREIGN KEY (ClienteId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT FK_Solicitudes_Shaper FOREIGN KEY (ShaperId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT FK_Solicitudes_Producto FOREIGN KEY (ProductoBaseId) REFERENCES dbo.Productos(Id),
        CONSTRAINT CK_Solicitudes_Estado CHECK (Estado BETWEEN 0 AND 9),
        CONSTRAINT CK_Solicitudes_Precio CHECK (PrecioEstimado >= 0)
    );
    CREATE INDEX IX_Solicitudes_ShaperEstadoFecha ON dbo.SolicitudesPersonalizadas(ShaperId, Estado, FechaCreacion DESC);
    CREATE INDEX IX_Solicitudes_ClienteFecha ON dbo.SolicitudesPersonalizadas(ClienteId, FechaCreacion DESC);
END;
GO
