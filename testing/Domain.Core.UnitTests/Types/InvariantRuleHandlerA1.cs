﻿using Jcg.Domain.Aggregates.InvarianRuleHandlers;

namespace Jcg.Domain.UnitTests.Types
{
    public class InvariantRuleHandlerA1 : InvariantRuleHandlerBase<AggregateA>
    {
        /// <inheritdoc />
        protected override void AssertEntityStateIsValid(AggregateA aggregate)
        {
            throw new NotImplementedException();
        }
    }
}