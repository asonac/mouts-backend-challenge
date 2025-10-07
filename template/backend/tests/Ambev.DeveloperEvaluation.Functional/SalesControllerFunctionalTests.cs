using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.FunctionalTests;

public class SalesControllerFunctionalTests : IClassFixture<WebApplicationFactory<WebApi.Program>>
{
    private readonly HttpClient _client;

    public SalesControllerFunctionalTests(WebApplicationFactory<WebApi.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Should create sale successfully")]
    public async Task CreateSale_ReturnsCreated()
    {
        var request = new CreateSaleRequest
        {
            SaleNumber = "12345",
            SaleDate = DateTime.UtcNow,
            CustomerId = "CUST001",
            CustomerName = "Wesley",
            BranchId = "BR001",
            BranchName = "Lins",
            Items = new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest
                {
                    ProductId = "PROD001",
                    ProductName = "Product A",
                    Quantity = 5,
                    UnitPrice = 100
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/sales", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Should retrieve sale by ID")]
    public async Task GetSale_ReturnsSale()
    {
        var createRequest = new CreateSaleRequest
        {
            SaleNumber = "GET001",
            SaleDate = DateTime.UtcNow,
            CustomerId = "CUST002",
            CustomerName = "João",
            BranchId = "BR002",
            BranchName = "São Paulo",
            Items = new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest
                {
                    ProductId = "PROD002",
                    ProductName = "Product B",
                    Quantity = 4,
                    UnitPrice = 100
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>();

        var getResponse = await _client.GetAsync($"/api/sales/{created?.Data?.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getResponse.Content.ReadFromJsonAsync<ApiResponseWithData<GetSaleResponse>>();
        var sale = result?.Data;

        sale.Should().NotBeNull();
        sale?.CustomerName.Should().Be("João");
        sale?.TotalAmount.Should().Be(360); // 400 - 10%
    }

    [Fact(DisplayName = "Should update sale successfully")]
    public async Task UpdateSale_ReturnsUpdated()
    {
        var createRequest = new CreateSaleRequest
        {
            SaleNumber = "UPD001",
            SaleDate = DateTime.UtcNow,
            CustomerId = "CUST003",
            CustomerName = "Maria",
            BranchId = "BR003",
            BranchName = "Campinas",
            Items = new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest
                {
                    ProductId = "PROD003",
                    ProductName = "Product C",
                    Quantity = 3,
                    UnitPrice = 100
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>();

        var updateRequest = new UpdateSaleRequest
        {
            SaleNumber = "UPD001",
            SaleDate = DateTime.UtcNow,
            CustomerId = "CUST003",
            CustomerName = "Maria Updated",
            BranchId = "BR003",
            BranchName = "Campinas Updated",
            Items = new List<UpdateSaleItemRequest>
            {
                new UpdateSaleItemRequest
                {
                    ProductId = "PROD003",
                    ProductName = "Product C",
                    Quantity = 6,
                    UnitPrice = 100
                }
            }
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/sales/{created?.Data?.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<UpdateSaleResponse>();
        updated.Should().NotBeNull();
        //updated!.CustomerName.Should().Be("Maria Updated");
        //updated.TotalAmount.Should().Be(540); // 600 - 10%
    }

    [Fact(DisplayName = "Should delete sale successfully")]
    public async Task DeleteSale_ReturnsOk()
    {
        var createRequest = new CreateSaleRequest
        {
            SaleNumber = "DEL001",
            SaleDate = DateTime.UtcNow,
            CustomerId = "CUST004",
            CustomerName = "Carlos",
            BranchId = "BR004",
            BranchName = "Ribeirão",
            Items = new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest
                {
                    ProductId = "PROD004",
                    ProductName = "Product D",
                    Quantity = 2,
                    UnitPrice = 100
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>();

        var deleteResponse = await _client.DeleteAsync($"/api/sales/{created?.Data?.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/sales/{created?.Data?.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
