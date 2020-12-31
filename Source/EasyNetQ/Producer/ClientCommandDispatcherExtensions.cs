using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ.Internals;
using RabbitMQ.Client;

namespace EasyNetQ.Producer
{
    internal static class ClientCommandDispatcherExtensions
    {
        public static Task InvokeAsync<TCommand>(
            this IClientCommandDispatcher dispatcher,
            TCommand command,
            ChannelDispatchOptions channelOptions,
            CancellationToken cancellationToken = default
        ) where TCommand : IClientCommand<NoContentStruct>
        {
            return dispatcher.InvokeAsync<NoContentStruct, TCommand>(command, channelOptions, cancellationToken);
        }

        public static Task<TResult> InvokeAsync<TResult>(
            this IClientCommandDispatcher dispatcher,
            Func<IModel, TResult> channelAction,
            ChannelDispatchOptions channelOptions,
            CancellationToken cancellationToken = default
        )
        {
            return dispatcher.InvokeAsync<TResult, FuncBasedClientCommand<TResult>>(
                new FuncBasedClientCommand<TResult>(channelAction), channelOptions, cancellationToken
            );
        }

        public static Task<TResult> InvokeAsync<TResult>(
            this IClientCommandDispatcher dispatcher,
            Func<IModel, TResult> channelAction,
            CancellationToken cancellationToken = default
        )
        {
            return dispatcher.InvokeAsync(channelAction, ChannelDispatchOptions.Default, cancellationToken);
        }

        public static Task InvokeAsync(
            this IClientCommandDispatcher dispatcher,
            Action<IModel> channelAction,
            ChannelDispatchOptions channelOptions,
            CancellationToken cancellationToken = default
        )
        {
            return dispatcher.InvokeAsync<NoContentStruct, ActionBasedClientCommand>(
                new ActionBasedClientCommand(channelAction), channelOptions, cancellationToken
            );
        }

        public static Task InvokeAsync(
            this IClientCommandDispatcher dispatcher,
            Action<IModel> channelAction,
            CancellationToken cancellationToken = default
        )
        {
            return dispatcher.InvokeAsync(channelAction, ChannelDispatchOptions.Default, cancellationToken);
        }

        private readonly struct ActionBasedClientCommand : IClientCommand<NoContentStruct>
        {
            private readonly Action<IModel> action;

            public ActionBasedClientCommand(Action<IModel> action)
            {
                this.action = action;
            }

            public NoContentStruct Invoke(IModel model)
            {
                action(model);
                return default;
            }
        }

        private readonly struct FuncBasedClientCommand<TResult> : IClientCommand<TResult>
        {
            private readonly Func<IModel, TResult> func;

            public FuncBasedClientCommand(Func<IModel, TResult> func)
            {
                this.func = func;
            }

            public TResult Invoke(IModel model)
            {
                return func(model);
            }
        }
    }
}
