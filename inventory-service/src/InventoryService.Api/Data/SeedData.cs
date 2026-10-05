using InventoryService.Api.Models;

namespace InventoryService.Api.Data;

public static class SeedData
{
    // Mirrors the product catalog seeded by the OrderManager monolith so the two
    // services agree on product ids during the decomposition transition.
    private static readonly (int ProductId, string Name, string Sku)[] Catalog =
    {
        (1, "Widget A", "WGT-001"),
        (2, "Widget B", "WGT-002"),
        (3, "Gadget X", "GDG-001"),
        (4, "Gadget Y", "GDG-002"),
        (5, "Thingamajig", "THG-001"),
    };

    public static void Initialize(InventoryDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.InventoryItems.Any()) return;

        var items = Catalog.Select((p, i) => new InventoryItem
        {
            ProductId = p.ProductId,
            ProductName = p.Name,
            ProductSku = p.Sku,
            QuantityOnHand = (i + 1) * 50,
            ReorderLevel = 10,
            WarehouseLocation = $"A-{i + 1:D2}"
        });
        context.InventoryItems.AddRange(items);
        context.SaveChanges();
    }
}
