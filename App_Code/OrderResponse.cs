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
/// Summary description for OrderResponse
/// </summary>
public class OrderResponse
{
    public Meta meta { get; set; }
    public Response response { get; set; }
}

public class CancelOrderResponse
{
    public Meta meta { get; set; }
    public List<object> response { get; set; }
}

public class Meta
{
    public bool error { get; set; }
    public string message { get; set; }
    public int statusCode { get; set; }
}

public class Response
{
    public int current_page { get; set; }
    public List<Datum> data { get; set; }
    public string first_page_url { get; set; }
    public int from { get; set; }
    public int last_page { get; set; }
    public string last_page_url { get; set; }
    public object next_page_url { get; set; }
    public string path { get; set; }
    public string per_page { get; set; }
    public object prev_page_url { get; set; }
    public int to { get; set; }
    public int total { get; set; }
}

public class Datum
{
    public int id { get; set; }
    public string order_id { get; set; }
    public string external_id { get; set; }
    public string price { get; set; }
    public string shipping_fee { get; set; }
    public string voucher { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string external_created_at { get; set; }
    public string external_updated_at { get; set; }
    public string updated_at { get; set; }
    public List<string> products_statuses { get; set; }
    public int? integration_id { get; set; }
    public string delivery_info { get; set; }
    public AddressBilling address_billing { get; set; }
    public AddressShipping address_shipping { get; set; }
    public string payment_method { get; set; }
    public string payment_country { get; set; }
    public string remarks { get; set; }
    public string created_at { get; set; }
    public int status { get; set; }
    public string status_label { get; set; }
    public bool to_ship { get; set; }
    public bool to_cancel { get; set; }
    public bool action_fulfillment { get; set; }
    public bool action_cancel { get; set; }
    public bool action_update_invoice { get; set; }
    public bool action_get_invoice { get; set; }
    public bool action_get_airway { get; set; }
    public bool action_get_shipping_label { get; set; }
    public bool action_send_invoice { get; set; }
    public Integration integration { get; set; }
    public List<object> items { get; set; }
    //public List<Item> items { get; set; }
}

public class AddressBilling
{
    public string name { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string state { get; set; }
    public string address { get; set; }
    public string country { get; set; }
    public string postcode { get; set; }
}

public class AddressShipping
{
    public string name { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string state { get; set; }
    public string address { get; set; }
    public string country { get; set; }
    public string postcode { get; set; }
}

public class Integration
{
    public int id { get; set; }
    public int marketplace_id { get; set; }
    public string name { get; set; }
    public string username { get; set; }
    public bool has_password { get; set; }
    public string short_api_key { get; set; }
    public string marketplace_name { get; set; }
    public Marketplace marketplace { get; set; }
}


public class Marketplace
{
    public int id { get; set; }
    public string name { get; set; }
}

public class Item
{
    public int id { get; set; }
    public int order_id { get; set; }
    public string name { get; set; }
    public string sku { get; set; }
    public string external_id { get; set; }
    public string paid_price { get; set; }
    public int quantity { get; set; }
    public string status { get; set; }
    public object product_id { get; set; }
    public bool to_ship { get; set; }
    public bool to_cancel { get; set; }
    public bool oversold { get; set; }
    public object product { get; set; }
    public Order order { get; set; }
}

public class Order
{
    public int id { get; set; }
    public int integration_id { get; set; }
    public int shop_id { get; set; }
    public string external_id { get; set; }
    public int calculate { get; set; }
    public string number { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string remarks { get; set; }
    public int product_counts { get; set; }
    public string delivery_info { get; set; }
    public string national_reg_number { get; set; }
    public string is_gift { get; set; }
    public string gift_message { get; set; }
    public string payment_method { get; set; }
    public string price { get; set; }
    public object paid { get; set; }
    public string voucher { get; set; }
    public string shipping_fee { get; set; }
    public string promised_shipping_time { get; set; }
    public string external_created_at { get; set; }
    public string external_updated_at { get; set; }
    public object deleted_at { get; set; }
    public string created_at { get; set; }
    public string updated_at { get; set; }
    public object is_hold { get; set; }
    public string payment_country { get; set; }
    public List<string> products_statuses { get; set; }
    public object extra_attributes { get; set; }
    public AddressBilling address_billing { get; set; }
    public AddressShipping address_shipping { get; set; }
    public Data data { get; set; }
    public object order_id { get; set; }
    public int status { get; set; }
    public object shipment_provider { get; set; }
    public int restock_status { get; set; }
    public string status_label { get; set; }
    public bool to_ship { get; set; }
    public bool to_cancel { get; set; }
    public bool action_fulfillment { get; set; }
    public bool action_cancel { get; set; }
    public bool action_update_invoice { get; set; }
    public bool action_get_invoice { get; set; }
    public bool action_get_airway { get; set; }
    public bool action_get_shipping_label { get; set; }
    public bool action_send_invoice { get; set; }
    public Integration2 integration { get; set; }
}

public class Data
{
    public string price { get; set; }
    public string remarks { get; set; }
    public int voucher { get; set; }
    public long order_id { get; set; }
    public List<string> statuses { get; set; }
    public string tax_code { get; set; }
    public string created_at { get; set; }
    public string updated_at { get; set; }
    public bool gift_option { get; set; }
    public int items_count { get; set; }
    public string gift_message { get; set; }
    public long order_number { get; set; }
    public string shipping_fee { get; set; }
    public string voucher_code { get; set; }
    public string branch_number { get; set; }
    public string delivery_info { get; set; }
    public string payment_method { get; set; }
    public int voucher_seller { get; set; }
    public AddressBilling2 address_billing { get; set; }
    public AddressShipping2 address_shipping { get; set; }
    public string extra_attributes { get; set; }
    public int voucher_platform { get; set; }
    public string customer_last_name { get; set; }
    public string customer_first_name { get; set; }
    public string promised_shipping_times { get; set; }
    public string national_registration_number { get; set; }
}

public class AddressBilling2
{
    public string city { get; set; }
    public string phone { get; set; }
    public string phone2 { get; set; }
    public string country { get; set; }
    public string address1 { get; set; }
    public string address2 { get; set; }
    public string address3 { get; set; }
    public string address4 { get; set; }
    public string address5 { get; set; }
    public string last_name { get; set; }
    public string post_code { get; set; }
    public string first_name { get; set; }
}

public class AddressShipping2
{
    public string city { get; set; }
    public string phone { get; set; }
    public string phone2 { get; set; }
    public string country { get; set; }
    public string address1 { get; set; }
    public string address2 { get; set; }
    public string address3 { get; set; }
    public string address4 { get; set; }
    public string address5 { get; set; }
    public string last_name { get; set; }
    public string post_code { get; set; }
    public string first_name { get; set; }
}

public class Integration2
{
    public int id { get; set; }
    public int shop_id { get; set; }
    public int marketplace_id { get; set; }
    public string username { get; set; }
    public string api_key { get; set; }
    public Data2 data { get; set; }
    public object cookie { get; set; }
    public bool active { get; set; }
    public int requires_authentication { get; set; }
    public object deleted_at { get; set; }
    public string created_at { get; set; }
    public string updated_at { get; set; }
    public string track_since { get; set; }
    public string track_created_time { get; set; }
    public string track_created_status { get; set; }
    public string track_updated_time { get; set; }
    public string track_updated_status { get; set; }
    public string name { get; set; }
    public object track_product_since { get; set; }
    public string track_product_created_time { get; set; }
    public string track_product_created_status { get; set; }
    public string track_product_updated_time { get; set; }
    public string track_product_updated_status { get; set; }
    public int inventory_synced { get; set; }
    public int update_inventory_from_orders { get; set; }
    public bool description_update { get; set; }
    public bool remove_image_whiteline { get; set; }
    public object track_enquiry_sync_time { get; set; }
    public string automated_email { get; set; }
    public object remark { get; set; }
    public int allowed_duplicate_sku { get; set; }
    public int cancelled_restock { get; set; }
    public bool has_password { get; set; }
    public string short_api_key { get; set; }
    public string marketplace_name { get; set; }
    public Marketplace2 marketplace { get; set; }
}

public class Data2
{
    public int expiry { get; set; }
    public string country { get; set; }
    public string platform { get; set; }
    public List<UserInfo> user_info { get; set; }
    public int refreshExpiry { get; set; }
    public string refresh_token { get; set; }
}

public class UserInfo
{
    public string country { get; set; }
    public string user_id { get; set; }
    public string seller_id { get; set; }
    public string short_code { get; set; }
}

public class Marketplace2
{
    public int id { get; set; }
    public string name { get; set; }
    public int region { get; set; }
    public string currency { get; set; }
    public string value { get; set; }
    public List<Label> label { get; set; }
    public int create_product { get; set; }
    public int sync_order { get; set; }
    public int sync_products { get; set; }
    public int create_coupon { get; set; }
}

public class Label
{
    public string field { get; set; }
    public string label { get; set; }
}
