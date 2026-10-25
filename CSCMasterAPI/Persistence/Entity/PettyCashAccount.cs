using CSCMasterAPI.Models;

namespace CSCMasterAPI.Persistence.Entity
{
    public class PettyCashAccount
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal TotalLineAmount { get; set; }
        public decimal TotalLineDiscount { get; set; }
        public decimal BillDiscount { get; set; }
        public decimal BillCharges { get; set; }
        public decimal NetAmount { get; set; }
        public string PaymentJSon { get; set; }
        public string BillAdustmentsJSon { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal UPIAmount { get; set; }
        public string Notes { get; set; }
    }
    
    public class Customer
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string ContactInfoJSon { get; set; }
    }
    
    public class PettyCashAccountItem
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int ItemId { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
        public decimal Total => (MRP * Quantity) - Discount;
    }

    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CompanyBranch
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
    }
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal MRP { get; set; }
        public TransactionType TransactionType { get; set; }
    }
}
