namespace SoapIt.Core.Models;

public record SoapType(Type Interface, Type Implement);

public sealed class SoapTypeCache()
{
    public Dictionary<string, SoapType> Types { get; set; } = [];
    private object TypesLock { get; set; } = new { };

    public bool TryGet<TClient>(out SoapType soapType)
    {
        soapType = Types.GetValueOrDefault(typeof(TClient).Name);
        return soapType != null;
    }

    public void Add<TClient>(Type soapInterface, Type soapImplement)
    {
        lock (TypesLock)
        {
            Types[typeof(TClient).Name] = new(soapInterface, soapImplement);
        }
    }
}