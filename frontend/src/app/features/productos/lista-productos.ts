import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Paginado, Producto } from '../../core/models/producto.model';
import { ProductoService } from '../../core/services/producto.service';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NotificacionService } from '../../core/services/notificacion.service';

@Component({
  selector: 'app-lista-productos',
  imports: [FormsModule, CurrencyPipe, RouterLink],
  templateUrl: './lista-productos.html'
})
export class ListaProductos implements OnInit {
  private productos = inject(ProductoService);
  auth = inject(AuthService);
  private notificaciones = inject(NotificacionService);
  porEliminar = signal<Producto | null>(null);

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

  get totalPaginas(): number {
    const resultado = this.resultado();
    return resultado ? Math.max(1, Math.ceil(resultado.total / resultado.tamanoPagina)) : 1;
  }
}