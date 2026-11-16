export interface Oferta {
  idLote: number;
  numeroLote: string;
  idProveedor: number;
  proveedor: string;
  precio: number;
  stock: number;
}

export interface Producto {
  id: number;
  codigo: string;
  nombre: string;
  descripcion: string | null;
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
}