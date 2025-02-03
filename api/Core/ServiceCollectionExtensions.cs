using Microsoft.SemanticKernel;
using MinimalApi.Services;
using MinimalApi.Services.Skills;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

namespace Assistants.API.Core
{
    internal static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddAzureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("WeatherAPI", client =>
            {
                client.BaseAddress = new Uri("https://api.weather.gov/");
            });

            services.AddHttpClient("ServiceNowAPI", client =>
            {
                string serviceNowInstanceUrl = configuration["ServiceNowInstanceUrl"];
                string serviceNowUsername = configuration["ServiceNowUsername"];
                string serviceNowPassword = configuration["ServiceNowPassword"];
                client.BaseAddress = new Uri(serviceNowInstanceUrl);
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{serviceNowUsername}:{serviceNowPassword}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            });



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


                Kernel autoBody = Kernel.CreateBuilder()
                   .AddAzureOpenAIChatCompletion(deployedModelName3, azureOpenAiServiceEndpoint3, azureOpenAiServiceKey3)
                   .Build();

                // Build Plugins
                kernel3.Plugins.AddFromObject(new WeatherPlugins(sp.GetRequiredService<IHttpClientFactory>()));
                kernel4.Plugins.AddFromObject(new WeatherPlugins(sp.GetRequiredService<IHttpClientFactory>()));
                kernel3.Plugins.AddFromObject(new ServiceNowPlugins(sp.GetRequiredService<IHttpClientFactory>()));
                kernel4.Plugins.AddFromObject(new ServiceNowPlugins(sp.GetRequiredService<IHttpClientFactory>()));
                autoBody.Plugins.AddFromType<AutoDamageAnalysisTools>("AutoDamageAnalysisTools");

                var facade =  new OpenAIClientFacade(deployedModelName3, kernel3, deployedModelName4, kernel4);
                facade.RegisterKernel("AutoDamageAnalysis", autoBody);
                return facade;
            });

            services.AddSingleton<WeatherChatService>();
            services.AddSingleton<ServiceNowChatService>();
            services.AddSingleton<AutoDamageAnalysisChatService>();
            return services;
        }
    }
}
