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

/// <summary>
/// Summary description for PaymentMethod
/// </summary>
public class PaymentMethod
{
    public List<PaymentMethods> PaymentMethods { get; set; }
}
public class PaymentMethods
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Full { get; set; }
    public string Display { get; set; }
}