using FrontendService.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Add support for MVC with views
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FrontendService", Version = "v1" });
});

// Configure HttpClient for each service using base addresses from configuration
var baseAddresses = builder.Configuration.GetSection("BaseAddresses").Get<Dictionary<string, string>>();
foreach (var (serviceName, baseAddress) in baseAddresses)
{
    builder.Services.AddHttpClient(serviceName, client =>
    {
        client.BaseAddress = new Uri(baseAddress);
    });
}

// Add SignalR
builder.Services.AddSignalR();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FrontendService v1");
        c.RoutePrefix = "swagger"; // Make Swagger UI accessible via /swagger endpoint
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Frontend}/{action=Index}/{id?}");

app.MapHub<UpdateHub>("/updateHub");

app.Run();
