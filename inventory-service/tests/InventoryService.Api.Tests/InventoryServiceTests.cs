using InventoryService.Api.Data;
using InventoryService.Api.Models;
using InventoryService.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InventoryService.Api.Tests;

public class InventoryServiceTests
{
    private static InventoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new InventoryDbContext(options);
        SeedData.Initialize(context);
        return context;
    }

    private static Api.Services.InventoryService CreateService(InventoryDbContext context) =>
        new(context, NullLogger<Api.Services.InventoryService>.Instance);

    [Fact]
    public async Task GetAll_ReturnsSeededItems()
    {
        using var context = CreateContext();
        var items = await CreateService(context).GetAllAsync();
        Assert.Equal(5, items.Count);
        Assert.All(items, i => Assert.False(string.IsNullOrEmpty(i.ProductSku)));
    }

    [Fact]
    public async Task Restock_IncreasesQuantityAndUpdatesTimestamp()
    {
        using var context = CreateContext();
        var service = CreateService(context);
        var before = await context.InventoryItems.FirstAsync(i => i.ProductId == 1);
        var qty = before.QuantityOnHand;
        var restockedAt = before.LastRestocked;

        var after = await service.RestockAsync(1, 25);

        Assert.Equal(qty + 25, after.QuantityOnHand);
        Assert.True(after.LastRestocked >= restockedAt);
    }

    [Fact]
    public async Task Deduct_ReducesQuantity()
    {
        using var context = CreateContext();
        var service = CreateService(context);
        var qty = (await context.InventoryItems.FirstAsync(i => i.ProductId == 2)).QuantityOnHand;

        var after = await service.DeductAsync(2, 10);

        Assert.Equal(qty - 10, after.QuantityOnHand);
    }

    [Fact]
    public async Task Deduct_ThrowsInsufficientStock_WhenNotEnough()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var ex = await Assert.ThrowsAsync<InsufficientStockException>(() => service.DeductAsync(1, 99999));
        Assert.Equal(1, ex.ProductId);
        Assert.Equal(99999, ex.Requested);
        Assert.Equal(50, ex.Available);
    }

    [Fact]
    public async Task Deduct_ThrowsNotFound_ForUnknownProduct()
    {
        using var context = CreateContext();
        await Assert.ThrowsAsync<InventoryNotFoundException>(() => CreateService(context).DeductAsync(999, 1));
    }

    [Fact]
    public async Task GetLowStock_ReturnsItemsAtOrBelowReorderLevel()
    {
        using var context = CreateContext();
        var service = CreateService(context);
        await service.DeductAsync(1, 45);

        var low = await service.GetLowStockAsync();

        Assert.Single(low);
        Assert.Equal(1, low[0].ProductId);
        Assert.True(low[0].IsLowStock);
    }

    [Fact]
    public async Task CheckAvailability_ReportsPerLineAndOverall()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.CheckAvailabilityAsync(new StockCheckRequest(new List<StockCheckLine>
        {
            new(1, 10),
            new(2, 500),
            new(999, 1)
        }));

        Assert.False(result.AllAvailable);
        Assert.True(result.Items[0].Sufficient);
        Assert.False(result.Items[1].Sufficient);
        Assert.Equal(0, result.Items[2].Available);
    }

    [Fact]
    public async Task Create_RejectsDuplicateProduct()
    {
        using var context = CreateContext();
        var service = CreateService(context);
        var request = new CreateInventoryItemRequest(1, "Widget A", "WGT-001", 5, 2, "B-01");

        await Assert.ThrowsAsync<DuplicateInventoryException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task Create_ThenUpdate_PersistsChanges()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var created = await service.CreateAsync(new CreateInventoryItemRequest(42, "Doohickey", "DHK-001", 7, 3, "C-07"));
        var updated = await service.UpdateAsync(42, new UpdateInventoryItemRequest(null, null, 5, "C-08"));

        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(5, updated.ReorderLevel);
        Assert.Equal("C-08", updated.WarehouseLocation);
        Assert.Equal("Doohickey", updated.ProductName);
    }
}
