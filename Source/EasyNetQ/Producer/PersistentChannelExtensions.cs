using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ.Internals;
using RabbitMQ.Client;

namespace EasyNetQ.Producer
{
    internal static class PersistentChannelExtensions
    {
        public static Task<TResult> InvokeChannelActionAsync<TResult>(
            this IPersistentChannel source, Func<IModel, TResult> channelAction, CancellationToken cancellationToken = default
        )
        {
            return source.InvokeChannelActionAsync<TResult, FuncBasedPersistentChannelAction<TResult>>(
                new FuncBasedPersistentChannelAction<TResult>(channelAction), cancellationToken
            );
        }

        public static void InvokeChannelAction(
            this IPersistentChannel source, Action<IModel> channelAction, CancellationToken cancellationToken = default
        )
        {
            source.InvokeChannelActionAsync(channelAction, cancellationToken)
                .GetAwaiter()
                .GetResult();
        }


        public static Task InvokeChannelActionAsync(
            this IPersistentChannel source, Action<IModel> channelAction, CancellationToken cancellationToken = default
        )
        {
            return source.InvokeChannelActionAsync<NoContentStruct>(model =>
            {
                channelAction(model);
                return default;
            }, cancellationToken);
        }


        private readonly struct FuncBasedPersistentChannelAction<TResult> : IPersistentChannelAction<TResult>
        {
            private readonly Func<IModel, TResult> func;

            public FuncBasedPersistentChannelAction(Func<IModel, TResult> func)
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
