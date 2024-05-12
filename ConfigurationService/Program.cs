using Consul;
using Winton.Extensions.Configuration.Consul;

var builder = WebApplication.CreateBuilder(args);

// Add Consul Configuration
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddConsul(
        $"{hostingContext.HostingEnvironment.ApplicationName}/{hostingContext.HostingEnvironment.EnvironmentName}",
        options =>
        {
            options.ConsulConfigurationOptions = cco =>
            {
                cco.Address = new Uri("http://localhost:8500"); // Consul server address
            };
            options.Optional = true;
            options.ReloadOnChange = true;
            options.OnLoadException = exceptionContext => { exceptionContext.Ignore = true; };
        });
});

// Add services to the DI container.
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
    // Consul configuration setup
    var address = builder.Configuration["Consul:Host"];
    consulConfig.Address = new Uri(address);
}));

var app = builder.Build();

// Middleware pipeline configuration
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
