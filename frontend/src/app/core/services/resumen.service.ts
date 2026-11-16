import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LotePorReponer {
  idLote: number;
  numeroLote: string;
  idProducto: number;
  codigoProducto: string;
  producto: string;
  proveedor: string;
  precio: number;
  stock: number;
  fechaActualizacion: string;
}

export interface Resumen {
  totalProductos: number;
  totalProveedores: number;
  stockTotal: number;
  lotesStockBajo: number;
  umbral: number;
  porReponer: LotePorReponer[];
  ultimosLotes: LotePorReponer[];
}

@Injectable({ providedIn: 'root' })
export class ResumenService {
  private http = inject(HttpClient);

  obtener(maximo = 8): Observable<Resumen> {
    const params = new HttpParams().set('maximo', maximo);
    return this.http.get<Resumen>(`${environment.apiUrl}/resumen`, { params });
  }
}
