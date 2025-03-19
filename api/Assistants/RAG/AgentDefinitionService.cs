using MinimalApi.Services.Search;
using System.Numerics;

namespace Assistants.Hub.API.Assistants.RAG
{
    public static class AgentDefinitionService
    {
        public static AgentDefinition GetAgentDefinition(string id)
        {
            return new AgentDefinition("autoserviceadvisor", "AutoServiceRAGChatSystemPrompt", new VectorSearchSettings(
                IndexName: "manuals-auto-ci-20240528182950",
                DocumentCount: 25,
                EmbeddingFieldName: "embeddings",
                EmbeddingModelName: "text-embedding",
                MaxSourceTokens: 12000,
                KNearestNeighborsCount: 5,
                Exhaustive: false,
                UseSemanticRanker: false,
                SemanticConfigurationName: "",
                SourceContainer: "manuals-auto-chunks",
                CitationUseSourcePage: true));  
        }
    }

    public record AgentDefinition(string Id, string SystemPrompt, VectorSearchSettings VectorSearchSettings);
}
