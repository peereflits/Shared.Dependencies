using System;

namespace Peereflits.Shared.Dependencies;

public class MultipleTypeRegistrationsException : Exception
{
    public MultipleTypeRegistrationsException(Type interfaceType)
            : base($"Assembly {interfaceType.Assembly} requires {interfaceType.Name} to be implemented exactly once "
                 + "but multiple registrations have been found."
                  ) { }
}