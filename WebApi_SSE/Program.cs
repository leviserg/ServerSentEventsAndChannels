using System.Threading.Channels;
using WebApi_SSE;
using WebApi_SSE.BackgroundJobs;
using WebApi_SSE.Models;
using WebApi_SSE.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

#region registerChannels

var channel = Channel.CreateUnbounded<Order>();

builder.Services.AddSingleton(channel);
builder.Services.AddSingleton(channel.Reader);
builder.Services.AddSingleton(channel.Writer);

#endregion registerChannels

builder.Services.AddSingleton<OrderEventsBuffer>();
builder.Services.AddSingleton<ChannelConnectionBuilder>();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddHostedService<OrderGenerationService>();

builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseHttpsRedirection();

app.MapOrderEventsEndpoints();

app.Run();
