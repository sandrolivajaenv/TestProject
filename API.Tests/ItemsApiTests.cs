using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace API.Tests;
public class ItemsApiTests(CustomWebAppFactory factory) : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_and_list_items_under_product()
    {
        // create a product first
        var response = await _client.PostAsJsonAsync("/api/v1/products", new { productName = "WithItems" });
        response.EnsureSuccessStatusCode(); // throws if not 2xx

        var prod = await response.Content.ReadFromJsonAsync<CreatedProductResponse>();
        int productId = prod.Id;

        // add item
        var r1 = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/items", new { quantity = 5 });
        r1.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // list
        var r2 = await _client.GetAsync($"/api/v1/products/{productId}/items");
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await r2.Content.ReadFromJsonAsync<List<CreatedItemResponse>>();
        items!.Count.Should().Be(1);
        ((int)items![0].Quantity).Should().Be(5);
    }

}
