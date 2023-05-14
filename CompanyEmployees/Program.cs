using CompanyEmployees.Extensions;
using Contracts;
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
builder.Services.AddAutoMapper(typeof(Program));

// becuse normal convention of having controller in the main project was not followed 
// we have to point the progrqam file to where it can find the controller and that is in 
// our presentation project without the our api won't work
builder.Services.AddControllers(config =>
{
    // configing the services controller to return xml or text 
    config.RespectBrowserAcceptHeader = true;
    // configuring the service controller to return un accepted for
    // unrecognised format 
    config.ReturnHttpNotAcceptable = true;
})
    .AddXmlDataContractSerializerFormatters()
    // custom csv formatter
    .AddCustomCSVFormatter()
    .AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);

var app = builder.Build();

// this must be done before the builder.Build method 
// reference UACWA pg 72
var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);

if (app.Environment.IsProduction())
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


