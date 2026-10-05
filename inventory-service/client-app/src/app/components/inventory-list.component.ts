import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InventoryApiService } from '../services/inventory-api.service';
import { InventoryItem } from '../models/inventory';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h2>Inventory</h2>
    <div class="toolbar">
      <label><input type="checkbox" [(ngModel)]="lowStockOnly"> Low stock only</label>
      <button type="button" (click)="load()">Refresh</button>
      <span *ngIf="message" class="success">{{ message }}</span>
      <span *ngIf="error" class="error">{{ error }}</span>
    </div>
    <p *ngIf="loaded && !visibleItems.length">No inventory records.</p>
    <table *ngIf="visibleItems.length">
      <thead>
        <tr>
          <th>Product</th><th>SKU</th><th>On Hand</th><th>Reorder Level</th>
          <th>Location</th><th>Last Restocked</th><th>Restock</th>
        </tr>
      </thead>
      <tbody>
        <tr *ngFor="let i of visibleItems" [class.low-stock]="i.isLowStock">
          <td>{{ i.productName }} <span *ngIf="i.isLowStock" class="badge">LOW</span></td>
          <td>{{ i.productSku }}</td>
          <td>{{ i.quantityOnHand }}</td>
          <td>{{ i.reorderLevel }}</td>
          <td>{{ i.warehouseLocation }}</td>
          <td>{{ i.lastRestocked | date:'medium' }}</td>
          <td>
            <form class="inline" (ngSubmit)="restock(i)">
              <input type="number" min="1" [(ngModel)]="restockQty[i.productId]" name="qty-{{ i.productId }}" required>
              <button type="submit" [disabled]="!(restockQty[i.productId] > 0)">Add</button>
            </form>
          </td>
        </tr>
      </tbody>
    </table>
  `
})
export class InventoryListComponent implements OnInit {
  items: InventoryItem[] = [];
  restockQty: Record<number, number> = {};
  lowStockOnly = false;
  loaded = false;
  message = '';
  error = '';

  constructor(private api: InventoryApiService) {}

  ngOnInit() { this.load(); }

  get visibleItems(): InventoryItem[] {
    return this.lowStockOnly ? this.items.filter(i => i.isLowStock) : this.items;
  }

  load() {
    this.error = '';
    this.api.getAll().subscribe({
      next: data => { this.items = data; this.loaded = true; },
      error: err => { this.error = err?.error?.error ?? 'Failed to load inventory'; this.loaded = true; }
    });
  }

  restock(item: InventoryItem) {
    const qty = this.restockQty[item.productId];
    if (!(qty > 0)) { return; }
    this.message = '';
    this.error = '';
    this.api.restock(item.productId, qty).subscribe({
      next: updated => {
        this.items = this.items.map(i => i.productId === updated.productId ? updated : i);
        this.restockQty[item.productId] = 0;
        this.message = `Restocked ${updated.productName} (+${qty}); now ${updated.quantityOnHand} on hand`;
      },
      error: err => { this.error = err?.error?.error ?? 'Restock failed'; }
    });
  }
}
