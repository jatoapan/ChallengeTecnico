import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { rolGuard } from './core/guards/rol.guard';
import { Layout } from './shared/layout/layout';

// Carga perezosa: cada pantalla se descarga solo cuando se visita (loadComponent).
// El layout se carga al inicio porque envuelve a todas las pantallas con sesión.
export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login').then(m => m.Login) },
  {
    path: '',
    component: Layout,
    canActivate: [authGuard],
    children: [
      {
        path: 'resumen',
        loadComponent: () => import('./features/resumen/resumen').then(m => m.Resumen)
      },
      {
        path: 'productos',
        loadComponent: () => import('./features/productos/catalogo-productos').then(m => m.CatalogoProductos)
      },
      {
        path: 'inventario',
        loadComponent: () => import('./features/inventario/control-inventario').then(m => m.ControlInventario)
      },
      {
        path: 'proveedores',
        canActivate: [rolGuard('Administrador')],
        loadComponent: () => import('./features/proveedores/gestion-proveedores').then(m => m.GestionProveedores)
      },
      {
        path: 'matriz',
        loadComponent: () => import('./features/productos/matriz-precios').then(m => m.MatrizPrecios)
      },
      {
        path: 'usuarios',
        canActivate: [rolGuard('Administrador')],
        loadComponent: () => import('./features/usuarios/gestion-usuarios').then(m => m.GestionUsuarios)
      },
      {
        path: 'historial',
        canActivate: [rolGuard('Administrador')],
        loadComponent: () => import('./features/auditoria/historial').then(m => m.Historial)
      },
      { path: '', pathMatch: 'full', redirectTo: 'resumen' }
    ]
  },
  { path: '**', redirectTo: '' }
];
