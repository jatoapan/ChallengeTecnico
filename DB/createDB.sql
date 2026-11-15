IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InventarioProductos')
BEGIN
    CREATE DATABASE InventarioProductos;
END
GO
USE InventarioProductos;
GO

CREATE TABLE Usuario(
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombres VARCHAR(60) NOT NULL,
    apellidos VARCHAR(60) NOT NULL,
    usuario VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(50) NOT NULL UNIQUE,
    passwordHash VARCHAR(255) NOT NULL,
    rol VARCHAR(20) NOT NULL CHECK (rol IN ('Administrador', 'Operador')),
    estado BIT NOT NULL DEFAULT 1,
    fechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Proveedor(
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(60) NOT NULL UNIQUE,
    email VARCHAR(50) NOT NULL,
    celular VARCHAR(10),
    estado BIT NOT NULL DEFAULT 1,
    fechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Producto(
    id INT IDENTITY(1,1) PRIMARY KEY,
    codigo VARCHAR(10) NOT NULL UNIQUE,
    nombre VARCHAR(60) NOT NULL,
    descripcion VARCHAR(255),
    estado BIT NOT NULL DEFAULT 1,
    fechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Lote(
    id INT IDENTITY(1,1) PRIMARY KEY,
    idProducto INT NOT NULL,
    idProveedor INT NOT NULL,
    numeroLote VARCHAR(20) NOT NULL UNIQUE, -- LOT-NNNN-PP
    estado BIT NOT NULL DEFAULT 1,
    fechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FKLoteProducto  FOREIGN KEY (idProducto)  REFERENCES Producto(id),
    CONSTRAINT FKLoteProveedor FOREIGN KEY (idProveedor) REFERENCES Proveedor(id)
);

CREATE TABLE Inventario(
    id INT IDENTITY(1,1) PRIMARY KEY,
    idLote INT NOT NULL UNIQUE,
    costo  DECIMAL(10,2) NOT NULL DEFAULT 0.00 CHECK (costo  >= 0),
    precio DECIMAL(10,2) NOT NULL DEFAULT 0.00 CHECK (precio >= 0),
    stock  INT NOT NULL DEFAULT 0 CHECK (stock >= 0),
    fechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    fechaActualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FKInventarioLote FOREIGN KEY (idLote) REFERENCES Lote(id)
);
GO

CREATE INDEX IXLoteProducto  ON Lote(idProducto);
CREATE INDEX IXLoteProveedor ON Lote(idProveedor);
GO