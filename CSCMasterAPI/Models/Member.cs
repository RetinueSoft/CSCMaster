using System.Reflection;

namespace CSCMasterAPI.Models
{
    public class LoginModel
    {
        public string Mobile { get; set; }
        public string Password { get; set; }
    }

    public class TokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class TokenResponse : TokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public LoginSettings Settings { get; set; }
        public UserModel User { get; set; }
    }

    public class LoginSettings
    {
        public string CurrencySymbole { get; set; }
        public string DateTimeFormat { get; set; }
        public byte[] DefaultImg { get; set; }
    }

    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string StationId { get; set; }
        public Role Role { get; set; }
    }

    public class DistrictEntrollmentModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserCount { get; set; }
        public int UserEntrolledCount { get; set; }
        public int ChildCount { get; set; }
        public int MobileCount { get; set; }
        public int BioUpdateCount { get; set; }
        public int UpdateCount { get; set; }
        public int NewCount { get; set; }
        public int TotalCount { get; set; }

        public decimal ChildAmount { get; set; }
        public decimal MobileAmount { get; set; }
        public decimal BioUpdateAmount { get; set; }
        public decimal UpdateAmount { get; set; }
        public decimal NewAmount { get; set; }
        public decimal TotalTypeAmount { get; set; }

        public decimal ChildGST { get; set; }
        public decimal MobileGST { get; set; }
        public decimal BioUpdateGST { get; set; }
        public decimal UpdateGST { get; set; }
        public decimal NewGST { get; set; }
        public decimal TotalTypeGST { get; set; }

        public decimal ChildTotal { get; set; }
        public decimal MobileTotal { get; set; }
        public decimal BioUpdateTotal { get; set; }
        public decimal UpdateTotal { get; set; }
        public decimal NewTotal { get; set; }
        public decimal TotalTypeTotal { get; set; }
    }

    public class MemberModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Block { get; set; }
        public string District { get; set; }
        public string Mobile { get; set; }
        public string StationId { get; set; }
    }

    public class MemberEntrollmentModel : MemberModel
    {
        public int ChildCount { get; set; }
        public int MobileCount { get; set; }
        public int BioUpdateCount { get; set; }
        public int UpdateCount { get; set; }
        public int NewCount { get; set; }
        public int TotalCount { get; set; }
        public decimal Amount { get; set; }
        public decimal GST { get; set; }
        public decimal Total { get; set; }
        public decimal CC { get; set; }
    }
    public class EntrollmentReportModel
    {
        public DateTime UploadedDate { get; set; }
        public string EID { get; set; }
        public DateTime EIDDate { get; set; }
        public string ChildName { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal GST { get; set; }
        public decimal Total { get; set; }
        public decimal CC { get; set; }
    }
    public class EntrollmentImportModel : EntrollmentReportModel
    {        
        public int Uploaded { get; set; }
        public string Status { get; set; }
        public string OperatorAadhaar { get; set; }
        public string MANDATORY_BIO_METRIC_UPDATE_ONLY { get; set; }
        public string IS_NRI { get; set; }
        public string OPERATOR_ID { get; set; }
        public decimal GST_AMOUNT { get; set; }
        public decimal AMOUNT_CHARGED_FOR_NEW_ENROLMENT { get; set; }
        public decimal AMOUNT_CHARGED_FOR_UPDATE_ENROLMENT { get; set; }
        public decimal TOTAL_AMOUNT_CHARGED { get; set; }
        public string PROCESSING_STATE_DESCRIPTION { get; set; }
        public string REJECT_REASON_DESCRIPTION { get; set; }
        public string PACKET_SKIPPED { get; set; }
        public string FOREIGN_RESIDENT { get; set; }
    }

    public class MyDistrictEnrolmentModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class MyDistrictMemberEnrolmentModel: MyDistrictEnrolmentModel
    {
        public int DistrictId { get; set; }
    }
    public class MyMemberEnrolmentModel : MyDistrictEnrolmentModel
    {
        public int MemberId { get; set; }
    }
    public class UploadEnrolmentRequest
    {
        public string Member { get; set; }
        public DateTime Date { get; set; }
        public IFormFile File { get; set; }
    }

    public enum Role
    {
        Unknown = 0,
        User,
        Member,
    }
}
