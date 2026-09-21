SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.CursosShaper','U') IS NULL
BEGIN
 CREATE TABLE dbo.CursosShaper(
  Id INT IDENTITY(1,1) PRIMARY KEY, ShaperId INT NOT NULL,
  Titulo NVARCHAR(140) NOT NULL, Resumen NVARCHAR(280) NOT NULL, Descripcion NVARCHAR(MAX) NOT NULL,
  Modalidad NVARCHAR(30) NOT NULL, Ubicacion NVARCHAR(180) NOT NULL, FechaInicio DATETIME2 NOT NULL,
  DuracionHoras DECIMAL(6,1) NOT NULL, Cupos INT NOT NULL, Precio DECIMAL(10,2) NOT NULL,
  ImagenUrl NVARCHAR(600) NULL, InstructorNombre NVARCHAR(140) NOT NULL CONSTRAINT DF_CursosShaper_Instructor DEFAULT N'',
  InstructorBio NVARCHAR(1800) NOT NULL CONSTRAINT DF_CursosShaper_InstructorBio DEFAULT N'',
  PublicoObjetivo NVARCHAR(2500) NOT NULL CONSTRAINT DF_CursosShaper_Publico DEFAULT N'',
  Incluye NVARCHAR(3000) NOT NULL CONSTRAINT DF_CursosShaper_Incluye DEFAULT N'', Requisitos NVARCHAR(1800) NOT NULL CONSTRAINT DF_CursosShaper_Requisitos DEFAULT N'',
  CronogramaUrl NVARCHAR(600) NULL, Contacto NVARCHAR(500) NULL, CuotasMaximas TINYINT NOT NULL CONSTRAINT DF_CursosShaper_Cuotas DEFAULT 1,
  DescuentoAcompanantePorcentaje DECIMAL(5,2) NOT NULL CONSTRAINT DF_CursosShaper_Descuento DEFAULT 0,
  HospedajeDisponible BIT NOT NULL CONSTRAINT DF_CursosShaper_Hospedaje DEFAULT 0,
  Publicado BIT NOT NULL CONSTRAINT DF_CursosShaper_Publicado DEFAULT 1,
  FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_CursosShaper_Fecha DEFAULT SYSUTCDATETIME(), FechaActualizacion DATETIME2 NULL,
  CONSTRAINT FK_CursosShaper_Usuarios FOREIGN KEY(ShaperId) REFERENCES dbo.Usuarios(Id),
  CONSTRAINT CK_CursosShaper_Cupos CHECK(Cupos>0), CONSTRAINT CK_CursosShaper_Precio CHECK(Precio>0),
  CONSTRAINT CK_CursosShaper_Modalidad CHECK(Modalidad IN(N'Presencial',N'Online',N'Híbrido')),
  CONSTRAINT CK_CursosShaper_Cuotas CHECK(CuotasMaximas BETWEEN 1 AND 24),
  CONSTRAINT CK_CursosShaper_Descuento CHECK(DescuentoAcompanantePorcentaje BETWEEN 0 AND 100)
 );
 CREATE INDEX IX_CursosShaper_Perfil ON dbo.CursosShaper(ShaperId,Publicado,FechaInicio);
END;

IF COL_LENGTH('dbo.CursosShaper','InstructorNombre') IS NULL ALTER TABLE dbo.CursosShaper ADD InstructorNombre NVARCHAR(140) NOT NULL CONSTRAINT DF_CursosShaper_Instructor DEFAULT N'';
IF COL_LENGTH('dbo.CursosShaper','InstructorBio') IS NULL ALTER TABLE dbo.CursosShaper ADD InstructorBio NVARCHAR(1800) NOT NULL CONSTRAINT DF_CursosShaper_InstructorBio DEFAULT N'';
IF COL_LENGTH('dbo.CursosShaper','PublicoObjetivo') IS NULL ALTER TABLE dbo.CursosShaper ADD PublicoObjetivo NVARCHAR(2500) NOT NULL CONSTRAINT DF_CursosShaper_Publico DEFAULT N'';
IF COL_LENGTH('dbo.CursosShaper','Incluye') IS NULL ALTER TABLE dbo.CursosShaper ADD Incluye NVARCHAR(3000) NOT NULL CONSTRAINT DF_CursosShaper_Incluye DEFAULT N'';
IF COL_LENGTH('dbo.CursosShaper','Requisitos') IS NULL ALTER TABLE dbo.CursosShaper ADD Requisitos NVARCHAR(1800) NOT NULL CONSTRAINT DF_CursosShaper_Requisitos DEFAULT N'';
IF COL_LENGTH('dbo.CursosShaper','CronogramaUrl') IS NULL ALTER TABLE dbo.CursosShaper ADD CronogramaUrl NVARCHAR(600) NULL;
IF COL_LENGTH('dbo.CursosShaper','Contacto') IS NULL ALTER TABLE dbo.CursosShaper ADD Contacto NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.CursosShaper','CuotasMaximas') IS NULL ALTER TABLE dbo.CursosShaper ADD CuotasMaximas TINYINT NOT NULL CONSTRAINT DF_CursosShaper_Cuotas DEFAULT 1;
IF COL_LENGTH('dbo.CursosShaper','DescuentoAcompanantePorcentaje') IS NULL ALTER TABLE dbo.CursosShaper ADD DescuentoAcompanantePorcentaje DECIMAL(5,2) NOT NULL CONSTRAINT DF_CursosShaper_Descuento DEFAULT 0;
IF COL_LENGTH('dbo.CursosShaper','HospedajeDisponible') IS NULL ALTER TABLE dbo.CursosShaper ADD HospedajeDisponible BIT NOT NULL CONSTRAINT DF_CursosShaper_Hospedaje DEFAULT 0;

IF OBJECT_ID('dbo.CursoModulos','U') IS NULL
 CREATE TABLE dbo.CursoModulos(Id INT IDENTITY PRIMARY KEY,CursoId INT NOT NULL REFERENCES dbo.CursosShaper(Id),Nombre NVARCHAR(120) NOT NULL,Descripcion NVARCHAR(1000) NOT NULL,Encuentros INT NOT NULL,DuracionHoras DECIMAL(6,1) NOT NULL,Precio DECIMAL(10,2) NULL,PermiteCompraIndividual BIT NOT NULL DEFAULT 0,Orden INT NOT NULL DEFAULT 0,CONSTRAINT CK_CursoModulos_Encuentros CHECK(Encuentros>0),CONSTRAINT CK_CursoModulos_Precio CHECK(Precio IS NULL OR Precio>0));

IF OBJECT_ID('dbo.CursoImagenes','U') IS NULL
 CREATE TABLE dbo.CursoImagenes(Id INT IDENTITY PRIMARY KEY,CursoId INT NOT NULL REFERENCES dbo.CursosShaper(Id),ImagenUrl NVARCHAR(600) NOT NULL,Orden INT NOT NULL DEFAULT 0);

IF OBJECT_ID('dbo.InscripcionesCursos','U') IS NULL
BEGIN
 CREATE TABLE dbo.InscripcionesCursos(Id INT IDENTITY(1,1) PRIMARY KEY,CursoId INT NOT NULL,ClienteId INT NOT NULL,ModuloId INT NULL,PrecioSnapshot DECIMAL(10,2) NOT NULL,PorcentajeComision DECIMAL(5,2) NOT NULL,ComisionPlataforma DECIMAL(10,2) NOT NULL,NetoShaper DECIMAL(10,2) NOT NULL,Estado TINYINT NOT NULL CONSTRAINT DF_InscripcionesCursos_Estado DEFAULT 0,MercadoPagoPreferenceId NVARCHAR(150) NULL,MercadoPagoPaymentId NVARCHAR(150) NULL,FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_InscripcionesCursos_Fecha DEFAULT SYSUTCDATETIME(),FechaPago DATETIME2 NULL,CONSTRAINT FK_InscripcionesCursos_Curso FOREIGN KEY(CursoId) REFERENCES dbo.CursosShaper(Id),CONSTRAINT FK_InscripcionesCursos_Cliente FOREIGN KEY(ClienteId) REFERENCES dbo.Usuarios(Id),CONSTRAINT FK_InscripcionesCursos_Modulo FOREIGN KEY(ModuloId) REFERENCES dbo.CursoModulos(Id),CONSTRAINT UQ_InscripcionesCursos_ClienteCurso UNIQUE(CursoId,ClienteId),CONSTRAINT CK_InscripcionesCursos_Estado CHECK(Estado IN(0,1,2,3)));
 CREATE INDEX IX_InscripcionesCursos_CursoEstado ON dbo.InscripcionesCursos(CursoId,Estado);
 CREATE INDEX IX_InscripcionesCursos_Cliente ON dbo.InscripcionesCursos(ClienteId,FechaCreacion DESC);
END;
IF COL_LENGTH('dbo.InscripcionesCursos','ModuloId') IS NULL ALTER TABLE dbo.InscripcionesCursos ADD ModuloId INT NULL CONSTRAINT FK_InscripcionesCursos_Modulo REFERENCES dbo.CursoModulos(Id);

COMMIT;
