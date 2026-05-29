# SimpleMapper

`SimpleMapper` is a lightweight object mapper that uses preconfigured expressions instead of reflection at runtime. You define mappings up front, build an `IMapper`, and map single objects or collections.

## Quick start

```csharp
using SimpleMapper.Configuration;
using SimpleMapper.Interfaces;

var configuration = new MapperConfiguration();

configuration.CreateMap<User, UserDto>()
    .Map(destination => destination.FullName, source => source.Name)
    .Convention(destination => destination.Age, source => source.Age)
    .Convention(destination => destination.Email, source => source.Email);

var mapper = configuration.BuildMapper();

var dto = mapper.Map<User, UserDto>(new User
{
    Name = "Ada Lovelace",
    Age = 36,
    Email = "ada@example.com"
});
```

## Register with dependency injection

```csharp
using SimpleMapper;
using SimpleMapper.Configuration;
using SimpleMapper.Interfaces;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

var configuration = new MapperConfiguration();
configuration.CreateMap<User, UserDto>()
    .Map(destination => destination.FullName, source => source.Name)
    .Convention(destination => destination.Age, source => source.Age);

services.AddMapper(configuration);

var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<IMapper>();
```

You can still use the action overload if you prefer to configure mappings inline:

```csharp
services.AddMapper(config =>
{
    config.CreateMap<User, UserDto>()
        .Map(destination => destination.FullName, source => source.Name)
        .Convention(destination => destination.Age, source => source.Age);
});
```

## Reusable mapping classes

For larger applications, move each map into a dedicated configuration type and register it with `AddMap`.

```csharp
using SimpleMapper.Configuration;
using SimpleMapper.Interfaces;

public sealed class UserToUserDtoMap : IMapperConfiguration<User, UserDto>
{
    public void Configure(TypeMapBuilder<User, UserDto> map)
    {
        map.Map(destination => destination.FullName, source => source.Name)
            .Convention(destination => destination.Age, source => source.Age)
            .Convention(destination => destination.Email, source => source.Email);
    }
}

var configuration = new MapperConfiguration();
configuration.AddMap<User, UserDto, UserToUserDtoMap>();
```

## Mapping options

### Explicit member mapping

Use `Map` when source and destination member names differ.

```csharp
configuration.CreateMap<User, UserDto>()
    .Map(destination => destination.FullName, source => source.Name);
```

### Convention mapping

Use `Convention` for direct member-to-member mappings. Convention assignments run before explicit assignments, so explicit mappings can override them.

```csharp
configuration.CreateMap<User, UserDto>()
    .Convention(destination => destination.Age, source => source.Age)
    .Convention(destination => destination.Email, source => source.Email)
    .Map(destination => destination.Email, source => source.PrimaryEmail);
```

### Ignore destination members

Use `Ignore` to skip writing a destination member even if you configured it through `Convention` or `Map`.

```csharp
configuration.CreateMap<User, UserDto>()
    .Convention(destination => destination.Age, source => source.Age)
    .Ignore(destination => destination.Age);
```

### Reverse mapping

Use `Reverseable` to register the reverse direction automatically for simple member mappings.

```csharp
configuration.CreateMap<User, UserDto>()
    .Map(destination => destination.FullName, source => source.Name)
    .Convention(destination => destination.Age, source => source.Age)
    .Reverseable();

var user = mapper.Map<UserDto, User>(new UserDto
{
    FullName = "Grace Hopper",
    Age = 85
});
```

Only simple member access expressions are reversible. A computed expression such as `source => $"{source.FirstName} {source.LastName}"` maps forward only.

## Collection mapping

`IMapper` can map sequences and returns a `List<TDestination>`.

```csharp
var users = new[]
{
    new User { Name = "Ada Lovelace", Age = 36 },
    new User { Name = "Grace Hopper", Age = 85 }
};

List<UserDto> dtos = mapper.Map<User, UserDto>(users);
```

## Runtime behavior

- Passing `null` to either `Map` overload throws `ArgumentNullException`.
- Mapping without a registered configuration throws `InvalidOperationException`.
- Destination types used with `IMapper` must be reference types with a public parameterless constructor.

## Example models

```csharp
public sealed class User
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PrimaryEmail { get; set; }
    public int Age { get; set; }
}

public sealed class UserDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
}
```
