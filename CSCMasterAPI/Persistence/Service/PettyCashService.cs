using Azure.Core;
using CSCMasterAPI.Models;
using CSCMasterAPI.Persistence;
using CSCMasterAPI.Persistence.Entity;
using CSCMasterAPI.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CSCMasterAPI.Persistence.Service
{
    public interface IPettyCashService
    {
        public Item CreateItem(Item customer);
        public Item EditItem(Item customer);
        public List<Item> GetItems();

        public CustomerModel CreateCustomer(CustomerModel customer);
        public CustomerModel EditCustomer(CustomerModel customer);
        public List<CustomerModel> GetCustomers(string searchBy);

        public void Add(PettyCashAccountModel pettyCashAccount);
        public void Edit(int accountId, PettyCashAccountModel pettyCashAccount);
        public void Delete(int pettyCashAccountId);
        public PaginatedList<PettyCashAccountListModel> GetAll(int? customerId, DateTime? fromDate, DateTime? toDate, int pageIndex, int pageSize);
        public PettyCashAccountHeaderModel GetHeader(int? customerId, DateTime? fromDate, DateTime? toDate);
    }
    public class PettyCashService : IPettyCashService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoginContext _loginContext;
        public PettyCashService(IServiceProvider serviceProvider)
        {
            _dbContext = (ApplicationDbContext)serviceProvider.GetService(typeof(ApplicationDbContext));
            _loginContext = serviceProvider.GetRequiredService<ILoginContext>();
            //_loginContext = (LoginContext)serviceProvider.GetService(typeof(LoginContext));
        }
        
        public CustomerModel CreateCustomer(CustomerModel customer)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var cust = new Customer()
                {
                    Name = customer.Name,
                    Address = customer.Address,
                    ContactInfoJSon = JsonSerializer.Serialize<List<MobileModel>>(customer.ContactNumber),
                    CompanyId = _loginContext.CompanyId,
                    BranchId = _loginContext.BranchId,
                };
                _dbContext.Customer.Add(cust);
                _dbContext.SaveChanges();
                customer.Id = cust.Id;
                transaction.Commit();
                return customer;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public CustomerModel EditCustomer(CustomerModel customer)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var cust = GetCustomer(customer.Id);
                cust.Name = customer.Name;
                cust.ContactInfoJSon = JsonSerializer.Serialize<List<MobileModel>>(customer.ContactNumber);
                _dbContext.Customer.Update(cust);
                _dbContext.SaveChanges();
                transaction.Commit();
                return customer;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        private Customer? GetCustomer(int customerId) => _dbContext.Customer.FirstOrDefault(x => x.Id == customerId && x.CompanyId == _loginContext.CompanyId && x.BranchId == _loginContext.BranchId);
        public List<CustomerModel> GetCustomers(string searchBy) =>
            _dbContext.Customer.AsNoTracking().Where(x=> x.CompanyId == _loginContext.CompanyId && x.BranchId == _loginContext.BranchId)
            .Where(x => searchBy.IsNullOrEmpty() || x.ContactInfoJSon.Contains(searchBy) || x.Name.Contains(searchBy))
            .Select(x => new CustomerModel()
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                ContactNumber = JsonSerializer.Deserialize<List<MobileModel>>(x.ContactInfoJSon,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            }).ToList();

        public Item CreateItem(Item item)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var itm = new Item() { Name = item.Name,  MRP= item.MRP, TransactionType = item.TransactionType };
                _dbContext.Item.Add(itm);
                _dbContext.SaveChanges();
                item.Id = itm.Id;
                transaction.Commit();
                return item;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public Item EditItem(Item item)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var itm = GetItem(item.Id);
                itm.Name = item.Name;
                itm.MRP = item.MRP;
                _dbContext.Item.Update(itm);
                _dbContext.SaveChanges();
                transaction.Commit();
                return item;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        private Item? GetItem(int itemId) => _dbContext.Item.FirstOrDefault(x => x.Id == itemId);
        public List<Item> GetItems() => _dbContext.Item.AsNoTracking().ToList();

        public void Add(PettyCashAccountModel pettyCashAccount)
        {
            if (pettyCashAccount == null || pettyCashAccount.Items == null || pettyCashAccount.Items.Count < 1)
                throw new Exception("No data found to upload.");

            //var customer = GetCustomer(pettyCashAccount.CustomerId);
            if (pettyCashAccount.Customer == null)
                throw new Exception("Invalid Customer or Customer not found.");

            if (pettyCashAccount.Date > DateTime.Now)
                throw new Exception("Future date transaction cannot possible");

            if (pettyCashAccount.Items == null || pettyCashAccount.Items.Count == 0)
                throw new Exception("Without item cannot save");

            if (pettyCashAccount.Items.Any(x=>x.Item == null))
                throw new Exception("Some of the item is empty");

            if (pettyCashAccount.Items.Any(x=>x.Quantity == 0))
                throw new Exception("All Item Qty should be grater than 0");

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var items = pettyCashAccount.Items.Select(x => new PettyCashAccountItem()
                {
                    ItemId = x.Item.Id,
                    MRP = x.MRP,
                    Quantity = x.Quantity,
                    Discount = x.Discount,
                    Notes = x.Notes
                }).ToList();

                var billCharges = pettyCashAccount.Adjustments.Where(x => x.TransactionType == TransactionType.Credit).Sum(x => x.Amount);
                var billDiscount = pettyCashAccount.Adjustments.Where(x => x.TransactionType == TransactionType.Debit).Sum(x => x.Amount);

                var account = new PettyCashAccount()
                {
                    CompanyId = _loginContext.CompanyId,
                    BranchId = _loginContext.BranchId,
                    UserId = _loginContext.UserId,
                    Date = pettyCashAccount.Date,
                    CustomerId = pettyCashAccount.Customer.Id,
                    Notes = pettyCashAccount.Notes,
                    TransactionType = pettyCashAccount.TransactionType,
                    PaymentJSon = JsonSerializer.Serialize<List<PaymentModel>>(pettyCashAccount.Payment),
                    BillAdustmentsJSon = JsonSerializer.Serialize<List<BillAdjustments>>(pettyCashAccount.Adjustments),
                    PaidAmount = pettyCashAccount.Payment.Where(x => Enum.IsDefined(typeof(PaymentMode), x.PaymentType)).Sum(x => x.Amount),
                    CashAmount = pettyCashAccount.Payment.Where(x => x.PaymentType == PaymentMode.Cash).Sum(x => x.Amount),
                    UPIAmount = pettyCashAccount.Payment.Where(x => x.PaymentType == PaymentMode.UPI).Sum(x => x.Amount),
                    BillCharges = billCharges,
                    BillDiscount = billDiscount,
                    TotalLineAmount = items.Sum(x => x.MRP),
                    TotalLineDiscount = items.Sum(x => x.Discount),
                    NetAmount = items.Sum(x => x.Total) + (billCharges - billDiscount)
                };
                _dbContext.PettyCashAccount.Add(account);
                _dbContext.SaveChanges();

                items.ForEach(i => i.AccountId = account.Id);

                _dbContext.PettyCashAccountItem.AddRange(items);
                _dbContext.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public void Edit(int accountId, PettyCashAccountModel pettyCashAccount)
        {
            if (pettyCashAccount == null || pettyCashAccount.Items == null || pettyCashAccount.Items.Count < 1)
                throw new Exception("No data found to upload.");

            if (pettyCashAccount.Customer == null)
                throw new Exception("Invalid Customer or Customer not found.");

            if (pettyCashAccount.Date > DateTime.Now)
                throw new Exception("Future date transaction cannot possible");

            if (pettyCashAccount.Items == null || pettyCashAccount.Items.Count == 0)
                throw new Exception("Without item cannot save");

            if (pettyCashAccount.Items.Any(x => x.Item == null))
                throw new Exception("Some of the item is empty");

            if (pettyCashAccount.Items.Any(x => x.Quantity == 0))
                throw new Exception("All Item Qty should be grater than 0");

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var account = _dbContext.PettyCashAccount.First(x => x.Id == accountId);

                var items = pettyCashAccount.Items.Select(x => new PettyCashAccountItem()
                {
                    ItemId = x.Item.Id,
                    MRP = x.MRP,
                    Quantity = x.Quantity,
                    Discount = x.Discount,
                    Notes = x.Notes
                }).ToList();

                var billCharges = pettyCashAccount.Adjustments.Where(x => x.TransactionType == TransactionType.Credit).Sum(x => x.Amount);
                var billDiscount = pettyCashAccount.Adjustments.Where(x => x.TransactionType == TransactionType.Debit).Sum(x => x.Amount);
                
                account.Date = pettyCashAccount.Date;
                account.CustomerId = pettyCashAccount.Customer.Id;
                account.Notes = pettyCashAccount.Notes;
                account.TransactionType = pettyCashAccount.TransactionType;
                account.PaymentJSon = JsonSerializer.Serialize<List<PaymentModel>>(pettyCashAccount.Payment);
                account.BillAdustmentsJSon = JsonSerializer.Serialize<List<BillAdjustments>>(pettyCashAccount.Adjustments);
                account.PaidAmount = pettyCashAccount.Payment.Where(x => Enum.IsDefined(typeof(PaymentMode), x.PaymentType)).Sum(x => x.Amount);
                account.CashAmount = pettyCashAccount.Payment.Where(x => x.PaymentType == PaymentMode.Cash).Sum(x => x.Amount);
                account.UPIAmount = pettyCashAccount.Payment.Where(x => x.PaymentType == PaymentMode.UPI).Sum(x => x.Amount);
                account.BillCharges = billCharges;
                account.BillDiscount = billDiscount;
                account.TotalLineAmount = items.Sum(x => x.MRP);
                account.TotalLineDiscount = items.Sum(x => x.Discount);
                account.NetAmount = items.Sum(x => x.Total) + (billCharges - billDiscount);

                
                _dbContext.PettyCashAccount.Update(account);
                _dbContext.SaveChanges();

                items.ForEach(i => i.AccountId = account.Id);
                _dbContext.PettyCashAccountItem.RemoveRange(_dbContext.PettyCashAccountItem.Where(x=>x.AccountId == accountId));
                _dbContext.PettyCashAccountItem.AddRange(items);
                _dbContext.SaveChanges();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public void Delete(int pettyCashAccountId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                _dbContext.PettyCashAccountItem.RemoveRange(_dbContext.PettyCashAccountItem.Where(x => x.AccountId == pettyCashAccountId));
                _dbContext.PettyCashAccount.Remove(_dbContext.PettyCashAccount.First(x=>x.Id == pettyCashAccountId));
                _dbContext.SaveChanges();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public class PettyCashAccountQueryModel
        {
            public PettyCashAccount p { get; set; }
            public Customer c { get; set; }
            public List<PettyCashAccountItemModel> pi { get; set; }
        }
        public PettyCashAccountHeaderModel GetHeader(int? customerId, DateTime? fromDate, DateTime? toDate) =>
            (from p in _dbContext.PettyCashAccount.AsNoTracking()
             join c in _dbContext.Customer.AsNoTracking() on p.CustomerId equals c.Id
             where p.CompanyId == _loginContext.CompanyId && p.BranchId == _loginContext.BranchId &&
                         (!customerId.HasValue || p.CustomerId == customerId.Value) &&
                         (!fromDate.HasValue || p.Date.Date >= fromDate.Value.Date) &&
                         (!toDate.HasValue || p.Date.Date <= toDate.Value.Date)
             group p by 1 into g
             select new PettyCashAccountHeaderModel()
             {
                 TotalBillAmount = g.Where(x => x.TransactionType == TransactionType.Credit).Sum(x => x.NetAmount),
                 TotalPaidAmount = g.Where(x => x.TransactionType == TransactionType.Credit).Sum(x => x.PaidAmount),
                 TotalCashPaidAmount = g.Where(x => x.TransactionType == TransactionType.Credit).Sum(x => x.CashAmount),
                 TotalExpenseAmount = g.Where(x=>x.TransactionType == TransactionType.Debit).Sum(x => x.NetAmount),
                 TotalSettledAmount = g.Where(x => x.TransactionType == TransactionType.Debit).Sum(x => x.PaidAmount),
                 TotalCashSettledAmount = g.Where(x => x.TransactionType == TransactionType.Debit).Sum(x => x.CashAmount),
                 TotalBillCount = g.Count(),
             }).FirstOrDefault();
        public PaginatedList<PettyCashAccountListModel> GetAll(int? customerId, DateTime? fromDate, DateTime? toDate, int pageIndex, int pageSize)
        {
            var accountRes = (from p in _dbContext.PettyCashAccount.AsNoTracking()
                              join c in _dbContext.Customer.AsNoTracking() on p.CustomerId equals c.Id
                              where p.CompanyId == _loginContext.CompanyId && p.BranchId == _loginContext.BranchId &&
                                          (!customerId.HasValue || p.CustomerId == customerId.Value) &&
                                          (!fromDate.HasValue || p.Date.Date >= fromDate.Value.Date) &&
                                          (!toDate.HasValue || p.Date.Date <= toDate.Value.Date)
                              orderby p.Date descending, p.Id descending
                              select new PettyCashAccountQueryModel()
                              {
                                  p = p,
                                  c= c,
                                  pi = (from i in _dbContext.PettyCashAccountItem.AsNoTracking()
                                       join it in _dbContext.Item.AsNoTracking() on i.ItemId equals it.Id
                                       where i.AccountId == p.Id
                                       select new PettyCashAccountItemModel()
                                       {
                                            Discount = i.Discount,
                                            MRP = i.MRP,
                                           Quantity = i.Quantity,
                                           Notes = i.Notes,
                                           Item = it
                                       }).ToList(),
                              });
            return PaginatedList<PettyCashAccountQueryModel>.Create(accountRes, pageIndex, pageSize)
                    .MapTo(x => new PettyCashAccountListModel
                    {
                        Id = x.p.Id,
                        Customer = new CustomerModel
                        {
                            Id = x.c.Id,
                            Name = x.c.Name,
                            Address = x.c.Address,
                            ContactNumber = ConvertTo<List<MobileModel>>(x.c.ContactInfoJSon)
                        },
                        Date = x.p.Date,
                        TransactionType = x.p.TransactionType,
                        Notes = x.p.Notes,
                        Payment = ConvertTo<List<PaymentModel>>(x.p.PaymentJSon),
                        Items = x.pi,
                        PaidAmount = x.p.PaidAmount,
                        BillAmount = x.p.NetAmount
                    });
        }

        private T ConvertTo<T>(string inp) =>
             JsonSerializer.Deserialize<T>(inp, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
