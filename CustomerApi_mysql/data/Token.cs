using CustomerApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Data
{
    public class Token
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string TokenValue { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}

