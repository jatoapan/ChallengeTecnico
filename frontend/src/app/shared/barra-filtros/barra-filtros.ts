import { Component, input, output, signal } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';

let siguienteId = 0;

// Barra de búsqueda con un botón "Filtros" que abre un panel con los filtros de la pantalla.
// El grupo del formulario debe tener un control "buscar"; los campos del panel se proyectan con
// <ng-content /> y se enlazan con [formControl] (no con formControlName, porque no están dentro de este <form>).
// El <form> lleva [formGroup]: así Enter aplica la búsqueda sin que el navegador recargue la página.
@Component({
  selector: 'app-barra-filtros',
  imports: [ReactiveFormsModule],
  template: `
    <form class="mb-3" role="search" [formGroup]="grupo()" (ngSubmit)="buscar.emit()">
      <div class="d-flex flex-wrap gap-2">
        <div class="flex-grow-1">
          <label [for]="idBuscar()" class="visually-hidden">{{ placeholder() }}</label>
          <input [id]="idBuscar()" type="search" class="form-control" formControlName="buscar" [placeholder]="placeholder()">
        </div>

        <button type="button" class="btn" [class.btn-outline-secondary]="!activos()" [class.btn-outline-primary]="activos() > 0"
                [attr.aria-expanded]="abierto()" [attr.aria-controls]="idPanel" (click)="abierto.set(!abierto())">
          <i class="bi bi-funnel me-1" aria-hidden="true"></i>Filtros
          @if (activos() > 0) {
            <span class="badge text-bg-primary ms-1">{{ activos() }}<span class="visually-hidden"> activos</span></span>
          }
        </button>

        <button type="submit" class="btn btn-primary"><i class="bi bi-search me-1" aria-hidden="true"></i>Buscar</button>

        @if (activos() > 0 || grupo().value.buscar) {
          <button type="button" class="btn btn-outline-secondary" (click)="limpiar.emit()">Limpiar</button>
        }
      </div>

      @if (abierto()) {
        <div [id]="idPanel" class="tabla-panel p-3 mt-2">
          <div class="row g-3">
            <ng-content />
          </div>
        </div>
      }
    </form>
  `
})
export class BarraFiltros {
  grupo = input.required<FormGroup>();
  placeholder = input.required<string>();
  idBuscar = input.required<string>();
  // Cuántos filtros del panel hay aplicados (sin contar el texto de búsqueda).
  activos = input(0);

  buscar = output<void>();
  limpiar = output<void>();

  abierto = signal(false);
  readonly idPanel = `panel-filtros-${++siguienteId}`;
}
