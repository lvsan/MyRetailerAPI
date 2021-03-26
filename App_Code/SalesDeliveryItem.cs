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
/// Summary description for SalesDeliveryItem
/// </summary>
public class SalesDeliveryItem
{
    //public string ItemID { get; set; }
    //public string SupBarItemID { get; set; }
    //public decimal ItemQty { get; set; }
    //public decimal ItemPrice { get; set; }
    

    public int ItemID { get; set; }
    public string SupBarItem { get; set; }
    public int SupBarItemID { get; set; }
    public int ID { get; set; }
    public string ItemSKU { get; set; }
    public string ItemDesc { get; set; }
    public string ItemRemark { get; set; }
    public decimal ItemQty { get; set; }
    public decimal ItemSOQty { get; set; }
    public decimal ActualQty { get; set; }
    public string ItemUOM { get; set; }
    public string GST { get; set; }
    public string GSTType { get; set; }
    public decimal GstRate { get; set; }
    public decimal ItemPrice { get; set; }
    public string ItemUnit { get; set; }
    public int ItemUnitID { get; set; }
    public decimal ItemDiscAmt { get; set; }
    public decimal Disc_pcn1 { get; set; }
    public decimal Disc_pcn2 { get; set; }
    public decimal Disc_pcn3 { get; set; }
    public decimal ItemSubTotal { get; set; }
    public decimal ItemGST { get; set; }
    public decimal TotalDisc { get; set; }
    public decimal Total { get; set; }
    public decimal LocalItemPrice { get; set; }
    public decimal LocalItemDiscAmt { get; set; }
    public decimal LocalTotalDisc { get; set; }
    public decimal LocalItemSubTotal { get; set; }
    public decimal LocalItemGST { get; set; }
    public decimal LocalTotal { get; set; }
    public string ItemFoc { get; set; }
    public int Item_ac_asset { get; set; }
    public string LastUser { get; set; }
    public string LastUpdate { get; set; }
    public string LockUser { get; set; }
    public string LockUpdate { get; set; }
    public string LockStatus { get; set; }
    public string RecordStatus { get; set; }
    public string RecordUpdate { get; set; }
    public string QueueStatus { get; set; }
    public int TerminalID { get; set; }
    public int RetailID { get; set; }
}
