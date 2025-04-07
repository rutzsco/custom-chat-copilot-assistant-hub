using System.ComponentModel;
using Assistants.API.Core;
using Assistants.API.Services.Prompts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinimalApi.Services.Skills;

public class DataMapperPlugins
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    public DataMapperPlugins(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [KernelFunction("GetDataProductMappings")]
    [Description("This content linking XSLT source -target to the data product and names the source")]
    [return: Description("A CSV This file with source -target to the data product and names the source")]
    public string GetDataProductMappings([Description("The name of the data product")] string dataProduct, KernelArguments arguments)
    {
        var refernceDataFile = _configuration["DataProductMappings"];
        var content = refernceDataFile.ReadFileContent();
        return content;
    }

    [KernelFunction("GetDataProductTemplate")]
    [Description("Get the template schema for the provided data product")]
    [return: Description("schema template")]
    public string GetDataProductTemplate([Description("The name of the data product")] string dataProduct, KernelArguments arguments)
    {
        var refernceDataFile = _configuration["DataProductTemplate"];
        var content = refernceDataFile.ReadFileContent();
        return content;
    }

    [KernelFunction("GetDataProductAttributeDefinitions")]
    [Description("Gets a list of description for fields in the data product")]
    [return: Description("list of description for fields in the provided data product")]
    public string GetDataProductAttributeDefinitions([Description("The name of the data product")] string dataProduct, KernelArguments arguments)
    {
        var refernceDataFile = _configuration["DataProductAttributeDefinitions"];
        var content = refernceDataFile.ReadFileContent();
        return content;
    }

    [KernelFunction("GetTransformation")]
    [Description("Get XSLT used to create DataProductMappings")]
    [return: Description("XSLT used to create DataProductMappings")]
    public string GetTransformation([Description("The name of the data product")] string dataProduct, KernelArguments arguments)
    {
        var refernceDataFile = _configuration["TransformationFile"];
        var content = refernceDataFile.ReadFileContent();
        return content;
    }
}
