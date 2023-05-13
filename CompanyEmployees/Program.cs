using CompanyEmployees.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using NLog;

var builder = WebApplication.CreateBuilder(args);

// get the Nlog configuration class and get the on the correct one based on 
// based on the current environment 
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(),
    $"/nlog.{builder.Environment.EnvironmentName}.config"));

// Add services to the container.

builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
// registering sql and passing it the iconfig class to
// get the conn string
builder.Services.ConfigureSqlContext(builder.Configuration);


builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();


