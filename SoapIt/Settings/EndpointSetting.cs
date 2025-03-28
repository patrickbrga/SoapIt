namespace SoapIt.Settings;

public record CredentialsSetting(string Username, string Password);

public class EndpointSetting
{
    public string Address { get; set; }
    public string Namespace { get; set; }
    public int Timeout { get; set; } = 30; // Time in Seconds
    public SoapConnectionType ConnectionType { get; set; } = SoapConnectionType.Basic;
    public bool AllowCookies { get; set; } = true;
    public CredentialsSetting Credentials { get; set; }
}