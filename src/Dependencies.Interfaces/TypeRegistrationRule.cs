using System;

namespace Peereflits.Shared.Dependencies;

public class TypeRegistrationRule
{
    public TypeRegistrationRule(Type typeRegistration, bool multipleInstancesAllowed)
    {
        Interface= typeRegistration;
        MultipleInstancesAllowed = multipleInstancesAllowed;
    }

    public Type Interface { get; }
    public bool MultipleInstancesAllowed { get; }
    public TypeRegistrationRule AllowMultipleInstances() => new TypeRegistrationRule(Interface, true);
}

public class TypeRegistrationRule<TInterface> : TypeRegistrationRule
{
    public TypeRegistrationRule() : base(typeof(TInterface), false) { }
    public TypeRegistrationRule(bool multipleInstancesAllowed): base(typeof(TInterface), multipleInstancesAllowed) { }
}