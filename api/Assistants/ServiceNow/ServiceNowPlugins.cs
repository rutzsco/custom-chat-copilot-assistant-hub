using System.ComponentModel;
using System.Text.Json;
using Assistants.Hub.API.Assistants.ServiceNow;
using Microsoft.SemanticKernel;

namespace MinimalApi.Services.Skills;

public class ServiceNowPlugins
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ServiceNowPlugins(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [KernelFunction("GetIncidents")]
    [Description("Get current IT active incidents from Service Now")]
    [return: Description("A list of actice IT incidents")]
    public async Task<List<Incident>> GetIncidentsFromServiceNowAsync(KernelArguments arguments)
    {
        using var httpClient = _httpClientFactory.CreateClient("ServiceNowAPI");
        var endpoint = "api/now/table/incident?sysparm_query=active=true";

        var response = await httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Failed to retrieve incidents from ServiceNow");
        }

        try
        {
            var payloadString = await response.Content.ReadAsStringAsync();
            var incidentResponse = JsonSerializer.Deserialize<IncidentResponse>(payloadString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return incidentResponse.Result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse incidents from ServiceNow", ex);
        }
    }

    [KernelFunction("GetTickets")]
    [Description("Get current tickets for a provided user from Service Now")]
    [return: Description("A list of tickets for a given user")]
    public async Task<List<Incident>> GetTicketsFromServiceNowAsync([Description("The user id of the current user")] string userId, KernelArguments arguments)
    {
        using var httpClient = _httpClientFactory.CreateClient("ServiceNowAPI");

        var query = $"caller_id={userId}^active=true";
        var endpoint = $"api/now/table/ticket?sysparm_query={Uri.EscapeDataString(query)}";

        var response = await httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Failed to retrieve incidents from ServiceNow");
        }

        try
        {
            var payloadString = await response.Content.ReadAsStringAsync();
            var incidentResponse = JsonSerializer.Deserialize<IncidentResponse>(payloadString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return incidentResponse.Result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse incidents from ServiceNow", ex);
        }
    }
}