import { Component, DOCUMENT, ElementRef, OnDestroy, afterNextRender, inject, input, output, viewChild } from '@angular/core';

let siguienteId = 0;

// Modal accesible: role="dialog", título asociado, cierre con Esc o clic en el fondo,
// foco al abrir y devolución del foco al elemento que lo abrió al cerrar.
// El contenido (modal-body y modal-footer) se proyecta con <ng-content />.
@Component({
  selector: 'app-modal',
  host: { '(document:keydown.escape)': 'cerrar.emit()' },
  template: `
    <div class="modal-backdrop fade show"></div>
    <div #dialogo class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true"
         [attr.aria-labelledby]="idTitulo" (click)="clicFondo($event)">
      <div class="modal-dialog modal-dialog-centered" [class.modal-lg]="grande()">
        <div class="modal-content">
          <div class="modal-header">
            <h2 class="modal-title h5" [id]="idTitulo">{{ titulo() }}</h2>
            <button type="button" class="btn-close" aria-label="Cerrar" (click)="cerrar.emit()"></button>
          </div>
          <ng-content />
        </div>
      </div>
    </div>
  `
})
export class Modal implements OnDestroy {
  titulo = input.required<string>();
  grande = input(false);
  cerrar = output<void>();

  readonly idTitulo = `modal-titulo-${++siguienteId}`;

  private dialogo = viewChild.required<ElementRef<HTMLElement>>('dialogo');
  private documento = inject(DOCUMENT);
  private focoAnterior = this.documento.activeElement as HTMLElement | null;

  constructor() {
    // Lleva el foco al diálogo para que el teclado y el lector de pantalla empiecen ahí.
    afterNextRender(() => this.dialogo().nativeElement.focus());
  }

  clicFondo(evento: MouseEvent): void {
    if (evento.target === evento.currentTarget) {
      this.cerrar.emit();
    }
  }

  ngOnDestroy(): void {
    this.focoAnterior?.focus();
  }
}
