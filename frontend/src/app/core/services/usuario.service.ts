import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FiltroEstado } from '../models/producto.model';

export interface Usuario {
  id: number;
  nombres: string;
  apellidos: string;
  usuario: string;
  email: string;
  idRol: number;
  rol: string;
  estado: boolean;
  fechaCreacion: string;
}

export interface Rol {
  id: number;
  nombre: string;
}

// Al crear, la contraseña es obligatoria; al editar, solo se envía si se quiere restablecer.
export interface UsuarioGuardar {
  nombres: string;
  apellidos: string;
  usuario: string;
  email: string;
  idRol: number;
  password?: string | null;
}

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/usuarios`;

  listar(buscar: string, estado: FiltroEstado = 'Activos'): Observable<Usuario[]> {
    let params = new HttpParams().set('estado', estado);
    if (buscar.trim()) params = params.set('buscar', buscar.trim());
    return this.http.get<Usuario[]>(this.url, { params });
  }

  roles(): Observable<Rol[]> {
    return this.http.get<Rol[]>(`${this.url}/roles`);
  }

  crear(datos: UsuarioGuardar): Observable<Usuario> {
    return this.http.post<Usuario>(this.url, datos);
  }

  actualizar(id: number, datos: UsuarioGuardar): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.url}/${id}`, datos);
  }

  desactivar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  reactivar(id: number): Observable<Usuario> {
    return this.http.post<Usuario>(`${this.url}/${id}/reactivar`, {});
  }
}
