using System;

namespace Peereflits.Shared.Dependencies;

public abstract class TypeRegistration
{
    protected TypeRegistration(Type interfaceType, Type serviceType, Lifetime lifetime)
    {
        Interface = interfaceType;
        Service = serviceType;
        Lifetime = lifetime;
    }

    protected TypeRegistration(Type interfaceType, Func<IServiceProvider, object> implementationFactory, Lifetime lifetime)
    {
        Interface = interfaceType;
        ImplementationFactory = implementationFactory;
        Lifetime = lifetime;
    }

    public Type Interface { get; }
    public Type? Service { get; }
    public Func<IServiceProvider, object>? ImplementationFactory { get; }
    public Lifetime Lifetime { get; }
}

public class TypeRegistration<TInterface, TService> : TypeRegistration where TService : TInterface
{
    public TypeRegistration(Lifetime lifetime) : base(typeof(TInterface), typeof(TService), lifetime) { }
}

public class TypeRegistration<TInterface> : TypeRegistration
{
    public TypeRegistration(Func<IServiceProvider, object> implementationFactory, Lifetime lifetime) : base(typeof(TInterface), implementationFactory, lifetime) { }
}