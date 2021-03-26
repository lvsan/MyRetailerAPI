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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        string fieldCriteria = "";
        if (retailID != "" || retailID != "0")
        {
            fieldCriteria = " retailid = '" + retailID;
        }
        string strSql = "SELECT inventory_retail.ItemID as ItemID, inventory.ItemSKU as ItemSKU, inventory.ItemDescp as ItemDescp, inventory_supbar.SupBarCode as SupBarCode, list_units.Nick AS UOM, inventory_unit.RTLSellPx as Price,inventory_retail.OnHandQty" +
            " FROM inventory_retail LEFT JOIN inventory ON inventory_retail.ItemID = inventory.ItemID " +
            " LEFT JOIN inventory_supbar ON inventory_supbar.ItemID = inventory_retail.ItemID AND inventory_supbar.RecordStatus <> 'DELETED' " +
            " LEFT JOIN inventory_unit ON inventory_retail.ItemID = inventory_unit.ItemID AND inventory_unit.RecordStatus <> 'DELETED' " +
            " LEFT JOIN list_units ON list_units.ID=inventory_unit.ItemUnit AND list_units.RecordStatus <> 'DELETED' " +
            " WHERE inventory_retail.RecordStatus <> 'DELETED' AND " + fieldCriteria + "'" +
            " Group By ItemID,ItemSKU,ItemDescp,SupBarCode,UOM";

        DataSet inventoryDS = dataAccessor.RunSPRetDataset(strSql, "inventory");

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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        string fieldCriteria = "";
        if (ItemSKU != "" || ItemSKU != "0")
        {
            fieldCriteria = " ItemSKU = '" + ItemSKU + "' OR inventory_supbar.SupBarCode = '" + ItemSKU;
        }
        string strSql = "SELECT list_units.Nick AS UOM, inventory_unit.RTLSellPx as Price" +
            " FROM inventory " +
            " LEFT JOIN inventory_supbar ON inventory_supbar.ItemID = inventory.ItemID AND inventory_supbar.RecordStatus <> 'DELETED' " +
            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID AND inventory_unit.RecordStatus <> 'DELETED' " +
            " LEFT JOIN list_units ON list_units.ID=inventory_unit.ItemUnit AND list_units.RecordStatus <> 'DELETED' " +
            " WHERE inventory.RecordStatus <> 'DELETED' AND (" + fieldCriteria + "')" +
            " Order By UOM";

        DataSet inventoryDS = dataAccessor.RunSPRetDataset(strSql, "InventoryUOM");

        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);

        return json;
    }
    
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getInventoryMultipleUOM(string companyCode, string retailID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        List<getInventory> getInventories = new List<getInventory>();
        string fieldCriteria = " retailid = '" + retailID;

        string sql = "SELECT inventory_retail.ItemID as ItemID, inventory.ItemSKU as ItemSKU, inventory.ItemDescp as ItemDescp, inventory_supbar.SupBarCode as SupBarCode " +
            " FROM inventory_retail" +
            " LEFT JOIN inventory ON inventory_retail.ItemID = inventory.ItemID" +
            " LEFT JOIN inventory_supbar ON inventory_supbar.ItemID = inventory_retail.ItemID AND inventory_supbar.RecordStatus <> 'DELETED' " +
            " WHERE inventory_retail.RecordStatus <> 'DELETED' AND " + fieldCriteria + "'";

        DataTable dt = dataAccessor.GetData(sql);
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
        SalesMasterDCS sales = new SalesMasterDCS();
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
            sales = serializer.Deserialize<SalesMasterDCS>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveSales(sales);

        return response;

    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitAdroindSales(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
        SalesMasterDCS sales = new SalesMasterDCS();
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
    public string submitOnlineSales(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
        SalesMaster sales = new SalesMaster();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

      /*  json = @"{""TransNo"":""A1234512"",
                ""RetailID"":""ONLINE"",
                ""SalesDate"":""2020/02/24 15:20:43"",
                ""SalesStatus"":""SALES"",
                ""MemberID"":""CUST917800001944774"",
                ""SalesTaxType"":""I"",
                ""SalesTaxRate"":""7"",
                ""SalesTaxTtl"":""49"",
                ""SalesRounding"":""0"",
                ""SalesTotalAmount"":""700"",
                ""ItemSales"":[{""ItemID"":""BSBN104"",""SupBarCode"":"""",""ItemSellPrice"":""350"",""ItemQty"":""2"",
			                ""ItemUOMDesc"":""Ctn (75gx72)"",""ItemDisc"":""0"",""ItemDisc2"":""0"",""ItemDisc3"":""0"",
			                ""ItemPrice"":""384.000"",""ItemTax"":""24.50"",""ItemTotal"":""700"",
			                ""ItemVoucher"":[{""VoucherNo"":""VC123"",""VoucherAmount"":""700""
							                }]
			                }],
                ""SalesPayments"":[{""paymentID"":"""",""strPayment"":""VISA"",""SalesPayTtl"":""700"",
			                ""SalesBalTtl"":""700"",""ChangeAmount"":""0"",""TipsAmount"":""0"",
			                ""SaleVoucher"":[{""VoucherNo"":""VC122"",""VoucherAmount"":""700""
							                }]
			                }],
                ""SalesPersons"":[]
                }"; */
        try
        {
            sales = serializer.Deserialize<SalesMaster>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveOnlineSales(sales);

        return response;

    }

	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public decimal getItemPromotion(string companyCode, DateTime SalesDate, string ItemSKU, string RetailID, decimal Qty, string UOM)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        string fieldCriteria = "";
        if (SalesDate != null)
        {
            fieldCriteria = " AND promo.Promo_DateFrom<='" + SalesDate.Date + "' AND promo.Promo_DateTo>='" + SalesDate.Date +"' ";
        }
        if (RetailID != null)
        {
            fieldCriteria = "AND (promo.Promo_RetailID = '0' AND promo.Promo_RetailID = '" + RetailID + "') ";
        }
        if (UOM != null)
        {
            fieldCriteria = "AND promo_item.ItemUOM = '" + UOM + "' ";
        }
        if (ItemSKU != "" || ItemSKU != "0")
        {
            fieldCriteria = " AND (inventory.ItemSKU = '" + ItemSKU + "' OR inventory_supbar.SupBarCode = '" + ItemSKU + "')";
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

        decimal maxTierPromoPrice = dataAccessor.calcPromoPrice(strSql, Qty);

        return maxTierPromoPrice;
    }

	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getMember(string companyCode, string find1, string find2)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

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
                fieldCriteria1 = " (CustICNO LIKE '%" + find1 + "%' OR email LIKE '%" + find1 + "%' OR hph LIKE '%" + find1 + "%' OR cardnumber LIKE '%" + find1 + "%' OR customerFirstName LIKE '%" + find1 + "%' OR customerLastName LIKE '%" + find1 + "%')";
            }
            if (find2 != null)
            {
                if (find1 == null)
                {
                    fieldCriteria2 = " (CustICNO LIKE '%" + find2 + "%' OR email LIKE '%" + find2 + "%' OR hph LIKE '%" + find2 + "%' OR cardnumber LIKE '%" + find2 + "%' OR customerFirstName LIKE '%" + find2 + "%' OR customerLastName LIKE '%" + find2 + "%')";
                }
                else
                {
                    fieldCriteria2 = " AND (CustICNO LIKE '%" + find2 + "%' OR email LIKE '%" + find2 + "%' OR hph LIKE '%" + find2 + "%' OR cardnumber LIKE '%" + find2 + "%' OR customerFirstName LIKE '%" + find2 + "%' OR customerLastName LIKE '%" + find2 + "%')";
                }
            }
        }
        string strSql = "SELECT ID AS MemberID,custcode, CustICNO, Email, hph as MobileNo, cardnumber, CustomerDOB AS DOB, customerFirstName as FirstName, customerLastName AS LastName, " +
                        "CustomerAddress1 AS Address1, CustomerAddress2  AS Address2, CustomerAddress3  AS Address3, CustomerPostcode AS PostalCode, OpeningLP " +
                        " FROM customer" +
                        " WHERE RecordStatus <> 'DELETED' AND " + fieldCriteria1 + fieldCriteria2 +
                        " Order By custcode,CustICNO,Email";

        DataSet memberDS = dataAccessor.RunSPRetDataset(strSql, "Member");

        string json = JsonConvert.SerializeObject(memberDS, Formatting.Indented);

        return json;
    }
	
	[WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitOrders(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        List<ItemPrice> prices = new List<ItemPrice>();
        //DataTable dt = GetData(string.Format("SELECT Item_UnitID,RTLSellPx FROM inventory_unit Where RecordStatus <> 'DELETED' AND ItemID ='{0}'", ItemID));
        DataTable dt = dataAccessor.GetData(string.Format("SELECT Item_UnitID,(SELECT Nick FROM list_units WHERE ID= ItemUnit) AS UOM ,RTLSellPx,ItemUnitDef " + 
                                " FROM  inventory_unit WHERE recordstatus<>'DELETED' AND ItemID='{0}' ORDER BY ItemID ASC, ItemUnitDef DESC " , ItemID));
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
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

    /****** below is to pull member point information ***********/
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getMemberPoint(string companyCode,string hph,string nric)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        string fieldCriteria = "";
        if (hph != "" || hph != "0")
        {
            fieldCriteria = " WHERE hph = '" + hph + "'";
        }
        if (nric != "" || nric != "0")
        {
            if (fieldCriteria != "") {
                fieldCriteria += " AND RIGHT(custicno ,4) ='" + nric + "'";
            }
            else {
                fieldCriteria = " WHERE RIGHT(custicno ,4) ='" + nric + "'";
            }
        }

        string strSql = "SELECT hph,TotalLP AS BalancePoint,TotalEP AS EarnPoint,TotalRP AS RedeemPoint,TotalAP AS AdjustPoint,(SELECT getExpiringPoint(customer.ID)) AS ExpiryPoint," +
                        "ExpiryPointDate AS ExpiringDate FROM customer " + fieldCriteria; 

       /* string strSql = "SELECT SPV01 AS MemberID,hph AS MobileNo,TotalLP AS BalancePoint,0 AS RedeemPoint,0 AS AdjustPoint,0 AS ExpiryPoint," +
                        "(SELECT DATE_FORMAT(LAST_DAY(CONCAT(SUBSTR(CURDATE(),1,4)+1,LPAD(PointCutOffMonth,2,'0'),'01')),'%d-%m') AS expiryDate FROM customer_definitions) AS ExpiringDate " +
                        " FROM customer " + fieldCriteria; */
        DataSet memLPDS = dataAccessor.RunSPRetDataset(strSql, "Member");

        string json = JsonConvert.SerializeObject(memLPDS, Formatting.Indented);
        //List<string[]> asd = inventoryDS;

        //return asd;
        return json;
    }

    /*** below is for new integration for e-commerce ******/
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string AddECategories(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        ECategories EC = new ECategories();
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
            {"eCategory":[{"cat_id":"1","cat_code":"Bird’s Nest & Snow Jelly","cat_descp":"Bird’s Nest & Snow Jelly","cat_otherlanguage":"","cat_createdate":"2019-11-13"},
  {"cat_id":"2","cat_code":"Dried Bird’s Nest","cat_descp":"Dried Bird’s Nest","cat_otherlanguage":"","cat_createdate":"2019-11-13"}]}
          */
        try
        {
            EC = serializer.Deserialize<ECategories>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveECateoriesData(EC);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UpdateECategories(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        ECategories EC = new ECategories();
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
            {"eCategory":[{"cat_id":"1","cat_code":"Bird’s Nest & Snow Jelly","cat_descp":"Bird’s Nest & Snow Jelly","cat_otherlanguage":"","cat_createdate":"2019-11-13"},
  {"cat_id":"2","cat_code":"Dried Bird’s Nest","cat_descp":"Dried Bird’s Nest","cat_otherlanguage":"","cat_createdate":"2019-11-13"}]}
          */
        try
        {
            EC = serializer.Deserialize<ECategories>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.updateECateoriesData(EC);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string DeleteECategories(string companyCode, string cateid)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        string response = dataAccessor.deleteECateoriesData(cateid);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string AddEMember(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        MemberInfor member = new MemberInfor();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        string response = "";

        /*string jSon = @"{
            "MemberID":"12122222",
            "MemberName":"Testing181212",
            "Gender":"F", 
            "DOB":"1988-12-12", 
            "CreateDate":"2019-11-07",
            "NRIC":"890S", 
            "Email":"test@gmail.com", 
            "HPH":"91838364", 
            "Address1":"blk 63", 
            "Address2":"kallang bahru", 
            "Address3":"", 
            "PostalCode":"330063", 
            "Country":"SGD"
            }";
         
            passing value : 
            {"MemberID":"12122222","MemberName":"Testing181212","Gender":"F",
             "DOB":"2078-12-12","CreateDate":"2019-11-13","NRIC":"","Email":"","HPH":"","Address1":"","Address2":"","Address3":"",
             "PostalCode":"","Country":""}
         */
        try
        {
            member = serializer.Deserialize<MemberInfor>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.saveMemberData(member);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CreateItem(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        string strSql = "SELECT custcode, CustomerFirstName AS FirstName,CustomerMiddleName AS MiddleName,CustomerLastName AS LastName,CustomerMetaName As NickName,CustomerOtherLanguageName AS OtherName," +
                        "CustomerSexID AS Sex,CustICNO AS NRIC,CustomerDOB AS DOB,IF((SELECT VALUE FROM list_classifications WHERE ID= CustomerClassificationID) IS NULL,'',(SELECT VALUE FROM list_classifications WHERE ID= CustomerClassificationID)) AS Classification," +
                        "IF((SELECT VALUE FROM list_designations WHERE ID= CustomerDesignationID) IS NULL,'',(SELECT VALUE FROM list_designations WHERE ID= CustomerDesignationID)) AS Designation," +
                        "IF((SELECT VALUE FROM list_departments WHERE ID= CustomerDepartmentID) IS NULL,'',(SELECT VALUE FROM list_departments WHERE ID= CustomerDepartmentID)) AS Department," +
                        "hph_CountryCode,hph_AreaCode, hph,CustomerAddress1 AS Address1,CustomerAddress2 AS Address2,CustomerAddress3 AS Address3," +
                        "CustomerPostcode AS PostalCode,(SELECT Nick FROM list_countries WHERE ID=CustomerCountryID) AS Country," +
                        "CustomerStartDate AS StartDate,CustomerEndDate AS EndDate,CustomerID AS MemberID FROM tblexportcustomer"; 
        DataSet memberDS = dataAccessor.RunSPRetDataset(strSql, "member");

        string json = JsonConvert.SerializeObject(memberDS, Formatting.Indented);
       // List<string[]> asd = memberDS;

        //return asd;
        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string CombineSellRetrieveOrder(string companyCode, string LoginID, string Pswd, string fromDate, string toDate)
    {

        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
        List<OrderReturn> OrderReturns = new List<OrderReturn>();
        
        //Login to get token
        //LoginID = "dcs@combinesell.com";
        //Pswd = "gnPeotrWVqT1YRHX";
        var token = "";
        var token_type = "";
        string responseStr = "";
        string loginurl = "https://app.combinesell.com/api/auth/login";

        var postData = 
            new {
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
		string orderUrl = "https://app.combinesell.com/api/orders?" +limit +page +updateFromDate +updateToDate +createFromDate +createToDate +status +search;
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
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
            dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, dbCode);

        string response = "";

        response = dataAccessor.updateWarrantyStatus(companyCode, status);

        return response;
    }


    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getWarranty(string dbCode, string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, dbCode);

        //string response = "";

        string strSql = " SELECT Company, SetupDate, LicenseType, Key_License, ActivateDate, MaintenanceStatus" +
                        " FROM tbllicense" +
                        " WHERE Company='" + companyCode +"'";

        DataSet inventoryDS = dataAccessor.RunSPRetDataset(strSql, "Warranty");

        string json = JsonConvert.SerializeObject(inventoryDS, Formatting.Indented);

        return json;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getPaymentMethod(string companyCode)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

        string strSQL = "";
        if (Query =="")
        {
            return "No data to be found.";
        } else {
			strSQL = Query;
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
        dataAccessor.connectionstring = dataAccessor.getConnectionString(dataAccessor, companyCode);

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
	
}

