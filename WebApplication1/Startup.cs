using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddMvc();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.Authority = Configuration["Jwt:Authority"];
                o.Audience = Configuration["Jwt:Audience"];
                o.RequireHttpsMetadata = false;   // !!!!  to tylko dla srodowiska developreskiego   !!!!
                o.SaveToken = true;

                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    // ValidIssuer = Configuration["Tokens:Issuer"],
                    // ValidAudience = Configuration["Tokens:Issuer"],
                   // ValidateIssuerSigningKey = true,
                    
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    AudienceValidator = AudienceValidator1,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false

                    //IssuerSigningKey = new AsymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
                };

                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                       // c.NoResult();

                       // c.Response.StatusCode = 500;
                       // c.Response.ContentType = "text/plain";
                        Log.Error(c.Exception.ToString());
                        return Task.CompletedTask;
                    },

                    OnForbidden = c =>
                    {
                        Log.Information("OnForbidden !");
                        return Task.CompletedTask;
                    },
                    
                    OnTokenValidated = c =>
                    {
                        /*                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var userId = int.Parse(context.Principal.Identity.Name);
                        var user = userService.GetById(userId);
                        if (user == null)
                        {
                            // return unauthorized if user no longer exists
                            context.Fail("Unauthorized");
                        }
                        */
                        //c.Response.StatusCode = 200;
                        //c.Success();
                        Log.Information("OnTokenValidated !");
                        return Task.CompletedTask;
                    }

                };


            });

            /*  
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrator", policy => policy.RequireClaim("user_roles", "[Administrator]"));
            });
            */
            /*
            services.AddAuthorization(options =>
            {
                options.AddPolicy("GetKeyRole", policy =>
                {
                    policy.RequireRole("GetKey");
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });

            });
            */

        }

        public bool AudienceValidator1(IEnumerable<string> audiences, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            string jsonString;
            jsonString = JsonSerializer.Serialize(audiences);
            Log.Debug("AudienceValidator1 !: " + jsonString);

            return true;
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
