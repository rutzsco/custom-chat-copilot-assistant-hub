using System.Text.Json.Serialization;

namespace Assistants.Hub.API.Assistants.ServiceNow
{
    public class TicketResponse
    {
        [JsonPropertyName("result")]
        public List<Ticket> Result { get; set; } = new List<Ticket>();
    }

    public class Ticket
    {
        [JsonPropertyName("sys_id")]
        public string SysId { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

        [JsonPropertyName("priority")]
        public string Priority { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }
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
