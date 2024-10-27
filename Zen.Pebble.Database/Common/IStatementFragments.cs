namespace Zen.Pebble.Database.Common
{
    public interface IStatementFragments
    {
        string ParametrizedValue { get; }
        string InlineValue { get; }
        string Column { get; }

        StatementMasks.MarkerSet Markers { get; }
        StatementMasks.ValueSet Values { get; }

        string NodeAdd { get; }
        string NodeAnd { get; }
        string NodeAndAlso { get; }
        string NodeDivide { get; }
        string NodeEqual { get; }
        string NodeExclusiveOr { get; }
        string NodeGreaterThan { get; }
        string NodeGreaterThanOrEqual { get; }
        string NodeLessThan { get; }
        string NodeLessThanOrEqual { get; }
        string NodeModulo { get; }
        string NodeMultiply { get; }
        string NodeNegate { get; }
        string NodeNot { get; }
        string NodeNotEqual { get; }
        string NodeOr { get; }
        string NodeOrElse { get; }
        string NodeSubtract { get; }
        string NodeConvert { get; }
    }
}