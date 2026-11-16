import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-lista-productos',
  template: `
    <div class="container py-4">
      <h1 class="h3">Productos</h1>
      <p>Sesión iniciada como {{ auth.usuario() }} ({{ auth.rol() }})</p>
      <button class="btn btn-outline-secondary" (click)="salir()">Cerrar sesión</button>
    </div>
  `
})
export class ListaProductos {
  auth = inject(AuthService);
  private router = inject(Router);

  salir(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}