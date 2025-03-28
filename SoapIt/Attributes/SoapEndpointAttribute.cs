namespace SoapIt;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
public sealed class SoapEndpointAttribute(string key) : Attribute
{
    public string Key { get; } = key;

    public static string Get<T>()
        => ((SoapEndpointAttribute)typeof(T).GetCustomAttributes(typeof(SoapEndpointAttribute), false).FirstOrDefault())?.Key;
}