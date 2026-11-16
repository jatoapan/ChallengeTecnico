import { Component, inject } from '@angular/core';
import { NotificacionService } from '../../core/services/notificacion.service';

@Component({
  selector: 'app-toasts',
  templateUrl: './toasts.html'
})
export class Toasts {
  servicio = inject(NotificacionService);
}