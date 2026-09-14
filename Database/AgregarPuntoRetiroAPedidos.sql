/* Ejecutar una vez sobre bases existentes. Es idempotente. */
IF COL_LENGTH('dbo.Pedidos','PuntoRetiroId') IS NULL
BEGIN
    ALTER TABLE dbo.Pedidos ADD
        PuntoRetiroId INT NULL,
        PuntoRetiroNombre NVARCHAR(150) NULL,
        PuntoRetiroDireccion NVARCHAR(250) NULL,
        PuntoRetiroCiudad NVARCHAR(120) NULL,
        PuntoRetiroHorario NVARCHAR(250) NULL,
        PuntoRetiroIndicaciones NVARCHAR(500) NULL,
        PuntoRetiroLatitud DECIMAL(9,6) NULL,
        PuntoRetiroLongitud DECIMAL(9,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Pedidos')
      AND name = 'IX_Pedidos_PuntoRetiroId'
)
BEGIN
    CREATE INDEX IX_Pedidos_PuntoRetiroId
        ON dbo.Pedidos(PuntoRetiroId)
        WHERE PuntoRetiroId IS NOT NULL;
END;
GO
