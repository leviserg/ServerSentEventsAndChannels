using WebApi_SSE.Models;
using WebApi_SSE.Services;
using Xunit;

namespace WebApi_SSE.Tests.Services
{
    public class OrderEventStreamBuilderTests
    {
        [Fact]
        public async Task GetOrdersForAllUsers_MergesEventsFromAllChannels()
        {
            var builder = new ChannelConnectionBuilder();
            const string u1 = "u1@example.com";
            const string u2 = "u2@example.com";

            var c1 = builder.GetOrCreateChannel(u1);
            var c2 = builder.GetOrCreateChannel(u2);

            var o1 = CreateOrder(u1, "ORD-1");
            var o2 = CreateOrder(u2, "ORD-2");

            await c1.Writer.WriteAsync(o1);
            await c2.Writer.WriteAsync(o2);
            c1.Writer.TryComplete();
            c2.Writer.TryComplete();

            var received = new List<Order>();
            await foreach (var order in OrderEventStreamBuilder.GetOrdersForAllUsers(builder, [u1, u2], CancellationToken.None))
            {
                received.Add(order);
            }

            Assert.Equal(2, received.Count);
            Assert.Contains(o1, received);
            Assert.Contains(o2, received);
        }

        [Fact]
        public async Task GetBufferedOrdersForUser_ReplaysMissed_ThenStreamsLive()
        {
            var buffer = new OrderEventsBuffer();
            var channels = new ChannelConnectionBuilder();
            const string user = "u1@example.com";

            var channel = channels.GetOrCreateChannel(user);

            _ = buffer.Add(CreateOrder(user, "ORD-1")); // id=1
            _ = buffer.Add(CreateOrder(user, "ORD-2")); // id=2
            _ = buffer.Add(CreateOrder(user, "ORD-3")); // id=3

            await channel.Writer.WriteAsync(CreateOrder(user, "ORD-4"));
            channel.Writer.TryComplete();

            var eventIds = new List<string>();
            await foreach (var item in OrderEventStreamBuilder.GetBufferedOrdersForUser(
                buffer,
                channel.Reader,
                user,
                lastEventId: "1",
                CancellationToken.None))
            {
                eventIds.Add(item.EventId!);
            }

            Assert.Equal(3, eventIds.Count);
            Assert.Equal(new[] { "2", "3", "4" }, eventIds);
        }

        private static Order CreateOrder(string userEmail, string externalId) =>
            new(Guid.CreateVersion7(), externalId, userEmail, "Customer", 123.45m, DateTime.UtcNow);

    }
}
