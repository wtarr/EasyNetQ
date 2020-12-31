using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ.Internals;
using RabbitMQ.Client;

namespace EasyNetQ.Producer
{
    /// <summary>
    ///     Invokes client commands using single channel
    /// </summary>
    public sealed class SingleChannelClientCommandDispatcher : IClientCommandDispatcher
    {
        private readonly ConcurrentDictionary<ChannelDispatchOptions, IPersistentChannel> channelPerOptions;
        private readonly Func<ChannelDispatchOptions, IPersistentChannel> createChannelFactory;

        /// <summary>
        /// Creates a dispatcher
        /// </summary>
        /// <param name="channelFactory">The channel factory</param>
        public SingleChannelClientCommandDispatcher(IPersistentChannelFactory channelFactory)
        {
            Preconditions.CheckNotNull(channelFactory, "channelFactory");

            channelPerOptions = new ConcurrentDictionary<ChannelDispatchOptions, IPersistentChannel>();
            createChannelFactory = o => channelFactory.CreatePersistentChannel(new PersistentChannelOptions(o.PublisherConfirms));
        }

        /// <inheritdoc />
        public Task<TResult> InvokeAsync<TResult, TCommand>(
           TCommand command, ChannelDispatchOptions options, CancellationToken cancellationToken = default
        ) where TCommand : IClientCommand<TResult>
        {
            // TODO createChannelFactory could be called multiple time, fix it
            var channel = channelPerOptions.GetOrAdd(options, createChannelFactory);
            return channel.InvokeChannelActionAsync<TResult, PersistentChannelActionProxy<TResult, TCommand>>(
                new PersistentChannelActionProxy<TResult, TCommand>(command), cancellationToken
            );
        }

        /// <inheritdoc />
        public void Dispose() => channelPerOptions.ClearAndDispose();

        private readonly struct PersistentChannelActionProxy<TResult, TCommand> : IPersistentChannelAction<TResult> where TCommand : IClientCommand<TResult>
        {
            private readonly TCommand command;

            public PersistentChannelActionProxy(in TCommand command)
            {
                this.command = command;
            }

            public TResult Invoke(IModel model)
            {
                return command.Invoke(model);
            }
        }
    }
}
