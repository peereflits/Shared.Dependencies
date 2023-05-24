using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Peereflits.Shared.Dependencies;

public static class ValidateTypeRegistrations
{
    public static void Validate<TProvideRegistrationRules>(this IServiceCollection services)
            where TProvideRegistrationRules : IProvideTypeRegistrationRules, new()
    {
        var rulesProvider = new TProvideRegistrationRules();

        foreach(TypeRegistrationRule rule in rulesProvider.Execute())
        {
            if(rule.MultipleInstancesAllowed)
            {
                ValidateInterfaceIsImplementedAtLeastOnce(services, rule.Interface);
            }
            else
            {
                ValidateInterfaceIsImplementedExactlyOnce(services, rule.Interface);
            }
        }
    }

    private static void ValidateInterfaceIsImplementedAtLeastOnce(IServiceCollection services, Type serviceInterface)
    {
        int registrationsFound = GetRegistrationCount(services, serviceInterface);

        if(registrationsFound == 0)
        {
            throw new UnregisteredTypeException(serviceInterface);
        }
    }

    private static void ValidateInterfaceIsImplementedExactlyOnce(IServiceCollection services, Type serviceInterface)
    {
        int registrationsFound = GetRegistrationCount(services, serviceInterface);

        switch(registrationsFound)
        {
            case 0: throw new UnregisteredTypeException(serviceInterface);
            case 1: return;
            default: throw new MultipleTypeRegistrationsException(serviceInterface);
        }
    }

    private static int GetRegistrationCount(IServiceCollection services, Type serviceInterface)
    {
        return services.Count(s => s.ServiceType == serviceInterface);
    }
}