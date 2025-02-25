using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.S3;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ValetParkingAPI.Extensions;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingBLL.Middleware;
using ValetParkingBLL.Repository;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.AWSModels;

namespace ValetParkingAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMemoryCache();
            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                //options.SuppressModelStateInvalidFilter = true;
                options.InvalidModelStateResponseFactory = actionContext =>
                    {
                        var modelState = actionContext.ModelState.Values;
                        return new OkObjectResult(new Response { Message = modelState.LastOrDefault().Errors.LastOrDefault().ErrorMessage });

                        // String.Join(",", modelState.Select(e => e.Errors.LastOrDefault().ErrorMessage))

                    };
            }).SetCompatibilityVersion(CompatibilityVersion.Latest)
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                // options.JsonSerializerOptions.MaxDepth = 500;
                // options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            }).AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                option.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });


            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


            var appSettingsSection = Configuration.GetSection("ServiceConfiguration");
            //   services.AddAWSService<IAmazonS3>();
            services.Configure<ServiceConfiguration>(appSettingsSection);

            services.AddLocalization();
            var appsettingssection =
            Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appsettingssection);
            services.RegisterDI();

            //  services.AddSingleton<CacheRepo>();

            var appSettings = appsettingssection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(au =>
           {
               au.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               au.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
           }).AddJwtBearer(jwt =>
           {
               jwt.RequireHttpsMetadata = false;
               jwt.SaveToken = true;
               jwt.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(key),
                   ValidateIssuer = false,
                   ValidateAudience = false
               };
           });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            app.UseAuthentication();
            app.UseAuthorization();
            //  app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
