import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Paginado } from '../models/producto.model';

// Mismas acciones que el CHECK de la tabla Auditoria.
export type AccionAuditoria =
  'Crear' | 'Actualizar' | 'Eliminar' | 'Ajustar' | 'Reactivar' | 'Ingreso' | 'IngresoFallido' | 'Bloqueo';

export interface RegistroAuditoria {
  id: number;
  usuario: string;
  rol: string | null;      // rol que tenía al momento de la acción
  accion: AccionAuditoria;
  entidad: string;
  idEntidad: number;
  detalle: string;
  ip: string | null;
  fecha: string;
}

export interface FiltroAuditoria {
  buscar?: string;
  accion?: AccionAuditoria | '';
  entidad?: string;
  desde?: string;   // yyyy-MM-dd
  hasta?: string;
}

export interface EstadisticasAuditoria {
  total: number;
  porAccion: Partial<Record<AccionAuditoria, number>>;
}

// Texto y color del badge de cada acción, en un solo lugar para todas las pantallas.
export const ACCIONES: Record<AccionAuditoria, { texto: string; color: string }> = {
  Crear: { texto: 'Crear', color: 'text-bg-success' },
  Actualizar: { texto: 'Actualizar', color: 'text-bg-primary' },
  Eliminar: { texto: 'Desactivar', color: 'text-bg-danger' },
  Ajustar: { texto: 'Ajustar', color: 'text-bg-warning' },
  Reactivar: { texto: 'Reactivar', color: 'text-bg-success' },
  Ingreso: { texto: 'Ingreso', color: 'text-bg-secondary' },
  IngresoFallido: { texto: 'Ingreso fallido', color: 'text-bg-danger' },
  Bloqueo: { texto: 'Bloqueo', color: 'text-bg-dark' }
};

@Injectable({ providedIn: 'root' })
export class AuditoriaService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/auditoria`;

  listar(pagina: number, tamanoPagina: number, filtro: FiltroAuditoria = {}): Observable<Paginado<RegistroAuditoria>> {
    const params = this.parametros(filtro).set('pagina', pagina).set('tamanoPagina', tamanoPagina);
    return this.http.get<Paginado<RegistroAuditoria>>(this.url, { params });
  }

  estadisticas(filtro: FiltroAuditoria = {}): Observable<EstadisticasAuditoria> {
    return this.http.get<EstadisticasAuditoria>(`${this.url}/estadisticas`, { params: this.parametros(filtro) });
  }

  // Solo se envían los filtros con valor.
  private parametros(filtro: FiltroAuditoria): HttpParams {
    let params = new HttpParams();
    for (const [clave, valor] of Object.entries(filtro)) {
      if (valor) params = params.set(clave, valor);
    }
    return params;
  }
}
