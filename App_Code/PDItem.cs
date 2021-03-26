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
/// Summary description for PDItem
/// </summary>
public class PDItem
{
    public string ItemSKU { get; set; }
    public string SupBarCode { get; set; }
    public string ItemUOM { get; set; }
    public decimal ItemQty { get; set; }
    public decimal ItemPrice { get; set; }
}
