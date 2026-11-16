import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { Resumen as DatosResumen, ResumenService } from '../../core/services/resumen.service';
import { EstadoVacio } from '../../shared/estado-vacio/estado-vacio';

// Página de inicio: métricas, últimos lotes, accesos rápidos, lotes por reponer.
@Component({
  selector: 'app-resumen',
  imports: [EstadoVacio, RouterLink, CurrencyPipe, DatePipe, DecimalPipe],
  templateUrl: './resumen.html',
  styleUrl: './resumen.scss'
})
export class Resumen implements OnInit {
  private resumenServicio = inject(ResumenService);
  auth = inject(AuthService);

  datos = signal<DatosResumen | null>(null);
  cargando = signal(true);

  // Accesos rápidos según el rol.
  readonly accesos = this.auth.tieneRol('Administrador')
    ? [
        { ruta: '/productos', consulta: { nuevo: 1 }, icono: 'bi-plus-square', titulo: 'Nuevo producto', detalle: 'Registrar un producto con su primer lote' },
        { ruta: '/inventario', icono: 'bi-sliders', titulo: 'Ajustar inventario', detalle: 'Cambiar stock, costo o precio de un lote' },
        { ruta: '/matriz', icono: 'bi-grid-3x3', titulo: 'Comparativo de precios', detalle: 'Ver qué proveedor ofrece el mejor precio' }
      ]
    : [
        { ruta: '/inventario', icono: 'bi-sliders', titulo: 'Ajustar inventario', detalle: 'Cambiar stock, costo o precio de un lote' },
        { ruta: '/productos', icono: 'bi-box-seam', titulo: 'Consulta de productos', detalle: 'Ver el catálogo de productos' },
        { ruta: '/matriz', icono: 'bi-grid-3x3', titulo: 'Comparativo de precios', detalle: 'Ver qué proveedor ofrece el mejor precio' }
      ];

  ngOnInit(): void {
    this.resumenServicio.obtener().subscribe({
      next: datos => {
        this.datos.set(datos);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }
}
