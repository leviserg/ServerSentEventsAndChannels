using Microsoft.AspNetCore.Mvc;
using WebApi_SSE.Models;
using WebApi_SSE.Services;

namespace WebApi_SSE
{
    public static class OrderEventsStreamEndpoints
    {
        public static void MapOrderEventsEndpoints(this IEndpointRouteBuilder app)
        {

            app.MapPost("/api/login", (TokenService tokenService, [FromBody] LoginRequest request) =>
            {
                if (string.IsNullOrWhiteSpace(request.UserName))
                {
                    return Results.BadRequest(new {error = "userName is required"});
                }

                var userIds = UsersMap.GetUserNames;

                if(!userIds.Contains(request.UserName))
                {
                    return Results.Unauthorized();
                }

                var userEmail = UsersMap.GetUserEmail(request.UserName);

                var token = tokenService.GenerateToken(userEmail!);
                return Results.Ok(new {token});
            }).WithName("Login");


            // Simple events streaming, no wrapping, no buffering, no missed events handling.
            app.MapGet("/api/orders/stream",
                (ChannelConnectionBuilder channelBuilder, CancellationToken cancellationToken) =>
                {
                    var orders = OrderEventStreamBuilder.GetOrdersForAllUsers(
                        channelBuilder,
                        UsersMap.Users.Values,
                        cancellationToken);

                    return Results.ServerSentEvents(orders, nameof(Order).ToLower());
                });


            // Streams buffered order events, including any missed events since the last event ID.
            app.MapGet("/api/orders/buffer-stream",
                (OrderEventsBuffer buffer,
                        TokenService tokenService,
                        ChannelConnectionBuilder channelBuilder,
                        [FromQuery] string? token,
                        [FromHeader(Name = "x-LastEventId")] string? lastEventId,
                        CancellationToken cancellationToken)
                    =>
                {
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return Results.Unauthorized();
                    }

                    var userEmail = tokenService.ValidateToken(token);

                    if (userEmail is null)
                    {
                        return Results.Unauthorized();
                    }

                    var channel = channelBuilder.GetOrCreateChannel(userEmail);

                    var stream = OrderEventStreamBuilder.GetBufferedOrdersForUser(
                        buffer,
                        channel.Reader,
                        userEmail,
                        lastEventId,
                        cancellationToken);

                    return TypedResults.ServerSentEvents(stream, nameof(Order).ToLower());
                });
        }
    }
}
