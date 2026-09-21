/* Ejecutar una sola vez en SurfDB después de AgregarSolicitudesPersonalizadas.sql. */
IF OBJECT_ID('dbo.SolicitudesPersonalizadas', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.SolicitudesPersonalizadas', 'FechaRespuestaCliente') IS NULL
        ALTER TABLE dbo.SolicitudesPersonalizadas
        ADD FechaRespuestaCliente DATETIME2 NULL;

    IF EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.SolicitudesPersonalizadas')
          AND name = 'CK_Solicitudes_Estado')
        ALTER TABLE dbo.SolicitudesPersonalizadas
        DROP CONSTRAINT CK_Solicitudes_Estado;

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.SolicitudesPersonalizadas')
          AND name = 'CK_Solicitudes_Estado')
        ALTER TABLE dbo.SolicitudesPersonalizadas
        ADD CONSTRAINT CK_Solicitudes_Estado CHECK (Estado BETWEEN 0 AND 9);
END;
GO
