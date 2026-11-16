import { Injectable, signal } from '@angular/core';

export type TipoNotificacion = 'exito' | 'error';

export interface Notificacion {
  id: number;
  tipo: TipoNotificacion;
  mensaje: string;
  titulo?: string;
}

@Injectable({ providedIn: 'root' })
export class NotificacionService {
  private contador = 0;
  readonly notificaciones = signal<Notificacion[]>([]);

  exito(mensaje: string, titulo?: string): void {
    this.mostrar('exito', mensaje, titulo);
  }

  error(mensaje: string, titulo?: string): void {
    this.mostrar('error', mensaje, titulo);
  }

  cerrar(id: number): void {
    this.notificaciones.update(lista => lista.filter(n => n.id !== id));
  }

  private mostrar(tipo: TipoNotificacion, mensaje: string, titulo?: string): void {
    const id = ++this.contador;
    this.notificaciones.update(lista => [...lista, { id, tipo, mensaje, titulo }]);
    setTimeout(() => this.cerrar(id), 5000);
  }
}
