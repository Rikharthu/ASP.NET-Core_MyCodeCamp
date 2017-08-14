using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyCodeCamp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            _config = builder.Build();
        }

        private IConfigurationRoot _config;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddSingleton(_config);
            services.AddScoped<ICampRepository, CampRepository>();

            string connectionString = _config.GetConnectionString("DefaultConnection");
            services.AddDbContext<CampContext>(
                options => options.UseSqlServer(connectionString),
                ServiceLifetime.Scoped);

            // Add AutoMapper for model mapping
            // This call searches the assembly for types that derive from AutoMapper.Profile class
            // In order to configure itself (how to map different objects)
            services.AddAutoMapper();

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    // configure json serializer to ignore reference looping
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            CampContext context)
        {
            loggerFactory.AddConsole(_config.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc(routes => { routes.MapRoute("MainAPIRoute", "api/{controller=Camps}/{action=Get}"); });

            CampDbInitializer dbInitializer = new CampDbInitializer(context);
            dbInitializer.Seed().Wait();
        }
    }
}