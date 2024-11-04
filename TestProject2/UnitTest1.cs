using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using CustomerApi.Data;



namespace CustomerApi_Test;
public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 配置 MySQL 數據庫
            services.AddDbContext<YourDbContext>(options =>
            {
                options.UseMySql("Server=localhost;Database=crud;User=root;Password=wind2009;",
                    new MySqlServerVersion(new Version(8, 0, 21)));
            });

            // 建立數據庫範圍
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<YourDbContext>();
                // 清除資料庫並插入測試資料
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // 插入初始數據
                if (!db.Users.Any())
                {
                    db.Users.Add(new User { Username = "user", Password = "password", Role = "User" });
                    db.Users.Add(new User { Username = "admin", Password = "password", Role = "Admin" });
                    db.SaveChanges();
                }
                if (!db.Customers.Any())
                {
                    // 添加測試用戶數據
                    db.Customers.Add(new Customer { Id = 1, Name = "John Doe", Birthday = DateOnly.FromDateTime(new DateTime(1990, 1, 1)), Gender = "Male", Address = "123 Street", Phone = "1234567890", Note1 = "aaaaaaaaaa", Note2 = "1111111111111111" });
                    db.Customers.Add(new Customer { Id = 2, Name = "Jane Doe", Birthday = DateOnly.FromDateTime(new DateTime(1995, 2, 2)), Gender = "Female", Address = "456 Avenue", Phone = "0987654321", Note1 = "bbbbbbbbbbbb", Note2 = "2222222222222" });
                    db.SaveChanges();
                }
            }
        });
    }
}

public class CustomerControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomerControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        //_client = _factory.WithWebHostBuilder(builder => {
        //    builder.ConfigureServices(services => {
        //        // 配置 MySQL 數據庫
        //        services.AddDbContext<YourDbContext>(options => {
        //            options.UseMySql("Server=localhost;Database=crud;User=root;Password=wind2009;",
        //                new MySqlServerVersion(new Version(8, 0, 21)));
        //        });

        //        // 建立數據庫範圍
        //        var serviceProvider = services.BuildServiceProvider();
        //        using (var scope = serviceProvider.CreateScope())
        //        {
        //            var db = scope.ServiceProvider.GetRequiredService<YourDbContext>();
        //            db.Database.EnsureDeleted();
        //            db.Database.EnsureCreated();
        //            if (!db.Users.Any())
        //            {
        //                // 添加測試用戶數據
        //                db.Users.Add(new User { Username = "user", Password = "password", Role = "User" });
        //                db.Users.Add(new User { Username = "admin", Password = "password", Role = "Admin" });
        //                db.SaveChanges();
        //            }
        //            if (!db.Customers.Any())
        //            {
        //                // 添加測試用戶數據
        //                db.Customers.Add(new Customer { Id = 1, Name = "John Doe", Birthday = DateOnly.FromDateTime(new DateTime(1990, 1, 1)), Gender = "Male", Address = "123 Street", Phone = "1234567890", Note1 = "aaaaaaaaaa", Note2 = "1111111111111111" });
        //                db.Customers.Add(new Customer { Id = 2, Name = "Jane Doe", Birthday = DateOnly.FromDateTime(new DateTime(1995, 2, 2)), Gender = "Female", Address = "456 Avenue", Phone = "0987654321", Note1 = "bbbbbbbbbbbb", Note2 = "2222222222222" });
        //                db.SaveChanges();
        //            }


        //        }
        //    });
        //}).CreateClient();
    }
    public class TokenResponse
    {
        public string Token { get; set; }
    }
    private async Task<string> LoginAsync(string username, string password)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
        loginResponse.EnsureSuccessStatusCode();
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse.Token;
    }

    [Fact]
    public async Task DeleteCustomer_ReturnsNoContent()
    {
        // Arrange
        var token = await LoginAsync("admin", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync("/api/customers/1");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_ReturnsOk()
    {
        // Arrange
        var token = await LoginAsync("user", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/customers/1");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}


public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Token> Tokens { get; set; }
    public DbSet<Item> Items { get; set; }


}

//public class User
//{
//    public int Id { get; set; }
//    public string Username { get; set; }
//    public string Password { get; set; }
//    public string Role { get; set; }
//}
//public class Customer
//{
//    public int Id { get; set; }
//    public string Name { get; set; }
//    public string Gender { get; set; }
//    public string Phone { get; set; }
//    public string Address { get; set; }
//    public DateOnly Birthday { get; set; }
//    public string Note1 { get; set; }
//    public string Note2 { get; set; }


//}