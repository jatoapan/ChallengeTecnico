import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginRespuesta } from '../models/auth.model';

const CLAVE_SESION = 'sesion';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private sesion = signal<LoginRespuesta | null>(this.leerSesion());

  usuario = computed(() => this.sesion()?.usuario ?? null);
  rol = computed(() => this.sesion()?.rol ?? null);

  get token(): string | null {
    return this.estaAutenticado() ? this.sesion()!.token : null;
  }

  estaAutenticado(): boolean {
    const sesion = this.sesion();
    return !!sesion && new Date(sesion.expira) > new Date();
  }

  tieneRol(rol: string): boolean {
    return this.rol() === rol;
  }

  login(datos: LoginRequest): Observable<LoginRespuesta> {
    return this.http.post<LoginRespuesta>(`${environment.apiUrl}/auth/login`, datos).pipe(
      tap(respuesta => {
        localStorage.setItem(CLAVE_SESION, JSON.stringify(respuesta));
        this.sesion.set(respuesta);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(CLAVE_SESION);
    this.sesion.set(null);
  }

  private leerSesion(): LoginRespuesta | null {
    try {
      const texto = localStorage.getItem(CLAVE_SESION);
      return texto ? JSON.parse(texto) : null;
    } catch {
      return null;
    }
  }
}