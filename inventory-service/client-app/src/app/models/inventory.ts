export interface InventoryItem {
  id: number;
  productId: number;
  productName: string;
  productSku: string;
  quantityOnHand: number;
  reorderLevel: number;
  warehouseLocation: string;
  lastRestocked: string;
  updatedAt: string;
  isLowStock: boolean;
}

export interface CreateInventoryItemRequest {
  productId: number;
  productName: string;
  productSku: string;
  quantityOnHand: number;
  reorderLevel: number;
  warehouseLocation: string;
}

export interface StockCheckLine {
  productId: number;
  quantity: number;
}

export interface StockCheckResult extends StockCheckLine {
  requested: number;
  available: number;
  sufficient: boolean;
}

export interface StockCheckResponse {
  allAvailable: boolean;
  items: StockCheckResult[];
}
