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
public class myShareAPI : System.Web.Services.WebService
{

    public myShareAPI()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string submitOnlineSales(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
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
    public string getMember(string companyCode,string MemberID, string find1, string find2)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string fieldCriteria = "";
        string fieldCriteria1 = "";
        string fieldCriteria2 = "";
        if (MemberID=="" && find1 == "" && find2 == "")
        {
            return "Please provide search criteria.";
        }
        else 
        {
            if (MemberID != null && MemberID != "")
            {
                fieldCriteria = " (ID='" + MemberID + "') ";
            }
            if (find1 != null && find1 != "")
            {
                if (MemberID == null || MemberID == "")
                {
                    fieldCriteria1 = " (CustICNO LIKE '%" + find1 + "%' OR email LIKE '%" + find1 + "%' OR hph LIKE '%" + find1 + "%' OR cardnumber LIKE '%" + find1 + "%' OR customerFirstName LIKE '%" + find1 + "%' OR customerLastName LIKE '%" + find1 + "%')";
                } else {
                    fieldCriteria1 = " AND (CustICNO LIKE '%" + find1 + "%' OR email LIKE '%" + find1 + "%' OR hph LIKE '%" + find1 + "%' OR cardnumber LIKE '%" + find1 + "%' OR customerFirstName LIKE '%" + find1 + "%' OR customerLastName LIKE '%" + find1 + "%')";
                }
            }
            if (find2 != null && find2 != "")
            {
                if (find1 == null || find1 == "")
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
                        "CustomerAddress1 AS Address1, CustomerAddress2  AS Address2, CustomerAddress3  AS Address3, CustomerPostcode AS PostalCode, " + 
                        "IF(CustomerCountryID='','',(SELECT FULL FROM list_countries WHERE ID=customer.CustomerCountryID)) AS Country," +
                        "IF(CustomerSexID='','',(SELECT Nick FROM list_sexes WHERE ID= CustomerSexID)) AS Gender,OpeningLP " +
                        " FROM customer" +
                        " WHERE RecordStatus <> 'DELETED' AND " + fieldCriteria + fieldCriteria1 + fieldCriteria2 +
                        " Order By custcode,CustICNO,Email";

        DataSet memberDS = dataAccessor.RunSPRetDataset(strSql, "Member");

        string json = JsonConvert.SerializeObject(memberDS, Formatting.Indented);

        return json;
    }
	
    /****** below is to pull member point information ***********/
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string getMemberPoint(string companyCode,string MemberID,string hph,string nric)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string fieldCriteria = "";
        if (MemberID != "" && MemberID != "0")
        {
            fieldCriteria = " WHERE ID ='" + MemberID + "' OR VendorMemberID = '" + MemberID + "'";
        }
        if (hph != "" && hph != "0")
        {
            if (fieldCriteria != "")
            {
                fieldCriteria += " AND hph = '" + hph + "'";
            }
            else
            {
                fieldCriteria = " WHERE hph = '" + hph + "'";
            }            
        }
        if (nric != "" && nric != "0")
        {
            if (fieldCriteria != "") {
                fieldCriteria += " AND RIGHT(custicno ,4) ='" + nric + "'";
            }
            else {
                fieldCriteria = " WHERE RIGHT(custicno ,4) ='" + nric + "'";
            }
        }

        string strSql = "SELECT hph,TotalLP AS BalancePoint,TotalEP AS EarnPoint,TotalRP AS RedeemPoint,TotalAP AS AdjustPoint,(SELECT getExpiringPoint(customer.ID)) AS ExpiryPoint," +
                        "IF(ExpiryPointDate = 0000-00-00," +
                        "(SELECT DATE_FORMAT(LAST_DAY(CONCAT(SUBSTR(CURDATE(),1,4)+1,LPAD(PointCutOffMonth,2,'0'),'01')),'%d-%m') AS expiryDate FROM customer_definitions),ExpiryPointDate)  AS ExpiringDate, " +
                        " ID AS MemberID FROM customer " + fieldCriteria; 

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
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

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
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

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
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        string response = dataAccessor.deleteECateoriesData(cateid);

        return response;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string AddEMember(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

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
            "Country":"SGD",
            "VendorMemberID" :"";
            }";
         
            passing value : 
            {"MemberID":"12122222","MemberName":"Testing181212","Gender":"F",
             "DOB":"2078-12-12","CreateDate":"2019-11-13","NRIC":"","Email":"","HPH":"","Address1":"","Address2":"","Address3":"",
             "PostalCode":"","Country":"","VendorMemberID":""}
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
    public string UpdateEMember(string companyCode, string json)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

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
            "Country":"SGD",
            "VendorMemberID":"",
            }";
         
            passing value : 
            {"MemberID":"12122222","MemberName":"Testing181212","Gender":"F",
             "DOB":"2078-12-12","CreateDate":"2019-11-13","NRIC":"","Email":"","HPH":"","Address1":"","Address2":"","Address3":"",
             "PostalCode":"","Country":"","VendorMemberID":""}
         */
        try
        {
            member = serializer.Deserialize<MemberInfor>(json);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        response = dataAccessor.updateMemberData(member);

        return response;
    }
    
}

