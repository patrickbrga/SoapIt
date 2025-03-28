using SoapIt.Settings;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;

namespace SoapIt.Core.Builders;

public sealed class SoapBuilder<TClient> : ChannelFactory<TClient>
    where TClient : class
{
    public SoapBuilder(EndpointSetting settings)
        : base(CreateBinding(settings), CreateEndpoint(settings))
    {
        ConfigureCredentials(settings);
        //TODO: Configurar certificado

        Endpoint.Binding.SendTimeout = TimeSpan.FromSeconds(settings.Timeout);
    }

    private static Binding CreateBinding(EndpointSetting settings)
    {
        if (settings.ConnectionType == SoapConnectionType.Basic)
        {
            var binding = new BasicHttpBinding()
            {
                MaxBufferPoolSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                MaxReceivedMessageSize = int.MaxValue,
                AllowCookies = settings?.AllowCookies ?? true
            };

            return binding;
        }

        if (settings.ConnectionType == SoapConnectionType.Soap12)
        {
            var binding = new CustomBinding();

            var textBindingElement = new TextMessageEncodingBindingElement
            {
                MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.None)
            };

            var httpBindingElement = new HttpTransportBindingElement
            {
                AllowCookies = settings?.AllowCookies ?? true,
                MaxBufferSize = int.MaxValue,
                MaxBufferPoolSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
            };

            binding.Elements.AddRange(textBindingElement, httpBindingElement);

            return binding;
        }

        throw new NotImplementedException("Soap Connection Type not implemented");
    }

    private static EndpointAddress CreateEndpoint(EndpointSetting settings)
    {
        if (string.IsNullOrEmpty(settings.Address))
            throw new ArgumentException("Endpoint Address not found");

        return new(new Uri(settings.Address));
    }

    private void ConfigureCredentials(EndpointSetting settings)
    {
        if (settings.Credentials is null) return;

        if (Endpoint.EndpointBehaviors.Any(x => x.GetType() == typeof(ClientCredentials))) return;

        Endpoint.EndpointBehaviors.Add(new ClientCredentials()
        {
            UserName =
            {
                UserName = settings.Credentials.Username,
                Password = settings.Credentials.Password
            }
        });
    }
}