import { CurrencyPipe, NgTemplateOutlet } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LoteListado, Paginado, Proveedor } from '../../core/models/producto.model';
import { AuthService } from '../../core/services/auth.service';
import { LoteService } from '../../core/services/lote.service';
import { NotificacionService } from '../../core/services/notificacion.service';
import { ProductoService } from '../../core/services/producto.service';
import { ProveedorService } from '../../core/services/proveedor.service';
import { BarraFiltros } from '../../shared/barra-filtros/barra-filtros';
import { CamposFiltroLotes, contarFiltrosLotes, crearFiltrosLotes } from '../../shared/barra-filtros/filtros-lotes';
import { Encabezado } from '../../shared/encabezado/encabezado';
import { EstadoVacio } from '../../shared/estado-vacio/estado-vacio';
import { Modal } from '../../shared/modal/modal';
import { Paginacion } from '../../shared/paginacion/paginacion';

// Gestión de productos: una fila por lote (producto, proveedor, costo, precio y stock).
// Administrador: crear y editar producto (en modales; al editar también se desactiva el producto), agregar lote y desactivar lote.
// Operador: solo consulta. Ajustar stock y precio se hace en Control de inventario.
@Component({
  selector: 'app-catalogo-productos',
  imports: [BarraFiltros, CamposFiltroLotes, Encabezado, EstadoVacio, Modal, Paginacion, ReactiveFormsModule, CurrencyPipe, NgTemplateOutlet],
  templateUrl: './catalogo-productos.html'
})
export class CatalogoProductos implements OnInit {
  private lotesServicio = inject(LoteService);
  private proveedoresServicio = inject(ProveedorService);
  private productos = inject(ProductoService);
  private ruta = inject(ActivatedRoute);
  private router = inject(Router);
  private notificaciones = inject(NotificacionService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  readonly tamanoPagina = 10;
  readonly stockBajo = 10;
  readonly esAdmin = this.auth.tieneRol('Administrador');

  // Texto de búsqueda y filtros del panel (proveedor, rango de stock y rango de precio).
  filtros = crearFiltrosLotes();
  pagina = signal(1);
  // Si la última consulta llevó búsqueda o filtros: cambia el mensaje cuando no hay resultados.
  hayCriterios = signal(false);
  resultado = signal<Paginado<LoteListado> | null>(null);
  cargando = signal(false);
  proveedores = signal<Proveedor[]>([]);

  // Fila sobre la que se abrió cada modal (null = cerrado).
  nuevoLote = signal<LoteListado | null>(null);
  porDesactivar = signal<LoteListado | null>(null);

  // Modales de producto: nuevo (producto + primer lote) y editar (solo los datos del catálogo).
  modalNuevo = signal(false);
  editando = signal<number | null>(null);
  confirmandoBaja = signal(false);
  guardando = signal(false);

  formularioNuevo = this.fb.nonNullable.group({
    codigo: ['', [Validators.required, Validators.maxLength(10)]],
    nombre: ['', [Validators.required, Validators.maxLength(60)]],
    descripcion: ['', [Validators.maxLength(255)]],
    idProveedor: [0, [Validators.min(1)]],
    numeroLote: ['', [Validators.required, Validators.maxLength(20)]],
    costo: [0, [Validators.required, Validators.min(0)]],
    precio: [0, [Validators.required, Validators.min(0.01)]],
    stock: [0, [Validators.required, Validators.min(0)]]
  });

  // El código no se cambia una vez creado el producto.
  formularioEditar = this.fb.nonNullable.group({
    codigo: [{ value: '', disabled: true }],
    nombre: ['', [Validators.required, Validators.maxLength(60)]],
    descripcion: ['', [Validators.maxLength(255)]]
  });

  formularioLote = this.fb.nonNullable.group({
    idProveedor: [0, [Validators.min(1)]],
    numeroLote: ['', [Validators.required, Validators.maxLength(20)]],
    costo: [0, [Validators.required, Validators.min(0)]],
    precio: [0, [Validators.required, Validators.min(0.01)]],
    stock: [0, [Validators.required, Validators.min(0)]]
  });

  ngOnInit(): void {
    this.cargarProveedores();
    this.cargar();

    // Permite llegar con el modal de nuevo producto abierto, por ejemplo desde el Panel principal (?nuevo=1).
    if (this.esAdmin && this.ruta.snapshot.queryParamMap.has('nuevo')) {
      this.abrirNuevo();
      this.router.navigate([], { queryParams: { nuevo: null }, replaceUrl: true });
    }
  }

  cargar(): void {
    this.cargando.set(true);
    this.hayCriterios.set(!!this.filtros.controls.buscar.value.trim() || this.contarFiltros() > 0);

    this.lotesServicio.listar(this.filtros.getRawValue(), this.pagina(), this.tamanoPagina).subscribe({
      next: respuesta => {
        this.resultado.set(respuesta);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  buscarProductos(): void {
    this.pagina.set(1);
    this.cargar();
  }

  limpiarBusqueda(): void {
    this.filtros.reset();
    this.buscarProductos();
  }

  contarFiltros(): number {
    return contarFiltrosLotes(this.filtros);
  }

  cambiarPagina(pagina: number): void {
    this.pagina.set(pagina);
    this.cargar();
  }

  invalido(control: { invalid: boolean; touched: boolean }): boolean {
    return control.invalid && control.touched;
  }

  // --- Agregar un lote al producto de la fila ---
  private cargarProveedores(): void {
    if (!this.proveedores().length) {
      this.proveedoresServicio.listar().subscribe(lista => this.proveedores.set(lista));
    }
  }

  abrirNuevoLote(fila: LoteListado): void {
    this.cargarProveedores();
    this.formularioLote.reset({ idProveedor: 0, numeroLote: '', costo: 0, precio: 0, stock: 0 });
    this.nuevoLote.set(fila);
  }

  guardarNuevoLote(): void {
    const fila = this.nuevoLote();
    if (!fila) return;

    if (this.formularioLote.invalid) {
      this.formularioLote.markAllAsTouched();
      return;
    }

    this.lotesServicio.agregar(fila.idProducto, this.formularioLote.getRawValue()).subscribe({
      next: creado => {
        this.notificaciones.exito(`Lote ${creado.numeroLote} agregado a ${creado.producto}.`);
        this.nuevoLote.set(null);
        this.cargar();
      }
    });
  }

  // --- Desactivar un lote (solo ese lote) ---
  confirmarDesactivar(): void {
    const fila = this.porDesactivar();
    if (!fila) return;

    this.lotesServicio.desactivar(fila.idLote).subscribe({
      next: () => {
        this.notificaciones.exito(`Lote ${fila.numeroLote} de ${fila.producto} desactivado.`);
        this.porDesactivar.set(null);
        if (this.resultado()?.items.length === 1 && this.pagina() > 1) {
          this.pagina.update(p => p - 1);
        }
        this.cargar();
      },
      error: () => this.porDesactivar.set(null)
    });
  }

  // --- Nuevo producto con su primer lote ---
  abrirNuevo(): void {
    this.cargarProveedores();
    this.formularioNuevo.reset({ codigo: '', nombre: '', descripcion: '', idProveedor: 0, numeroLote: '', costo: 0, precio: 0, stock: 0 });
    this.modalNuevo.set(true);
  }

  guardarNuevo(): void {
    if (this.formularioNuevo.invalid) {
      this.formularioNuevo.markAllAsTouched();
      return;
    }

    const { codigo, nombre, descripcion, ...lote } = this.formularioNuevo.getRawValue();
    this.guardando.set(true);

    this.productos.crear({ codigo, nombre, descripcion: descripcion || null, ofertas: [lote] }).subscribe({
      next: () => {
        this.notificaciones.exito(`Producto ${codigo} - ${nombre} creado con el lote ${lote.numeroLote}.`);
        this.guardando.set(false);
        this.modalNuevo.set(false);
        this.buscarProductos();
      },
      error: () => this.guardando.set(false)
    });
  }

  // --- Editar producto (y desactivarlo) ---
  abrirEditar(fila: LoteListado): void {
    this.productos.obtener(fila.idProducto).subscribe(producto => {
      this.formularioEditar.reset({ codigo: producto.codigo, nombre: producto.nombre, descripcion: producto.descripcion ?? '' });
      this.confirmandoBaja.set(false);
      this.editando.set(producto.id);
    });
  }

  guardarEditar(): void {
    const id = this.editando();
    if (!id) return;

    if (this.formularioEditar.invalid) {
      this.formularioEditar.markAllAsTouched();
      return;
    }

    const datos = this.formularioEditar.getRawValue();
    this.guardando.set(true);

    this.productos.actualizar(id, { ...datos, descripcion: datos.descripcion || null }).subscribe({
      next: () => {
        this.notificaciones.exito(`Producto ${this.formularioEditar.getRawValue().codigo} actualizado.`);
        this.guardando.set(false);
        this.editando.set(null);
        this.cargar();
      },
      error: () => this.guardando.set(false)
    });
  }

  desactivarProducto(): void {
    const id = this.editando();
    if (!id) return;

    const codigo = this.formularioEditar.getRawValue().codigo;
    this.productos.eliminar(id).subscribe({
      next: () => {
        this.notificaciones.exito(`Producto ${codigo} y sus lotes desactivados.`);
        this.editando.set(null);
        this.buscarProductos();
      },
      error: () => this.confirmandoBaja.set(false)
    });
  }
}
