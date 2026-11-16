import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { LogService } from '../services/log.service';
import { NotificacionService } from '../services/notificacion.service';

interface MensajeError {
  titulo: string;
  mensaje: string;
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const notificaciones = inject(NotificacionService);
  const log = inject(LogService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const esLogin = req.url.includes('/auth/login');
      const { titulo, mensaje } = obtenerMensaje(error, esLogin);
      const traceId = obtenerTraceId(error);

      // Errores del cliente (4xx) son advertencias; sin conexión o fallos del servidor son errores.
      // El traceId permite encontrar la misma petición en los logs de Serilog del back.
      const entrada = {
        mensaje,
        metodo: req.method,
        url: req.urlWithParams,
        status: error.status,
        traceId,
        detalle: error.error
      };
      if (error.status === 0 || error.status >= 500) {
        log.error(entrada);
      } else {
        log.advertencia(entrada);
      }

      if (error.status === 401 && !esLogin) {
        auth.logout();
        router.navigate(['/login']);
      }

      // En errores del servidor se muestra un código corto que el usuario puede reportar a soporte.
      const mensajeFinal = error.status >= 500 && traceId
        ? `${mensaje} Código de referencia: ${traceId.slice(0, 8)}`
        : mensaje;

      notificaciones.error(mensajeFinal, titulo);
      return throwError(() => error);
    })
  );
};

function obtenerMensaje(error: HttpErrorResponse, esLogin: boolean): MensajeError {
  const cuerpo = error.error;
  // El back responde con ProblemDetails: "errors" en validaciones y "detail" en el resto.
  const validaciones = cuerpo?.errors ? Object.values(cuerpo.errors).flat().join(' ') : null;
  const detalle: string | undefined = validaciones ?? cuerpo?.detail;

  switch (error.status) {
    case 0:
      return { titulo: 'Sin conexión', mensaje: 'No se pudo conectar con el servidor. Verifique que la API esté en ejecución.' };
    case 400:
      return { titulo: 'Solicitud inválida', mensaje: detalle ?? 'Revise los datos enviados.' };
    case 401:
      return esLogin
        ? { titulo: 'Credenciales inválidas', mensaje: detalle ?? 'El usuario o la contraseña son incorrectos.' }
        : { titulo: 'Sesión expirada', mensaje: 'Su sesión expiró. Inicie sesión nuevamente.' };
    case 403:
      return { titulo: 'Acceso denegado', mensaje: 'No tiene permisos para realizar esta acción.' };
    case 404:
      return { titulo: 'No encontrado', mensaje: detalle ?? 'El recurso solicitado no existe.' };
    case 429:
      return { titulo: 'Demasiados intentos', mensaje: detalle ?? 'Espere unos minutos e intente nuevamente.' };
    case 500:
      return { titulo: 'Error del servidor', mensaje: detalle ?? 'Ocurrió un error interno. Intente más tarde.' };
    default:
      return { titulo: `Error (${error.status})`, mensaje: detalle ?? 'Ocurrió un error inesperado.' };
  }
}

// El back envía el traceId en el ProblemDetails. Las validaciones automáticas de ASP.NET lo envían
// con formato "00-<traceId>-<spanId>-00"; en ese caso se toma solo la parte del traceId.
function obtenerTraceId(error: HttpErrorResponse): string | undefined {
  const valor: unknown = error.error?.traceId;
  if (typeof valor !== 'string') return undefined;
  return valor.split('-')[1] ?? valor;
}
