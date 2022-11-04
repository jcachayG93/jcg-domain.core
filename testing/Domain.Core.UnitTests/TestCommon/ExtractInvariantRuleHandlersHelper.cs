﻿using Domain.Core.Aggregates;
using Domain.Core.Aggregates.InvarianRuleHandlers;

namespace Domain.Core.UnitTests.TestCommon;

public static class ExtractInvariantRuleHandlersHelper
{
    /// <summary>
    ///     Extracts all the handlers from the pipeline
    /// </summary>
    public static IEnumerable<InvariantRuleHandlerBase<TAggregate>>
        ExtractHandlers<TAggregate>(
            InvariantRuleHandlerBase<TAggregate> pipeline)
        where TAggregate : AggregateRootBase
    {
        var result = new List<InvariantRuleHandlerBase<TAggregate>>();

        var current = pipeline;

        while (current.NextHandler != null)
        {
            result.Add(current);
            current = current.NextHandler;
        }

        result.Add(current);

        return result;
    }
}