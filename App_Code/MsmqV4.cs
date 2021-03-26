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
using System.Configuration;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;
using System.Net.NetworkInformation;
using MySql.Data.MySqlClient;

/// <summary>
/// Summary description for MsmqV4
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class MsmqV4 : System.Web.Services.WebService
{

    public MsmqV4()
    {
        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int GetMessageCount(string companyCode, string retailID, string terminalID)
    {
        MessageQueue myQueue;
        int messageCount = 0;
        string str_foldername = companyCode + "_" + retailID.PadLeft(3, '0')  + "_" + terminalID.PadLeft(2, '0')  + "_out";
        string path_queue = @".\private$\" + str_foldername;
        if (MessageQueue.Exists(path_queue))
        {
            myQueue = new MessageQueue(path_queue);
            myQueue.DefaultPropertiesToSend.Recoverable = true;
            myQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

            System.Messaging.Message[] myMessage = myQueue.GetAllMessages();

            if (myMessage != null && myMessage.Length > 0)
            {
                messageCount = myMessage.Length;
            }
        }
        return messageCount;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
	public DataSet GetMessages(string companyCode, string retailID, string terminalID)
        {
            MessageQueue myQueue;
            string str_foldername = companyCode + "_" + retailID.PadLeft(3, '0')  + "_" + terminalID.PadLeft(2, '0') + "_out";
            string path_queue = @".\private$\" + str_foldername;
			
			string str_backupfoldername = companyCode + "_" + retailID.PadLeft(3, '0')  + "_" + terminalID.PadLeft(2, '0') + "_backup";
            string path_backupqueue = @".\private$\" + str_backupfoldername;
            DataSet result = new DataSet();

            DataTable NewDT = new DataTable();
            NewDT.Columns.Add("SNo", typeof(string));
            NewDT.Columns.Add("Context", typeof(string));
            NewDT.Columns.Add("Priority", typeof(string));
            NewDT.Columns.Add("Label", typeof(string));


            if (MessageQueue.Exists(path_queue))
            {
                myQueue = new MessageQueue(path_queue);
                myQueue.DefaultPropertiesToSend.Recoverable = true;
                myQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

                createMsmqQueue(path_backupqueue);
                MessageQueue myBackupQueue = new MessageQueue(path_backupqueue);
                myBackupQueue.DefaultPropertiesToSend.Recoverable = true;
                System.Messaging.Message myBackupMessage = new System.Messaging.Message();

                int count = 1;
                while (true)
                {
                    try
                    {
                        myQueue.MessageReadPropertyFilter.Priority = true;
                        System.Messaging.Message m = myQueue.Receive(new TimeSpan(0, 0, 3));

                        m.Formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(string) });

                        string context = m.Body.ToString();

                        if (context != null && context.Length > 0)
                        {
                            NewDT.Rows.Add(count.ToString(), context, m.Priority.ToString(), m.Label.ToString());

                            myBackupMessage.Label = m.Label;
                            myBackupMessage.Body = m.Body;
                            myBackupMessage.Priority = m.Priority;
                            myBackupQueue.Send(myBackupMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }

                result.Tables.Add(NewDT);
            }
            return result;
        }
	
	
	
	
    

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string ReceiveMessages(string companyCode, string retailID, string terminalID)
    {
        //WILL REMOVE FROM QUEUE
        string str_foldername = companyCode + "_" + retailID.PadLeft(3, '0')  + "_" + terminalID.PadLeft(2, '0') + "_out";
        string queueName = @".\private$\" + str_foldername;
        string result = "Finished";

        //Receiving Messages from MSMQ
        MessageQueue messageQueue = new MessageQueue(queueName);
        messageQueue.DefaultPropertiesToSend.Recoverable = true;
        messageQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

        //To keep a check that if no messages present in MSMQ,
        //control should return back in 1 second. 
        TimeSpan timeout = new TimeSpan(5);
        System.Messaging.Message message = null;

        //Get all the messages present in the MSMQ and iterate through it.
        MessageEnumerator enumerator = messageQueue.GetMessageEnumerator2();

        int i = 0;
        while (enumerator.MoveNext(timeout) && i < 100)
        {
            message = enumerator.RemoveCurrent(timeout);
            if (message != null)
            {
                string str_backupfoldername = companyCode + "_" + retailID.PadLeft(3, '0')  + "_" + terminalID.PadLeft(2, '0') + "_backup";
                string path_backupqueue = @".\private$\" + str_backupfoldername;

                createMsmqQueue(path_backupqueue);
                MessageQueue myBackupQueue = new MessageQueue(path_backupqueue);
                myBackupQueue.DefaultPropertiesToSend.Recoverable = true;
                System.Messaging.Message myBackupMessage = new System.Messaging.Message();

                string tablename;
                XmlMessageFormatter formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                message.Formatter = formatter;
                string context = message.Body.ToString();
                if (context != null && context.Length > 0)
                {
                    myBackupMessage.Label = "Generate_Server_" + message.Label;
                    myBackupMessage.Body = message.Body;
                    myBackupQueue.Send(myBackupMessage);
                }
            }
            enumerator.Reset();
            i++;
        }
        return result;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool PostMessages(string companyCode, string retailID, string terminalID)
    {
        bool results;
        results = runMSMQ(companyCode, retailID, terminalID, "SS", "");
        return results;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int CalculateCustomerPoint(string companyCode, string field, string fieldID)
    {
        int result = 0;
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        string fieldCriteria = "";
        if (fieldID != "0" && fieldID !="")
        {
            string sql_field = "SELECT COUNT(*) FROM  information_schema.COLUMNS WHERE TABLE_SCHEMA=@DBName AND TABLE_NAME='CUSTOMER'  AND COLUMN_NAME=@Field";
            MySqlParameter[] objparam_field =
            {
                new MySqlParameter("@DBName", dataAccessor.dbname),
                new MySqlParameter("@Field", field)
            };
            int count = 0;
            try
            {
                count = int.Parse(dataAccessor.ExecScalarVal_Vapt(sql_field, "", objparam_field).ToString());
            }catch(Exception exf)
            {
                count = 0;
            }

            if (count > 1)
            {
                fieldCriteria = " AND " + field + "=@FieldID";
            }

        }
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@FieldID", fieldID)
        };
       /* string strSql = "SELECT CustID, SUM(TotalAmount) AS spendAmt,SUM(AdjustPoint)+SUM(LoyaltyPoint) AS LP,SUM(RedeemPoint) AS RP" +
            " FROM customer_salesdetails WHERE 1=1 " + fieldCriteria + " GROUP BY CustID"; */
        string strSql = "SELECT t.*,((SELECT OPENINGLP FROM CUSTOMER WHERE ID=t.CustID) + t.LP - t.RP + ADJLP) AS BalPoint FROM (" +
                       "SELECT CustID, SUM(TotalAmount) AS spendAmt,SUM(LoyaltyPoint) AS LP,SUM(RedeemPoint) AS RP, " +
                       "SUM(AdjustPoint) AS ADJLP FROM customer_salesdetails WHERE 1=1 " + fieldCriteria + " GROUP BY CustID ) AS t";
        DataSet customerDS = dataAccessor.RunSPRetDataset_Vapt(strSql, "customer_salesdetails", objparam);
        string CustID, criteria;
        double balPoint = 0.00;
        double spendAmt = 0.00;
        double LP=0.00, RP=0.00, AP=0.00;
        string PurchaseDate = "";
        string AdjustDate = "";
        string RedeemDate = "";
        decimal redeemPoint = 0;
        decimal lastEarnPoint = 0;
        foreach (DataRow dsTableRow in customerDS.Tables[0].Rows)
        {
            CustID = dsTableRow["CustID"].ToString();
            spendAmt = double.Parse(dsTableRow["spendAmt"].ToString());
            LP = double.Parse(dsTableRow["LP"].ToString());
            RP = double.Parse(dsTableRow["RP"].ToString());
            AP = double.Parse(dsTableRow["ADJLP"].ToString());

            if (dsTableRow["BalPoint"] != null && dsTableRow["BalPoint"].ToString().Length > 0)
            {
                balPoint = double.Parse(dsTableRow["BalPoint"].ToString());
            }

            string sqlstr = "SELECT DISTINCT TransDate,IF((LoyaltyPoint) IS NULL,0,(LoyaltyPoint)) AS LoyaltyPoint FROM customer_salesdetails WHERE bitLoyalty='Y' AND LoyaltyPoint>0 AND custid=@CustID ORDER BY RecordUpdate DESC LIMIT 1";
            MySqlParameter[] objparam1 =
            {
                new MySqlParameter("@CustID", CustID)
            };
            DataTable DT1 = dataAccessor.GetData_Vapt(sqlstr, objparam1);
            if (DT1.Rows.Count != 0)
            {
                PurchaseDate = Convert.ToDateTime(DT1.Rows[0]["TransDate"]).ToString("yyyy-M-d");
                lastEarnPoint= Convert.ToDecimal(DT1.Rows[0]["LoyaltyPoint"]);
            }
            DT1.Dispose();
            DT1.Clear();

            sqlstr = "SELECT DISTINCT TransDate,IF((RedeemPoint) IS NULL,0,(RedeemPoint)) AS RedeemPoint FROM customer_salesdetails WHERE bitRedeem='Y' AND custid=@CustID ORDER BY RecordUpdate DESC LIMIT 1";
            DT1 = dataAccessor.GetData_Vapt(sqlstr, objparam1);
            if (DT1.Rows.Count != 0)
            {
                RedeemDate = Convert.ToDateTime(DT1.Rows[0]["TransDate"]).ToString("yyyy-M-d");
                redeemPoint = Convert.ToDecimal(DT1.Rows[0]["RedeemPoint"]);
            }
            DT1.Dispose();
            DT1.Clear();

            sqlstr = "SELECT DISTINCT TransDate FROM customer_salesdetails WHERE bitAdjust='Y' AND custid=@CustID ORDER BY RecordUpdate DESC LIMIT 1";
            DT1 = dataAccessor.GetData_Vapt(sqlstr, objparam1);
            if (DT1.Rows.Count != 0)
            {
                AdjustDate = Convert.ToDateTime(DT1.Rows[0]["TransDate"]).ToString("yyyy-M-d");
            }
            DT1.Dispose();
            DT1.Clear();

            criteria = " WHERE ID=@CustID";
            strSql = "UPDATE customer SET TotalLP=@TotalLP,TotalEP=@TotalEP,TotalRP=@TotalRP,TotalAP=@TotalAP,TotalYTDSales=@TotalYTDSales" +
                    ",LastPurchaseDate=@LastPurchaseDate,LastEarnPoint=@LastEarnPoint,LastRedeemDate=@LastRedeemDate,LastRedeemPoint=@LastRedeemPoint," +
                    "LastAdjustDate=@LastAdjustDate WHERE ID=@CustID";
            MySqlParameter[] objparam2 =
            {
                new MySqlParameter("@CustID", CustID),
                new MySqlParameter("@TotalLP", balPoint),
                new MySqlParameter("@TotalEP", LP),
                new MySqlParameter("@TotalRP", RP),
                new MySqlParameter("@TotalAP", AP),
                new MySqlParameter("@TotalYTDSales", spendAmt),
                new MySqlParameter("@LastPurchaseDate", PurchaseDate),
                new MySqlParameter("@LastEarnPoint", lastEarnPoint),
                new MySqlParameter("@LastRedeemDate", RedeemDate),
                new MySqlParameter("@LastRedeemPoint", redeemPoint),
                new MySqlParameter("@LastAdjustDate", AdjustDate)
            };
            result = dataAccessor.Exec_UpdateQuery_Vapt(strSql, "", objparam2);

            //SendMessages(dataAccessor, companyCode, "customer", criteria, terminalList);
        }
        customerDS.Dispose();
        customerDS.Clear();
        return result;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string reCalcMemberPoint(string companyCode )
    {
        decimal BalPoint = 0;
        decimal TotalSalesAmt = 0;
        decimal TotalEarnPoint = 0;
        decimal TotalRP = 0;
        decimal TotalAdjustPoint = 0;
        decimal redeemPoint=0;
        decimal lastEarnPoint = 0;

        string AdjustDate="";
        string PurchaseDate="";
        string RedeemDate="";
        string Status = "";
        string strSQL1 ="";
        string strMemberID = "";

        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        DataTable DT = dataAccessor.GetData(string.Format("SELECT * FROM (SELECT Customer.ID,Customer.OpeningLP, customer.TotalLP AS TotalLP1," +
                        "(OpeningLP + t.balPoint) AS BalPoint,t.totallp,t.totalRP,t.TotalAP,t.TotalAmt  FROM customer " +
                        " INNER JOIN (SELECT CustID,IF(SUM(loyaltypoint - RedeemPoint + AdjustPoint) IS NULL,0,SUM(loyaltypoint - RedeemPoint + AdjustPoint)) AS balpoint," +
                        "IF(SUM(loyaltypoint) IS NULL,0,SUM(loyaltypoint)) AS totallp,IF(SUM(RedeemPoint) IS NULL,0,SUM(RedeemPoint)) AS totalRP," +
                        "IF(SUM(AdjustPoint) IS NULL,0,SUM(AdjustPoint)) AS TotalAP,IF(SUM(TotalAmount) IS NULL,0,SUM(TotalAmount)) AS TotalAmt  " +
                        " FROM customer_salesdetails GROUP BY CustID) AS t ON t.CustID= customer.ID " +
                        " WHERE RecordStatus<>'DELETED') AS P WHERE P.BalPoint <>TotalLP1 ORDER BY ID"));

        if (DT.Rows.Count != 0)
        {
            Status="CALC";

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                strSQL1 = "";
                strMemberID = Convert.ToString(DT.Rows[i]["ID"]);
                BalPoint = Convert.ToDecimal(DT.Rows[i]["BalPoint"]);
                TotalSalesAmt = Convert.ToDecimal(DT.Rows[i]["TotalAmt"]);
                TotalEarnPoint = Convert.ToDecimal(DT.Rows[i]["totallp"]);
                TotalRP = Convert.ToDecimal(DT.Rows[i]["totalRP"]);
                TotalAdjustPoint = Convert.ToDecimal(DT.Rows[i]["TotalAP"]);

                string sqlstr = "SELECT DISTINCT TransDate,IF((LoyaltyPoint) IS NULL,0,(LoyaltyPoint)) AS LoyaltyPoint FROM customer_salesdetails WHERE bitLoyalty='Y' AND LoyaltyPoint>0 AND custid=@MemberID ORDER BY RecordUpdate DESC LIMIT 1";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@MemberID", strMemberID)
                };
                DataTable DT1 = dataAccessor.GetData_Vapt(sqlstr, objparam);
                if (DT1.Rows.Count != 0)
                {
                    PurchaseDate = Convert.ToDateTime(DT1.Rows[0]["TransDate"]).ToString("yyyy-M-d");
                    lastEarnPoint = Convert.ToDecimal(DT1.Rows[0]["LoyaltyPoint"]);
                }
                DT1.Dispose();
                DT1.Clear();

                sqlstr = "SELECT DISTINCT TransDate,IF((RedeemPoint) IS NULL,0,(RedeemPoint)) AS RedeemPoint FROM customer_salesdetails WHERE bitRedeem='Y' AND custid=@MemberID ORDER BY RecordUpdate DESC LIMIT 1";
                DT1 = dataAccessor.GetData_Vapt(sqlstr, objparam);
                if (DT1.Rows.Count != 0)
                {
                    RedeemDate = Convert.ToDateTime(DT1.Rows[0]["TransDate"]).ToString("yyyy-M-d");
                    redeemPoint = Convert.ToDecimal(DT1.Rows[0]["RedeemPoint"]);
                }
                DT1.Dispose();
                DT1.Clear();

                sqlstr = "SELECT DISTINCT TransDate FROM customer_salesdetails WHERE bitAdjust='Y' AND custid=@MemberID ORDER BY RecordUpdate DESC LIMIT 1";
                DT1 = dataAccessor.GetData_Vapt(sqlstr, objparam);
                if (DT1.Rows.Count != 0)
                {
                    AdjustDate = Convert.ToDateTime(DT1.Rows[0]["TransDate"]).ToString("yyyy-M-d");
                }
                DT1.Dispose();
                DT1.Clear();
         		
		        if(PurchaseDate!= ""){
                    strSQL1 += " LastPurchaseDate=@LastPurchaseDate,LastEarnPoint=@LastEarnPoint,";
		        }
        		
		        if(RedeemDate!=""){
                    strSQL1 += "LastRedeemDate=@LastRedeemDate,LastRedeemPoint=@LastRedeemPoint,";
		        }
        		
		        if(AdjustDate!=""){
                    strSQL1 += "LastAdjustDate=@LastAdjustDate,";
		        }

                string strSQL = "UPDATE customer SET " + strSQL1 +                     
                            "TotalLP=@TotalLP,TotalEP=@TotalEP,TotalRP=@TotalRP,TotalAP=@TotalAP," +
                            "TotalYTDSales=@TotalYTDSales,RECORDUPDATE = @RecordUpdate WHERE ID=@MemberID";
                using (MySqlConnection objCnn = new MySqlConnection(dataAccessor.connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(strSQL, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@TotalLP", BalPoint);
                            cmd.Parameters.AddWithValue("@TotalEP", TotalEarnPoint);
                            cmd.Parameters.AddWithValue("@TotalRP", TotalRP);
                            cmd.Parameters.AddWithValue("@TotalAP", TotalAdjustPoint);
                            cmd.Parameters.AddWithValue("@TotalYTDSales", TotalSalesAmt);
                            cmd.Parameters.AddWithValue("@LastPurchaseDate", PurchaseDate);
                            cmd.Parameters.AddWithValue("@LastEarnPoint", lastEarnPoint);
                            cmd.Parameters.AddWithValue("@LastRedeemDate", RedeemDate);
                            cmd.Parameters.AddWithValue("@LastRedeemPoint", redeemPoint);
                            cmd.Parameters.AddWithValue("@LastAdjustDate", AdjustDate);                            
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
							cmd.Parameters.AddWithValue("@MemberID", strMemberID);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                    }
                }
                Status += i;
            }
        }
        DT.Dispose();
        DT.Clear();

        return Status;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int CalculateInventoryOnhandQty(string companyCode, string RetailID, string field, string fieldID)
    {
        int result = 0;
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        string retailCriteria = "";
        if (RetailID != "0")
        {
            retailCriteria = " AND IR.RetailID=@RetailID";
        }
        string strSql = "";
        string itemCriteria = "";
        string ItemID = "";
        if (fieldID != "0")
        {
            string sql_field = "SELECT COUNT(*) FROM  information_schema.COLUMNS WHERE TABLE_SCHEMA=@DBName AND TABLE_NAME='INVENTORY'  AND COLUMN_NAME=@Field";
            MySqlParameter[] objparam_field =
            {
                new MySqlParameter("@DBName", dataAccessor.dbname),
                new MySqlParameter("@Field", field)
            };
            int count = 0;
            try
            {
                count = int.Parse(dataAccessor.ExecScalarVal_Vapt(sql_field, "", objparam_field).ToString());
            }
            catch (Exception exf)
            {
                count = 0;
            }

            if (count > 0)
            {
                strSql = "SELECT ItemID FROM inventory WHERE " + field + "=@FieldID";
                MySqlParameter[] objparam_field1 =
                {
                    new MySqlParameter("@FieldID", fieldID)
                };
                ItemID = dataAccessor.ExecScalarVal_Vapt(strSql, "", objparam_field1).ToString();
                itemCriteria = " AND IR.ItemID=@ItemID";
            }
        }
        strSql = "SELECT t.*,IF(t.ItemActQty IS NULL,t.OnHandQty,FLOOR(t.OnHandQty/t.ItemActQty)) AS OnHandDefQty FROM (" +
                 "SELECT IR.ItemID, IR.RetailID, getItemOnHandQty(IR.ItemID,IR.RetailID, NOW()) AS OnHandQty," +
                 "(SELECT ItemActQty FROM inventory_unit WHERE ItemID = IR.ItemID AND ItemUnitDef='Y' AND RecordStatus<>'DELETED') AS ItemActQty " +
                 " FROM inventory_retail IR WHERE 1=1 " + itemCriteria + retailCriteria + " GROUP BY IR.ItemID, IR.RetailID) AS t";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@RetailID", RetailID),
            new MySqlParameter("@ItemID", ItemID)
        };
        DataSet inventoryDS = dataAccessor.RunSPRetDataset_Vapt(strSql, "inventory_retail", objparam);
        
        string criteria;
        double OnHandQty = 0.00;
        double onHandDefQty = 0.00;
        foreach (DataRow dsTableRow in inventoryDS.Tables[0].Rows)
        {
            ItemID = dsTableRow["ItemID"].ToString();
            RetailID = dsTableRow["RetailID"].ToString();
            OnHandQty = double.Parse(dsTableRow["OnHandQty"].ToString());
            onHandDefQty = double.Parse(dsTableRow["OnHandDefQty"].ToString());

            criteria = " WHERE ItemID=@ItemID AND RetailID=@RetailID";

            strSql = "UPDATE inventory_retail SET OnHandQty=" + OnHandQty + ",OnHandDefQty=" + onHandDefQty + " " + criteria;
            MySqlParameter[] objparam1 =
            {
                new MySqlParameter("@RetailID", RetailID),
                new MySqlParameter("@ItemID", ItemID),
                new MySqlParameter("@OnHandQty", OnHandQty),
                new MySqlParameter("@OnHandDefQty", onHandDefQty)
            };

            result = dataAccessor.Exec_UpdateQuery_Vapt(strSql, "", objparam1);
            //SendMessages(dataAccessor, companyCode, "inventory_retail", criteria, terminalList);
        }
        inventoryDS.Dispose();
        inventoryDS.Clear();
        return result;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int CalculateItemOnHandQty(string companyCode, string TransID, string TransNo, string RetailID)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);

        MySqlParameter[] objParam =  {
                        dataAccessor.InputParam("pID", MySqlDbType.VarChar,200,TransID.ToString()),
                        dataAccessor.InputParam("pTransNo",MySqlDbType.VarChar,30,TransNo.ToString()),   
                        dataAccessor.InputParam("pRetailID",MySqlDbType.Int32,25,RetailID.ToString())
                   };

        int intresult = 0;
        try
        {
            intresult = dataAccessor.Exec_SPNonReturn("RecalculateOnHandQtyBySales", objParam);
            return intresult;
        }
        catch (MySqlException ex)
        {
            string errMessage = ("ERROR SOURCE: "
                        + (ex.Source + "<br/>"));
            errMessage = (errMessage + (" ERROR MESSAGE: "
                        + (ex.Message + "<br/>")));
            errMessage = (errMessage + (" ERROR CODE: "
                        + (ex.ErrorCode.ToString() + "<br/>")));
            errMessage = (errMessage + (" ERROR CODE: "
                        + (ex.StackTrace.ToString() + "<br/>")));

            return 0;
        }
        finally
        {
            dataAccessor = null;
        }

        return intresult;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int UpdateVoucher(string companyCode, string serialno, string status, string value)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        List<string[]> terminalList = getAllTerminals(dataAccessor, "0");

        string criteria = " WHERE SerialNo=@SerialNo";
        string field = "";
        status = status.ToLower();
        if (status == "sold")
        {
            field = "bitSold";
        }
        else if (status == "redeem")
        {
            field = "bitRedeem";
        }
        int result = 0;
        if (field != "")
        {
            string strSql = "UPDATE inventory_voucher SET " + field + "=@Values " + criteria;
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@SerialNo", serialno),
                new MySqlParameter("@Values", value)
            };
            result = dataAccessor.Exec_UpdateQuery_Vapt(strSql, "", objparam);
            SendMessages(dataAccessor, companyCode, "inventory_voucher", criteria, terminalList);
        }
        return result;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int UpdateItemCollection(string companyCode, string TransNo, string ID, string status)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
        dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        List<string[]> terminalList = getAllTerminals(dataAccessor, "0");

        string criteria = " WHERE TransNo=@TransNo";
        int result = 0;

        string strSql = "UPDATE tblitemcollection SET ItemStatus=@ItemStatus " + criteria;
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@ItemStatus", status),
            new MySqlParameter("@TransNo", TransNo)
        };
        result = dataAccessor.Exec_UpdateQuery_Vapt(strSql, "", objparam);
        SendMessages(dataAccessor, companyCode, "tblitemcollection", criteria, terminalList);

        return result;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string[] GetAllFiles(string companyID, string retailID, string terminalID)
    {
        System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        System.Configuration.KeyValueConfigurationElement msmqSetting = rootWebConfig1.AppSettings.Settings["MSMQ_CONFIG"];

        ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
        configMap.ExeConfigFilename = msmqSetting.Value;
        Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
        string POSGATEPath = config.AppSettings.Settings["LocalOutFolder"].Value;
        string fileDirPath = POSGATEPath + companyID + "\\" + retailID + "\\" + terminalID;
        string[] fileEntries = Directory.GetFiles(fileDirPath);
        return fileEntries;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string BackupFile(string companyID, string retailID, string terminalID, string fileName)
    {
        System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        System.Configuration.KeyValueConfigurationElement msmqSetting = rootWebConfig1.AppSettings.Settings["MSMQ_CONFIG"];

        ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
        configMap.ExeConfigFilename = msmqSetting.Value;
        Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

        string POSGATEPath = config.AppSettings.Settings["LocalOutFolder"].Value;
        string POSGATEBackupPath = config.AppSettings.Settings["BackupSentFolder"].Value;
        string mode = config.AppSettings.Settings["BackupMode"].Value.ToLower();
        string filePath = companyID + "\\" + retailID + "\\" + terminalID;
        string sourcePath = POSGATEPath + filePath;
        string sourceFilePath = sourcePath + "\\" + fileName;
        string destPath = POSGATEBackupPath + filePath;
        string destFilePath = destPath + "\\" + fileName;
        string result = "FAIL to move/delete " + sourceFilePath;
        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }
        if (File.Exists(sourceFilePath))
        {
            if (mode == "move")
            {
                File.Move(sourceFilePath, destFilePath);
                result = "File Moved to " + destFilePath;
            }
            else if (mode == "delete")
            {
                File.Delete(sourceFilePath);
                result = "File Deleted : " + sourceFilePath;
            }
        }
        else
        {
            return "File does not exist";
        }
        return result;
        /*System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        System.Configuration.KeyValueConfigurationElement msmqSetting = rootWebConfig1.AppSettings.Settings["MSMQ_CONFIG"];
        string strSQL = "";
        try
        {
            string path = Path.GetTempFileName();
            FileStream fs = File.Open("GG" + path, FileMode.Open);
            return "TRY";
        }
        catch (Exception ex1)
        {
            try
            {
                return "CATCH-TRY";
            }
            catch (Exception ex2)
            {
                return "CATCH-CATCH";
            }
        }
        finally
        {
            strSQL += "FINALLY";
            Console.Write(strSQL);
        }
        return strSQL;*/
    }

    private void createMsmqQueue(string path_queue)
    {
        if (!MessageQueue.Exists(path_queue))
        {
            MessageQueue.Create(path_queue);
        }
    }

    public List<string[]> getAllTerminals(clsDataAccessor dataAccessor, string retailID)
    {
        List<string[]> arr_terminals = new List<string[]>();
        string sqlCriteria = "";
        if (retailID != "0")
        {
            sqlCriteria = " AND RetailerID=@RetailerID";
        }
        string sql = "SELECT RetailerID, TerminalID FROM definitions_terminal WHERE `Display`='Y' AND RecordStatus<>'DELETED'";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@RetailerID", retailID)
        };
        DataSet ds_Databaseinfor = dataAccessor.RunSPRetDataset_Vapt(sql + sqlCriteria, "definitions_terminal", objparam);

        string[] result = new string[2];
        foreach (DataRow dsTableRow in ds_Databaseinfor.Tables[0].Rows)
        {
            result = new string[2];
            result[0] = dsTableRow["RetailerID"].ToString();
            result[1] = dsTableRow["TerminalID"].ToString();
            arr_terminals.Add(result);
        }

        return arr_terminals;
    }

    public ArrayList GenerateMessages(clsDataAccessor dataAccessor, string tableName, string strCriteria)
    {
        ArrayList arr_mess = new ArrayList();
        try
        {
            bool gotRecord = true;
            int page = 1;
            int limit = 5;
            while (gotRecord)
            {
                string sql = "";

                if (strCriteria == "")
                {
                    sql = "SELECT * FROM " + FunctionHelper.GetSetting(tableName) + " WHERE PendingSync='Y' LIMIT " + ((page - 1) * limit) + ", " + limit;
                }
                else
                {
                    sql = "SELECT * FROM " + FunctionHelper.GetSetting(tableName) + " " + FunctionHelper.GetSetting(strCriteria);
                }
                DataSet ds_salesdata = new DataSet();
                ds_salesdata = dataAccessor.RunSPRetDataset(sql, tableName);
                if (ds_salesdata != null && ds_salesdata.Tables != null && ds_salesdata.Tables[0] != null && ds_salesdata.Tables[0].Rows.Count > 0)
                {
                    string mess = clsXmlTransfer.CDataToXml(ds_salesdata);
                    arr_mess.Add(mess);

                    if (strCriteria == "")
                    {
                        page++;
                    }
                    else
                    {
                        gotRecord = false;
                    }
                }
                else
                {
                    gotRecord = false;
                }
                ds_salesdata.Clear();
                ds_salesdata.Dispose();
            }
        }
        catch (Exception ex)
        {
            return null;
        }
        return arr_mess;
    }

    public bool SendMessages(clsDataAccessor dataAccessor, string companyname, string tableName, string strCriteria, List<string[]> terminalList)
    {
        try
        {
            ArrayList arr_mess = GenerateMessages(dataAccessor, tableName, strCriteria);

            if (arr_mess.Count > 0)
            {

                List<MessageQueue> myQueue = new List<MessageQueue>();
                if (terminalList.Count > 0)
                {
                    string queuename, retailer, terminal;
                    string[] retailerTerminal;
                    int terminalListCount = terminalList.Count;
                    for (int i = 0; i < terminalListCount; i++)
                    {
                        retailerTerminal = terminalList[i].ToArray();
                        if (retailerTerminal.Length > 0)
                        {
                            retailer = retailerTerminal[0].ToString();
                            terminal = retailerTerminal[1].ToString();
                            queuename = companyname + "_" + retailer.PadLeft(3, '0') + "_" + terminal.PadLeft(2, '0') + "_OUT";
                            myQueue.Add(openMessageQueue(queuename));
                        }
                    }
                }

                System.Messaging.Message myMessage = new System.Messaging.Message();
                int arr_messCount = arr_mess.Count;
                for (int i = 0; i < arr_messCount; i++)
                {
                    string mess = arr_mess[i].ToString();
                    if (mess.Length > 0)
                    {
                        myMessage.Label = "Generate_Server_API";
                        myMessage.Body = mess;
                        myMessage.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                        int myQueueCount = myQueue.Count;
                        for (int m = 0; m < myQueueCount; m++)
                        {
                            myQueue[m].Send(myMessage);
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private MessageQueue openMessageQueue(string queuename)
    {
        MessageQueue myQueue;
        string path_queue = @".\private$\" + queuename;

        if (!MessageQueue.Exists(path_queue))
        {
            MessageQueue.Create(path_queue);
            myQueue = new MessageQueue(path_queue);
            myQueue.DefaultPropertiesToSend.Recoverable = true;
            myQueue.SetPermissions("Everyone", MessageQueueAccessRights.FullControl);
            myQueue.SetPermissions("ANONYMOUS LOGON", MessageQueueAccessRights.FullControl);
        }
        else
        {
            myQueue = new MessageQueue(path_queue);
        }
        return myQueue;
    }

    private bool runMSMQ(string companyCode, string retailID, string terminalID, string mode, string tables)
    {
        bool results = false;
        System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["MSMQAPP"];
        string MSMQPath = customSetting.Value;

        string paths = MSMQPath;
        if (tables != "")
        {
            tables = " " + tables;
        }
        if (File.Exists(paths))
        {
            string argsstr = mode + tables + " " + companyCode + " " + retailID + " " + terminalID;
            Process myprocess = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(paths, argsstr);
            myprocess.StartInfo = startInfo;
            myprocess.StartInfo.UseShellExecute = true;
            myprocess.Start();

            myprocess.WaitForExit();
            if (myprocess.HasExited)
            {
                results = true;
            }
        }
        else
        {
            results = false;
        }
        return results;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UploadFileService()
    {
        // Create a http request to the server endpoint that will pick up the
        // file and file description.
        HttpWebRequest requestToServerEndpoint =
        (HttpWebRequest)WebRequest.Create("http://localhost:8057/upload.aspx");
        //(HttpWebRequest)WebRequest.Create("http://172.16.1.213:8057/upload.aspx");

        string boundaryString = "----SomeRandomText";
        //string fileUrl = @"C:\DCS\Svr.txt";
        string fileUrl = @"C:\POSGATEWAY\Out\TweeSvr.zip";

        // Set the http request header \\
        // Turn off the buffering of data to be written, to prevent
        // OutOfMemoryException when sending data
        requestToServerEndpoint.AllowWriteStreamBuffering = false;
        requestToServerEndpoint.Method = WebRequestMethods.Http.Post;
        requestToServerEndpoint.ContentType = "multipart/form-data; boundary=" + boundaryString;
        requestToServerEndpoint.KeepAlive = false;
        requestToServerEndpoint.Credentials = System.Net.CredentialCache.DefaultCredentials;

        // Use a MemoryStream to form the post data request,
        // so that we can get the content-length attribute.
        MemoryStream postDataStream = new MemoryStream();
        StreamWriter postDataWriter = new StreamWriter(postDataStream);

        // Include value from the myFileDescription text area in the post data
        postDataWriter.Write("\r\n--" + boundaryString + "\r\n");
        postDataWriter.Write("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}",
        "myFileDescription",
        "A sample file description");

        // Include the file in the post data
        postDataWriter.Write("\r\n--" + boundaryString + "\r\n");
        postDataWriter.Write("Content-Disposition: form-data;"
        + "name=\"{0}\";"
        + "filename=\"{1}\""
        + "\r\nContent-Type: {2}\r\n\r\n",
        "myFile",
        Path.GetFileName(fileUrl),
        Path.GetExtension(fileUrl));
        postDataWriter.Flush();

        // Read the file
        FileStream fileStream = new FileStream(fileUrl, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[1024];
        int bytesRead = 0;
        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
        {
            postDataStream.Write(buffer, 0, bytesRead);
        }
        fileStream.Close();

        postDataWriter.Write("\r\n--" + boundaryString + "--\r\n");
        postDataWriter.Flush();

        // Set the http request body content length
        requestToServerEndpoint.ContentLength = postDataStream.Length;

        // Dump the post data from the memory stream to the request stream
        using (Stream s = requestToServerEndpoint.GetRequestStream())
        {
            postDataStream.WriteTo(s);
        }
        postDataStream.Close();
        // Grab the response from the server. WebException will be thrown
        // when a HTTP OK status is not returned
        WebResponse response = requestToServerEndpoint.GetResponse();
        StreamReader responseReader = new StreamReader(response.GetResponseStream());
        string replyFromServer = responseReader.ReadToEnd();
        return replyFromServer;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string UploadLargeFileService()
    {
        // Create a http request to the server endpoint that will pick up the
        // file and file description.
        HttpWebRequest requestToServer =
        (HttpWebRequest)WebRequest.Create("http://localhost:8057/upload.aspx");
        //(HttpWebRequest)WebRequest.Create("http://172.16.1.213:8057/upload.aspx");

        string boundaryString = "----SomeRandomText";
        string fileUrl = @"C:\POSGATEWAY\Out\TweeSvr.zip";
        string companyID = "DCS";
        string retailID = "1";
        string terminalID = "1";

        // Set the http request header \\
        // Turn off the buffering of data to be written, to prevent
        // OutOfMemoryException when sending data
        requestToServer.AllowWriteStreamBuffering = false;
        requestToServer.Method = WebRequestMethods.Http.Post;
        requestToServer.ContentType = "multipart/form-data; boundary=" + boundaryString;
        requestToServer.KeepAlive = false;
        requestToServer.Credentials = System.Net.CredentialCache.DefaultCredentials;

        ASCIIEncoding ascii = new ASCIIEncoding();
        string boundaryStringLine = "\r\n--" + boundaryString + "\r\n";
        byte[] boundaryStringLineBytes = ascii.GetBytes(boundaryStringLine);
        //CompanyID
        string companyIDContentDisposition = String.Format(
            "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}",
            "CompanyID",
            companyID);
        byte[] companyIDContentDispositionBytes
            = ascii.GetBytes(companyIDContentDisposition);
        //RetailID
        string retailIDContentDisposition = String.Format(
            "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}",
            "RetailID",
            retailID);
        byte[] retailIDContentDispositionBytes
            = ascii.GetBytes(retailIDContentDisposition);
        //TerminalID
        string terminalIDContentDisposition = String.Format(
            "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}",
            "TerminalID",
            terminalID);
        byte[] terminalIDContentDispositionBytes
            = ascii.GetBytes(terminalIDContentDisposition);

        string lastBoundaryStringLine = "\r\n--" + boundaryString + "--\r\n";
        byte[] lastBoundaryStringLineBytes = ascii.GetBytes(lastBoundaryStringLine);

        // Get the byte array of the myFileDescription content disposition
        string myFileDescriptionContentDisposition = String.Format(
            "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}",
            "myFileDescription",
            "A sample file description");
        byte[] myFileDescriptionContentDispositionBytes
            = ascii.GetBytes(myFileDescriptionContentDisposition);

        // Get the byte array of the string part of the myFile content
        // disposition
        string myFileContentDisposition = String.Format(
            "Content-Disposition: form-data;name=\"{0}\"; "
             + "filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n",
            "myFile", Path.GetFileName(fileUrl), Path.GetExtension(fileUrl));
        byte[] myFileContentDispositionBytes =
            ascii.GetBytes(myFileContentDisposition);

        FileInfo fileInfo = new FileInfo(fileUrl);

        // Calculate the total size of the HTTP request
        long totalRequestBodySize = boundaryStringLineBytes.Length * 5
            + lastBoundaryStringLineBytes.Length
            + myFileDescriptionContentDispositionBytes.Length
            + companyIDContentDispositionBytes.Length
            + retailIDContentDispositionBytes.Length
            + terminalIDContentDispositionBytes.Length
            + myFileContentDispositionBytes.Length
            + fileInfo.Length;
        // And indicate the value as the HTTP request content length
        requestToServer.ContentLength = totalRequestBodySize;

        // Write the http request body directly to the server
        using (Stream s = requestToServer.GetRequestStream())
        {
            // Send the file description content disposition over to the server
            s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
            s.Write(myFileDescriptionContentDispositionBytes, 0,
                myFileDescriptionContentDispositionBytes.Length);

            // Send the file description content disposition over to the server
            s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
            s.Write(companyIDContentDispositionBytes, 0,
                companyIDContentDispositionBytes.Length);

            // Send the file description content disposition over to the server
            s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
            s.Write(retailIDContentDispositionBytes, 0,
                retailIDContentDispositionBytes.Length);

            // Send the file description content disposition over to the server
            s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
            s.Write(terminalIDContentDispositionBytes, 0,
                terminalIDContentDispositionBytes.Length);

            // Send the file content disposition over to the server
            s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
            s.Write(myFileContentDispositionBytes, 0,
                myFileContentDispositionBytes.Length);

            // Send the file binaries over to the server, in 1024 bytes chunk
            FileStream fileStream = new FileStream(fileUrl, FileMode.Open,
                FileAccess.Read);
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                s.Write(buffer, 0, bytesRead);
            } // end while
            fileStream.Close();

            // Send the last part of the HTTP request body
            s.Write(lastBoundaryStringLineBytes, 0, lastBoundaryStringLineBytes.Length);
        } // end using

        // Grab the response from the server. WebException will be thrown
        // when a HTTP OK status is not returned
        WebResponse response = requestToServer.GetResponse();
        StreamReader responseReader = new StreamReader(response.GetResponseStream());
        string replyFromServer = responseReader.ReadToEnd();
        return replyFromServer;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string ExecuteSQL(string companyCode,string sql, string dbname, string tblName)
    {
        clsDataAccessor dataAccessor = new clsDataAccessor();
		dataAccessor.connectionstring = dataAccessor.getConnectionString_Vapt(dataAccessor, companyCode);
        //dataAccessor.connectionstring = "SERVER=localhost;Database=" + dbname + ";UID=root;PASSWORD=mlp098;CharSet=utf8;Convert Zero Datetime=True;";

        string[] stringSeparators = new string[] { ";" };
        string[] sqlArr;
		string blnstatus ="Success";
		int intRetVal =0;
		string strMessage="";
		
		string queryarr = FunctionHelper.GetSetting(sql);
        sqlArr = sql.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);	
		
        try
        {
			//dataAccessor.Exec_UpdateQuery(sql, "");
            foreach (string strSql in sqlArr)
            {
                dataAccessor.Exec_UpdateQuery(strSql, "");
            }					
        }
        catch (Exception e)
        {			
            blnstatus= "Fail";
        }
		
        /*finally
        {
            if(tblName.ToLower() =="stocktransfer")
                dataAccessor.Exec_UpdateQuery("UPDATE stocktransfer_detail STD SET ID=(SELECT ID FROM stocktransfer WHERE SERVERID=STD.SERVERID) WHERE ID=0;", "");
			
			blnstatus="Success";
			/*if(intRetVal==1)
				blnstatus="Success";
			else
				blnstatus="Fail";
			*/
        //} 
		//blnstatus
        
		return blnstatus;		
    }

    private string test(ArrayList arrServerID)
    {
        if (arrServerID.Count <= 0)
        {
            return "LESS";
        }
        else
        {
            return "MORE";
        }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string PostStockTransfer(String ID)
    {
        // Create a http request to the server endpoint that will pick up the
        // file and file description.
        HttpWebRequest requestToServer =
        (HttpWebRequest)WebRequest.Create("http://49.128.61.175:8081/myRetailerPlus/web/stocktransfer/post?ID=" + ID + "&api=true");

        requestToServer.Method = "POST";

        // Grab the response from the server. WebException will be thrown
        // when a HTTP OK status is not returned
        WebResponse response = requestToServer.GetResponse();
        StreamReader responseReader = new StreamReader(response.GetResponseStream());
        string replyFromServer = responseReader.ReadToEnd();
        return replyFromServer;
    }
}

