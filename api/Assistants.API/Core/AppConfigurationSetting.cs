namespace Assistants.API.Core
{
    //public static class AppConfiguration
    //{
    //    public static int SearchIndexDocumentCount { get; private set; }

    //    public static string AzureStorageAccountConnectionString { get; private set; }

    //    public static string DataProtectionKeyContainer { get; private set; }

    //    public static bool EnableDataProtectionBlobKeyStorage { get; private set; }

    //    public static void Load(IConfiguration configuration)
    //    {
    //        SearchIndexDocumentCount = configuration.GetValue<int>("SearchIndexDocumentCount", 15);

    //        AzureStorageAccountConnectionString = configuration.GetValue<string>("AzureStorageAccountConnectionString");
    //        DataProtectionKeyContainer = configuration.GetValue<string>("SearchIndexSourceFieldName", "dataprotectionkeys");
    //        EnableDataProtectionBlobKeyStorage = configuration.GetValue<bool>("EnableDataProtectionBlobKeyStorage", true);
    //    }
    //}

    public static class AppConfigurationSetting
    {
        public static string AOAIStandardChatGptDeployment { get; } = "AOAIStandardChatGptDeployment";
        public static string AOAIStandardServiceEndpoint { get; } = "AOAIStandardServiceEndpoint";
        public static string AOAIStandardServiceKey { get; } = "AOAIStandardServiceKey";

        public static string AOAIPremiumChatGptDeployment { get; } = "AOAIPremiumChatGptDeployment";
        public static string AOAIPremiumServiceEndpoint { get; } = "AOAIPremiumServiceEndpoint";
        public static string AOAIPremiumServiceKey { get; } = "AOAIPremiumServiceKey";
    }
}