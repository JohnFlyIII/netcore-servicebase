using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ServiceBase.OData.Core.Interfaces;
using ServiceBase.OData.Infrastructure.Data;
using ServiceBase.OData.Infrastructure.Repositories;
using ServiceBase.OData.Web.Configuration;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Swashbuckle.AspNetCore.Swagger;

namespace ServiceBase.OData.Web
{
    public class Startup
    {
        public IHostingEnvironment _hostinEnvironment { get; set; }
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _hostinEnvironment = hostingEnvironment;

            var elasticUri = Configuration["Elasticsearch:Uri"];

            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(configuration);

            if (!string.IsNullOrWhiteSpace(elasticUri))
            {
                var esOptions = new ElasticsearchSinkOptions(new Uri(elasticUri))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6
                };

                loggerConfig = loggerConfig.WriteTo.Elasticsearch(esOptions);
            }

            Log.Logger = loggerConfig.CreateLogger();
        }

        /// <summary>
        /// ASPNETCORE ConfigureServices
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            AddCors(services);

            services.AddHttpContextAccessor();

            ConfigurePersistance(services);

            AddOData(services);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            VersionedODataModelBuilder modelBuilder,
            IApiVersionDescriptionProvider provider,
            IApplicationLifetime applicationLifetime)
        {
            app.UseCors("CorsPolicy");

            if (!env.IsDevelopment())
            {
                
            }
            
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            ConfigureAutoMapper();

            var models = modelBuilder.GetEdmModels();

            app.UseMvc(routes =>
            {
                routes.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                routes.MapVersionedODataRoutes("odata", "odata", models);
            });

            if (File.Exists(XmlCommentsFilePath))
            {
                app.UseSwagger();
                app.UseSwaggerUI(
                    options =>
                    {
                        foreach (var description in provider.ApiVersionDescriptions)
                        {
                            options.SwaggerEndpoint(
                                $"/swagger/{description.GroupName}/swagger.json",
                                description.GroupName.ToUpperInvariant());
                        }
                    });
            }
            else
            {
                Log.Fatal("XML file does not exist. Check build properties.");
                applicationLifetime.StopApplication();
            }

        }

        private void ConfigurePersistance(IServiceCollection services)
        {
            var heroesContextConnectionString = Configuration.GetConnectionString("HeroesContext");

            var persistenceProvider = Configuration["Persistence:Provider"].ToUpperInvariant();

            var heroesContextOptionsBuilder = new DbContextOptionsBuilder<HeroesContext>();

            switch (persistenceProvider)
            {
                case "POSTGRES":
                    heroesContextOptionsBuilder.UseNpgsql(heroesContextConnectionString);
                    break;
                default:
                    throw new NotImplementedException($"The persistenceProvider option: '{persistenceProvider}' is unsupported");
            }
            var o = heroesContextOptionsBuilder.Options;

            services.AddScoped<DbContextOptions<HeroesContext>>(_=>o);

            services.AddDbContext<HeroesContext>(options => options = heroesContextOptionsBuilder);

            services.AddScoped<IHeroesRepository, HeroesRepository>();
        }

        private void AddOData(IServiceCollection services)
        {
            services.AddApiVersioning(
                options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = ApiVersions.V1;
            });

            services.AddOData().EnableApiVersioning();

            services.AddODataApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });
            services.AddSwaggerGen(
                options =>
                {
                    options
                    .AddSecurityDefinition(
                        "Bearer",
                        new ApiKeyScheme
                        {
                            In = "header",
                            Description = "Please enter JWT with Bearer into field",
                            Name = "Authorization",
                            Type = "apiKey"
                        });

                    options
                    .AddSecurityRequirement(
                        new Dictionary<string, IEnumerable<string>>
                        {
                            { "Bearer", Enumerable.Empty<string>() },
                        });

                    // resolve the IApiVersionDescriptionProvider service
                    // note: that we have to build a temporary service provider here because one has not been created yet
                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                    // add a swagger document for each discovered API version
                    // note: you might choose to skip or document deprecated API versions differently
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
                    }

                    // add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    // integrate xml comments
                    options.IncludeXmlComments(XmlCommentsFilePath);
                });
        }
        private void AddCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }

        private static string XmlCommentsFilePath
        {
            get
            {
                var basePath = AppContext.BaseDirectory;
                var assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                var fileName = Path.GetFileName(assemblyName + ".xml");
                return Path.Combine(basePath, fileName);
            }
        }

        private static Info CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new Info()
            {
                Title = $"Sample API {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = "A sample application with Swagger, Swashbuckle, and API versioning.",
                Contact = new Contact() { Name = "Bill Mei", Email = "bill.mei@somewhere.com" },
                TermsOfService = "Shareware",
                License = new License() { Name = "MIT", Url = "https://opensource.org/licenses/MIT" }
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        private void ConfigureAutoMapper()
        {
            AutoMapper.Mapper.Initialize(config =>
            {
                config.CreateMap<Models.Hero, Core.Entities.HeroEntity>();
                config.CreateMap<Core.Entities.HeroEntity, Infrastructure.Models.HeroDataModel>();

                
                //config.CreateMap<User, Core.Entities.User>();

                //config.CreateMap<Role, Core.Entities.Role>();
                //config.CreateMap<Core.Entities.Role, Role>();
            });
        }
    }
}
