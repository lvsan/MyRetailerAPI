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
/// Summary description for SalesOrder
/// </summary>
public class SalesOrder
{
    public string RefID { get; set; }
    public string RefNo { get; set; }
    public string QueueNo { get; set; }
    public string RetailID { get; set; }
    public string TerminalID { get; set; }
    public ItemOrders[] ItemOrder { get; set; }
}
