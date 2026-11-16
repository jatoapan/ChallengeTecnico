import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoteGuardar, LoteListado, Paginado } from '../models/producto.model';

// Filtros de la lista de lotes. Los que vienen vacíos (null, 0 o texto vacío) no se envían.
export interface FiltroLotes {
  buscar?: string;
  idProveedor?: number | null;
  stockMin?: number | null;
  stockMax?: number | null;
  precioMin?: number | null;
  precioMax?: number | null;
}

// Operaciones por lote: listar (una fila por lote), agregar un lote a un producto y desactivarlo.
@Injectable({ providedIn: 'root' })
export class LoteService {
  private http = inject(HttpClient);

  listar(filtro: FiltroLotes, pagina: number, tamanoPagina: number): Observable<Paginado<LoteListado>> {
    let params = new HttpParams().set('pagina', pagina).set('tamanoPagina', tamanoPagina);

    for (const [clave, valor] of Object.entries(filtro)) {
      const dato = typeof valor === 'string' ? valor.trim() : valor;
      // El 0 solo significa "sin filtro" en el proveedor; en stock y precio es un valor válido (stock máximo 0 = sin stock).
      const vacio = dato === null || dato === undefined || dato === '' || (clave === 'idProveedor' && dato === 0);
      if (!vacio) {
        params = params.set(clave, dato);
      }
    }

    return this.http.get<Paginado<LoteListado>>(`${environment.apiUrl}/lotes`, { params });
  }

  agregar(idProducto: number, datos: LoteGuardar): Observable<LoteListado> {
    return this.http.post<LoteListado>(`${environment.apiUrl}/productos/${idProducto}/lotes`, datos);
  }

  desactivar(idLote: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/lotes/${idLote}`);
  }
}
