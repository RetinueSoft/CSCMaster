using CSCMasterAPI.Models;
using CSCMasterAPI.Persistence;
using CSCMasterAPI.Persistence.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSCMasterAPI.Persistence.Service
{
    public interface IEnrolmentService
    {
        public MemberModel GetMember(string stationID);
        public List<DistrictEntrollmentModel> GetUserDistrict(int userId, DateTime fromDate, DateTime toDate);
        public List<DistrictEntrollmentModel> GetMemberDistrict(int memberId, DateTime fromDate, DateTime toDate);
        public List<MemberEntrollmentModel> GetMembers(int userId, int districtId, DateTime fromDate, DateTime toDate);
        public List<MemberEntrollmentModel> GetMembers(int memberId, DateTime fromDate, DateTime toDate);
        public List<EntrollmentReportModel> GetEnrolments(int memberId, DateTime fromDate, DateTime toDate);
        public void Override(int memberId, List<EntrollmentImportModel> entrollments);
        public void Append(int memberId, List<EntrollmentImportModel> entrollments);
    }
    public class EnrolmentService : IEnrolmentService
    {
        private readonly ApplicationDbContext _dbContext;

        public EnrolmentService(IServiceProvider serviceProvider)
        {
            _dbContext = (ApplicationDbContext)serviceProvider.GetService(typeof(ApplicationDbContext));
        }
        public void Override(int memberId, List<EntrollmentImportModel> entrollments)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                entrollments.Select(x=>new { x.EIDDate, x.Type }).Distinct().ToList().ForEach(x =>
                {
                    var existingEntrollments = _dbContext.Entrollment
                    .Where(e => e.MemberId == memberId && e.EIDDate.Date == x.EIDDate.Date && e.Type == x.Type)
                    .ToList();
                    var ss = _dbContext.Entrollment.Select(s => s.EIDDate.Date).Distinct().ToList();
                    _dbContext.Entrollment.RemoveRange(existingEntrollments);
                });

                Append(memberId, entrollments);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public void Append(int memberId, List<EntrollmentImportModel> entrollments)
        {
            using var transaction = _dbContext.Database.CurrentTransaction == null
                ? _dbContext.Database.BeginTransaction()
                : null;
            try
            {
                if (entrollments == null || entrollments.Count < 1)
                    throw new Exception("No data found to upload.");

                var member = GetMember(memberId);
                if (member == null)
                    throw new Exception("Invalid Member or Member not found.");

                if (entrollments.Select(x => x.OperatorAadhaar).Distinct().Count() > 1)
                    throw new Exception("Multiple Operator Aadhaar found in the uploaded data. Please upload data for single operator at a time.");

                //if (member.Aadhaar != entrollments.First().OperatorAadhaar)
                //    throw new Exception("Operator aadhaar and excel aadhaar are diffrent, cannot import.");

                var newEntrollments = entrollments.Select(e => new Entrollment
                {
                    MemberId = memberId,
                    DistrictId = member.DistrictId,
                    District = member.District,
                    UserId = member.UserId,
                    StationId = member.StationId,
                    Block = member.Block,
                    EID = e.EID,
                    EIDDate = e.EIDDate,
                    ChildName = e.ChildName,
                    Type = e.Type,
                    Uploaded = e.Uploaded == 1,
                    UploadedDate = DateTime.Now,
                    OperatorAadhaar = e.OperatorAadhaar,
                    Status = e.Status,
                    Amount = e.Type == "U" || e.Type == "BU" ?  e.AMOUNT_CHARGED_FOR_UPDATE_ENROLMENT : e.AMOUNT_CHARGED_FOR_NEW_ENROLMENT,
                    GST = e.GST_AMOUNT,
                    Total = e.TOTAL_AMOUNT_CHARGED,
                    ProcessingStateDescription = e.PROCESSING_STATE_DESCRIPTION,
                    RejectReasonDescription = e.REJECT_REASON_DESCRIPTION,
                    PacketSkipped = e.PACKET_SKIPPED,
                    ForeignResident = e.FOREIGN_RESIDENT
                    //CC = e.CC,
                }).ToList();

                newEntrollments.Select(e =>
                {
                    e.Type = e.Type == "BU" && e.Total == 0 ? e.Type : "BUC"; //Bio metric for child
                    e.Type = e.Type == "U" && e.Total == 125 ? e.Type : "BUN"; //It should be Bu but why it is U is Unkown so BUN

                    if (e.Type == "M")
                    {
                        e.Total = 75;
                        e.Amount = 63.56m;
                        e.GST = 11.44m;
                    }

                    //Some excel not having proper GST amount insted it is comming %
                    var calculatedGst = e.Total - e.Amount;
                    e.GST = Math.Abs(e.GST - calculatedGst) > 0.50m ? calculatedGst : e.Amount == 0 ? 0 : e.GST;
                    return e;
                }).ToList();

                _dbContext.Entrollment.AddRange(newEntrollments);
                _dbContext.SaveChanges();

                transaction?.Commit();
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
        }
        private Member? GetMember(int memberId) => _dbContext.Member.FirstOrDefault(x => x.Id == memberId);
        public MemberModel GetMember(string stationID)
        {
            #pragma warning disable CS8603 // Possible null reference return.
            return _dbContext.Member.Where(x => x.StationId == stationID).Select(x => new MemberModel()
            {
                Block = x.Block,
                District = x.District,
                Id = x.Id,
                Mobile = x.Phone,
                Name = x.Name
            }).FirstOrDefault();
            #pragma warning restore CS8603 // Possible null reference return.
        }
        public List<DistrictEntrollmentModel> GetUserDistrict(int userId, DateTime fromDate, DateTime toDate)
        {
            var entrolQry = _dbContext.Entrollment.Where(x => x.EIDDate.Date >= fromDate.Date && x.EIDDate.Date <= toDate.Date);
            var userDetailQry = from ud in _dbContext.UserDistrict
                                join d in _dbContext.District on ud.DistrictId equals d.Id
                                where ud.UserId == userId
                                select new DistrictEntrollmentModel()
                                {
                                    Id = d.Id,
                                    Name = d.Name,
                                    UserCount = _dbContext.Member.Count(x=>x.DistrictId == d.Id),
                                    UserEntrolledCount = entrolQry.Where(x => x.DistrictId == d.Id).Select(x=>x.MemberId).Distinct().Count(),
                                    ChildCount = entrolQry.Count(x => x.DistrictId == d.Id && x.Type == "C"),
                                    MobileCount = entrolQry.Count(x => x.DistrictId == d.Id && x.Type == "M"),
                                    BioUpdateCount = entrolQry.Count(x => x.DistrictId == d.Id && (x.Type == "BU" || x.Type == "BUC" || x.Type == "BUN")),
                                    NewCount = entrolQry.Count(x => x.DistrictId == d.Id && x.Type == "N"),
                                    UpdateCount = entrolQry.Count(x => x.DistrictId == d.Id && x.Type == "U"),
                                    TotalCount = entrolQry.Count(x => x.DistrictId == d.Id),

                                    ChildAmount = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "C").Sum(x=>x.Amount),
                                    MobileAmount = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "M").Sum(x => x.Amount),
                                    BioUpdateAmount = entrolQry.Where(x => x.DistrictId == d.Id && (x.Type == "BU" || x.Type == "BUC" || x.Type == "BUN")).Sum(x => x.Amount),
                                    NewAmount = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "N").Sum(x => x.Amount),
                                    UpdateAmount = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "U").Sum(x => x.Amount),
                                    TotalTypeAmount = entrolQry.Where(x => x.DistrictId == d.Id).Sum(x => x.Amount),

                                    ChildGST = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "C").Sum(x => x.GST),
                                    MobileGST = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "M").Sum(x => x.GST),
                                    BioUpdateGST = entrolQry.Where(x => x.DistrictId == d.Id && (x.Type == "BU" || x.Type == "BUC" || x.Type == "BUN")).Sum(x => x.GST),
                                    NewGST = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "N").Sum(x => x.GST),
                                    UpdateGST = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "U").Sum(x => x.GST),
                                    TotalTypeGST = entrolQry.Where(x => x.DistrictId == d.Id).Sum(x => x.GST),

                                    ChildTotal = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "C").Sum(x => x.Total),
                                    MobileTotal = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "M").Sum(x => x.Total),
                                    BioUpdateTotal = entrolQry.Where(x => x.DistrictId == d.Id && (x.Type == "BU" || x.Type == "BUC" || x.Type == "BUN")).Sum(x => x.Total),
                                    NewTotal = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "N").Sum(x => x.Total),
                                    UpdateTotal = entrolQry.Where(x => x.DistrictId == d.Id && x.Type == "U").Sum(x => x.Total),
                                    TotalTypeTotal = entrolQry.Where(x => x.DistrictId == d.Id).Sum(x => x.Total),
                                };

            return userDetailQry.ToList();
        }
        public List<DistrictEntrollmentModel> GetMemberDistrict(int memberId, DateTime fromDate, DateTime toDate)
        {
            var entrolQry = _dbContext.Entrollment.Where(x => x.EIDDate.Date >= fromDate.Date && x.EIDDate.Date <= toDate.Date);
            var userDetailQry = from m in _dbContext.Member
                                join d in _dbContext.District on m.DistrictId equals d.Id
                                where m.UserId == memberId
                                select new DistrictEntrollmentModel()
                                {
                                    Id = d.Id,
                                    Name = d.Name,

                                    UserCount = _dbContext.UserDistrict.Count(x => x.DistrictId == m.Id),
                                    UserEntrolledCount = entrolQry.Where(x => x.MemberId == m.Id).Select(x => x.MemberId).Distinct().Count(),
                                    ChildCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "C"),
                                    MobileCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "M"),
                                    BioUpdateCount = entrolQry.Count(x => x.MemberId == m.Id && (x.Type == "BU" || x.Type == "BUC" || x.Type == "BUN")),
                                    NewCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "N"),
                                    UpdateCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "U"),
                                    TotalCount = entrolQry.Count(x => x.MemberId == m.Id),

                                    ChildAmount = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "C").Sum(x => x.Amount),
                                    MobileAmount = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "M").Sum(x => x.Amount),
                                    BioUpdateAmount = entrolQry.Where(x => x.MemberId == m.Id && (x.Type == "BU" || x.Type == "BUC" || x.Type == "BUN")).Sum(x => x.Amount),
                                    NewAmount = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "N").Sum(x => x.Amount),
                                    UpdateAmount = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "U").Sum(x => x.Amount),
                                    TotalTypeAmount = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => x.Amount),

                                    ChildGST = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "C").Sum(x => x.GST),
                                    MobileGST = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "M").Sum(x => x.GST),
                                    BioUpdateGST = entrolQry.Where(x => x.MemberId == m.Id && (x.Type == "BU" || x.Type == "BUC" || x.Type == "BUN")).Sum(x => x.GST),
                                    NewGST = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "N").Sum(x => x.GST),
                                    UpdateGST = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "U").Sum(x => x.GST),
                                    TotalTypeGST = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => x.GST),

                                    ChildTotal = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "C").Sum(x => x.Total),
                                    MobileTotal = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "M").Sum(x => x.Total),
                                    BioUpdateTotal = entrolQry.Where(x => x.MemberId == m.Id && (x.Type == "BU" || x.Type == "BUC" || x.Type == "BUN")).Sum(x => x.Total),
                                    NewTotal = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "N").Sum(x => x.Total),
                                    UpdateTotal = entrolQry.Where(x => x.MemberId == m.Id && x.Type == "U").Sum(x => x.Total),
                                    TotalTypeTotal = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => x.Total),
                                };

            return userDetailQry.ToList();
        }
        public List<MemberEntrollmentModel> GetMembers(int userId, int districtId, DateTime fromDate, DateTime toDate)
        {
            var entrolQry = _dbContext.Entrollment.Where(x => x.EIDDate.Date >= fromDate.Date && x.EIDDate.Date <= toDate.Date);
            var userDetailQry = from ud in _dbContext.UserDistrict
                                join m in _dbContext.Member on new { ud.UserId, ud.DistrictId } equals new { UserId = m.UserId, DistrictId = m.DistrictId }
                                where (userId == 1 || ud.UserId == userId) && ud.DistrictId == districtId
                                select new MemberEntrollmentModel()
                                {
                                    Id = m.Id,
                                    Name = m.Name,
                                    Block = m.Block,
                                    District = m.District,
                                    Mobile = m.Phone,
                                    StationId = m.StationId,
                                    ChildCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "C"),
                                    MobileCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "M"),
                                    BioUpdateCount = entrolQry.Count(x => x.MemberId == m.Id && (x.Type == "BU" || x.Type == "BUC" || x.Type == "BUN")),
                                    NewCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "N"),
                                    UpdateCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "U"),
                                    TotalCount = entrolQry.Count(x => x.MemberId == m.Id),
                                    Amount = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => (decimal?)x.Amount) ?? 0,
                                    GST = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => (decimal?)x.GST) ?? 0,
                                    Total = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => (decimal?)x.Total) ?? 0,
                                    CC = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => (decimal?)x.CC) ?? 0,
                                };

            return userDetailQry.ToList();
        }
        public List<MemberEntrollmentModel> GetMembers(int memberId, DateTime fromDate, DateTime toDate)
        {
            var entrolQry = _dbContext.Entrollment.Where(x => x.EIDDate.Date >= fromDate.Date && x.EIDDate.Date <= toDate.Date);
            var userDetailQry = from m in GetMemberQry(fromDate, toDate)
                                where m.Id == memberId
                                select m;

            return userDetailQry.ToList();
        }
        private IQueryable<MemberEntrollmentModel> GetMemberQry(DateTime fromDate, DateTime toDate)
        {
            var entrolQry = _dbContext.Entrollment.Where(x => x.EIDDate.Date >= fromDate.Date && x.EIDDate.Date <= toDate.Date);

            return from m in _dbContext.Member
            select new MemberEntrollmentModel()
            {
                Id = m.Id,
                Name = m.Name,
                Block = m.Block,
                District = m.District,
                Mobile = m.Phone,
                ChildCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "C"),
                MobileCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "M"),
                BioUpdateCount = entrolQry.Count(x => x.MemberId == m.Id && (x.Type == "BU" || x.Type == "BUC" || x.Type == "BUN")),
                NewCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "N"),
                UpdateCount = entrolQry.Count(x => x.MemberId == m.Id && x.Type == "U"),
                TotalCount = entrolQry.Count(x => x.MemberId == m.Id),
                Amount = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => (decimal?)x.Amount) ?? 0,
                GST = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => (decimal?)x.GST) ?? 0,
                Total = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => (decimal?)x.Total) ?? 0,
                CC = entrolQry.Where(x => x.MemberId == m.Id).Sum(x => (decimal?)x.CC) ?? 0,
            };
        }
        public List<EntrollmentReportModel> GetEnrolments(int memberId, DateTime fromDate, DateTime toDate)
        {
            var userDetailQry = from e in _dbContext.Entrollment
                                where e.MemberId == memberId && e.EIDDate.Date >= fromDate.Date && e.EIDDate.Date <= toDate.Date
                                select new EntrollmentReportModel()
                                {
                                    Type = e.Type,
                                    ChildName = e.ChildName,
                                    EID = e.EID,
                                    EIDDate = e.EIDDate,
                                    UploadedDate = e.UploadedDate,
                                    Amount = e.Amount,
                                    GST = e.GST,
                                    Total = e.Total,
                                    CC = e.CC
                                };

            return userDetailQry.ToList();
        }
    }
}
