import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';

interface Enlace {
  ruta: string;
  texto: string;
  textoOperador?: string;   // nombre para el Operador, si es distinto
  icono: string;
  soloAdmin?: boolean;
}

// Estructura de las pantallas con sesión: menú lateral fijo en escritorio
// y menú desplegable (con botón) en pantallas pequeñas.
@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layout.html',
  styleUrl: './layout.scss',
  host: { '(document:keydown.escape)': 'cerrarMenu()' }
})
export class Layout {
  auth = inject(AuthService);
  private router = inject(Router);

  menuAbierto = signal(false);

  readonly enlaces: Enlace[] = [
    { ruta: '/resumen', texto: 'Panel principal', icono: 'bi-clipboard-data' },
    { ruta: '/productos', texto: 'Gestión de productos', textoOperador: 'Consulta de productos', icono: 'bi-box-seam' },
    { ruta: '/inventario', texto: 'Control de inventario', icono: 'bi-boxes' },
    { ruta: '/proveedores', texto: 'Gestión de proveedores', icono: 'bi-building', soloAdmin: true },
    { ruta: '/matriz', texto: 'Comparativo de precios', icono: 'bi-grid-3x3' },
    { ruta: '/usuarios', texto: 'Gestión de usuarios', icono: 'bi-people', soloAdmin: true },
    { ruta: '/historial', texto: 'Auditoría de cambios', icono: 'bi-clock-history', soloAdmin: true }
  ];

  constructor() {
    // Al navegar, el menú del móvil se cierra solo.
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd), takeUntilDestroyed())
      .subscribe(() => this.menuAbierto.set(false));
  }

  texto(enlace: Enlace): string {
    return !this.auth.tieneRol('Administrador') && enlace.textoOperador ? enlace.textoOperador : enlace.texto;
  }

  visible(enlace: Enlace): boolean {
    return !enlace.soloAdmin || this.auth.tieneRol('Administrador');
  }

  alternarMenu(): void {
    this.menuAbierto.update(abierto => !abierto);
  }

  cerrarMenu(): void {
    this.menuAbierto.set(false);
  }

  salir(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
