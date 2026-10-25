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
using System.IO;

namespace CSCMasterAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PettyCashController : ControllerBase
    {
        private readonly IPettyCashService _pettyCashService;
        public PettyCashController(IPettyCashService pettyCashService)
        {
            _pettyCashService = pettyCashService;
        }
        [HttpGet("MobileNumberTypes")]
        public IActionResult MobileNumberTypes() =>
                       new JsonResult(new ActionResponse()
                       {
                           SucessValue = Enum.GetValues(typeof(MobileNoType)).Cast<MobileNoType>()
                                             .Select(x => new { Id = (int)x, Value = x.ToString() }).ToList()
                       });

        [HttpGet("TransactionTypes")]
        public IActionResult TransactionTypes() =>
                        new JsonResult(new ActionResponse()
                        {
                            SucessValue = Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>()
                                              .Select(x => new { Id = (int)x, Value = x.ToString() }).ToList()
                        });

        [HttpGet("Paymodes")]
        public IActionResult Paymodes() =>
                        new JsonResult(new ActionResponse()
                        {
                            SucessValue = Enum.GetValues(typeof(PaymentMode)).Cast<PaymentMode>()
                                              .Select(x => new { Id = (int)x, Value = x.ToString() }).ToList()
                        });

        [HttpGet("Items")]
        public IActionResult Items([FromQuery] string? searchBy) =>
                new JsonResult(new ActionResponse() { SucessValue = _pettyCashService.GetItems() });

        [HttpPost("CreateItem")]
        public IActionResult CreateItem(Item item) =>
                new JsonResult(new ActionResponse() { SucessValue = _pettyCashService.CreateItem(item) });

        [HttpPut("EditItem")]
        public IActionResult UpdateItem(Item item) =>
                new JsonResult(new ActionResponse() { SucessValue = _pettyCashService.EditItem(item) });

        [HttpGet("Customers")]
        public IActionResult Customers([FromQuery] string? searchBy) =>
               new JsonResult(new ActionResponse() { SucessValue = _pettyCashService.GetCustomers(searchBy) });

        [HttpPost("CreateCustomer")]
        public IActionResult CreateCustomer(CustomerModel customer) =>
                new JsonResult(new ActionResponse() { SucessValue = _pettyCashService.CreateCustomer(customer) });

        [HttpPut("EditCustomer")]
        public IActionResult UpdateCustomer(CustomerModel customer) =>
                new JsonResult(new ActionResponse() { SucessValue = _pettyCashService.EditCustomer(customer) });

        [HttpPost("All")]
        public IActionResult Accounts([FromBody] PettyCashGetAllModel input) =>
               new JsonResult(new ActionResponse() { SucessValue = _pettyCashService.GetAll(input?.CustomerId, input?.FromDate, input?.ToDate, input.PageIndex, input.PageSize.Value) });

        [HttpPost("ListHeader")]
        public IActionResult ListHeader([FromBody] PettyCashGetAllModel input) =>
                new JsonResult(new ActionResponse() { SucessValue = _pettyCashService.GetHeader(input?.CustomerId, input?.FromDate, input?.ToDate) });

        [HttpPost("Create")]
        public IActionResult Create(PettyCashAccountModel account)
        {
            _pettyCashService.Add(account);
            return new JsonResult(new ActionResponse());
        }
        [HttpPost("Edit")]
        public IActionResult Edit(PettyCashAccountListModel account)
        {
            _pettyCashService.Edit(account.Id, account);
            return new JsonResult(new ActionResponse());
        }
        [HttpDelete("Delete")]
        public IActionResult Delete(int accountId)
        {
            _pettyCashService.Delete(accountId);
            return new JsonResult(new ActionResponse());
        }
    }
}
