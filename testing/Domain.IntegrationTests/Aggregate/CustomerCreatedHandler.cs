﻿using Jcg.Domain.Aggregates.DomainEventHandlers;
using Jcg.Domain.Aggregates.DomainEvents;

namespace Domain.IntegrationTests.Aggregate;

internal class CustomerCreatedHandler : DomainEventHandlerBase<Customer>
{
    /// <inheritdoc />
    protected override bool PerformHandling(Customer aggregate,
        IDomainEvent domainEvent)
    {
        if (domainEvent is DomainEvents.CustomerCreated cev)
        {
            aggregate.Id = cev.AggregateId;
            aggregate.Name = cev.Name;

            return true;
        }

        return false;
    }
}