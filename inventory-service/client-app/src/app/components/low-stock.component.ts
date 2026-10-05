import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InventoryApiService } from '../services/inventory-api.service';
import { InventoryItem } from '../models/inventory';

@Component({
  selector: 'app-low-stock',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h2>Low Stock</h2>
    <p *ngIf="error" class="error">{{ error }}</p>
    <p *ngIf="loaded && !items.length && !error" class="success">All products are above their reorder level.</p>
    <table *ngIf="items.length">
      <thead><tr><th>Product</th><th>SKU</th><th>On Hand</th><th>Reorder Level</th><th>Shortfall</th><th>Location</th></tr></thead>
      <tbody>
        <tr *ngFor="let i of items" class="low-stock">
          <td>{{ i.productName }}</td>
          <td>{{ i.productSku }}</td>
          <td>{{ i.quantityOnHand }}</td>
          <td>{{ i.reorderLevel }}</td>
          <td>{{ i.reorderLevel - i.quantityOnHand }}</td>
          <td>{{ i.warehouseLocation }}</td>
        </tr>
      </tbody>
    </table>
  `
})
export class LowStockComponent implements OnInit {
  items: InventoryItem[] = [];
  loaded = false;
  error = '';

  constructor(private api: InventoryApiService) {}

  ngOnInit() {
    this.api.getLowStock().subscribe({
      next: data => { this.items = data; this.loaded = true; },
      error: () => { this.error = 'Failed to load low-stock items'; this.loaded = true; }
    });
  }
}
