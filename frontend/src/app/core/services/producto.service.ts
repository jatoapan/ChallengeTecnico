import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Paginado, Producto, ProductoGuardar } from '../models/producto.model';

@Injectable({ providedIn: 'root' })
export class ProductoService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/productos`;

  listar(buscar: string, pagina: number, tamanoPagina: number): Observable<Paginado<Producto>> {
    let params = new HttpParams().set('pagina', pagina).set('tamanoPagina', tamanoPagina);

    if (buscar.trim()) {
      params = params.set('buscar', buscar.trim());
    }

    return this.http.get<Paginado<Producto>>(this.url, { params });
  }

  obtener(id: number): Observable<Producto> {
    return this.http.get<Producto>(`${this.url}/${id}`);
  }

  crear(datos: ProductoGuardar): Observable<Producto> {
    return this.http.post<Producto>(this.url, datos);
  }

  actualizar(id: number, datos: ProductoGuardar): Observable<Producto> {
    return this.http.put<Producto>(`${this.url}/${id}`, datos);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}