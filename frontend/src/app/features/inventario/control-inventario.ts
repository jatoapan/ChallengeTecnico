import { CurrencyPipe, DatePipe, DecimalPipe, NgTemplateOutlet } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { LoteListado, Paginado, Proveedor } from '../../core/models/producto.model';
import { InventarioService } from '../../core/services/inventario.service';
import { LoteService } from '../../core/services/lote.service';
import { NotificacionService } from '../../core/services/notificacion.service';
import { ProveedorService } from '../../core/services/proveedor.service';
import { BarraFiltros } from '../../shared/barra-filtros/barra-filtros';
import { CamposFiltroLotes, contarFiltrosLotes, crearFiltrosLotes } from '../../shared/barra-filtros/filtros-lotes';
import { Encabezado } from '../../shared/encabezado/encabezado';
import { EstadoVacio } from '../../shared/estado-vacio/estado-vacio';
import { Modal } from '../../shared/modal/modal';
import { Paginacion } from '../../shared/paginacion/paginacion';

// Control de inventario: una fila por lote y una sola acción, ajustar costo, precio y stock.
// Crear productos, agregar o desactivar lotes se hace en Gestión de productos.
@Component({
  selector: 'app-control-inventario',
  imports: [BarraFiltros, CamposFiltroLotes, Encabezado, EstadoVacio, ReactiveFormsModule, CurrencyPipe, DatePipe, DecimalPipe, NgTemplateOutlet, Modal, Paginacion],
  templateUrl: './control-inventario.html'
})
export class ControlInventario implements OnInit {
  private lotes = inject(LoteService);
  private inventario = inject(InventarioService);
  private notificaciones = inject(NotificacionService);
  private fb = inject(FormBuilder);
  private ruta = inject(ActivatedRoute);
  private proveedoresServicio = inject(ProveedorService);

  readonly tamanoPagina = 10;
  readonly stockBajo = 10;
  // Texto de búsqueda y filtros del panel (proveedor, rango de stock y rango de precio).
  filtros = crearFiltrosLotes();
  proveedores = signal<Proveedor[]>([]);
  pagina = signal(1);
  // Si la última consulta llevó búsqueda o filtros (no lo que se está escribiendo): cambia el mensaje sin resultados.
  hayCriterios = signal(false);
  resultado = signal<Paginado<LoteListado> | null>(null);
  cargando = signal(false);

  // Lote sobre el que se abrió el ajuste (null = modal cerrado).
  ajuste = signal<LoteListado | null>(null);

  formularioAjuste = this.fb.nonNullable.group({
    costo: [0, [Validators.required, Validators.min(0)]],
    precio: [0, [Validators.required, Validators.min(0.01)]],
    stock: [0, [Validators.required, Validators.min(0)]]
  });

  ngOnInit(): void {
    // Permite llegar ya filtrado, por ejemplo desde el Panel principal o el catálogo (?buscar=MON001).
    const buscar = this.ruta.snapshot.queryParamMap.get('buscar');
    if (buscar) {
      this.filtros.controls.buscar.setValue(buscar);
    }
    this.proveedoresServicio.listar().subscribe(lista => this.proveedores.set(lista));
    this.cargar();
  }

  contarFiltros(): number {
    return contarFiltrosLotes(this.filtros);
  }

  cargar(): void {
    this.cargando.set(true);
    this.hayCriterios.set(!!this.filtros.controls.buscar.value.trim() || this.contarFiltros() > 0);

    this.lotes.listar(this.filtros.getRawValue(), this.pagina(), this.tamanoPagina).subscribe({
      next: respuesta => {
        this.resultado.set(respuesta);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  buscarLotes(): void {
    this.pagina.set(1);
    this.cargar();
  }

  limpiarBusqueda(): void {
    this.filtros.reset();
    this.buscarLotes();
  }

  cambiarPagina(pagina: number): void {
    this.pagina.set(pagina);
    this.cargar();
  }

  invalido(control: { invalid: boolean; touched: boolean }): boolean {
    return control.invalid && control.touched;
  }

  // Margen de venta: cuánto se gana por unidad y qué porcentaje del precio es.
  // Sin costo registrado (0) no se puede calcular.
  margen(costo: number, precio: number): { monto: number; porcentaje: number } | null {
    if (!costo || !precio) return null;
    const monto = precio - costo;
    return { monto, porcentaje: (monto / precio) * 100 };
  }

  // Botones + y − del ajuste: suman o restan sin bajar del mínimo permitido.
  cambiar(control: FormControl<number>, delta: number, minimo: number): void {
    const valor = Math.round(((Number(control.value) || 0) + delta) * 100) / 100;
    control.setValue(Math.max(minimo, valor));
    control.markAsTouched();
  }

  abrirAjuste(lote: LoteListado): void {
    this.formularioAjuste.reset({ costo: lote.costo, precio: lote.precio, stock: lote.stock });
    this.ajuste.set(lote);
  }

  guardarAjuste(): void {
    const lote = this.ajuste();
    if (!lote) return;

    if (this.formularioAjuste.invalid) {
      this.formularioAjuste.markAllAsTouched();
      return;
    }

    this.inventario.ajustar(lote.idLote, this.formularioAjuste.getRawValue()).subscribe({
      next: () => {
        this.notificaciones.exito(`Lote ${lote.numeroLote} actualizado.`);
        this.ajuste.set(null);
        this.cargar();
      }
    });
  }
}
