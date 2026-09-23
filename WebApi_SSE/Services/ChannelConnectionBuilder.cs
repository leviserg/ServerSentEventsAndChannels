using System.Collections.Concurrent;
using System.Threading.Channels;
using WebApi_SSE.Models;

namespace WebApi_SSE.Services
{
    public class ChannelConnectionBuilder
    {
        private readonly ConcurrentDictionary<string, Channel<Order>> _channels = new ConcurrentDictionary<string, Channel<Order>>();

        public Channel<Order> GetOrCreateChannel(string userEmail)
        {
            return _channels.GetOrAdd(userEmail, _ => Channel.CreateUnbounded<Order>());
        }

        public ChannelReader<Order>? GetChannelReader(string userEmail)
        {
            return _channels.TryGetValue(userEmail, out var channel) ? channel.Reader : null;
        }

        public bool TryGetChannelWriter(string userEmail, out ChannelWriter<Order> writer)
        {
            if (_channels.TryGetValue(userEmail, out var channel))
            {
                writer = channel.Writer;
                return true;
            }
            writer = null!;
            return false;
        }
    }
}
