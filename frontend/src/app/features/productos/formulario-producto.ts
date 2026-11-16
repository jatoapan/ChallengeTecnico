import { Component, OnInit, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Oferta, Producto, ProductoGuardar, Proveedor } from '../../core/models/producto.model';
import { NotificacionService } from '../../core/services/notificacion.service';
import { ProductoService } from '../../core/services/producto.service';
import { ProveedorService } from '../../core/services/proveedor.service';

@Component({
  selector: 'app-formulario-producto',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './formulario-producto.html'
})
export class FormularioProducto implements OnInit {
  private fb = inject(FormBuilder);
  private productos = inject(ProductoService);
  private proveedoresServicio = inject(ProveedorService);
  private notificaciones = inject(NotificacionService);
  private router = inject(Router);
  private ruta = inject(ActivatedRoute);

  proveedores = signal<Proveedor[]>([]);
  guardando = signal(false);
  id: number | null = null;

  formulario = this.fb.nonNullable.group({
    codigo: ['', [Validators.required, Validators.maxLength(10)]],
    nombre: ['', [Validators.required, Validators.maxLength(60)]],
    descripcion: ['', [Validators.maxLength(255)]],
    ofertas: this.fb.array([this.crearOferta()])
  });

  get ofertas() {
    return this.formulario.controls.ofertas;
  }

  ngOnInit(): void {
    this.proveedoresServicio.listar().subscribe(lista => this.proveedores.set(lista));

    const idRuta = this.ruta.snapshot.paramMap.get('id');

    if (idRuta) {
      this.id = Number(idRuta);
      this.productos.obtener(this.id).subscribe({
        next: producto => this.cargarProducto(producto),
        error: () => this.router.navigate(['/productos'])
      });
    }
  }

  agregarOferta(): void {
    this.ofertas.push(this.crearOferta());
  }

  quitarOferta(indice: number): void {
    if (this.ofertas.length > 1) {
      this.ofertas.removeAt(indice);
    }
  }

  invalido(control: AbstractControl | null): boolean {
    return !!control && control.invalid && control.touched;
  }

  guardar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    const valores = this.formulario.getRawValue();

    const datos: ProductoGuardar = {
      codigo: valores.codigo,
      nombre: valores.nombre,
      descripcion: valores.descripcion || null,
      ofertas: valores.ofertas.map(o => ({
        idLote: o.idLote,
        idProveedor: o.idProveedor,
        numeroLote: o.numeroLote,
        precio: o.precio,
        stock: o.stock
      }))
    };

    this.guardando.set(true);

    const peticion = this.id
      ? this.productos.actualizar(this.id, datos)
      : this.productos.crear(datos);

    peticion.subscribe({
      next: () => {
        this.notificaciones.exito(this.id ? 'Producto actualizado.' : 'Producto creado.');
        this.router.navigate(['/productos']);
      },
      error: () => this.guardando.set(false)
    });
  }

  private cargarProducto(producto: Producto): void {
    this.formulario.patchValue({
      codigo: producto.codigo,
      nombre: producto.nombre,
      descripcion: producto.descripcion ?? ''
    });

    this.ofertas.clear();
    producto.ofertas.forEach(oferta => this.ofertas.push(this.crearOferta(oferta)));
  }

  private crearOferta(oferta?: Oferta) {
    return this.fb.nonNullable.group({
      idLote: [oferta?.idLote ?? (null as number | null)],
      idProveedor: [oferta?.idProveedor ?? 0, [Validators.min(1)]],
      numeroLote: [oferta?.numeroLote ?? '', [Validators.required, Validators.maxLength(20)]],
      precio: [oferta?.precio ?? 0, [Validators.required, Validators.min(0)]],
      stock: [oferta?.stock ?? 0, [Validators.required, Validators.min(0)]]
    });
  }
}