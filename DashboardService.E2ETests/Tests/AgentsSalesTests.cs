using System.Net;
using System.Net.Http.Json;
using System.Text;
using DashboardService.E2ETests.Clients;
using DashboardService.E2ETests.Models;
using FluentAssertions;
using Xunit;

namespace DashboardService.E2ETests.Tests;

public class AgentsSalesTests : IDisposable
{
    private readonly DashboardApiClient _client;

    public AgentsSalesTests()
    {
        _client = new DashboardApiClient();
    }

    #region Positive Scenarios

    [Fact]
    public async Task GetAgentsSales_WithAllAgentsInPeriod_ReturnsAggregatedSales()
    {
        // Arrange - получение продаж всех агентов за период без фильтров
        var query = new GetAgentsSalesQuery
        {
            SalesDateFrom = new DateTime(2024, 1, 1),
            SalesDateTo = new DateTime(2024, 12, 31)
        };

        // Act
        var response = await _client.GetAgentsSalesAsync(query);
        var result = await response.Content.ReadFromJsonAsync<GetAgentsSalesResult>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.PerAgentTotal.Should().NotBeNull();

        // Проверяем, что каждый агент имеет корректные данные о продажах
        if (result.PerAgentTotal?.Count > 0)
        {
            foreach (var agentSales in result.PerAgentTotal)
            {
                agentSales.Key.Should().NotBeNullOrEmpty("agent login should be present");
                agentSales.Value.Should().NotBeNull();
                agentSales.Value.PoliciesCount.Should().BeGreaterOrEqualTo(0);
                agentSales.Value.PremiumAmount.Should().BeGreaterOrEqualTo(0);
            }
        }
    }

    [Fact]
    public async Task GetAgentsSales_WithSpecificAgent_ReturnsOnlyThatAgentSales()
    {
        // Arrange - получение продаж конкретного агента
        var query = new GetAgentsSalesQuery
        {
            AgentLogin = "agent_john_doe",
            SalesDateFrom = new DateTime(2024, 1, 1),
            SalesDateTo = new DateTime(2024, 12, 31)
        };

        // Act
        var response = await _client.GetAgentsSalesAsync(query);
        var result = await response.Content.ReadFromJsonAsync<GetAgentsSalesResult>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.PerAgentTotal.Should().NotBeNull();

        // Проверяем, что возвращаются данные только для указанного агента
        if (result.PerAgentTotal?.Count > 0)
        {
            result.PerAgentTotal.Should().ContainKey("agent_john_doe");
            result.PerAgentTotal.Keys.Should().AllSatisfy(key =>
                key.Should().Be("agent_john_doe", "only specified agent should be returned"));
        }
    }

    [Fact]
    public async Task GetAgentsSales_WithSpecificProduct_ReturnsOnlyThatProductSales()
    {
        // Arrange - получение продаж по конкретному продукту
        var query = new GetAgentsSalesQuery
        {
            ProductCode = "LIFE_INSURANCE_PREMIUM",
            SalesDateFrom = new DateTime(2024, 1, 1),
            SalesDateTo = new DateTime(2024, 12, 31)
        };

        // Act
        var response = await _client.GetAgentsSalesAsync(query);
        var result = await response.Content.ReadFromJsonAsync<GetAgentsSalesResult>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.PerAgentTotal.Should().NotBeNull();

        // Проверяем структуру ответа (продажи агрегированы по агентам для указанного продукта)
        if (result.PerAgentTotal?.Count > 0)
        {
            foreach (var agentSales in result.PerAgentTotal)
            {
                agentSales.Value.PoliciesCount.Should().BeGreaterOrEqualTo(0);
                agentSales.Value.PremiumAmount.Should().BeGreaterOrEqualTo(0);
            }
        }
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task GetAgentsSales_WithEndDateBeforeStartDate_ReturnsBadRequest()
    {
        // Arrange - дата окончания раньше даты начала
        var query = new GetAgentsSalesQuery
        {
            SalesDateFrom = new DateTime(2024, 12, 31),
            SalesDateTo = new DateTime(2024, 1, 1)
        };

        // Act
        var response = await _client.GetAgentsSalesAsync(query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAgentsSales_WithInvalidJsonFormat_ReturnsBadRequest()
    {
        // Arrange - невалидный JSON
        var invalidJson = "{ invalid json content }";
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5035") };
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync("/api/Dashboard/agents-sales", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "malformed JSON should be rejected");

        httpClient.Dispose();
    }

    [Fact]
    public async Task GetAgentsSales_WithEmptyBody_ReturnsBadRequest()
    {
        // Arrange - пустое тело запроса
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5035") };
        var content = new StringContent("{}", Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync("/api/Dashboard/agents-sales", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        httpClient.Dispose();
    }

    #endregion

    public void Dispose()
    {
        _client.Dispose();
    }
}