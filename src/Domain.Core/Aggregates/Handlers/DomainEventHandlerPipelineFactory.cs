﻿using System.Reflection;

namespace Domain.Core.Aggregates.Handlers
{
    internal class DomainEventHandlerPipelineFactory
    {
        private DomainEventHandlerPipelineFactory(Assembly assemblyToScan)
        {
            throw new NotImplementedException();
        }

        public static DomainEventHandlerPipelineFactory GetInstance(
            Assembly assemblyToScan)
        {
            var lockObject = new object();

            lock (lockObject)
            {
                if (_instance is null)
                {
                    _instance = new(assemblyToScan);
                }

                return _instance;
            }
        }

        public static IDomainEventHandlerPipeline<TAggregate> GetPipeline<
            TAggregate>()
            where TAggregate : AggregateRootBase
        {
            throw new NotImplementedException();
        }

        private static DomainEventHandlerPipelineFactory? _instance = null;
    }
}