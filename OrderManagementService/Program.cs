using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderManagementService.Services;
using Serilog;
using SharedLibrary;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var loggingServiceUrl = builder.Configuration["BaseAddresses:LoggingService"] + "/api/logs";
builder.Host.UseSerilog((context, services, configuration) => configuration
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Sink(new LoggingServiceSink(new HttpClient(), loggingServiceUrl))
);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HTTP client services
builder.Services.AddHttpClient("FileManagementServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseAddresses:FileManagementService"]);
});
builder.Services.AddHttpClient("SFTPCommunicationServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseAddresses:SFTPCommunicationService"]);
});
builder.Services.AddHttpClient("DataAccessServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseAddresses:DataAccessService"]);
});
builder.Services.AddHttpClient("FrontendServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseAddresses:FrontendService"]);
});

// Register IOrderService
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
