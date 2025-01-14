﻿using Jcg.Domain.Aggregates.DomainEventHandlers;
using Jcg.Domain.Aggregates.DomainEvents;

namespace Domain.IntegrationTests.Aggregate;

internal class OrderAddedHandler : DomainEventHandlerBase<Customer>
{
    /// <inheritdoc />
    protected override bool PerformHandling(Customer aggregate,
        IDomainEvent domainEvent)
    {
        if (domainEvent is DomainEvents.OrderAdded cev)
        {
            var order = new Order()
            {
                Id = cev.OrderId
            };
            aggregate.Orders.Add(order);

            return true;
        }

        return false;
    }
}