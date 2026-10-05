import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateInventoryItemRequest, InventoryItem, StockCheckLine, StockCheckResponse } from '../models/inventory';

@Injectable({ providedIn: 'root' })
export class InventoryApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/inventory`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<InventoryItem[]> {
    return this.http.get<InventoryItem[]>(this.baseUrl);
  }

  getLowStock(): Observable<InventoryItem[]> {
    return this.http.get<InventoryItem[]>(`${this.baseUrl}/low-stock`);
  }

  getByProductId(productId: number): Observable<InventoryItem> {
    return this.http.get<InventoryItem>(`${this.baseUrl}/product/${productId}`);
  }

  create(request: CreateInventoryItemRequest): Observable<InventoryItem> {
    return this.http.post<InventoryItem>(this.baseUrl, request);
  }

  restock(productId: number, quantity: number): Observable<InventoryItem> {
    return this.http.post<InventoryItem>(`${this.baseUrl}/product/${productId}/restock`, { quantity });
  }

  deduct(productId: number, quantity: number): Observable<InventoryItem> {
    return this.http.post<InventoryItem>(`${this.baseUrl}/product/${productId}/deduct`, { quantity });
  }

  check(items: StockCheckLine[]): Observable<StockCheckResponse> {
    return this.http.post<StockCheckResponse>(`${this.baseUrl}/check`, { items });
  }

  delete(productId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/product/${productId}`);
  }
}
