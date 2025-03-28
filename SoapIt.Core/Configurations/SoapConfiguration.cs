using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoapIt.Core.Models;
using SoapIt.Settings;

namespace SoapIt.Core.Configurations;

internal static class SoapConfiguration
{
    public static IServiceCollection AddSoapClient<TClient>(this IServiceCollection services, IConfiguration configuration)
        where TClient : ISoapClient
    {
        var setting = configuration.GetSection("Soap:Endpoints").Get<SoapSettings>();

        services.TryAddSingleton<SoapTypeCache>();

        services.AddScoped(typeof(TClient), provider =>
            {
                var soapCache = provider.GetRequiredService<SoapTypeCache>();
                return new SoapProxy(soapCache, setting[SoapEndpointAttribute.Get<TClient>()]).Build<TClient>();
            });

        return services;
    }
}