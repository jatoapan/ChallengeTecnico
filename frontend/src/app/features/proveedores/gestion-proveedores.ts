import { NgTemplateOutlet } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FiltroEstado, Proveedor } from '../../core/models/producto.model';
import { NotificacionService } from '../../core/services/notificacion.service';
import { ProveedorService } from '../../core/services/proveedor.service';
import { Encabezado } from '../../shared/encabezado/encabezado';
import { EstadoVacio } from '../../shared/estado-vacio/estado-vacio';
import { Modal } from '../../shared/modal/modal';

// Gestión de proveedores (solo Administrador): crear, editar y desactivar.
// Un proveedor con lotes activos no se puede desactivar (lo valida la API).
@Component({
  selector: 'app-gestion-proveedores',
  imports: [Encabezado, EstadoVacio, Modal, ReactiveFormsModule, NgTemplateOutlet],
  templateUrl: './gestion-proveedores.html'
})
export class GestionProveedores implements OnInit {
  private proveedoresServicio = inject(ProveedorService);
  private notificaciones = inject(NotificacionService);
  private fb = inject(FormBuilder);

  proveedores = signal<Proveedor[]>([]);
  cargando = signal(true);
  estado = signal<FiltroEstado>('Todos');
  // FormGroup en el <form>: Enter busca sin recargar la página.
  filtros = new FormGroup({ buscar: new FormControl('', { nonNullable: true }) });
  porReactivar = signal<Proveedor | null>(null);

  // Modal de formulario: null = cerrado; { id: null } = nuevo; { id: n } = editar.
  edicion = signal<{ id: number | null } | null>(null);
  porDesactivar = signal<Proveedor | null>(null);

  formulario = this.fb.nonNullable.group({
    nombre: ['', [Validators.required, Validators.maxLength(60)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(50)]],
    celular: ['', [Validators.pattern(/^\d{10}$/)]]
  });

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.cargando.set(true);
    this.proveedoresServicio.listar(this.estado(), this.filtros.controls.buscar.value).subscribe({
      next: lista => {
        this.proveedores.set(lista);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  cambiarEstado(estado: string): void {
    this.estado.set(estado as FiltroEstado);
    this.cargar();
  }

  confirmarReactivar(): void {
    const proveedor = this.porReactivar();
    if (!proveedor) return;

    this.proveedoresServicio.reactivar(proveedor.id).subscribe({
      next: () => {
        this.notificaciones.exito(`Proveedor ${proveedor.nombre} reactivado.`);
        this.porReactivar.set(null);
        this.cargar();
      },
      error: () => this.porReactivar.set(null)
    });
  }

  invalido(control: { invalid: boolean; touched: boolean }): boolean {
    return control.invalid && control.touched;
  }

  nuevo(): void {
    this.formulario.reset({ nombre: '', email: '', celular: '' });
    this.edicion.set({ id: null });
  }

  editar(proveedor: Proveedor): void {
    this.formulario.reset({
      nombre: proveedor.nombre,
      email: proveedor.email ?? '',
      celular: proveedor.celular ?? ''
    });
    this.edicion.set({ id: proveedor.id });
  }

  guardar(): void {
    const edicion = this.edicion();
    if (!edicion) return;

    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    const valores = this.formulario.getRawValue();
    const datos = { ...valores, celular: valores.celular || null };

    const peticion = edicion.id
      ? this.proveedoresServicio.actualizar(edicion.id, datos)
      : this.proveedoresServicio.crear(datos);

    peticion.subscribe({
      next: proveedor => {
        this.notificaciones.exito(edicion.id ? `Proveedor ${proveedor.nombre} actualizado.` : `Proveedor ${proveedor.nombre} creado.`);
        this.edicion.set(null);
        this.cargar();
      }
    });
  }

  confirmarDesactivar(): void {
    const proveedor = this.porDesactivar();
    if (!proveedor) return;

    this.proveedoresServicio.desactivar(proveedor.id).subscribe({
      next: () => {
        this.notificaciones.exito(`Proveedor ${proveedor.nombre} desactivado.`);
        this.porDesactivar.set(null);
        this.cargar();
      },
      error: () => this.porDesactivar.set(null)
    });
  }
}
