using System.ComponentModel;

namespace SoapIt;

public enum SoapConnectionType
{
    [Description("Default Connection")]
    Basic,
    [Description("Using Soap 1.2 Connection")]
    Soap12
}