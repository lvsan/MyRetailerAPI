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
/// Summary description for EnvolveData
/// </summary>
public class EnvolveData
{
    public List<Record> records { get; set; }
}

public class Record
{
    public string orderline_id { get; set; }
    public string order_id { get; set; }
    public string retailer_code { get; set; }
    public string store_code { get; set; }
    public string pos_code { get; set; }
    public string order_created_at { get; set; }
    public string staff_name { get; set; }
    public string payment_method { get; set; }
    public int order_amount { get; set; }
    public bool order_is_void { get; set; }
    public bool order_is_discount { get; set; }
    public int orderline_weight { get; set; }
    public int orderline_amount { get; set; }
    public int orderline_quantity { get; set; }
    public int orderline_unit_price { get; set; }
    public int orderline_discount_amount { get; set; }
    public string product_name { get; set; }
    public string product_code { get; set; }
    public string product_barcode { get; set; }
    public bool product_is_gst { get; set; }
    public string sub_category { get; set; }
    public string category_name { get; set; }
    public string sub_department { get; set; }
    public string department { get; set; }
    public string group { get; set; }
    public string manufacturer_name { get; set; }
    public string supplier_name { get; set; }
    public int product_cost { get; set; }
    public string order_currency { get; set; }
    public int order_tax { get; set; }
    public int orderline_tax { get; set; }
}