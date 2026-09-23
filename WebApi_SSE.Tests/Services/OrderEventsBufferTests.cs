using WebApi_SSE.Models;
using WebApi_SSE.Services;
using Xunit;

namespace WebApi_SSE.Tests.Services
{
    public class OrderEventsBufferTests
    {
        private const string Email = "u1@example.com";

        [Fact]
        public void Add_AssignsIncrementalIds_PerUser()
        {
            var sut = new OrderEventsBuffer();

            var e1 = sut.Add(CreateOrder(Email, "ORD-1"));
            var e2 = sut.Add(CreateOrder(Email, "ORD-2"));

            Assert.Equal("1", e1.EventId);
            Assert.Equal("2", e2.EventId);
        }

        [Fact]
        public void GetEventsAfter_ReturnsOnlyNewer()
        {
            var sut = new OrderEventsBuffer();

            _ = sut.Add(CreateOrder(Email, "ORD-1"));
            _ = sut.Add(CreateOrder(Email, "ORD-2"));
            _ = sut.Add(CreateOrder(Email, "ORD-3"));

            var items = sut.GetEventsAfter(Email, "1").ToList();

            Assert.Equal(2, items.Count);
            Assert.Equal("2", items[0].EventId);
            Assert.Equal("3", items[1].EventId);
        }

        [Fact]
        public void Buffer_RespectsMaxBufferSize()
        {
            var sut = new OrderEventsBuffer(maxBufferSize: 2);

            _ = sut.Add(CreateOrder(Email, "ORD-1"));
            _ = sut.Add(CreateOrder(Email, "ORD-2"));
            _ = sut.Add(CreateOrder(Email, "ORD-3"));

            var items = sut.GetEventsAfter(Email, "0").ToList();

            Assert.Equal(2, items.Count);
            Assert.Equal("2", items[0].EventId);
            Assert.Equal("3", items[1].EventId);
        }

        [Fact]
        public void GetCurrentEventId_UnknownUser_ReturnsZero()
        {
            var sut = new OrderEventsBuffer();

            var current = sut.GetCurrentEventId("missing@example.com");

            Assert.Equal(0, current);
        }

        private static Order CreateOrder(string email, string externalId) =>
            new(Guid.CreateVersion7(), externalId, email, "User", 10m, DateTime.UtcNow);

    }
}
