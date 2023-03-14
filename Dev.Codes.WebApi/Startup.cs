using System.Text;
using Dev.Codes.Lib.Authentication.Helpers;
using Dev.Codes.Lib.Authentication.Repository;
using Dev.Codes.Lib.Authentication.Services;
using Dev.Codes.Lib.Database.Contexts;
using Dev.Codes.Lib.Database.DbClients;
using Dev.Codes.Lib.Database.Interfaces;
using Dev.Codes.Lib.EmailService.EmailSenders;
using Dev.Codes.Lib.EmailService.Interfaces;
using Dev.Codes.Lib.Ioc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Dev.Codes.WebApi
{
    public class Startup
    {
        public static IConfiguration Configuration { get; set; }
        public static IWebHostEnvironment WebHostEnvironment { get; set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = Configuration["JWT:Issuer"],
                    ValidAudience = Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF32.GetBytes(Configuration["JWT:SecretKey"]))
                };
            });
            services.AddControllers();
            services.AddSwaggerGen();

            services.AddSingleton<IMongoDbClient, MongoDbClient>();
            services.AddSingleton<IMongoDbContext, MongoDbContext>();
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
            services.AddSingleton<AuthRepository>();
            services.AddSingleton<TokenHelper>();
            services.AddSingleton<AuthService>();
            services.AddSingleton<UserService>();
            
            IocContainer.Instance.SetServiceProvider(services.BuildServiceProvider());
            IocContainer.Instance.SetConfiguration(Configuration);
            //var dbClient = IocContainer.Instance.Resolve<IMongoDbClient>("MongoDbClient");
        }

        public void Configure(IApplicationBuilder app)
        {
            if (WebHostEnvironment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
