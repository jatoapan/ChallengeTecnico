import { Routes } from '@angular/router';
import { Login } from './features/auth/login';
import { ListaProductos } from './features/productos/lista-productos';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'productos', component: ListaProductos },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' }
];