import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Paginado, Producto } from '../../core/models/producto.model';
import { ProductoService } from '../../core/services/producto.service';

@Component({
  selector: 'app-lista-productos',
  imports: [FormsModule, CurrencyPipe],
  templateUrl: './lista-productos.html'
})
export class ListaProductos implements OnInit {
  private productos = inject(ProductoService);

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

  get totalPaginas(): number {
    const resultado = this.resultado();
    return resultado ? Math.max(1, Math.ceil(resultado.total / resultado.tamanoPagina)) : 1;
  }
}