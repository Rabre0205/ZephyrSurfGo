/* ============================================================
   ESQUEMA DE BASE DE DATOS - SurfDB
   Estrategias:
     - Usuario / Shaper  -> TPH (Table per Hierarchy)
     - Producto (Leash, Pad, Quilla, Tabla, Traje) -> TPT (Table per Type)
     - Enums -> tablas de catálogo
     - Auditoría -> tabla dedicada por cada tabla auditada, poblada
       automáticamente por triggers (INSERT/UPDATE/DELETE)
     - Soft delete -> solo en Productos, columna BIT "DELETED"
   ============================================================ */

IF DB_ID('SurfDB') IS NULL
BEGIN
    CREATE DATABASE SurfDB;
END
GO

USE SurfDB;
GO

/* ============================================================
   1. TABLAS DE CATÁLOGO (equivalentes a los enums de C#)
   ============================================================ */

CREATE TABLE Paises (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_Paises_Id PRIMARY KEY (Id)
);
GO
INSERT INTO Paises (Id, Nombre) VALUES (0, N'Uruguay'), (1, N'Argentina'), (2, N'Brasil');
GO

CREATE TABLE TiposDeUsuario (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_TiposDeUsuario_Id PRIMARY KEY (Id)
);
GO
INSERT INTO TiposDeUsuario (Id, Nombre) VALUES (0, N'Shaper'), (1, N'Cliente');
GO

CREATE TABLE SistemasDeEncaje (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_SistemasDeEncaje_Id PRIMARY KEY (Id)
);
GO
INSERT INTO SistemasDeEncaje (Id, Nombre) VALUES (0, N'FSS2'), (1, N'Future');
GO

CREATE TABLE TiposDeOla (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_TiposDeOla_Id PRIMARY KEY (Id)
);
GO
INSERT INTO TiposDeOla (Id, Nombre) VALUES (0, N'Plana'), (1, N'Power'), (2, N'Chica');
GO

CREATE TABLE EstilosDeSurf (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_EstilosDeSurf_Id PRIMARY KEY (Id)
);
GO
INSERT INTO EstilosDeSurf (Id, Nombre) VALUES (0, N'Agresivo'), (1, N'Fluido'), (2, N'Versatil'), (3, N'Recreativo');
GO

CREATE TABLE Experiencias (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_Experiencias_Id PRIMARY KEY (Id)
);
GO
INSERT INTO Experiencias (Id, Nombre) VALUES (0, N'SinExperiencia'), (1, N'Iniciado'), (2, N'Intermedio'), (3, N'Avanzado');
GO

CREATE TABLE Generos (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_Generos_Id PRIMARY KEY (Id)
);
GO
INSERT INTO Generos (Id, Nombre) VALUES (0, N'Masculino'), (1, N'Femenino'), (2, N'Unisex');
GO

CREATE TABLE Talles (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(10) NOT NULL UNIQUE,
    CONSTRAINT PK_Talles_Id PRIMARY KEY (Id)
);
GO
INSERT INTO Talles (Id, Nombre) VALUES (0, N'XXS'), (1, N'XS'), (2, N'S'), (3, N'M'), (4, N'L'), (5, N'XL'), (6, N'XXL');
GO
CREATE TABLE EstadosPedido (
    Id TINYINT PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL
);
Go

INSERT INTO EstadosPedido (Id, Nombre) VALUES
    (0, 'Pendiente'),
    (1, 'Aprobado'),
    (2, 'Rechazado'),
    (3, 'Cancelado'),
    (4, 'Completado');
GO


/* ============================================================
   2. USUARIOS (TPH: Usuario + Shaper en una sola tabla)
   ============================================================ */

CREATE TABLE Usuarios (
    Id               INT IDENTITY(1,1) NOT NULL,
    Email            NVARCHAR(150)  NOT NULL,
    Contrasenia      NVARCHAR(255)  NOT NULL,   -- Texto plano por ahora
    Nombre           NVARCHAR(150)  NOT NULL,
    PaisId           TINYINT        NOT NULL,
    TipoDeUsuarioId  TINYINT        NOT NULL,

    -- Columnas exclusivas de Shaper (NULL para Cliente)
    NombreDeNegosio  NVARCHAR(150)  NULL,
    Contacto         NVARCHAR(150)  NULL,
    LogoUrl          NVARCHAR(500)  NULL,

    CONSTRAINT PK_Usuario_Id PRIMARY KEY (Id),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
    CONSTRAINT FK_Usuarios_Paises FOREIGN KEY (PaisId) REFERENCES Paises(Id),
    CONSTRAINT FK_Usuarios_TiposDeUsuario FOREIGN KEY (TipoDeUsuarioId) REFERENCES TiposDeUsuario(Id),

    CONSTRAINT CK_Usuarios_DatosShaper CHECK (
        (TipoDeUsuarioId = 0 AND NombreDeNegosio IS NOT NULL AND Contacto IS NOT NULL)
        OR
        (TipoDeUsuarioId = 1 AND NombreDeNegosio IS NULL AND Contacto IS NULL AND LogoUrl IS NULL)
    )
);
GO

CREATE NONCLUSTERED INDEX IX_Usuarios_Shapers
    ON Usuarios (Id)
    INCLUDE (Nombre, NombreDeNegosio, Contacto, LogoUrl)
    WHERE TipoDeUsuarioId = 0;
GO

CREATE TABLE UsuariosAuditoria (
    Id                INT IDENTITY(1,1) NOT NULL,
    UsuarioAfectadoId INT           NOT NULL,
    Accion            NVARCHAR(10)  NOT NULL,
    DatosAnteriores   NVARCHAR(MAX) NULL,
    DatosNuevos       NVARCHAR(MAX) NULL,
    FechaAccion       DATETIME2     NOT NULL CONSTRAINT DF_UsuariosAuditoria_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_UsuariosAuditoria_Id PRIMARY KEY (Id),
    CONSTRAINT CK_UsuariosAuditoria_Accion CHECK (Accion IN (N'INSERT', N'UPDATE', N'DELETE'))
);
GO

CREATE OR ALTER TRIGGER TRG_Usuarios_Auditoria
ON Usuarios
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO UsuariosAuditoria (UsuarioAfectadoId, Accion, DatosAnteriores, DatosNuevos)
        SELECT d.Id, N'UPDATE',
               (SELECT * FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON AUTO),
               (SELECT * FROM inserted i2 WHERE i2.Id = d.Id FOR JSON AUTO)
        FROM deleted d;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO UsuariosAuditoria (UsuarioAfectadoId, Accion, DatosNuevos)
        SELECT i.Id, N'INSERT',
               (SELECT * FROM inserted i2 WHERE i2.Id = i.Id FOR JSON AUTO)
        FROM inserted i;
    END
    ELSE
    BEGIN
        INSERT INTO UsuariosAuditoria (UsuarioAfectadoId, Accion, DatosAnteriores)
        SELECT d.Id, N'DELETE',
               (SELECT * FROM deleted d2 WHERE d2.Id = d.Id FOR JSON AUTO)
        FROM deleted d;
    END
END
GO

/* ============================================================
   3. CREDENCIALESMERCADOPAGO
   ============================================================ */

CREATE TABLE CredencialesMercadoPago (
    UsuarioId           INT,
    MercadoPagoUserId   BIGINT        NOT NULL,
    AccessTokenCifrado  NVARCHAR(MAX) NOT NULL,
    RefreshTokenCifrado NVARCHAR(MAX) NOT NULL,
    TokenExpira         DATETIME2     NOT NULL,

    CONSTRAINT PK_CredencialesMercadoPago_UsuarioId PRIMARY KEY (UsuarioId),
    CONSTRAINT FK_CredencialesMercadoPago_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id)
);
GO

CREATE OR ALTER TRIGGER TRG_CredencialesMercadoPago_ValidarShaper
ON CredencialesMercadoPago
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN Usuarios u ON u.Id = i.UsuarioId
        WHERE u.TipoDeUsuarioId <> 0
    )
    BEGIN
        RAISERROR (N'CredencialesMercadoPago solo puede pertenecer a un Usuario de tipo Shaper.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO

CREATE TABLE CredencialesMercadoPagoAuditoria (
    Id                INT IDENTITY(1,1) NOT NULL,
    UsuarioIdAfectado INT           NOT NULL,
    Accion            NVARCHAR(10)  NOT NULL,
    DatosAnteriores   NVARCHAR(MAX) NULL,
    DatosNuevos       NVARCHAR(MAX) NULL,
    FechaAccion       DATETIME2     NOT NULL CONSTRAINT DF_CredencialesMPAuditoria_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_CredencialesMercadoPagoAuditoria_Id PRIMARY KEY (Id),
    CONSTRAINT CK_CredencialesMPAuditoria_Accion CHECK (Accion IN (N'INSERT', N'UPDATE', N'DELETE'))
);
GO

CREATE OR ALTER TRIGGER TRG_CredencialesMercadoPago_Auditoria
ON CredencialesMercadoPago
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO CredencialesMercadoPagoAuditoria (UsuarioIdAfectado, Accion, DatosAnteriores, DatosNuevos)
        SELECT d.UsuarioId, N'UPDATE',
               (SELECT * FROM deleted  d2 WHERE d2.UsuarioId = d.UsuarioId FOR JSON AUTO),
               (SELECT * FROM inserted i2 WHERE i2.UsuarioId = d.UsuarioId FOR JSON AUTO)
        FROM deleted d;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO CredencialesMercadoPagoAuditoria (UsuarioIdAfectado, Accion, DatosNuevos)
        SELECT i.UsuarioId, N'INSERT',
               (SELECT * FROM inserted i2 WHERE i2.UsuarioId = i.UsuarioId FOR JSON AUTO)
        FROM inserted i;
    END
    ELSE
    BEGIN
        INSERT INTO CredencialesMercadoPagoAuditoria (UsuarioIdAfectado, Accion, DatosAnteriores)
        SELECT d.UsuarioId, N'DELETE',
               (SELECT * FROM deleted d2 WHERE d2.UsuarioId = d.UsuarioId FOR JSON AUTO)
        FROM deleted d;
    END
END
GO

/* ============================================================
   4. PRODUCTOS (TPT: tabla base + una tabla hija por tipo)
   Soft delete: SOLO Productos tiene la columna BIT "DELETED".
   ============================================================ */

CREATE TABLE Productos (
    Id           INT IDENTITY(1,1) NOT NULL,
    Titulo       NVARCHAR(150)     NOT NULL,
    Subtitulo    NVARCHAR(200)     NULL,
    Precio       DECIMAL(10,2)     NOT NULL,
    Descripcion  NVARCHAR(MAX)     NULL,
    ImagenUrl    NVARCHAR(500)     NULL,
    ShaperId     INT               NOT NULL,
    TipoProducto NVARCHAR(20)      NOT NULL,
    DELETED      BIT               NOT NULL CONSTRAINT DF_Productos_DELETED DEFAULT 0,

    CONSTRAINT PK_Productos_Id PRIMARY KEY (Id),
    CONSTRAINT FK_Productos_Usuarios FOREIGN KEY (ShaperId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_Productos_Precio CHECK (Precio > 0),
    CONSTRAINT CK_Productos_TipoProducto CHECK (
        TipoProducto IN (N'Leash', N'Pad', N'Quilla', N'Tabla', N'Traje')
    )
);
GO

CREATE NONCLUSTERED INDEX IX_Productos_ShaperId ON Productos (ShaperId);
GO
CREATE NONCLUSTERED INDEX IX_Productos_TipoProducto ON Productos (TipoProducto);
GO

CREATE TABLE ProductosAuditoria (
    Id                 INT IDENTITY(1,1) NOT NULL,
    ProductoAfectadoId INT           NOT NULL,
    Accion             NVARCHAR(10)  NOT NULL,
    DatosAnteriores    NVARCHAR(MAX) NULL,
    DatosNuevos        NVARCHAR(MAX) NULL,
    FechaAccion        DATETIME2     NOT NULL CONSTRAINT DF_ProductosAuditoria_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_ProductosAuditoria_Id PRIMARY KEY (Id),
    CONSTRAINT CK_ProductosAuditoria_Accion CHECK (Accion IN (N'INSERT', N'UPDATE', N'DELETE'))
);
GO

CREATE OR ALTER TRIGGER TRG_Productos_Auditoria
ON Productos
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO ProductosAuditoria (ProductoAfectadoId, Accion, DatosAnteriores, DatosNuevos)
        SELECT d.Id, N'UPDATE',
               (SELECT * FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON AUTO),
               (SELECT * FROM inserted i2 WHERE i2.Id = d.Id FOR JSON AUTO)
        FROM deleted d;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO ProductosAuditoria (ProductoAfectadoId, Accion, DatosNuevos)
        SELECT i.Id, N'INSERT',
               (SELECT * FROM inserted i2 WHERE i2.Id = i.Id FOR JSON AUTO)
        FROM inserted i;
    END
    ELSE
    BEGIN
        INSERT INTO ProductosAuditoria (ProductoAfectadoId, Accion, DatosAnteriores)
        SELECT d.Id, N'DELETE',
               (SELECT * FROM deleted d2 WHERE d2.Id = d.Id FOR JSON AUTO)
        FROM deleted d;
    END
END
GO

-- ShaperId debe apuntar a un Usuario tipo Shaper (integridad reforzada en la BD)
CREATE OR ALTER TRIGGER TRG_Productos_ValidarShaper
ON Productos
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN Usuarios u ON u.Id = i.ShaperId
        WHERE u.TipoDeUsuarioId <> 0
    )
    BEGIN
        RAISERROR (N'ShaperId debe corresponder a un Usuario de tipo Shaper.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO

/* ------------------------------------------------------------
   4.1 Tablas hijas de Productos (TPT), cada una con su propia
       tabla de auditoría y trigger.
   ------------------------------------------------------------ */

-- Leash
CREATE TABLE Leashes (
    ProductoId              INT NOT NULL,
    LargoDeTablaRecomendado INT NOT NULL,
    Stock                   INT NOT NULL DEFAULT 0,

    CONSTRAINT PK_Leashes_ProductoId PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Leashes_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Leashes_Largo CHECK (LargoDeTablaRecomendado > 0)
);
GO

CREATE TABLE LeashesAuditoria (
    Id                 INT IDENTITY(1,1) NOT NULL,
    ProductoAfectadoId INT           NOT NULL,
    Accion             NVARCHAR(10)  NOT NULL,
    DatosAnteriores    NVARCHAR(MAX) NULL,
    DatosNuevos        NVARCHAR(MAX) NULL,
    FechaAccion        DATETIME2     NOT NULL CONSTRAINT DF_LeashesAuditoria_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_LeashesAuditoria_Id PRIMARY KEY (Id),
    CONSTRAINT CK_LeashesAuditoria_Accion CHECK (Accion IN (N'INSERT', N'UPDATE', N'DELETE'))
);
GO

CREATE OR ALTER TRIGGER TRG_Leashes_Auditoria
ON Leashes
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO LeashesAuditoria (ProductoAfectadoId, Accion, DatosAnteriores, DatosNuevos)
        SELECT d.ProductoId, N'UPDATE',
               (SELECT * FROM deleted  d2 WHERE d2.ProductoId = d.ProductoId FOR JSON AUTO),
               (SELECT * FROM inserted i2 WHERE i2.ProductoId = d.ProductoId FOR JSON AUTO)
        FROM deleted d;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO LeashesAuditoria (ProductoAfectadoId, Accion, DatosNuevos)
        SELECT i.ProductoId, N'INSERT',
               (SELECT * FROM inserted i2 WHERE i2.ProductoId = i.ProductoId FOR JSON AUTO)
        FROM inserted i;
    END
    ELSE
    BEGIN
        INSERT INTO LeashesAuditoria (ProductoAfectadoId, Accion, DatosAnteriores)
        SELECT d.ProductoId, N'DELETE',
               (SELECT * FROM deleted d2 WHERE d2.ProductoId = d.ProductoId FOR JSON AUTO)
        FROM deleted d;
    END
END
GO

-- Pad
CREATE TABLE Pads (
    ProductoId INT           NOT NULL,
    Largo      INT           NOT NULL,
    Ancho      INT           NOT NULL,
    Material   NVARCHAR(100) NOT NULL,
    Stock      INT NOT NULL DEFAULT 0,

    CONSTRAINT PK_Pads_ProductoId PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Pads_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Pads_Medidas CHECK (Largo > 0 AND Ancho > 0)
);
GO

CREATE TABLE PadsAuditoria (
    Id                 INT IDENTITY(1,1) NOT NULL,
    ProductoAfectadoId INT           NOT NULL,
    Accion             NVARCHAR(10)  NOT NULL,
    DatosAnteriores    NVARCHAR(MAX) NULL,
    DatosNuevos        NVARCHAR(MAX) NULL,
    FechaAccion        DATETIME2     NOT NULL CONSTRAINT DF_PadsAuditoria_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_PadsAuditoria_Id PRIMARY KEY (Id),
    CONSTRAINT CK_PadsAuditoria_Accion CHECK (Accion IN (N'INSERT', N'UPDATE', N'DELETE'))
);
GO

CREATE OR ALTER TRIGGER TRG_Pads_Auditoria
ON Pads
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO PadsAuditoria (ProductoAfectadoId, Accion, DatosAnteriores, DatosNuevos)
        SELECT d.ProductoId, N'UPDATE',
               (SELECT * FROM deleted  d2 WHERE d2.ProductoId = d.ProductoId FOR JSON AUTO),
               (SELECT * FROM inserted i2 WHERE i2.ProductoId = d.ProductoId FOR JSON AUTO)
        FROM deleted d;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO PadsAuditoria (ProductoAfectadoId, Accion, DatosNuevos)
        SELECT i.ProductoId, N'INSERT',
               (SELECT * FROM inserted i2 WHERE i2.ProductoId = i.ProductoId FOR JSON AUTO)
        FROM inserted i;
    END
    ELSE
    BEGIN
        INSERT INTO PadsAuditoria (ProductoAfectadoId, Accion, DatosAnteriores)
        SELECT d.ProductoId, N'DELETE',
               (SELECT * FROM deleted d2 WHERE d2.ProductoId = d.ProductoId FOR JSON AUTO)
        FROM deleted d;
    END
END
GO

-- Quilla
CREATE TABLE Quillas (
    ProductoId        INT     NOT NULL,
    SistemaDeEncajeId TINYINT NOT NULL,
    Stock             INT NOT NULL DEFAULT 0,

    CONSTRAINT PK_Quillas_ProductoId PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Quillas_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Quillas_SistemasDeEncaje FOREIGN KEY (SistemaDeEncajeId) REFERENCES SistemasDeEncaje(Id)
);
GO

CREATE TABLE QuillasAuditoria (
    Id                 INT IDENTITY(1,1) NOT NULL,
    ProductoAfectadoId INT           NOT NULL,
    Accion             NVARCHAR(10)  NOT NULL,
    DatosAnteriores    NVARCHAR(MAX) NULL,
    DatosNuevos        NVARCHAR(MAX) NULL,
    FechaAccion        DATETIME2     NOT NULL CONSTRAINT DF_QuillasAuditoria_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_QuillasAuditoria_Id PRIMARY KEY (Id),
    CONSTRAINT CK_QuillasAuditoria_Accion CHECK (Accion IN (N'INSERT', N'UPDATE', N'DELETE'))
);
GO

CREATE OR ALTER TRIGGER TRG_Quillas_Auditoria
ON Quillas
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO QuillasAuditoria (ProductoAfectadoId, Accion, DatosAnteriores, DatosNuevos)
        SELECT d.ProductoId, N'UPDATE',
               (SELECT * FROM deleted  d2 WHERE d2.ProductoId = d.ProductoId FOR JSON AUTO),
               (SELECT * FROM inserted i2 WHERE i2.ProductoId = d.ProductoId FOR JSON AUTO)
        FROM deleted d;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO QuillasAuditoria (ProductoAfectadoId, Accion, DatosNuevos)
        SELECT i.ProductoId, N'INSERT',
               (SELECT * FROM inserted i2 WHERE i2.ProductoId = i.ProductoId FOR JSON AUTO)
        FROM inserted i;
    END
    ELSE
    BEGIN
        INSERT INTO QuillasAuditoria (ProductoAfectadoId, Accion, DatosAnteriores)
        SELECT d.ProductoId, N'DELETE',
               (SELECT * FROM deleted d2 WHERE d2.ProductoId = d.ProductoId FOR JSON AUTO)
        FROM deleted d;
    END
END
GO

-- Tabla
CREATE TABLE Tablas (
    ProductoId        INT           NOT NULL,
    Altura            NVARCHAR(20)  NOT NULL,
    Ancho             INT           NOT NULL,
    Volumen           DECIMAL(5,2)  NOT NULL,
    SistemaDeEncajeId TINYINT       NOT NULL,
    TipoDeOlaId       TINYINT       NOT NULL,
    EstiloDeSurfId    TINYINT       NOT NULL,
    PesoMinimo        INT           NOT NULL,
    PesoMaximo        INT           NOT NULL,
    ExperienciaId     TINYINT       NOT NULL,
    ImagenAtrasUrl    NVARCHAR(500) NULL,
    Disponible        BIT NOT NULL DEFAULT 1,

    CONSTRAINT PK_Tablas_ProductoId PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Tablas_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Tablas_SistemasDeEncaje FOREIGN KEY (SistemaDeEncajeId) REFERENCES SistemasDeEncaje(Id),
    CONSTRAINT FK_Tablas_TiposDeOla FOREIGN KEY (TipoDeOlaId) REFERENCES TiposDeOla(Id),
    CONSTRAINT FK_Tablas_EstilosDeSurf FOREIGN KEY (EstiloDeSurfId) REFERENCES EstilosDeSurf(Id),
    CONSTRAINT FK_Tablas_Experiencias FOREIGN KEY (ExperienciaId) REFERENCES Experiencias(Id),
    CONSTRAINT CK_Tablas_Peso CHECK (PesoMinimo > 0 AND PesoMaximo >= PesoMinimo),
    CONSTRAINT CK_Tablas_Ancho CHECK (Ancho > 0),
    CONSTRAINT CK_Tablas_Volumen CHECK (Volumen > 0)
);
GO

CREATE TABLE TablasAuditoria (
    Id                 INT IDENTITY(1,1) NOT NULL,
    ProductoAfectadoId INT           NOT NULL,
    Accion             NVARCHAR(10)  NOT NULL,
    DatosAnteriores    NVARCHAR(MAX) NULL,
    DatosNuevos        NVARCHAR(MAX) NULL,
    FechaAccion        DATETIME2     NOT NULL CONSTRAINT DF_TablasAuditoria_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_TablasAuditoria_Id PRIMARY KEY (Id),
    CONSTRAINT CK_TablasAuditoria_Accion CHECK (Accion IN (N'INSERT', N'UPDATE', N'DELETE'))
);
GO

CREATE OR ALTER TRIGGER TRG_Tablas_Auditoria
ON Tablas
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO TablasAuditoria (ProductoAfectadoId, Accion, DatosAnteriores, DatosNuevos)
        SELECT d.ProductoId, N'UPDATE',
               (SELECT * FROM deleted  d2 WHERE d2.ProductoId = d.ProductoId FOR JSON AUTO),
               (SELECT * FROM inserted i2 WHERE i2.ProductoId = d.ProductoId FOR JSON AUTO)
        FROM deleted d;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO TablasAuditoria (ProductoAfectadoId, Accion, DatosNuevos)
        SELECT i.ProductoId, N'INSERT',
               (SELECT * FROM inserted i2 WHERE i2.ProductoId = i.ProductoId FOR JSON AUTO)
        FROM inserted i;
    END
    ELSE
    BEGIN
        INSERT INTO TablasAuditoria (ProductoAfectadoId, Accion, DatosAnteriores)
        SELECT d.ProductoId, N'DELETE',
               (SELECT * FROM deleted d2 WHERE d2.ProductoId = d.ProductoId FOR JSON AUTO)
        FROM deleted d;
    END
END
GO

-- Traje
CREATE TABLE Trajes (
    ProductoId  INT          NOT NULL,
    GeneroId    TINYINT      NOT NULL,
    Espesor     INT          NOT NULL,
    TalleId     TINYINT      NOT NULL,
    Temperatura NVARCHAR(50) NULL,
    Stock       INT NOT NULL DEFAULT 0,

    CONSTRAINT PK_Trajes_ProductoId PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Trajes_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Trajes_Generos FOREIGN KEY (GeneroId) REFERENCES Generos(Id),
    CONSTRAINT FK_Trajes_Talles FOREIGN KEY (TalleId) REFERENCES Talles(Id),
    CONSTRAINT CK_Trajes_Espesor CHECK (Espesor > 0)
);
GO

CREATE TABLE TrajesAuditoria (
    Id                 INT IDENTITY(1,1) NOT NULL,
    ProductoAfectadoId INT           NOT NULL,
    Accion             NVARCHAR(10)  NOT NULL,
    DatosAnteriores    NVARCHAR(MAX) NULL,
    DatosNuevos        NVARCHAR(MAX) NULL,
    FechaAccion        DATETIME2     NOT NULL CONSTRAINT DF_TrajesAuditoria_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_TrajesAuditoria_Id PRIMARY KEY (Id),
    CONSTRAINT CK_TrajesAuditoria_Accion CHECK (Accion IN (N'INSERT', N'UPDATE', N'DELETE'))
);
GO

CREATE OR ALTER TRIGGER TRG_Trajes_Auditoria
ON Trajes
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO TrajesAuditoria (ProductoAfectadoId, Accion, DatosAnteriores, DatosNuevos)
        SELECT d.ProductoId, N'UPDATE',
               (SELECT * FROM deleted  d2 WHERE d2.ProductoId = d.ProductoId FOR JSON AUTO),
               (SELECT * FROM inserted i2 WHERE i2.ProductoId = d.ProductoId FOR JSON AUTO)
        FROM deleted d;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO TrajesAuditoria (ProductoAfectadoId, Accion, DatosNuevos)
        SELECT i.ProductoId, N'INSERT',
               (SELECT * FROM inserted i2 WHERE i2.ProductoId = i.ProductoId FOR JSON AUTO)
        FROM inserted i;
    END
    ELSE
    BEGIN
        INSERT INTO TrajesAuditoria (ProductoAfectadoId, Accion, DatosAnteriores)
        SELECT d.ProductoId, N'DELETE',
               (SELECT * FROM deleted d2 WHERE d2.ProductoId = d.ProductoId FOR JSON AUTO)
        FROM deleted d;
    END
END
GO
-- Carrito
CREATE TABLE CarritoItems (
    Id INT IDENTITY,
    UsuarioId INT NOT NULL REFERENCES Usuarios(Id),
    ProductoId INT NOT NULL REFERENCES Productos(Id),
    Cantidad INT NOT NULL DEFAULT 1,
    FechaAgregado DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    
    CONSTRAINT PK_CarritoItems__Id Primary Key (Id),
    CONSTRAINT UQ_CarritoItems_UsuarioProducto UNIQUE (UsuarioId, ProductoId)
);
GO

CREATE TRIGGER TRG_CarritoItems_ValidarCantidadTabla
ON CarritoItems
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Productos p ON p.Id = i.ProductoId
        WHERE p.TipoProducto = 'Tabla' AND i.Cantidad <> 1
    )
    BEGIN
        RAISERROR('Una Tabla es una pieza única: la cantidad debe ser 1.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

-- Pedidos
CREATE TABLE Pedidos (
    Id INT IDENTITY,
    ClienteId INT NOT NULL REFERENCES Usuarios(Id),
    ShaperId INT NOT NULL REFERENCES Usuarios(Id),
    EstadoPedidoId TINYINT NOT NULL REFERENCES EstadosPedido(Id) DEFAULT 0,
    Total DECIMAL(10,2) NOT NULL,
    ComisionPlataforma DECIMAL(10,2) NOT NULL,
    MercadoPagoPreferenceId NVARCHAR(100) NULL,
    MercadoPagoPaymentId NVARCHAR(100) NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaActualizacion DATETIME2 NULL

    CONSTRAINT PK_Pedidos_Id Primary Key (Id)
);
GO

CREATE INDEX IX_Pedidos_ClienteId ON Pedidos(ClienteId);
GO
CREATE INDEX IX_Pedidos_ShaperId ON Pedidos(ShaperId);
GO

-- Pedido -Snapshot del item comprado-
CREATE TABLE PedidoItems (
    Id INT IDENTITY,
    PedidoId INT NOT NULL REFERENCES Pedidos(Id),
    ProductoId INT NOT NULL REFERENCES Productos(Id),
    TituloSnapshot NVARCHAR(150) NOT NULL,
    PrecioUnitarioSnapshot DECIMAL(10,2) NOT NULL,
    Cantidad INT NOT NULL

    CONSTRAINT PK_PedidoItems_Id Primary Key (Id)
);
GO

CREATE INDEX IX_PedidoItems_PedidoId ON PedidoItems(PedidoId);
GO

/* ============================================================
   NOTAS
   ------------------------------------------------------------
   1. Contrasenia en texto plano por decisión actual del proyecto
      (columna dimensionada NVARCHAR(255) para poder hashear luego).

   2. Cada tabla auditada tiene su propia tabla "<Tabla>Auditoria"
      con: Id afectado, Accion (INSERT/UPDATE/DELETE), DatosAnteriores
      y DatosNuevos (JSON con la fila completa vía FOR JSON AUTO) y
      FechaAccion (default SYSDATETIME()). No se registra qué usuario
      realizó la acción.

   3. Soft delete: SOLO Productos tiene la columna DELETED (BIT,
      default 0). El resto de las tablas (Usuarios, hijos de
      Productos, CredencialesMercadoPago) usan DELETE físico, que
      queda registrado igual en su tabla de auditoría correspondiente.
      A partir de ahora, en vez de:
         DELETE FROM Productos WHERE Id = @Id
      usar:
         UPDATE Productos SET DELETED = 1 WHERE Id = @Id
      y filtrar siempre por DELETED = 0 en listados/catálogo.

   4. Los catálogos (Paises, TiposDeUsuario, SistemasDeEncaje,
      TiposDeOla, EstilosDeSurf, Experiencias, Generos, Talles) no
      llevan auditoría ni soft delete; son datos de referencia fijos.
   ============================================================ */