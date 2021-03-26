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
/// Summary description for salesDelivery
/// </summary>
public class SalesDelivery
{
    //public string INVID { get; set; }
    //public string INVRef { get; set; }
    //public DateTime TransDate { get; set; }
    //public string RecipientName { get; set; }
    //public string RecipientAddress { get; set; }
    //public string RecipientPostCode { get; set; }
    //public string RecipientAttn { get; set; }
    //public string RecipientTel { get; set; }
    //public string Remark { get; set; }
    //public string GST { get; set; }
    //public SalesDeliveryItem[] SalesDeliveryItems { get; set; }

    public string Type { get; set; }
    public string CompanyAddr { get; set; }
    public string CompanyTel { get; set; }
    public string CompanyFax { get; set; }
    public string RecipientName { get; set; }
    public string RecipientAddr { get; set; }
    public string RecipientAddress { get; set; }
    public string RecipientPostCode { get; set; }
    public string RecipientAttn { get; set; }
    public string RecipientTel { get; set; }
    public string RecipientFax { get; set; }
    public string IDRef { get; set; }
    public int RetailerID { get; set; }
    public string Date { get; set; }
    public DateTime TransDate { get; set; }
    public string SN_Ref { get; set; }
    public int INVRetailerID { get; set; }
    public int INVID { get; set; }
    public string INVRef { get; set; }
    public string INVDate { get; set; }
    public int RetailSDID { get; set; }
    public string GST { get; set; }
    public string GSTIncEx { get; set; }
    public decimal GstRate { get; set; }
    public decimal BalSubTotal { get; set; }
    public decimal BalTax { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal BalTotal { get; set; }
    public decimal BalPayable { get; set; }
    public decimal LocalBalSubTotal { get; set; }
    public decimal LocalTax { get; set; }
    public decimal LocalTotalDiscount { get; set; }
    public decimal LocalTotal { get; set; }
    public decimal LocalBalPayable { get; set; }
    public string Remarks { get; set; }
    public string Document_Status { get; set; }
    public decimal OutStandingBal { get; set; }
    public decimal LocalOutStandingBal { get; set; }
    public decimal DepositAmount { get; set; }
    public string LastUser { get; set; }
    public string LastUpdate { get; set; }
    public string LockUser { get; set; }
    public string LockUpdate { get; set; }
    public string LockStatus { get; set; }
    public string RecordStatus { get; set; }
    public string RecordUpdate { get; set; }
    public string QueueStatus { get; set; }
    public int TerminalID { get; set; }
    public string Remark { get; set; }
    public List<SalesDeliveryItem> items { get; set; }
    public SalesDeliveryItem[] SalesDeliveryItems { get; set; }
}
