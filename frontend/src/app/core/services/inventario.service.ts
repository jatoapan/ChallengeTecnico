import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AjusteInventario {
  precio: number;
  stock: number;
}

@Injectable({ providedIn: 'root' })
export class InventarioService {
  private http = inject(HttpClient);

  ajustar(idLote: number, datos: AjusteInventario): Observable<unknown> {
    return this.http.put(`${environment.apiUrl}/inventario/${idLote}`, datos);
  }
}