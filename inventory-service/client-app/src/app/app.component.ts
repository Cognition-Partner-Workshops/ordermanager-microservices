import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav>
      <h1>Inventory Service</h1>
      <a routerLink="/inventory" routerLinkActive="active">Inventory</a>
      <a routerLink="/low-stock" routerLinkActive="active">Low Stock</a>
      <a routerLink="/new" routerLinkActive="active">Add Item</a>
    </nav>
    <main><router-outlet></router-outlet></main>
  `
})
export class AppComponent {}
