import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FiltroEstado, Proveedor, ProveedorGuardar } from '../models/producto.model';

@Injectable({ providedIn: 'root' })
export class ProveedorService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/proveedores`;

  // Por defecto solo activos: es lo que necesitan los formularios de lotes.
  listar(estado: FiltroEstado = 'Activos', buscar = ''): Observable<Proveedor[]> {
    let params = new HttpParams().set('estado', estado);
    if (buscar.trim()) params = params.set('buscar', buscar.trim());
    return this.http.get<Proveedor[]>(this.url, { params });
  }

  crear(datos: ProveedorGuardar): Observable<Proveedor> {
    return this.http.post<Proveedor>(this.url, datos);
  }

  actualizar(id: number, datos: ProveedorGuardar): Observable<Proveedor> {
    return this.http.put<Proveedor>(`${this.url}/${id}`, datos);
  }

  desactivar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  reactivar(id: number): Observable<Proveedor> {
    return this.http.post<Proveedor>(`${this.url}/${id}/reactivar`, {});
  }
}
