namespace CSCMasterAPI.Persistence.Entity
{
    public class Entrollment
    {
        public int Id { get; set; }
        public DateTime UploadedDate { get; set; }
        public int MemberId { get; set; }
        public int DistrictId { get; set; }
        public int UserId { get; set; }
        public string StationId { get; set; }
        public string District { get; set; }
        public string Block { get; set; }
        public string EID { get; set; }
        public DateTime EIDDate { get; set; }
        public string? ChildName { get; set; }
        public string Type { get; set; }
        public bool? Uploaded { get; set; }
        public string? Status { get; set; }
        public string? OperatorAadhaar { get; set; }
        public decimal Amount { get; set; }
        public decimal GST { get; set; }
        public decimal Total { get; set; }
        public decimal CC { get; set; }
        public string? ProcessingStateDescription { get; set; }
        public string? RejectReasonDescription { get; set; }
        public string? PacketSkipped { get; set; }
        public string? ForeignResident { get; set; }
    }
}
