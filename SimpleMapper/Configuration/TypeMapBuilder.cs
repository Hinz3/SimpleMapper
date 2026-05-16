using System.Linq.Expressions;

namespace SimpleMapper.Configuration;

/// <summary>
/// Provides a fluent API for configuring how a source type maps to a destination type.
/// </summary>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
public sealed class TypeMapBuilder<TSource, TDestination>
    where TSource : class
    where TDestination : class
{
    private readonly MapperConfiguration _configuration;
    private readonly TypeMapDefinition<TSource, TDestination> _definition;

    internal TypeMapBuilder(MapperConfiguration configuration, TypeMapDefinition<TSource, TDestination> definition)
    {
        _configuration = configuration;
        _definition = definition;
    }

    /// <summary>
    /// Registers an explicit assignment for a destination member.
    /// </summary>
    /// <param name="destinationProperty">The destination member to set.</param>
    /// <param name="sourceProperty">
    /// The source expression to read from. Simple member access is reversible; computed expressions are one-way.
    /// </param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="destinationProperty"/> does not target a member.</exception>
    public TypeMapBuilder<TSource, TDestination> Map(
        Expression<Func<TDestination, object?>> destinationProperty,
        Expression<Func<TSource, object?>> sourceProperty)
    {
        var destinationName = GetMemberName(destinationProperty);
        var sourceName = TryGetMemberName(sourceProperty);
        var accessor = sourceProperty.Compile();
        var setter = CompileObjectSetter(destinationProperty);
        _definition.AddExplicit(destinationName, accessor, setter, sourceName);
        return this;
    }

    /// <summary>
    /// Registers a conventional member mapping that can be applied before explicit assignments.
    /// </summary>
    /// <param name="destinationProperty">The destination member to set.</param>
    /// <param name="sourceProperty">The source member to read from.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destinationProperty"/> or <paramref name="sourceProperty"/> does not target a member.
    /// </exception>
    public TypeMapBuilder<TSource, TDestination> Convention(
        Expression<Func<TDestination, object?>> destinationProperty,
        Expression<Func<TSource, object?>> sourceProperty)
    {
        var destinationName = GetMemberName(destinationProperty);
        var sourceName = GetMemberName(sourceProperty);
        _definition.AddConvention(destinationName, sourceName, sourceProperty.Compile(), CompileObjectSetter(destinationProperty));
        return this;
    }

    /// <summary>
    /// Excludes a destination member from both conventional and explicit assignments.
    /// </summary>
    /// <param name="destinationProperty">The destination member to ignore.</param>
    /// <returns>The current builder instance.</returns>
    public TypeMapBuilder<TSource, TDestination> Ignore(Expression<Func<TDestination, object?>> destinationProperty)
    {
        _definition.Ignore(GetMemberName(destinationProperty));
        return this;
    }

    /// <summary>
    /// Registers a reverse mapping for simple member-to-member explicit mappings and convention mappings.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Explicit mappings that use computed expressions are not reversible because they do not point to a concrete source member.
    /// </remarks>
    public TypeMapBuilder<TSource, TDestination> Reverseable()
    {
        _configuration.EnableReverseFor(_definition);
        return this;
    }

    private static string GetMemberName<TModel>(Expression<Func<TModel, object?>> expression)
    {
        return TryGetMemberName(expression)
            ?? throw new ArgumentException("Expression must point to a member.", nameof(expression));
    }

    private static string? TryGetMemberName<TModel>(Expression<Func<TModel, object?>> expression)
    {
        var body = expression.Body;
        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            body = unary.Operand;
        }

        return body is MemberExpression memberExpression ? memberExpression.Member.Name : null;
    }

    private static Action<TDestination, object?> CompileObjectSetter(Expression<Func<TDestination, object?>> destinationProperty)
    {
        var body = destinationProperty.Body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert
            ? unary.Operand
            : destinationProperty.Body;

        if (body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must point to a member.", nameof(destinationProperty));
        }

        var destinationParameter = Expression.Parameter(typeof(TDestination), "destination");
        var valueParameter = Expression.Parameter(typeof(object), "value");
        var convertedValue = Expression.Convert(valueParameter, memberExpression.Type);
        var assignment = Expression.Assign(Expression.MakeMemberAccess(destinationParameter, memberExpression.Member), convertedValue);
        return Expression.Lambda<Action<TDestination, object?>>(assignment, destinationParameter, valueParameter).Compile();
    }
}
