using functions.Configration;
using functions.Service;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using static functions.Const.FunctionsConst;

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

            builder.Services.AddScoped<ComputerVisionService>();
            builder.Services.AddHttpClient<ComputerVisionService>()
                .AddTransientHttpErrorPolicy(
                p => p.RetryAsync(HttpClientRetryCountOnError));
        }
    }
}
