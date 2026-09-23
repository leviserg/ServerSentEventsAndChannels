using WebApi_SSE.Models;
using WebApi_SSE.Services;

namespace WebApi_SSE.BackgroundJobs
{
    public class OrderGenerationService(
        ChannelConnectionBuilder channelBuilder,
        ILogger<OrderGenerationService> logger
        ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var orderCount = 1;

            var random = new Random();

            var users = UsersMap.Users;

            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    var user = users.ElementAt(random.Next(users.Count));

                    var userEmail = user.Value;

                    var orderId = Guid.CreateVersion7();
                    var order = new Order(
                        orderId,
                        $"ORD-{orderCount++:D6}",
                        userEmail,
                        user.Key,
                        Math.Round((decimal)(random.NextDouble() * 1000 + 10), 2),
                        DateTime.UtcNow
                    );


                    logger.LogInformation("Generated order {OrderId} {ExternalId} for customer {CustomerName}", order.Id, order.ExternalId, order.CustomerName);

                    var channel = channelBuilder.GetOrCreateChannel(userEmail);

                    await channel.Writer.WriteAsync(order, stoppingToken);

                    await Task.Delay(TimeSpan.FromSeconds(random.Next(3,6)), stoppingToken); // message...3-6sec...message
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error generating order");
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }

            foreach (var user in users)
            {
                if(channelBuilder.TryGetChannelWriter(user.Value, out var writer))
                {
                    writer.Complete();
                }
            } 
        }
    }
}
