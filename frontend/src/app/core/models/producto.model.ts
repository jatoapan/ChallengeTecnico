export interface Oferta {
  idLote: number;
  numeroLote: string;
  idProveedor: number;
  proveedor: string;
  costo: number;
  precio: number;
  stock: number;
}

// Filtro por estado (borrado lógico), igual que FiltroEstado en la API.
export type FiltroEstado = 'Activos' | 'Inactivos' | 'Todos';

export interface Producto {
  id: number;
  codigo: string;
  nombre: string;
  descripcion: string | null;
  estado: boolean;
  ofertas: Oferta[];
}

export interface Paginado<T> {
  items: T[];
  total: number;
  pagina: number;
  tamanoPagina: number;
}

export interface OfertaGuardar {
  idLote?: number | null;
  idProveedor: number;
  numeroLote: string;
  costo: number;
  precio: number;
  stock: number;
}

export interface ProductoGuardar {
  codigo: string;
  nombre: string;
  descripcion?: string | null;
  ofertas: OfertaGuardar[];
}

export interface Proveedor {
  id: number;
  nombre: string;
  email?: string;
  celular?: string | null;
  lotesActivos?: number;
  estado?: boolean;
}

export interface ProveedorGuardar {
  nombre: string;
  email: string;
  celular?: string | null;
}

// Precio de un proveedor para un producto: el menor entre sus lotes con stock, y su stock total.
export interface PrecioProveedor {
  precio: number;
  stock: number;
}

// Fila de la matriz: oferta por id de proveedor (sin clave si el proveedor no tiene oferta).
export interface FilaMatrizPrecios {
  id: number;
  codigo: string;
  nombre: string;
  precios: Record<number, PrecioProveedor>;
}

export interface MatrizPrecios extends Paginado<FilaMatrizPrecios> {
  proveedores: Proveedor[];
}
// Datos del catálogo de un producto (sin lotes).
export interface ProductoActualizar {
  codigo: string;
  nombre: string;
  descripcion?: string | null;
}

// Una fila de la lista de inventario: un lote de un producto con su proveedor, precio y stock.
export interface LoteListado {
  idLote: number;
  numeroLote: string;
  idProducto: number;
  codigoProducto: string;
  producto: string;
  idProveedor: number;
  proveedor: string;
  costo: number;
  precio: number;
  stock: number;
  fechaActualizacion: string;
}

export interface LoteGuardar {
  idProveedor: number;
  numeroLote: string;
  costo: number;
  precio: number;
  stock: number;
}
