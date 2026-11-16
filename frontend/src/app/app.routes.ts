import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { Login } from './features/auth/login';
import { ListaProductos } from './features/productos/lista-productos';
import { Layout } from './shared/layout/layout';

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: '',
    component: Layout,
    canActivate: [authGuard],
    children: [
      { path: 'productos', component: ListaProductos },
      { path: '', pathMatch: 'full', redirectTo: 'productos' }
    ]
  },
  { path: '**', redirectTo: '' }
];