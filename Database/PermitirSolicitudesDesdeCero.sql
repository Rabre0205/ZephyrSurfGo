/* Ejecutar una sola vez en SurfDB después de AgregarSolicitudesPersonalizadas.sql.
   Permite crear una tabla personalizada sin asociarla a un producto existente. */
IF OBJECT_ID('dbo.SolicitudesPersonalizadas', 'U') IS NOT NULL
   AND EXISTS (
       SELECT 1
       FROM sys.columns
       WHERE object_id = OBJECT_ID('dbo.SolicitudesPersonalizadas')
         AND name = 'ProductoBaseId'
         AND is_nullable = 0
   )
BEGIN
    ALTER TABLE dbo.SolicitudesPersonalizadas
        ALTER COLUMN ProductoBaseId INT NULL;
END;
GO
