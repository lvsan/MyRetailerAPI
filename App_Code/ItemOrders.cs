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

/// <summary>
/// Summary description for ItemOrders
/// </summary>
public class ItemOrders
{
    public string ParentID { get; set; }
    public string SupBarCode { get; set; }
    public decimal ParentQty { get; set; }
    public string ItemUOM { get; set; }
    public ItemOrderDetail[] ItemOrderDetails { get; set; }
}

public class ItemOrderDetail
{
    public string ItemID { get; set; }
    public decimal ItemQty { get; set; }
}

