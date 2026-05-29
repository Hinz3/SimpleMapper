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
        var assignment = BuildAssignment(destinationProperty, sourceProperty);
        _definition.AddExplicit(destinationName, assignment, sourceName);
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
        var assignment = BuildAssignment(destinationProperty, sourceProperty);
        _definition.AddConvention(destinationName, sourceName, assignment);
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

    private static Expression<Action<TSource, TDestination>> BuildAssignment(
        Expression<Func<TDestination, object?>> destinationProperty,
        Expression<Func<TSource, object?>> sourceProperty)
    {
        var destinationBody = StripConvert(destinationProperty.Body);
        var sourceBody = StripConvert(sourceProperty.Body);

        if (destinationBody is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must point to a member.", nameof(destinationProperty));
        }

        var sourceParameter = Expression.Parameter(typeof(TSource), "source");
        var destinationParameter = Expression.Parameter(typeof(TDestination), "destination");
        var reboundSource = ReplaceParameter(sourceBody, sourceProperty.Parameters[0], sourceParameter);
        var destinationMember = Expression.MakeMemberAccess(destinationParameter, memberExpression.Member);
        var convertedSource = reboundSource.Type == destinationMember.Type
            ? reboundSource
            : Expression.Convert(reboundSource, destinationMember.Type);
        var assignment = Expression.Assign(destinationMember, convertedSource);
        return Expression.Lambda<Action<TSource, TDestination>>(assignment, sourceParameter, destinationParameter);
    }

    private static Expression StripConvert(Expression expression)
    {
        return expression is UnaryExpression unary && unary.NodeType == ExpressionType.Convert
            ? unary.Operand
            : expression;
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression original, ParameterExpression replacement)
    {
        return new ParameterReplaceVisitor(original, replacement).Visit(expression)!;
    }

    private sealed class ParameterReplaceVisitor(ParameterExpression original, ParameterExpression replacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == original ? replacement : base.VisitParameter(node);
        }
    }
}
