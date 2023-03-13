using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using Microsoft.EntityFrameworkCore;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DevExpress.ExpressApp.WebApi.Services;
using Microsoft.AspNetCore.OData;
using DevExpress.ExpressApp.Core;
using WebApi.JWT;
using DevExpress.ExpressApp.Security.Authentication;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using WebApi.Core;
using DevExpress.ExpressApp.AspNetCore.WebApi;
using WebApi.BusinessObjects;
using System.Runtime.InteropServices;

namespace WebApi;

public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services) {
        services
            .AddScoped<IAuthenticationTokenProvider, JwtTokenProviderService>()
            .AddScoped<IObjectSpaceProviderFactory, ObjectSpaceProviderFactory>()
            .AddSingleton<IWebApiApplicationSetup, WebApiApplicationSetup>();

        services.AddXafAspNetCoreSecurity(Configuration, options => {
            options.RoleType = typeof(PermissionPolicyRole);
            // ApplicationUser descends from PermissionPolicyUser and supports the OAuth authentication. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/402197
            // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
            options.UserType = typeof(BusinessObjects.ApplicationUser);
            // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
            // If you use PermissionPolicyUser or a custom user type, comment out the following line:
            options.UserLoginInfoType = typeof(BusinessObjects.ApplicationUserLoginInfo);

            options.SupportNavigationPermissionsForTypes = false;
        }).AddAuthenticationStandard(options => {
            options.IsSupportChangePassword = true;
        });
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters() {
                    ValidateIssuerSigningKey = true,
                    //ValidIssuer = Configuration["Authentication:Jwt:Issuer"],
                    //ValidAudience = Configuration["Authentication:Jwt:Audience"],
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:IssuerSigningKey"]))
                };
            });

        services.AddAuthorization(options => {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .RequireXafAuthentication()
                    .Build();
        });
        services.AddDbContextFactory<WebApi.BusinessObjects.WebAPIEFCoreDbContext>((serviceProvider, options) => {
            // Uncomment this code to use an in-memory database. This database is recreated each time the server starts. With the in-memory database, you don't need to make a migration when the data model is changed.
            // Do not use this code in production environment to avoid data loss.
            // We recommend that you refer to the following help topic before you use an in-memory database: https://docs.microsoft.com/en-us/ef/core/testing/in-memory
            //options.UseInMemoryDatabase("InMemory");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                string connectionString = Configuration.GetConnectionString("ConnectionString");
                options.UseSqlServer(connectionString);
            }
            else {
                string sqliteDBPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebAPIAuth2.db");
                options.UseSqlite($"Data Source={sqliteDBPath}");
            }

            options.UseChangeTrackingProxies();
            options.UseObjectSpaceLinkProxies();
            options.UseLazyLoadingProxies();
            options.UseSecurity(serviceProvider);
        }, ServiceLifetime.Scoped);

        services
            .AddXafWebApi(Configuration, options => {
                // Make your business objects available in the Web API and generate the GET, POST, PUT, and DELETE HTTP methods for it.
                options.BusinessObject<ApplicationUser>();
            });
        services
            .AddControllers()
            .AddOData((options, serviceProvider) => {
                options
                    .AddRouteComponents("api/odata", new EdmModelBuilder(serviceProvider).GetEdmModel())
                    .EnableQueryFeatures(100);
            });

        services.AddSwaggerGen(c => {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo {
                Title = "WebAPI API",
                Version = "v1",
                Description = @"Use AddXafWebApi(options) in the WebApi\Startup.cs file to make Business Objects available in the Web API."
            });
            c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme() {
                Type = SecuritySchemeType.Http,
                Name = "Bearer",
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme() {
                            Reference = new OpenApiReference() {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "JWT"
                            }
                        },
                        new string[0]
                    },
            });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if(env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI WebApi v1");
            });
        }
        else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
#if !DEBUG
        app.UseHttpsRedirection();
#endif
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
        });
    }
}
