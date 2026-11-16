import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FiltroEstado, MatrizPrecios, Paginado, Producto, ProductoActualizar, ProductoGuardar } from '../models/producto.model';

@Injectable({ providedIn: 'root' })
export class ProductoService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/productos`;

  listar(buscar: string, pagina: number, tamanoPagina: number, estado: FiltroEstado = 'Activos'): Observable<Paginado<Producto>> {
    let params = new HttpParams().set('pagina', pagina).set('tamanoPagina', tamanoPagina).set('estado', estado);

    if (buscar.trim()) {
      params = params.set('buscar', buscar.trim());
    }

    return this.http.get<Paginado<Producto>>(this.url, { params });
  }

  matrizPrecios(buscar: string, pagina: number, tamanoPagina: number): Observable<MatrizPrecios> {
    let params = new HttpParams().set('pagina', pagina).set('tamanoPagina', tamanoPagina);

    if (buscar.trim()) {
      params = params.set('buscar', buscar.trim());
    }

    return this.http.get<MatrizPrecios>(`${this.url}/matriz-precios`, { params });
  }

  obtener(id: number): Observable<Producto> {
    return this.http.get<Producto>(`${this.url}/${id}`);
  }

  crear(datos: ProductoGuardar): Observable<Producto> {
    return this.http.post<Producto>(this.url, datos);
  }

  // Solo datos del catálogo; los lotes se gestionan con LoteService e InventarioService.
  actualizar(id: number, datos: ProductoActualizar): Observable<Producto> {
    return this.http.put<Producto>(`${this.url}/${id}`, datos);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}