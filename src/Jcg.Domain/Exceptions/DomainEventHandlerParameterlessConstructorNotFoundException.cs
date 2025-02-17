﻿namespace Jcg.Domain.Exceptions;

public class DomainEventHandlerParameterlessConstructorNotFoundException
    : DomainCoreException
{
    public DomainEventHandlerParameterlessConstructorNotFoundException(
        string handlerTypeName) : base(
        $"Parameter-less constructor not found for handler: {handlerTypeName}")
    {
    }
}