import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ProductoService } from '../../core/services/producto.service';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NotificacionService } from '../../core/services/notificacion.service';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Oferta, Paginado, Producto } from '../../core/models/producto.model';
import { InventarioService } from '../../core/services/inventario.service';

@Component({
  selector: 'app-lista-productos',
  imports: [FormsModule, ReactiveFormsModule, CurrencyPipe, RouterLink],
  templateUrl: './lista-productos.html'
})
export class ListaProductos implements OnInit {
  private productos = inject(ProductoService);
  auth = inject(AuthService);
  private notificaciones = inject(NotificacionService);
  porEliminar = signal<Producto | null>(null);
  private inventario = inject(InventarioService);
  private fb = inject(FormBuilder);

  ajuste = signal<{ producto: Producto; oferta: Oferta } | null>(null);

  formularioAjuste = this.fb.nonNullable.group({
    precio: [0, [Validators.required, Validators.min(0)]],
    stock: [0, [Validators.required, Validators.min(0)]]
  });

  resultado = signal<Paginado<Producto> | null>(null);
  cargando = signal(false);
  buscar = '';
  pagina = 1;
  readonly tamanoPagina = 5;

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.cargando.set(true);

    this.productos.listar(this.buscar, this.pagina, this.tamanoPagina).subscribe({
      next: respuesta => {
        this.resultado.set(respuesta);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  buscarProductos(): void {
    this.pagina = 1;
    this.cargar();
  }

  cambiarPagina(pagina: number): void {
    this.pagina = pagina;
    this.cargar();
  }

  pedirEliminar(producto: Producto): void {
    this.porEliminar.set(producto);
  }

  cancelarEliminar(): void {
    this.porEliminar.set(null);
  }

  confirmarEliminar(): void {
    const producto = this.porEliminar();
    if (!producto) return;

    this.productos.eliminar(producto.id).subscribe({
      next: () => {
        this.notificaciones.exito('Producto eliminado.');
        this.porEliminar.set(null);

        // si era el único de la página, retrocede una
        if (this.resultado()?.items.length === 1 && this.pagina > 1) {
          this.pagina--;
        }
        this.cargar();
      },
      error: () => this.porEliminar.set(null)
    });
  }

    abrirAjuste(producto: Producto, oferta: Oferta): void {
    this.formularioAjuste.reset({ precio: oferta.precio, stock: oferta.stock });
    this.ajuste.set({ producto, oferta });
  }

  cerrarAjuste(): void {
    this.ajuste.set(null);
  }

  guardarAjuste(): void {
    const actual = this.ajuste();
    if (!actual) return;

    if (this.formularioAjuste.invalid) {
      this.formularioAjuste.markAllAsTouched();
      return;
    }

    this.inventario.ajustar(actual.oferta.idLote, this.formularioAjuste.getRawValue()).subscribe({
      next: () => {
        this.notificaciones.exito('Inventario actualizado.');
        this.ajuste.set(null);
        this.cargar();
      }
    });
  }

  get totalPaginas(): number {
    const resultado = this.resultado();
    return resultado ? Math.max(1, Math.ceil(resultado.total / resultado.tamanoPagina)) : 1;
  }
}