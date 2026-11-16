import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FiltroEstado } from '../../core/models/producto.model';
import { AuthService } from '../../core/services/auth.service';
import { NotificacionService } from '../../core/services/notificacion.service';
import { Rol, Usuario, UsuarioService } from '../../core/services/usuario.service';
import { Encabezado } from '../../shared/encabezado/encabezado';
import { EstadoVacio } from '../../shared/estado-vacio/estado-vacio';
import { Modal } from '../../shared/modal/modal';

// Misma política que la API: 8+ caracteres con mayúscula, minúscula, número y símbolo.
const POLITICA_CLAVE = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$/;

// Gestión de usuarios (solo Administrador): crear, editar, restablecer contraseña, desactivar y reactivar.
// Las reglas de seguridad (no desactivarse a sí mismo, no quedarse sin Administrador) las valida la API.
@Component({
  selector: 'app-gestion-usuarios',
  imports: [Encabezado, EstadoVacio, Modal, ReactiveFormsModule, NgTemplateOutlet, DatePipe],
  templateUrl: './gestion-usuarios.html'
})
export class GestionUsuarios implements OnInit {
  private usuariosServicio = inject(UsuarioService);
  private notificaciones = inject(NotificacionService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  usuarios = signal<Usuario[]>([]);
  roles = signal<Rol[]>([]);
  cargando = signal(true);
  verClave = signal(false);

  // Modal de formulario: null = cerrado; { id: null } = nuevo; { id: n } = editar.
  edicion = signal<{ id: number | null } | null>(null);
  porDesactivar = signal<Usuario | null>(null);
  porReactivar = signal<Usuario | null>(null);

  filtros = new FormGroup({
    buscar: new FormControl('', { nonNullable: true }),
    estado: new FormControl<FiltroEstado>('Todos', { nonNullable: true })
  });

  formulario = this.fb.nonNullable.group({
    nombres: ['', [Validators.required, Validators.maxLength(60)]],
    apellidos: ['', [Validators.required, Validators.maxLength(60)]],
    usuario: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(20), Validators.pattern(/^[a-zA-Z0-9._-]+$/)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(50)]],
    idRol: [0, [Validators.min(1)]],
    password: ['', [Validators.pattern(POLITICA_CLAVE)]]
  });

  ngOnInit(): void {
    this.usuariosServicio.roles().subscribe(lista => this.roles.set(lista));
    this.cargar();
  }

  cargar(): void {
    const { buscar, estado } = this.filtros.getRawValue();
    this.cargando.set(true);
    this.usuariosServicio.listar(buscar, estado).subscribe({
      next: lista => {
        this.usuarios.set(lista);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  esUstedMismo(usuario: Usuario): boolean {
    return usuario.usuario === this.auth.usuario();
  }

  invalido(control: { invalid: boolean; touched: boolean }): boolean {
    return control.invalid && control.touched;
  }

  nuevo(): void {
    this.formulario.reset({ nombres: '', apellidos: '', usuario: '', email: '', idRol: 0, password: '' });
    // Al crear, la contraseña es obligatoria.
    this.formulario.controls.password.setValidators([Validators.required, Validators.pattern(POLITICA_CLAVE)]);
    this.formulario.controls.password.updateValueAndValidity();
    this.verClave.set(false);
    this.edicion.set({ id: null });
  }

  editar(usuario: Usuario): void {
    this.formulario.reset({
      nombres: usuario.nombres,
      apellidos: usuario.apellidos,
      usuario: usuario.usuario,
      email: usuario.email,
      idRol: usuario.idRol,
      password: ''
    });
    // Al editar, la contraseña es opcional: vacía significa "no cambiar".
    this.formulario.controls.password.setValidators([Validators.pattern(POLITICA_CLAVE)]);
    this.formulario.controls.password.updateValueAndValidity();
    this.verClave.set(false);
    this.edicion.set({ id: usuario.id });
  }

  guardar(): void {
    const edicion = this.edicion();
    if (!edicion) return;

    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    const valores = this.formulario.getRawValue();
    const datos = { ...valores, password: valores.password || null };
    const peticion = edicion.id
      ? this.usuariosServicio.actualizar(edicion.id, datos)
      : this.usuariosServicio.crear(datos);

    peticion.subscribe({
      next: usuario => {
        this.notificaciones.exito(edicion.id ? `Usuario ${usuario.usuario} actualizado.` : `Usuario ${usuario.usuario} creado.`);
        this.edicion.set(null);
        this.cargar();
      }
    });
  }

  confirmarDesactivar(): void {
    const usuario = this.porDesactivar();
    if (!usuario) return;

    this.usuariosServicio.desactivar(usuario.id).subscribe({
      next: () => {
        this.notificaciones.exito(`Usuario ${usuario.usuario} desactivado.`);
        this.porDesactivar.set(null);
        this.cargar();
      },
      error: () => this.porDesactivar.set(null)
    });
  }

  confirmarReactivar(): void {
    const usuario = this.porReactivar();
    if (!usuario) return;

    this.usuariosServicio.reactivar(usuario.id).subscribe({
      next: () => {
        this.notificaciones.exito(`Usuario ${usuario.usuario} reactivado.`);
        this.porReactivar.set(null);
        this.cargar();
      },
      error: () => this.porReactivar.set(null)
    });
  }
}
