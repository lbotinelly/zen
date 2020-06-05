using Zen.Pebble.Database.Common;

namespace Zen.Pebble.Database.Renders.Relational
{
    public abstract class RelationalStatementFragments : IStatementFragments
    {
        public virtual string ParametrizedValue => "@p{0}";
        public virtual string InlineValue => "{0}";
        public virtual string Column => "{0}";
        public virtual StatementMasks.MarkerSet Markers => new StatementMasks.MarkerSet();
        public virtual StatementMasks.ValueSet Values => new StatementMasks.ValueSet();

        public virtual string NodeAdd => "{0} + {1}";
        public virtual string NodeAnd => "{0} & {1}";
        public virtual string NodeAndAlso => "{0} AND {1}";
        public virtual string NodeDivide => "{0} / {1}";
        public virtual string NodeEqual => "{0} = {1}";
        public virtual string NodeExclusiveOr => "{0} ^ {1}";
        public virtual string NodeGreaterThan => "{0} > {1}";
        public virtual string NodeGreaterThanOrEqual => "{0} >= {1}";
        public virtual string NodeLessThan => "{0} < {1}";
        public virtual string NodeLessThanOrEqual => "{0} <= {1}";
        public virtual string NodeModulo => "{0} % {1}";
        public virtual string NodeMultiply => "{0} * {1}";
        public virtual string NodeNegate => "{0} - {1}";
        public virtual string NodeNot => "{0} NOT {1}";
        public virtual string NodeNotEqual => "{0} <> {1}";
        public virtual string NodeOr => "{0} | {1}";
        public virtual string NodeOrElse => "{0} OR {1}";
        public virtual string NodeSubtract => "{0} - {1}";
        public virtual string NodeConvert => "{0}  {1}";
    }
}