using System.Reflection;
using System.Reflection.Emit;

namespace SoapIt.Core.Helpers;

public static class TypeHelper
{
    public static TypeBuilder CreateInterfaceType(Type baseType)
        => CreateType(baseType,
            "Soapit.ServiceContracts",
            "SoapitServiceContractModule",
            "ServiceContract",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Interface);

    public static TypeBuilder CreateImplementType(Type baseType)
        => CreateType(baseType, "Soapit.Implements", "SoapitImplementModule", "Implementation", TypeAttributes.Public | TypeAttributes.Class);

    private static TypeBuilder CreateType(Type baseType, string assemblyPrefix, string module, string typePrefix, TypeAttributes typeAttributes)
    {
        var assemblyName = new AssemblyName($"{baseType.Assembly.GetName().Name}.{assemblyPrefix}");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(module);

        return moduleBuilder.DefineType($"{baseType.Name}{typePrefix}", typeAttributes);
    }
}