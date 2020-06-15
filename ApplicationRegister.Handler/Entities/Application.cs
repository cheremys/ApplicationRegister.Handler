namespace ApplicationRegister.Handler.Entities
{
    public class Application
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public string DepartmentAddress { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public string Ip { get; set; }
    }
}
