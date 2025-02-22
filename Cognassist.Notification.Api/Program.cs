using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Notification API",
        Version = "v1",
        Description = "Notification API with Swagger documentation"
    });
});

builder.Services.AddHealthChecks();
  //              .AddCheck("self", () => HealthCheckResult.Healthy();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});

app.UseHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self") || r.Tags.Contains("dependency")
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
