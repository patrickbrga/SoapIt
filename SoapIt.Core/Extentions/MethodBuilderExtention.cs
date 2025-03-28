using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel;

namespace SoapIt.Core.Extentions;

internal static class MethodBuilderExtention
{
    public static void CreateOperationContract(this MethodBuilder methodBuilder, MethodInfo methodInfo, out string responseParameter)
    {
        var actionAttr = (ActionAttribute)methodInfo.GetCustomAttributes(typeof(ActionAttribute), false).FirstOrDefault();

        var action = actionAttr?.Action ?? methodInfo.Name.Replace("Async", "");
        var actionName = actionAttr?.Name;
        responseParameter = actionAttr?.ResponseParameter;

        var opertionContractType = typeof(OperationContractAttribute);

        List<PropertyInfo> properties = [
            opertionContractType.GetProperty("Action"),
            opertionContractType.GetProperty("Name"),
            opertionContractType.GetProperty("ReplyAction"),
        ];
        List<string> values = [action, actionName, "*"];

        var operationContractAttributeCtor = opertionContractType.GetConstructor(Type.EmptyTypes);

        var operationContractBuilder = new CustomAttributeBuilder(operationContractAttributeCtor, [], [.. properties], [.. values]);

        methodBuilder.SetCustomAttribute(operationContractBuilder);
    }

    public static void CreateInterfaceMethodParameter(this MethodBuilder method, int index, string parameterName)
        => method.DefineParameter(index, ParameterAttributes.In, parameterName);

    public static void CreateInterfaceMethodResult(this MethodBuilder method, string parameterName)
    {
        if (string.IsNullOrEmpty(parameterName)) return;

        var builder = method.DefineParameter(0, ParameterAttributes.None, null);

        var mpType = typeof(MessageParameterAttribute);
        var mpCtor = mpType.GetConstructor(Type.EmptyTypes);
        var nameProperty = mpType.GetProperty("Name");

        builder.SetCustomAttribute(new CustomAttributeBuilder(mpCtor, [], [nameProperty], [parameterName]));
    }

    public static void ImplementMethodBody(this MethodBuilder methodBuilder, PropertyBuilder clientProperty, MethodInfo method)
    {
        var il = methodBuilder.GetILGenerator();

        var clientGetMethod = clientProperty.GetGetMethod();
        var getMethod = clientGetMethod.ReturnType.GetMethod(method.Name);

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, clientGetMethod);

        for (int i = 0; i < method.GetParameters().Length; i++)
        {
            il.Emit(OpCodes.Ldarg, i + 1);
        }

        il.Emit(OpCodes.Callvirt, getMethod);

        if (!typeof(Task).IsAssignableFrom(method.ReturnType))
        {
            il.Emit(OpCodes.Call, typeof(Task).GetMethod("FromResult").MakeGenericMethod(method.ReturnType));
        }

        il.Emit(OpCodes.Ret);
    }
}