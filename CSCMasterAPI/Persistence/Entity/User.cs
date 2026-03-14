namespace CSCMasterAPI.Persistence.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Aadhaar { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
        public string Password { get; set; } = string.Empty;
        public bool LoginAllowed { get; set; } = false;
    }
}
