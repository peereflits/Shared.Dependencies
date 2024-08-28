![Logo](./img/peereflits-logo.svg) 

# Peereflits.Shared.Dependencies


[Inversion of control](https://en.wikipedia.org/wiki/Inversion_of_control) is one of the five [SOLID](https://en.wikipedia.org/wiki/SOLID) principles and is deemed so important that the .NET framework has its own [DI (Dependency Inversion)](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) container. However, there is a "gotcha" in this DI: when an application is set up in multiple assemblies (multiple layers), all types (classes & interfaces) that are not in the host project must be defined as `public`. Otherwise they cannot be registered in the .NET DI container (`Microsoft.Extensions.DependencyInjection`) of the host (eg an ASP&#46;NET app). The host (of the DI container) must be able to resolve all types.

The idea behind *Peereflits.Shared.Dependencies* is that the registration of types in the .NET DI container, and possible validation of these registrations, is simplified within a multi-layered application without the dependencies being leaked.

See the following as the basic design of an application architecture:

![Basic setup of an application architecture](./docs/Peereflits.Shared.Dependencies-Basic%20Architecture.png)
*Basic design of application architecture*

If a host (Api) wants to register all types at startup (`Startup.cs`, or `Program.cs` in minimal APIs), then all types in *Logic Domain*-Implementation & -Interfaces must be `public` are. This is undesirable as it violates one of the "[tenets of object orientation](https://codingarchitect.wordpress.com/2006/09/27/four-tenets-of-oop/)" and/or [coding guidelines](https://csharpcodingguidelines.com/maintainability-guidelines/#AV1501).

> **Note:** Including an `InternalsVisibleTo(...)` attribute in the host as a workaround is of course also a violation of the OOP principle "encapsultation". The intended usage of this attribute is to unit test internal classes.

*Peereflits.Shared.Dependencies* can solve this problem. It can register internal classes and interfaces and make it available to a host with respect to the access modifiers op the defined types. *Peereflits.Shared.Dependencies* consists of two packages:
1. *Peereflits.Shared.Dependencies.Interfaces* defines a lightweight API that allows registration of types and validations of registration. This is used in implementation projects;
1. *Peereflits.Shared.Dependencies.Extensions* is only used in the host project. It contains a number of extension methods that allow the registered types to be included in the .NET DI container.

The *Peereflits.Shared.Dependencies.Interfaces* api looks like this:

[![](https://mermaid.ink/img/pako:eNqlUsFqwzAM_RXj00ZLPyCUwmAtBFYY7Y6B4dhK5-HYwZa7hdJ8-2wnoSUsu-xk-Vl-ek_ShXIjgGaUK-bcs2Qny-pCpxvJX605SwFvbQMHOEmHlqE02l0KTch6LTWCrRiHzSYCi-03cI_w8EjyrfY1WFYq6Ka_u0JfxwrTt0S8iCjJR_IbdAR7lgOw85p3-YAMOu27KT-BY0fyulFQg8bEumMcjW3TvxdZAcoayBjcqRmhwR70HiJDb_Ao9UkBGr1MN24aECnMtUOmec819bRabW61ZlsasqbYXJMOXsFso0pjFNl7hTJ0YNTlnpQyXyDuvM4JieT_m29k6Gf8Z41fDEecLmkgrpkUYSmTjoLiR5hlQbMQCqhYMFfQUCCkMo_m2GpOM7QeltQ3giEMa0yziikXUBAyzH8_LHo8rj_MrBrt?type=png)](https://mermaid.live/edit#pako:eNqlUsFqwzAM_RXj00ZLPyCUwmAtBFYY7Y6B4dhK5-HYwZa7hdJ8-2wnoSUsu-xk-Vl-ek_ShXIjgGaUK-bcs2Qny-pCpxvJX605SwFvbQMHOEmHlqE02l0KTch6LTWCrRiHzSYCi-03cI_w8EjyrfY1WFYq6Ka_u0JfxwrTt0S8iCjJR_IbdAR7lgOw85p3-YAMOu27KT-BY0fyulFQg8bEumMcjW3TvxdZAcoayBjcqRmhwR70HiJDb_Ao9UkBGr1MN24aECnMtUOmec819bRabW61ZlsasqbYXJMOXsFso0pjFNl7hTJ0YNTlnpQyXyDuvM4JieT_m29k6Gf8Z41fDEecLmkgrpkUYSmTjoLiR5hlQbMQCqhYMFfQUCCkMo_m2GpOM7QeltQ3giEMa0yziikXUBAyzH8_LHo8rj_MrBrt)

Where as
1. `IProvideTypeRegistrations` + `TypeRegistration` take care of the *DI registration definition*;
1. `IProvideTypeRegistrationRules` + `TypeRegistrationRule` take care of the *DI registration validation*.

To use the DI registration definition, each implementation assembly (Logic Domain) must define a `public class` that implements `IProvideTypeRegistrations`. For DI registration validation, there must be a `public class` in each implementation assembly that implements `IProvideTypeRegistrationRules`.

>**Tip:** Use as convention for the DI registration definition `TypeRegistrations` and for the DI registration validation `TypeRegistrationRules`.

In the Host (Services&#46;Api) there should be an (internal) `class` that collects all type registration definitions (of all projects). This class also implements `IProvideTypeRegistrations`. Likewise, there should be an (internal) `class` that collects all type registration validations (of all projects). This class implements `IProvideTypeRegistrationRules`.

In the startup of the application (Api), the extension method 'AddTypeRegistrations' (with the class of the collected type registration definitions) can be called on the `IServiceCollection`; this adds the type registrations to the .Net DI container.

Then the extension method `Validate` (with the class of the collected type registration validations) can be called on the `IServiceCollection`; it validates that all interface types included in this list are actually registered in the .Net DI container.

>**Tip:** Use the convention for the collected DI registration definitions `TypeRegistrationsResolver` and for the collected DI registration validation use `TypeRegistrationRulesResolver`.

If during the validation process an interface is found that has no implementation, an `UnregisteredTypeException` is thrown. Also a `MultipleTypeRegistrationsException` may be thrown. This happens when an implementation of an interface is found multiple times while only allowing one implementation (this is the default behavior). The host/Api then already fails during boot; so even during a debug session. This way, type registration errors are detected early.

> **Note:** It is important that `Validate` is called *after* `AddTypeRegistrations`.

## Implementation example

In the example below, the starting point is that only the public interfaces needed in the host are included in a separate project (as in `Logic.Domain.Interfaces`). This project has no external dependencies to other projects or external libraries and only contains types from the BCL (preferably only POCOs or DTOs with primitive types). The "Implementation" contains all the logic for executing the domain logic where the own (internal) services also implement their own interface (which also makes unit testing easier).

Example:

in *MyOwn.Logic.Domain.Interfaces.dll*

```` csharp
namespace MyOwn.Logic.Domain;

public interface IGetUser
{
   UserDto Execute(Guid id);
}

public class UserDto
{
   public Guid Id { get; set; }
   public string FullName { get; set; }
}
````

in *MyOwn.Logic.Domain.dll*

```` csharp
namespace MyOwn.Logic.Domain;

internal interface IRepository<T>
{
   T GetById(id);
}

internal class UserEntity
{
   public Guid UserId { get; set; }
   public string FirstName { get; set; }
   public string LastName { get; set; }
}

internal class Repository<UserEntity> : IRepository<UserEntity>
{
   private readonly IDatabaseContext context;
   public Repository(IDatabaseContext context) => this.context = context;
   /* implementation ommited for brevity */
}

internal interface IMapper
{
   UserDto MapUser(UserEntity instance);
}

internal class Mapper: IMapper
{
   public UserDto MapUser(UserEntity instance) { ... }
}

internal class GetUserQuery : IGetUser
{
     private readonly IRepository<UserEntity> repository;
     private readonly IMapper mapper;

     public GetUserQuery
     (
        IRepository<UserEntity> repository,
        IMapper mapper
     )
     {
      
       this.repository = repository;
       this.mapper = mapper;
     }

     public UserDto Execute(Guid id)
     {
        var entity = repository.GetById(id);
        return mapper.MapUser(entity);
     }
}
````

The (internal) types of the domain layer can be registered in the host by implementing the `IProvideTypeRegistrations` interface in a public class (called `TypeRegistrations` by convention), like the following:

in *MyOwn.Logic.Domain.dll*

```` csharp
namespace MyOwn.Logic.Domain;

public class TypeRegistrations : IProvideTypeRegistrations
{
     private readonly List<TypeRegistration> registrations = new List<TypeRegistration>
                      {
                          new TypeRegistration<IRepository<UserEntity>, Repository<UserEntity>>(Lifetime.Scoped),
                          new TypeRegistration<IGetUser, GetUserQuery>(Lifetime.Scoped),
                          new TypeRegistration<IMapper, Mapper>(Lifetime.Singleton),
                      };

     public IEnumerable<TypeRegistration> Execute() => registrations;
}
````

The `TypeRegistrationRules` are intended to define which types *must* be registered for all logic to function. All interfaces from the `TypeRegistrations` could be included here, but the added value mainly lies in the recording of external dependencies (types from other projects or packages). So these are the interface types that <u>have no implementation</u> in *MyOwn.Logic.Domain.dll* but are <u>used</u> by services (consumed by classes) in *MyOwn .Logic.Domain.dll*. This becomes especially useful when using a [Hexagonal Architecture](http://alistair.cockburn.us/Hexagonal+architecture).

**Note:** Hexagonal Architecture is also referred to as "Onion Architecture". See [part 1](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/), [part 2](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-2/), [part 3](https://jeffreypalermo.com/2008/08/the-onion-architecture-part-3/) and [part 4](https://jeffreypalermo.com/2013/08/onion-architecture-part-4-after-four-years/) by Jeffrey Palermo, or this article on [clean architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) and [Common web application architectures](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture). In the illustration *Basic Application Architecture Setup*, *Infrastructure* (DTOs and Interfaces) is an example of this where it is defined in *Shared* while the host (Api) provides the implementation.

In the above code example there is no implementation of `IDatabaseContext` in *MyOwn.Logic.Domain.dll*; it comes from a different package. Since the `Repository<UserEntity>` implementation *does* depend on this interface, an instance must be available at runtime. This is enforced by explicitly including this dependecy in the `TypeRegistrationRules`:

in *MyOwn.Logic.Domain.dll*

```` csharp
internal class TypeRegistrationRules : IProvideTypeRegistrationRules
{
    private readonly List<TypeRegistrationRule> rules = new List<TypeRegistrationRule>
                     {
                         new TypeRegistrationRule<IRepository<UsUserEntityer>>(),
                         new TypeRegistrationRule<IGetUser>(),
                         new TypeRegistrationRule<IMapper>(),
                         new TypeRegistrationRule<IDatabaseContext>() // This type is NOT in this assembly
                     };

    public IEnumerable<TypeRegistrationRule> Execute() => rules;
}	
````

Everything must now be knitted together in the host/Api. The Api probably also has a number of own (internal) services that are registered in a `TypeRegistrations` and `TypeRegistrationRules` (in the namespace `MyOwn.Services.Api`). Therefore, the host has the following four types:
1. `TypeRegistrations`: the DI registration definition of the (internal) services in the host;
1. `TypeRegistrationRules`: the DI registration validation of the (internal) services in the host;
1. `TypeRegistrationsResolver`: the collected DI registration definitions;
1. `TypeRegistrationRulesResolver`: the collected DI registration validations.

in *MyOwn.Services.Api.dll*

``` csharp
namespace MyOwn.Services.Api;

public class TypeRegistrations : IProvideTypeRegistrations
{
    private readonly List<TypeRegistration> registrations = new List<TypeRegistration>
                     {
                         new TypeRegistration<IMapDtoToModel, toToModelMapper>(Lifetime.Scoped),
                         ... other Api type registrations
                     };

    public IEnumerable<TypeRegistration> Execute() => registrations;
}

internal class TypeRegistrationRules : IProvideTypeRegistrationRules
{
    private readonly List<TypeRegistrationRule> rules = new List<TypeRegistrationRule>
                     {
                         new TypeRegistrationRule<IMapDtoToModel>(),
                         ... other Api type registration rules
                     };

    public IEnumerable<TypeRegistrationRule> Execute() => rules;
}	

internal class TypeRegistrationsResolver : IProvideTypeRegistrations
{
    public IEnumerable<TypeRegistration> Execute()
    {
        var registrations = new List<IProvideTypeRegistrations>
                            {
                                new MyOwn.Logic.Domain.TypeRegistrations(),
                                new MyOwn.Services.Api.TypeRegistrations()
                            };

        return registrations.SelectMany(x=>x.Execute()).ToList();
    }
}

internal class TypeRegistrationRulesResolver : IProvideTypeRegistrationRules
{
    public IEnumerable<TypeRegistrationRule> Execute()
    {
        var rules = new List<IProvideTypeRegistrationRules>
                        {
                            new MyOwn.Logic.Domain.TypeRegistrationRules(),
                            new MyOwn.Services.Api.TypeRegistrationRules()
                        };

        return rules.SelectMany(x=>x.Execute()).ToList();
    }
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        /* Configure other services, behaviours and dependencies */
        services
          .AddTypeRegistrations<TypeRegistrationsResolver>()
          .Validate<TypeRegistrationRulesResolver>(); // Call the validate last!
    }

    public void Configure(IApplicationBuilder app, ...)
    {
      ...
    }
}
```

When an application is split into multiple layers (horizontal) or into multiple domains (vertical), only the `TypeRegistrationsResolver` and `TypeRegistrationRulesResolver` are adjusted accordingly.

![Setting up an application architecture in multiple domains](./docs/Peereflits.Shared.Dependencies-Multi-domain%20Architecture.png)<br />*Establishment of an application architecture in multiple domains | vertical split*

This then becomes:

in *MyOwn.Services.Api.dll*

``` csharp
namespace MyOwn.Services.Api;

internal class TypeRegistrationsResolver : IProvideTypeRegistrations
{
    public IEnumerable<TypeRegistration> Execute()
    {
        var registrations = new List<IProvideTypeRegistrations>
                            {
                                new MyOwn.Logic.Domain1.TypeRegistrations(),
                                new MyOwn.Logic.Domain2.TypeRegistrations(),
                                new MyOwn.Logic.DomainN.TypeRegistrations(),
                                new MyOwn.Services.Api.TypeRegistrations()
                            };

        return registrations.SelectMany(x=>x.Execute()).ToList();
    }
}

internal class TypeRegistrationRulesResolver : IProvideTypeRegistrationRules
{
    public IEnumerable<TypeRegistrationRule> Execute()
    {
        var rules = new List<IProvideTypeRegistrationRules>
                        {
                            new MyOwn.Logic.Domain1.TypeRegistrationRules(),
                            new MyOwn.Logic.Domain2.TypeRegistrationRules(),
                            new MyOwn.Logic.DomainN.TypeRegistrationRules(),
                            new MyOwn.Services.Api.TypeRegistrationRules()
                        };

        return rules.SelectMany(x=>x.Execute()).ToList();
    }
}
```

Othes changes should not be necessary.

### Version support

The libraries supports the following .NET versions:
1. .NET 6.0
1. .NET 7.0
1. .NET 8.0


---

<p align="center">
&copy; No copyright applicable<br />
&#174; "Peereflits" is my codename.
</p>

---
