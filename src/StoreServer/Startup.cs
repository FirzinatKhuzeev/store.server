namespace StoreServer
{
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Identity;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);

            services.AddCors(options =>
            {
                options.AddPolicy(nameof(Constants.AllowAll),
                    builder => builder.WithOrigins("*")
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddMvc();
            services.AddOptions();
            SetUpDataBase(services);

            services.Configure<CosmoDbSettings>(Configuration.GetSection(nameof(CosmoDbSettings)));
            services.Configure<JwtTokenSettings>(Configuration.GetSection(nameof(JwtTokenSettings)));
            services.AddSingleton(typeof(IDocumentDbRepository), typeof(DocumentDbRepositoryBase));

            var jwtTokenSettings = Configuration.GetSection(nameof(JwtTokenSettings));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                      .AddJwtBearer(options =>
                      {
                          options.RequireHttpsMetadata = false;
                          options.SaveToken = true;
                          options.TokenValidationParameters = new TokenValidationParameters()
                          {
                              ValidIssuer = jwtTokenSettings.GetSection(nameof(JwtTokenSettings.Issuer)).Value,
                              ValidAudience = jwtTokenSettings.GetSection(nameof(JwtTokenSettings.Audience)).Value,
                              ValidateIssuerSigningKey = true,
                              IssuerSigningKey = new SymmetricSecurityKey(
                                  Encoding.UTF8.GetBytes(jwtTokenSettings.GetSection(nameof(JwtTokenSettings.Key))
                                      .Value)),
                              ValidateLifetime = true
                          };
                      });

            services.AddIdentity<User, Role>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireDigit = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(nameof(Constants.AllowAll));
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                EnsureDatabaseCreated(dbContext);
            }
        }

        public virtual void SetUpDataBase(IServiceCollection services)
        {
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString(nameof(Constants.SecurityConnection))));
        }

        public virtual void EnsureDatabaseCreated(ApplicationDbContext dbContext)
        {
            dbContext.Database.Migrate();
        }
    }
}
