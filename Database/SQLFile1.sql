/* ============================================================
   ESQUEMA DE BASE DE DATOS - SurfDB
   Estrategias:
     - Usuario / Shaper  -> TPH (Table per Hierarchy)
     - Producto (Leash, Pad, Quilla, Tabla, Traje) -> TPT (Table per Type)
     - Enums -> tablas de catálogo (permiten agregar valores sin migrar)
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
   Los Id coinciden con el valor int del enum en C# para que el
   mapeo con EF Core sea directo (Id = (int)EnumValue).
   ============================================================ */

CREATE TABLE Paises (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_Paises_Id PRIMARY KEY (Id)
);
GO
INSERT INTO Paises (Id, Nombre) VALUES
(0, N'Uruguay'),
(1, N'Argentina'),
(2, N'Brasil');
GO

CREATE TABLE TiposDeUsuario (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_TiposDeUsuario_Id PRIMARY KEY (Id)
);
GO
INSERT INTO TiposDeUsuario (Id, Nombre) VALUES
(0, N'Shaper'),
(1, N'Cliente');
GO

CREATE TABLE SistemasDeEncaje (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_SistemasDeEncaje_Id PRIMARY KEY (Id)
);
GO
INSERT INTO SistemasDeEncaje (Id, Nombre) VALUES
(0, N'FSS2'),
(1, N'Future');
GO

CREATE TABLE TiposDeOla (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_TiposDeOla_Id PRIMARY KEY (Id)
);
GO
INSERT INTO TiposDeOla (Id, Nombre) VALUES
(0, N'Plana'),
(1, N'Power'),
(2, N'Chica');
GO

CREATE TABLE EstilosDeSurf (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_EstilosDeSurf_Id PRIMARY KEY (Id)
);
GO
INSERT INTO EstilosDeSurf (Id, Nombre) VALUES
(0, N'Agresivo'),
(1, N'Fluido'),
(2, N'Versatil'),
(3, N'Recreativo');
GO

CREATE TABLE Experiencias (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_Experiencias_Id PRIMARY KEY (Id)
);
GO
INSERT INTO Experiencias (Id, Nombre) VALUES
(0, N'SinExperiencia'),
(1, N'Iniciado'),
(2, N'Intermedio'),
(3, N'Avanzado');
GO

CREATE TABLE Generos (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_Generos_Id PRIMARY KEY (Id)
);
GO
INSERT INTO Generos (Id, Nombre) VALUES
(0, N'Masculino'),
(1, N'Femenino'),
(2, N'Unisex');
GO

CREATE TABLE Talles (
    Id     TINYINT      NOT NULL,
    Nombre NVARCHAR(10) NOT NULL UNIQUE,
    CONSTRAINT PK_Talles_Id PRIMARY KEY (Id)
);
GO
INSERT INTO Talles (Id, Nombre) VALUES
(0, N'XXS'),
(1, N'XS'),
(2, N'S'),
(3, N'M'),
(4, N'L'),
(5, N'XL'),
(6, N'XXL');
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

    -- Si es Shaper (Id=0), los campos de negocio son obligatorios
    CONSTRAINT CK_Usuarios_DatosShaper CHECK (
        TipoDeUsuarioId <> 0
        OR (NombreDeNegosio IS NOT NULL AND Contacto IS NOT NULL)
    )
);
GO

/* ============================================================
   3. PRODUCTOS (TPT: tabla base + una tabla hija por tipo)
   ============================================================ */

CREATE TABLE Productos (
    Id           INT IDENTITY(1,1) NOT NULL,
    Titulo       NVARCHAR(150)     NOT NULL,
    Subtitulo    NVARCHAR(200)     NULL,
    Precio       DECIMAL(10,2)     NOT NULL,
    Descripcion  NVARCHAR(MAX)     NULL,
    ImagenUrl    NVARCHAR(500)     NULL,
    ShaperId     INT               NOT NULL,

    -- Discriminador opcional, útil para consultas rápidas sin hacer JOIN a las 5 tablas hijas
    TipoProducto NVARCHAR(20)      NOT NULL,

    CONSTRAINT PK_Productos_Id PRIMARY KEY (Id),
    CONSTRAINT FK_Productos_Usuarios FOREIGN KEY (ShaperId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_Productos_Precio CHECK (Precio > 0),
    CONSTRAINT CK_Productos_TipoProducto CHECK (
        TipoProducto IN (N'Leash', N'Pad', N'Quilla', N'Tabla', N'Traje')
    )
);
GO

-- Leash
CREATE TABLE Leashes (
    ProductoId                  INT NOT NULL,
    LargoDeTablaRecomendado     INT NOT NULL,

    CONSTRAINT PK_Leashes_ProductoId PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Leashes_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Leashes_Largo CHECK (LargoDeTablaRecomendado > 0)
);
GO

-- Pad
CREATE TABLE Pads (
    ProductoId INT           NOT NULL,
    Largo      INT           NOT NULL,
    Ancho      INT           NOT NULL,
    Material   NVARCHAR(100) NOT NULL,

    CONSTRAINT PK_Pads_ProductoId PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Pads_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Pads_Medidas CHECK (Largo > 0 AND Ancho > 0)
);
GO

-- Quilla
CREATE TABLE Quillas (
    ProductoId         INT    NOT NULL,
    SistemaDeEncajeId  TINYINT NOT NULL,

    CONSTRAINT PK_Quillas_ProductoId PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Quillas_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Quillas_SistemasDeEncaje FOREIGN KEY (SistemaDeEncajeId) REFERENCES SistemasDeEncaje(Id)
);
GO

-- Tabla
CREATE TABLE Tablas (
    ProductoId         INT           NOT NULL,
    Altura             NVARCHAR(20)  NOT NULL,   -- string en C# (ej: 6'2")
    Ancho              INT           NOT NULL,
    Volumen            DECIMAL(5,2)  NOT NULL,
    SistemaDeEncajeId  TINYINT       NOT NULL,
    TipoDeOlaId        TINYINT       NOT NULL,
    EstiloDeSurfId     TINYINT       NOT NULL,
    PesoMinimo         INT           NOT NULL,
    PesoMaximo         INT           NOT NULL,
    ExperienciaId      TINYINT       NOT NULL,
    ImagenAtrasUrl     NVARCHAR(500) NULL,

    CONSTRAINT PK_Tablas_ProductoId PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Tablas_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Tablas_SistemasDeEncaje FOREIGN KEY (SistemaDeEncajeId) REFERENCES SistemasDeEncaje(Id),
    CONSTRAINT FK_Tablas_TiposDeOla FOREIGN KEY (TipoDeOlaId) REFERENCES TiposDeOla(Id),
    CONSTRAINT FK_Tablas_EstilosDeSurf FOREIGN KEY (EstiloDeSurfId) REFERENCES EstilosDesurf(Id),
    CONSTRAINT FK_Tablas_Experiencias FOREIGN KEY (ExperienciaId) REFERENCES Experiencias(Id),
    CONSTRAINT CK_Tablas_Peso CHECK (PesoMinimo > 0 AND PesoMaximo >= PesoMinimo),
    CONSTRAINT CK_Tablas_Ancho CHECK (Ancho > 0),
    CONSTRAINT CK_Tablas_Volumen CHECK (Volumen > 0)
);
GO

-- Traje
CREATE TABLE Trajes (
    ProductoId    INT          NOT NULL,
    GeneroId      TINYINT      NOT NULL,
    Espesor       INT          NOT NULL,
    TalleId       TINYINT      NOT NULL,
    Temperatura   NVARCHAR(50) NULL,

    CONSTRAINT PK_Trajes_ProductoId PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Trajes_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Trajes_Generos FOREIGN KEY (GeneroId) REFERENCES Generos(Id),
    CONSTRAINT FK_Trajes_Talles FOREIGN KEY (TalleId) REFERENCES Talles(Id),
    CONSTRAINT CK_Trajes_Espesor CHECK (Espesor > 0)
);
GO

/* ============================================================
   4. TRIGGER DE INTEGRIDAD: ShaperId debe apuntar a un Usuario
      cuyo TipoDeUsuario sea 'Shaper' (Id = 0).
      La app ya valida esto, pero se refuerza en la DB para
      evitar inconsistencias si algún proceso inserta directo.
   ============================================================ */

CREATE OR ALTER TRIGGER TRG_Productos_ValidarShaper
ON Productos
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Usuarios u ON u.Id = i.ShaperId
        WHERE u.TipoDeUsuarioId <> 0   -- 0 = Shaper
    )
    BEGIN
        RAISERROR (N'ShaperId debe corresponder a un Usuario de tipo Shaper.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO

/* ============================================================
   NOTAS
   ------------------------------------------------------------
   1. Contrasenia se guarda en texto plano por decisión actual del
      proyecto. La columna está dimensionada (NVARCHAR(255)) para
      poder pasar a un hash (ej. BCrypt/Argon2) sin migrar el tipo.

   2. Los catálogos (Paises, TiposDeUsuario, SistemasDeEncaje,
      TiposDeOla, EstilosDeSurf, Experiencias, Generos, Talles)
      están pensados para poder agregar filas nuevas sin tocar
      la estructura de las tablas ni los CHECK constraints.
      Si agregan un valor al enum de C#, solo hace falta un
      INSERT en la tabla de catálogo correspondiente con el
      mismo Id que el nuevo valor del enum.

   3. TipoDeTail está definido como enum vacío en el código y no
      se usa en ninguna clase, por lo que no se generó tabla para
      él. Si se llega a usar, avisame y agrego el catálogo.

   4. Longitudes de texto (NVARCHAR) usadas por defecto:
      Email 150, Nombre 150, Titulo 150, Subtitulo 200,
      Contacto/NombreDeNegosio 150, LogoUrl/ImagenUrl/ImagenAtrasUrl 500,
      Material 100, Altura 20, Temperatura 50.
      Son valores razonables pero ajustables según tu UI.
   ============================================================ */