using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net.Http.Json;
using CustomerApi;
using CustomerApi.Data;
using CustomerApi.Controllers;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using CustomerApi.data;
using Microsoft.AspNetCore.TestHost;

namespace CustomerApi.Tests
{
    public class CustomersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        
        private readonly CustomersController _controller;
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _client;
        private readonly string _token;

        private readonly CustomWebApplicationFactory<Program> _factory;
       
        public CustomersControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            // 讀取 .env 文件
            Env.Load();

            // 配置連線字串
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration["MYSQL_CONNECTION_STRING"];

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)))
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new CustomersController(_context);

            // 清除資料庫並插入測試資料
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed the database with test data
            _context.Users.AddRange(new List<User>
            {
             
                new User { Id = 1, Username = "user", Password = "password", Role = "User" },
                new User { Id = 2, Username = "admin", Password = "password", Role = "Admin" },
            });
            _context.Customers.AddRange(new List<Customer>
            {
                new Customer { Id = 1, Name = "John Doe", Birthday = DateOnly.FromDateTime(new DateTime(1990, 1, 1)), Gender = "Male", Address = "123 Street", Phone = "1234567890", Note1 = "Note1", Note2 = "Note2" },
                new Customer { Id = 2, Name = "Jane Doe", Birthday = DateOnly.FromDateTime(new DateTime(1995, 2, 2)), Gender = "Female", Address = "456 Avenue", Phone = "0987654321", Note1 = "Note1", Note2 = "Note2" }
            });
            _context.SaveChanges();

            // 初始化 HttpClient
            //_client = new HttpClient();
            // 獲取 JWT 令牌
            //_token = GetJwtToken0("god", "123456").GetAwaiter().GetResult();
            // 配置 HttpClient
            //_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
        private async Task<string> GetJwtToken0(string username, string password)
        {
            var loginModel = new LoginModel { Username = username, Password = password };
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", loginModel);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<dynamic>(responseString);

            return responseObject.Token;
        }
        private async Task<string> LoginAsync(string username, string password)
        {
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
            loginResponse.EnsureSuccessStatusCode();
            var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
            return tokenResponse.Token;
        }
        private async Task<string> GetJwtToken(string username, string password)
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
            response.EnsureSuccessStatusCode();

            // 使用強類型來解析 JSON 響應
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return tokenResponse.Token;
        }

        [Fact]
        public async Task GetCustomers_ReturnsAllCustomers()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await GetJwtToken("user", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var result = await _controller.GetCustomers();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Customer>>>(result);
            var customers = Assert.IsAssignableFrom<IEnumerable<Customer>>(okResult.Value);
            Assert.Equal(2, customers.Count());
        }

        [Fact]
        public async Task GetCustomer_ReturnsCustomerById()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => { }).CreateClient();
            var token = await GetJwtToken("user", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var result = await _controller.GetCustomer(1);

            // Assert
            var okResult = Assert.IsType<ActionResult<Customer>>(result);
            var customer = Assert.IsType<Customer>(okResult.Value);
            Assert.Equal(1, customer.Id);
            Assert.Equal("John Doe", customer.Name);
        }

        [Fact]
        public async Task GetCustomer_ReturnsNotFoundWhenIdDoesNotExist()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => { }).CreateClient();
            var token = await GetJwtToken("user", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var result = await _controller.GetCustomer(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostCustomer_AddsNewCustomer()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => { }).CreateClient();
            var token = await GetJwtToken("user", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var newCustomer = new Customer { Name = "New Customer", Birthday = DateOnly.FromDateTime(new DateTime(2000, 3, 3)), Gender = "Male", Address = "789 Road", Phone = "1112223333", Note1 = "Note1", Note2 = "Note2" };

            // Act
            var result = await _controller.PostCustomer(newCustomer);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdCustomer = Assert.IsType<Customer>(createdResult.Value);
            Assert.Equal(3, createdCustomer.Id);
            Assert.Equal("New Customer", createdCustomer.Name);
        }

        [Fact]
        public async Task PutCustomer_UpdatesExistingCustomer()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => { }).CreateClient();
            var token = await GetJwtToken("user", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            // Detach the existing entity
            var existingCustomer = await _context.Customers.FindAsync(1);
            if (existingCustomer != null)
            {
                _context.Entry(existingCustomer).State = EntityState.Detached;
            }

            var updatedCustomer = new Customer { Id = 1, Name = "Updated Name", Birthday = DateOnly.FromDateTime(new DateTime(1990, 1, 1)), Gender = "Male", Address = "123 Street", Phone = "1234567890", Note1 = "Note1", Note2 = "Note2" };

            // Act
            var result = await _controller.PutCustomer(1, updatedCustomer);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var customer = await _context.Customers.FindAsync(1);
            
            Assert.Equal("Updated Name", customer.Name);
        }

        [Fact]
        public async Task PutCustomer_ReturnsBadRequestWhenIdsDoNotMatch()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => { }).CreateClient();
            var token = await GetJwtToken("user", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var updatedCustomer = new Customer { Id = 2, Name = "Updated Name", Birthday = DateOnly.FromDateTime(new DateTime(1990, 1, 1)), Gender = "Male", Address = "123 Street", Phone = "1234567890", Note1 = "Note1", Note2 = "Note2" };

            // Act
            var result = await _controller.PutCustomer(1, updatedCustomer);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteCustomer_RemovesCustomerById()
        {
            // Arrange
            //var client = _factory.WithWebHostBuilder(builder => { }).CreateClient();
            //var client = _factory.CreateClient();
            //var token = await GetJwtToken("user", "password");
            //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var token = await LoginAsync("testuser", "password");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            // Detach the existing entity
            var existingCustomer = await _context.Customers.FindAsync(1);
            if (existingCustomer != null)
            {
                _context.Entry(existingCustomer).State = EntityState.Detached;
            }
            
            // Act
            var result = await _controller.DeleteCustomer(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var customer = await _context.Customers.FindAsync(1);
            Assert.Null(customer);
        }
        [Fact]
        public async Task DeleteCustomer_with_wrongRole()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => { }).CreateClient();
            var token = await GetJwtToken("user", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Detach the existing entity
            //var existingCustomer = await _context.Customers.FindAsync(1);
            //if (existingCustomer != null)
            //{
            //    _context.Entry(existingCustomer).State = EntityState.Detached;
            //}

            // Act
            var result = await _controller.DeleteCustomer(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var customer = await _context.Customers.FindAsync(1);
            Assert.Null(customer);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNotFoundWhenIdDoesNotExist()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder => { }).CreateClient();
            var token = await GetJwtToken("admin", "password");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var result = await _controller.DeleteCustomer(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }

    // 定義 TokenResponse 類別
    public class TokenResponse
    {
        public string Token { get; set; }
    }
}

