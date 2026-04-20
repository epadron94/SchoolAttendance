using School.Attendance.Services;
using Microsoft.Azure.Cosmos;
using Azure.Identity;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();




var cosmosEndpoint = builder.Configuration["CosmosDbEndpoint"];
var KeyVaultUri = builder.Configuration["KeyVault"];

//Keyvault
builder.Configuration.AddAzureKeyVault(
            new Uri(KeyVaultUri),
            new DefaultAzureCredential()
);



builder.Services.AddSingleton(t =>
{
    return new CosmosClient(
        accountEndpoint: cosmosEndpoint,
        authKeyOrResourceToken: builder.Configuration["primaryMasterKey"]
    );

});
builder.Services.AddScoped<CosmosDbService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
