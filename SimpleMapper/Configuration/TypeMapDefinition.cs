using System.Linq.Expressions;

namespace SimpleMapper.Configuration;

internal sealed class TypeMapDefinition<TSource, TDestination>
    where TSource : class
    where TDestination : class
{
    private readonly Dictionary<string, AssignmentRegistration<TSource, TDestination>> _conventions = new(StringComparer.Ordinal);
    private readonly List<AssignmentRegistration<TSource, TDestination>> _assignments = [];
    private readonly HashSet<string> _ignored = new(StringComparer.Ordinal);
    private readonly List<(string SourceName, string DestinationName, Action<TSource, TDestination> Assignment)> _reversible = [];
    private readonly List<(string SourceName, string DestinationName)> _conventionPairs = [];
    private readonly object _compiledMapLock = new();
    private Action<TSource, TDestination>? _compiledMap;

    public IReadOnlySet<string> Ignored => _ignored;
    public IReadOnlyList<(string SourceName, string DestinationName, Action<TSource, TDestination> Assignment)> Reversible => _reversible;
    public IReadOnlyList<(string SourceName, string DestinationName)> ConventionPairs => _conventionPairs;

    public void AddConvention(string destinationName, string sourceName, Expression<Action<TSource, TDestination>> assignment)
    {
        _conventions[destinationName] = new AssignmentRegistration<TSource, TDestination>(destinationName, assignment);
        _conventionPairs.Add((sourceName, destinationName));
        _compiledMap = null;
    }

    public void Ignore(string destinationName)
    {
        _ignored.Add(destinationName);
        _compiledMap = null;
    }

    public void AddExplicit(
        string destinationName,
        Expression<Action<TSource, TDestination>> assignment,
        string? sourceName)
    {
        _assignments.Add(new AssignmentRegistration<TSource, TDestination>(destinationName, assignment));
        _compiledMap = null;

        if (sourceName is not null)
        {
            _reversible.Add((sourceName, destinationName, assignment.Compile()));
        }
    }

    public Action<TSource, TDestination> GetCompiledMap()
    {
        if (_compiledMap is not null)
        {
            return _compiledMap;
        }

        lock (_compiledMapLock)
        {
            _compiledMap ??= BuildCompiledMap();
            return _compiledMap;
        }
    }

    private Action<TSource, TDestination> BuildCompiledMap()
    {
        var sourceParameter = Expression.Parameter(typeof(TSource), "source");
        var destinationParameter = Expression.Parameter(typeof(TDestination), "destination");
        var operations = new List<Expression>(_conventions.Count + _assignments.Count);

        foreach (var convention in _conventions.Values)
        {
            if (!_ignored.Contains(convention.DestinationName))
            {
                operations.Add(RebindAssignment(convention.Assignment, sourceParameter, destinationParameter));
            }
        }

        foreach (var assignment in _assignments)
        {
            if (!_ignored.Contains(assignment.DestinationName))
            {
                operations.Add(RebindAssignment(assignment.Assignment, sourceParameter, destinationParameter));
            }
        }

        if (operations.Count == 0)
        {
            return static (_, _) => { };
        }

        return Expression.Lambda<Action<TSource, TDestination>>(
            Expression.Block(operations),
            sourceParameter,
            destinationParameter).Compile();
    }

    private static Expression RebindAssignment(
        Expression<Action<TSource, TDestination>> assignment,
        ParameterExpression sourceParameter,
        ParameterExpression destinationParameter)
    {
        return new MultiParameterReplaceVisitor(assignment.Parameters[0], sourceParameter, assignment.Parameters[1], destinationParameter)
            .Visit(assignment.Body)!;
    }

    private sealed class MultiParameterReplaceVisitor(
        ParameterExpression sourceOriginal,
        ParameterExpression sourceReplacement,
        ParameterExpression destinationOriginal,
        ParameterExpression destinationReplacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == sourceOriginal)
            {
                return sourceReplacement;
            }

            if (node == destinationOriginal)
            {
                return destinationReplacement;
            }

            return base.VisitParameter(node);
        }
    }
}

internal sealed record AssignmentRegistration<TSource, TDestination>(
    string DestinationName,
    Expression<Action<TSource, TDestination>> Assignment)
    where TSource : class
    where TDestination : class;
