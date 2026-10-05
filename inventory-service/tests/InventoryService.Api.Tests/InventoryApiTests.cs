using System.Net;
using System.Net.Http.Json;
using InventoryService.Api.Data;
using InventoryService.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace InventoryService.Api.Tests;

public class InventoryApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<InventoryDbContext>>();
            services.RemoveAll<InventoryDbContext>();
            services.AddDbContext<InventoryDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }
}

public class InventoryApiTests : IClassFixture<InventoryApiFactory>
{
    private readonly HttpClient _client;

    public InventoryApiTests(InventoryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Metrics_ExposesPrometheusText()
    {
        var response = await _client.GetAsync("/metrics");
        response.EnsureSuccessStatusCode();
        Assert.Contains("# HELP", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetAll_ReturnsSeededInventory()
    {
        var items = await _client.GetFromJsonAsync<List<InventoryItem>>("/api/inventory");
        Assert.NotNull(items);
        Assert.Equal(5, items!.Count);
    }

    [Fact]
    public async Task GetByProduct_Returns404ForUnknown()
    {
        var response = await _client.GetAsync("/api/inventory/product/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deduct_Returns409OnInsufficientStock()
    {
        var response = await _client.PostAsJsonAsync("/api/inventory/product/3/deduct", new QuantityRequest(99999));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Insufficient stock", body);
    }

    [Fact]
    public async Task RestockThenDeduct_RoundTrips()
    {
        var before = await _client.GetFromJsonAsync<InventoryItem>("/api/inventory/product/4");
        var restock = await _client.PostAsJsonAsync("/api/inventory/product/4/restock", new QuantityRequest(10));
        restock.EnsureSuccessStatusCode();
        var deduct = await _client.PostAsJsonAsync("/api/inventory/product/4/deduct", new QuantityRequest(10));
        deduct.EnsureSuccessStatusCode();
        var after = await deduct.Content.ReadFromJsonAsync<InventoryItem>();

        Assert.Equal(before!.QuantityOnHand, after!.QuantityOnHand);
    }

    [Fact]
    public async Task Restock_Returns400ForNonPositiveQuantity()
    {
        var response = await _client.PostAsJsonAsync("/api/inventory/product/4/restock", new QuantityRequest(0));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Check_ReturnsAvailability()
    {
        var response = await _client.PostAsJsonAsync("/api/inventory/check",
            new StockCheckRequest(new List<StockCheckLine> { new(1, 1), new(5, 100000) }));
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<StockCheckResponse>();
        Assert.NotNull(result);
        Assert.False(result!.AllAvailable);
        Assert.True(result.Items[0].Sufficient);
        Assert.False(result.Items[1].Sufficient);
    }
}
