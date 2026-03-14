namespace CSCMasterAPI.Persistence.Entity
{
    public class District
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UserDistrict
    {
        public int UserId { get; set; }
        public int DistrictId { get; set; }
    }
}
