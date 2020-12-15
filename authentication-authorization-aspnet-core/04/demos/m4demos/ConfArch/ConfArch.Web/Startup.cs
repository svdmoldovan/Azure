using System;
using System.Net.Http.Headers;
using ConfArch.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConfArch.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(o =>
            o.Filters.Add(new AuthorizeFilter()));

            services.AddAuthentication(o =>
            {
                o.DefaultScheme =
                CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme =
                OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    options.Authority = "https://localhost:5000";

                    options.ClientId = "confarch_web";
                    //Store in application secrets
                    options.ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0";
                    options.CallbackPath = "/signin-oidc";

                    options.Scope.Add("confarch");
                    options.Scope.Add("confarch_api");

                    options.SaveTokens = true;

                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.ClaimActions.MapUniqueJsonKey("CareerStarted",
                        "CareerStarted");
                    options.ClaimActions.MapUniqueJsonKey("FullName", "FullName");
                    options.ClaimActions.MapUniqueJsonKey("Role", "role");
                    options.ClaimActions.MapUniqueJsonKey("Permission", "Permission");

                    options.ResponseType = "code";
                    options.ResponseMode = "form_post";

                    options.UsePkce = true;
                });

            services.AddHttpContextAccessor();
            services.AddHttpClient<IConfArchApiService, ConfArchApiService>(
                async (services, client) =>
            {
                var accessor = services.GetRequiredService<IHttpContextAccessor>();
                var accessToken = await accessor.HttpContext
                    .GetTokenAsync("access_token");
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("bearer", accessToken);
                client.BaseAddress = new Uri("https://localhost:5002");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Conference}/{action=Index}/{id?}");
            });
        }
    }
}
