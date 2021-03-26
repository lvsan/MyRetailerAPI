﻿using System;
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
    public decimal ShippingFee { get; set; }
    public string DiscID1 { get; set; }
    public string DiscID2 { get; set; }
    public string DiscID3 { get; set; }
	public string ContraSalesID { get; set; }
	public string ContraSalesNo { get; set; }
	public string ContraSalesDate { get; set; }
	public string ContraCreateTime { get; set; }
	public string ContraSalesStatus { get; set; }
    //public string MemberName { get; set; }
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
    public decimal ItemTotalDisc { get; set; }
    public decimal ItemPrice { get; set; }
    public decimal ItemTax { get; set; }
    public decimal ItemTotal { get; set; }
    public string RFID { get; set; } 
	public decimal ItemDiscGroupAmt {get; set;}
	public decimal GroupDiscAmt {get; set;}
	public decimal GroupDiscAmt2 {get; set;}
	public decimal GroupDiscAmt3 {get; set;}
	public decimal GroupDiscPerc {get; set;}
	public decimal GroupDiscPerc2 {get; set;}
	public decimal GroupDiscPerc3 {get; set;}
    public string DiscID1 { get; set; }
    public string DiscID2 { get; set; }
    public string DiscID3 { get; set; }
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
    public string PaymentReference { get; set; }
    public string PaymentStatus { get; set; }
    public string OthersPayment { get; set; }
    public string OthersPaymentRef { get; set; }
    public string PaymentCardNo { get; set; }
    public string TID { get; set; }
    public string MerchantID { get; set; }
    public string PaymentInvoiceNo { get; set; }
    public string PaymentApprovalCode { get; set; }
    public string Issuer_country { get; set; }
    public string Issuer_bank { get; set; }
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
    public decimal SellPrice { get; set; }
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

public class DiscountType
{
    public string ID { get; set; }
    public string Nick { get; set; }
    public string Value { get; set; }
    public string ButtonName { get; set; }
    public string PrintOnReceipt { get; set; }
    public string Full { get; set; }
    public string ItemDisc { get; set; }
    public string GroupDisc { get; set; }
    public string DiscType { get; set; }
    public decimal DiscAmount { get; set; }
    public string OpenDisc { get; set; }
    public string Display { get; set; }
}

public class StockTransfer
{
    public string ID { get; set; }
    public string FromRetailerID { get; set; }
    public string ToRetailerID { get; set; }
    public string IDRef { get; set; }
    public string StkTransDate { get; set; }
    public string StkTrans_Remark { get; set; }
    public string TerminalID { get; set; }
    public string LastUser1stConfirm { get; set; }
    public string LastUpdate1stConfirm { get; set; }
    public string RecordStatus1stConfirm { get; set; }
    public string CreateTime { get; set; }
    public StockTransferDetail[] ItemTransfer { get; set; }
}

public class StockTransferDetail
{
    public string StkTrans_DetailID { get; set; }
    public string ID { get; set; }
    public string ItemID { get; set; }
    public int Qty { get; set; }
    public int RcvdQty { get; set; }
    public string ItemUOMID { get; set; }
    public string ItemUOMDesc { get; set; }
    public string StkTrans_DRemark { get; set; }
    public string TerminalID { get; set; }
    public string FromRetailerID { get; set; }
    public string ToRetailerID { get; set; }
}

public class StockAdjust
{
    public string ID { get; set; }
    public string RetailerID { get; set; }
    public string IDRef { get; set; }
    public int StkAdj_Type { get; set; }
    public string StkAdjDate { get; set; }
    public string StkAdj_Remark { get; set; }
    public string TerminalID { get; set; }
    public string RecordStatus { get; set; }
    public string CreateTime { get; set; }
    public StockAdjustDetail[] ItemAdjust { get; set; }
}

public class StockAdjustDetail
{
    public string StkAdj_DetailID { get; set; }
    public string ID { get; set; }
    public string ItemID { get; set; }
    public int Qty { get; set; }
    public string ItemUOMID { get; set; }
    public string ItemUOMDesc { get; set; }
    public string StkAdj_DRemark { get; set; }
    public int StkAdj_Type { get; set; }
    public string RetailerID { get; set; }
    public string TransStatus { get; set; }
}

public class StockTake
{
    public string ID { get; set; }
    public string RetailerID { get; set; }
    public string IDRef { get; set; }
    public string StkTakeDate { get; set; }
    public string RemarkID { get; set; }
    public string StkTake_Remark { get; set; }
    public string StkAdjID { get; set; }
    public string PostedDate { get; set; }
    public string bitStockTakeByBatch { get; set; }
    public string bitIncludeZeroQty { get; set; }
    public string TerminalID { get; set; }
    public string RecordStatus { get; set; }
    public string CreateTime { get; set; }
    public StockTakeDetail[] ItemTake { get; set; }
}

//StkTake_DetailID, ID, serialNo, ItemID, SystemQty, CountQty, VarianceQty, StkTakeSys_UnitPx, StkTakeSys_TtlPx, StkTakeCnt_TtlPx, StkTake_DRemark, SupBarCode, ItemCost,
//ItemUnitID, UnitID, UOM, ItemActQty, ItemBaseCost
public class StockTakeDetail
{
    public string StkTake_DetailID { get; set; }
    public string ID { get; set; }
    public int serialNo { get; set; }
    public string ItemID { get; set; }
    public int SystemQty { get; set; }
    public int CountQty { get; set; }
    public int VarianceQty { get; set; }
    public decimal StkTakeSys_UnitPx { get; set; }
    public decimal StkTakeSys_TtlPx { get; set; }
    public decimal StkTakeCnt_TtlPx { get; set; }
    public string StkTake_DRemark { get; set; }
    public string SupBarCode { get; set; }
    public decimal ItemCost { get; set; }
    public string ItemUnitID { get; set; }
    public string UnitID { get; set; }
    public string UOM { get; set; }
    public decimal ItemActQty { get; set; }
    public decimal ItemBaseCost { get; set; }
}