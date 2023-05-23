﻿using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service;
using Service.Contracts;
using Microsoft.AspNetCore.Mvc.Versioning;
using Marvin.Cache.Headers;
using AspNetCoreRateLimit;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Linq.Dynamic.Core.Tokenizer;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        // Cors
        public static void ConfigureCors(this IServiceCollection services) =>
            services.AddCors(options =>
            {
            options.AddPolicy("CorsPolicy", builder =>
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("X-Pagination")
                );
            });
        // IIS Configuration
        public static void ConfigureIISIntegration(this IServiceCollection services) =>
            services.Configure<IISOptions>(options =>
            {

            });
        // logger
        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddSingleton<ILoggerManager, LoggerManager>();

        // repository manager which allow us add the two medel repoitories as one 
        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>();

        // services 
        public static void ConfigureServiceManager(this IServiceCollection services) =>
            services.AddScoped<IServiceManager, ServiceManager>();


        // registering the new db context
        public static void ConfigureSqlContext(this IServiceCollection services,
            IConfiguration configuration) =>
            services.AddDbContext<RepositoryContext>(opts =>
            opts.UseSqlServer(configuration.GetConnectionString("sqlConnection")));

        public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) =>
            builder.AddMvcOptions(config =>
            config.OutputFormatters.Add(new CsvOutputFormatter()));

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                // Retrieving the SystemTextJsonOutputFormatter from the configured output formatters
                var systemTextJsonOutputFormatter = config.OutputFormatters
                    .OfType<SystemTextJsonOutputFormatter>()?
                    .FirstOrDefault();

                // Checking if the SystemTextJsonOutputFormatter is found
                if (systemTextJsonOutputFormatter != null)
                {
                    // Adding a custom media type to the supported media types of SystemTextJsonOutputFormatter
                    systemTextJsonOutputFormatter.SupportedMediaTypes
                        .Add("application/vnd.codemaze.hateoas+json");
                    systemTextJsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.apiroot+json");
                }

                // Retrieving the XmlDataContractSerializerOutputFormatter from the configured output formatters
                var xmlOutputFormatter = config.OutputFormatters
                    .OfType<XmlDataContractSerializerOutputFormatter>()?
                    .FirstOrDefault();

                // Checking if the XmlDataContractSerializerOutputFormatter is found
                if (xmlOutputFormatter != null)
                {
                    // Adding a custom media type to the supported media types of XmlDataContractSerializerOutputFormatter
                    xmlOutputFormatter.SupportedMediaTypes
                        .Add("application/vnd.codemaze.hateoas+xml");
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.apiroot+xml");
                }
            });
        }
        public static void ConfigureVersioning(this IServiceCollection services)
        {
            // version controller
            services.AddApiVersioning(opt => 
            {
                opt.ReportApiVersions = true;
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.ApiVersionReader = new HeaderApiVersionReader("api-version");
                // to do this you can remove the apiversion decorator on the route
                //opt.Conventions.Controller<CompanyController>()
                //.HasApiVersion(new ApiVersion(1, 0));
                //opt.Conventions.Controller<CompaniesV2Controller>()
                //.HasDeprecatedApiVersion(new ApiVersion(2, 0));
            });
        }

        // caching configuration
        public static void ConfigureResponseCaching(this IServiceCollection services) =>
            services.AddResponseCaching();

        // cache validation with marvin.cache.headers
        public static void ConfigureHttpCacheHeaders(this IServiceCollection services) =>
            services.AddHttpCacheHeaders(
                // NOTE: configuration on action level will overide the configuration
                // on the controller level which will overide config on the global level
                (expirationOpt) =>
                {
                    expirationOpt.MaxAge = 200;
                    expirationOpt.CacheLocation = CacheLocation.Private;
                },
                (validationOpt) =>
                {
                    validationOpt.MustRevalidate = true;
                }
                );
        public static void ConfigureRateLimitingOptions(this IServiceCollection services) 
        {
            var rateLimitRules = new List<RateLimitRule> 
            {
                new RateLimitRule 
                {
                    Endpoint = "*",
                    Limit = 10,
                    Period = "5m" 
                }
            };
            services.Configure<IpRateLimitOptions>(opt =>
            {
                opt.GeneralRules = rateLimitRules;
            });
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>(); 
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>(); 
        }

        public static void ConfigureIdentity(this IServiceCollection services) 
        {
            var builder = services.AddIdentity<User, IdentityRole>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 10;
                o.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<RepositoryContext>()
                .AddDefaultTokenProviders(); 
        }
        public static void ConfigureJWT(this IServiceCollection services,
            IConfiguration configuration) 
        {
            var jwtSettings = configuration.GetSection("JwtSettings"); 
            var secretKey = Environment.GetEnvironmentVariable("SECRET");
            services.AddAuthentication(opt => 
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => 
            {
                options.TokenValidationParameters = new TokenValidationParameters 
                {
                    // The issuer is the actual server that created the token (ValidateIssuer=true)
                    ValidateIssuer = true,
                    //The receiver of the token is a valid recipient(ValidateAudience = true)
                    ValidateAudience = true,
                    //The token has not expired(ValidateLifetime = true)
                    ValidateLifetime = true,
                    //The signing key is valid and is trusted by the server(ValidateIssuerSigningKey = true)
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["validIssuer"], 
                    ValidAudience = jwtSettings["validAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) 
                }; 
            });
        }
    }
}
