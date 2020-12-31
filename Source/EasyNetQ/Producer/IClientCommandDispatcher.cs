using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace EasyNetQ.Producer
{
    /// <summary>
    /// A dispatch options of channel
    /// </summary>
    public readonly struct ChannelDispatchOptions
    {
        /// <summary>
        ///     Default options: mostly for topology operations
        /// </summary>
        public static readonly ChannelDispatchOptions Default = new ChannelDispatchOptions("Default");

        /// <summary>
        ///     Options for publish without confirms
        /// </summary>
        public static readonly ChannelDispatchOptions Publish = new ChannelDispatchOptions("Publish");

        /// <summary>
        ///     Options for publish confirms
        /// </summary>
        public static readonly ChannelDispatchOptions PublishWithConfirms = new ChannelDispatchOptions("PublishWithConfirms", true);

        /// <summary>
        /// Creates ChannelDispatchOptions
        /// </summary>
        /// <param name="name">The channel name</param>
        /// <param name="publisherConfirms">True if publisher confirms are enabled</param>
        public ChannelDispatchOptions(string name, bool publisherConfirms = false)
        {
            Name = name;
            PublisherConfirms = publisherConfirms;
        }

        /// <summary>
        ///     A name associated with channel
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     True if publisher confirms are enabled
        /// </summary>
        public bool PublisherConfirms { get; }
    }

    public interface IClientCommand<out TResult>
    {
        TResult Invoke(IModel model);
    }

    /// <summary>
    ///     Responsible for invoking client commands.
    /// </summary>
    public interface IClientCommandDispatcher : IDisposable
    {
        /// <summary>
        /// Invokes a command on top of model
        /// </summary>
        /// <param name="command"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        Task<TResult> InvokeAsync<TResult, TCommand>(
            TCommand command, ChannelDispatchOptions options, CancellationToken cancellationToken = default
        ) where TCommand : IClientCommand<TResult>;
    }
}
