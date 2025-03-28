using SoapIt.Core.Builders;
using SoapIt.Core.Extentions;
using SoapIt.Core.Helpers;
using SoapIt.Settings;

namespace SoapIt.Core.Models;

public class SoapProxy(SoapTypeCache soapCache, EndpointSetting _settings)
{
    private const string PROPERTY_NAME = "Channel";

    public TClient Build<TClient>()
    {
        if (soapCache.TryGet<TClient>(out var soapType))
        {
            var channel = BuildChannel(soapType.Interface);

            var implementedClass = Activator.CreateInstance(soapType.Implement);

            soapType.Implement.GetProperty(PROPERTY_NAME).SetValue(implementedClass, channel);

            return (TClient)implementedClass;
        }

        return Create<TClient>();
    }

    private TClient Create<TClient>()
    {
        var baseType = typeof(TClient);

        var soapInterfaceType = GenerateDynamicSoapInterfaceType(baseType);

        var channel = BuildChannel(soapInterfaceType);

        var generatedType = GenerateDynamicType(baseType, channel);
        var implementedClass = Activator.CreateInstance(generatedType);

        generatedType.GetProperty(PROPERTY_NAME).SetValue(implementedClass, channel);

        soapCache.Add<TClient>(soapInterfaceType, generatedType);

        return (TClient)implementedClass;
    }

    private Type GenerateDynamicSoapInterfaceType(Type baseType)
    {
        var typeBuilder = TypeHelper.CreateInterfaceType(baseType);

        typeBuilder.CreateServiceContract(_settings.Namespace);

        typeBuilder.CloneInterfaceMethods(baseType.GetMethods());

        return typeBuilder.CreateType();
    }

    private object BuildChannel<T>(T soapInterfaceType)
        where T : Type
    {
        var channelFactoryType = typeof(SoapBuilder<>).MakeGenericType(soapInterfaceType);
        var instance = Activator.CreateInstance(channelFactoryType, [_settings]);

        var createChannelMethod = instance.GetType().GetMethod("CreateChannel", []);
        return createChannelMethod.Invoke(instance, null);
    }

    private static Type GenerateDynamicType(Type baseType, object channel)
    {
        var typeBuilder = TypeHelper.CreateImplementType(baseType);

        typeBuilder.AddInterfaceImplementation(baseType);

        var clientProperty = typeBuilder.CreateProperty(channel, PROPERTY_NAME);

        typeBuilder.ImplementMethods(clientProperty, baseType.GetMethods());

        return typeBuilder.CreateType();
    }
}
