// Required namespaces
using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Extensions;
using Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using NLog;
using Service.DataShaping;
using Shared.DataTransferObjects;
using CompanyEmployees.Utility;

var builder = WebApplication.CreateBuilder(args);

// Get the NLog configuration class and load the appropriate configuration based on the current environment.
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), $"/nlog.{builder.Environment.EnvironmentName}.config"));

// Add services to the container.

// Configure Cross-Origin Resource Sharing (CORS).
builder.Services.ConfigureCors();

// Configure IIS Integration.
builder.Services.ConfigureIISIntegration();

// Configure Logger Service.
builder.Services.ConfigureLoggerService();

// Configure SQL Context and pass it the IConfiguration class to get the connection string.
builder.Services.ConfigureSqlContext(builder.Configuration);
// Configure Repository Manager.
builder.Services.ConfigureRepositoryManager(); 

// Add AutoMapper.
builder.Services.AddAutoMapper(typeof(Program));
// Add ValidationFilterAttribute as a scoped service.
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
// Configure Service Manager.
builder.Services.ConfigureServiceManager();
// versioning
builder.Services.ConfigureVersioning();
// caching
builder.Services.ConfigureResponseCaching();
// caching validation
builder.Services.ConfigureHttpCacheHeaders();


builder.Services.AddScoped<ValidateMediaTypeAttribute>();
builder.Services.AddScoped<IEmployeeLinks, EmployeeLinks>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // This will overwrite the behavior of the ApiController decorator on our controller class
    // when returning ModelState errors for empty [FromBody] parameters.
    // We want to specify our own custom error handling model.
    options.SuppressModelStateInvalidFilter = true;
});

// valid the accept headers

NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() =>
    // By adding a method like this in the Program class,
    // we are creating a local function. This function configures
    // support for JSON Patch using Newtonsoft.Json while leaving
    // the other formatters unchanged.
    new ServiceCollection()
    .AddLogging()
    .AddMvc()
    .AddNewtonsoftJson()
    .Services
    .BuildServiceProvider()
    .GetRequiredService<IOptions<MvcOptions>>()
    .Value
    .InputFormatters
    .OfType<NewtonsoftJsonPatchInputFormatter>()
    .First();

// Because the normal convention of having controllers in the main project was not followed, 
// we have to point the Program file to where it can find the controllers, which is in 
// our presentation project. Without this, our API won't work.
builder.Services.AddControllers(config =>
{
    // Configure the services controller to return XML or text.
    config.RespectBrowserAcceptHeader = true;

    // Configure the services controller to return 406 Not Acceptable for unrecognised format.
    config.ReturnHttpNotAcceptable = true;

    config.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
    config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 });
})
    .AddXmlDataContractSerializerFormatters()
    // Add custom CSV formatter.
    .AddCustomCSVFormatter()
    .AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);

builder.Services.AddCustomMediaTypes();
var app = builder.Build();

// This must be done before the builder.Build method.
// Reference UACWA pg 72.
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
// cache config cache store
app.UseResponseCaching();
// cache validation
app.UseHttpCacheHeaders();

app.UseAuthorization();

app.MapControllers();

app.Run();
