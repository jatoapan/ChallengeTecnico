USE InventarioProductos;
GO

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

INSERT INTO Inventario (idLote, precio, stock)
VALUES
(1, 250, 15), (2, 300, 10), (3, 200, 25),
(4, 150, 12), (5, 200, 8),  (6, 100, 30),
(7, 400, 20), (8, 600, 6),  (9, 350, 18);
GO