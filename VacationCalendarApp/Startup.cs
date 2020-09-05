using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using VacationCalendarApp.Data;
using VacationCalendarApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using IdentityModel;
using System.Linq;
using AutoMapper;


namespace VacationCalendarApp
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            //services.Configure<IdentityOptions>(options =>
            //{
            //    options.ClaimsIdentity.RoleClaimType = JwtClaimTypes.Role;
                
            //});

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>()

                .AddDefaultTokenProviders();

            

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options => {
                    options.IdentityResources["openid"].UserClaims.Add("role");
                    options.ApiResources.Single().UserClaims.Add("role");
                });
            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

            services.AddAuthorization()
                .AddAuthentication()
                .AddIdentityServerJwt()
                .AddJwtBearer();

            //services.AddAuthentication(o =>
            //    {
            //        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //    })
            //    .AddJwtBearer(o =>
            //    {
            //        //Additional config snipped
            //        o.Events = new JwtBearerEvents
            //        {
            //            OnTokenValidated = async ctx =>
            //            {
            //                //Get the calling app client id that came from the token produced by Azure AD
            //                string clientId = ctx.Principal.FindFirstValue("appid");

            //                //Add claim if yes
            //                var claims = new List<Claim> { new Claim("IsAdmin  ", "true") };
            //                var appIdentity = new ClaimsIdentity(claims);

            //                ctx.Principal.AddIdentity(appIdentity);

            //            }
            //        };
            //    }).AddIdentityServerJwt();


            //services.AddAuthorization(options =>
            //    options.AddPolicy("Admin", policy =>
            //    policy.RequireAuthenticatedUser()
            //    .RequireClaim("role", "Admin")));

            //services.AddHttpContextAccessor();
            //var mapperConfig = new MapperConfiguration(mc =>
            //{
            //    mc.AddProfile(new MappingProfile());
            //});

            //IMapper mapper = mapperConfig.CreateMapper();
            //services.AddSingleton(mapper);

            services.AddControllersWithViews();
            services.AddRazorPages();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
