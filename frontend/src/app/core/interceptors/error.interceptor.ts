import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificacionService } from '../services/notificacion.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const notificaciones = inject(NotificacionService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const esLogin = req.url.includes('/auth/login');

      if (error.status === 401 && !esLogin) {
        auth.logout();
        router.navigate(['/login']);
        notificaciones.error('Su sesión expiró. Inicie sesión nuevamente.');
      } else {
        notificaciones.error(obtenerMensaje(error));
      }

      return throwError(() => error);
    })
  );
};

function obtenerMensaje(error: HttpErrorResponse): string {
  if (error.status === 0) return 'No se pudo conectar con el servidor.';
  if (error.status === 403) return 'No tiene permisos para realizar esta acción.';

  const cuerpo = error.error;

  if (cuerpo?.errors) {
    return Object.values(cuerpo.errors).flat().join(' ');
  }

  return cuerpo?.detail ?? 'Ocurrió un error inesperado.';
}