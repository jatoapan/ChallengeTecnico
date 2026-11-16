import { Component, input } from '@angular/core';

// Encabezado común de cada pantalla: título, descripción opcional y acciones a la derecha
// (las acciones se proyectan con <ng-content />).
@Component({
  selector: 'app-encabezado',
  template: `
    <div class="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
      <div>
        <h1 class="h3 mb-1">{{ titulo() }}</h1>
        @if (descripcion()) {
          <p class="text-body-secondary mb-0">{{ descripcion() }}</p>
        }
      </div>
      <div class="acciones d-flex flex-wrap gap-2">
        <ng-content />
      </div>
    </div>
  `,
  // Si la pantalla no tiene acciones, el contenedor vacío no debe ocupar espacio.
  styles: `.acciones:empty { display: none !important; }`
})
export class Encabezado {
  titulo = input.required<string>();
  descripcion = input<string>();
}
