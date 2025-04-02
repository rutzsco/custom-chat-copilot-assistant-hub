using Microsoft.SemanticKernel;
using MinimalApi.Services;
using MinimalApi.Services.Skills;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using Microsoft.Agents.Authentication;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Protocols.Connector;
using Microsoft.Agents.Protocols.Primitives;
using MinimalApi.Services.Search;
using Assistants.Hub.API.Assistants.RAG;
using Azure;
using Assistants.Hub.API.Assistants;

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
            services.AddSingleton<SearchClientFactory>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new SearchClientFactory(config, null, new AzureKeyCredential(config[AppConfigurationSetting.AzureSearchServiceKey]));
            });
            services.AddSingleton<OpenAIClientFacade>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var standardChatGptDeployment = config["AOAIStandardChatGptDeployment"];
                var standardServiceEndpoint = config["AOAIStandardServiceEndpoint"];
                var standardServiceKey = config["AOAIStandardServiceKey"];


                var facade =  new OpenAIClientFacade(configuration, new Azure.AzureKeyCredential(standardServiceKey), null, sp.GetRequiredService<IHttpClientFactory>(), sp.GetRequiredService<SearchClientFactory>());
                return facade;
            });

            services.AddSingleton<AutoAdvisorAgent>();
            services.AddSingleton<RAGChatService>();
            services.AddSingleton<WeatherChatService>();
            services.AddSingleton<ServiceNowChatService>();
            services.AddSingleton<AutoDamageAnalysisChatService>();
            return services;
        }


        public static IHostApplicationBuilder AddBot<TBot, THandler>(this IHostApplicationBuilder builder) where TBot : IBot where THandler : class, TBot
        {
            // builder.Services.AddBotAspNetAuthentication(builder.Configuration);

            // Add Connections object to access configured token connections.
            builder.Services.AddSingleton<IConnections, ConfigurationConnections>();

            // Add factory for ConnectorClient and UserTokenClient creation
            builder.Services.AddSingleton<IChannelServiceClientFactory, RestChannelServiceClientFactory>();

            // Add the BotAdapter, this is the default adapter that works with Azure Bot Service and Activity Protocol.
            builder.Services.AddCloudAdapter();

            // Add the Bot,  this is the primary worker for the bot. 
            builder.Services.AddTransient<IBot, THandler>();

            return builder;
        }
    }
}