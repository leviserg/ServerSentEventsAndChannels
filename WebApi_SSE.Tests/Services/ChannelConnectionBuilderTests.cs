using WebApi_SSE.Services;
using Xunit;

namespace WebApi_SSE.Tests.Services
{
    public class ChannelConnectionBuilderTests
    {
        [Fact]
        public void GetOrCreateChannel_SameUser_ReturnsSameInstance()
        {
            var sut = new ChannelConnectionBuilder();

            var c1 = sut.GetOrCreateChannel("u1@example.com");
            var c2 = sut.GetOrCreateChannel("u1@example.com");

            Assert.Same(c1, c2);
        }

        [Fact]
        public void GetChannelReader_UnknownUser_ReturnsNull()
        {
            var sut = new ChannelConnectionBuilder();

            var reader = sut.GetChannelReader("missing@example.com");

            Assert.Null(reader);
        }

        [Fact]
        public void TryGetChannelWriter_ExistingUser_ReturnsTrueAndWriter()
        {
            var sut = new ChannelConnectionBuilder();
            _ = sut.GetOrCreateChannel("u1@example.com");

            var ok = sut.TryGetChannelWriter("u1@example.com", out var writer);

            Assert.True(ok);
            Assert.NotNull(writer);
        }

        [Fact]
        public void TryGetChannelWriter_UnknownUser_ReturnsFalse()
        {
            var sut = new ChannelConnectionBuilder();

            var ok = sut.TryGetChannelWriter("missing@example.com", out var writer);

            Assert.False(ok);
            Assert.Null(writer);
        }
    }
}
