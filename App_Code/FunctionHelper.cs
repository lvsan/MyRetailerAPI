using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

public class FunctionHelper
{
    public string Get_Auto_Custcode(clsDataAccessor objDA, MySqlTransaction objtran)
    {
        string cust_auto_num = "";
        string custautono = "";
        DataSet ds_cust = new DataSet();

        string sql = "SELECT VCHPREFIXCUST, CUSTSTARTNO,CUSTNEXTNO, BITCUSTCODEEQUALICNO FROM CUSTOMER_DEFINITIONS WHERE DISPLAY = 'Y'";
        ds_cust = objDA.ExecuteDataSet(objtran, CommandType.Text, sql, null);

        if (ds_cust.Tables[0].Rows.Count > 0)
        {
            int runNo = int.Parse(ds_cust.Tables[0].Rows[0]["CUSTSTARTNO"].ToString()) + int.Parse(ds_cust.Tables[0].Rows[0]["CUSTNEXTNO"].ToString()) + 1;
            custautono = ds_cust.Tables[0].Rows[0]["VCHPREFIXCUST"].ToString() + (runNo).ToString();
        }

        ds_cust.Clear();
        ds_cust.Dispose();

        return cust_auto_num;
    }
	
    public string getTransCount(clsDataAccessor objDA, MySqlTransaction objtran, string StoreNo, string TerminalNo)
    {
        string sql_queuenodigit = "SELECT FUNCTIONVALUE FROM DEFINITIONS_GENERALSETTING WHERE FUNCTIONFIELDNAME = 'NOOFDIGITQUEUENO' AND RETAILERID=@RetailerID";
        int queuedigit = 4;

        MySqlParameter[] objparam =
            {
                new MySqlParameter("@RetailerID", StoreNo),
                new MySqlParameter("@TerminalID", TerminalNo)
            };

        try
        {
            queuedigit = int.Parse(objDA.ExecuteScalar(objtran, CommandType.Text, sql_queuenodigit, objparam).ToString());
        }
        catch (Exception e)
        {
            queuedigit = 4;
        }


        string storenostr = int.Parse(StoreNo).ToString("00");
        int seqlength = queuedigit - 2;

        string tostringzerostr = "";
        string maxnum = "1";

        for (int i = 0; i < seqlength; i++)
        {
            tostringzerostr += "0";
        }

        maxnum += tostringzerostr;

        string sqlstr = "SELECT LastQueueNo FROM DEFINITIONS_TERMINAL WHERE RETAILERID=@RetailerID AND TerminalID=@TerminalID AND DISPLAY = 'Y' ";

        int lastno = 0;

        try
        {
            lastno = int.Parse(objDA.ExecuteScalar(objtran, CommandType.Text, sqlstr, objparam).ToString());
        }
        catch (System.Exception ex)
        {
            lastno = 0;
        }

        lastno += 1;

        if (lastno == int.Parse(maxnum))
        {
            string sql_forqueueno = "UPDATE DEFINITIONS_TERMINAL SET LastQueueNo = '0' WHERE DISPLAY = 'Y' AND RETAILERID=@RetailerID AND TERMINALID=@TerminalID";
            objDA.ExecuteNonQuery(objtran, CommandType.Text, sql_forqueueno, objparam);

            lastno = 1;
        }

        string queuestr = storenostr + lastno.ToString(tostringzerostr);

        return queuestr;
    }

    public string Get_CustType(clsDataAccessor objDA, MySqlTransaction objtran)
    {
        string custypeid = "";

        DataSet ds_cust = new DataSet();

        string sqlstr = "SELECT CUSTTYPEID FROM CUSTOMER_TYPE WHERE BLNDEFAULT = 'Y' AND bitFrontendLock='N' and RecordStatus<>'DELETED' and Display='Y'";
        try
        {
            custypeid = objDA.ExecuteScalar(objtran, CommandType.Text, sqlstr, null).ToString();
        }
        catch (System.Exception ex)
        {
            custypeid = "";
        }

        return custypeid;
    }

    public static string GetSetting(string s)
    {
        string result = CryptoClass.DESDecrypt(s, "12345678");
        return result;
    }
}
