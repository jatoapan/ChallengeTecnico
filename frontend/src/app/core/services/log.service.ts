import { Injectable } from '@angular/core';

export interface EntradaLog {
  mensaje: string;
  metodo?: string;
  url?: string;
  status?: number;
  traceId?: string;
  detalle?: unknown;
}

// Punto único para registrar errores del front.
// Hoy escribe en la consola; mañana podría enviarlos a la API sin tocar a quien lo usa.
@Injectable({ providedIn: 'root' })
export class LogService {
  error(entrada: EntradaLog): void {
    console.error(this.formatear('ERROR', entrada), entrada.detalle ?? '');
  }

  advertencia(entrada: EntradaLog): void {
    console.warn(this.formatear('ADVERTENCIA', entrada), entrada.detalle ?? '');
  }

  private formatear(nivel: string, entrada: EntradaLog): string {
    const fecha = new Date().toISOString();
    const peticion = entrada.metodo ? ` ${entrada.metodo} ${entrada.url}` : '';
    const status = entrada.status !== undefined ? ` (${entrada.status})` : '';
    const traza = entrada.traceId ? ` [traceId ${entrada.traceId}]` : '';
    return `[${fecha}] [${nivel}]${peticion}${status} ${entrada.mensaje}${traza}`;
  }
}
