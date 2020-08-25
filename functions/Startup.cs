using functions.Configration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(WeddingPhotoSharing.Startup))]

namespace WeddingPhotoSharing
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configBuilder = new ConfigurationBuilder()
                            .AddJsonFile("local.settings.json", true)
                            .AddEnvironmentVariables();

            AppSettings.Configuration = configBuilder.Build();
        }
    }
}
