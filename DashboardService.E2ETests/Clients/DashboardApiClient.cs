using System.Net.Http.Json;
using System.Text.Json;
using DashboardService.E2ETests.Models;

namespace DashboardService.E2ETests.Clients;

public class DashboardApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public DashboardApiClient(string baseUrl = "http://localhost:5035")
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<HttpResponseMessage> GetAgentsSalesAsync(GetAgentsSalesQuery query)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Dashboard/agents-sales", query, _jsonOptions);
        return response;
    }

    public async Task<GetAgentsSalesResult?> GetAgentsSalesResultAsync(GetAgentsSalesQuery query)
    {
        var response = await GetAgentsSalesAsync(query);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GetAgentsSalesResult>(_jsonOptions);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
