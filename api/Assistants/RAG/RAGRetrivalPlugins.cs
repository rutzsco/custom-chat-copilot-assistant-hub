﻿using System.ComponentModel;
using Assistants.API.Core;
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
    public async Task<IEnumerable<KnowledgeSource>> GetKnowledgeSourcesAsync(Kernel kernel, [Description("Search query")] string searchQuery)
    {
        var settings = kernel.Data["VectorSearchSettings"] as VectorSearchSettings;
        if (settings == null)
            throw new ArgumentNullException(nameof(settings), "VectorSearchSettings cannot be null");

        var logic = new SearchLogic<KwiecienCustomIndexDefinitionV2>(_azureOpenAIClient, _searchClientFactory, KwiecienCustomIndexDefinitionV2.SelectFieldNames, settings);
        var results = await logic.SearchAsync(searchQuery);

        // Add kernel context for diagnostics
        kernel.AddFunctionCallResult("get_knowledge_articles", $"Search Query: {searchQuery} /n {System.Text.Json.JsonSerializer.Serialize(results)}", results);

        return results;
    }
}