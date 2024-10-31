namespace CustomerApi.Data
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateOnly Birthday { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
    }
}
