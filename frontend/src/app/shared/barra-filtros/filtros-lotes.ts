import { Component, input } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Proveedor } from '../../core/models/producto.model';

// Filtros de la lista de lotes: los comparten Gestión de productos y Control de inventario.
// Los campos vacíos valen null y no se envían a la API.
export function crearFiltrosLotes() {
  return new FormGroup({
    buscar: new FormControl('', { nonNullable: true }),
    idProveedor: new FormControl<number | null>(null),
    stockMin: new FormControl<number | null>(null),
    stockMax: new FormControl<number | null>(null),
    precioMin: new FormControl<number | null>(null),
    precioMax: new FormControl<number | null>(null)
  });
}

export type FiltrosLotes = ReturnType<typeof crearFiltrosLotes>;

// Cuántos filtros del panel tienen valor (el texto de búsqueda no cuenta).
export function contarFiltrosLotes(filtros: FiltrosLotes): number {
  const { buscar, ...panel } = filtros.getRawValue();
  return Object.values(panel).filter(valor => valor !== null && valor !== undefined).length;
}

// Campos del panel de filtros. El componente ocupa todo el ancho de la fila de <app-barra-filtros>
// (col-12) y trae su propia fila interna para que las columnas conserven la separación.
@Component({
  selector: 'app-campos-filtro-lotes',
  imports: [ReactiveFormsModule],
  host: { class: 'col-12' },
  template: `
    <div class="row g-3">
    <div class="col-12 col-md-6 col-lg-4">
      <label for="filtro-proveedor" class="form-label">Proveedor</label>
      <select id="filtro-proveedor" class="form-select" [formControl]="grupo().controls.idProveedor">
        <option [ngValue]="null">Todos los proveedores</option>
        @for (p of proveedores(); track p.id) {
          <option [ngValue]="p.id">{{ p.nombre }}</option>
        }
      </select>
    </div>

    <div class="col-6 col-lg-2">
      <label for="filtro-stock-min" class="form-label">Stock mínimo</label>
      <input id="filtro-stock-min" type="number" min="0" step="1" class="form-control"
             [formControl]="grupo().controls.stockMin" placeholder="0">
    </div>
    <div class="col-6 col-lg-2">
      <label for="filtro-stock-max" class="form-label">Stock máximo</label>
      <input id="filtro-stock-max" type="number" min="0" step="1" class="form-control"
             [formControl]="grupo().controls.stockMax" placeholder="Sin límite">
    </div>

    <div class="col-6 col-lg-2">
      <label for="filtro-precio-min" class="form-label">Precio mínimo</label>
      <input id="filtro-precio-min" type="number" min="0" step="0.01" class="form-control"
             [formControl]="grupo().controls.precioMin" placeholder="0.00">
    </div>
    <div class="col-6 col-lg-2">
      <label for="filtro-precio-max" class="form-label">Precio máximo</label>
      <input id="filtro-precio-max" type="number" min="0" step="0.01" class="form-control"
             [formControl]="grupo().controls.precioMax" placeholder="Sin límite">
    </div>
    </div>
  `
})
export class CamposFiltroLotes {
  grupo = input.required<FiltrosLotes>();
  proveedores = input.required<Proveedor[]>();
}
