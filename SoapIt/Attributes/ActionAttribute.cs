namespace SoapIt;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ActionAttribute : Attribute
{
    public string Action { get; set; } = string.Empty;
    public string Name { get; set; }
    public string ResponseParameter { get; set; }

    public ActionAttribute() { }

    public ActionAttribute(string action)
    {
        Action = action;
    }
}