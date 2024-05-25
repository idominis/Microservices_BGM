using LoggingService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR services
builder.Services.AddSignalR();

// Configure HttpClient for FrontendService
builder.Services.AddHttpClient("FrontendServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseAddresses:FrontendService"]);
});

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

// Map SignalR hubs
app.MapHub<LogHub>("/logHub");

app.Run();
