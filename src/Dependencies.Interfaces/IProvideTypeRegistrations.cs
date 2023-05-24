using System.Collections.Generic;

namespace Peereflits.Shared.Dependencies;

public interface IProvideTypeRegistrations
{
    IEnumerable<TypeRegistration> Execute();
}