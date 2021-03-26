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
/// Summary description for SalesMaster
/// </summary>
public class SalesMaster
{
    public string TransNo { get; set; }
    public string RetailID { get; set; }
    public DateTime SalesDate { get; set; }
    public string SalesStatus { get; set; }
    public string MemberID { get; set; }
    public string SalesTaxType { get; set; }
    public decimal SalesTaxRate { get; set; }
    public decimal SalesTaxTtl { get; set; }
    public decimal SalesRounding { get; set; }
    public decimal SalesTotalAmount { get; set; }
    public decimal SalesDiscPerc { get; set; }
    public decimal SalesDiscPerc2 { get; set; }
    public decimal SalesDiscPerc3 { get; set; }
    public decimal SalesDiscAmt { get; set; }
    public decimal SalesDiscAmt2 { get; set; }
    public decimal SalesDiscAmt3 { get; set; }
    public decimal SalesTotalDiscount { get; set; }
    public decimal MemberDisc { get; set; }
    public decimal MemberAmt { get; set; }
    //public System.Collections.ObjectModel.Collection<SalesDetails> ItemSales { get; set; }
    //public System.Collections.ObjectModel.Collection<SalesPayment> SalesPayments { get; set; }
    public SalesDetails[] ItemSales { get; set; }
    public SalesPayment[] SalesPayments { get; set; }
    public SalesPerson[] SalesPersons { get; set; }
    //public List<InventoryAging> ItemAging { get; set; }
}

public class SalesDetails
{
    public string ItemID { get; set; }
 	public string SupBarCode { get; set; }
    public decimal ItemSellPrice { get; set; }
    public int ItemQty { get; set; }
    public string ItemUOMDesc { get; set; }
    public decimal ItemDiscPerc { get; set; }
    public decimal ItemDiscPerc2 { get; set; }
    public decimal ItemDiscPerc3 { get; set; }
    public decimal ItemDisc { get; set; }
    public decimal ItemDisc2 { get; set; }
    public decimal ItemDisc3 { get; set; }
    public decimal ItemPrice { get; set; }
    public decimal ItemTax { get; set; }
    public decimal ItemTotal { get; set; }
    public string RFID { get; set; } 
    public ItemVoucher[] ItemVoucher { get; set; }
}

public class ItemVoucher
{
    public string VoucherNo { get; set; }
    public decimal VoucherAmount { get; set; }
}

public class SalesPayment
{
	public string paymentID { get; set; }
    public string strPayment { get; set; }
    public decimal SalesPayTtl { get; set; }
    public decimal SalesBalTtl { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal TipsAmount { get; set; }
    public SalesVoucher[] SaleVoucher { get; set; }
}

public class SalesPerson
{
    public string ItemCode { get; set; }
    public string SalesPersonID { get; set; }
    public decimal CommPerc { get; set; }
    public decimal CommAmount { get; set; }
}

public class SalesVoucher
{
    public string VoucherNo { get; set; }
    public decimal VoucherAmount { get; set; }
}

// below is use to store the data into table inventory_aging
public class InventoryAgings
{
    public List<InventoryAging> ItemAging { get; set; }
   // public InventoryAging[] ItemAging { get; set; }
}

public class InventoryAging
{
    public string ID { get; set; }
    public string SupplierID { get; set; }
    public string RetailID { get; set; }
    public string ItemID { get; set; }
    public string ItemSKU { get; set; }
    public string TransID { get; set; }
    public string TransNo { get; set; }
    public DateTime TransDate { get; set; }    
    public string ItemUOMID { get; set; }
    public string ItemUOM { get; set; }
    public string ItemBaseUOMID { get; set; }
    public string ItemBaseUOM { get; set; }
    public decimal Qty { get; set; }
    public decimal ItemActualQty { get; set; }
    public string CurrencyID { get; set; }
    public decimal ExcRate { get; set; }
    public decimal CostUnitPx { get; set; }
    public decimal LocalCostUnitPx { get; set; }
    public DateTime CreateTime { get; set; }
    public string BatchNo { get; set; }
    public string HSCode { get; set; }
    public string ExpireDate { get; set; }
    public int ExpiryDay { get; set; }
    public decimal ItemDefActualQty { get; set; }
    public decimal PDQty { get; set; }
    public decimal SoldQty { get; set; }
    public decimal TrfInQty { get; set; }
    public decimal TrfOutQty { get; set; }
    public decimal AdjQty { get; set; }
    public decimal RetQty { get; set; }
    public decimal SDQty { get; set; }
    public decimal KitQty { get; set; }
    public decimal DekitQty { get; set; }
    public decimal ReserveQty { get; set; }
    public decimal InTransitQty { get; set; }
    public decimal QtyBalance { get; set; }
    public string RFID { get; set; }
}
