import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { Login } from './features/auth/login';
import { ListaProductos } from './features/productos/lista-productos';
import { Layout } from './shared/layout/layout';
import { rolGuard } from './core/guards/rol.guard';
import { FormularioProducto } from './features/productos/formulario-producto';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'productos/nuevo', component: FormularioProducto, canActivate: [rolGuard('Administrador')] },
  { path: 'productos/:id/editar', component: FormularioProducto, canActivate: [rolGuard('Administrador')] },
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