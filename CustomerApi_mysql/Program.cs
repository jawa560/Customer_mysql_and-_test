using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CustomerApi.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Net;
using DotNetEnv;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerApi", Version = "v1" });

                // To Enable authorization using Swagger (JWT)
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: 'Bearer 12345abcdef'",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
                });
                // Include XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Add example filters
                c.SchemaFilter<ExampleFilters>();
                c.OperationFilter<ExampleFilters>();
            });
            // Configure JWT authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var tokenValue = context.SecurityToken as JwtSecurityToken;
                        if (tokenValue != null)
                        {
                            var token = await context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>().Tokens
                                .SingleOrDefaultAsync(t => t.TokenValue == tokenValue.RawData);

                            if (token == null || token.ExpiryDate < DateTime.Now)
                            {
                                context.Fail("Invalid token");
                            }
                        }
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/json";
                        var result = new
                        {
                            message = "授權失敗，請先登入."
                        };
                        return context.Response.WriteAsJsonAsync(result);
                    }
                };
            });

            // Configure MySQL database

            // 讀取 .env 文件
            //builder.Configuration.AddEnvironmentVariables(prefix: "DOTNET_");

            // 讀取環境變數
            //builder.Configuration.AddEnvironmentVariables();


            // 輸出 MYSQL_CONNECTION_STRING 的內容
            var connectionString = builder.Configuration["MYSQL_CONNECTION_STRING"];
            Console.WriteLine($"MYSQL_CONNECTION_STRING: {connectionString}");


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                // ----------- Read connection string from appsettings.json    
                //    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                //    new MySqlServerVersion(new Version(8, 0, 21))));
                // ----------- Read connection string from .env  
                options.UseMySql(builder.Configuration[key: "MYSQL_CONNECTION_STRING"],
                 new MySqlServerVersion(new Version(8, 0, 21))));

            Console.WriteLine($"MYSQL_CONNECTION_STRING: {connectionString}");

            // Add authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("User", policy => policy.RequireRole("User"));
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
