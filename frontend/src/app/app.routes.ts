import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },
  {
    path: 'produtos',
    loadComponent: () =>
      import('./features/produtos/produtos')
        .then(m => m.Produtos)
  },
  {
    path: 'notas',
    loadComponent: () =>
      import('./features/notas/notas')
        .then(m => m.Notas)
  }
];