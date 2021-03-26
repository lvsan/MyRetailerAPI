using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Summary description for MemberInfor
/// </summary>
public class MemberInfor
{
    public string MemberName { get; set; }
    public string Gender { get; set; }
    public string DOB { get; set; }
    public string CreateDate { get; set; }
    public string NRIC { get; set; }
    public string Email { get; set; }
    public string HPH { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string MemberID { get; set; }
    public string VendorMemberID { get; set; }
}

public class MemberInfor_Order
{
    public string MemberName { get; set; }
    public string MemberHp { get; set; }
    public string MemberAddress { get; set; }
}

public class SalesHoldOrder
{
    public string CompanyID { get; set; }
    public string RetailerID { get; set; }
    public string TransNo { get; set; }
    public decimal TotalDue { get; set; }
    public decimal TotalGST { get; set; }
    public decimal TotalDisc { get; set; }
    public string TransDate { get; set; }
    public string CreateTime { get; set; }
    public decimal TotalQty { get; set; }
    public string CashierID { get; set; }
    public string MemberID { get; set; }
    public string isNewCust { get; set; }
    public string SalesPersonID { get; set; }
    public string CommID { get; set; }
    public decimal CommPerc { get; set; }
    public string ReceiptOrderStatus { get; set; }
    public string vchQueueNo { get; set; }
	public int intTableNo { get; set; }
    public string MacAddress { get; set; }
    public string TerminalID { get; set; }
    public string PendingSync { get; set; }
    public string LastUser { get; set; }
    public string LastUpdate { get; set; }
    public string LockUser { get; set; }
    public string LockUpdate { get; set; }
    public string LockStatus { get; set; }
    public string RecordStatus { get; set; }
    public string RecordUpdate { get; set; }
    public string QueueStatus { get; set; }
    public List<SalesHoldOrder_Item> saleshold_item { get; set; }
}

public class SalesHoldOrder_Item
{
    public string RecordNo { get; set; }
    public string LineNo { get; set; }
    public decimal ItemQty { get; set; }
    public decimal ItemPrice { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal ItemGST { get; set; }
    public string ItemDiscType { get; set; }
    public decimal ItemDisc1 { get; set; }
    public decimal ItemDisc2 { get; set; }
    public decimal ItemDisc3 { get; set; }
    public string ItemID { get; set; }
    public string ItemBarcode { get; set; }
    public string ItemUOM { get; set; }
    public string ItemGSTInEx { get; set; }
    public decimal ItemCost { get; set; }
    public decimal ItemActQty { get; set; }
    public string ItemUOMID { get; set; }
    public decimal ItemGroupDisc { get; set; }
    public string ItemSKU { get; set; }
    public string SupplierID { get; set; }
    public string SalesPersonID { get; set; }
    public string SalesCommTypeID { get; set; }
    public decimal SalesCommPerc { get; set; }
    public decimal ItemCommPerc { get; set; }
    public decimal ItemCommAmt { get; set; }
    public string ItemSerialNo { get; set; }
    public string DISCID { get; set; }
    public string ItemIMEINo { get; set; }
    public string ItemBatchNo { get; set; }
    public string ItemStatus { get; set; }
    public string OpenPriceRemark { get; set; }
    public string ItemRemark { get; set; }
    public string ExpireDate { get; set; }
    public int ExpiryDay { get; set; }
    public decimal RedeemPoint { get; set; }
    public string ParentItemID_ADDON { get; set; }
    public string bitAddOnItem { get; set; }
    public string ParentDetailID_ADDON { get; set; }
    public decimal MemDOBDiscPerc { get; set; }
    public decimal MemDOBDiscAmount { get; set; }
    public string ReceiptOrderStatus { get; set; }
    public string TerminalID { get; set; }
    public string RFID { get; set; }
    public string PendingSync { get; set; }
    public string LastUser { get; set; }
    public string LastUpdate { get; set; }
    public string LockUser { get; set; }
    public string LockUpdate { get; set; }
    public string LockStatus { get; set; }
    public string RecordStatus { get; set; }
    public string RecordUpdate { get; set; }
    public string QueueStatus { get; set; }
}

public class SalesOnlineOrder
{
    public string RetailID { get; set; }
    public string SalesNo { get; set; }
    public string SalesTax { get; set; }
    public decimal SalesTaxVal { get; set; }
    public string SalesDate { get; set; }
    public string CloseRetailID { get; set; }
    public string CloseDate { get; set; }
    public string CloseTime { get; set; }
    public string SalesPersonID { get; set; }
    public string SalesPerson { get; set; }
    public string SalesRemark { get; set; }
    public decimal SalesDisc { get; set; }
    public decimal SalesDisc2 { get; set; }
    public decimal SalesDisc3 { get; set; }
    public decimal SalesDiscAmt { get; set; }
    public decimal SalesDiscAmt2 { get; set; }
    public decimal SalesDiscAmt3 { get; set; }
    public decimal SalesDiscGroupPct { get; set; }
    public decimal SalesDiscGroupAmt { get; set; }
    public decimal SalesTotalGroupDisc { get; set; }
    public string SalesDiscGroupType { get; set; }
    public decimal SalesSubTtl { get; set; }
    public decimal SalesTaxTtl { get; set; }
    public decimal SalesBalTtl { get; set; }
    public decimal SalesPayTtl { get; set; }
    public decimal SalesChangeAmt { get; set; }
    public decimal SalesDeposit { get; set; }
    public string SalesStatus { get; set; }
    public string ContraRetailID { get; set; }
    public string ContraSalesID { get; set; }
    public string ContraSalesNo { get; set; }
    public string ContraSalesDate { get; set; }
    public string CreateBy { get; set; }
    public string CreateTime { get; set; }
    public decimal SalesRounding { get; set; }
    public string MemberID { get; set; }
    public decimal MemberDisc { get; set; }
    public decimal MemberAmt { get; set; }
    public decimal CardAmt { get; set; }
    public string CommID { get; set; }
    public decimal CommPerc { get; set; }
    public string DepositStatus { get; set; }
    public string CollectionDate { get; set; }
    public string CollectionRetailID { get; set; }
    public string CloseTerminalID { get; set; }
    public string EmployeeID { get; set; }
    public string EmployeeName { get; set; }
    public string MacAddress { get; set; }
    public decimal MemDOBDiscPerc { get; set; }
    public decimal MemDOBDiscAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public string dteDeliveryDate { get; set; }
    public string dteDeliveryTimeFr { get; set; }
    public string dteDeliveryTimeTo { get; set; }
    public string vchQueueNo { get; set; }
    public string ReceiptOrderStatus { get; set; }
    public string TableNumber { get; set; }
    public string PendingSync { get; set; }
    public string SyncAPI { get; set; }
    public string LastUser { get; set; }
    public string LastUpdate { get; set; }
    public string LockUser { get; set; }
    public string LockUpdate { get; set; }
    public string LockStatus { get; set; }
    public string RecordStatus { get; set; }
    public string RecordUpdate { get; set; }
    public string QueueStatus { get; set; }
    public string TerminalID { get; set; }
    public List<SalesOnlineOrder_Item> salesorder_item { get; set; }
    public List<SalesOnlineOrder_Payment> salesorder_payment { get; set; }
    public MemberInfor_Order memberinfor { get; set; }
}

public class SalesOnlineOrder_Item
{
	public int LineNo { get; set; }
    public string SalesID { get; set; }
    public string RetailID { get; set; }
    public string ItemID { get; set; }
    public string SupbarCode { get; set; }
    public decimal ItemQty { get; set; }
    public string ItemUOM { get; set; }
    public string ItemUOMDesc { get; set; }
    public decimal ItemQtyAct { get; set; }
    public decimal ItemUnitPrice { get; set; }
    public decimal ItemUnitCost { get; set; }
    public decimal ItemDisc { get; set; }
    public decimal ItemDisc2 { get; set; }
    public decimal ItemDisc3 { get; set; }
    public decimal ItemDiscAmt { get; set; }
    public decimal ItemDiscAmt2 { get; set; }
    public decimal ItemDiscAmt3 { get; set; }
    public decimal ItemTotalDisc { get; set; }
    public decimal ItemDiscGroupAmt { get; set; }
    public decimal ItemSubTotal { get; set; }
    public decimal ItemTaxTotal { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal ItemQtyDeliver { get; set; }
    public decimal ItemQtyRemain { get; set; }
    public string ItemTaxType { get; set; }
    public string SupplierID { get; set; }
    public decimal PromoDiscAmt { get; set; }
    public decimal PromoVIPDiscAmt { get; set; }
    public string PromoDiscTypeCode { get; set; }
    public string PromoDiscTypeID { get; set; }
    public string PromoID { get; set; }
    public decimal GroupDiscAmt { get; set; }
    public decimal GroupDiscAmt2 { get; set; }
    public decimal GroupDiscAmt3 { get; set; }
    public decimal GroupDiscPerc { get; set; }
    public decimal GroupDiscPerc2 { get; set; }
    public decimal GroupDiscPerc3 { get; set; }
    public decimal MemberDisc { get; set; }
    public decimal MemberAmt { get; set; }
    public decimal CardAmt { get; set; }
    public string OpenPriceRemark { get; set; }
    public string ItemRemark { get; set; }
    public string ItemDiscType { get; set; }
    public string SalesPersonID { get; set; }
    public string SalesCommTypeID { get; set; }
    public decimal SalesCommPerc { get; set; }
    public decimal ItemCommPerc { get; set; }
    public decimal ItemCommAmt { get; set; }
    public string ItemSerialNo { get; set; }
    public string ItemIMEINo { get; set; }
    public string ItemStatus { get; set; }
    public decimal RedeemPoint { get; set; }
    public string ParentItemID_ADDON { get; set; }
    public string bitAddOnItem { get; set; }
    public string ParentDetailID_ADDON { get; set; }
    public decimal MemDOBDiscPerc { get; set; }
    public decimal MemDOBDiscAmount { get; set; }
    public string CollectionRetailID { get; set; }
    public string CollectionDate { get; set; }
    public string CollectionTerminalID { get; set; }
    public decimal ItemSSPx { get; set; }
    public string ReceiptOrderStatus { get; set; }
    public string TerminalID { get; set; }
    public string RFID { get; set; }
    public string PendingSync { get; set; }
    public string LastUser { get; set; }
    public string LastUpdate { get; set; }
    public string LockUser { get; set; }
    public string LockUpdate { get; set; }
    public string LockStatus { get; set; }
    public string RecordStatus { get; set; }
    public string RecordUpdate { get; set; }
    public string QueueStatus { get; set; }
}

public class onlineSalesPayment
{
    public SalesOnlineOrder_Payment[] salesorder_payment { get; set; }
}

public class SalesOnlineOrder_Payment
{
    public string SalesID { get; set; }
    public string RetailID { get; set; }
    public string PaymentID { get; set; }
    public string PaymentReference { get; set; }
    public string PaymentRemarks { get; set; }
    public string SalesPayTtl { get; set; }
    public decimal SalesBalTtl { get; set; }
    public decimal SalesDeposit { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal TipsAmount { get; set; }
    public string PaymentStatus { get; set; }
    public string OthersPayment { get; set; }
    public string OthersPaymentRef { get; set; }
    public string DepositStatus { get; set; }
    public string Close_RetailID { get; set; }
    public string Close_SalesID { get; set; }
    public string Close_TerminalID { get; set; }
    public decimal CardDisc { get; set; }
    public decimal CardAmt { get; set; }
    public string TerminalID { get; set; }
    public string PendingSync { get; set; }
    public string LastUser { get; set; }
    public string LastUpdate { get; set; }
    public string LockUser { get; set; }
    public string LockUpdate { get; set; }
    public string LockStatus { get; set; }
    public string RecordStatus { get; set; }
    public string RecordUpdate { get; set; }
    public string QueueStatus { get; set; }
    public string PaymentCardNo { get; set; }
    public string TID { get; set; }
    public string MerchantID { get; set; }
    public string PaymentInvoiceNo { get; set; }
    public string PaymentApprovalCode { get; set; }
    public string Issuer_country { get; set; }
    public string Issuer_bank { get; set; }
}


public class ModifierInfo
{
    public string ItemPackage { get; set; }
    public string ItemModifier { get; set; }
    public List<ModifierItem> ModifierItems { get; set; }
}

public class ModifierItem
{
    public string ItemID { get; set; }
    public string ItemSKU { get; set; }
    public string ItemDescp { get; set; }
    public string ItemName { get; set; }
    public string ItemPic { get; set; }
    public double ItemQty { get; set; }
    public double ItemPrice { get; set; }
    public string ItemUnitID { get; set; }
    public string ItemUnit { get; set; }
    public string UOM { get; set; }
    public string ItemOutOfStock { get; set; }
    public string ItemDepartment { get; set; }
    public string DepartmentName { get; set; }
    public string ItemSalesType { get; set; }

}
