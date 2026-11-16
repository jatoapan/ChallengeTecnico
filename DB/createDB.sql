IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InventarioProductos')
BEGIN
    CREATE DATABASE InventarioProductos;
END
GO
USE InventarioProductos;
GO

-- Catálogo de roles. Su nombre viaja en el JWT y las policies de la API lo usan.
CREATE TABLE Rol(
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(20) NOT NULL UNIQUE,
    estado BIT NOT NULL DEFAULT 1,
    fechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Usuario(
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombres VARCHAR(60) NOT NULL,
    apellidos VARCHAR(60) NOT NULL,
    usuario VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(50) NOT NULL UNIQUE,
    passwordHash VARCHAR(255) NOT NULL,
    idRol INT NOT NULL,
    estado BIT NOT NULL DEFAULT 1,
    fechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FKUsuarioRol FOREIGN KEY (idRol) REFERENCES Rol(id)
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

-- Registro de quién cambió qué. Solo se inserta, nunca se edita ni se borra.
-- Incluye los accesos: ingresos, intentos fallidos y bloqueos. En un intento fallido con un usuario
-- que no existe, idUsuario queda NULL y "usuario" guarda lo que se escribió.
CREATE TABLE Auditoria(
    id INT IDENTITY(1,1) PRIMARY KEY,
    idUsuario INT NULL,
    usuario VARCHAR(20) NOT NULL,
    rol VARCHAR(20) NULL,              -- rol que tenía el usuario al momento de la acción
    accion VARCHAR(20) NOT NULL,
    entidad VARCHAR(30) NOT NULL,
    idEntidad INT NOT NULL,
    detalle VARCHAR(500) NOT NULL,
    ip VARCHAR(45) NULL,
    fecha DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FKAuditoriaUsuario FOREIGN KEY (idUsuario) REFERENCES Usuario(id),
    CONSTRAINT CKAuditoriaAccion CHECK (accion IN
        ('Crear', 'Actualizar', 'Eliminar', 'Ajustar', 'Reactivar', 'Ingreso', 'IngresoFallido', 'Bloqueo'))
);
GO

CREATE INDEX IXLoteProducto  ON Lote(idProducto);
CREATE INDEX IXLoteProveedor ON Lote(idProveedor);
CREATE INDEX IXAuditoriaFecha ON Auditoria(fecha DESC);
GO

-- =====================================================================
-- Datos iniciales (roles, proveedores, productos y sus lotes). Los usuarios
-- de prueba los crea la API al arrancar, con la contraseña cifrada con BCrypt.
-- =====================================================================
INSERT INTO Rol (nombre)
VALUES ('Administrador'), ('Operador');

INSERT INTO Proveedor (nombre, email, celular)
VALUES
('Proveedor A', 'contacto@proveedora.com', '0991112233'),
('Proveedor B', 'ventas@proveedorb.com', '0982223344'),
('Proveedor N', 'distribucion@proveedorn.com', '0973334455');

INSERT INTO Producto (codigo, nombre, descripcion)
VALUES
('MON001', 'Monitor 50 pulgadas 4K', 'Monitor Ultra HD 4K'),
('SON001', 'Equipo de Sonido 20000', 'Sistema de sonido estéreo'),
('PRD001', 'Producto N', 'Producto de catálogo general');

INSERT INTO Lote (idProducto, idProveedor, numeroLote)
VALUES
(1, 1, 'LOT-0001-01'), (1, 2, 'LOT-0001-02'), (1, 3, 'LOT-0001-03'),
(2, 1, 'LOT-0002-01'), (2, 2, 'LOT-0002-02'), (2, 3, 'LOT-0002-03'),
(3, 1, 'LOT-0003-01'), (3, 2, 'LOT-0003-02'), (3, 3, 'LOT-0003-03');

-- costo: lo que se pagó al proveedor; precio: precio de venta.
INSERT INTO Inventario (idLote, costo, precio, stock)
VALUES
(1, 180, 250, 15), (2, 210, 300, 10), (3, 140, 200, 25),
(4, 110, 150, 12), (5, 150, 200, 8),  (6,  70, 100, 30),
(7, 280, 400, 20), (8, 450, 600, 6),  (9, 260, 350, 18);
GO
