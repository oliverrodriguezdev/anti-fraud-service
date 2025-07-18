using AntiFraudService.API.Controllers;
using AntiFraudService.Application.DTOs;
using AntiFraudService.Application.Interfaces;
using AntiFraudService.Application.Services;
using AntiFraudService.Infrastructure;
using AntiFraudService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace AntiFraudService.Tests.Integration;

[Trait("Category", "Integration")]
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<Infrastructure.Messaging.KafkaEventPublisher> _mockPublisher;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _mockPublisher = new Mock<Infrastructure.Messaging.KafkaEventPublisher>();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Reemplazar servicios reales con mocks para testing
                services.AddScoped<Infrastructure.Messaging.KafkaEventPublisher>(_ => _mockPublisher.Object);
                
                // Usar base de datos en memoria para tests
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
    }

    [Fact]
    public async Task CreateTransaction_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var transactionData = new CreateTransactionDto
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 100
        };

        var json = JsonSerializer.Serialize(transactionData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/transactions", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeEmpty();
        
        _mockPublisher.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTransaction_WithExistingId_ShouldReturnTransaction()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Crear transacción primero
        var transactionData = new CreateTransactionDto
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 100
        };

        var json = JsonSerializer.Serialize(transactionData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var createResponse = await client.PostAsync("/api/transactions", content);
        var transactionId = await createResponse.Content.ReadAsStringAsync();

        // Act
        var getResponse = await client.GetAsync($"/api/transactions/{transactionId}");

        // Assert
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var responseContent = await getResponse.Content.ReadAsStringAsync();
        responseContent.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTransaction_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/transactions/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTransaction_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidData = new { invalidField = "test" };

        var json = JsonSerializer.Serialize(invalidData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/transactions", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
} 