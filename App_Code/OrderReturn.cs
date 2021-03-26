using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

public class OrderReturn
{
    public string Message { get; set; }
    public List<SD> SDs { get; set; }
}

public class SD
{
    public string ID { get; set; }
    public string RecipientName { get; set; }
    public string Address { get; set; }
    public string PostCode { get; set; }
    public string RecipientAttn { get; set; }
    public string Tel { get; set; }
    public string OrderStatus { get; set; }
    public string PONo { get; set; }
    public string IDRef { get; set; }
    public string Date { get; set; }
    public string TotalAmount { get; set; }
    public List<SD_Item> SD_Items { get; set; }
}


public class SD_Item
{
    public string KeyCol { get; set; }
    public string ItemID { get; set; }
    public string SupBarCode { get; set; }
    public string Item_SupBarID { get; set; }
    public string ItemQty { get; set; }
    public string ItemPrice { get; set; }
    public string UOM { get; set; }
    public string Total { get; set; }
}

public class zxc
{
    public string FirstName { get; set; }
    public List<asd> SD_Items { get; set; }
}

public class asd
{
    public string LastName { get; set; }

}

public class checkSD
{
    public string IDRef { get; set; }
    public string PONo { get; set; }
}