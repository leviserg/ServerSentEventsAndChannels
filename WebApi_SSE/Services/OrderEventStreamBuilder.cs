using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using WebApi_SSE.Models;

namespace WebApi_SSE.Services
{
    public static class OrderEventStreamBuilder
    {
        public static async IAsyncEnumerable<Order> GetOrdersForAllUsers(
            ChannelConnectionBuilder channelBuilder,
            IEnumerable<string> userEmails,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var mergedChannel = Channel.CreateUnbounded<Order>();

            var forwarders = userEmails
                .Select(userEmail => ForwardOrdersForUser(userEmail, channelBuilder, mergedChannel.Writer, cancellationToken))
                .ToArray();

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.WhenAll(forwarders);
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
                finally
                {
                    mergedChannel.Writer.TryComplete();
                }
            });

            await foreach (var order in mergedChannel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return order;
            }
        }

        public static async IAsyncEnumerable<SseItem<Order>> GetBufferedOrdersForUser(
            OrderEventsBuffer buffer,
            ChannelReader<Order> channelReader,
            string userEmail,
            string? lastEventId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(lastEventId))
            {
                var missedEvents = buffer.GetEventsAfter(userEmail, lastEventId);

                foreach (var missedEvent in missedEvents)
                {
                    yield return missedEvent;
                }
            }

            await foreach (var order in channelReader.ReadAllAsync(cancellationToken))
            {
                if (!order.UserEmail.Equals(userEmail, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var sseItem = buffer.Add(order);
                yield return sseItem;
            }
        }

        private static async Task ForwardOrdersForUser(
            string userEmail,
            ChannelConnectionBuilder channelBuilder,
            ChannelWriter<Order> mergedWriter,
            CancellationToken cancellationToken)
        {
            var reader = channelBuilder.GetOrCreateChannel(userEmail).Reader;

            await foreach (var order in reader.ReadAllAsync(cancellationToken))
            {
                if (!order.UserEmail.Equals(userEmail, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                await mergedWriter.WriteAsync(order, cancellationToken);
            }
        }

    }
}
