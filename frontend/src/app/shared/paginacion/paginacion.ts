import { Component, computed, input, output } from '@angular/core';

// Paginación reutilizable: recibe la página actual y el total, y avisa cuando se cambia de página.
@Component({
  selector: 'app-paginacion',
  // Sin resultados no hay nada que paginar: el componente no se muestra.
  template: `
    @if (total() > 0) {
    <nav class="d-flex justify-content-between align-items-center" aria-label="Paginación">
      <span class="text-muted small" aria-live="polite">Página {{ pagina() }} de {{ totalPaginas() }}</span>
      <div class="btn-group">
        <button type="button" class="btn btn-outline-secondary btn-sm" [disabled]="pagina() <= 1"
                (click)="cambiar.emit(pagina() - 1)">Anterior</button>
        <button type="button" class="btn btn-outline-secondary btn-sm" [disabled]="pagina() >= totalPaginas()"
                (click)="cambiar.emit(pagina() + 1)">Siguiente</button>
      </div>
    </nav>
    }
  `
})
export class Paginacion {
  pagina = input.required<number>();
  total = input.required<number>();
  tamanoPagina = input.required<number>();

  cambiar = output<number>();

  totalPaginas = computed(() => Math.max(1, Math.ceil(this.total() / this.tamanoPagina())));
}
