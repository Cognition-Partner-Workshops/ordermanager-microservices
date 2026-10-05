using InventoryService.Api.Models;

namespace InventoryService.Api.Services;

public interface IInventoryService
{
    Task<List<InventoryItem>> GetAllAsync();
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<InventoryItem?> GetByProductIdAsync(int productId);
    Task<List<InventoryItem>> GetLowStockAsync();
    Task<InventoryItem> CreateAsync(CreateInventoryItemRequest request);
    Task<InventoryItem> UpdateAsync(int productId, UpdateInventoryItemRequest request);
    Task<InventoryItem> RestockAsync(int productId, int quantity);
    Task<InventoryItem> DeductAsync(int productId, int quantity);
    Task<StockCheckResponse> CheckAvailabilityAsync(StockCheckRequest request);
    Task<bool> DeleteAsync(int productId);
}
