var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("FileManagementServiceClient", client =>
{
    var baseAddress = builder.Configuration["BaseAddresses:FileManagementService"];
    if (string.IsNullOrEmpty(baseAddress))
    {
        throw new ArgumentNullException(nameof(baseAddress), "Base address for FileManagementService is not configured.");
    }
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddHttpClient("SFTPCommunicationServiceClient", client =>
{
    var baseAddress = builder.Configuration["BaseAddresses:SFTPCommunicationService"];
    if (string.IsNullOrEmpty(baseAddress))
    {
        throw new ArgumentNullException(nameof(baseAddress), "Base address for SFTPCommunicationService is not configured.");
    }
    client.BaseAddress = new Uri(baseAddress);
});
builder.Services.AddHttpClient("DataAccessServiceClient", client =>
{
    var baseAddress = builder.Configuration["BaseAddresses:DataAccessService"];
    if (string.IsNullOrEmpty(baseAddress))
    {
        throw new ArgumentNullException(nameof(baseAddress), "Base address for DataAccessService is not configured.");
    }
    client.BaseAddress = new Uri(baseAddress);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
