using Consul;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Consul service
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
    var address = builder.Configuration["ConsulConfig:Address"];
    consulConfig.Address = new Uri(address);
}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Register the service with Consul
var lifetime = app.Lifetime;
var consulClient = app.Services.GetRequiredService<IConsulClient>();

var registration = new AgentServiceRegistration()
{
    ID = "DataAccessService", // Unique ID for the service
    Name = "DataAccessService",
    Address = "localhost",
    Port = 7238,
    Tags = new[] { "DataAccessService" }
};

lifetime.ApplicationStarted.Register(() => {
    consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
    consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);
});

lifetime.ApplicationStopped.Register(() => {
    consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
});

app.Run();
