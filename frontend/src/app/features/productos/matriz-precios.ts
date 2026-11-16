import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatrizPrecios as Matriz, Proveedor } from '../../core/models/producto.model';
import { ProductoService } from '../../core/services/producto.service';
import { Encabezado } from '../../shared/encabezado/encabezado';
import { EstadoVacio } from '../../shared/estado-vacio/estado-vacio';
import { Paginacion } from '../../shared/paginacion/paginacion';

interface Fila {
  id: number;
  nombre: string;
  precios: (number | null)[];  // un precio por proveedor (null: ese proveedor no lo ofrece)
  total: number;               // suma de los precios del producto
  pvp: number;                 // promedio de los precios del producto
}

const suma = (valores: number[]): number => valores.reduce((total, v) => total + v, 0);

@Component({
  selector: 'app-matriz-precios',
  imports: [Encabezado, EstadoVacio, ReactiveFormsModule, CurrencyPipe, Paginacion],
  templateUrl: './matriz-precios.html',
  styleUrl: './matriz-precios.scss'
})
export class MatrizPrecios implements OnInit {
  private productos = inject(ProductoService);

  readonly tamanoPagina = 10;
  // FormGroup: hace que Angular controle el submit (Enter) y evita que el navegador recargue la página.
  filtros = new FormGroup({ buscar: new FormControl('', { nonNullable: true }) });
  pagina = signal(1);
  busquedaAplicada = signal('');
  resultado = signal<Matriz | null>(null);
  cargando = signal(false);

  proveedores = computed<Proveedor[]>(() => this.resultado()?.proveedores ?? []);

  // Se calcula una vez por respuesta, no en cada ciclo de detección de cambios.
  filas = computed<Fila[]>(() => {
    const matriz = this.resultado();
    if (!matriz) return [];

    return matriz.items.map(item => {
      const precios = matriz.proveedores.map(p => item.precios[p.id]?.precio ?? null);
      const ofrecidos = precios.filter((p): p is number => p !== null);
      const total = suma(ofrecidos);
      return { id: item.id, nombre: item.nombre, precios, total, pvp: ofrecidos.length ? total / ofrecidos.length : 0 };
    });
  });

  // Totales acumulados: suma de los precios de cada proveedor entre los productos de la página.
  totales = computed(() =>
    this.proveedores().map((_, i) => suma(this.filas().map(f => f.precios[i] ?? 0)))
  );

  granTotal = computed(() => suma(this.totales()));

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.cargando.set(true);
    this.busquedaAplicada.set(this.filtros.controls.buscar.value.trim());

    this.productos.matrizPrecios(this.filtros.controls.buscar.value, this.pagina(), this.tamanoPagina).subscribe({
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

  cambiarPagina(pagina: number): void {
    this.pagina.set(pagina);
    this.cargar();
  }
}
