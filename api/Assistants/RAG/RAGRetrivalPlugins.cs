using System.ComponentModel;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using MinimalApi.Services.Search;
using MinimalApi.Services.Search.IndexDefinitions;

namespace Assistants.Hub.API.Assistants.RAG;

public class RAGRetrivalPlugins
{
    private readonly SearchClientFactory _searchClientFactory;
    private readonly AzureOpenAIClient _azureOpenAIClient;

    public RAGRetrivalPlugins(SearchClientFactory searchClientFactory, AzureOpenAIClient azureOpenAIClient)
    {
        _searchClientFactory = searchClientFactory;
        _azureOpenAIClient = azureOpenAIClient;
    }

    [KernelFunction("get_knowledge_sources")]
    [Description("Gets relevant information based on the provided search term.")]
    [return: Description("A list relevant source information based on the provided search term.")]
    public async Task<IEnumerable<KnowledgeSource>> GetKnowledgeSourcesAsync(Kernel kernel, [Description("Search term")] string searchTerm)
    {
        var settings = kernel.Data["VectorSearchSettings"] as VectorSearchSettings;
        if (settings == null)
            throw new ArgumentNullException(nameof(settings), "VectorSearchSettings cannot be null");

        var logic = new SearchLogic<KwiecienCustomIndexDefinitionV2>(_azureOpenAIClient, _searchClientFactory, KwiecienCustomIndexDefinitionV2.SelectFieldNames, settings);
        var result = await logic.SearchAsync(searchTerm);
        var resultList = result.ToList();
        return resultList;
    }
}