﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Models;
using Users.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Users
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IPasswordValidator<AppUser>, CustomPasswordValidator>();

            services.AddTransient<IUserValidator<AppUser>, CustomUserValidator>();

            services.AddSingleton<IClaimsTransformation, LocationClaimsProvider>();

            services.AddTransient<IAuthorizationHandler, BlockUsersHandler>();

            services.AddTransient<IAuthorizationHandler, DocumentAuthorizationHandler>();

            services.AddDbContext<AppIdentityDbContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("IdentityDbContext")));

            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("DCUsers", policy =>
                {
                    policy.RequireRole("Users");
                    policy.RequireClaim(ClaimTypes.StateOrProvince, "DC");
                });
                opts.AddPolicy("NotBob", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new BlockUsersRequirement("Bob"));
                });
                opts.AddPolicy("AuthorsAndEditors", policy =>
                {
                    policy.AddRequirements(new DocumentAuthorizationRequirement
                    {
                        AllowAuthors = true,
                        AllowEditors = true
                    });
                });
            });

            services.AddAuthentication().AddGoogle(opts =>
            {
                opts.ClientId = "16460336012-997nl59rgqb5c502v7avo878jilu8c1k.apps.googleusercontent.com";
                opts.ClientSecret = "ANTmX_OJoV5mntyVy20sKDxk";
            });

            services.AddIdentity<AppUser, IdentityRole>(opts => {
                    opts.User.RequireUniqueEmail = true;
                    opts.Password.RequiredLength = 6;
                    opts.Password.RequireNonAlphanumeric = false;
                    opts.Password.RequireLowercase = false;
                    opts.Password.RequireUppercase = false;
                    opts.Password.RequireDigit = false;
                })
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStatusCodePages();
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            //AppIdentityDbContext.CreateAdminAccount(app.ApplicationServices, Configuration).Wait();
        }
    }
}
