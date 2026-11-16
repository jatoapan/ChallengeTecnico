import { Component, input } from '@angular/core';

// Mensaje para listas vacías: explica qué pasa y ofrece la acción siguiente (proyectada con ng-content).
@Component({
  selector: 'app-estado-vacio',
  template: `
    <div class="text-center py-5 px-3">
      <i class="bi {{ icono() }} fs-1 text-body-secondary" aria-hidden="true"></i>
      <p class="fw-semibold mt-2 mb-1">{{ titulo() }}</p>
      @if (mensaje()) {
        <p class="text-body-secondary mb-3">{{ mensaje() }}</p>
      }
      <ng-content />
    </div>
  `
})
export class EstadoVacio {
  icono = input('bi-inbox');
  titulo = input.required<string>();
  mensaje = input<string>();
}
