using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(WeddingPhotoSharing.Startup))]

namespace WeddingPhotoSharing
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // TODO: AzureStorage関連の処理など
        }
    }
}
