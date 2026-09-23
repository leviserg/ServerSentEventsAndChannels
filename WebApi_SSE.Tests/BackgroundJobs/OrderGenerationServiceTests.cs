using Microsoft.Extensions.Logging.Abstractions;
using WebApi_SSE.BackgroundJobs;
using WebApi_SSE.Services;
using Xunit;

namespace WebApi_SSE.Tests.BackgroundJobs
{
    public class OrderGenerationServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenCanceled_CompletesExistingUserChannels()
        {
            var channelBuilder = new ChannelConnectionBuilder();

            foreach (var userEmail in UsersMap.Users.Values)
            {
                _ = channelBuilder.GetOrCreateChannel(userEmail);
            }

            var sut = new TestableOrderGenerationService(channelBuilder);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await sut.RunAsync(cts.Token);

            foreach (var userEmail in UsersMap.Users.Values)
            {
                var reader = channelBuilder.GetOrCreateChannel(userEmail).Reader;
                await reader.Completion.WaitAsync(TimeSpan.FromSeconds(1));
                Assert.True(reader.Completion.IsCompleted);
            }
        }

        private sealed class TestableOrderGenerationService : OrderGenerationService
        {
            public TestableOrderGenerationService(ChannelConnectionBuilder channelBuilder)
                : base(channelBuilder, NullLogger<OrderGenerationService>.Instance)
            {
            }

            public Task RunAsync(CancellationToken token) => ExecuteAsync(token);
        }
    }
}
