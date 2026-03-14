using CSCMasterAPI.Models;
using CSCMasterAPI.Persistence.Entity;
using CSCMasterAPI.Persistence.Service;
using CSCMasterAPI.Utils;
using HtmlAgilityPack;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace CSCMasterAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly IEnrolmentService _enrolmentService;
        public EnrolmentsController(IEnrolmentService enrolmentService)
        {
            _enrolmentService = enrolmentService;
        }

        [HttpPost("MyDistricts")]
        public IActionResult MyDistricts([FromBody] MyDistrictEnrolmentModel input)
        {
            int userId = FindUserId(out var role);

            if (role == Role.User)
                return new JsonResult(new ActionResponse() { SucessValue = _enrolmentService.GetUserDistrict(userId, input.FromDate, input.ToDate) });
            else
                return new JsonResult(new ActionResponse() { SucessValue = _enrolmentService.GetMemberDistrict(userId, input.FromDate, input.ToDate) });
        }

        [HttpPost("MyDistrictMembers")]
        public IActionResult MyDistrictMembers([FromBody] MyDistrictMemberEnrolmentModel input)
        {
            int userId = FindUserId(out var role);
            if (role == Role.User)
                return new JsonResult(new ActionResponse() { SucessValue = _enrolmentService.GetMembers(userId, input.DistrictId, input.FromDate, input.ToDate) });
            else
                return new JsonResult(new ActionResponse() { SucessValue = _enrolmentService.GetMembers(userId, input.FromDate, input.ToDate) });
        }

        [HttpPost("MyMemberEnrolments")]
        public IActionResult MyMemberEnrolments([FromBody] MyMemberEnrolmentModel input)
        {
            return new JsonResult(new ActionResponse() { SucessValue = _enrolmentService.GetEnrolments(input.MemberId, input.FromDate, input.ToDate) });
        }

        [HttpGet("GetMember")]
        public IActionResult GetMember([FromQuery] string stationID)
        {
            int userId = FindUserId(out var role);
            var stationIdClaim = User.FindFirst("StationId");
            if (role != Role.User && stationIdClaim.Value != stationID)
            {
                var res = new ActionResponse();
                res.AddError("Wrong stationId, Cannot find");
                return new JsonResult(res);
            }
            return new JsonResult(new ActionResponse() { SucessValue = _enrolmentService.GetMember(stationID) });
        }
        [Consumes("multipart/form-data")]
        [HttpPost("UploadEnrolmentData")]
        public async Task<IActionResult> UploadEnrolmentData([FromForm] UploadEnrolmentRequest request)
        {
            var response = new ActionResponse();

            if (request.File == null || request.File.Length <= 0)
                return BadRequest("Invalid file, file not uploaded.");

            var memberObj = JsonSerializer.Deserialize<MemberModel>(
                request.Member,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var importModelList = new List<EntrollmentImportModel>();

            if (request.File.ContentType == "application/x-zip-compressed" || request.File.ContentType == "application/zip")
            {

                //using var outerStream = new MemoryStream();
                //request.File.CopyTo(outerStream);
                //outerStream.Position = 0;

                string zipPassword = "123";
                using var zipStream = request.File.OpenReadStream();
                using var zipFile = new ZipFile(zipStream);
                zipFile.Password = zipPassword;
                for (int i = 0; i < zipFile.Count; i++)
                {
                    var entry = zipFile[i] as ZipEntry;
                    if (entry != null && entry.IsFile && entry.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        using var entryStream = zipFile.GetInputStream(entry);
                        var importModel = entryStream.ReadCSVToObject<EntrollmentImportModel>();
                        importModelList.AddRange(importModel);
                    }
                }
            }
            else
            {
                var stream = request.File.OpenReadStream();
                var importModel = stream.ReadCSVToObject<EntrollmentImportModel>();
                importModelList.AddRange(importModel);
            }

            if (importModelList.Count <= 0)
            {
                var res = new ActionResponse();
                res.AddError("Invalid file, file not uploaded.");
                return new JsonResult(res);
            }
            if (importModelList.Any(x => x.EID.IsNullOrEmpty()))
            {
                var res = new ActionResponse();
                res.AddError("Invalid file/file format, file not uploaded");
                return new JsonResult(res);
            }

            var reportData = importModelList.Select(x =>
            {
                int index = x.EID.LastIndexOf(' ');
                if (DateTime.TryParseExact(x.EID.Substring(x.EID.Length - 19), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    x.EIDDate = parsedDate;
                else
                {
                    var digits = new string(x.EID.Where(char.IsDigit).ToArray());
                    if (digits.Length >= 14)
                    {
                        var dateTimeString = digits.Substring(digits.Length - 14);
                        x.EIDDate = DateTime.ParseExact(dateTimeString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    }
                }
                x.UploadedDate = DateTime.Now;
                x.Type = x.Type.IsNullOrEmpty() ? (x.ChildName.IsNullOrEmpty() ? "M" : "C") : x.MANDATORY_BIO_METRIC_UPDATE_ONLY == "Yes" ? "BU" :  x.Type;
                return x;
            }).ToList();
            _enrolmentService.Override(memberObj.Id, reportData);
            return new JsonResult(new ActionResponse());
        }

        private int FindUserId(out Role role)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            role = Role.Unknown;
            if (roleClaim != null && Enum.TryParse<Role>(roleClaim.Value, out var parsedRole))
                role = parsedRole;

            if (role == Role.User)
            {
                var userIdClaim = User.FindFirst("UserId");
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            }
            else if (role == Role.Member)
            {
                var userIdClaim = User.FindFirst("MemberId");
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            }
            return 0;
        }
    }
}
