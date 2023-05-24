using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Peereflits.Shared.Dependencies;

public static class RegisterTypeRegistrations
{
    public static IServiceCollection AddTypeRegistrations<TTypeRegistrations>(this IServiceCollection services)
            where TTypeRegistrations : IProvideTypeRegistrations, new()
    {
        IEnumerable<TypeRegistration> registrations = new TTypeRegistrations().Execute();
        foreach (TypeRegistration registration in registrations)
        {
            if (registration.Service == null)
            {
                services.AddService(registration.Lifetime, registration.Interface, registration.ImplementationFactory);
            }
            else
            {
                services.AddService(registration.Lifetime, registration.Interface, registration.Service);
            }
        }

        return services;
    }

    public static void AddService(this IServiceCollection services, Lifetime lifetime, Type serviceType, Type implementationType)
    {
        switch (lifetime)
        {
            case Lifetime.Singleton:
                services.AddSingleton(serviceType, implementationType);
                break;
            case Lifetime.Scoped:
                services.AddScoped(serviceType, implementationType);
                break;
            case Lifetime.Instance:
                services.AddTransient(serviceType, implementationType);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), $"Lifetime '{lifetime}' used by registration for service '{serviceType.FullName}' has not been implemented");
        }
    }

    public static void AddService(this IServiceCollection services, Lifetime lifetime, Type serviceType, Func<IServiceProvider, object> implementationFactory)
    {
        switch (lifetime)
        {
            case Lifetime.Singleton:
                services.AddSingleton(serviceType, implementationFactory);
                break;
            case Lifetime.Scoped:
                services.AddScoped(serviceType, implementationFactory);
                break;
            case Lifetime.Instance:
                services.AddTransient(serviceType, implementationFactory);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), $"Lifetime '{lifetime}' used by registration for service '{serviceType.FullName}' has not been implemented");
        }
    }
}