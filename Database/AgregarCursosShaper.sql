SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.CursosShaper','U') IS NULL
BEGIN
 CREATE TABLE dbo.CursosShaper(
  Id INT IDENTITY(1,1) PRIMARY KEY, ShaperId INT NOT NULL,
  Titulo NVARCHAR(140) NOT NULL, Resumen NVARCHAR(280) NOT NULL,
  Descripcion NVARCHAR(MAX) NOT NULL, Modalidad NVARCHAR(30) NOT NULL,
  Ubicacion NVARCHAR(180) NOT NULL, FechaInicio DATETIME2 NOT NULL,
  DuracionHoras DECIMAL(6,1) NOT NULL, Cupos INT NOT NULL, Precio DECIMAL(10,2) NOT NULL,
  ImagenUrl NVARCHAR(600) NULL, Publicado BIT NOT NULL CONSTRAINT DF_CursosShaper_Publicado DEFAULT 1,
  FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_CursosShaper_Fecha DEFAULT SYSUTCDATETIME(),
  FechaActualizacion DATETIME2 NULL,
  CONSTRAINT FK_CursosShaper_Usuarios FOREIGN KEY(ShaperId) REFERENCES dbo.Usuarios(Id),
  CONSTRAINT CK_CursosShaper_Cupos CHECK(Cupos>0), CONSTRAINT CK_CursosShaper_Precio CHECK(Precio>0),
  CONSTRAINT CK_CursosShaper_Modalidad CHECK(Modalidad IN(N'Presencial',N'Online',N'Híbrido'))
 );
 CREATE INDEX IX_CursosShaper_Perfil ON dbo.CursosShaper(ShaperId,Publicado,FechaInicio);
END;

IF OBJECT_ID('dbo.InscripcionesCursos','U') IS NULL
BEGIN
 CREATE TABLE dbo.InscripcionesCursos(
  Id INT IDENTITY(1,1) PRIMARY KEY, CursoId INT NOT NULL, ClienteId INT NOT NULL,
  PrecioSnapshot DECIMAL(10,2) NOT NULL, PorcentajeComision DECIMAL(5,2) NOT NULL,
  ComisionPlataforma DECIMAL(10,2) NOT NULL, NetoShaper DECIMAL(10,2) NOT NULL,
  Estado TINYINT NOT NULL CONSTRAINT DF_InscripcionesCursos_Estado DEFAULT 0,
  MercadoPagoPreferenceId NVARCHAR(150) NULL, MercadoPagoPaymentId NVARCHAR(150) NULL,
  FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_InscripcionesCursos_Fecha DEFAULT SYSUTCDATETIME(),
  FechaPago DATETIME2 NULL,
  CONSTRAINT FK_InscripcionesCursos_Curso FOREIGN KEY(CursoId) REFERENCES dbo.CursosShaper(Id),
  CONSTRAINT FK_InscripcionesCursos_Cliente FOREIGN KEY(ClienteId) REFERENCES dbo.Usuarios(Id),
  CONSTRAINT UQ_InscripcionesCursos_ClienteCurso UNIQUE(CursoId,ClienteId),
  CONSTRAINT CK_InscripcionesCursos_Estado CHECK(Estado IN(0,1,2,3))
 );
 CREATE INDEX IX_InscripcionesCursos_CursoEstado ON dbo.InscripcionesCursos(CursoId,Estado);
 CREATE INDEX IX_InscripcionesCursos_Cliente ON dbo.InscripcionesCursos(ClienteId,FechaCreacion DESC);
END;

COMMIT;
