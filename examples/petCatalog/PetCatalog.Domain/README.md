﻿# Example application for jcg-domain-core

This sample application consists of a domain project and a test project.

The following are some notes about it.

## Encapsulation of the domain

The sample application is an example of one way to encapsulate the domain layer. 

See the following code for the aggregate in this project:

***IPetCatalog.cs***
```
 public interface IPetCatalog
    {
        CatalogId Id { get; }

        string CatalogName { get; }

        IReadOnlyCollection<IPet> Pets { get; }

        void AddPet(PetId petId, string name);

    }
```

***PetCatalog.cs***
```
  internal class PetCatalog : AggregateRootBase, IPetCatalog
    {
        public PetCatalog(CatalogId id, string name)
        {
            ...
        }
        public CatalogId Id { get; set; }
        public string CatalogName { get; set; } = "";
        #region Pets

        public IReadOnlyCollection<IPet> Pets => this.APets;

        public List<Pet> APets { get; set; } = new();
        #endregion
        public void AddPet(PetId petId, string name)
        {
            ...
        }

        ...
       
    }
```

### The interface is exposed to the other layers
The interface is public, and immutable, forcing the clients to use aggregate methods to operate on the aggregate.

### The implementation is internal and mutable
The implementation (the class above) has **internal access**, so its accessible from the domain project only (you can grant extra access, for example, to the Infrastructure layer)

It also has automatic properties:

```
public string CatalogName { get; set; } = "";
```

So the domain event handlers, and potentially the persistence layer, can operate freely on it. Another way to achieve the same result is by providing methods for this purpose that are members of the implementation but not the interface. 

### Use factories
For the application layer to create the aggregate, you just need to create a factory service, not shown in this project:

For example:

```
public class PetCatalogFactoryService : IPetCatalogFactoryService
{
    public IPetCatalog Create(CatalogId id, string name)
    {
        return new PetCatalog(id, name);
    }
}
```

## Domain events


### Create domain events

***DomainEvents.cs***
```
  public static class DomainEvents
    {
        public record PetCatalogCreated(Guid AggregateId, string Name) : ICreationalDomainEvent;

        public record PetAdded(Guid AggregateId, Guid PetId, string PetName) : INonCreationalDomainEvent;
    }
```

Observe there are two types of domain events: ICreationalDomainEvent and INonCreationalDomainEvent

Both implement the **IDomainEvent** interface, wich has a single AggregateId property.

The difference is:

> Applying a NonCreational domain event to an aggregate with an Id that does not match the event AggregateId value will throw an exception.

The Creational domain event skips this check.

### Add a domain event handler

***PetAddedHandler.cs***
```
internal class PetAddedHandler : DomainEventHandlerBase<PetCatalog>
{
    protected override bool PerformHandling(PetCatalog aggregate, IDomainEvent domainEvent)
    {
        if (domainEvent is DomainEvents.PetAdded cev)
        {
           // ... crete a pet and add it to the aggregate
                
           // You must return true so the library knows the event was handled.
            return true;
        }

        // return false if the domain event type did not match the type intended for this handler,
        // so the library can try the next handler in the pipeline
        return false;
    }
}
```

> If you apply a domain event for which a handler does not exist, an exception will be thrown.

### Find the domain events and build the handler pipeline

See this method inside the aggregate.

***PetCatalog.cs***
```
protected override void When(IDomainEvent domainEvent)
        {
            var pipeline = DomainEventHandlingPipelineProvider
                .GetInstance(Assembly.GetExecutingAssembly())
                .GetPipeline<PetCatalog>();

            pipeline.Handle(this, domainEvent);
        }
```

the following code
```
DomainEventHandlingPipelineProvider
                .GetInstance(Assembly.GetExecutingAssembly())
                .GetPipeline<PetCatalog>();
```

**DomainEventHandlingPipelineProvider** is a singleton. 

The first time you call it (during the application lifetime), it scans the assembly for all domain event handlers for all existing aggregates,
and it uses these handlers to build a pipeline (Chain of Responsibility GOF Pattern).

The following code gets the pipeline from the provider.
```
.GetPipeline<PetCatalog>();
```
The following code handles the domain event.
```
pipeline.Handle(this, domainEvent);
```

> Because the DomainEventHandligPipelineProvider is a singleton, the pipeline is assembled only once during the application lifetime.

### Apply a domain event in the aggregate
To apply a domain event, create an instance of the event and use the Apply method as in the following example.

***PetCatalog.cs***
```
 internal class PetCatalog : AggregateRootBase, IPetCatalog
    {
       ...

        public void AddPet(PetId petId, string name)
        {
            var ev = new DomainEvents.PetAdded(Id.Id, petId.Id, name);

            Apply(ev);
        }


       ...

       
    }
```

**Avoid changing the aggregate state other than by applying domain events.

## Invariant Rule Handlers

> Handlers that assert that the aggregate fulfills all its invariants. Each handler implements one single rule. 

> You can have zero to many handlers.

**Invariant Rule handlers are ran each time you apply a domain event**

### Add an Invariant rule handler

***PetNameIsRequiredRuleHandler.cs***
```
internal class PetNameIsRequiredRuleHandler : InvariantRuleHandlerBase<PetCatalog>
{
    protected override void AssertEntityStateIsValid(PetCatalog aggregate)
    {
        if (aggregate.APets.Any(p => string.IsNullOrWhiteSpace(p.Name)))
        {
            throw new PetNameIsBlankException();
        }
    }
}
```

### Find the Invariant Rule Handlers and build the handler pipeline

See this method inside the aggregate.

***PetCatalog.cs***
```
 protected override void AssertEntityStateIsValid()
        {
            var pipeline = InvariantRuleHandlingPipelineProvider
                .GetInstance(Assembly.GetExecutingAssembly())
                .GetPipeline<PetCatalog>();

            pipeline.Handle(this);
        }
```

Acts in similar fashion than the DomainEvent Handlers, but for InvariantRuleHandlers


> Because the DomainEventHandligPipelineProvider is a singleton, the pipeline is assembled only once during the application lifetime.

## Other important AggregateRootBase methods

### Optimistic concurrency version

***AggregateRootBase.cs***
```
public long Version { get; protected set; }
```

This value increments each time a domain event is applied. You should set it when loading an aggregate from the database (that is why it has protected access).

It has a protected setter, so your aggregate can set the version to match the stored value when loading it from a database.

One use case for the Version property is optimistic concurrency.

### Changes

Use this method to get the Changes (all the domain events applied since construction)

***AggregateRootBase.cs***
```
public IDomainEvent[] Changes => _changes.ToArray();
```

A typical use for this method is to publish domain events when saving the aggregate to the database.

Use the following method to reset the Changes list to an empty collection.

***AggregateRootBase.cs***
```
public void ResetChanges();
```

A typical use case for this method is to reset the changes when the domain events are published or when loading the aggregate from the database.


