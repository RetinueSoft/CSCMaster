namespace CSCMasterAPI.Persistence.Entity
{
    public class Member
    {
        public int Id { get; set; }
        public int DistrictId { get; set; } = 0;
        public int UserId { get; set; } = 0;
        public string District { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string Aadhaar { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public DateTime OnboardDate { get; set; }
        public bool Status { get; set; } = false;
        public string Password { get; set; } = string.Empty;
        public bool LoginAllowed { get; set; } = false;
    }
}
