
using Domain.Contracts;
using Domain.Entities.SecurityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Data;
using Services.Abstractions;
using Services;
using Shared.SecurityModels;
using AutoMapper;
using Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.SignalR;

namespace Start
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            
            // Register IChatHubContext implementation
            builder.Services.AddScoped<IChatHubContext, ChatHubContext>();
            
            // Add CORS - Allow ALL frontend requests
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });
            
            // Add Identity services
            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<StoreContext>()
                .AddDefaultTokenProviders();
            
            // Add DbContext
            builder.Services.AddDbContext<StoreContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
            });
            
            // Add AutoMapper - scan all assemblies for profiles
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            // Add Unit of Work
            builder.Services.AddScoped<IUnitOFWork, UnitOfWork>();
            
            // Add Service Manager
            builder.Services.AddScoped<IServiceManager, ServiceManager>();
            
            // Add DbInitializer
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            
            // Configure JWT Options
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
            builder.Services.Configure<DomainSettings>(builder.Configuration.GetSection("DomainUrls"));

            // Add JWT Authentication
            var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });
                
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API", Version = "v1" });

                // Add JWT Authentication
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();
            await InitializeDbAsync(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Use CORS - Allow ALL requests from any origin
            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<ChatHub>("/chathub");

            app.Run();
            async Task InitializeDbAsync(WebApplication app)
            {
                using var scope = app.Services.CreateScope();
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                await dbInitializer.InitializeAsync();
            }
        }
    }
}
