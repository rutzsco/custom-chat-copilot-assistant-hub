//using Azure.Storage.Blobs;
//using Azure.Storage.Blobs.Models;

using Azure.AI.OpenAI;
using Azure;
using Microsoft.SemanticKernel;
using Microsoft.Azure.Cosmos.Fluent;
using MinimalApi.Services;
using MinimalApi.Services.Skills;

namespace Assistants.API.Core
{
    internal static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddAzureServices(this IServiceCollection services)
        {

            services.AddSingleton<OpenAIClientFacade>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var deployedModelName3 = config[AppConfigurationSetting.AOAIStandardChatGptDeployment];
                var azureOpenAiServiceEndpoint3 = config[AppConfigurationSetting.AOAIStandardServiceEndpoint];
                var azureOpenAiServiceKey3 = config[AppConfigurationSetting.AOAIStandardServiceKey];

                ArgumentException.ThrowIfNullOrEmpty(deployedModelName3);
                ArgumentException.ThrowIfNullOrEmpty(azureOpenAiServiceEndpoint3);
                ArgumentException.ThrowIfNullOrEmpty(azureOpenAiServiceKey3);

                var deployedModelName4 = config[AppConfigurationSetting.AOAIPremiumChatGptDeployment];
                var azureOpenAiServiceEndpoint4 = config[AppConfigurationSetting.AOAIPremiumServiceEndpoint];
                var azureOpenAiServiceKey4 = config[AppConfigurationSetting.AOAIPremiumServiceKey];
                ArgumentException.ThrowIfNullOrEmpty(deployedModelName4);
                ArgumentException.ThrowIfNullOrEmpty(azureOpenAiServiceEndpoint4);
                ArgumentException.ThrowIfNullOrEmpty(azureOpenAiServiceKey4);



                // Build Kernels
                Kernel kernel3 = Kernel.CreateBuilder()
                   .AddAzureOpenAIChatCompletion(deployedModelName3, azureOpenAiServiceEndpoint3, azureOpenAiServiceKey3)
                   .Build();

                Kernel kernel4 = Kernel.CreateBuilder()
                   .AddAzureOpenAIChatCompletion(deployedModelName4, azureOpenAiServiceEndpoint4, azureOpenAiServiceKey4)
                   .Build();

                // Build Plugins
                kernel3.Plugins.AddFromType<WeatherPlugins>("Weather");
                kernel4.Plugins.AddFromType<WeatherPlugins>("Weather");

                return new OpenAIClientFacade(deployedModelName3, kernel3, deployedModelName4, kernel4);
            });

            services.AddSingleton<WeatherChatService>();
            return services;
        }
    }
}
