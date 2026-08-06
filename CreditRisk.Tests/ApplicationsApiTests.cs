using System.Net;
using System.Net.Http.Json;
using CreditRisk.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ApplicationsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApplicationsApiTests(WebApplicationFactory<Program> factory)
    {
        // replace the real Postgres DbContext with an in-memory one
       _factory = factory.WithWebHostBuilder(builder =>
{
    builder.ConfigureServices(services =>
    {
        // remove every EF Core / DbContext-related registration
        var toRemove = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<CreditRiskDbContext>) ||
            d.ServiceType == typeof(CreditRiskDbContext) ||
            (d.ServiceType.Namespace?.Contains("EntityFrameworkCore") ?? false))
            .ToList();

        foreach (var d in toRemove)
            services.Remove(d);

        // register a clean in-memory database
        services.AddDbContext<CreditRiskDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
    });
});
    }

    [Fact]
    public async Task GetApplications_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/applications");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetApplication_ByMissingId_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/applications/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}