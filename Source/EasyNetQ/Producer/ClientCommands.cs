using EasyNetQ.Internals;
using EasyNetQ.Topology;
using RabbitMQ.Client;

namespace EasyNetQ.Producer
{
    internal readonly struct PublishWithoutConfirmsCommand : IClientCommand<NoContentStruct>
    {
        private readonly Exchange exchange;
        private readonly string routingKey;
        private readonly bool mandatory;
        private readonly ProducedMessage message;

        public PublishWithoutConfirmsCommand(in Exchange exchange, string routingKey, bool mandatory, in ProducedMessage message)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.mandatory = mandatory;
            this.message = message;
        }

        public NoContentStruct Invoke(IModel model)
        {
            var properties = model.CreateBasicProperties();
            message.Properties.CopyTo(properties);
            model.BasicPublish(exchange.Name, routingKey, mandatory, properties, message.Body);
            return default;
        }
    }
}
