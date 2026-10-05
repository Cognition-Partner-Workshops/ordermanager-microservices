import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/inventory', pathMatch: 'full' },
  { path: 'inventory', loadComponent: () => import('./components/inventory-list.component').then(m => m.InventoryListComponent) },
  { path: 'low-stock', loadComponent: () => import('./components/low-stock.component').then(m => m.LowStockComponent) },
  { path: 'new', loadComponent: () => import('./components/inventory-form.component').then(m => m.InventoryFormComponent) },
];
