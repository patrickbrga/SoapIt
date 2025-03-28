using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel;

namespace SoapIt.Core.Extentions;

internal static class TypeBuilderExtention
{
    public static void CloneInterfaceMethods(this TypeBuilder typeBuilder, MethodInfo[] methods)
    {
        foreach (var method in methods)
        {
            var parameterInfos = method.GetParameters();

            var methodBuilder = typeBuilder.DefineMethod(method.Name, method.Attributes, method.ReturnType, [.. parameterInfos.Select(x => x.ParameterType)]);

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterInfo = parameterInfos[i];
                methodBuilder.CreateInterfaceMethodParameter(i + 1, parameterInfo.Name);
            }

            methodBuilder.CreateOperationContract(method, out var responseParameter);
            methodBuilder.CreateInterfaceMethodResult(responseParameter);
        }
    }

    public static void ImplementMethods(this TypeBuilder typeBuilder, PropertyBuilder clientProperty, MethodInfo[] methods)
    {
        foreach (var method in methods)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                method.ReturnType,
                [.. method.GetParameters().Select(x => x.ParameterType)]);

            methodBuilder.ImplementMethodBody(clientProperty, method);

            typeBuilder.DefineMethodOverride(methodBuilder, method);
        }
    }

    public static PropertyBuilder CreateProperty(this TypeBuilder typeBuilder, object value, string propertyName)
    {
        var clientType = value.GetType();

        var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", clientType, FieldAttributes.Private);
        var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, clientType, null);

        var defaultAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", defaultAttributes, clientType, Type.EmptyTypes);

        var getIl = getMethodBuilder.GetILGenerator();
        getIl.Emit(OpCodes.Ldarg_0);
        getIl.Emit(OpCodes.Ldfld, fieldBuilder);
        getIl.Emit(OpCodes.Ret);
        propertyBuilder.SetGetMethod(getMethodBuilder);

        var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", defaultAttributes, null, [clientType]);

        var setIl = setMethodBuilder.GetILGenerator();
        setIl.Emit(OpCodes.Ldarg_0);
        setIl.Emit(OpCodes.Ldarg_1);
        setIl.Emit(OpCodes.Stfld, fieldBuilder);
        setIl.Emit(OpCodes.Ret);
        propertyBuilder.SetSetMethod(setMethodBuilder);

        return propertyBuilder;
    }

    public static void CreateServiceContract(this TypeBuilder typeBuilder, string soapNamespace)
    {
        var serviceContractType = typeof(ServiceContractAttribute);

        var serviceContractCtor = serviceContractType.GetConstructor(Type.EmptyTypes);

        CustomAttributeBuilder serviceContractBuilder = !string.IsNullOrEmpty(soapNamespace)
            ? new(serviceContractCtor, [], [serviceContractType.GetProperty("Namespace")], [soapNamespace])
            : new(serviceContractCtor, []);

        typeBuilder.SetCustomAttribute(serviceContractBuilder);
    }
}