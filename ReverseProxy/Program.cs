var builder = WebApplication.CreateBuilder(args);


// IMPORTANT: Install Yarp (Yet Another Reverse Proxy) NuGet Package

builder.Services.AddHttpForwarder();


var app = builder.Build();

app.UseHttpsRedirection();

app.MapForwarder("/{**catch-all}", "https://localhost:7106");

app.Run();