using System;
using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace WebApp_OpenIDConnect_DotNet
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            var azureAd = Configuration.GetSection("AzureAd");
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
	            .AddMicrosoftIdentityWebApp(o =>
	            {
		            o.Instance = azureAd.GetValue<string>("Instance");
		            o.Domain = azureAd.GetValue<string>("Domain");
		            o.ClientId = azureAd.GetValue<string>("ClientId");
		            o.TenantId = azureAd.GetValue<string>("TenantId");
		            o.CallbackPath = azureAd.GetValue<string>("CallbackPath");
		            o.SaveTokens = true;

		            o.Events = new OpenIdConnectEvents
		            {
			            OnRemoteFailure = context =>
			            {
				            context.HandleResponse();
				            context.Response.Redirect("Home/Error?statusCode=403");
				            return Task.FromResult(0);
			            },
			            OnTokenValidated = context =>
			            {
				            var jwt = context.SecurityToken;
				            return Task.FromResult(jwt);
                        }
                    };
	            },
	            o => { o.ExpireTimeSpan = TimeSpan.FromMinutes(100); });

            //services.Configure(AzureADDefaults.OpenIdScheme, options =>
            //{
            //    options.Authority = options.Authority + "/v2.0/";         // Microsoft identity platform

            //    options.TokenValidationParameters.ValidateIssuer = false; // accept several tenants (here simplified)
            //});

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("MyGroup", builder => builder.RequireClaim("groups", "6b990d8c-0feb-4ebc-b974-31323a25a966"));
            });

            services.AddRazorPages()
                .AddMicrosoftIdentityUI();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
