import { HttpErrorResponse } from '@angular/common/http';
import { ErrorHandler, Injectable, inject } from '@angular/core';
import { LogService } from '../services/log.service';
import { NotificacionService } from '../services/notificacion.service';

// Captura los errores de JavaScript que nadie manejó (en componentes, promesas, eventos).
// Los errores HTTP ya los registra y muestra errorInterceptor, así que aquí se ignoran
// para no duplicar el log ni el mensaje.
@Injectable()
export class ManejadorErrores implements ErrorHandler {
  private log = inject(LogService);
  private notificaciones = inject(NotificacionService);

  handleError(error: unknown): void {
    if (error instanceof HttpErrorResponse) return;

    const mensaje = error instanceof Error ? error.message : String(error);
    this.log.error({ mensaje: `Error no controlado: ${mensaje}`, detalle: error });
    this.notificaciones.error('Algo salió mal en la aplicación. Intente de nuevo.', 'Error inesperado');
  }
}
