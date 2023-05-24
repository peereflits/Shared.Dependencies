using System.Collections.Generic;

namespace Peereflits.Shared.Dependencies;

public interface IProvideTypeRegistrationRules
{
    IEnumerable<TypeRegistrationRule> Execute();
}