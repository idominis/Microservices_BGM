var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the IXmlService and its implementation
builder.Services.AddSingleton<FileManagementService.Services.FileManager>();
builder.Services.AddSingleton<FileManagementService.Interfaces.IXmlService, FileManagementService.Services.XmlService>();

//builder.Services.AddSingleton<FileManagementService.Interfaces.IXmlService, FileManagementService.Services.XmlService>();


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

app.Run();
