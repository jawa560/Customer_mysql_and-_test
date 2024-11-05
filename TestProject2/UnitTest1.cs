using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using CustomerApi.Data;
using Microsoft.AspNetCore.Mvc;
using CustomerApi.Controllers;
using Microsoft.Extensions.Configuration;



namespace CustomerApi_Test;
public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            //// 配置 MySQL 數據庫
            //services.AddDbContext<ApplicationDbContext>(options =>
            //{
            //    options.UseMySql("Server=localhost;Database=crud;User=root;Password=wind2009;",
            //        new MySqlServerVersion(new Version(8, 0, 21)));
                
            //});
            
            //// 建立數據庫範圍
            //var serviceProvider = services.BuildServiceProvider();
            //using (var scope = serviceProvider.CreateScope())
            //{
            //    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                

            //    // 清除資料庫並插入測試資料
            //    db.Database.EnsureDeleted();
            //    db.Database.EnsureCreated();

            //    // 插入初始數據
            //    if (!db.Users.Any())
            //    {
            //        db.Users.Add(new User { Username = "user", Password = "password", Role = "User" });
            //        db.Users.Add(new User { Username = "admin", Password = "password", Role = "Admin" });
            //        db.SaveChanges();
            //    }
            //    if (!db.Customers.Any())
            //    {
            //        // 添加測試用戶數據
            //        db.Customers.Add(new Customer { Id = 1, Name = "John Doe", Birthday = DateOnly.FromDateTime(new DateTime(1990, 1, 1)), Gender = "Male", Address = "123 Street", Phone = "1234567890", Note1 = "aaaaaaaaaa", Note2 = "1111111111111111" });
            //        db.Customers.Add(new Customer { Id = 2, Name = "Jane Doe", Birthday = DateOnly.FromDateTime(new DateTime(1995, 2, 2)), Gender = "Female", Address = "456 Avenue", Phone = "0987654321", Note1 = "bbbbbbbbbbbb", Note2 = "2222222222222" });
            //        db.SaveChanges();
            //    }
            //}
        });
    }
    
}

public class CustomerControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly CustomersController _controller;
    private readonly ApplicationDbContext _context;


    public CustomerControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        /// 使用 WebApplicationFactory 來創建 ApplicationDbContext 實例
        var scope = _factory.Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        _context = scopedServices.GetRequiredService<ApplicationDbContext>();

        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 配置 MySQL 數據庫
                var configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();
                var connectionString = configuration["MYSQL_CONNECTION_STRING"];
                
                // 配置 MySQL 數據庫
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseMySql(connectionString,
                        new MySqlServerVersion(new Version(8, 0, 21)));
                });

                // 建立數據庫範圍
                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    if (!db.Users.Any())
                    {
                        // 添加測試用戶數據
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
        }).CreateClient();
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
    public async Task GetCustomers_ReturnAllCustomers()
    {
        // Arrange
        var token = await LoginAsync("user", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.EnsureSuccessStatusCode();
        var customers = await response.Content.ReadFromJsonAsync<IEnumerable<Customer>>();
        Assert.Equal(2, customers.Count());
    }

    [Fact]
    public async Task GetCustomer_ReturnsCustomerById()
    {
        // Arrange
        var token = await LoginAsync("user", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/customers/1");

        // Assert
        response.EnsureSuccessStatusCode();
        var customer = await response.Content.ReadFromJsonAsync<Customer>();
        Assert.Equal(1, customer.Id);
        Assert.Equal("John Doe", customer.Name);
    }

    [Fact]
    public async Task GetCustomer_ReturnsNotFoundWhenIdDoesNotExist()
    {
        // Arrange
        var token = await LoginAsync("user", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/customers/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostCustomer_AddsNewCustomer()
    {
        // Arrange
        var token = await LoginAsync("user", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var newCustomer = new Customer { Name = "New Customer", Birthday = DateOnly.FromDateTime(new DateTime(2000, 3, 3)), Gender = "Male", Address = "789 Road", Phone = "1112223333", Note1 = "Note1", Note2 = "Note2" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", newCustomer);

        // Assert
        response.EnsureSuccessStatusCode();
        var createdCustomer = await response.Content.ReadFromJsonAsync<Customer>();
        Assert.Equal(3, createdCustomer.Id);
        Assert.Equal("New Customer", createdCustomer.Name);
    }

    [Fact]
    public async Task PutCustomer_UpdatesExistingCustomer()
    {
        // Arrange
        var token = await LoginAsync("user", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Detach the existing entity
        var existingCustomer = await _context.Customers.FindAsync(1);
        if (existingCustomer != null)
        {
            _context.Entry(existingCustomer).State = EntityState.Detached;
        }

        var updatedCustomer = new Customer { Id = 1, Name = "Updated Name", Birthday = DateOnly.FromDateTime(new DateTime(1990, 1, 1)), Gender = "Male", Address = "123 Street", Phone = "1234567890", Note1 = "Note1", Note2 = "Note2" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/customers/1", updatedCustomer);

        // Assert
        response.EnsureSuccessStatusCode();
        var customer = await _context.Customers.FindAsync(1);
        Assert.Equal("Updated Name", customer.Name);
    }

    [Fact]
    public async Task PutCustomer_ReturnsBadRequestWhenIdsDoNotMatch()
    {
        // Arrange
        var token = await LoginAsync("user", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updatedCustomer = new Customer { Id = 2, Name = "Updated Name", Birthday = DateOnly.FromDateTime(new DateTime(1990, 1, 1)), Gender = "Male", Address = "123 Street", Phone = "1234567890", Note1 = "Note1", Note2 = "Note2" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/customers/1", updatedCustomer);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_RemovesCustomerById()
    {
        // Arrange
        var token = await LoginAsync("admin", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Detach the existing entity
        var existingCustomer = await _context.Customers.FindAsync(1);
        if (existingCustomer != null)
        {
            _context.Entry(existingCustomer).State = EntityState.Detached;
        }

        // Act
        var response = await _client.DeleteAsync("/api/customers/1");

        // Assert
        response.EnsureSuccessStatusCode();
        var customer = await _context.Customers.FindAsync(1);
        Assert.Null(customer);
    }

    [Fact]
    public async Task DeleteCustomer_with_WrongRole()
    {
        // Arrange
        var token = await LoginAsync("user", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync("/api/customers/1");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_ReturnsNotFoundWhenIdDoesNotExist()
    {
        // Arrange
        var token = await LoginAsync("admin", "password");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync("/api/customers/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 清理資料庫連線
    public void Dispose()
    {
        _context.Dispose();
    }
}


