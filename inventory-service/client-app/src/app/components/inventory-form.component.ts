import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InventoryApiService } from '../services/inventory-api.service';
import { CreateInventoryItemRequest } from '../models/inventory';

@Component({
  selector: 'app-inventory-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h2>Add Inventory Item</h2>
    <form #f="ngForm" (ngSubmit)="submit()">
      <p><label>Product ID <input type="number" name="productId" min="1" required [(ngModel)]="model.productId"></label></p>
      <p><label>Product Name <input name="productName" required maxlength="200" [(ngModel)]="model.productName"></label></p>
      <p><label>SKU <input name="productSku" required maxlength="50" [(ngModel)]="model.productSku"></label></p>
      <p><label>Quantity On Hand <input type="number" name="quantityOnHand" min="0" required [(ngModel)]="model.quantityOnHand"></label></p>
      <p><label>Reorder Level <input type="number" name="reorderLevel" min="0" required [(ngModel)]="model.reorderLevel"></label></p>
      <p><label>Warehouse Location <input name="warehouseLocation" maxlength="50" [(ngModel)]="model.warehouseLocation"></label></p>
      <button type="submit" [disabled]="f.invalid || saving">Create</button>
      <span *ngIf="error" class="error">{{ error }}</span>
    </form>
  `
})
export class InventoryFormComponent {
  model: CreateInventoryItemRequest = {
    productId: 0, productName: '', productSku: '', quantityOnHand: 0, reorderLevel: 10, warehouseLocation: ''
  };
  saving = false;
  error = '';

  constructor(private api: InventoryApiService, private router: Router) {}

  submit() {
    this.saving = true;
    this.error = '';
    this.api.create(this.model).subscribe({
      next: () => this.router.navigate(['/inventory']),
      error: err => {
        this.error = err?.error?.error ?? 'Failed to create inventory item';
        this.saving = false;
      }
    });
  }
}
