using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CustomerApi.Tests
{
  
    public class CustomWebApplicationFactory<Program> : WebApplicationFactory<Program> where Program : class
    {
      
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 配置 JWT 認證
                var key = Encoding.ASCII.GetBytes("530646AC-CA28-418A-8568-72AD4BC4960D"); // 至少 32 字節

                var authenticationService = services.BuildServiceProvider().GetService<IAuthenticationSchemeProvider>();
                if (authenticationService.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme).Result == null)
                {
                    services.AddAuthentication(x =>
                    {
                        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(x =>
                    {
                        x.RequireHttpsMetadata = false;
                        x.SaveToken = true;
                        x.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = "Jwt:Issuer",
                            ValidAudience = "Jwt:Audience",
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ClockSkew = TimeSpan.Zero
                        };
                    });
                }
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("User", policy => policy.RequireRole("User"));
                    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                });
            });
        }
    }
}