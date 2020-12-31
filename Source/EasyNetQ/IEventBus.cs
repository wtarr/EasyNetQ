using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EasyNetQ.Logging;

namespace EasyNetQ
{
    /// <inheritdoc />
    public delegate void Handler<TEvent>(in TEvent @event);

    /// <summary>
    ///     An internal pub-sub bus to distribute events within EasyNetQ
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        ///     Publishes the event
        /// </summary>
        /// <param name="event">The event</param>
        /// <typeparam name="TEvent">The event type</typeparam>
        void Publish<TEvent>(in TEvent @event);

        /// <summary>
        ///     Subscribes to the event type
        /// </summary>
        /// <param name="handler">The event handler</param>
        /// <typeparam name="TEvent">The event type</typeparam>
        /// <returns>Unsubscription disposable</returns>
        IDisposable Subscribe<TEvent>(Handler<TEvent> handler);
    }

    /// <inheritdoc />
    public sealed class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, object> subscriptions = new ConcurrentDictionary<Type, object>();

        /// <inheritdoc />
        public void Publish<TEvent>(in TEvent @event)
        {
            if (!subscriptions.TryGetValue(typeof(TEvent), out var handlers))
                return;

            ((Handlers<TEvent>)handlers).Handle(@event);
        }

        /// <inheritdoc />
        public IDisposable Subscribe<TEvent>(Handler<TEvent> handler)
        {
            var handlers = (Handlers<TEvent>)subscriptions.GetOrAdd(typeof(TEvent), _ => new Handlers<TEvent>());
            handlers.Add(handler);
            return new Subscription<TEvent>(handlers, handler);
        }

        private sealed class Handlers<TEvent>
        {
            private readonly ILog log = LogProvider.For<Handlers<TEvent>>();
            private readonly object mutex = new object();
            private volatile List<Handler<TEvent>> handlers = new List<Handler<TEvent>>();

            public void Add(Handler<TEvent> handler)
            {
                lock (mutex)
                {
                    var newHandlers = new List<Handler<TEvent>>(handlers);
                    newHandlers.Add(handler);
                    handlers = newHandlers;
                }
            }

            public void Remove(Handler<TEvent> handler)
            {
                lock (mutex)
                {
                    var newHandlers = new List<Handler<TEvent>>(handlers);
                    newHandlers.Remove(handler);
                    handlers = newHandlers;
                }
            }

            public void Handle(in TEvent @event)
            {
                // ReSharper disable once InconsistentlySynchronizedField
                foreach (var handler in handlers)
                    try
                    {
                        handler(in @event);
                    }
                    catch (Exception exception)
                    {
                        log.ErrorException("Failed to handle {event}", exception, @event);
                    }
            }
        }

        private sealed class Subscription<TEvent> : IDisposable
        {
            private readonly Handlers<TEvent> handlers;
            private readonly Handler<TEvent> handler;

            public Subscription(Handlers<TEvent> handlers, Handler<TEvent> handler)
            {
                this.handlers = handlers;
                this.handler = handler;
            }

            public void Dispose() => handlers.Remove(handler);
        }
    }
}
