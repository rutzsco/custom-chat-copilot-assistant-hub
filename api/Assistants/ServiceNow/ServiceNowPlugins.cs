using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    public class IncidentResponse
    {
        [JsonPropertyName("result")]
        public List<Incident> Result { get; set; }
    }


    public class Incident
    {
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("priority")]
        public string Priority { get; set; }

        [JsonPropertyName("active")]
        public string Active { get; set; }
    }
}