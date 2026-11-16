import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Paginado } from '../../core/models/producto.model';
import {
  ACCIONES, AccionAuditoria, AuditoriaService, EstadisticasAuditoria, FiltroAuditoria, RegistroAuditoria
} from '../../core/services/auditoria.service';
import { BarraFiltros } from '../../shared/barra-filtros/barra-filtros';
import { Encabezado } from '../../shared/encabezado/encabezado';
import { EstadoVacio } from '../../shared/estado-vacio/estado-vacio';
import { Paginacion } from '../../shared/paginacion/paginacion';

// Auditoría de cambios y accesos (solo Administrador): estadísticas y filtros.
@Component({
  selector: 'app-historial',
  imports: [BarraFiltros, Encabezado, EstadoVacio, DatePipe, Paginacion, ReactiveFormsModule],
  templateUrl: './historial.html'
})
export class Historial implements OnInit {
  private auditoria = inject(AuditoriaService);

  readonly tamanoPagina = 10;
  readonly acciones = ACCIONES;
  readonly listaAcciones = Object.keys(ACCIONES) as AccionAuditoria[];
  readonly entidades = ['Producto', 'Lote', 'Proveedor', 'Usuario', 'Sesion'];

  resultado = signal<Paginado<RegistroAuditoria> | null>(null);
  estadisticas = signal<EstadisticasAuditoria | null>(null);
  cargando = signal(false);
  pagina = signal(1);
  hayFiltros = signal(false);

  // FormGroup en el <form>: Enter aplica los filtros sin recargar la página.
  filtros = new FormGroup({
    buscar: new FormControl('', { nonNullable: true }),
    accion: new FormControl<AccionAuditoria | ''>('', { nonNullable: true }),
    entidad: new FormControl('', { nonNullable: true }),
    desde: new FormControl('', { nonNullable: true }),
    hasta: new FormControl('', { nonNullable: true })
  });

  // Cuántos filtros del panel tienen valor (el texto de búsqueda no cuenta).
  contarFiltros(): number {
    const { accion, entidad, desde, hasta } = this.filtros.getRawValue();
    return [accion, entidad, desde, hasta].filter(valor => !!valor).length;
  }

  ngOnInit(): void {
    this.cargar();
  }

  private filtro(): FiltroAuditoria {
    const f = this.filtros.getRawValue();
    return { ...f, buscar: f.buscar.trim() };
  }

  cargar(): void {
    const filtro = this.filtro();
    this.hayFiltros.set(Object.values(filtro).some(v => !!v));
    this.cargando.set(true);

    this.auditoria.listar(this.pagina(), this.tamanoPagina, filtro).subscribe({
      next: respuesta => {
        this.resultado.set(respuesta);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
    this.auditoria.estadisticas(filtro).subscribe(e => this.estadisticas.set(e));
  }

  aplicarFiltros(): void {
    this.pagina.set(1);
    this.cargar();
  }

  limpiarFiltros(): void {
    this.filtros.reset();
    this.aplicarFiltros();
  }

  cambiarPagina(pagina: number): void {
    this.pagina.set(pagina);
    this.cargar();
  }

  nombreEntidad(entidad: string): string {
    return entidad === 'Sesion' ? 'Inicio de sesión' : entidad;
  }

  // Qué registro se tocó: "Producto #3". En los accesos no hay registro, solo la sesión.
  registro(r: RegistroAuditoria): string {
    return r.entidad === 'Sesion' ? 'Sesión' : `${r.entidad} #${r.idEntidad}`;
  }

  // Suma de varias acciones en las estadísticas (por ejemplo, ingresos fallidos + bloqueos).
  contar(...acciones: AccionAuditoria[]): number {
    const porAccion = this.estadisticas()?.porAccion ?? {};
    return acciones.reduce((total, a) => total + (porAccion[a] ?? 0), 0);
  }
}
