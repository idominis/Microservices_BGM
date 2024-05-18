using Consul;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FileManagementService.Services;
using FileManagementService.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the IXmlService and its implementation
builder.Services.AddSingleton<FileManager>();
builder.Services.AddSingleton<IXmlService, XmlService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("https://localhost:7145") // URL of your FrontendService
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Register Consul client
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
    var address = builder.Configuration["Consul:Host"];
    consulConfig.Address = new Uri(address);
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowSpecificOrigins");

app.UseAuthorization();

app.MapControllers();

// Register with Consul
var lifetime = app.Lifetime;
var consul = app.Services.GetRequiredService<IConsulClient>();

var registration = new AgentServiceRegistration()
{
    ID = "FileManagementService", // Unique ID for the service
    Name = "FileManagementService", // Service name
    Address = "localhost", // Your service's IP address
    Port = 7128, // Your service's port
    Tags = new[] { "file-management" }
};

consul.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
consul.Agent.ServiceRegister(registration).ConfigureAwait(true);

lifetime.ApplicationStopping.Register(() =>
{
    consul.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
});

app.Run();
