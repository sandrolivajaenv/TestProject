using Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using static API.Tests.ItemsApiTests;

namespace API.Tests;

public class ProductsApiTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;

    public ProductsApiTests(CustomWebAppFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_products_returns_200()
    {
        var resp = await _client.GetAsync("/api/v1/products?page=1&pageSize=10&includeItems=true");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await resp.Content.ReadFromJsonAsync<ProductListResponse>();

        payload?.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_product_respects_idempotency_key()
    {
        var dto = new { productName = "TestProd" };
        var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/products")
        {
            Content = JsonContent.Create(dto)
        };
        req1.Headers.Add("Idempotency-Key", "abc123");

        var r1 = await _client.SendAsync(req1);
        r1.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await r1.Content.ReadFromJsonAsync<CreatedProductResponse>();
        var id = (int)created!.Id;

        // same key + same body => 201 with same id
        var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/products")
        {
            Content = JsonContent.Create(dto)
        };
        req2.Headers.Add("Idempotency-Key", "abc123");
        var r2 = await _client.SendAsync(req2);

        r2.StatusCode.Should().Be(HttpStatusCode.Created);
        var created2 = await r2.Content.ReadFromJsonAsync<CreatedProductResponse>();
        ((int)created2!.Id).Should().Be(id);

        // same key + different body => 409
        var req3 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/products")
        {
            Content = JsonContent.Create(new { productName = "Other" })
        };
        req3.Headers.Add("Idempotency-Key", "abc123");
        var r3 = await _client.SendAsync(req3);

        r3.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_Product_with_empty_name_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/products", new { productName = "" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("must not be empty");
    }

    private class ProductListResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<ProductReadDto>? Items { get; set; }
    }
}
