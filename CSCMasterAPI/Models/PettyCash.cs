using CSCMasterAPI.Persistence.Entity;
using System.Reflection;

namespace CSCMasterAPI.Models
{
    public enum PaymentMode
    {
        Cash = 1,
        UPI = 2
    }

    public enum TransactionType
    {
        Credit = 1,
        Debit = 2
    }
    public enum MobileNoType
    {
        PrimaryAndWhatsapp = 1,
        Primary,
        Whatsapp,
        Telegram,
        Others
    }
    public class PaymentModel
    {
        public PaymentMode PaymentType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
    public class MobileModel
    {
        public MobileNoType Type { get; set; }
        public string Number { get; set; }
    }
    public class CustomerModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<MobileModel> ContactNumber { get; set; }
    }
    public class PettyCashAccountHeaderModel
    {
        public decimal TotalBillAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalCashPaidAmount { get; set; }
        public decimal TotalExpenseAmount { get; set; }
        public decimal TotalSettledAmount { get; set; }
        public decimal TotalCashSettledAmount { get; set; }
        public decimal TotalBillCount { get; set; }
    }
    public class PettyCashAccountListModel : PettyCashAccountModel
    {
        public int Id { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BillAmount { get; set; }
    }
    public class PettyCashEditModel : PettyCashAccountModel
    {
    }
    public class PettyCashAccountModel
    {
        public CustomerModel Customer { get; set; }
        public DateTime Date { get; set; }
        public TransactionType TransactionType { get; set; }
        public List<PaymentModel> Payment { get; set; }
        public string Notes { get; set; }
        public List<PettyCashAccountItemModel> Items { get; set; }
        public List<BillAdjustments> Adjustments { get; set; }
    }
    public class PettyCashAccountItemModel
    {
        public Item Item { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
    }
    public class BillAdjustments
    {
        public string Detail { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
    }

    public class PettyCashGetAllModel
    {
        public int? CustomerId { get; set; }
        public int PageIndex { get; set; }
        public int? PageSize { get; set; } = 10;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
