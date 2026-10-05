namespace InventoryService.Api.Services;

public class InventoryNotFoundException : Exception
{
    public InventoryNotFoundException(int productId)
        : base($"No inventory record for product {productId}") { }
}

public class InsufficientStockException : Exception
{
    public int ProductId { get; }
    public int Requested { get; }
    public int Available { get; }

    public InsufficientStockException(int productId, int requested, int available)
        : base($"Insufficient stock for product {productId}. Requested: {requested}, Available: {available}")
    {
        ProductId = productId;
        Requested = requested;
        Available = available;
    }
}

public class DuplicateInventoryException : Exception
{
    public DuplicateInventoryException(int productId)
        : base($"Inventory record for product {productId} already exists") { }
}
