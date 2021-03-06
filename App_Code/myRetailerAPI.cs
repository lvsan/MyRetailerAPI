using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using System.Messaging;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Web.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using ZXing.Common;
using ZXing;
using ZXing.QrCode;
using System.Drawing;
using System.Drawing.Imaging;

/// <summary>
/// Summary description for API
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class myRetailerAPI : System.Web.Services.WebService {

    public myRetailerAPI()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getInventory(string companyCode, string retailID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string fieldCriteria = "";
        if (retailID != "" || retailID != "0")
        {
            fieldCriteria = " retailid = '" + retailID;
        }

        string strSql = "SELECT inventory_retail.ItemID as ItemID, inventory.ItemSKU as ItemSKU, inventory.ItemDescp as ItemDescp, inventory_supbar.SupBarCode as SupBarCode," +
            "list_units.Nick AS UOM, inventory_unit.RTLSellPx as Price,IF(inventory_retail.OnHandQty IS NULL,0,inventory_retail.OnHandQty) AS OnHandQty, " +
            "inventory.ItemOtherLanguage AS ItemOtherLanguage,inventory.ItemPicFront AS ItemImage ,inventory.ItemDepartment as Department, inventory.ItemCategory as Category " +
            " FROM inventory_retail LEFT JOIN inventory ON inventory_retail.ItemID = inventory.ItemID " +
            " LEFT JOIN inventory_supbar ON inventory_supbar.ItemID = inventory_retail.ItemID AND inventory_supbar.RecordStatus <> 'DELETED' " +
            " LEFT JOIN inventory_unit ON inventory_retail.ItemID = inventory_unit.ItemID AND inventory_unit.RecordStatus <> 'DELETED' " +
            " LEFT JOIN list_units ON list_units.ID=inventory_unit.ItemUnit AND list_units.RecordStatus <> 'DELETED' " +
            " WHERE inventory_retail.RecordStatus <> 'DELETED' AND " + fieldCriteria + "'" +
            " Group By ItemID,ItemSKU,ItemDescp,SupBarCode,UOM";

        DataSet inventoryDS = dataAccessor.RunSPRetDataset_Vapt(strSql, "inventory", null);

        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);
        //List<string[]> asd = inventoryDS;

        //return asd;
        return json;
    }
	
	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getInventoryUOM(string companyCode, string ItemSKU)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string fieldCriteria = "";
        if (ItemSKU != "" || ItemSKU != "0")
        {
            fieldCriteria = " ItemSKU = @ItemSKU OR inventory_supbar.SupBarCode = @ItemSKU ";
        }
        string strSql = "SELECT list_units.Nick AS UOM, inventory_unit.RTLSellPx as Price" +
            " FROM inventory " +
            " LEFT JOIN inventory_supbar ON inventory_supbar.ItemID = inventory.ItemID AND inventory_supbar.RecordStatus <> 'DELETED' " +
            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID AND inventory_unit.RecordStatus <> 'DELETED' " +
            " LEFT JOIN list_units ON list_units.ID=inventory_unit.ItemUnit AND list_units.RecordStatus <> 'DELETED' " +
            " WHERE inventory.RecordStatus <> 'DELETED'";
        if (fieldCriteria.Length > 0)
        {
            strSql += " AND (" + fieldCriteria + ")";
        }
        strSql += " Order By UOM";

        MySqlParameter[] objparam =
        {
            new MySqlParameter("@ItemSKU", ItemSKU)
        };

        DataSet inventoryDS = dataAccessor.RunSPRetDataset_Vapt(strSql, "InventoryUOM", objparam);

        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);

        return json;
    }
    
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getInventoryMultipleUOM(string companyCode, string retailID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        List<getInventory> getInventories = new List<getInventory>();
        string sql = "SELECT inventory_retail.ItemID as ItemID, inventory.ItemSKU as ItemSKU, inventory.ItemDescp as ItemDescp, inventory_supbar.SupBarCode as SupBarCode " +
            " FROM inventory_retail" +
            " LEFT JOIN inventory ON inventory_retail.ItemID = inventory.ItemID" +
            " LEFT JOIN inventory_supbar ON inventory_supbar.ItemID = inventory_retail.ItemID AND inventory_supbar.RecordStatus <> 'DELETED' " +
            " WHERE inventory_retail.RecordStatus <> 'DELETED' AND inventory_retail.RetailID=@RetailID";

        MySqlParameter[] objparam =
        {
            new MySqlParameter("@RetailID", retailID)
        };

        DataTable dt = dataAccessor.RunSPRetDataset_Vapt(sql, "inventory_retail", objparam).Tables[0];
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            getInventory Item = new getInventory
            {
                ItemID = Convert.ToString(dt.Rows[i]["ItemID"])
                ,
                ItemSKU = Convert.ToString(dt.Rows[i]["ItemSKU"])
                ,
                ItemDescp = Convert.ToString(dt.Rows[i]["ItemDescp"])
                ,
                Suppliers = dataAccessor.GetSuppliers(Convert.ToString(dt.Rows[i]["ItemID"]))
                ,
                Prices = dataAccessor.GetPrices(Convert.ToString(dt.Rows[i]["ItemID"]), retailID)
            };
            getInventories.Add(Item);
        }
        var json = new JavaScriptSerializer().Serialize(getInventories);

        return json;
    }

	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitSales(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        List<SalesMaster> list_sales = new List<SalesMaster>();
        //SalesMaster sales = new SalesMaster();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        /*string jSon = @"{
            ""TransNo"":""Testing181212"",
            ""RetailID"":""10"", 
            ""SalesDate"":""2018-12-12"", 
            ""SalesStatus"":""SALES"", 
            ""MemberID"":""9362200101"", 
            ""SalesTaxTtl"":""0.660000"", 
            ""SalesRounding"":""0"", 
            ""ItemSales"": [{
                ""ItemID"":""396"",""ItemQty"":""2"",""ItemUOMDesc"":""Bowl (200g)"",""ItemPrice"":""5"",""ItemDisc"":""0"",""ItemTax"":""0.70"",""ItemTotal"":""10""},
                {""ItemID"":""396"",""ItemQty"":""4"",""ItemUOMDesc"":""Bowl (200g)"",""ItemPrice"":""5"",""ItemDisc"":""5"",""ItemTax"":""1.40"",""ItemTotal"":""20""
            }],
            ""SalesPayments"": [
				{""strPayment"":""CASH"",""SalesPayTtl"":""50"",""SalesBalTtl"":""5"",""ChangeAmount"":""45""},
                {""strPayment"":""NETS"",""SalesPayTtl"":""5"",""SalesBalTtl"":""5"",""ChangeAmount"":""0""}
            ]
        }";*/

        try
        {
            list_sales = serializer.Deserialize<List<SalesMaster>>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveSales(list_sales);

        return response;

    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitAdroindSales(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        SalesMaster sales = new SalesMaster();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        /*string jSon = @"{
            ""TransNo"":""Testing181212"",
            ""RetailID"":""10"", 
            ""SalesDate"":""2018-12-12"", 
            ""SalesStatus"":""SALES"", 
            ""MemberID"":""9362200101"", 
            ""SalesTaxTtl"":""0.660000"", 
            ""SalesRounding"":""0"", 
            ""ItemSales"": [{
                ""ItemID"":""396"",""ItemQty"":""2"",""ItemUOMDesc"":""Bowl (200g)"",""ItemPrice"":""5"",""ItemDisc"":""0"",""ItemTax"":""0.70"",""ItemTotal"":""10""},
                {""ItemID"":""396"",""ItemQty"":""4"",""ItemUOMDesc"":""Bowl (200g)"",""ItemPrice"":""5"",""ItemDisc"":""5"",""ItemTax"":""1.40"",""ItemTotal"":""20""
            }],
            ""SalesPayments"": [
				{""strPayment"":""CASH"",""SalesPayTtl"":""50"",""SalesBalTtl"":""5"",""ChangeAmount"":""45""},
                {""strPayment"":""NETS"",""SalesPayTtl"":""5"",""SalesBalTtl"":""5"",""ChangeAmount"":""0""}
            ]
        }";*/

        try
        {
            sales = serializer.Deserialize<SalesMaster>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveAndroidSales(sales);

        return response;

    }

	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public decimal getItemPromotion(string companyCode, DateTime SalesDate, string ItemSKU, string RetailID, decimal Qty, string UOM)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string fieldCriteria = "";
        if (SalesDate != null)
        {
            fieldCriteria = " AND promo.Promo_DateFrom<=@DateCond AND promo.Promo_DateTo>=@DateCond ";
        }
        if (RetailID != null)
        {
            fieldCriteria = "AND (promo.Promo_RetailID = '0' AND promo.Promo_RetailID = @RetailID) ";
        }
        if (UOM != null)
        {
            fieldCriteria = "AND promo_item.ItemUOM = @ItemUOM ";
        }
        if (ItemSKU != "" || ItemSKU != "0")
        {
            fieldCriteria = " AND (inventory.ItemSKU = @ItemSKU OR inventory_supbar.SupBarCode = @ItemSKU)";
        }

        string strSql = "SELECT inventory_unit.RTLSellPx, promo_item.Item_Qty,promo_item.Item_Amt,promo_item.Item_Percentage,promo_item.Item_MemberAmt,promo_item.Item_MemberPerc," +
            "promo_item.Item_Qty2,promo_item.Item_Amt2,promo_item.Item_Percentage2,promo_item.Item_MemberAmt2,promo_item.Item_MemberPerc2," +
            "promo_item.Item_Qty3,promo_item.Item_Amt3,promo_item.Item_Percentage3,promo_item.Item_MemberAmt3,promo_item.Item_MemberPerc3," +
            "promo_item.Item_Qty4,promo_item.Item_Amt4,promo_item.Item_Percentage4,promo_item.Item_MemberAmt4,promo_item.Item_MemberPerc4," +
            "promo_item.Item_Qty5,promo_item.Item_Amt5,promo_item.Item_Percentage5,promo_item.Item_MemberAmt5,promo_item.Item_MemberPerc5," +
            "promo_item.Item_Qty6,promo_item.Item_Amt6,promo_item.Item_Percentage6,promo_item.Item_MemberAmt6,promo_item.Item_MemberPerc6," +
            "promo_item.Item_Qty7,promo_item.Item_Amt7,promo_item.Item_Percentage7,promo_item.Item_MemberAmt7,promo_item.Item_MemberPerc7," +
            "promo_item.Item_Qty8,promo_item.Item_Amt8,promo_item.Item_Percentage8,promo_item.Item_MemberAmt8,promo_item.Item_MemberPerc8," +
            "promo_item.Item_Qty9,promo_item.Item_Amt9,promo_item.Item_Percentage9,promo_item.Item_MemberAmt9,promo_item.Item_MemberPerc9," +
            "promo_item.Item_Qty10,promo_item.Item_Amt10,promo_item.Item_Percentage10,promo_item.Item_MemberAmt10,promo_item.Item_MemberPerc10" +
            " FROM promo_item " +
            " LEFT JOIN promo ON promo_item.PromoID = promo.PromoID AND promo.RecordStatus <> 'DELETED' " +
            " LEFT JOIN inventory ON promo_item.ItemID = inventory.ItemID AND inventory.RecordStatus <> 'DELETED' " +
            " LEFT JOIN inventory_unit ON promo_item.itemID = inventory_unit.ItemID AND inventory_unit.Item_UnitID = promo_item.Item_UnitID " +
            " LEFT JOIN inventory_supbar ON inventory_supbar.ItemID = inventory.ItemID AND inventory_supbar.RecordStatus <> 'DELETED' " +
            " WHERE promo_item.RecordStatus <> 'DELETED' " + fieldCriteria;

        MySqlParameter[] objparam =
        {
            new MySqlParameter("@DateCond", SalesDate.Date),
            new MySqlParameter("@RetailID", RetailID),
            new MySqlParameter("@ItemUOM", UOM),
            new MySqlParameter("@ItemSKU", ItemSKU)
        };
        decimal maxTierPromoPrice = dataAccessor.calcPromoPrice(strSql, Qty);

        return maxTierPromoPrice;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getPromotion(string companyCode) //, string RetailID
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        List<Promotion> getPromoitons = new List<Promotion>();

        string sql = "SELECT PromoID,Promo_RetailID,Promo_DateFrom,Promo_DateTo,Promo_Name,Promo_Type," +
                    "(SELECT Promo_TypeCode FROM promo_type WHERE Promo_TypeID=Promo_Type) AS Promo_TypeCode," +
                    "Promo_Priority FROM promo WHERE RecordStatus<>'DELETED' AND Display='Y' " ; //AND Promo_RetailID='" + RetailID + "'";
        DataTable dt = dataAccessor.GetData(sql);
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            Promotion Promo = new Promotion
            {
                PromoID = Convert.ToString(dt.Rows[i]["PromoID"]),
                PromoName = Convert.ToString(dt.Rows[i]["Promo_Name"]),
                Promo_Type_ID = Convert.ToString(dt.Rows[i]["Promo_Type"]),
                Promo_TypeCode = Convert.ToString(dt.Rows[i]["Promo_TypeCode"]),
                Promo_DateFrom = Convert.ToString(dt.Rows[i]["Promo_DateFrom"]),
                Promo_DateTo = Convert.ToString(dt.Rows[i]["Promo_DateTo"]),
                PromoRetails = dataAccessor.GetPromoRetailer(Convert.ToString(dt.Rows[i]["PromoID"])),
                PromoItems = dataAccessor.GetPromoItems(Convert.ToString(dt.Rows[i]["PromoID"])),
                PromoMixMatch = dataAccessor.GetPromoMixMatch(Convert.ToString(dt.Rows[i]["PromoID"]))
            };
            getPromoitons.Add(Promo);
        }
        var json = new JavaScriptSerializer().Serialize(getPromoitons);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getMember(string companyCode, string find1, string find2)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string fieldCriteria1 = "";
        string fieldCriteria2 = "";
        if (find1 == "" && find2 == "")
        {
            return "Please provide search criteria.";
        }
        else
        {
            if (find1 != null)
            {
                fieldCriteria1 = " (CustICNO LIKE CONCAT('%',@Find1,'%') OR email LIKE CONCAT('%',@Find1,'%') OR hph LIKE CONCAT('%',@Find1,'%') OR cardnumber LIKE CONCAT('%',@Find1,'%') OR customerFirstName LIKE CONCAT('%',@Find1,'%') OR customerLastName LIKE CONCAT('%',@Find1,'%'))";
            }
            if (find2 != null)
            {
                if (find1 == null)
                {
                    fieldCriteria2 = " (CustICNO LIKE CONCAT('%',@Find2,'%') OR email LIKE CONCAT('%',@Find2,'%') OR hph LIKE CONCAT('%',@Find2,'%') OR cardnumber LIKE CONCAT('%',@Find2,'%') OR customerFirstName LIKE CONCAT('%',@Find2,'%') OR customerLastName LIKE CONCAT('%',@Find2,'%'))";
                }
                else
                {
                    fieldCriteria2 = " AND (CustICNO LIKE CONCAT('%',@Find2,'%') OR email LIKE CONCAT('%',@Find2,'%') OR hph LIKE CONCAT('%',@Find2,'%') OR cardnumber LIKE CONCAT('%',@Find2,'%') OR customerFirstName LIKE CONCAT('%',@Find2,'%') OR customerLastName LIKE CONCAT('%',@Find2,'%'))";
                }
            }
        }
        string strSql = "SELECT ID AS MemberID,custcode, CustICNO, Email, hph as MobileNo, cardnumber, CustomerDOB AS DOB, customerFirstName as FirstName, customerLastName AS LastName, " +
                        "CustomerAddress1 AS Address1, CustomerAddress2  AS Address2, CustomerAddress3  AS Address3, CustomerPostcode AS PostalCode, OpeningLP " +
                        " FROM customer" +
                        " WHERE RecordStatus <> 'DELETED' AND " + fieldCriteria1 + fieldCriteria2 +
                        " Order By custcode,CustICNO,Email";

        MySqlParameter[] objparam =
        {
            new MySqlParameter("@Find1", find1),
            new MySqlParameter("@Find2", find2)
        };

        DataSet memberDS = dataAccessor.RunSPRetDataset_Vapt(strSql, "Member", objparam);

        string json = JsonConvert.SerializeObject(memberDS, Formatting.Indented);

        return json;
    }
	
	
	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitOrders(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        SalesOrder orders = new SalesOrder();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        /*string jSon = @"{
            ""RefID"":""Testing181212"",
            ""RefNo"":""ABC12345"", 
            ""QueueNo"":""N01"", 
            ""RetailID"":""1"",  
            ""TerminalID"":""1"", 
            ""ItemOrder"": [{
                ""ParentID"":""396"",
                ""SupBarCode"":""2"",
                ""ParentQty"":""2"",
                ""ItemUOM"":""Bowl (200g)"",
                ""ItemOrderDetails"": [{""ItemID"":""396"",""ItemQty"":""2""}]},
                {""ParentID"":""781"",""SupBarCode"":""8886469723894"",""ParentQty"":""2"",""ItemUOM"":""Pkt (30g)"",""ItemOrderDetails"":[]
            }]
        }";*/

        try
        {
            orders = serializer.Deserialize<SalesOrder>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        //return orders;
        response = dataAccessor.saveOrders(orders);

        return response;
    }
	
	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string cancelOrders(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        CancelOrder calOrder = new CancelOrder();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        /*string jSon = @"{
            ""RefID"":""Testing181212"",
            ""RefNo"":""ABC12345"", 
            ""RetailID"":""1"",  
            ""ParentItemID"":""781""
        }";*/

        try
        {
            calOrder = serializer.Deserialize<CancelOrder>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
		
        response = dataAccessor.cancelOrders(calOrder);

        return response;
    }

	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitPurchaseDelivery(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        purchaseDelivery PD = new purchaseDelivery();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        /*
		string jSon = @"{
            ""IDRef"":""PDO/100001/2018"",
            ""DONo"":""123456"",
            ""PDItems"": [{
                ""ItemSKU"":""SIKN141"",""SupBarCode"":""8886469719200"",""ItemUOM"":""Gram"",""ItemQty"":""5000"",""ItemPrice"":""0.0100""},
                {""ItemSKU"":""VFV102"",""SupBarCode"":""8886469711686"",""ItemUOM"":""Bot (600mL)"",""ItemQty"":""12"",""ItemPrice"":""4""
            }]
        }";*/

        try
        {
            PD = serializer.Deserialize<purchaseDelivery>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.savePD(PD);

        return response;
    }
	
	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitSalesDelivery_v3(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        SalesDelivery SD = new SalesDelivery();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

      /*  string jSon = @"{
            ""INVID"":""812112"",
            ""INVRef"":""ABC133451"", 
            ""TransDate"":""2019-01-04 12:07:44"", 
            ""RecipientName"":""Jash Lim"",  
            ""RecipientAddress"":""katong shopping centre"", 
            ""RecipientPostCode"":""909031"",  
            ""RecipientAttn"":""Jash"", 
            ""RecipientTel"":""098321382"", 
            ""Remark"":""attach wish card"", 
            ""GST"":""Y"",
            ""SalesDeliveryItems"": [{
                ""ItemID"":""640"",
                ""SupBarItemID"":""626"",
                ""ItemQty"":""2"",
                ""ItemPrice"":""9.99"",
                ""ItemUOM"":""Pkt (500g)""},
				{
                ""ItemID"":""733"",
                ""SupBarItemID"":""719"",
                ""ItemQty"":""2"",
                ""ItemPrice"":""19.99"",
                ""ItemUOM"":""Pkt (100g)""}]
        }";
*/
        try
        {
            SD = serializer.Deserialize<SalesDelivery>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        //return SD;
        response = dataAccessor.saveSalesDelivery_v3(SD);

        return response;
    }


    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitSalesDelivery_v4(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        SalesDelivery SD = new SalesDelivery();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

 /*       string jSon = @"{
            ""INVID"":""182112"",
            ""INVRef"":""ABC133451"", 
            ""TransDate"":""2019-01-04 12:07:44"", 
            ""RecipientName"":""Jash Lim"",  
            ""RecipientAddress"":""katong shopping centre"", 
            ""RecipientPostCode"":""909031"",  
            ""RecipientAttn"":""Jash"", 
            ""RecipientTel"":""098321382"", 
            ""Remark"":""attach wish card"", 
            ""GST"":""Y"",
            ""SalesDeliveryItems"": [{
                ""ItemID"":""640"",
                ""SupBarItemID"":""626"",
                ""ItemQty"":""2"",
                ""ItemPrice"":""9.99"",
                ""ItemUOM"":""Pkt (500g)""},
				{
                ""ItemID"":""733"",
                ""SupBarItemID"":""719"",
                ""ItemQty"":""2"",
                ""ItemPrice"":""19.99"",
                ""ItemUOM"":""Pkt (100g)""}]
        }";
*/

        try
        {
            SD = serializer.Deserialize<SalesDelivery>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        //return SD;
        response = dataAccessor.saveSalesDelivery_v4(SD);

        return response;
    }


    public List<ItemPrice> GetPrices(string companyCode, string ItemID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        List<ItemPrice> prices = new List<ItemPrice>();
        //DataTable dt = GetData(string.Format("SELECT Item_UnitID,RTLSellPx FROM inventory_unit Where RecordStatus <> 'DELETED' AND ItemID ='{0}'", ItemID));
        DataTable dt = dataAccessor.GetData(string.Format("SELECT Item_UnitID,(SELECT Nick FROM list_units WHERE ID= ItemUnit) AS UOM ,RTLSellPx,ItemUnitDef " +
                                " FROM  inventory_unit WHERE recordstatus<>'DELETED' AND ItemID='{0}' ORDER BY ItemID ASC, ItemUnitDef DESC ", ItemID));
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            prices.Add(new ItemPrice
            {
                Item_UnitID = Convert.ToString(dt.Rows[i]["Item_UnitID"])
                ,
                UOM = Convert.ToString(dt.Rows[i]["UOM"])
                ,
                RTLSellPx = Convert.ToDecimal(dt.Rows[i]["RTLSellPx"])
                ,
                ItemUnitDef = Convert.ToString(dt.Rows[i]["ItemUnitDef"])
            });
        }
        return prices;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdatePD(string companyCode)
    {
        /*clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        List<string[]> terminalList;
        string retailCriteria = "";
        if (RetailID != "0")
        {
            terminalList = getAllTerminals(dataAccessor, "0");
            retailCriteria = " AND IR.RetailID=" + RetailID;
        }
        else
        {
            terminalList = getAllTerminals(dataAccessor, RetailID);
        }
        string strSql = "";
        string itemCriteria = "";
        string ItemID;
        if (fieldID != "0")
        {
            strSql = "SELECT ItemID FROM inventory WHERE " + field + "=" + fieldID;
            ItemID = dataAccessor.ExecScalarVal(strSql, "").ToString();
            itemCriteria = " AND IR.ItemID=" + ItemID;
        }

        strSql = "SELECT IR.ItemID, IR.RetailID, SUM(Qty) AS OnHandQty FROM inventory_retail IR LEFT JOIN inventory_tran IT" +
                " ON IR.ItemID=IT.ItemID AND IR.RetailID=IT.RetailID" +
                " WHERE 1=1 " + itemCriteria + retailCriteria +
                " GROUP BY IR.ItemID, IR.RetailID";
        DataSet inventoryDS = dataAccessor.RunSPRetDataset(strSql, "inventory_tran");
        string OnHandQty, criteria;
        foreach (DataRow dsTableRow in inventoryDS.Tables[0].Rows)
        {
            ItemID = dsTableRow["ItemID"].ToString();
            RetailID = dsTableRow["RetailID"].ToString();
            OnHandQty = dsTableRow["OnHandQty"].ToString();
            criteria = " WHERE ItemID=" + ItemID + " AND RetailID=" + RetailID;
            strSql = "UPDATE inventory_retail SET OnHandQty='" + OnHandQty + "' " + criteria;
            dataAccessor.Exec_UpdateQuery(strSql, "");
            SendMessages(dataAccessor, companyCode, "inventory_retail", criteria, terminalList);
        }
        return inventoryDS;*/
        return companyCode;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CreateItem(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT ItemSKU AS SKU,ItemName,ItemDescp,ItemOtherLanguage," +
                        "(SELECT Nick FROM list_units WHERE ID=ItemUnit) AS UOM," +
                        "(SELECT VALUE FROM list_inv_departments WHERE ID= ItemDepartment) AS Department," +
                        "(SELECT VALUE FROM list_brand WHERE ID= ItemBrand) AS Brand," +
                        "IF((SELECT VALUE FROM list_inv_groups WHERE ID=ItemGroup) IS NULL,'',(SELECT VALUE FROM list_inv_groups WHERE ID=ItemGroup)) AS GroupDesc," +
                        "eItemSPV01 AS CategoryCode1,eItemSPV02 AS CategoryCode2,eItemSPV03 AS CategoryCode3,eItemSPV04 AS CategoryCode4,eItemSPV05 AS CategoryCode5," +
                        "eItemSPV06 AS CategoryCode6,eItemSPV07 AS CategoryCode7,eItemSPV08 AS CategoryCode8,eItemSPV09 AS CategoryCode9,eItemSPV10 AS CategoryCode10," +
                        "ItemRtlPx,ItemRtlPx2 FROM tblexportinventory"; 
        DataSet inventoryDS = dataAccessor.RunSPRetDataset(strSql, "inventory");

        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);
        //List<string[]> asd = inventoryDS;

        //return asd;
        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CreateMember(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT custcode, CustomerFirstName AS FirstName,CustomerMiddleName AS MiddleName,CustomerLastName AS LastName,CustomerMetaName As NickName,CustomerOtherLanguageName AS OtherName," +
                        "CustomerSexID AS Sex,CustICNO AS NRIC,CustomerDOB AS DOB,IF((SELECT VALUE FROM list_classifications WHERE ID= CustomerClassificationID) IS NULL,'',(SELECT VALUE FROM list_classifications WHERE ID= CustomerClassificationID)) AS Classification," +
                        "IF((SELECT VALUE FROM list_designations WHERE ID= CustomerDesignationID) IS NULL,'',(SELECT VALUE FROM list_designations WHERE ID= CustomerDesignationID)) AS Designation," +
                        "IF((SELECT VALUE FROM list_departments WHERE ID= CustomerDepartmentID) IS NULL,'',(SELECT VALUE FROM list_departments WHERE ID= CustomerDepartmentID)) AS Department," +
                        "hph_CountryCode,hph_AreaCode, hph,CustomerAddress1 AS Address1,CustomerAddress2 AS Address2,CustomerAddress3 AS Address3," +
                        "CustomerPostcode AS PostalCode,(SELECT Nick FROM list_countries WHERE ID=CustomerCountryID) AS Country," +
                        "CustomerStartDate AS StartDate,CustomerEndDate AS EndDate,CustomerID AS MemberID FROM tblexportcustomer"; 
        DataSet memberDS = dataAccessor.RunSPRetDataset(strSql, "member");

        string json = JsonConvert.SerializeObject(memberDS, Formatting.Indented);
        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CombineSellRetrieveOrder(string companyCode, string LoginID, string Pswd, string fromDate, string toDate)
    {

        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        List<OrderReturn> OrderReturns = new List<OrderReturn>();
        
        //Login to get token
        //LoginID = "dcs@combinesell.com";
        //Pswd = "gnPeotrWVqT1YRHX";
        var token = "";
        var token_type = "";
        string responseStr = "";
        string loginurl = "https://app.combinesell.com/api/auth/login";

        var postData =
            new
            {
                email = LoginID.ToString(),
                password = Pswd.ToString()
            };
        string login = JsonConvert.SerializeObject(postData, Formatting.Indented);
        //return login;
        try
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(loginurl);
            //WebHeaderCollection myWebHeaderCollection = request.Headers;
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.ContentLength = login.Length;
            request.Method = "POST";//type is POST
            //request.Referer = loginurl;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {

                streamWriter.Write(login);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                responseStr = streamReader.ReadToEnd();
                SuccessLogin success = JsonConvert.DeserializeObject<SuccessLogin>(responseStr);
                token = success.access_token;
                token_type = success.token_type;
            }
        }
        catch (Exception ex)
        {
            //return "Error : " + ex.Message.ToString();
            OrderReturn orderreturns = new OrderReturn()
            {
                Message = "Error: Wrong username or password. Please try again..."
            };
            OrderReturns.Add(orderreturns);
            string json = JsonConvert.SerializeObject(orderreturns, Formatting.Indented);
            return json;
            //return "Error: Wrong username or password. Please try again...";
        }

        //return token;

        //Get orders
        //fromDate = "2019-01-01" + " 00:00:00";
        //toDate = System.DateTime.Now.Date.ToString("yyyy-MM-dd") + " 23:59:59";
        fromDate = fromDate + " 00:00:00";
        toDate = toDate + " 23:59:59";
        string txtstatus = "All";
        string limit = "limit=50";
        string page = "&page=1";
        string updateFromDate = "&updated_at_min=" + HttpUtility.UrlEncode(fromDate);
        string updateToDate = "&updated_at_max=" + HttpUtility.UrlEncode(toDate);
        string createFromDate = "&created_at_min=" + HttpUtility.UrlEncode(fromDate);
        string createToDate = "&created_at_max=" + HttpUtility.UrlEncode(toDate);
        string status = "";
        string search = "";
        if(txtstatus == "All")
        {
            status = "";
            search = "";
        }
        else
        {
            status = "&status=" + txtstatus;
			search = "&search=" + txtstatus;
        }
		string orderUrl = "https://app.combinesell.com/api/orders?" + limit + page + updateFromDate + updateToDate + createFromDate + createToDate + status + search;
        string shopid = "";

        string sqlShopID = "SELECT RetailID, RetailCode, Shop_id FROM retailer WHERE RetailType = 'ONLINE' AND RecordStatus <> 'DELETED' AND Shop_id <> '0'";
        DataTable ShopIDDT = dataAccessor.GetData(string.Format(sqlShopID));
        if (ShopIDDT.Rows.Count != 0)
        {
            shopid = Convert.ToString(ShopIDDT.Rows[0]["Shop_id"]);
        }
        else
        {
            return "Shop_ID cannot found";
        }

        //string authorization = "Authorization: " +token_type +" " +token;
		//string shop = "Shop-Id: " +shopid;

        //return toDate;
        try
        {
            HttpWebRequest requestOrder = (HttpWebRequest)WebRequest.Create(orderUrl);
            //also can use below method
            //WebHeaderCollection myWebHeaderCollectionOrder = requestOrder.Headers;
            //myWebHeaderCollectionOrder.Add("Authorization", token_type + " " + token);
            //myWebHeaderCollectionOrder.Add("Shop-Id", shopid);
            requestOrder.Accept = "application/json";
            requestOrder.Method = "GET";//type is POST
            requestOrder.Headers.Add("Authorization", token_type + " " + token);
            requestOrder.Headers.Add("Shop-Id", shopid);
            requestOrder.Referer = orderUrl;


            var httpResponse = (HttpWebResponse)requestOrder.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                responseStr = streamReader.ReadToEnd();
                try
                {
                    OrderResponse orderResponse = JsonConvert.DeserializeObject<OrderResponse>(responseStr);
                    var responseQWE = dataAccessor.CombineSellOrder(orderResponse);

                    return responseQWE;
                }
                catch (Exception ex)
                {
                    //return "Error : " + ex.Message.ToString();
                    //return "Error : No new order record";
                    OrderReturn orderreturns = new OrderReturn()
                    {
                        Message = "No new order record."
                    };
                    OrderReturns.Add(orderreturns);
                    string json = JsonConvert.SerializeObject(orderreturns, Formatting.Indented);
                    return json;
                }

                //target framework 4, framework 3.5 cannot work
                /*
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseStr);
                var deserialized = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, dynamic>>>(responseStr);
                List<object> values = deserialized.SelectMany(result => result.Value).Cast<object>().ToList();*/

                //return responseStr;
            }

        }
        catch (Exception ex)
        {
            //return "Error : " + ex.Message.ToString();
            OrderReturn orderreturns = new OrderReturn()
            {
                Message = "Error : " + ex.Message.ToString()
            };
            OrderReturns.Add(orderreturns);
            string json = JsonConvert.SerializeObject(orderreturns, Formatting.Indented);
            return json;
        }


        /*
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(loginurl);
            WebHeaderCollection myWebHeaderCollection = request.Headers;
            
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.ContentLength = login.Length;
            request.Method = "POST";//type is POST
            request.Referer = loginurl;

            byte[] byteArray = Encoding.UTF8.GetBytes(login);//要發送的字串轉為byte[]
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }

            string responseStr = "";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                responseStr = reader.ReadToEnd();
                //var asd = JsonConvert.DeserializeObject<dynamic>(responseStr);
                SuccessLogin success = JsonConvert.DeserializeObject<SuccessLogin>(responseStr);

                return success.access_token;
            }
        }
        catch (Exception ex)
        {
            //return "Error : " + ex.Message.ToString();
            return "Error: Wrong username or password. Please try again...";
        }*/
        /*
        string responseStr = "";
        try
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            //string postData = "user=" + user + "&pass=" + pass;
            byte[] data = Encoding.GetEncoding("UTF-8").GetBytes(login);

            WebRequest request = WebRequest.Create(loginurl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();

            WebResponse response = request.GetResponse();
            stream = response.GetResponseStream();

            StreamReader sr = new StreamReader(stream);
            responseStr = sr.ReadToEnd();
            SuccessLogin success = JsonConvert.DeserializeObject<SuccessLogin>(responseStr);
            sr.Close();
            stream.Close();
            token = success.access_token;
            return success.access_token;
        }
        catch (Exception ex)
        {
            return "Error : " + ex.Message.ToString();
        }
        */        
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CombineSellAcceptOrder(string companyCode, string LoginID, string Pswd, string SDID, string RetailID, string User)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        List<OrderReturn> OrderReturns = new List<OrderReturn>();

        //Login to get token
        //LoginID = "dcs@combinesell.com";
        //Pswd = "gnPeotrWVqT1YRHX";
        var token = "";
        var token_type = "";
        string responseStr = "";
        string loginurl = "https://app.combinesell.com/api/auth/login";

        var postData =
            new
            {
                email = LoginID.ToString(),
                password = Pswd.ToString()
            };
        string login = JsonConvert.SerializeObject(postData, Formatting.Indented);
        //return login;
        try
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(loginurl);
            //WebHeaderCollection myWebHeaderCollection = request.Headers;
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.ContentLength = login.Length;
            request.Method = "POST";//type is POST
            //request.Referer = loginurl;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {

                streamWriter.Write(login);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                responseStr = streamReader.ReadToEnd();
                SuccessLogin success = JsonConvert.DeserializeObject<SuccessLogin>(responseStr);
                token = success.access_token;
                token_type = success.token_type;
            }
        }
        catch (Exception ex)
        {
            //return "Error : " + ex.Message.ToString();
            OrderReturn orderreturns = new OrderReturn()
            {
                Message = "Error: Wrong username or password. Please try again..."
            };
            OrderReturns.Add(orderreturns);
            string json = JsonConvert.SerializeObject(orderreturns, Formatting.Indented);
            return json;
            //return "Error: Wrong username or password. Please try again...";
        }
        //return token;

        string IDRef = "";
        string PONo = "";
        //SDID = "d182880b-64c1-4116-acfa-73bc6c407005";
        //checkSD responseQWE = dataAccessor.checkSD(SDID);
        string sqlSD = "SELECT IDRef, PONo FROM salesdelivery WHERE ID='{0}'";
        DataTable SDDT = dataAccessor.GetData(string.Format(sqlSD,SDID));
        if (SDDT.Rows.Count != 0)
        {
            IDRef = Convert.ToString(SDDT.Rows[0]["IDRef"]);
            PONo = Convert.ToString(SDDT.Rows[0]["PONo"]);
        }
        else
        {
            return "this record cannot found";
        }

        string strResponse = dataAccessor.CombineSellAcceptOrder(RetailID, User, SDID, IDRef);

        string AcceptOrderUrl = "https://app.combinesell.com/api/orders/" + PONo +"/ship?tracking_number=" +HttpUtility.UrlEncode(IDRef);
        
        string shopid = "";

        string sqlShopID = "SELECT RetailID, RetailCode, Shop_id FROM retailer WHERE RetailType = 'ONLINE' AND RecordStatus <> 'DELETED' AND Shop_id <> '0'";
        DataTable ShopIDDT = dataAccessor.GetData(string.Format(sqlShopID));
        if (ShopIDDT.Rows.Count != 0)
        {
            shopid = Convert.ToString(ShopIDDT.Rows[0]["Shop_id"]);
        }
        else
        {
            return "Shop_ID cannot found";
        }


        var shipData =
            new
            {
                tracking_number = IDRef,
                address_id = "",
                pickup_time_id = "",
                branch_id = "",
                sender_real_name = "",
                delivery_type = "",
                shipment_providers = ""
            };
        string ship = JsonConvert.SerializeObject(shipData, Formatting.Indented);



        try
        {
            HttpWebRequest requestAcceptOrder = (HttpWebRequest)WebRequest.Create(AcceptOrderUrl);
            requestAcceptOrder.Accept = "application/json";
            requestAcceptOrder.ContentType = "application/json";
            requestAcceptOrder.ContentLength = ship.Length;
            requestAcceptOrder.Method = "POST";//type is POST
            requestAcceptOrder.Headers.Add("Authorization", token_type + " " + token);
            requestAcceptOrder.Headers.Add("Shop-Id", shopid);
            requestAcceptOrder.Referer = AcceptOrderUrl;

            using (var streamWriter = new StreamWriter(requestAcceptOrder.GetRequestStream()))
            {

                streamWriter.Write(ship);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)requestAcceptOrder.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                try
                {
                    return responseStr = streamReader.ReadToEnd();
                    //string AcceptOrderResponse = JsonConvert.DeserializeObject<string>(responseStr);
                    //var responseQWE = dataAccessor.CombineSellOrder(orderResponse);
                    //return AcceptOrderResponse;
                    //return responseQWE;
                }
                catch (Exception ex)
                {
                    return "Error : " + ex.Message.ToString();
                    //return "Error : No new order record";
                }
                //return responseStr;
            }

        }
        catch (Exception ex)
        {
            return "Error : " + ex.Message.ToString();
        }

    }


    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CombineSellCancelOrder(string companyCode, string LoginID, string Pswd, string SDID, string RetailID, string User, string Reason, string Remark)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        List<OrderReturn> OrderReturns = new List<OrderReturn>();

        //Login to get token
        //LoginID = "dcs@combinesell.com";
        //Pswd = "gnPeotrWVqT1YRHX";
        var token = "";
        var token_type = "";
        string responseStr = "";
        string loginurl = "https://app.combinesell.com/api/auth/login";

        var postData =
            new
            {
                email = LoginID.ToString(),
                password = Pswd.ToString()
            };
        string login = JsonConvert.SerializeObject(postData, Formatting.Indented);
        //return login;
        try
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(loginurl);
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.ContentLength = login.Length;
            request.Method = "POST";//type is POST
            //request.Referer = loginurl;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {

                streamWriter.Write(login);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                responseStr = streamReader.ReadToEnd();
                SuccessLogin success = JsonConvert.DeserializeObject<SuccessLogin>(responseStr);
                token = success.access_token;
                token_type = success.token_type;
            }
        }
        catch (Exception ex)
        {
            //return "Error : " + ex.Message.ToString();
            return "Error: Wrong username or password. Please try again...";
        }
        //return token;



        string IDRef = "";
        string PONo = "";
        SDID = "b2725586-a104-4719-9f63-5563db536e66";
        //checkSD responseQWE = dataAccessor.checkSD(SDID);
        string sqlSD = "SELECT IDRef, PONo FROM salesdelivery WHERE ID='{0}'";
        DataTable SDDT = dataAccessor.GetData(string.Format(sqlSD, SDID));
        if (SDDT.Rows.Count != 0)
        {
            IDRef = Convert.ToString(SDDT.Rows[0]["IDRef"]);
            PONo = Convert.ToString(SDDT.Rows[0]["PONo"]);
        }
        else
        {
            return "This record cannot found";
        }

        string CancelOrderUrl = "https://app.combinesell.com/api/orders/" + PONo + "/cancel?reason=" + HttpUtility.UrlEncode(Reason) + "&remark=" + HttpUtility.UrlEncode(Remark);
        string shopid = "216";

        string sqlShopID = "SELECT RetailID, RetailCode, Shop_id FROM retailer WHERE RetailType = 'ONLINE' AND RecordStatus <> 'DELETED' AND Shop_id <> '0'";
        DataTable ShopIDDT = dataAccessor.GetData(string.Format(sqlShopID));
        if (ShopIDDT.Rows.Count != 0)
        {
            shopid = Convert.ToString(ShopIDDT.Rows[0]["Shop_id"]);
        }
        else
        {
            return "Shop_ID cannot found";
        }

        try
        {
            HttpWebRequest requestCancelOrder = (HttpWebRequest)WebRequest.Create(CancelOrderUrl);
            requestCancelOrder.Accept = "application/json";
            requestCancelOrder.Method = "POST";//type is POST
            requestCancelOrder.Headers.Add("Authorization", token_type + " " + token);
            requestCancelOrder.Headers.Add("Shop-Id", shopid);
            requestCancelOrder.Referer = CancelOrderUrl;

            var httpResponse = (HttpWebResponse)requestCancelOrder.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                responseStr = streamReader.ReadToEnd();
                //return responseStr;
                try
                {
                    CancelOrderResponse cancelOrderResponse = JsonConvert.DeserializeObject<CancelOrderResponse>(responseStr);
                    if(cancelOrderResponse.meta.error == false)
                    {
                        dataAccessor.CombineSellCancelOrder(RetailID,User,IDRef,PONo);
                    }
                    
                    return cancelOrderResponse.meta.message;
                }
                catch (Exception ex)
                {
                    return "Error : " + ex.Message.ToString();
                    //return "Error : No new order record";
                }
                //return responseStr;
            }

        }
        catch (Exception ex)
        {
            return "Error : " + ex.Message.ToString();
        }
    }
    
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string EnvolvePostSales(string companyCode, string json)
    {
        
        clsDataAccessor dataAccessor = new clsDataAccessor();
        if(companyCode != "")
        {
            dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        }


        var ApiKey = "j4QnkCuSpm3skdHBvOzGCaNanVbwsWJz1nCMhX7l";
        var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VyIjoic2x5dHBoOXVvOCIsImV4cCI6MTU3ODg3MzYwMH0.pXFDLrqJNV1uQ-_I4PZPjEzqUjG_wYh-GhrKj0GwWwM";
        var token_type = "Bearer";

        string responseStr = "";
        string ApiUrl = "https://openapi.envolve.ai/sandbox/transaction/upload";
        /*
        json = @"{""records"":[
                {""orderline_id"":""string"",
                ""order_id"":""string"",
                ""retailer_code"":""string"",
                ""store_code"":""string"",
                ""pos_code"":""string"",
                ""order_created_at"":""2019-12-25T19:20:30+01:00"",
                ""staff_name"":""string"",
                ""payment_method"":""string"",
                ""order_amount"":0,
                ""order_is_void"":true,
                ""order_is_discount"":true,
                ""orderline_weight"":0,
                ""orderline_amount"":0,
                ""orderline_quantity"":0,
                ""orderline_unit_price"":0,
                ""orderline_discount_amount"":0,
                ""product_name"":""string"",
                ""product_code"":""string"",
                ""product_barcode"":""string"",
                ""product_is_gst"":true,
                ""sub_category"":""string"",
                ""category_name"":""string"",
                ""sub_department"":""string"",
                ""department"":""string"",
                ""group"":""string"",
                ""manufacturer_name"":""string"",
                ""supplier_name"":""string"",
                ""product_cost"":0,
                ""order_currency"":""string"",
                ""order_tax"":0,
                ""orderline_tax"":0}
                ]}";
        */
        
        /*
        List<Record> Record = new List<Record>();
        Record records = new Record();
        records.orderline_id = "123456";
        records.order_id = "123";
        records.retailer_code = "ZTPJP";
        records.store_code = "ZTPJP";
        records.pos_code = "370036";
        records.order_created_at = "2019-12-25T19:20:30+01:00";
        records.staff_name = "JASH";
        records.payment_method = "CASH";
        records.order_amount = 6;
        records.order_is_void = false;
        records.order_is_discount = true;
        records.orderline_weight = 5;
        records.orderline_amount = 4;
        records.orderline_quantity = 3;
        records.orderline_unit_price = 2;
        records.orderline_discount_amount = 1;
        records.product_name = "asdzxc zxc ";
        records.product_code = "zcxx";
        records.product_barcode = "asd";
        records.product_is_gst = true;
        records.sub_category = "testing";
        records.category_name = "testing";
        records.sub_department = "test";
        records.department = "test";
        records.group = "test";
        records.manufacturer_name = "tester";
        records.supplier_name = "tester";
        records.product_cost = 0;
        records.order_currency = "SGD";
        records.order_tax = 0;
        records.orderline_tax = 0;

        Record.Add(records);

        EnvolveData ED = new EnvolveData()
        {
            records = Record
        };
        var jSon = JsonConvert.SerializeObject(ED, Formatting.Indented);
        //return JsonConvert.SerializeObject(ED, Formatting.Indented);
        //return json;
        EnvolveData Data = JsonConvert.DeserializeObject<EnvolveData>(json);
        //var jSon = JsonConvert.SerializeObject(Data, Formatting.Indented);
        //return jSon;
        */
        try
        {
            ServicePointManager.MaxServicePointIdleTime = 1000;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            ServicePointManager.Expect100Continue = true;
            //enable this when change to entity framework 4.5
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiUrl);
            //WebHeaderCollection myWebHeaderCollectionSales = request.Headers;
            //myWebHeaderCollectionSales.Add("Authorization", token_type + " " + token);
            //myWebHeaderCollectionSales.Add("x-api-key", ApiKey);
            request.KeepAlive = false;
            request.Accept = "*/*";
            request.ContentType = "application/json";
            request.Method = "POST";//type is POST
            request.Headers.Add("x-api-key", ApiKey);
            request.Headers.Add("authorization", token_type + " " + token);
            request.Referer = ApiUrl;

            request.ProtocolVersion = HttpVersion.Version10; // THIS DOES THE TRICK
            
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            
            var httpResponse = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                responseStr = streamReader.ReadToEnd();
                return responseStr;
            }
        }
        catch (Exception ex)
        {
            return "Error : " + ex.Message.ToString();
        }


    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string updateWarranty(string dbCode, string companyCode, string status)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, dbCode);

        string response = "";

        response = dataAccessor.updateWarrantyStatus(companyCode, status);

        return response;
    }


    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getWarranty(string dbCode, string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, dbCode);

        //string response = "";

        string strSql = " SELECT Company, SetupDate, LicenseType, Key_License, ActivateDate, MaintenanceStatus" +
                        " FROM tbllicense" +
                        " WHERE Company=@Company";

        MySqlParameter[] objparam =
        {
            new MySqlParameter("@Company", companyCode)
        };

        DataSet inventoryDS = dataAccessor.RunSPRetDataset_Vapt(strSql, "Warranty", objparam);

        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getPaymentMethod(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        //string response = "";

        string strSql = " SELECT ID, Nick AS PaymentCode, Full, Display" +
                        " FROM list_paymentmethods" +
                        " WHERE RecordStatus='DELETED'";

        DataSet paymentDS = dataAccessor.RunSPRetDataset(strSql, "list_paymentmethods");

        string json = JsonConvert.SerializeObject(paymentDS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string updatePaymentMethod(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string response = "";

        PaymentMethod paymentMethod = new PaymentMethod();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        /*
        string jSon = @"{""PaymentMethods"":[{
            ""ID"":""1"",
            ""Name"":""Cash"",
            ""Full"":""Cash 100"",
            ""Display"":""Y""
        },{
            ""ID"":""22"",
            ""Name"":""Cheque2"",
            ""Full"":""Cheque 2"",
            ""Display"":""Y""
        }]}";*/

        try
        {
            paymentMethod = serializer.Deserialize<PaymentMethod>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        //json = JsonConvert.SerializeObject(paymentMethod, Formatting.Indented);

        response = dataAccessor.updatePaymentMethod(paymentMethod);

        return response;
    }
        
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string InsertInventoryAging(string companyCode, string json)
    {
        if (json == "") {
            return "No data to be insert";
        }
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        InventoryAgings invaging = new InventoryAgings();        
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        /*    passing value : 
            {"ItemAging":[{"ID":"e5115a91-209c-11ea-9f85-00155d01ca02","SupplierID":"ccb22966-1d51-11ea-b430-00155d01ca02","RetailID":"0","ItemID":"cbbae5c1-1d51-11ea-b430-00155d01ca02",
                "ItemSKU":"","TransID":"604d1554-2094-11ea-9f85-00155d01ca02","TransNo":"","TransDate":"2019-12-17","ItemUOMID":"d09f7be3-1d51-11ea-b430-00155d01ca02","ItemUOM":"Box (75g x 6)","ItemBaseUOMID":"d0998c72-1d51-11ea-b430-00155d01ca02","ItemBaseUOM":"Bot (75g)","Qty":0,"ItemActualQty":"6.000","CurrencyID":"e25458f3-f8f0-11e7-95c5-68f7282584d2",
                "ExcRate":1,"CostUnitPx":"8.23","LocalCostUnitPx":"8.23","CreateTime":"2019-12-17 03:20:02","BatchNo":"","HSCode":"","ExpireDate":"","ExpiryDay":0,"PDQty":0,"SoldQty":0,"TrfInQty":0,"TrfOutQty":0,"AdjQty":6,"RetQty":0,
                "SDQty":0,"KitQty":0,"DekitQty":0,"ReserveQty":0,"InTransitQty":0,"QtyBalance":6,"RFID":""}]}
         */
        try
        {
            invaging = serializer.Deserialize<InventoryAgings>(json);

            //Console.WriteLine(invaging);
            //invaging = JsonConvert.DeserializeObject<InventoryAgings>(json);

        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.SaveInventoryAging(invaging);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getInventoryList(string companyCode, string Query)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSQL = "";
        if (Query == "")
        {
            return "No data to be found.";
        }
        else
        {
            strSQL = FunctionHelper.GetSetting(Query);
        }
        DataSet inventoryDS = dataAccessor.RunSPRetDataset(strSQL, "inventory");
        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);
        //List<string[]> asd = inventoryDS;

        return json;
    }
    
	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getSalesPerson(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT  u.ID, u.user AS LoginID, u.password AS LoginPswd, u.UsersFirstName AS UserName, " +
            " u.email AS Email, u.hph AS HandphoneNo, u.UsersDOB AS DOB, u.accesslevel AS UserLevel, " +
            " IF(ISNULL(list_secgroup.Nick),'',list_secgroup.Nick) AS UserGroup, " +
            " IF(ISNULL(list_salutation.Nick),'',list_salutation.Nick) AS Salutation, u.UsersCommision" +
            " FROM users u " +
            " LEFT JOIN list_secgroup ON list_secgroup.ID = u.usergroup " +
            " LEFT JOIN list_salutation ON list_salutation.ID = u.UsersSalutation " +
            " WHERE u.RecordStatus <> 'DELETED' AND u.Display = 'Y' " +
            " ORDER BY LoginID";

        DataSet SalesPersonDS = dataAccessor.RunSPRetDataset(strSql, "SalesPersons");

        string json = JsonConvert.SerializeObject(SalesPersonDS, Formatting.Indented);

        return json;
    }
		
	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getCustomers(string companyCode, string FromRow, string ToRow)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string fieldCriteria1 = "";

        try
        {
            decimal range = decimal.Parse(ToRow) - (decimal.Parse(FromRow) - 1);

            if (range > 5000)
            {
                return "Range of FrowRow & ToRow don't more than 5000.";
            }
            fieldCriteria1 = "LIMIT " + (Convert.ToDecimal(FromRow) - 1) + ", " + range;
        }
        catch (Exception ex)
        {
            return "There are more than 10000 records. Please key in Numbers for FrowRow & ToRow field.";
        }

        string strSql = "SELECT c.ID, c.custcode, c.customerFirstName AS FirstName, c.customerLastName AS LastName, " +
                        " c.CustICNO, c.Email, c.hph AS MobileNo, c.cardnumber, c.CustomerDOB AS DOB, " +
                        " c.CustomerAddress1 AS Address1, c.CustomerAddress2 AS Address2, c.CustomerAddress3 AS Address3, " +
                        " c.CustomerPostcode AS PostalCode, customer_type.CustTypeName AS CustomerType, " +
                        " list_sexes.Nick AS Gender, c.OpeningLP AS LoyaltyPoint " +
                        " FROM customer c " +
                        " LEFT JOIN customer_type ON customer_type.CustTypeID = c.customertype " +
                        " LEFT JOIN list_sexes ON list_sexes.ID = c.CustomerSexID " +
                        " WHERE c.RecordStatus <> 'DELETED' " +
                        " Order By custcode,CustICNO,Email " + fieldCriteria1;

        DataSet CustomersDS = dataAccessor.RunSPRetDataset(strSql, "Customers");

        string json = JsonConvert.SerializeObject(CustomersDS, Formatting.Indented);

        return json;
    }
	
	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getCustomersTotalCount(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);


        string sql = "SELECT COUNT(*) as TotalCount " +
            " FROM customer" +
            " WHERE RecordStatus <> 'DELETED' AND Display = 'Y'";
        DataSet CustomerDS = dataAccessor.RunSPRetDataset(sql, "Customers");

        string json = JsonConvert.SerializeObject(CustomerDS, Formatting.Indented);

        return json;
    }
	
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetAddressforMember(string companyCode, string memberhp)
    {
        string newCustID = "";
        string s = "Fail";
        string msql = "";
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        try
        {
            string sql = "select customerAddress1,customerAddress2,customerAddress3  from customer where hph=@Hph";

            MySqlParameter[] objParam =
                {
                    new MySqlParameter("@Hph",  memberhp )
                 };

            DataSet ds = dataAccessor.RunSPRetDataset_Vapt(sql, "customer", objParam);

            return JsonConvert.SerializeObject(ds, Formatting.Indented);
        }
        catch (Exception e)
        {
            return e.Message.ToString();
        }
    }

    /****** below is to pull member point information ***********/
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getMemberPoint(string companyCode, string MemberID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string fieldCriteria = "";
        if (MemberID != "" || MemberID != "0")
        {
            fieldCriteria = " WHERE ID=@MemberID";
        }
        else {
            return "Invalid MemberID";
        }

        string strSql = "SELECT custcode, CustICNO, Email, hph,cardnumber, CustomerDOB AS DOB, customerFirstName as FirstName, customerLastName AS LastName," +
                        " TotalLP AS BalancePoint,TotalEP AS EarnPoint,TotalRP AS RedeemPoint,TotalAP AS AdjustPoint,(SELECT getExpiringPoint(customer.ID)) AS ExpiryPoint," +
                        "IF(ExpiryPointDate = 0000-00-00," +
                        "(SELECT DATE_FORMAT(LAST_DAY(CONCAT(SUBSTR(CURDATE(),1,4)+1,LPAD(PointCutOffMonth,2,'0'),'01')),'%d-%m') AS expiryDate FROM customer_definitions),ExpiryPointDate)  AS ExpiringDate, " +
                        " ID AS MemberID FROM customer " + fieldCriteria;
        MySqlParameter[] objParam =
                {
                    new MySqlParameter("@MemberID",  MemberID )
                 };

        DataSet memLPDS = dataAccessor.RunSPRetDataset_Vapt(strSql, "Member", objParam);

        string json = JsonConvert.SerializeObject(memLPDS, Formatting.Indented);
        //List<string[]> asd = inventoryDS;

        //return asd;
        return json;
    }
    

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string PostSales(string companyCode, string json, string salestype)
    {
        /* passing value : 
           {"CompanyID":"5","RetailerID":"4","TransNo":"ON000001","TotalDue":"20","TotalGST":"0.00","TotalDisc":"0.00","TransDate":"2020-06-17","CreateTime":"2020-06-17 16:28:22", "TotalQty":"2","CashierID":"","MemberID":"0bb22288-b073-11ea-84cf-00155d01ca02","isNewCust":"Y","SalesPersonID":"","CommID":"","CommPerc":"0","ReceiptOrderStatus":"","vchQueueNo":"0001","MacAddress":"","TerminalID":"1","PendingSync":"N","LastUser":"","LastUpdate":"2020-06-17 16:28:22","LockUser":"N","LockUpdate":"2020-06-17 16:28:22", "LockStatus":"0","RecordStatus":"READY","RecordUpdate":"2020-06-17 16:28:22","QueueStatus":"READY","salesorder_item":[{"RecordNo":"1","LineNo":"1","ItemQty":"2","ItemPrice":"10", "ItemTotal":"20.00","ItemGST":"0.00","ItemDiscType":"","ItemDisc1":"0.00","ItemDisc2":"0.00","ItemDisc3":"0.00","ItemID":"4ffb9a83-4efe-11ea-84ac-0894ef44a723","ItemBarcode":"4710227231304", "ItemUOM":"", "ItemGSTInEx":"N","ItemCost":"6.00","ItemActQty":"1.000","ItemUOMID":"", "ItemGroupDisc":"0.00","ItemSKU":"4710227231304","SupplierID":"","SalesPersonID":"","SalesCommTypeID":"","SalesCommPerc":"0.00","ItemCommPerc":"0.00","ItemCommAmt":"0.00","ItemSerialNo":"","DISCID":"","ItemIMEINo":"","ItemBatchNo":"","ItemStatus":"","OpenPriceRemark":"","ItemRemark":"", "ExpireDate":"0000-00-00", "ExpiryDay":"0","RedeemPoint":"0","ParentItemID_ADDON":"","bitAddOnItem":"N", "ParentDetailID_ADDON":"","MemDOBDiscPerc":"0.00", "MemDOBDiscAmount":"0.00","ReceiptOrderStatus":"","TerminalID":"1","RFID":"","PendingSync":"N","LastUser":"","LastUpdate":"2020-06-17 16:28:22","LockUser":"N","LockUpdate":"2020-06-17 16:28:22","LockStatus":"0","RecordStatus":"READY","RecordUpdate":"2020-06-17 16:28:22","QueueStatus":"READY"}]}
         */
        string user = "online", status = "READY";
        string todaystr = DateTime.Now.ToString("yyyy-MM-dd");
        string todaytimestamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

        clsDataAccessor dataAccessor1 = new clsDataAccessor();
        string connstr = dataAccessor1.getConnectionString_Vapt(dataAccessor1, companyCode);

        MySqlTransaction objTran = null;
        MySqlConnection objcon = new MySqlConnection(connstr);
        objcon.Open();
        objTran = (MySqlTransaction)objcon.BeginTransaction();

        FunctionHelper f = new FunctionHelper();

        string queuenostr = "0000";
        int queueno = 0;

        if (salestype == "HOLD")
        {
            SalesHoldOrder order = new SalesHoldOrder();
            try
            {
                order = JsonConvert.DeserializeObject<SalesHoldOrder>(json);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            clsDataAccessor dataAccessor = new clsDataAccessor();
            dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
            string sqlc = "select TransID from retail_sales_holdhdr where retailerid=@RetailerID and intTableNo=@TableNo";
            string ctransid = "";

            if (order.intTableNo != 0)
            {
                try
                {
                    MySqlParameter[] objparam =
                    {
                        new MySqlParameter("@RetailerID", order.RetailerID),
                        new MySqlParameter("@TableNo", order.intTableNo)
                    };

                    ctransid = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sqlc, objparam).ToString();
                }
                catch (Exception ex)
                {
                    ctransid = "";
                }
            }

            string sqluuid = "select uuid()";
            string ID = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sqluuid, null).ToString();
            if (ctransid.Length > 0)
            {
                ID = ctransid;
            }
            string s = "Fail";
            string Insert_main = "";
            bool insertFlag = false;

            MySqlParameter[] objparam_InsertMain =
                    {
                        new MySqlParameter("@TotalDue", order.TotalDue ),
                        new MySqlParameter("@TotalGST", order.TotalGST ),
                        new MySqlParameter("@TotalDisc", order.TotalDisc ),
                        new MySqlParameter("@CreateTime", todaytimestamp ),
                        new MySqlParameter("@TotalQty", order.TotalQty ),
                        new MySqlParameter("@TransID", ID )
                    };

            int xmain = 0;
            if (ctransid.Length > 0)
            {
                Insert_main = "update retail_sales_holdhdr set TotalDue=TotalDue+@TotalDue,";
                Insert_main += "TotalGST=TotalGST+@TotalGST,";
                Insert_main += "TotalDisc=TotalDisc+@TotalDisc,";
                Insert_main += "CreateTime=@CreateTime,";
                Insert_main += "TotalQty=TotalQty+@TotalQty,";
                Insert_main += "LastUpdate=@CreateTime,";
                Insert_main += "RecordUpdate=@CreateTime where TransID=@TransID";

                MySqlParameter[] objparam_main =
                 {
                    new MySqlParameter("@TotalDue", order.TotalDue),
                    new MySqlParameter("@TotalGST", order.TotalGST),
                    new MySqlParameter("@TotalDisc", order.TotalDisc),
                    new MySqlParameter("@CreateTime", todaytimestamp),
                    new MySqlParameter("@TotalQty", order.TotalQty),
                    new MySqlParameter("@TransID", ID)
                };

                string sql_queueno = "SELECT vchQueueNo from retail_sales_holdhdr where TransID=@TransID";
                try
                {
                    MySqlParameter[] objparam =
                    {
                        new MySqlParameter("@TransID", ID)
                    };
                    queuenostr = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sql_queueno, objparam).ToString();
                }
                catch (Exception eq)
                {
                    queuenostr = f.getTransCount(dataAccessor, objTran, order.RetailerID, order.TerminalID);
                }
				xmain = dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, Insert_main, objparam_main);
            }
            else
            {
                queuenostr = f.getTransCount(dataAccessor, objTran, order.RetailerID, order.TerminalID);

                Insert_main = "insert into retail_sales_holdhdr(TransID, CompanyID, RetailerID, TransNo, TotalDue, TotalGST, TotalDisc, TransDate, CreateTime, TotalQty, CashierID, MemberID, isNewCust,";
                Insert_main += "SalesPersonID, CommID, CommPerc, ReceiptOrderStatus, vchQueueNo, intTableNo, MacAddress, TerminalID, PendingSync, LastUser, LastUpdate, LockUser, LockUpdate, LockStatus, RecordStatus,";
                Insert_main += "RecordUpdate, QueueStatus) values(@ID, @CompanyID, @RetailerID, @TransNo, @TotalDue, @TotalGST, @TotalDisc, @TransDate, @CreateTime, @TotalQty, @CashierID, ";
                Insert_main += "@MemberID, @isNewCust, @SalesPersonID, @CommID, @CommPerc, @ReceiptOrderStatus, @QueueNo, @IntTableNo, @MacAddress, @TerminalID, @PendingSync,";
                Insert_main += "@LastUser, @CreateTime, @LockUser, @LockUpdate, @LockStatus, @Status, @CreateTime, @Status);";

                MySqlParameter[] objparam_main =
                {
                    new MySqlParameter("@ID", ID),
                    new MySqlParameter("@CompanyID", order.CompanyID),
                    new MySqlParameter("@RetailerID", order.RetailerID),
                    new MySqlParameter("@TransNo", order.TransNo),
                    new MySqlParameter("@TotalDue", order.TotalDue),
                    new MySqlParameter("@TotalGST", order.TotalGST),
                    new MySqlParameter("@TotalDisc", order.TotalDisc),
                    new MySqlParameter("@TransDate", order.TransDate),
                    new MySqlParameter("@CreateTime", todaytimestamp),
                    new MySqlParameter("@TotalQty", order.TotalQty),
                    new MySqlParameter("@CashierID", order.CashierID),
                    new MySqlParameter("@MemberID", order.MemberID),
                    new MySqlParameter("@isNewCust", order.isNewCust),
                    new MySqlParameter("@SalesPersonID", order.SalesPersonID),
                    new MySqlParameter("@CommID", order.CommID),
                    new MySqlParameter("@CommPerc", order.CommPerc),
                    new MySqlParameter("@ReceiptOrderStatus", order.ReceiptOrderStatus),
                    new MySqlParameter("@QueueNo", queuenostr),
                    new MySqlParameter("@intTableNo", order.intTableNo.ToString()),
                    new MySqlParameter("@MacAddress", order.MacAddress),
                    new MySqlParameter("@TerminalID", order.TerminalID),
                    new MySqlParameter("@PendingSync", order.PendingSync),
                    new MySqlParameter("@LastUser", user),
                    new MySqlParameter("@LockUser", order.LockUser),
                    new MySqlParameter("@LockUpdate", order.LockUpdate),
                    new MySqlParameter("@LockStatus", order.LockStatus),
                    new MySqlParameter("@Status",status)
                };
				xmain = dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, Insert_main, objparam_main);
            }

            string idstr = ID.ToString();
            string Insert_item = "";

            List<SalesHoldOrder_Item> items = new List<SalesHoldOrder_Item>();

            items = order.saleshold_item;

            if (items.Count > 0)
            {
                int maxlineno = 0;
                if (ctransid.Length > 0)
                {
                    string countMaxLineno = "SELECT MAX(LINENO) FROM RETAIL_SALES_HOLDDTL WHERE TRANSID=@TransID";
                    MySqlParameter[] objprarm1 =
                    {
                        new MySqlParameter("@TransID", ctransid)
                    };
                    
                    try
                    {
                        maxlineno = int.Parse(dataAccessor.ExecuteScalar(objTran, CommandType.Text, countMaxLineno, objprarm1).ToString());
                    }
                    catch (Exception exmaxlineno)
                    {
                        maxlineno = 0;
                    }
                }

				int xitem = 0;
                for (int j = 0; j < items.Count; j++)
                {
                    SalesHoldOrder_Item i = items[j];

                    string itemsupbarcode = "", itemGSTIncExc = "", ItemSKU = "", ItemSupplierID = "", ItemUOM = "";
                    double itemactqty = 0.000, itemcost = 0.00, itemcommperc = 0.00, itemcommamt = 0.00, itemretailcost = 0.00, itemactcost = 0.00;
                    string sql_filteriteminfor = "select inventory_supbar.SupBarCode,inventory_retail.GSTIncExc, inventory.ItemSKU, inventory_supbar.SupplierID from inventory left join inventory_supbar on inventory.itemid = inventory_supbar.itemid "
                    + "INNER JOIN inventory_retail ON (inventory.ItemID=inventory_retail.ItemID) where inventory.ItemID=@ItemID";
                    MySqlParameter[] objparam2 =
                    {
                        new MySqlParameter("@ItemID", i.ItemID)
                    };
                    DataSet ds_iteminfor = new DataSet();
                    ds_iteminfor = dataAccessor.ExecuteDataSet(objTran, CommandType.Text, sql_filteriteminfor, objparam2);
                    if (ds_iteminfor.Tables.Count > 0 && ds_iteminfor.Tables[0].Rows.Count > 0)
                    {
                        itemsupbarcode = ds_iteminfor.Tables[0].Rows[0]["SupBarCode"].ToString();
                        if (ds_iteminfor.Tables[0].Rows[0]["GSTIncExc"].ToString() == "2")
                        {
                            itemGSTIncExc = "E";
                        }
                        else
                        {
                            itemGSTIncExc = "I";
                        }
                        ItemSKU = ds_iteminfor.Tables[0].Rows[0]["ItemSKU"].ToString();
                        ItemSupplierID = ds_iteminfor.Tables[0].Rows[0]["SupplierID"].ToString();
                    }

                    string itemunitid = "";

                    sql_filteriteminfor = "SELECT ID FROM LIST_UNITS WHERE VALUE=@ItemUOM";
                    MySqlParameter[] objparam3 =
                    {
                        new MySqlParameter("@ItemUOM", i.ItemUOM)
                    };
                    try
                    {
                        itemunitid = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sql_filteriteminfor, objparam3).ToString();
                    }
                    catch (Exception exunit)
                    {
                        itemunitid = "";
                    }

                    if (itemunitid.Length == 0)
                    {
                        sql_filteriteminfor = "SELECT ID FROM LIST_UNITS WHERE ID = (SELECT ITEMUNIT FROM INVENTORY_UNIT WHERE ITEMID=@ItemID AND ITEMUNITDEF = 'Y' AND RECORDSTATUS <> 'DELETED' LIMIT 1)";
                        try
                        {
                            itemunitid = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sql_filteriteminfor, objparam2).ToString();
                        }
                        catch (Exception exunit)
                        {
                            itemunitid = "";
                        }
                    }

                    sql_filteriteminfor = "SELECT ITEMACTQTY, PURCHASECOST,COMMPERC,COMMAMT, LIST_UNITS.VALUE AS ITEMUOM FROM INVENTORY_UNIT LEFT JOIN LIST_UNITS ON INVENTORY_UNIT.ITEMUNIT = LIST_UNITS.ID WHERE ITEMID = '" + i.ItemID + "'";
                    sql_filteriteminfor += " AND INVENTORY_UNIT.RECORDSTATUS <> 'DELETED' AND INVENTORY_UNIT.ITEMUNIT=@ItemUnit";
                    MySqlParameter[] objparam4 =
                    {
                        new MySqlParameter("@ItemUnit", itemunitid)
                    };
                    ds_iteminfor = dataAccessor.ExecuteDataSet(objTran, CommandType.Text, sql_filteriteminfor, objparam4);

                    if (ds_iteminfor.Tables.Count > 0 && ds_iteminfor.Tables[0].Rows.Count > 0)
                    {
                        itemactqty = double.Parse(ds_iteminfor.Tables[0].Rows[0]["ITEMACTQTY"].ToString());
                        ItemUOM = ds_iteminfor.Tables[0].Rows[0]["ITEMUOM"].ToString();
                        itemcost = double.Parse(ds_iteminfor.Tables[0].Rows[0]["PURCHASECOST"].ToString());
                        itemcommamt = double.Parse(ds_iteminfor.Tables[0].Rows[0]["COMMAMT"].ToString());
                        itemcommperc = double.Parse(ds_iteminfor.Tables[0].Rows[0]["COMMPERC"].ToString());
                    }

                    string sql_retailcost = "SELECT ItemCost FROM inventory_retail WHERE ItemID=@ItemID AND RetailID=@RetailID AND RecordStatus<>'DELETED'";
                    MySqlParameter[] objparam5 =
                    {
                        new MySqlParameter("@ItemID", i.ItemID),
                        new MySqlParameter("@RetailID", order.RetailerID)
                    };
                    ds_iteminfor = dataAccessor.ExecuteDataSet(objTran, CommandType.Text, sql_retailcost, objparam5);
                    if (ds_iteminfor.Tables.Count > 0 && ds_iteminfor.Tables[0].Rows.Count > 0)
                    {
                        itemretailcost = double.Parse(ds_iteminfor.Tables[0].Rows[0]["ItemCost"].ToString());
                        itemactcost = itemretailcost * itemactqty;
                    }
                    else
                    {
                        itemactcost = itemcost;
                    }
                    ds_iteminfor.Dispose();
                    ds_iteminfor.Clear();


                    Insert_item = "Insert into retail_sales_holddtl(TransID, RecordNo, LineNo, ItemQty, ItemPrice, ItemTotal, ItemGST, ItemDiscType, ItemDisc1, ItemDisc2, ItemDisc3, ItemID, ItemBarcode,";
                    Insert_item += "ItemUOM, ItemGSTInEx, ItemCost, ItemActQty, ItemUOMID, ItemGroupDisc, ItemSKU, SupplierID, SalesPersonID, SalesCommTypeID, SalesCommPerc, ItemCommPerc, ItemCommAmt,";
                    Insert_item += "ItemSerialNo, DISCID, ItemIMEINo, ItemBatchNo, ItemStatus, OpenPriceRemark, ItemRemark, ExpireDate, ExpiryDay, RedeemPoint, ParentItemID_ADDON, bitAddOnItem,";
                    Insert_item += "ParentDetailID_ADDON, MemDOBDiscPerc, MemDOBDiscAmount, ReceiptOrderStatus, TerminalID, RFID, PendingSync,  LastUser, LastUpdate, LockUser, LockUpdate, LockStatus, ";
                    Insert_item += "RecordStatus, RecordUpdate, QueueStatus) values (@TransID, @RecordNo, @LineNo, @ItemQty, @ItemPrice,@ItemTotal,@ItemGST, @ItemDiscType, @ItemDisc1, @ItemDisc2, @ItemDisc3,";
                    Insert_item += "@ItemID, @ItemBarcode,@ItemUOM, @ItemGSTInEx, @ItemCost, @ItemActQty, @ItemUOMID, @ItemGroupDisc, @ItemSKU, @SupplierID, @SalesPersonID, @SalesCommTypeID, @SalesCommPerc,";
                    Insert_item += "@ItemCommPerc, @ItemCommAmt, @ItemSerialNo, @DISCID, @ItemIMEINo, @ItemBatchNo, @ItemStatus, @OpenPriceRemark, @ItemRemark, @ExpireDate, @ExpiryDay, @RedeemPoint, @ParentItemID_ADDON, @bitAddOnItem, ";
                    Insert_item += "@ParentDetailID_ADDON, @MemDOBDiscPerc, @MemDOBDiscAmount, @ReceiptOrderStatus, @TerminalID, @RFID, @PendingSync,  @LastUser, @LastUpdate, @LockUser, @LockUpdate, @LockStatus,";
                    Insert_item += "@RecordStatus, @RecordUpdate, @QueueStatus);";

                    MySqlParameter[] objparam_item =
                    {
                        new MySqlParameter("@TransID", ID),
                        new MySqlParameter("@RecordNo", (int.Parse(i.RecordNo) + maxlineno).ToString()),
                        new MySqlParameter("@LineNo", (int.Parse(i.LineNo) + maxlineno).ToString() ),
                        new MySqlParameter("@ItemQty",  i.ItemQty),
                        new MySqlParameter("@ItemPrice",  i.ItemPrice),
                        new MySqlParameter("@ItemTotal", i.ItemTotal),
                        new MySqlParameter("@ItemGST", i.ItemGST),
                        new MySqlParameter("@ItemDiscType", i.ItemDiscType),
                        new MySqlParameter("@ItemDisc1", i.ItemDisc1),
                        new MySqlParameter("@ItemDisc2", i.ItemDisc2),
                        new MySqlParameter("@ItemDisc3", i.ItemDisc3),
                        new MySqlParameter("@ItemID", i.ItemID),
                        new MySqlParameter("@ItemBarcode", itemsupbarcode),
                        new MySqlParameter("@ItemUOM", ItemUOM),
                        new MySqlParameter("@ItemGSTInEx", itemGSTIncExc),
                        new MySqlParameter("@ItemCost", itemactcost),
                        new MySqlParameter("@ItemActQty", itemactqty),
                        new MySqlParameter("@ItemUOMID", itemunitid),
                        new MySqlParameter("@ItemGroupDisc", i.ItemGroupDisc),
                        new MySqlParameter("@ItemSKU", ItemSKU),
                        new MySqlParameter("@SupplierID", ItemSupplierID),
                        new MySqlParameter("@SalesPersonID", i.SalesPersonID),
                        new MySqlParameter("@SalesCommTypeID", i.SalesCommTypeID),
                        new MySqlParameter("@SalesCommPerc", i.SalesCommPerc),
                        new MySqlParameter("@ItemCommPerc", itemcommperc),
                        new MySqlParameter("@ItemCommAmt", itemcommamt),
                        new MySqlParameter("@ItemSerialNo", i.ItemSerialNo),
                        new MySqlParameter("@DISCID", i.DISCID),
                        new MySqlParameter("@ItemIMEINo", i.ItemIMEINo),
                        new MySqlParameter("@ItemBatchNo", i.ItemBatchNo),
                        new MySqlParameter("@ItemStatus", i.ItemStatus),
                        new MySqlParameter("@OpenPriceRemark", i.OpenPriceRemark),
                        new MySqlParameter("@ItemRemark", i.ItemRemark),
                        new MySqlParameter("@ExpireDate", i.ExpireDate),
                        new MySqlParameter("@ExpiryDay", i.ExpiryDay),
                        new MySqlParameter("@RedeemPoint", i.RedeemPoint),
                        new MySqlParameter("@ParentItemID_ADDON", i.ParentItemID_ADDON),
                        new MySqlParameter("@bitAddOnItem",  i.bitAddOnItem),
                        new MySqlParameter("@ParentDetailID_ADDON", i.ParentDetailID_ADDON),
                        new MySqlParameter("@MemDOBDiscPerc", i.MemDOBDiscPerc),
                        new MySqlParameter("@MemDOBDiscAmount", i.MemDOBDiscAmount),
                        new MySqlParameter("@ReceiptOrderStatus", i.ReceiptOrderStatus),
                        new MySqlParameter("@TerminalID", i.TerminalID),
                        new MySqlParameter("@RFID",  i.RFID),
                        new MySqlParameter("@PendingSync",  i.PendingSync),
                        new MySqlParameter("@LastUser", user),
                        new MySqlParameter("@LastUpdate", todaytimestamp),
                        new MySqlParameter("@LockUser", order.LockUser),
                        new MySqlParameter("@LockUpdate", order.LockUpdate),
                        new MySqlParameter("@LockStatus", order.LockStatus),
                        new MySqlParameter("@RecordStatus", status),
                        new MySqlParameter("@RecordUpdate", todaytimestamp),
                        new MySqlParameter("@QueueStatus", status)
                    };

                    xitem = dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, Insert_item, objparam_item);
                }

                string sql_forqueueno = "UPDATE DEFINITIONS_TERMINAL SET LastQueueNo = LastQueueNo + 1 WHERE DISPLAY = 'Y' AND RETAILERID=@RetailerID AND TERMINALID=@TerminalID";
                MySqlParameter[] objparam_queueno =
                {
                    new MySqlParameter("@RetailerID", order.RetailerID),
                    new MySqlParameter("@TerminalID", (order.TerminalID == "0" ? "1" : order.TerminalID))
                };
                int xqueueno = dataAccessor.ExecuteNonQuery(objTran, CommandType.Text, sql_forqueueno, objparam_queueno);

                try
                {
                    objTran.Commit();
                    if (xmain > 0 && xitem > 0 && xqueueno > 0)
                    {
                        s = "Success";
                    }
                    else
                    {
                        objTran.Rollback();
                        s = "Fail";
                    }
                }
                catch (Exception emain)
                {
                    objTran.Rollback();
                    s = "Fail";
                }
            }

            objcon.Close();
            objcon.Dispose();
            objTran.Dispose();

            return new JavaScriptSerializer().Serialize(new { Status = s, OrderID = idstr, QueueNo = queuenostr });
        }
        else
        {
            SalesOnlineOrder order = new SalesOnlineOrder();
            try
            {
                order = JsonConvert.DeserializeObject<SalesOnlineOrder>(json);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            clsDataAccessor dataAccessor = new clsDataAccessor();
            dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
            string sqluuid = "select uuid()";
            string ID = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sqluuid, null).ToString();
            string idstr = ID.ToString();
            string s = "Fail";

            string CustID = "";

            MemberInfor_Order m = new MemberInfor_Order();

            if (order.memberinfor != null)
            {
                m = order.memberinfor;
                string msql = "";
                if (m.MemberHp.Length > 0)
                {
                    msql = "select ID,customerAddress1,customerAddress2,customerAddress3  from customer where hph=@Hph";
                    MySqlParameter[] objparam_member =
                    {
                        new MySqlParameter("@Hph", m.MemberHp)
                    };
                    DataSet ds_memberinfor = dataAccessor.ExecuteDataSet(objTran, CommandType.Text, msql, objparam_member);

                    if (ds_memberinfor.Tables.Count > 0 && ds_memberinfor.Tables[0].Rows.Count > 0)
                    {
                        CustID = ds_memberinfor.Tables[0].Rows[0]["ID"].ToString();
                        string ad1 = ds_memberinfor.Tables[0].Rows[0]["customerAddress1"].ToString();
                        string ad2 = ds_memberinfor.Tables[0].Rows[0]["customerAddress2"].ToString();
                        string ad3 = ds_memberinfor.Tables[0].Rows[0]["customerAddress3"].ToString();

                        if (!m.MemberAddress.Trim().Equals(ad1) && !m.MemberAddress.Trim().Equals(ad2) && !m.MemberAddress.Trim().Equals(ad3))
                        {
                            if ((ad1.Length > 0 && ad2.Length > 0 && ad3.Length > 0) || ad1.Length == 0)
                            {
                                msql = "update customer set customerAddress1=@Address where id=@MemberID";
                            }

                            if (ad1.Length > 0 && ad2.Length > 0 && ad3.Length == 0)
                            {
                                msql = "update customer set customerAddress3=@Address where id=@MemberID";
                            }

                            if (ad1.Length > 0 && ad2.Length == 0 && ad3.Length == 0)
                            {
                                msql = "update customer set customerAddress2=@Address where id=@MemberID";
                            }

                            MySqlParameter[] objparam_msql =
                            {
                                new MySqlParameter("@Address", m.MemberAddress),
                                new MySqlParameter("@MemberID", CustID)
                            };

                            dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, msql, objparam_msql);
                        }
                    }
                    else
                    {
                        CustID = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sqluuid, null).ToString();
                        string custcode = f.Get_Auto_Custcode(dataAccessor, objTran);
                        string custtypeid = f.Get_CustType(dataAccessor, objTran);

                        msql = "Insert into customer(id, custcode, uniqueno, hph, customertype, customerfirstname, retailid, customerstartdate, customerenddate, customeraddress1, customernotes, lastuser, lastupdate, recordstatus, recordupdate )";
                        msql += " values(@MemberID, @CustCode, @UniqueNo, @Hph, @CustType, @CustName, @RetailID, @StartDate, @EndDate, @Address1, @CustNotes, @Lastuser, @Lastupdate, @RecordStatus, @RecordUpdate)";

                        MySqlParameter[] objparam_msql =
                        {
                            new MySqlParameter("@MemberID", CustID),
                            new MySqlParameter("@CustCode", custcode),
                            new MySqlParameter("@UniqueNo", m.MemberHp),
                            new MySqlParameter("@Hph", m.MemberHp),
                            new MySqlParameter("@CustType", custtypeid),
                            new MySqlParameter("@CustName", m.MemberName ),
                            new MySqlParameter("@RetailID", "0"),
                            new MySqlParameter("@StartDate", todaystr),
                            new MySqlParameter("@EndDate", "2035-01-01"),
                            new MySqlParameter("@Address1", m.MemberAddress),
                            new MySqlParameter("@CustNotes", ""),
                            new MySqlParameter("@Lastuser", "online"),
                            new MySqlParameter("@Lastupdate", todaytimestamp),
                            new MySqlParameter("@RecordStatus", "READY"),
                            new MySqlParameter("@RecordUpdate", todaytimestamp)
                        };

                    	dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, msql, null);
                	}
                }
            }

            queuenostr = f.getTransCount(dataAccessor, objTran, order.RetailID, order.TerminalID);

            string Insert_main = "INSERT INTO retail_sales (SalesID,RetailID,SalesNo,SalesTax,SalesTaxVal,SalesDate,SalesPersonID,SalesPerson,SalesRemark,SalesDisc,SalesDisc2,SalesDisc3,";
            Insert_main += "SalesDiscAmt,SalesDiscAmt2,SalesDiscAmt3,SalesDiscGroupPct,SalesDiscGroupAmt,SalesTotalGroupDisc,SalesDiscGroupType,SalesSubTtl,SalesTaxTtl,SalesBalTtl,SalesPayTtl,SalesChangeAmt,SalesDeposit,";
            Insert_main += "SalesStatus,CreateBy,CreateTime,SalesRounding,MemberID,MemberDisc,MemberAmt,CardAmt,CommID,CommPerc,DepositStatus,CollectionDate,dteDeliveryDate,dteDeliveryTimeFr,dteDeliveryTimeTo,";
            Insert_main += "CollectionRetailID,CloseTerminalID,EmployeeID,EmployeeName,MacAddress,MemDOBDiscPerc,MemDOBDiscAmount,ShippingFee,vchQueueNo,ReceiptOrderStatus,TableNumber,PendingSync,SyncAPI,LastUser,LastUpdate,";
            Insert_main += "LockUser,LockUpdate,LockStatus,RecordStatus,RecordUpdate, QueueStatus,TerminalID) values (@SalesID,@RetailID,@SalesNo,@SalesTax,@SalesTaxVal,@SalesDate,@SalesPersonID,@SalesPerson,@SalesRemark,";
            Insert_main += "@SalesDisc,@SalesDisc2,@SalesDisc3,@SalesDiscAmt,@SalesDiscAmt2,@SalesDiscAmt3,@SalesDiscGroupPct,@SalesDiscGroupAmt,@SalesTotalGroupDisc,@SalesDiscGroupType,@SalesSubTtl,@SalesTaxTtl,@SalesBalTtl,@SalesPayTtl,@SalesChangeAmt,@SalesDeposit,";
            Insert_main += "@SalesStatus,@CreateBy,@CreateTime,@SalesRounding,@MemberID,@MemberDisc,@MemberAmt,@CardAmt,@CommID,@CommPerc,@DepositStatus,@CollectionDate,@dteDeliveryDate,@dteDeliveryTimeFr,@dteDeliveryTimeTo,";
            Insert_main += "@CollectionRetailID,@CloseTerminalID,@EmployeeID,@EmployeeName,@MacAddress,@MemDOBDiscPerc,@MemDOBDiscAmount,@ShippingFee,@vchQueueNo,@ReceiptOrderStatus,@TableNumber,@PendingSync,@SyncAPI,@LastUser,@LastUpdate,";
            Insert_main += "@LockUser,@LockUpdate,@LockStatus,@RecordStatus,@RecordUpdate,@QueueStatus,@TerminalID);";

            MySqlParameter[] objparam_main =
            {
                new MySqlParameter("@SalesID", ID),
                new MySqlParameter("@RetailID", order.RetailID),
                new MySqlParameter("@SalesNo", order.SalesNo),
                new MySqlParameter("@SalesTax", order.SalesTax),
                new MySqlParameter("@SalesTaxVal", order.SalesTaxVal),
                new MySqlParameter("@SalesDate", order.SalesDate),
                new MySqlParameter("@SalesPersonID", order.SalesPersonID),
                new MySqlParameter("@SalesPerson", order.SalesPerson),
                new MySqlParameter("@SalesRemark", order.SalesRemark),
                new MySqlParameter("@SalesDisc", order.SalesDisc),
                new MySqlParameter("@SalesDisc2", order.SalesDisc2),
                new MySqlParameter("@SalesDisc3", order.SalesDisc3),
                new MySqlParameter("@SalesDiscAmt", order.SalesDisc),
                new MySqlParameter("@SalesDiscAmt2", order.SalesDisc2),
                new MySqlParameter("@SalesDiscAmt3", order.SalesDisc),
                new MySqlParameter("@SalesDiscGroupPct", order.SalesDiscGroupPct),
                new MySqlParameter("@SalesDiscGroupAmt", order.SalesDiscGroupAmt),
                new MySqlParameter("@SalesTotalGroupDisc", order.SalesTotalGroupDisc),
                new MySqlParameter("@SalesDiscGroupType", order.SalesDiscGroupType),
                new MySqlParameter("@SalesSubTtl", order.SalesSubTtl),
                new MySqlParameter("@SalesTaxTtl", order.SalesTaxTtl),
                new MySqlParameter("@SalesBalTtl", order.SalesBalTtl),
                new MySqlParameter("@SalesPayTtl", order.SalesPayTtl),
                new MySqlParameter("@SalesChangeAmt", order.SalesChangeAmt),
                new MySqlParameter("@SalesDeposit", order.SalesDeposit),
                new MySqlParameter("@SalesStatus", order.SalesStatus),
                new MySqlParameter("@CreateBy", order.CreateBy),
                new MySqlParameter("@CreateTime", order.CreateTime),
                new MySqlParameter("@SalesRounding", order.SalesRounding),
                new MySqlParameter("@MemberID", CustID),
                new MySqlParameter("@MemberDisc", order.MemberDisc),
                new MySqlParameter("@MemberAmt", order.MemberAmt),
                new MySqlParameter("@CardAmt", order.CardAmt),
                new MySqlParameter("@CommID", order.CommID),
                new MySqlParameter("@CommPerc", order.CommPerc),
                new MySqlParameter("@DepositStatus", order.DepositStatus),
                new MySqlParameter("@CollectionDate", order.CollectionDate),
                new MySqlParameter("@dteDeliveryDate", order.dteDeliveryDate),
                new MySqlParameter("@dteDeliveryTimeFr", order.dteDeliveryTimeFr),
                new MySqlParameter("@dteDeliveryTimeTo", order.dteDeliveryTimeTo),
                new MySqlParameter("@CollectionRetailID", order.CollectionRetailID),
                new MySqlParameter("@CloseTerminalID", order.CloseTerminalID),
                new MySqlParameter("@EmployeeID", order.EmployeeID),
                new MySqlParameter("@EmployeeName", order.EmployeeName),
                new MySqlParameter("@MacAddress", order.MacAddress),
                new MySqlParameter("@MemDOBDiscPerc", order.MemDOBDiscPerc),
                new MySqlParameter("@MemDOBDiscAmount", order.MemDOBDiscAmount),
                new MySqlParameter("@ShippingFee", order.ShippingFee),
                new MySqlParameter("@vchQueueNo", queuenostr),
                new MySqlParameter("@ReceiptOrderStatus", order.ReceiptOrderStatus),
                new MySqlParameter("@TableNumber", order.TableNumber),
                new MySqlParameter("@PendingSync", order.PendingSync),
                new MySqlParameter("@SyncAPI", order.SyncAPI),
                new MySqlParameter("@LastUser", user),
                new MySqlParameter("@LastUpdate", todaytimestamp),
                new MySqlParameter("@LockUser", user),
                new MySqlParameter("@LockUpdate", todaytimestamp),
                new MySqlParameter("@LockStatus", "0"),
                new MySqlParameter("@RecordStatus", status),
                new MySqlParameter("@RecordUpdate", todaytimestamp),
                new MySqlParameter("@QueueStatus", status),
                new MySqlParameter("@TerminalID", order.TerminalID)
            };
            int xmain = dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, Insert_main, objparam_main);

            if (CustID.Length > 0)
            {
                string custdetailid = dataAccessor.ExecScalarVal(sqluuid, "").ToString();
                string insert_cust = "INSERT INTO Customer_SalesDetailS(ID,RetailID,TransID,TransDate,CustID,CustHP,CustName,bitCreateNew,TransRef,TotalAmount,bitLoyalty,LoyaltyPoint,bitDiscount,TotalDiscount,bitRedeem,RedeemPoint,LastUser,LastUpdate,";
                insert_cust += "LockUser,LockUpdate,LockStatus,RecordStatus,RecordUpdate,QueueStatus,TerminalID,bitRedeemMemDOBDisc,MemDOBDiscPerc,MemDOBDiscAmount,PreviousPoint,ExpiryPoint,PendingSync) values(@ID, @RetailID,@TransID,@TransDate,";
                insert_cust += "@CustID,@CustHP,@CustName,@bitCreateNew,@TransRef,@TotalAmount,@bitLoyalty,@LoyaltyPoint,@bitDiscount,@TotalDiscount,@bitRedeem,@RedeemPoint,@LastUser,@LastUpdate,";
                insert_cust += "@LockUser,@LockUpdate,@LockStatus,@RecordStatus,@RecordUpdate,@QueueStatus,@TerminalID,@bitRedeemMemDOBDisc,@MemDOBDiscPerc,@MemDOBDiscAmount,@PreviousPoint,@ExpiryPoint,@PendingSync);";

                MySqlParameter[] objparam_cust =
                {
                    new MySqlParameter("@ID" ,custdetailid),
                    new MySqlParameter("@RetailID" ,order.RetailID),
                    new MySqlParameter("@TransID" ,ID),
                    new MySqlParameter("@TransDate" ,order.SalesDate),
                    new MySqlParameter("@CustID" ,CustID),
                    new MySqlParameter("@CustHP" ,m.MemberHp),
                    new MySqlParameter("@CustName" ,m.MemberName),
                    new MySqlParameter("@bitCreateNew" ,"N"),
                    new MySqlParameter("@TransRef" ,""),
                    new MySqlParameter("@TotalAmount" ,order.SalesBalTtl),
                    new MySqlParameter("@bitLoyalty" ,"N"),
                    new MySqlParameter("@LoyaltyPoint" ,0),
                    new MySqlParameter("@bitDiscount" ,"N"),
                    new MySqlParameter("@TotalDiscount" ,0),
                    new MySqlParameter("@bitRedeem" ,"N"),
                    new MySqlParameter("@RedeemPoint" ,0),
                    new MySqlParameter("@LastUser" ,user),
                    new MySqlParameter("@LastUpdate" ,todaytimestamp),
                    new MySqlParameter("@LockUser" ,user),
                    new MySqlParameter("@LockUpdate" ,todaytimestamp),
                    new MySqlParameter("@LockStatus" ,"0" ),
                    new MySqlParameter("@RecordStatus" ,status),
                    new MySqlParameter("@RecordUpdate" ,todaytimestamp),
                    new MySqlParameter("@QueueStatus" ,status),
                    new MySqlParameter("@TerminalID" ,order.TerminalID),
                    new MySqlParameter("@bitRedeemMemDOBDisc" ,"N"),
                    new MySqlParameter("@MemDOBDiscPerc" ,0),
                    new MySqlParameter("@MemDOBDiscAmount" ,0),
                    new MySqlParameter("@PreviousPoint" ,0),
                    new MySqlParameter("@ExpiryPoint" ,0),
                    new MySqlParameter("@PendingSync" ,"Y")
                };

                dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, insert_cust, objparam_cust);
            }

            int xitem = 0;
            Hashtable ht_lineno = new Hashtable();
            if (order.salesorder_item.Count > 0)
            {
                for (int i = 0; i < order.salesorder_item.Count; i++)
                {
                    SalesOnlineOrder_Item item = order.salesorder_item[i];
                    string itemuuid = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sqluuid, null).ToString();

                    string itemsupbarcode = "", itemGSTIncExc = "", ItemSKU = "", ItemSupplierID = "", ItemUOM = "";
                    double itemactqty = 0.000, itemcost = 0.00, itemcommperc = 0.00, itemcommamt = 0.00, itemretailcost=0.00, itemactcost=0.00;

                    string sql_filteriteminfor = "select inventory_supbar.SupBarCode,inventory_retail.GSTIncExc, inventory.ItemSKU, inventory_supbar.SupplierID from inventory left join inventory_supbar on inventory.itemid = inventory_supbar.itemid "
                    + "INNER JOIN inventory_retail ON (inventory.ItemID=inventory_retail.ItemID) where inventory.ItemID=@ItemID";
                    MySqlParameter[] objparam_itemid =
                    {
                        new MySqlParameter("@ItemID", item.ItemID)
                    };

                    DataSet ds_iteminfor = new DataSet();
                    ds_iteminfor = dataAccessor.ExecuteDataSet(objTran, CommandType.Text, sql_filteriteminfor, objparam_itemid);

                    if (ds_iteminfor.Tables.Count > 0 && ds_iteminfor.Tables[0].Rows.Count > 0)
                    {
                        itemsupbarcode = ds_iteminfor.Tables[0].Rows[0]["SupBarCode"].ToString();
                        if (ds_iteminfor.Tables[0].Rows[0]["GSTIncExc"].ToString() == "2")
                        {
                            itemGSTIncExc = "E";
                        }
                        else
                        {
                            itemGSTIncExc = "I";
                        }
                        ItemSKU = ds_iteminfor.Tables[0].Rows[0]["ItemSKU"].ToString();
                        ItemSupplierID = ds_iteminfor.Tables[0].Rows[0]["SupplierID"].ToString();
                    }

                    string itemunitid = "";

                    sql_filteriteminfor = "SELECT ID FROM LIST_UNITS WHERE VALUE=@ItemUOMDesc";

                    MySqlParameter[] objparam_uomdesc =
                    {
                        new MySqlParameter("@ItemUOMDesc", item.ItemUOMDesc)
                    };
                    try
                    {
                        itemunitid = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sql_filteriteminfor, objparam_uomdesc).ToString();
                    }
                    catch (Exception exunit)
                    {
                        itemunitid = "";
                    }

                    if (itemunitid.Length == 0)
                    {
                        sql_filteriteminfor = "SELECT ID FROM LIST_UNITS WHERE ID = (SELECT ITEMUNIT FROM INVENTORY_UNIT WHERE ITEMID=@ItemID AND ITEMUNITDEF = 'Y' AND RECORDSTATUS <> 'DELETED' LIMIT 1)";
                        try
                        {
                            itemunitid = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sql_filteriteminfor, objparam_itemid).ToString();
                        }
                        catch (Exception exunit)
                        {
                            itemunitid = "";
                        }
                    }

                    sql_filteriteminfor = "SELECT ITEMACTQTY, PURCHASECOST,COMMPERC,COMMAMT, LIST_UNITS.NICK AS ITEMUOM FROM INVENTORY_UNIT LEFT JOIN LIST_UNITS ON INVENTORY_UNIT.ITEMUNIT = LIST_UNITS.ID WHERE ITEMID = '" + item.ItemID + "'";
                    sql_filteriteminfor += " AND INVENTORY_UNIT.RECORDSTATUS <> 'DELETED' AND INVENTORY_UNIT.ITEMUNIT=@ItemUnit";
                    MySqlParameter[] objparam_itemunit =
                    {
                        new MySqlParameter("@ItemUnit", itemunitid)
                    };

                    ds_iteminfor = dataAccessor.ExecuteDataSet(objTran, CommandType.Text, sql_filteriteminfor, objparam_itemunit);

                    if (ds_iteminfor.Tables.Count > 0 && ds_iteminfor.Tables[0].Rows.Count > 0)
                    {
                        itemactqty = double.Parse(ds_iteminfor.Tables[0].Rows[0]["ITEMACTQTY"].ToString());
                        ItemUOM = ds_iteminfor.Tables[0].Rows[0]["ITEMUOM"].ToString();
                        itemcost = double.Parse(ds_iteminfor.Tables[0].Rows[0]["PURCHASECOST"].ToString());
                        itemcommamt = double.Parse(ds_iteminfor.Tables[0].Rows[0]["COMMAMT"].ToString());
                        itemcommperc = double.Parse(ds_iteminfor.Tables[0].Rows[0]["COMMPERC"].ToString());
                    }


                    string sql_retailcost = "SELECT ItemCost FROM inventory_retail WHERE ItemID=@ItemID AND RetailID=@RetailID AND RecordStatus<>'DELETED'";
                    MySqlParameter[] objparam1 =
                    {
                        new MySqlParameter("@ItemID", item.ItemID),
                        new MySqlParameter("@RetailID", order.RetailID)
                    };

                    ds_iteminfor = dataAccessor.ExecuteDataSet(objTran, CommandType.Text, sql_retailcost, objparam1);
                    if (ds_iteminfor.Tables.Count > 0 && ds_iteminfor.Tables[0].Rows.Count > 0)
                    {
                        itemretailcost = double.Parse(ds_iteminfor.Tables[0].Rows[0]["ItemCost"].ToString());
                        itemactcost = itemretailcost * itemactqty;
                    }
                    else {
                        itemactcost = itemcost;
                    }
                    ds_iteminfor.Dispose();
                    ds_iteminfor.Clear();

                    ht_lineno.Add(item.LineNo, itemuuid);
                    string addondetailid = "";
                    if (item.bitAddOnItem == "Y" && item.ParentDetailID_ADDON.Length > 0)
                    {
                        addondetailid = ht_lineno[item.LineNo].ToString();
                    }

                    string Insert_item = "INSERT INTO retail_sales_detail(Sales_DetailID,SalesID,RetailID,ItemID,SupbarCode,ItemQty,ItemUOM,ItemUOMDesc,ItemQtyAct,ItemUnitPrice,ItemUnitCost,ItemDisc,ItemDisc2,ItemDisc3,ItemDiscAmt,ItemDiscAmt2,";
                    Insert_item += "ItemDiscAmt3,ItemTotalDisc,ItemDiscGroupAmt,ItemSubTotal,ItemTaxTotal,ItemTotal,ItemQtyDeliver,ItemQtyRemain,ItemTaxType,SupplierID,PromoDiscAmt,PromoVIPDiscAmt,PromoDiscTypeCode,PromoDiscTypeID,PromoID,";
                    Insert_item += "GroupDiscAmt,GroupDiscAmt2,GroupDiscAmt3,GroupDiscPerc,GroupDiscPerc2,GroupDiscPerc3,MemberDisc,MemberAmt,CardAmt,OpenPriceRemark,ItemRemark,ItemDiscType,SalesPersonID,SalesCommTypeID,SalesCommPerc,";
                    Insert_item += "ItemCommPerc,ItemCommAmt,ItemSerialNo,ItemIMEINo,ItemStatus,RedeemPoint,ParentItemID_ADDON,bitAddOnItem,ParentDetailID_ADDON,MemDOBDiscPerc,MemDOBDiscAmount,CollectionRetailID,CollectionDate,CollectionTerminalID,";
                    Insert_item += "ItemSSPx,ReceiptOrderStatus,TerminalID,RFID,PendingSync,LastUser,LastUpdate,LockUser,LockUpdate,LockStatus,RecordStatus,RecordUpdate,QueueStatus) values (@Sales_DetailID,@SalesID,@RetailID,@ItemID,@SupbarCode,@ItemQty,@ItemUOM,";
                    Insert_item += "@ItemUOMDesc,@ItemQtyAct,@ItemUnitPrice,@ItemUnitCost,@ItemDisc,@ItemDisc2,@ItemDisc3,@ItemDiscAmt,@ItemDiscAmt2,@ItemDiscAmt3,@ItemTotalDisc,@ItemDiscGroupAmt,@ItemSubTotal,@ItemTaxTotal,@ItemTotal,";
                    Insert_item += "@ItemQtyDeliver,@ItemQtyRemain,@ItemTaxType,@SupplierID,@PromoDiscAmt,@PromoVIPDiscAmt,@PromoDiscTypeCode,@PromoDiscTypeID,@PromoID,@GroupDiscAmt,@GroupDiscAmt2,@GroupDiscAmt3,@GroupDiscPerc,@GroupDiscPerc2,@GroupDiscPerc3,";
                    Insert_item += "@MemberDisc,@MemberAmt,@CardAmt,@OpenPriceRemark,@ItemRemark,@ItemDiscType,@SalesPersonID,@SalesCommTypeID,@SalesCommPerc,@ItemCommPerc,@ItemCommAmt,@ItemSerialNo,@ItemIMEINo,@ItemStatus,@RedeemPoint,@ParentItemID_ADDON,";
                    Insert_item += "@bitAddOnItem,@ParentDetailID_ADDON,@MemDOBDiscPerc,@MemDOBDiscAmount,@CollectionRetailID,@CollectionDate,@CollectionTerminalID,@ItemSSPx,@ReceiptOrderStatus,@TerminalID,@RFID,@PendingSync,";
                    Insert_item += "@LastUser,@LastUpdate,@LockUser,@LockUpdate,@LockStatus,@RecordStatus,@RecordUpdate,@QueueStatus);";

                    MySqlParameter[] objparam_item =
                    {
                        new MySqlParameter("@Sales_DetailID", itemuuid),
                        new MySqlParameter("@SalesID", ID),
                        new MySqlParameter("@RetailID", item.RetailID),
                        new MySqlParameter("@ItemID", item.ItemID),
                        new MySqlParameter("@SupbarCode", itemsupbarcode),
                        new MySqlParameter("@ItemQty", item.ItemQty),
                        new MySqlParameter("@ItemUOM", itemunitid),
                        new MySqlParameter("@ItemUOMDesc", ItemUOM),
                        new MySqlParameter("@ItemQtyAct", itemactqty),
                        new MySqlParameter("@ItemUnitPrice", item.ItemUnitPrice),
                        new MySqlParameter("@ItemUnitCost", itemactcost),
                        new MySqlParameter("@ItemDisc", item.ItemDisc ),
                        new MySqlParameter("@ItemDisc2", item.ItemDisc2),
                        new MySqlParameter("@ItemDisc3", item.ItemDisc3),
                        new MySqlParameter("@ItemDiscAmt", item.ItemDiscAmt),
                        new MySqlParameter("@ItemDiscAmt2", item.ItemDiscAmt2),
                        new MySqlParameter("@ItemDiscAmt3", item.ItemDiscAmt3),
                        new MySqlParameter("@ItemTotalDisc", item.ItemTotalDisc),
                        new MySqlParameter("@ItemDiscGroupAmt", item.ItemDiscGroupAmt ),
                        new MySqlParameter("@ItemSubTotal", item.ItemSubTotal),
                        new MySqlParameter("@ItemTaxTotal", item.ItemTaxTotal ),
                        new MySqlParameter("@ItemTotal", item.ItemTotal),
                        new MySqlParameter("@ItemQtyDeliver", item.ItemQtyDeliver),
                        new MySqlParameter("@ItemQtyRemain",  item.ItemQtyRemain),
                        new MySqlParameter("@ItemTaxType", itemGSTIncExc),
                        new MySqlParameter("@SupplierID", ItemSupplierID),
                        new MySqlParameter("@PromoDiscAmt", item.PromoDiscAmt),
                        new MySqlParameter("@PromoVIPDiscAmt", item.PromoVIPDiscAmt),
                        new MySqlParameter("@PromoDiscTypeCode",  item.PromoDiscTypeCode),
                        new MySqlParameter("@PromoDiscTypeID", item.PromoDiscTypeID),
                        new MySqlParameter("@PromoID", item.PromoID),
                        new MySqlParameter("@GroupDiscAmt", item.GroupDiscAmt),
                        new MySqlParameter("@GroupDiscAmt2", item.GroupDiscAmt2),
                        new MySqlParameter("@GroupDiscAmt3", item.GroupDiscAmt3),
                        new MySqlParameter("@GroupDiscPerc", item.GroupDiscPerc),
                        new MySqlParameter("@GroupDiscPerc2", item.GroupDiscPerc2),
                        new MySqlParameter("@GroupDiscPerc3", item.GroupDiscPerc3),
                        new MySqlParameter("@MemberDisc", item.MemberDisc ),
                        new MySqlParameter("@MemberAmt", item.MemberAmt ),
                        new MySqlParameter("@CardAmt", item.CardAmt ),
                        new MySqlParameter("@OpenPriceRemark", item.OpenPriceRemark),
                        new MySqlParameter("@ItemRemark", item.ItemRemark),
                        new MySqlParameter("@ItemDiscType", item.ItemDiscType),
                        new MySqlParameter("@SalesPersonID", item.SalesPersonID),
                        new MySqlParameter("@SalesCommTypeID", item.SalesCommTypeID),
                        new MySqlParameter("@SalesCommPerc", item.SalesCommPerc),
                        new MySqlParameter("@ItemCommPerc", itemcommperc),
                        new MySqlParameter("@ItemCommAmt", itemcommamt),
                        new MySqlParameter("@ItemSerialNo", item.ItemSerialNo),
                        new MySqlParameter("@ItemIMEINo", item.ItemIMEINo),
                        new MySqlParameter("@ItemStatus", item.ItemStatus),
                        new MySqlParameter("@RedeemPoint", item.RedeemPoint),
                        new MySqlParameter("@ParentItemID_ADDON", item.ParentItemID_ADDON),
                        new MySqlParameter("@bitAddOnItem", item.bitAddOnItem ),
                        new MySqlParameter("@ParentDetailID_ADDON", addondetailid),
                        new MySqlParameter("@MemDOBDiscPerc", item.MemDOBDiscPerc),
                        new MySqlParameter("@MemDOBDiscAmount", item.MemDOBDiscAmount),
                        new MySqlParameter("@CollectionRetailID", item.CollectionRetailID),
                        new MySqlParameter("@CollectionDate", item.CollectionDate),
                        new MySqlParameter("@CollectionTerminalID", item.CollectionTerminalID),
                        new MySqlParameter("@ItemSSPx", item.ItemSSPx),
                        new MySqlParameter("@ReceiptOrderStatus", item.ReceiptOrderStatus),
                        new MySqlParameter("@TerminalID", item.TerminalID),
                        new MySqlParameter("@RFID", item.RFID),
                        new MySqlParameter("@PendingSync", item.PendingSync),
                        new MySqlParameter("@LastUser", user),
                        new MySqlParameter("@LastUpdate", todaytimestamp),
                        new MySqlParameter("@LockUser", user),
                        new MySqlParameter("@LockUpdate", todaytimestamp),
                        new MySqlParameter("@LockStatus", "0" ),
                        new MySqlParameter("@RecordStatus", status),
                        new MySqlParameter("@RecordUpdate", todaytimestamp),
                        new MySqlParameter("@QueueStatus", status)
                    };
                    int x = dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, Insert_item, objparam_item);

                    if (x <= 0)
                    {
                        xitem = 0;
                    }
                    else
                    {
                        xitem = 1;
                    }
                }
            }

            int xpayment = 0;
            if (order.salesorder_payment.Count > 0)
            {
                for (int i = 0; i < order.salesorder_payment.Count; i++)
                {
                    SalesOnlineOrder_Payment payment = order.salesorder_payment[i];
                    string paymentuuid = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sqluuid, null).ToString();

                    string Insert_payment = "INSERT INTO retail_sales_payment(SalesPaymentID,SalesID,RetailID,PaymentID,PaymentReference,PaymentRemarks,SalesPayTtl,SalesBalTtl,SalesDeposit,ChangeAmount,TipsAmount,PaymentStatus,OthersPayment,";
                    Insert_payment += "OthersPaymentRef,DepositStatus,Close_RetailID,Close_SalesID,Close_TerminalID,CardDisc,CardAmt,TerminalID,PendingSync,LastUser,LastUpdate,LockUser,LockUpdate,LockStatus,RecordStatus,RecordUpdate,QueueStatus) values (";
                    Insert_payment += "@SalesPaymentID,@SalesID,@RetailID,@PaymentID,@PaymentReference,@PaymentRemarks,@SalesPayTtl,@SalesBalTtl,@SalesDeposit,@ChangeAmount,@TipsAmount,@PaymentStatus,@OthersPayment,";
                    Insert_payment += "@OthersPaymentRef,@DepositStatus,@Close_RetailID,@Close_SalesID,@Close_TerminalID,@CardDisc,@CardAmt,@TerminalID,@PendingSync,@LastUser,@LastUpdate,@LockUser,@LockUpdate,@LockStatus,@RecordStatus,@RecordUpdate,@QueueStatus)";
                    MySqlParameter[] objparam_payment =
                    {
                        new MySqlParameter("@SalesPaymentID", paymentuuid),
                        new MySqlParameter("@SalesID", ID),
                        new MySqlParameter("@RetailID", payment.RetailID),
                        new MySqlParameter("@PaymentID", ""),
                        new MySqlParameter("@PaymentReference", payment.PaymentReference),
                        new MySqlParameter("@PaymentRemarks", payment.PaymentRemarks ),
                        new MySqlParameter("@SalesPayTtl", payment.SalesPayTtl),
                        new MySqlParameter("@SalesBalTtl", payment.SalesBalTtl),
                        new MySqlParameter("@SalesDeposit", payment.SalesDeposit),
                        new MySqlParameter("@ChangeAmount", payment.ChangeAmount ),
                        new MySqlParameter("@TipsAmount", payment.TipsAmount),
                        new MySqlParameter("@PaymentStatus", payment.PaymentStatus),
                        new MySqlParameter("@OthersPayment", payment.OthersPayment),
                        new MySqlParameter("@OthersPaymentRef", payment.OthersPaymentRef),
                        new MySqlParameter("@DepositStatus", payment.DepositStatus),
                        new MySqlParameter("@Close_RetailID", payment.Close_RetailID),
                        new MySqlParameter("@Close_SalesID", payment.Close_SalesID),
                        new MySqlParameter("@Close_TerminalID", payment.Close_TerminalID),
                        new MySqlParameter("@CardDisc",  payment.CardDisc),
                        new MySqlParameter("@CardAmt", payment.CardAmt),
                        new MySqlParameter("@TerminalID", payment.TerminalID),
                        new MySqlParameter("@PendingSync", payment.PendingSync),
                        new MySqlParameter("@LastUser", user),
                        new MySqlParameter("@LastUpdate", todaytimestamp),
                        new MySqlParameter("@LockUser", user),
                        new MySqlParameter("@LockUpdate", todaytimestamp),
                        new MySqlParameter("@LockStatus", "0"),
                        new MySqlParameter("@RecordStatus", status),
                        new MySqlParameter("@RecordUpdate", todaytimestamp),
                        new MySqlParameter("@QueueStatus", status)
                    };

                    int x = dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, Insert_payment, objparam_payment);

                    if (x <= 0)
                    {
                        xpayment = 0;
                    }
                    else
                    {
                        xpayment = 1;
                    }
                }
            }

            string sql = "SELECT PREFIXSDO, SDOSTARTNO FROM DEFINITIONS WHERE DISPLAY = 'Y'";
            DataSet ds_definitions = dataAccessor.ExecuteDataSet(objTran, CommandType.Text, sql, null);
            string prefix = "SDO";
            int startno = 1000;
            if (ds_definitions.Tables[0].Rows.Count > 0)
            {
                prefix = ds_definitions.Tables[0].Rows[0]["PREFIXSDO"].ToString();
                startno = int.Parse(ds_definitions.Tables[0].Rows[0]["SDOSTARTNO"].ToString());
            }

            sql = "SELECT count(ID) FROM SALESDELIVERY";
            int maxid = 0;
            try
            {
                maxid = int.Parse(dataAccessor.ExecuteScalar(objTran, CommandType.Text, sql, null).ToString());
            }
            catch (System.Exception ex)
            {

            }

            string IDRef = prefix + "/" + (startno + maxid + 1).ToString("0000") + "/" + DateTime.Now.Year.ToString() + "_" + order.RetailID + order.TerminalID;

            string deliveryid = dataAccessor.ExecuteScalar(objTran, CommandType.Text, sqluuid, null).ToString();

            string insert_delivery = "INSERT INTO SALESDELIVERY (ID, TYPE,DATE,CompanyAddr,RecipientAddr,SN_Ref,RecipientName,RecipientAttn,RecipientPostCode,RecipientTel,RecipientFax,";
            insert_delivery += "CompanyTel,CompanyFax,IDRef,RetailerID,INVID,INVRef,INVDate,INVRetailerID,Gst,GSTIncEx,GstRate,BalSubTotal,BalTax,TotalDiscount,BalTotal,BalPayable,";
            insert_delivery += "LocalBalSubTotal,LocalTax,LocalTotalDiscount,LocalTotal,LocalBalPayable,vchRemarks,Document_Status,OutStandingBal,LocalOutStandingBal,DepositAmount,";
            insert_delivery += "LastUser,LastUpdate,LockUser,LockUpdate,LockStatus,PendingSync,RecordUpdate,QueueStatus,TerminalID) ";
            insert_delivery += "SELECT @ID, 'SDO', @TransDate,'',@RecipientAddr,'',@RecipientName,'', '', @RecipientTel,'', '', '', @IDRef, @RetailerID, SALESID, SALESNO, SALESDATE, RETAILID,";
            insert_delivery += "CASE WHEN SALESTAXTTL > 0 THEN 'Y' ELSE 'N' END, (SELECT ID FROM list_gstincexc WHERE LEFT(Nick,1) = SALESTAX AND RecordStatus<>'DELETED') AS SALESTAX,";
            insert_delivery += " SALESTAXVAL,SALESSUBTTL, SALESTAXTTL,SALESTOTALGROUPDISC+MEMBERAMT+CARDAMT,SALESBALTTL,SALESPAYTTL, SALESSUBTTL,SALESTAXTTL,SALESTOTALGROUPDISC+MEMBERAMT+CARDAMT,";
            insert_delivery += "SALESBALTTL,SALESPAYTTL,'','DEPOSIT',SALESBALTTL - SALESPAYTTL,SALESBALTTL - SALESPAYTTL,SALESDEPOSIT,@LastUser, @LastUpdate, @LockUser, @LockUpdate, @LockStatus, @PendingSync,";
            insert_delivery += "@RecordUpdate, @QueueStatus, @TerminalID FROM RETAIL_SALES WHERE SALESID=@SalesID LIMIT 1";

            MySqlParameter[] objparam_delivery =
            {
                new MySqlParameter("@ID", deliveryid),
                new MySqlParameter("@TransDate", todaystr),
                new MySqlParameter("@RecipientAddr", m.MemberAddress),
                new MySqlParameter("@RecipientName",  m.MemberName),
                new MySqlParameter("@RecipientTel", m.MemberHp),
                new MySqlParameter("@IDRef", IDRef),
                new MySqlParameter("@RetailerID", order.RetailID),
                new MySqlParameter("@LastUser", user),
                new MySqlParameter("@LastUpdate", todaytimestamp),
                new MySqlParameter("@LockUser", user),
                new MySqlParameter("@LockUpdate", todaytimestamp),
                new MySqlParameter("@LockStatus", "0"),
                new MySqlParameter("@PendingSync", "Y"),
                new MySqlParameter("@RecordUpdate", todaytimestamp),
                new MySqlParameter("@QueueStatus", "READY"),
                new MySqlParameter("@TerminalID", order.TerminalID),
                new MySqlParameter("@SalesID", ID)
            };

            //insert_delivery += "SELECT '" + deliveryid + "', 'SDO', '" + todaystr + "','','" + m.MemberAddress + "','','" + m.MemberName + "','', '', '" + m.MemberHp + "','', '', '', '";
            //insert_delivery += IDRef + "', '" + order.RetailID + "'";
            //insert_delivery += ", SALESID, SALESNO, SALESDATE, RETAILID,  CASE WHEN SALESTAXTTL > 0 THEN 'Y' ELSE 'N' END, (SELECT ID FROM list_gstincexc WHERE LEFT(Nick,1) = SALESTAX AND RecordStatus<>'DELETED') AS SALESTAX,";
            //insert_delivery += " SALESTAXVAL,SALESSUBTTL,";
            //insert_delivery += " SALESTAXTTL,SALESTOTALGROUPDISC+MEMBERAMT+CARDAMT,SALESBALTTL,SALESPAYTTL, SALESSUBTTL,SALESTAXTTL,SALESTOTALGROUPDISC+MEMBERAMT+CARDAMT,";
            //insert_delivery += "SALESBALTTL,SALESPAYTTL,'','DEPOSIT',SALESBALTTL - SALESPAYTTL,SALESBALTTL - SALESPAYTTL,SALESDEPOSIT,'";
            //insert_delivery += user + "','" + todaytimestamp + "','', '" + todaytimestamp + "', '0', 'Y', '" + todaytimestamp;
            //insert_delivery += "', 'READY', '" + order.TerminalID + "' FROM RETAIL_SALES WHERE SALESID = '" + ID + "' LIMIT 1";

            int xdelivery = dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, insert_delivery, objparam_delivery);

            insert_delivery = "INSERT INTO SALESDELIVERY_ITEM (KEYCOL, ITEMID, SUPBARITEM, SUPBARITEMID, OBJID, ID,";
            insert_delivery += "ITEMREMARK, PRICELEVEL, ITEMQTY,ITEMSOQTY, ACTUALQTY, ACTUALSOQTY, GST, GSTTYPE, GSTRATE,";
            insert_delivery += "ITEMPRICE, ITEMDISCAMT, DISC_PCN1, DISC_PCN2, DISC_PCN3, ITEMDATA, ITEMUNIT, ITEMUNITID, ITEMBAL, ITEMSUBTOTAL,";
            insert_delivery += "ITEMGST, TOTALDISC, TOTAL, LOCALITEMPRICE, LOCALITEMDISCAMT, LOCALTOTALDISC, LOCALITEMSUBTOTAL, LOCALITEMGST,";
            insert_delivery += "LOCALTOTAL, ITEMFOC, DTEDELIVERY, QTYREMAIN, ITEM_AC_ASSET, LASTUSER, LASTUPDATE, LOCKUSER, LOCKUPDATE, LOCKSTATUS,";
            insert_delivery += "RECORDSTATUS, RECORDUPDATE, QUEUESTATUS, TERMINALID, RETAILERID, RFID,PENDINGSYNC) SELECT UUID(), ITEMID, SUPBARCODE, ITEMID, 0, @ID, ";
            insert_delivery += "ITEMREMARK,";
            insert_delivery += "NULL,'0' AS ITEMQTYDELIVER, 0, 1, 0, IF(ItemTaxTotal=0,'N','Y') AS ITEMTAX,";
            insert_delivery += "(SELECT ID FROM list_gstincexc WHERE LEFT(Nick,1) = ITEMTAXTYPE AND RecordStatus<>'DELETED') AS ITEMTAXTYPE, CASE WHEN ITEMTAXTYPE = 'N'";
            insert_delivery += " THEN 0 ELSE 7 END, ITEMUNITPRICE, ITEMDISCAMT+ITEMDISCAMT2+ITEMDISCAMT3, ITEMDISC, ITEMDISC2, ITEMDISC3, 0, ITEMUOMDESC,";
            insert_delivery += "ITEMUOM, 0, ITEMSUBTOTAL, ITEMTAXTOTAL, ITEMDISCGROUPAMT+GROUPDISCAMT+GROUPDISCAMT2+GROUPDISCAMT3+CARDAMT+MEMBERAMT,";
            insert_delivery += "ITEMTOTAL,ITEMUNITPRICE,ITEMDISCAMT+ITEMDISCAMT2+ITEMDISCAMT3,ITEMDISCGROUPAMT+GROUPDISCAMT+GROUPDISCAMT2+GROUPDISCAMT3+CARDAMT+MEMBERAMT,";
            insert_delivery += "ITEMSUBTOTAL, ITEMTAXTOTAL,ITEMTOTAL, CASE WHEN PROMODISCTYPECODE = 'FOC' THEN 'Y' ELSE 'N' END, NULL, NULL, 0, ";
            insert_delivery += "LASTUSER, LASTUPDATE,LOCKUSER, LOCKUPDATE, LOCKSTATUS,'READY' AS RECORDSTATUS, RECORDUPDATE, QUEUESTATUS,TERMINALID,RETAILID, RFID,'Y' AS PENDINGSYNC FROM RETAIL_SALES_DETAIL";
            insert_delivery += " WHERE SALESID=@SalesID";

            MySqlParameter[] objparam_deliveryitem =
            {
                new MySqlParameter("@ID", deliveryid),
                new MySqlParameter("@SalesID", ID)
            };

            int xdeliveryitem = dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, insert_delivery, objparam_deliveryitem);

            string sql_forqueueno = "UPDATE DEFINITIONS_TERMINAL SET LastQueueNo = LastQueueNo + 1 WHERE DISPLAY = 'Y' AND RETAILERID=@RetailerID AND TERMINALID=@TerminalID";
            MySqlParameter[] objparam_queueno =
            {
                new MySqlParameter("@RetailerID", order.RetailID),
                new MySqlParameter("@TerminalID", (order.TerminalID == "0" ? "1" : order.TerminalID))
            };
            int xqueueno = dataAccessor.ExecuteNonQuery(objTran, CommandType.Text, sql_forqueueno, objparam_queueno);

            try
            {
                objTran.Commit();
                if (xmain > 0 && xitem > 0 && xpayment > 0 && xdelivery > 0 && xdeliveryitem > 0 && xqueueno > 0)
                {
                    s = "Success";
                    idstr = ID;
                }
                else
                {
                    s = "Fail";
                    idstr = "";
                }
            }
            catch (Exception emain)
            {
                objTran.Rollback();
                s = "Fail";
                idstr = "";
            }
            objcon.Close();
            objcon.Dispose();
            objTran.Dispose();
            return new JavaScriptSerializer().Serialize(new { Status = s, OrderID = idstr, QueueNo = queuenostr });
        }

    }
 

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetSalesForOrdering(string companyCode, string retailid)
    {
        try
        {
            clsDataAccessor dataAccessor = new clsDataAccessor();
            dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

            string OnlineRetailID = "0";
            string sqlstr = "SELECT RETAILID FROM RETAILER WHRER RETAILNAME = 'ONLINE'";
            try
            {
                OnlineRetailID = dataAccessor.ExecScalarVal(sqlstr, "").ToString();
            }
            catch (Exception eon)
            {
                OnlineRetailID = "0";
            }

            sqlstr = "SELECT SALESID, SALESNO, DATE_FORMAT(SALESDATE,'%Y-%m-%d') AS SALESDATE, DATE_FORMAT(CREATETIME,'%H:%i:%s') AS SALESTIME, SALESBALTTL AS SALESAMOUNT,'O' AS RECEIPTFLAG, vchQueueNo as QueueNo, TableNumber as TableNo,ReceiptOrderStatus AS OrderStatus FROM RETAIL_SALES WHERE SALESDATE = DATE_FORMAT(CURRENT_DATE(),'%Y-%m-%d') AND COLLECTIONRETAILID = 0 AND SALESSTATUS = 'SALES' ";
            sqlstr += " UNION ALL ";
            sqlstr += "SELECT TRANSID AS SALESID, TRANSNO AS SALESNO, DATE_FORMAT(TRANSDATE,'%Y-%m-%d') AS SALESDATE, DATE_FORMAT(CREATETIME,'%H:%i:%s') AS SALESTIME, TOTALDUE AS SALESAMOUNT, 'H' AS RECEIPTFLAG, vchQueueNo as QueueNo, intTableNo as TableNo,ReceiptOrderStatus AS OrderStatus FROM RETAIL_SALES_HOLDHDR WHERE TRANSDATE =  DATE_FORMAT(CURRENT_DATE(),'%Y-%m-%d') AND RETAILERID=@RetailID";
            DataSet ds_online = new DataSet();

            MySqlParameter[] objparam =
            {
                new MySqlParameter("@RetailID", retailid)
            };
            ds_online = dataAccessor.RunSPRetDataset_Vapt(sqlstr, "RETAIL_SALES", objparam);

            return JsonConvert.SerializeObject(ds_online, Formatting.Indented);

        }
        catch (Exception E)
        {
            return E.Message.ToString();
        }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdateStatusForOnlineOrder(string companyCode, string retailid, string terminalid, string salesno, string salestype)
    {
        try
        {
            string statusstr = "FAIL";

            clsDataAccessor dataAccessor = new clsDataAccessor();
            dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
            string sqlstr = "";

            MySqlParameter[] objParam =
            {
                new MySqlParameter("@TransNo", salesno),
                new MySqlParameter("@RetailID", retailid),
                new MySqlParameter("@TerminalID", terminalid)
            };
            if (salestype == "HOLD")
            {
                sqlstr = "DELETE FROM RETAIL_SALES_HOLDHDR WHERE TRANSNO=@TransNo AND RETAILERID=@RetailID;";
                sqlstr += "DELETE FROM RETAIL_SALES_HOLDDTL WHERE TRANSID = (SELECT TRANSID FROM RETAIL_SALES_HOLDHDR WHERE TRANSNO=@TransNo AND RETAILERID=@RetailID);";
            }
            else
            {
                sqlstr = "UPDATE RETAIL_SALES SET COLLECTIONRETAILID=@RetailID,CLOSETERMINALID=@TerminalID WHERE SALESNO=@TransNo";
            }

            int i = dataAccessor.Exec_UpdateQuery_Vapt(sqlstr, "", objParam);

            if (i > 0)
            {
                statusstr = "Success";
            }
            else
            {
                statusstr = "FAIL";
            }

            return new JavaScriptSerializer().Serialize(new { Status = statusstr });
        }
        catch (Exception E)
        {
            return E.Message.ToString();
        }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //UPDATE BY JULIE @2020-07-06
	public string GetDetailsForOrder(string companyCode, string salesno, string retailid, string salesstatus)
    {
        try
        {
            string statusstr = "FAIL";
            string sqlstr = "";
            string JsonDatastr = "";
            DataSet ds = new DataSet();
            clsDataAccessor dataAccessor = new clsDataAccessor();
            dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

            if (salesstatus == "HOLD")
            {
                sqlstr = "select * from retail_sales_holdhdr where transno=@TransNo and retailerid=@RetailID";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@TransNo", salesno),
                    new MySqlParameter("@RetailID", retailid)
                };
                DataSet ds_result = dataAccessor.RunSPRetDataset_Vapt(sqlstr, "retail_sales_holdhdr", objparam);

                if (ds_result.Tables.Count > 0 && ds_result.Tables[0].Rows.Count > 0)
                {
                    string holdid = "";
                    DataTable dt = ds_result.Tables[0];
                    holdid = dt.Rows[0]["TransID"].ToString();
                    dt.TableName = "RETAIL_SALES_HOLDHDR";
                    ds.Tables.Add(dt.Copy());

                    sqlstr = "select retail_sales_holddtl.*, inventory.itemname FROM retail_sales_holddtl LEFT JOIN inventory ON retail_sales_holddtl.itemid = inventory.itemid where retail_sales_holddtl.TransID=@TransID";
                    MySqlParameter[] objparam1 =
                    {
                        new MySqlParameter("@TransID", holdid)
                    };
                    ds_result = dataAccessor.RunSPRetDataset_Vapt(sqlstr, "retail_sales_holddtl", objparam1);
                    if (ds_result.Tables.Count > 0 && ds_result.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt1 = ds_result.Tables[0];
                        dt1.TableName = "RETAIL_SALES_HOLDDTL";
                        ds.Tables.Add(dt1.Copy());
                    }
                    JsonDatastr = JsonConvert.SerializeObject(ds, Formatting.Indented);
                }
            }
            else
            {
                sqlstr = "select * from retail_sales where salesno=@SalesNo";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@SalesNo", salesno)
                };
                DataSet ds_result = dataAccessor.RunSPRetDataset_Vapt(sqlstr, "retail_sales", objparam);
                if (ds_result.Tables.Count > 0 && ds_result.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds_result.Tables[0];
                    string salesid = dt.Rows[0]["SALESID"].ToString();
                    dt.TableName = "RETAIL_SALES";
                    ds.Tables.Add(dt.Copy());

                    MySqlParameter[] objparam1 =
                    {
                        new MySqlParameter("@SalesID", salesid)
                    };

                    sqlstr = "select retail_sales_detail.*, inventory.ItemName from retail_sales_detail left join inventory on retail_sales_detail.itemid = inventory.itemid where retail_sales_detail.salesid=@SalesID";
                    ds_result = dataAccessor.RunSPRetDataset_Vapt(sqlstr, "retail_sales_detail", objparam1);
                    if (ds_result.Tables.Count > 0 && ds_result.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt1 = ds_result.Tables[0];
                        dt1.TableName = "RETAIL_SALES_DETAIL";
                        ds.Tables.Add(dt1.Copy());
                    }

                    sqlstr = "select * from retail_sales_payment where salesid=@SalesID";
                    ds_result = dataAccessor.RunSPRetDataset_Vapt(sqlstr, "retail_sales_payment", objparam1);

                    if (ds_result.Tables.Count > 0 && ds_result.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt2 = ds_result.Tables[0];
                        dt2.TableName = "RETAIL_SALES_PAYMENT";
                        ds.Tables.Add(dt2.Copy());
                    }

                    sqlstr = "select * from customer_salesdetails where transid=@SalesID";
                    ds_result = dataAccessor.RunSPRetDataset_Vapt(sqlstr, "customer_salesdetails", objparam1);

                    if (ds_result.Tables.Count > 0 && ds_result.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt3 = ds_result.Tables[0];
                        dt3.TableName = "CUSTOMER_SALESDETAILS";
                        ds.Tables.Add(dt3.Copy());
                    }


                    sqlstr = "select * from salesdelivery where invid=@SalesID";
                    ds_result = dataAccessor.RunSPRetDataset_Vapt(sqlstr, "salesdelivery", objparam1);
                    string deliveryid = "";
                    if (ds_result.Tables.Count > 0 && ds_result.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt4 = ds_result.Tables[0];
                        deliveryid = dt4.Rows[0]["ID"].ToString();
                        dt4.TableName = "SALESDELIVERY";
                        ds.Tables.Add(dt4.Copy());
                    }

                    sqlstr = "select * from salesdelivery_item where id=@ID";
                    MySqlParameter[] objparam2 = { new MySqlParameter("@ID", deliveryid) };

                    ds_result = dataAccessor.RunSPRetDataset_Vapt(sqlstr, "salesdelivery_item", objparam2);

                    if (ds_result.Tables.Count > 0 && ds_result.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt5 = ds_result.Tables[0];
                        dt5.TableName = "salesdelivery_item";
                        ds.Tables.Add(dt5.Copy());
                    }

                    JsonDatastr = JsonConvert.SerializeObject(ds, Formatting.Indented);
                }
            }

            return JsonDatastr;
        }
        catch (Exception E)
        {
            return E.Message.ToString();
        }
    }
    
/*    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdatePaymentForOnlineOrder(string companyCode, string salesid, string paymentuniqueno, string paymentstr)
    {
        try
        {
            string statusstr = "FAIL";

            clsDataAccessor dataAccessor = new clsDataAccessor();
            dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

            string paymentidsql = "SELECT ID FROM LIST_PAYMENTMETHODS WHERE VALUE = '" + paymentstr + "'";
            string paymentid = "";

            try
            {
                paymentid = dataAccessor.ExecScalarVal(paymentidsql, "").ToString();
            }
            catch (Exception exp)
            {

            }

            string sqlstr = "UPDATE RETAIL_SALES SET SALESSTATUS = 'SALES' WHERE SALESID = '" + salesid + "';";
            sqlstr += "UPDATE RETAIL_SALES_PAYMENT SET PAYMENTID = '" + paymentid + "', OTHERSPAYMENT = 'ONLINE', OTHERSPAYMENTREF = '" + paymentuniqueno + "' WHERE SALESID = '" + salesid + "';";
            int i = dataAccessor.Exec_UpdateQuery(sqlstr, "");

            if (i > 0)
            {
                statusstr = "Success";
            }
            else
            {
                statusstr = "FAIL";
            }

            return new JavaScriptSerializer().Serialize(new { Status = statusstr });
        }
        catch (Exception E)
        {
            return E.Message.ToString();
        }
    }
    */
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdatePaymentForOnlineOrder(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string response = "";

        onlineSalesPayment salespayment = new onlineSalesPayment();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        try
        {
            //salespayment = serializer.Deserialize<onlineSalesPayment>(json);
            salespayment = JsonConvert.DeserializeObject<onlineSalesPayment>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.updateOnlineSalesPayment(salespayment);

        return response;
    }


    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string DeleteSales(string companyCode, string salesid)
    {
        try
        {
            string statusstr = "FAIL";

            clsDataAccessor dataAccessor1 = new clsDataAccessor();
            string connstr = dataAccessor1.getConnectionString_Vapt(dataAccessor1, companyCode);
            MySqlTransaction objTran = null;
            MySqlConnection objcon = new MySqlConnection(connstr);
            objcon.Open();
            objTran = (MySqlTransaction)objcon.BeginTransaction();

            clsDataAccessor dataAccessor = new clsDataAccessor();
            dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

            string sql = "UPDATE RETAIL_SALES SET SalesStatus='FAIL' WHERE SALESID=@SalesID;";
            sql += "DELETE FROM CUSTOMER_SALESDETAILS WHERE TRANSID=@SalesID;";
            sql += "UPDATE SALESDELIVERY SET RecordStatus='DELETED' WHERE INVID=@SalesID;";
            sql += "UPDATE SALESDELIVERY_ITEM SET RecordStatus='DELETED' WHERE ID  = (SELECT ID FROM SALESDELIVERY WHERE INVID=@SalesID);";

            MySqlParameter[] objparam =
                {
                new MySqlParameter("@SalesID", salesid)
            };
            dataAccessor.ExecuteNonQuery(objcon, CommandType.Text, sql, objparam);

            try
            {
                objTran.Commit();
                return new JavaScriptSerializer().Serialize(new { Status = "Success" });
            }
            catch (Exception emain)
            {
                objTran.Rollback();
                return new JavaScriptSerializer().Serialize(new { Status = "Fail" });
            }

            objcon.Close();
            objcon.Dispose();
            objTran.Dispose();
            
        }
        catch (Exception E)
        {
            return E.Message.ToString();
        }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetModifierItem(string companyCode, string itemid)
    {
        try
        {
            clsDataAccessor dataAccessor = new clsDataAccessor();
            dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
            string sql_inventory = "SELECT ITEMPACKAGE, ITEMMODIFIER FROM INVENTORY WHERE ITEMID=@ItemID";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@ItemID", itemid)
            };
            DataSet ds = new DataSet();
            ds = dataAccessor.RunSPRetDataset_Vapt(sql_inventory, "INVENTORY", objparam);
            ModifierInfo m = new ModifierInfo();
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                m.ItemPackage = ds.Tables[0].Rows[0]["ItemPackage"].ToString();
                m.ItemModifier = ds.Tables[0].Rows[0]["ItemModifier"].ToString();
            }

            List<ModifierItem> modifieritems = new List<ModifierItem>();

            sql_inventory = "SELECT A.ITEMID, A.ITEMSKU, A.ITEMDESCP, A.ITEMNAME, A.ITEMPIC,C.ITEMQTY, C.ITEMPRICE ,C.Item_UnitID AS ITEMUNITID,IU.ITEMUNIT,IU.UOM,A.ITEMOUTOFSTOCK, ";
            sql_inventory += "A.ItemDepartment,(SELECT VALUE FROM list_inv_departments WHERE ID=A.ItemDepartment) AS DepartmentName, A.ITEMSALESTYPE FROM INVENTORY AS A LEFT JOIN INVENTORY_PACKAGE AS C ON A.ITEMID = C.INV_ITEMID LEFT JOIN ";
            sql_inventory += "(SELECT ITEMID, ITEM_UNITID,ITEMUNIT,U.NICK AS UOM FROM INVENTORY_UNIT LEFT JOIN LIST_UNITS AS U ON U.ID=INVENTORY_UNIT.ItemUnit) AS IU ";
            sql_inventory += " ON IU.ItemID=C.Inv_ItemID WHERE A.RECORDSTATUS <> 'DELETED' AND C.ITEMID=@ItemID AND C.RECORDSTATUS <> 'DELETED' ORDER BY C.ID ASC, A.ItemDepartment ASC";
            ds = dataAccessor.RunSPRetDataset_Vapt(sql_inventory, "INVENTORY", objparam);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ModifierItem items = new ModifierItem();
                    items.ItemID = ds.Tables[0].Rows[i]["ItemID"].ToString();
                    items.ItemSKU = ds.Tables[0].Rows[i]["ItemSKU"].ToString();
                    items.ItemDescp = ds.Tables[0].Rows[i]["ItemDescp"].ToString();
                    items.ItemName = ds.Tables[0].Rows[i]["ItemName"].ToString();
                    items.ItemPic = ds.Tables[0].Rows[i]["ItemPic"].ToString();
                    items.ItemQty = double.Parse(ds.Tables[0].Rows[i]["ItemQty"].ToString());
                    items.ItemPrice = double.Parse(ds.Tables[0].Rows[i]["ItemPrice"].ToString());
                    items.ItemUnitID = ds.Tables[0].Rows[i]["ItemUnitID"].ToString();
                    items.ItemUnit = ds.Tables[0].Rows[i]["ItemUnit"].ToString();
                    items.UOM = ds.Tables[0].Rows[i]["UOM"].ToString();
                    items.ItemOutOfStock = ds.Tables[0].Rows[i]["ItemOutOfStock"].ToString();
                    items.ItemDepartment = ds.Tables[0].Rows[i]["ItemDepartment"].ToString();
                    items.DepartmentName = ds.Tables[0].Rows[i]["DepartmentName"].ToString();
                    items.ItemSalesType = ds.Tables[0].Rows[i]["ItemSalesType"].ToString();

                    modifieritems.Add(items);
                }
            }

            m.ModifierItems = modifieritems;

            string json = JsonConvert.SerializeObject(m);
            return json;
        }
        catch (Exception E)
        {
            return E.Message.ToString();
        }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public byte[] generateQRCode(string companyCode, string retailid, int tableno, int width, int height)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        string linkurl = "select RetailWeb from Retailer where RetailID=@RetailID";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@RetailID", retailid)
        };
        string context = dataAccessor.ExecScalarVal_Vapt(linkurl, "", objparam).ToString();

        if (tableno > 0)
        {

            context += "\\" + tableno.ToString();
        }
        Bitmap result = null;
        try
        {
            BarcodeWriter barCodeWriter = new BarcodeWriter();
            barCodeWriter.Format = BarcodeFormat.QR_CODE;
            barCodeWriter.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            barCodeWriter.Options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.H);
            barCodeWriter.Options.Height = height;
            barCodeWriter.Options.Width = width;
            barCodeWriter.Options.Margin = 0;
            ZXing.Common.BitMatrix bm = barCodeWriter.Encode(context);
            result = barCodeWriter.Write(bm);

            using (MemoryStream stream = new MemoryStream())
            {
                result.Save(stream, ImageFormat.Jpeg);
                byte[] data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                return data;
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UploadImg(string companyCode, string retailid, string terminalid, string filepath)
    {
        try
        {
            int index = filepath.LastIndexOf('.');
            string suffix = filepath.Substring(index).ToLower();

            string serverfilename = "";
            if (suffix == ".jpg" || suffix == ".jpeg" || suffix == ".png" || suffix == ".gif" || suffix == ".bmp")
            {
                string pictureName = DateTime.Now.Ticks.ToString() + suffix;
                string savePath = Server.MapPath("/Files/Images/" + companyCode + "//" + retailid + "//" + terminalid + "//");
                Byte[] MeaningFile;
                FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                int size = Convert.ToInt32(stream.Length);
                MeaningFile = new Byte[size];
                stream.Read(MeaningFile, 0, size);
                stream.Close();
                FileStream fos = null;

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                fos = new FileStream(savePath + pictureName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                fos.Write(MeaningFile, 0, MeaningFile.Length);
                fos.Close();

                serverfilename = savePath + pictureName;
            }

            return serverfilename;
        }
        catch (Exception E)
        {
            return E.Message.ToString();
        }
    }
    

    /*********************** for ordering online system *********************/
    
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string AddCategories(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineCategories OnlineC = new OnlineCategories();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        /*  json = @"{
          ""cat_id"":""12122222"",
          ""cat_code"":""Testing181212"",
          ""cat_descp"":""test"", 
          ""cat_otherlanguage"":"""", 
          ""cat_createdate"":""2019-11-07"",
          }";
          
          passing value : 
            {"OnlineCategory":[{"cat_id":"1","cat_code":"Bird’s Nest & Snow Jelly","cat_descp":"Bird’s Nest & Snow Jelly",
    "cat_otherlanguage":"","cat_image":"","cat_display":"Y","cat_createby":"Y","cat_updateby":"Y",
    "cat_createdate":"2019-11-13", "cat_lastupdatetime":"2019-11-13"},
  {"cat_id":"1","cat_code":"Bird’s Nest ","cat_descp":"Bird’s Nest ",
    "cat_otherlanguage":"","cat_image":"","cat_display":"Y","cat_createby":"Y","cat_updateby":"Y",
    "cat_createdate":"2019-11-13", "cat_lastupdatetime":"2019-11-13"}]}
          */
        try
        {
            OnlineC = serializer.Deserialize<OnlineCategories>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveCategoriesData(OnlineC);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdateCategories(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineCategories OnlineC = new OnlineCategories();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        /*  json = @"{
          ""cat_id"":""12122222"",
          ""cat_code"":""Testing181212"",
          ""cat_descp"":""test"", 
          ""cat_otherlanguage"":"""", 
          ""cat_createdate"":""2019-11-07"",
          }";
          
          passing value : 
         {"OnlineCategory":[{"cat_id":"1","cat_code":"Bird’s Nest & Snow Jelly","cat_descp":"Bird’s Nest & Snow Jelly",
    "cat_otherlanguage":"","cat_image":"","cat_display":"Y","cat_createby":"Y","cat_updateby":"Y",
    "cat_createdate":"2019-11-13", "cat_lastupdatetime":"2019-11-13"},
  {"cat_id":"1","cat_code":"Bird’s Nest ","cat_descp":"Bird’s Nest ",
    "cat_otherlanguage":"","cat_image":"","cat_display":"Y","cat_createby":"Y","cat_updateby":"Y",
    "cat_createdate":"2019-11-13", "cat_lastupdatetime":"2019-11-13"}]}
          */
        try
        {
            OnlineC = serializer.Deserialize<OnlineCategories>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.updateCategoriesData(OnlineC);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string DeleteCategories(string companyCode, string category_id)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string response = dataAccessor.deleteCategoriesData(category_id);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getUOM(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = " SELECT ID,Nick as UOM FROM list_units WHERE RecordStatus<>'DELETED' " + 
                        " AND Display='Y' ORDER BY blnDefault DESC";

        DataSet inventoryDS = dataAccessor.RunSPRetDataset(strSql, "listunits");

        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CreateProduct(string companyCode,string Retailer, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineInventory OI = new OnlineInventory();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        /*  json = @"{
          ""cat_id"":""12122222"",
          ""cat_code"":""Testing181212"",
          ""cat_descp"":""test"", 
          ""cat_otherlanguage"":"""", 
          ""cat_createdate"":""2019-11-07"",
          }";
                    
           passing value :
           {
                "ItemID":"1ffbe71c-bc03-11ea-92ec-00155d01ca02","ItemSKU":"DNBS001",
                "ItemDescp":"Bird’s Nest & Snow Jelly","ItemRemark":"This item have contain...... ",
                "ItemUOM":"4f9fe82d-f8f3-11e7-95c5-68f7282584d2",
                "ItemOriPrice":"35.00","ItemPrice":"33.00","ItemCategory":"1f4e7f5d-61a5-11e9-937f-001917e116f0",
                "ItemImage":"49.128.60.174:8080/TASTY/Category/BSBN01.jpg","ItemDisplay":"Y",
                "CreateBy":"superuser","CreateTime":"2019-11-13","LastUpdateBy":"superuser","LastUpdateTime":"2019-11-13",

                  "ItemBOM":[{"ParentItemID":"1ffbe71c-bc03-11ea-92ec-00155d01ca02",
                            "ChildItemID":"db30747b-bc04-11ea-92ec-00155d01ca02","UOM":"4f9fe82d-f8f3-11e7-95c5-68f7282584d2",
                        "ChildItemQty":"1","ItemActQty":"1","RTLSellPx":"0","ItemModifier":"Y","ItemStatus":"OPTIONAL"},
          {"ParentItemID":"1ffbe71c-bc03-11ea-92ec-00155d01ca02",
                            "ChildItemID":"db30454b-bc04-11ea-92ec-00155d01ca02","UOM":"SET",
            "ChildItemQty":"1","ItemActQty":"1","RTLSellPx":"0","ItemModifier":"Y","ItemStatus":"OPTIONAL"}]}
                  

          */
        try
        {
            OI = serializer.Deserialize<OnlineInventory>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveProductData(OI, Retailer);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdateProduct(string companyCode, string Retailer, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineInventory OI = new OnlineInventory();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        /*  json = @"{
          ""cat_id"":""12122222"",
          ""cat_code"":""Testing181212"",
          ""cat_descp"":""test"", 
          ""cat_otherlanguage"":"""", 
          ""cat_createdate"":""2019-11-07"",
          }";
                    
           passing value :
           {
                "ItemID":"1ffbe71c-bc03-11ea-92ec-00155d01ca02","ItemSKU":"DNBS001",
                "ItemDescp":"Bird’s Nest & Snow Jelly","ItemRemark":"This item have contain...... ",
                "ItemUOM":"4f9fe82d-f8f3-11e7-95c5-68f7282584d2",
                "ItemOriPrice":"35.00","ItemPrice":"33.00","ItemCategory":"1f4e7f5d-61a5-11e9-937f-001917e116f0",
                "ItemImage":"49.128.60.174:8080/TASTY/Category/BSBN01.jpg","ItemDisplay":"Y",
                "CreateBy":"superuser","CreateTime":"2019-11-13","LastUpdateBy":"superuser","LastUpdateTime":"2019-11-13",

                  "ItemBOM":[{"ParentItemID":"1ffbe71c-bc03-11ea-92ec-00155d01ca02",
                            "ChildItemID":"db30747b-bc04-11ea-92ec-00155d01ca02","UOM":"4f9fe82d-f8f3-11e7-95c5-68f7282584d2",
                        "ChildItemQty":"1","ItemActQty":"1","RTLSellPx":"0","ItemModifier":"Y","ItemStatus":"OPTIONAL"},
          {"ParentItemID":"1ffbe71c-bc03-11ea-92ec-00155d01ca02",
                            "ChildItemID":"db30454b-bc04-11ea-92ec-00155d01ca02","UOM":"SET",
            "ChildItemQty":"1","ItemActQty":"1","RTLSellPx":"0","ItemModifier":"Y","ItemStatus":"OPTIONAL"}]}
                  

          */
        try
        {
            OI = serializer.Deserialize<OnlineInventory>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveProductData(OI, Retailer);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string DeleteProduct(string companyCode, string Retailer, string ItemID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string response = dataAccessor.deleteProductData(Retailer,ItemID);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetInventoryMenu(string companyCode, string RetailID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string fieldCriteria = "";
        string useretailid = "";
        if (RetailID != "" && RetailID != "0")
        {
            fieldCriteria = " AND Retailid=@RetailID";
            useretailid = RetailID;
        }
        else
        {
            string sqlstr = "SELECT RetailID FROM retailer WHERE RetailType='ONLINE' AND RecordStatus<>'DELETED'";
            DataTable DT = dataAccessor.GetData(string.Format(sqlstr));
            if (DT.Rows.Count != 0)
            {
                string strRetailID = Convert.ToString(DT.Rows[0]["RetailID"]);
                useretailid = strRetailID;
                fieldCriteria = " AND Retailid=@RetailID";
            }
            DT.Clear();
            DT.Dispose();
        }
        string strSql = "SELECT inventory_retail.ItemID AS ProductID, inventory.ItemSKU AS ProductCode, inventory.ItemDescp AS ProductName, " +
                    " inventory.ItemOther AS ProductDescription,inventory.ItemCategory AS CategoryID,inventory.ItemOtherLanguage AS ItemOtherLanguage," +
                    " list_units.Nick AS UOM, inventory_unit.RTLSellPx AS ProductDiscountPrice,inventory.ItemPicFront AS ProductImage," +
                    " IF(inventory_unit.eOriginalPx IS NULL,inventory_unit.RTLSellPx,inventory_unit.eOriginalPx) AS ProductOriginalPrice, " +
                    " inventory_retail.ItemTopMenu AS TopMenu,inventory_unit.ItemWeight,inventory_unit.ItemCustomWidth AS ItemWidth," +
                    " inventory_unit.ItemCustomDepth AS ItemDepth,inventory_unit.ItemCustomHeight AS ItemHeight,inventory_unit.ItemMeasurement," +
                    " inventory_unit.ItemBoxSize,inventory.ItemOther As OthersInfo,inventory.eItemRemarks AS ItemRemarks,inventory.eItemRemarks1 AS ItemRemarks2,inventory.eItemRemarks2 AS ItemRemarks3, " +
                    " inventory_retail.OnHandQty as OnHandQty  FROM inventory_retail LEFT JOIN inventory ON inventory_retail.ItemID = inventory.ItemID " +
                    " LEFT JOIN inventory_unit ON inventory_retail.ItemID = inventory_unit.ItemID AND inventory_unit.RecordStatus <> 'DELETED' " +
                    " LEFT JOIN list_units ON list_units.ID=inventory_unit.ItemUnit AND list_units.RecordStatus <> 'DELETED' " +
                    " LEFT JOIN list_inv_departments ON list_inv_departments.ID = inventory.ItemDepartment " +
                    " WHERE inventory_retail.RecordStatus <> 'DELETED'  AND inventory.ItemDisplay='Y' AND inventory.RecordStatus<>'DELETED' " +
                    " AND list_inv_departments.bitAddOn<>'Y' AND inventory.ItemSalesType<>'S' " + fieldCriteria + " ORDER BY inventory.ItemSKU ";

        MySqlParameter[] objparam =
        {
            new MySqlParameter("@RetailID", useretailid)
        };
        DataSet inventoryDS = dataAccessor.RunSPRetDataset_Vapt(strSql, "inventory", objparam);

        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);
        //List<string[]> asd = inventoryDS;

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetAllInventoryMenu(string companyCode, string RetailID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

		string useretailid = "";
        string fieldCriteria = "";
        if (RetailID != "" && RetailID != "0")
        {
            fieldCriteria = " AND Retailid=@RetailID";
            useretailid = RetailID;
        }
        else {
            string sqlstr = "SELECT RetailID FROM retailer WHERE RetailType='ONLINE' AND RecordStatus<>'DELETED'";
            DataTable DT = dataAccessor.GetData(string.Format(sqlstr));
            if (DT.Rows.Count != 0)
            {
                string strRetailID = Convert.ToString(DT.Rows[0]["RetailID"]);
                fieldCriteria = " AND Retailid=@RetailID";
                useretailid = strRetailID;
            }            
            DT.Clear();
            DT.Dispose();        
        }
        string strSql = "SELECT inventory_retail.ItemID AS ProductID, inventory.ItemSKU AS ProductCode, inventory.ItemDescp AS ProductName, " +
                    " inventory.ItemOther AS ProductDescription,inventory.ItemCategory AS CategoryID,inventory.ItemOtherLanguage AS ItemOtherLanguage," +
                    " list_units.Nick AS UOM, inventory_unit.RTLSellPx AS ProductDiscountPrice,inventory.ItemPicFront AS ProductImage," +
                    " IF(inventory_unit.eOriginalPx IS NULL,inventory_unit.RTLSellPx,inventory_unit.eOriginalPx) AS ProductOriginalPrice, " +
                    " inventory_retail.ItemTopMenu AS TopMenu,inventory_unit.ItemWeight,inventory_unit.ItemCustomWidth AS ItemWidth," +
                    " inventory_unit.ItemCustomDepth AS ItemDepth,inventory_unit.ItemCustomHeight AS ItemHeight,inventory_unit.ItemMeasurement," +
                    " inventory_unit.ItemBoxSize,inventory.ItemOther As OthersInfo, inventory.eItemRemarks AS ItemRemarks,inventory.eItemRemarks1 AS ItemRemarks2,inventory.eItemRemarks2 AS ItemRemarks3," +
                    " inventory_retail.OnHandQty as OnHandQty FROM inventory_retail LEFT JOIN inventory ON inventory_retail.ItemID = inventory.ItemID " +
                    " LEFT JOIN inventory_unit ON inventory_retail.ItemID = inventory_unit.ItemID AND inventory_unit.RecordStatus <> 'DELETED' " +
                    " LEFT JOIN list_units ON list_units.ID=inventory_unit.ItemUnit AND list_units.RecordStatus <> 'DELETED' " +
                    " LEFT JOIN list_inv_departments ON list_inv_departments.ID = inventory.ItemDepartment " +
                    " WHERE inventory_retail.RecordStatus <> 'DELETED'  AND inventory.ItemDisplay='Y' AND inventory.RecordStatus<>'DELETED' " +
                    " AND list_inv_departments.bitAddOn<>'Y' AND inventory.ItemSalesType<>'S' " + fieldCriteria + " ORDER BY inventory.ItemSKU";

        MySqlParameter[] objparam =
       {
            new MySqlParameter("@RetailID", useretailid)
        };

        DataSet inventoryDS = dataAccessor.RunSPRetDataset_Vapt(strSql, "inventory", objparam);
        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);
        //List<string[]> asd = inventoryDS;

        //return RetailID;
        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetInventoryMenu1(string companyCode, string RetailID, int limit)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string fieldCriteria = "";
        if (RetailID != "" && RetailID != "0")
        {
            fieldCriteria = " AND Retailid=@RetailID";
        }
        string strSql = "SELECT inventory_retail.ItemID AS ProductID, inventory.ItemSKU AS ProductCode, inventory.ItemDescp AS ProductName, " +
                    " inventory.ItemOther AS ProductDescription,inventory.ItemCategory AS CategoryID,inventory.ItemOtherLanguage AS ItemOtherLanguage," +
                    " list_units.Nick AS UOM, inventory_unit.RTLSellPx AS ProductDiscountPrice,inventory.ItemPicFront AS ProductImage," +
                    " IF(inventory_unit.eOriginalPx IS NULL,inventory_unit.RTLSellPx,inventory_unit.eOriginalPx) AS ProductOriginalPrice, " +
                    " inventory_retail.ItemTopMenu AS TopMenu,inventory_unit.ItemWeight,inventory_unit.ItemCustomWidth AS ItemWidth," +
                    " inventory_unit.ItemCustomDepth AS ItemDepth,inventory_unit.ItemCustomHeight AS ItemHeight,inventory_unit.ItemMeasurement," +
                    " inventory_unit.ItemBoxSize,inventory.ItemOther As OthersInfo,inventory.eItemRemarks AS ItemRemarks,inventory.eItemRemarks1 AS ItemRemarks2,inventory.eItemRemarks2 AS ItemRemarks3, " +
                    " inventory_retail.OnHandQty as OnHandQty FROM inventory_retail LEFT JOIN inventory ON inventory_retail.ItemID = inventory.ItemID " +
                    " LEFT JOIN inventory_unit ON inventory_retail.ItemID = inventory_unit.ItemID AND inventory_unit.RecordStatus <> 'DELETED' " +
                    " LEFT JOIN list_units ON list_units.ID=inventory_unit.ItemUnit AND list_units.RecordStatus <> 'DELETED' " +
                    " LEFT JOIN list_inv_departments ON list_inv_departments.ID = inventory.ItemDepartment " +
                    " WHERE inventory_retail.RecordStatus <> 'DELETED'  AND inventory.ItemDisplay='Y' AND inventory.RecordStatus<>'DELETED' " +
                    " AND list_inv_departments.bitAddOn<>'Y' AND inventory.ItemSalesType<>'S' " + fieldCriteria + " ORDER BY inventory.ItemSKU";

        if (limit > 0)
        {
            strSql += " LIMIT " + limit;
        }

        MySqlParameter[] objparam =
        {
            new MySqlParameter("@RetailID", RetailID)
        };

        DataSet inventoryDS = dataAccessor.RunSPRetDataset_Vapt(strSql, "inventory", objparam);

        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);
        //List<string[]> asd = inventoryDS;

        //return asd;
        return json;
    }


    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetCategoryMenu(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT ID AS CategoryID,Nick as CategoryCode,IF(ISNULL(FULL) OR FULL='',VALUE,FULL) AS CategoryName,CatImage AS CategoryImage, " +
                        "OtherLanguage FROM list_inv_categories WHERE Display='Y' AND RecordStatus<>'DELETED'";

        DataSet CatDS = dataAccessor.RunSPRetDataset(strSql, "categories");

        string json = JsonConvert.SerializeObject(CatDS, Formatting.Indented);

        return json;
    }
    
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetOrderingSetting(string companyCode,string RetailID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT ReceiptPrefix,NextReceiptNo,Merchant_ID AS MerchantID,SecretKey, " +
                        "IsIntegrateDelivery,IntergrateDeliveryAgency,DeliveryAccount AS DeliveryClientId,DeliveryPassword AS DeliveryClientSecret " +
                        " FROM definitions_terminal WHERE RetailerID=@RetailID AND RecordStatus<>'DELETED' " +
                        " AND TerminalID=1 LIMIT 1";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@RetailID", RetailID)
        };
        DataSet DS = dataAccessor.RunSPRetDataset_Vapt(strSql, "definitions", objparam);

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        string response = dataAccessor.updateOrderingReceiptNo(RetailID);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetOnlineCompanyInfor(string companyCode, string RetailCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        
        string strSql="";
        if (dataAccessor.connectionstring != "")
        {
            if (RetailCode == "" || RetailCode == "0")
            {
                strSql = "SELECT CompanyName,CompanyName_OtherLanguage," +
                            "CompanyAdd1,CompanyAdd2,CompanyAdd3,CompanyPost,CompanyTel,CompanyFax,CompanyRegNo," +
                            "CompanyTaxReg,ImgURL,CompanyLogoURL,QRCodeURL,vchRemarks AS Notes, " +
                            "(SELECT RetailID FROM retailer WHERE RetailType='ONLINE' AND RecordStatus<>'DELETED') AS RetailID, " +
                            "(SELECT RetailOnlineType FROM retailer WHERE RetailType='ONLINE' AND RecordStatus<>'DELETED') AS RetailType " +
                            " FROM definitions ";
            }
            else
            {
                strSql = "SELECT CompanyName,CompanyName_OtherLanguage," +
                            "CompanyAdd1,CompanyAdd2,CompanyAdd3,CompanyPost,CompanyTel,CompanyFax,CompanyRegNo," +
                            "CompanyTaxReg,ImgURL,CompanyLogoURL,QRCodeURL,vchRemarks AS Notes, " +
                            "(SELECT RetailID FROM retailer WHERE RetailCode=@RetailCode AND RecordStatus<>'DELETED') AS RetailID, " +
                            "(SELECT RetailType FROM retailer WHERE RetailCode=@RetailCode AND RecordStatus<>'DELETED') AS RetailType " +
                            " FROM definitions ";
            }
            MySqlParameter[] objparam =
        {
            new MySqlParameter("@RetailCode", RetailCode)
        };

            DataSet DS = dataAccessor.RunSPRetDataset_Vapt(strSql, "definitions", objparam);

            string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

            return json;
        }
        else {
            return new JavaScriptSerializer().Serialize(new
            {
                Status = "INVALID",
                CompanyCode = companyCode,
                Message = "The Website is under maintenance. Please come back tomorrow. Sorry for any Inconvinience cause."
            });
        }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdateOrderingSetting(string companyCode, string RetailID, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineDefinitions OD = new OnlineDefinitions();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        try
        {
            OD = serializer.Deserialize<OnlineDefinitions>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.SaveDefinitionOnlineSetting(OD, RetailID);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CreateOnlineUser(string companyCode, string Retailer, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineUser OUser = new OnlineUser();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        /*  json = @"{
          ""user_id"":""12122222"",
          ""user_code"":""Testing181212"",
          ""user_name"":""test"", 
          ""user_password"":"""", 
          ""cat_createdate"":""2019-11-07"",
          }";
                    
           passing value :
           {
                "user_id":"1ffbe71c-bc03-11ea-92ec-00155d01ca02","user_code":"cashier",
                "user_name":"julie chong","user_password":"111221",
                "user_display":"Y",
                "user_createby":"superuser","user_createdate":"2019-11-13","user_updateby":"superuser","user_lastupdatetime":"2019-11-13",
            }

          */
        try
        {
            OUser = serializer.Deserialize<OnlineUser>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveOnLineUser(OUser, Retailer);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdateOnlineUser(string companyCode, string Retailer, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineUser OUser = new OnlineUser();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        /*  json = @"{
          ""user_id"":""12122222"",
          ""user_code"":""Testing181212"",
          ""user_name"":""test"", 
          ""user_password"":"""", 
          ""cat_createdate"":""2019-11-07"",
          }";
                    
           passing value :
           {
                "user_id":"1ffbe71c-bc03-11ea-92ec-00155d01ca02","user_code":"cashier",
                "user_name":"julie chong","user_password":"111221",
                "user_display":"Y",
                "user_createby":"superuser","user_createdate":"2019-11-13","user_updateby":"superuser","user_lastupdatetime":"2019-11-13",
            }

          */
        try
        {
            OUser = serializer.Deserialize<OnlineUser>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveOnLineUser(OUser, Retailer);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string DeleteOnlineUser(string companyCode, string userid)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string response = dataAccessor.deleteOnLineUser(userid);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetOnlineUser(string companyCode, string usercode, string weburl)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT ID AS user_id,USER AS user_code,PASSWORD AS user_password,email AS user_email,UsersFirstName AS user_name," +
                        "hph AS ContactNo,Display AS user_display,@CompanyCode AS companyCode, " +
                        "(SELECT RetailID FROM retailer WHERE RecordStatus<>'DELETED' AND RetailWeb<>'' AND RetailType='ONLINE') AS RetailID " +
                        " FROM users WHERE RecordStatus<>'DELETED' ";

        if (usercode != "")
        {
            strSql += " AND USER=@UserCode";
        }

        MySqlParameter[] objparam =
        {
            new MySqlParameter("@CompanyCode", companyCode),
            new MySqlParameter("@UserCode", usercode)
        };

        DataSet DS = dataAccessor.RunSPRetDataset_Vapt(strSql, "users", objparam);

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetDeliveryScheduler(string companyCode, string RetailID, string SchedulerType)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string sql_retail = "SELECT RetailType from retailer where RetailID=@RetailID";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@RetailID", RetailID)
        };

        DataSet ds_retail = dataAccessor.RunSPRetDataset_Vapt(sql_retail, "Retailer", objparam);

        string strRetailType = "";
        if (ds_retail.Tables[0].Rows.Count != 0)
        {
            strRetailType = Convert.ToString(ds_retail.Tables[0].Rows[0]["RetailType"]);
        }
        ds_retail.Clear();

        string strRetailID = "";
        if (strRetailType == "ONLINE")
        {
            strRetailID = "0";
        }
        else {
            strRetailID = RetailID.ToString();
        }
        string strSql = "SELECT definitions_deliveryscheduler.ID AS ScheduleID,definitions_deliveryscheduler.RetailerID," +
                        "(SELECT RetailCode FROM retailer WHERE RetailID =definitions_deliveryscheduler.RetailerID) AS RetailCode ," +
                        "definitions_deliveryscheduler.ScheduleType,OperateDayName,OperateTimeStart,OperateTimeClose,TimeSlotStart,TimeSlotClose,IntervalTime," +
                        "DeliveryMinValueOrder AS MinValueOrder,DeliveryMaxValueOrder AS MaxValueOrder ,DeliveryFee,IF(Active='Y','OPEN','CLOSE') AS IsActive ," +
                        "definitions_terminal.IsCustomDeliveryFee,definitions_terminal.IsOnlinePay,definitions_terminal.IsIntegrateDelivery,definitions_terminal.IntergrateDeliveryAgency," +
                        "definitions_terminal.DeliveryAccount AS DeliveryClientId,definitions_terminal.DeliveryPassword AS DeliveryClientSecret " +
                        " FROM definitions_deliveryscheduler INNER JOIN definitions_terminal ON definitions_terminal.RetailerID = definitions_deliveryscheduler.RetailerID " +
                        " WHERE definitions_deliveryscheduler.RetailerID=@RetailerID AND ScheduleType=@ScheduleType AND definitions_deliveryscheduler.RecordStatus<>'DELETED' " +
                        " AND definitions_terminal.TerminalID=1 ORDER BY SeqNo ";
        MySqlParameter[] objparam1 =
        {
            new MySqlParameter("@RetailerID", strRetailID),
            new MySqlParameter("@ScheduleType", SchedulerType)
        };

        DataSet DS = dataAccessor.RunSPRetDataset_Vapt(strSql, "definitions", objparam1);

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetDeliverySchedulerAll(string companyCode, string SchedulerType)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT definitions_deliveryscheduler.ID AS ScheduleID,definitions_deliveryscheduler.RetailerID," +
                        "(SELECT RetailCode FROM retailer WHERE RetailID =definitions_deliveryscheduler.RetailerID) AS RetailCode ," +
                        "definitions_deliveryscheduler.ScheduleType,OperateDayName,OperateTimeStart,OperateTimeClose,TimeSlotStart,TimeSlotClose,IntervalTime," +
                        "DeliveryMinValueOrder AS MinValueOrder,DeliveryMaxValueOrder AS MaxValueOrder ,DeliveryFee,IF(Active='Y','OPEN','CLOSE') AS IsActive ," +
                        "definitions_terminal.IsCustomDeliveryFee,definitions_terminal.IsOnlinePay,definitions_terminal.IsIntegrateDelivery,definitions_terminal.IntergrateDeliveryAgency," +
                        "definitions_terminal.DeliveryAccount AS DeliveryClientId,definitions_terminal.DeliveryPassword AS DeliveryClientSecret  " +
                        "FROM definitions_deliveryscheduler INNER JOIN definitions_terminal ON definitions_terminal.RetailerID = definitions_deliveryscheduler.RetailerID " +
                        "WHERE definitions_deliveryscheduler.RetailerID<>0 AND ScheduleType=@ScheduleType " +
                        "AND definitions_deliveryscheduler.RecordStatus<>'DELETED' " +
                        "AND definitions_terminal.TerminalID=1 ORDER BY definitions_deliveryscheduler.RetailerID, " +
                        "definitions_deliveryscheduler.ScheduleType,definitions_deliveryscheduler.SeqNo";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@ScheduleType", SchedulerType)
        };
        DataSet DS = dataAccessor.RunSPRetDataset_Vapt(strSql, "definitions", objparam);

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CreateDeliverySchedule(string companyCode, string Retailer, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineDeliveryInfor ODS = new OnlineDeliveryInfor();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        /*  json = @"{
          ""user_id"":""12122222"",
          ""user_code"":""Testing181212"",
          ""user_name"":""test"", 
          ""user_password"":"""", 
          ""cat_createdate"":""2019-11-07"",
          }";
                    
           passing value :
           {
                "user_id":"1ffbe71c-bc03-11ea-92ec-00155d01ca02","user_code":"cashier",
                "user_name":"julie chong","user_password":"111221",
                "user_display":"Y",
                "user_createby":"superuser","user_createdate":"2019-11-13","user_updateby":"superuser","user_lastupdatetime":"2019-11-13",
            }

          */
        try
        {
            ODS = serializer.Deserialize<OnlineDeliveryInfor>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveDeliverySchedule(ODS, Retailer);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdateDeliverySchedule(string companyCode, string Retailer, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineDeliveryInfor ODS = new OnlineDeliveryInfor();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        /*  json = @"{
          ""user_id"":""12122222"",
          ""user_code"":""Testing181212"",
          ""user_name"":""test"", 
          ""user_password"":"""", 
          ""cat_createdate"":""2019-11-07"",
          }";
                    
           passing value :
           {
                "user_id":"1ffbe71c-bc03-11ea-92ec-00155d01ca02","user_code":"cashier",
                "user_name":"julie chong","user_password":"111221",
                "user_display":"Y",
                "user_createby":"superuser","user_createdate":"2019-11-13","user_updateby":"superuser","user_lastupdatetime":"2019-11-13",
            }

          */
        try
        {
            ODS = serializer.Deserialize<OnlineDeliveryInfor>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveDeliverySchedule(ODS, Retailer);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetRetailerLocation(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT RetailID,RetailCode,RetailBranch,RetailAddr,RetailTel,RetailFax,RetailType,RetailOnlineType FROM retailer WHERE RetailType<>'ONLINE' AND RecordStatus<>'DELETED'";

        DataSet DS = dataAccessor.RunSPRetDataset(strSql, "retailer");

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetAllRetailer(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT RetailID,RetailCode,RetailBranch,RetailAddr,RetailTel,RetailFax,RetailType,RetailOnlineType FROM retailer WHERE RecordStatus<>'DELETED'";

        DataSet DS = dataAccessor.RunSPRetDataset(strSql, "retailer");

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        return json;
    }


    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetWebPageDesign(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT ID,FrameID,CompanyLogoURL AS LogoImg,TopBannerBackColor AS TopBannerBackColor,vchBackColor AS BackgroundCode,vchTitle AS PageTitle,vchContent AS PageContent," +
                        "PageImage FROM webdesignsetting WHERE Display='Y' AND RecordStatus<>'DELETED'";

        DataSet DS = dataAccessor.RunSPRetDataset(strSql, "webdesign");

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CreateWebPageDesign(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineWebDesignInfor WD = new OnlineWebDesignInfor();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        /*  json = @"{
          ""user_id"":""12122222"",
          ""user_code"":""Testing181212"",
          ""user_name"":""test"", 
          ""user_password"":"""", 
          ""cat_createdate"":""2019-11-07"",
          }";
                    
           passing value :
           {
                "user_id":"1ffbe71c-bc03-11ea-92ec-00155d01ca02","user_code":"cashier",
                "user_name":"julie chong","user_password":"111221",
                "user_display":"Y",
                "user_createby":"superuser","user_createdate":"2019-11-13","user_updateby":"superuser","user_lastupdatetime":"2019-11-13",
            }

          */
        try
        {
            WD = serializer.Deserialize<OnlineWebDesignInfor>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveWebPageDesign(WD);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdateWebPageDesign(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        OnlineWebDesignInfor WD = new OnlineWebDesignInfor();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        /*  json = @"{
          ""user_id"":""12122222"",
          ""user_code"":""Testing181212"",
          ""user_name"":""test"", 
          ""user_password"":"""", 
          ""cat_createdate"":""2019-11-07"",
          }";
                    
           passing value :
           {
                "user_id":"1ffbe71c-bc03-11ea-92ec-00155d01ca02","user_code":"cashier",
                "user_name":"julie chong","user_password":"111221",
                "user_display":"Y",
                "user_createby":"superuser","user_createdate":"2019-11-13","user_updateby":"superuser","user_lastupdatetime":"2019-11-13",
            }

          */
        try
        {
            WD = serializer.Deserialize<OnlineWebDesignInfor>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveWebPageDesign(WD);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string DeleteWebPageDesign(string companyCode, string FrameID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string response = dataAccessor.deleteWebPageDesign(FrameID);
        return response;
    }
	
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetSalesReport(string companyCode, string RetailID, string DateRangeFrom, string DateRangeTo,
         string category, string department, string transno, string reporttype)
    {
        DataSet ds = new DataSet();
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string Itemcond = " ";
        string Receiptcond = " 1 = 1";

        if (category.Length > 0)
        {
            Itemcond += " And Inventory.ItemCategory=@Category ";
        }

        if (department.Length > 0)
        {
            Itemcond += " And Inventory.ItemDepartment=@Department ";
        }

        if (RetailID.Length > 0)
            Receiptcond += " And retail_sales.RetailID=@RetailID";

        if (DateRangeFrom.Length > 0)
            Receiptcond += " And retail_sales.SalesDate>=@DateFrom";

        if (DateRangeTo.Length > 0)
            Receiptcond += " And retail_sales.SalesDate<=@DateTo";

        if (transno.Length > 0)
            Receiptcond += " And retail_sales.SalesNo=@TransNo";

        string sql = ""; 
        if (reporttype == "Detail")
        {
            sql = "SELECT (@cnt:= @cnt + 1) AS SNO, IF(RetailerID = '0', '0', IF(RetailerID IS NULL, '0', RetailerID)) AS RetailerID, IF(RetailerID = '0', 'Head Office/Warehouse', IF(RetailerID IS NULL, 'Head Office/Warehouse', Retailer)) AS Retailer,";
            sql += "Retail_SalesID, Retail_SalesDate, CreateTime, Retail_strPayment,Retail_SalesStatus, Retail_SalesNo, ItemID, ItemSKU, ItemDescp, ItemUnitPrice, UOM,SUM(ItemQty) AS ItemQty, SUM(ItemGSwoGST) AS ItemGSwoGST, SUM(ItemDisc) AS ItemDisc,";
            sql += "SUM(ItemGroupDisc) AS ItemGroupDisc,SUM(ItemTotalDisc) AS ItemTotalDisc,SUM(ItemGSwoGSTwoDisc) AS ItemGSwoGSTwoDisc,SUM(ItemTaxTotal) AS ItemTaxTotal,SUM(ItemNetSales) AS ItemNetSales, SUM(ItemTotalCost) AS ItemTotalCost,";
            sql += "SalesRounding AS RoundAmt, BR.SubRoundAmt,C.RoundAmt AS TotalRoundAmt,C.GrandTotalNS FROM (SELECT retailer.RetailID AS RetailerID, CONCAT(retailer.RetailCode, ' - ', retailer.RetailName) AS Retailer, retail_sales_detail.SalesID AS Retail_SalesID,";
            sql += " retail_sales.SalesDate AS Retail_SalesDate,DATE_FORMAT(retail_sales.CreateTime, '%h:%i %p') AS CreateTime, (SELECT GROUP_CONCAT(DISTINCT Nick ORDER BY Nick ASC SEPARATOR ',') AS strPayment FROM retail_sales_payment LEFT JOIN ";
            sql += " list_paymentmethods ON retail_sales_payment.PaymentID = list_paymentmethods.ID WHERE  SalesID = retail_sales_detail.SalesID AND RetailID = retail_sales_detail.RetailID) AS Retail_strPayment, retail_sales.SalesStatus AS Retail_SalesStatus,";
            sql += " retail_sales.SalesNo AS Retail_SalesNo, retail_sales_detail.ItemID AS ItemID, inventory.ItemSKU AS ItemSKU, inventory.ItemDescp AS ItemDescp, retail_sales_detail.ItemUnitPrice AS ItemUnitPrice,  retail_sales_detail.ItemUOMDesc AS UOM, SUM(retail_sales_detail.ItemQty) AS ItemQty,";
            sql += " SUM(IF(retail_sales_detail.ItemtaxType = 'I', ((retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty) - retail_sales_detail.ItemTaxTotal), (retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty))) AS ItemGSwoGST, SUM(retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty - retail_sales_detail.ItemTotal) AS ItemDisc,";
            sql += " SUM(retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1) AS ItemGroupDisc, SUM(ROUND(ROUND(ROUND(retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty - retail_sales_detail.ItemTotal + retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + (retail_sales_detail.MemberAmt * -1) + retail_sales_detail.CardAmt, 4), 3), 2)) AS ItemTotalDisc,";
            sql += " SUM(IF(retail_sales_detail.ItemtaxType = 'I', ((retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty) - ((retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty - retail_sales_detail.ItemTotal + retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt)) - retail_sales_detail.ItemTaxTotal),";
            sql += " (retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty) - ((retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty - retail_sales_detail.ItemTotal + retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt))";
            sql += " )) AS ItemGSwoGSTwoDisc,SUM(retail_sales_detail.ItemTotal - ((retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt))) AS ItemNetSales,";
            sql += " SUM(retail_sales_detail.ItemTaxTotal) AS ItemTaxTotal, SUM(retail_sales_detail.ItemUnitCost * retail_sales_detail.ItemQty) AS ItemTotalCost, retail_sales.SalesRounding AS SalesRounding FROM retail_sales_detail LEFT OUTER JOIN retail_sales ON retail_sales_detail.SalesID = retail_sales.SalesID ";
            sql += " LEFT OUTER JOIN retailer ON retail_sales_detail.RetailID = retailer.RetailID LEFT JOIN inventory ON inventory.ItemID = retail_sales_detail.ItemID WHERE(retail_sales_detail.ItemID <> '0' AND retail_sales_detail.ItemID <> '' AND(retail_sales_detail.PromoDiscTypeCode = '0' OR retail_sales_detail.PromoDiscTypeCode = '') ";
            sql += Itemcond + " and " + Receiptcond + ") GROUP BY RetailerID , Retail_SalesID , Retail_SalesNo , Retail_SalesDate , ItemID , ItemUnitPrice , UOM ORDER BY RetailerID, Retail_SalesNo, Retail_SalesDate, CreateTime ASC ) AS grptbl ";
            sql += "LEFT JOIN (SELECT SUM(SalesRounding) AS SubRoundAmt, RetailID FROM retail_sales WHERE " + Receiptcond + " GROUP BY RetailID) BR ON BR.RetailID = grptbl.RetailerID CROSS JOIN (SELECT SUM(SalesRounding) AS RoundAmt, SUM(SalesBalTtl) AS GrandTotalNS";
            sql += " FROM retail_sales WHERE " + Receiptcond + ") C CROSS JOIN (SELECT @cnt:= 0) AS dummy GROUP BY RetailerID , Retail_SalesID , Retail_SalesNo , Retail_SalesDate,ItemID , ItemUnitPrice,UOM ORDER BY SNO";
        }
        else
        {
            sql = "SELECT (@cnt:=@cnt + 1) AS SNO,IF(RetailerID='0','0', IF(RetailerID IS NULL,'0',RetailerID)) AS RetailerID,IF(RetailerID='0','Head Office/Warehouse', IF(RetailerID IS NULL,'Head Office/Warehouse',Retailer)) AS Retailer,IF(ShiftCode IS NULL, '', ShiftCode) AS ShiftCode,  TerminalID, ItemID, ItemSKU, ItemDescp, UOM,";
            sql += " SUM(ItemQty) AS ItemQty,  SUM(ItemDisc) AS ItemDisc,  SUM(ItemGroupDisc) AS ItemGroupDisc,  SUM(TotalDisc) AS TotalDisc,  SUM(GSwoGST) AS GSwoGST,  SUM(GSwoGSTwoDisc) AS GSwoGSTwoDisc,  SUM(ItemTaxTotal) AS ItemTaxTotal,  SUM(NetSales) AS NetSales,  SUM(TotalCost) AS TotalCost,  SUM(GM) AS GM,  ROUND(ROUND(ROUND(((GM / GSwoGSTwoDisc) * 100), 4), 3), 2) AS GMPerc,";
            sql += " SalesRounding AS RoundAmt, BR.SubRoundAmt,C.RoundAmt AS TotalRoundAmt,C.GrandTotalNS FROM (SELECT retailer.RetailID AS RetailerID,CONCAT(retailer.RetailCode, ' - ', retailer.RetailName) AS Retailer,retail_sales.ZReadNo AS ShiftCode,retail_sales_detail.TerminalID AS TerminalID,retail_sales_detail.ItemID,inventory.ItemSKU AS ItemSKU,inventory.ItemDescp AS ItemDescp,";
            sql += "retail_sales_detail.ItemUOMDesc AS UOM,SUM(retail_sales_detail.ItemQty) AS ItemQty,SUM(retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty - retail_sales_detail.ItemTotal) AS ItemDisc,SUM(retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1) AS ItemGroupDisc,";
            sql += "SUM(retail_sales_detail.MemberAmt * -1) AS MemberDisc,    SUM(retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty - retail_sales_detail.ItemTotal + retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt) AS TotalDisc,";
            sql += "SUM(IF(retail_sales_detail.ItemtaxType = 'I', ((retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty) - retail_sales_detail.ItemTaxTotal), (retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty))) AS GSwoGST, SUM(IF(retail_sales_detail.ItemtaxType = 'I', ((retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty) - ((retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty - retail_sales_detail.ItemTotal + retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt)) - retail_sales_detail.ItemTaxTotal),";
            sql += "(retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty) - ((retail_sales_detail.ItemUnitPrice * retail_sales_detail.ItemQty - retail_sales_detail.ItemTotal + retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt)) )) AS GSwoGSTwoDisc,SUM(retail_sales_detail.ItemTaxTotal) AS ItemTaxTotal,";
            sql += "SUM(retail_sales_detail.ItemTotal - ((retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt))) AS NetSales,SUM(retail_sales_detail.ItemUnitCost * retail_sales_detail.ItemQty) AS TotalCost, retail_sales.SalesRounding AS SalesRounding,SUM((retail_sales_detail.ItemTotal - ((retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt))) - (retail_sales_detail.ItemUnitCost * retail_sales_detail.ItemQty)) AS GM,";
            sql += "ROUND(ROUND(ROUND(CASE(retail_sales_detail.ItemTotal - ((retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt))) WHEN 0 THEN 0 ELSE((SUM((retail_sales_detail.ItemTotal - ((retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt))) - (retail_sales_detail.ItemUnitCost * retail_sales_detail.ItemQty)) / SUM(retail_sales_detail.ItemTotal - ((retail_sales_detail.ItemDiscGroupAmt + (retail_sales_detail.GroupDiscAmt + retail_sales_detail.GroupDiscAmt2 + retail_sales_detail.GroupDiscAmt3) * -1 + retail_sales_detail.MemberAmt * -1 + retail_sales_detail.CardAmt)))) * 100)";
            sql += "END, 4), 3), 2) AS GMPerc FROM retail_sales_detail LEFT OUTER JOIN retail_sales ON retail_sales_detail.SalesID = retail_sales.SalesID AND retail_sales_detail.RetailID = retail_sales.RetailID AND retail_sales.TerminalID = retail_sales_detail.TerminalID LEFT OUTER JOIN retailer ON retail_sales_detail.RetailID = retailer.RetailID LEFT OUTER JOIN inventory ON retail_sales_detail.ItemID = inventory.ItemID LEFT OUTER JOIN company ON retail_sales_detail.SupplierID = company.CompanyID LEFT OUTER JOIN artcode ON inventory.ArtCodeID = artcode.ArtCodeID";
            sql += " LEFT OUTER JOIN list_inv_categories ON inventory.ItemCategory = list_inv_categories.ID LEFT OUTER JOIN list_inv_departments ON inventory.ItemDepartment = list_inv_departments.ID WHERE (retail_sales_detail.ItemID <> '0' AND retail_sales_detail.ItemID <> '' AND(retail_sales_detail.PromoDiscTypeCode = '0' OR retail_sales_detail.PromoDiscTypeCode = '')" + Itemcond + " and " + Receiptcond + ") GROUP BY RetailerID , ShiftCode , TerminalID , ItemID , UOM ORDER BY RetailerID ASC , ShiftCode ASC, TerminalID ASC , ItemSKU ASC) AS grptbl";
            sql += " LEFT JOIN (SELECT SUM(SalesRounding) AS SubRoundAmt, RetailID FROM retail_sales WHERE " + Receiptcond + " GROUP BY RetailID) BR ON BR.RetailID = grptbl.RetailerID CROSS JOIN (SELECT SUM(SalesRounding) AS RoundAmt, SUM(SalesBalTtl) AS GrandTotalNS FROM retail_sales WHERE " + Receiptcond + ") C CROSS JOIN (SELECT @cnt:= 0) AS dummy GROUP BY RetailerID , ShiftCode , TerminalID , ItemID , UOM ORDER BY SNO ";
        }

        if (sql.Length > 0)
        {
            MySqlParameter[] objprama =
            {
                new MySqlParameter("@Category", category),
                new MySqlParameter("@Department", department),
                new MySqlParameter("@RetailID", RetailID),
                new MySqlParameter("@DateFrom", DateRangeFrom),
                new MySqlParameter("@DateTo", DateRangeTo),
                new MySqlParameter("@TransNo", transno)
            };

            ds = dataAccessor.RunSPRetDataset_Vapt(sql, "retail_sales", objprama);
        }

        string json = JsonConvert.SerializeObject(ds, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetPaymentReport(string companyCode, string RetailID, string DateRangeFrom, string DateRangeTo, string transno, string reporttype)
    {
        DataSet ds = new DataSet();
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string Datecond = " 1 = 1";
        string Cond = " 1 = 1 ";

        if (RetailID.Length > 0)
            Cond += " And T.RetailID=@RetailID";

        if (DateRangeFrom.Length > 0)
        {
            Datecond += " And SalesDate>=@DateFrom";
            Cond += " And T.TransDate>=@DateFrom";
        }

        if (DateRangeTo.Length > 0)
        {
            Datecond += " And SalesDate<=@DateTo";
            Cond += " And T.TransDate<=@DateTo";
        }

        if (transno.Length > 0)
            Cond += " And T.TransNo=@TransNo";

        string sql = "";

        if (reporttype == "Date")
        {
            sql = "SELECT id, IF(RetailID='0','0', IF(RetailID IS NULL,'0',RetailID)) AS RetailID,IF(RetailID='0','HQ/WH', IF(RetailID IS NULL,'HQ/WH',RetailCode)) AS RetailCode,IF(RetailID='0','Head Office/Warehouse', IF(RetailID IS NULL,'Head Office/Warehouse',Retailer)) AS Retailer,";
            sql += "DATE_FORMAT(TransDate, '%d-%m-%Y') AS TransDate, strPayment, SUM(SalesAmount) AS SalesAmount, SUM(BalanceAmount) AS Total_Amount, SUM(PayAmount) AS Pay_Amount,SUM(ChangeAmount) AS Change_Amount, SUM(DepositAmount) AS Deposit_Amount, TerminalID, SUM(TaxAmount) AS TaxAmount, SUM(RoundAmount) AS RoundAmount FROM ";
            sql += "(SELECT CONCAT(retailer.RetailID, TP.TerminalID) AS id, retailer.RetailID AS RetailID, retailer.RetailCode, retailer.RetailName AS Retailer,T.TransID, T.TransNo, T.TransDate, T.CreateTime, T.SalesStatus AS TransType, T.SalesPerson, TP.SalesBalTtl AS SalesAmount, TP.PaymentID, P.Nick AS strPayment, SUM(IT.ItemUnitPrice * IT.ItemQty) AS GrossSales,";
            sql += "SUM(IT.ItemUnitPrice * IT.ItemQty - IT.ItemTotal + IT.ItemDiscGroupAmt + ((IT.GroupDiscAmt + IT.GroupDiscAmt2 + IT.GroupDiscAmt3) * -1) + (IT.MemberAmt * -1) + IT.CardAmt) AS TotalDisc, T.SalesSubTtl AS SubTotal, TP.SalesBalTtl AS BalanceAmount, TP.SalesPayTtl AS PayAmount, CASE WHEN P.Nick = 'CASH' THEN IF(TP.ChangeAmount = 0, '', CONCAT('Change :', SUM(TP.ChangeAmount))) ELSE '' END AS ChangeAmount, TP.SalesDeposit AS DepositAmount,";
            sql += "TP.TerminalID AS TerminalID, T.SalesTaxTtl AS TaxAmount, T.SalesRounding AS RoundAmount FROM(SELECT SalesID AS TransID, RetailID, SalesDate AS TransDate,CONCAT(DATE_FORMAT(CreateTime, '%h:%i %p')) AS CreateTime, SalesStatus, SalesNo AS TransNo, SalesPerson, SalesSubTtl, SalesTaxTtl, SalesRounding, TerminalID AS TerminalID1 FROM retail_sales WHERE " + Datecond + ") T ";
            sql += "LEFT JOIN retail_sales_payment TP ON TP.SalesID = T.TransID LEFT OUTER JOIN retail_sales_detail IT ON IT.SalesID = T.TransID LEFT OUTER JOIN retailer ON T.RetailID = retailer.RetailID LEFT JOIN list_paymentmethods P ON TP.PaymentID = P.ID WHERE ( " + Cond;
            sql += ") GROUP BY id, RetailID, Retailer, TerminalID, PaymentID, TransID, T.TransDate) AS pivottbl GROUP BY id, RetailID, Retailer, TerminalID, TransDate, strPayment";

        }
        else
        {
            sql = "SELECT id,IF(RetailID='0','0', IF(RetailID IS NULL,'0',RetailID)) AS RetailID,IF(RetailID='0','HQ/WH', IF(RetailID IS NULL,'HQ/WH',RetailCode)) AS RetailCode,IF(RetailID = '0', 'Head Office/Warehouse', IF(RetailID IS NULL, 'Head Office/Warehouse', Retailer)) AS Retailer, TransID, TransNo, SUM(SalesAmount) AS SalesAmount, strPayment, SUM(BalanceAmount) AS Total_Amount, ";
            sql += " SUM(PayAmount) AS Pay_Amount, SUM(ChangeAmount) AS Change_Amount, SUM(DepositAmount) AS Deposit_Amount,TerminalID, SUM(TaxAmount) AS TaxAmount, SUM(RoundAmount) AS RoundAmount FROM(SELECT CONCAT(retailer.RetailID, TP.TerminalID) AS id, retailer.RetailID AS RetailID, retailer.RetailCode, retailer.RetailName AS Retailer, T.TransID, T.TransNo, T.TransDate, ";
            sql += "T.CreateTime, T.SalesStatus AS TransType, T.SalesPerson, TP.SalesBalTtl AS SalesAmount, TP.PaymentID, P.Nick AS strPayment, SUM(IT.ItemUnitPrice * IT.ItemQty) AS GrossSales, SUM(IT.ItemUnitPrice * IT.ItemQty - IT.ItemTotal + IT.ItemDiscGroupAmt + ((IT.GroupDiscAmt + IT.GroupDiscAmt2 + IT.GroupDiscAmt3) * -1) + (IT.MemberAmt * -1) + IT.CardAmt) AS TotalDisc,";
            sql += "T.SalesSubTtl AS SubTotal, TP.SalesBalTtl AS BalanceAmount, TP.SalesPayTtl AS PayAmount, CASE WHEN P.Nick = 'CASH' THEN IF(TP.ChangeAmount = 0, '', CONCAT('Change :', SUM(TP.ChangeAmount))) ELSE '' END AS ChangeAmount, TP.SalesDeposit AS DepositAmount, TP.TerminalID AS TerminalID, T.SalesTaxTtl AS TaxAmount, T.SalesRounding AS RoundAmount FROM(SELECT SalesID AS TransID,";
            sql += "RetailID, SalesDate AS TransDate, CONCAT(DATE_FORMAT(CreateTime, '%h:%i %p')) AS CreateTime, SalesStatus, SalesNo AS TransNo, SalesPerson, SalesSubTtl,SalesTaxTtl, SalesRounding, TerminalID AS TerminalID1 FROM retail_sales WHERE " + Datecond + ") T LEFT JOIN retail_sales_payment TP ON TP.SalesID = T.TransID ";
            sql += "LEFT OUTER JOIN retail_sales_detail IT ON IT.SalesID = T.TransID LEFT OUTER JOIN retailer ON T.RetailID = retailer.RetailID LEFT JOIN list_paymentmethods P ON TP.PaymentID = P.ID WHERE(" + Cond + " ) GROUP BY id , RetailID , Retailer , TerminalID , PaymentID , TransID , T.TransDate) AS pivottbl";
            sql += " GROUP BY id , RetailID , Retailer , TerminalID , TransID , strPayment ORDER BY TransNo ASC ";
        }

        if (sql.Length > 0)
        {
            MySqlParameter[] objprama =

            {
                new MySqlParameter("@RetailID", RetailID),
                new MySqlParameter("@DateFrom", DateRangeFrom),
                new MySqlParameter("@DateTo", DateRangeTo),
                new MySqlParameter("@TransNo", transno)
            };

            ds = dataAccessor.RunSPRetDataset_Vapt(sql, "retail_sales", objprama);
        }

        string json = JsonConvert.SerializeObject(ds, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetDiscountList(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "SELECT ID, Nick, VALUE, ButtonName, PrintOnReceipt, FULL, ItemDisc, GroupDisc, DiscType, DiscAmount, OpenDisc FROM list_discount_sales where RecordStatus<>'DELETED'";

        DataSet DS = dataAccessor.RunSPRetDataset(strSql, "list_discount_sales");

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CreateDiscount(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        List<DiscountType> dtlist = new List<DiscountType>();
        //DiscountType dt = new DiscountType();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string response = "";

        try
        {
            dtlist = serializer.Deserialize<List<DiscountType>>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }

        response = dataAccessor.saveDiscount(dtlist);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetUsersList(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "select * from users where display='Y' and recordstatus<>'DELETED'";

        DataSet DS = dataAccessor.RunSPRetDataset(strSql, "users");

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetSecurityListSys(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "select * from securitylist_sys where sl_db='FRONTEND' and recordstatus<>'DELETED'";

        DataSet DS = dataAccessor.RunSPRetDataset(strSql, "Securitylist_sys");

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetUsersPermission(string companyCode, string sl_id, string userid)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string strSql = "select id, userid, slid, sl_code, accessaction from securityaccessright where slid in ( select slid from securitylist_sys where sl_db='FRONTEND' and recordstatus<>'DELETED') and display = 'Y' and recordstatus <> 'DELETED'";
        if (sl_id.Length > 0)
            strSql += " and slid=@SLID";
        if (userid.Length > 0)
            strSql += " and userid=@UserID";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@SLID", sl_id),
            new MySqlParameter("@UserID", userid)
        };

        DataSet DS = dataAccessor.RunSPRetDataset_Vapt(strSql, "securityaccessright", objparam);

        string json = JsonConvert.SerializeObject(DS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitStockTransfer(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        List<StockTransfer> list_transfer = new List<StockTransfer>();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        try
        {
            list_transfer = serializer.Deserialize<List<StockTransfer>>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveStockTransfer(list_transfer);

        return response;

    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitStockAdjust(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        List<StockAdjust> list_adjust = new List<StockAdjust>();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        try
        {
            list_adjust = serializer.Deserialize<List<StockAdjust>>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveStockAdjust(list_adjust);

        return response;

    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitStockTake(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        List<StockTake> list_take = new List<StockTake>();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        try
        {
            list_take = serializer.Deserialize<List<StockTake>>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveStockTake(list_take);

        return response;

    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string ImageUpload(string companyCode, string Base64ImgStr, string ImgFileName, string ImgType, string isDepartment, string isItem, string ItemDepartment, string ItemID, string ImgeUrl )
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string response = "";

        try
        {
            string sqlstr = "";
            string Imagetype = "Item";
            if (isDepartment == "Y")
                Imagetype = "Department";
            string imagepath = @"C:\Images\" + companyCode + "\\" + Imagetype + "\\";
            if (!Directory.Exists(imagepath))
            {
                Directory.CreateDirectory(imagepath);
            }
            byte[] bt = Convert.FromBase64String(Base64ImgStr);
            string ImgFullPath = imagepath + ImgFileName + "." + ImgType;
            File.WriteAllBytes(ImgFullPath, bt);
               
            if (File.Exists(ImgFullPath))
            {
                string idstr = "";
                if (isDepartment == "Y")
                {
                    sqlstr = "Update List_inv_departments set DeptImage=@ImageUrl where ID=@ID";
                    idstr = ItemDepartment;
                }

                if (isItem == "Y")
                {
                    sqlstr = "Update inventory set ItemPic=@ImageUrl, ItemPicFront=@ImageUrl where ItemID=@ID";
                    idstr = ItemID;
                }

                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@ImageUrl", ImgeUrl),
                    new MySqlParameter("@ID", idstr)
                };

                dataAccessor.Exec_UpdateQuery_Vapt(sqlstr, "", objparam);
                response = "Success";
            }
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }

        return response;

    }
}

