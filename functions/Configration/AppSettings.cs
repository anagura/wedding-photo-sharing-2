using Microsoft.Extensions.Configuration;

namespace functions.Configration
{
    public class AppSettings
    {
        public static IConfiguration Configuration { get; set; }
    }
}
