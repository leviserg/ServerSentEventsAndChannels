using System.Collections.Concurrent;
using System.Net.ServerSentEvents;
using WebApi_SSE.Models;

namespace WebApi_SSE.Services 
{
    // Purpose : make possibility to replay missed events by client
    public class OrderEventsBuffer(int maxBufferSize = 100)
    {
        private readonly ConcurrentDictionary<string, UserEventBuffer> _userBuffers = new();

        private class UserEventBuffer(int maxBufferSize)
        {
            private readonly ConcurrentQueue<SseItem<Order>> _buffer = new();
            private long _nextEventId = 1;

            public SseItem<Order> Add(Order order)
            {
                var eventId = Interlocked.Increment(ref _nextEventId) - 1;
                var sseItem = new SseItem<Order>(order)
                {
                    EventId = eventId.ToString()
                };

                _buffer.Enqueue(sseItem);

                while (_buffer.Count > maxBufferSize)
                {
                    _buffer.TryDequeue(out _);
                }

                return sseItem;
            }

            public IEnumerable<SseItem<Order>> GetEventsAfter(string? lastEventId)
            {
                if (string.IsNullOrEmpty(lastEventId))
                {
                    return [];
                }

                if (!long.TryParse(lastEventId, out var lastId))
                {
                    return [];
                }

                return _buffer
                    .Where(item => long.TryParse(item.EventId, out var itemId) && itemId > lastId)
                    .OrderBy(item => long.Parse(item.EventId!));
            }

            public long GetCurrentEventId() => _nextEventId - 1;
        }

        private UserEventBuffer GetOrCreateUserBuffer(string userEmail)
        {
            return _userBuffers.GetOrAdd(userEmail, _ => new UserEventBuffer(maxBufferSize));
        }

        public SseItem<Order> Add(Order order)
        {
            var userBuffer = GetOrCreateUserBuffer(order.UserEmail);
            return userBuffer.Add(order);
        }

        public IEnumerable<SseItem<Order>> GetEventsAfter(string userEmail, string? lastEventId)
        {
            if (!_userBuffers.TryGetValue(userEmail, out var userBuffer))
            {
                return [];
            }

            return userBuffer.GetEventsAfter(lastEventId);
        }

        public long GetCurrentEventId(string userId)
        {
            if (!_userBuffers.TryGetValue(userId, out var userBuffer))
            {
                return 0;
            }

            return userBuffer.GetCurrentEventId();
        }
    }
}
