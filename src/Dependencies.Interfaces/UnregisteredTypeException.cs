using System;

namespace Peereflits.Shared.Dependencies;

public class UnregisteredTypeException : Exception
{
    public UnregisteredTypeException(Type interfaceType)
            : base($"Assembly {interfaceType.Assembly} requires {interfaceType.Name} to be implemented. "
                 + "Did you forget to register it?"
                  ) { }
}