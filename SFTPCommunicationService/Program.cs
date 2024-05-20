using Serilog;
using SFTPCommunicationService;
using SFTPCommunicationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the SFTP services
var sftpConfig = builder.Configuration.GetSection("SftpConfig").Get<SftpConfig>();
builder.Services.AddSingleton(sftpConfig);
builder.Services.AddSingleton<SftpClientManager>();
builder.Services.AddSingleton<SftpFileHandler>();

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
