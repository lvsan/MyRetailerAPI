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

/// <summary>
/// Summary description for Price
/// </summary>
public class Price
{
    public string Item_UnitID { get; set; }
    public string UOM { get; set; }
    public decimal RTLSellPx { get; set; }
    public string ItemUnitDef { get; set; }
	public decimal OnHandQty { get; set; }

}
