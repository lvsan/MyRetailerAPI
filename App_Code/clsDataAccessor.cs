﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;

public class clsDataAccessor
{
    public string connectionstring;

    public int varOutputRet;
    //Initialize Datatable Globally
    public DataTable pDt = new DataTable();

    private int intresult;
    public string message = " ";
	public string dbname = "";
    public string test;

    public clsDataAccessor()
    {
        System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["ConnectionString"];
        connectionstring = customSetting.Value;
    }

    public clsDataAccessor(string constr)
    {
        connectionstring = constr;
    }

    public MySqlConnection OpenDB()
    {
        MySqlConnection retConn = null;
        try
        {
            retConn = new MySqlConnection(connectionstring);
            retConn.Open();
            return retConn;
        }
        catch
        {
            return retConn;
        }
    }

    public MySqlConnection getLocalCon()
    {
        MySqlConnection retConn = null;
        retConn = new MySqlConnection(connectionstring);
        return retConn;
    }


    protected void CloseDB(MySqlConnection objconn)
    {
        if ((objconn != null))
        {
            if (objconn.State == ConnectionState.Open)
            {
                objconn.Close();
                objconn = null;
            }
        }
    }

    public MySqlParameter InputParam(string strParamName, MySqlDbType oParamType, int oParamSize, object objValue)
    {
        return CreateParam(strParamName, oParamType, oParamSize, ParameterDirection.Input, objValue);
    }

    public MySqlParameter OutputParam(string strParamName, MySqlDbType oParamType, int oParamSize)
    {
        object ttemp = null;
        return CreateParam(strParamName, oParamType, oParamSize, ParameterDirection.Output, ttemp);
    }

    private MySqlParameter CreateParam(string strParamName, MySqlDbType oParamType, int oParamSize, ParameterDirection oParamDirection, object objValue)
    {
        MySqlParameter objParam = null;

        if (oParamSize > 0)
        {
            //Instantiate a new parameter object with size
            objParam = new MySqlParameter(strParamName, oParamType, oParamSize);
        }
        else
        {
            //Instantiate a new parameter object without size
            objParam = new MySqlParameter(strParamName, oParamType);
        }
        //Assign Parameter direction
        objParam.Direction = oParamDirection;

        if (!(oParamDirection == ParameterDirection.Output & objValue == null))
        {
            //This is an input parameter assigns a value to the objParam object
            objParam.Value = objValue;
        }

        return objParam;
    }

    public MySqlDataReader RunPassSQL(string strSQL, string strDatabase)
    {
        MySqlDataReader objDR = null;
        MySqlConnection objCnn = new MySqlConnection();
        objCnn = OpenDB();
        //Instantiate a new command object
        MySqlCommand objCmd = new MySqlCommand(strSQL.Trim(), objCnn);
        objCmd.CommandType = CommandType.Text;
        try
        {
            objDR = objCmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
        catch (MySqlException exdb)
        {
            
        }
        finally
        {
            objCmd.Dispose();
            objCnn.Close();
        }

        return objDR;
    }

    protected MySqlCommand CreateSP(string strCmdName, params MySqlParameter[] ParamSP)
    {
        MySqlCommand objSQLCmd = null;
        MySqlConnection objConn = OpenDB();

        objSQLCmd = new MySqlCommand(strCmdName.Trim(), objConn);
        objSQLCmd.CommandType = CommandType.StoredProcedure;

        MySqlParameter objParam = null;
        foreach (MySqlParameter objParam_loopVariable in ParamSP)
        {
            objParam = objParam_loopVariable;
            objSQLCmd.Parameters.Add(objParam);
        }
		
        return objSQLCmd;
    }

    public int Exec_StoreProcedure(string strSPName, params MySqlParameter[] Params)
    {
        int intRetVal = 0;

        //Instantiate a new command object
        MySqlCommand objCmd = CreateSP(strSPName.Trim(), Params);
        try
        {
            intRetVal = objCmd.ExecuteNonQuery();
        }
        catch (MySqlException exdb)
        {
            throw exdb;
        }
        finally
        {
            //Assign the return value of output parameter from an executed stored procedure
            varOutputRet = Convert.ToInt32(objCmd.Parameters[0].Value);
            //Deallocate command object
            objCmd.Dispose();
        }
        return intRetVal;
    }

    public int Exec_SPNonReturn(string strSPName, params MySqlParameter[] Params)
    {
        int intRetVal = 0;

        //Instantiate a new command object
        MySqlCommand objCmd = CreateSP(strSPName.Trim(), Params);
        try
        {
            intRetVal = objCmd.ExecuteNonQuery();
        }
        catch (MySqlException exdb)
        {
            throw exdb;
        }
        finally
        {
            intRetVal = 1;
            //Assign the return value of output parameter from an executed stored procedure
          //  varOutputRet = Convert.ToInt32(objCmd.Parameters[0].Value);
            //Deallocate command object
            objCmd.Dispose();
        }
        return intRetVal;
    }

    public int Exec_UpdateQuery(string strSQL, string strDatabase)
    {
        MySqlConnection objCnn = new MySqlConnection();
        int intRetVal = 0;
        objCnn = OpenDB();
        //Instantiate a new command object
        MySqlCommand objCmd = new MySqlCommand();
        try
        {
            objCmd.Connection = objCnn;
            objCmd.CommandText = strSQL.Trim();
            objCmd.CommandType = CommandType.Text;
            objCmd.CommandTimeout = 5000000;
            intRetVal = objCmd.ExecuteNonQuery();
        }
        catch (MySqlException exdb)
        {
            //errMessage = ("ERROR SOURCE: "
            //                                + (exdb.Source + "<br/>"));
            //errMessage = (errMessage + (" ERROR MESSAGE: "
            //            + (exdb.Message + "<br/>")));
            //errMessage = (errMessage + (" ERROR CODE: "
            //            + (exdb.ErrorCode.ToString() + "<br/>")));
            //errMessage = (errMessage + (" ERROR CODE: "
            //                            + (exdb.StackTrace.ToString() + "<br/>")));
 			objCnn.Close();
            throw exdb;
        }
        finally
        {
            objCnn.Close();
            objCnn.Dispose();
            objCmd.Dispose();
        }
        return intRetVal;
    }

    public int Exec_UpdateQuery_Vapt(string strSQL, string strDatabase, MySqlParameter[] objparam)
    {
        MySqlConnection objCnn = new MySqlConnection();
        int intRetVal = 0;
        objCnn = OpenDB();
        //Instantiate a new command object
        MySqlCommand objCmd = PrepareCommand(objCnn, CommandType.Text, strSQL, objparam);
        try
        {
            intRetVal = objCmd.ExecuteNonQuery();
        }
        catch (MySqlException exdb)
        {
            objCnn.Close();
            throw exdb;
        }
        finally
        {
            objCnn.Close();
            objCnn.Dispose();
            objCmd.Dispose();
        }
        return intRetVal;
    }

    public int Exec_InsertQuery(string strSQL, string strDatabase)
    {
        MySqlConnection objCnn = new MySqlConnection();
        int intRetVal = 0;
        objCnn = OpenDB();

        //Instantiate a new command object
        MySqlCommand objCmd = new MySqlCommand();

        try
        {
            objCmd.Connection = objCnn;
            objCmd.CommandText = strSQL.Trim();
            objCmd.CommandType = CommandType.Text;
            objCmd.CommandTimeout = 5000000;
            intRetVal = objCmd.ExecuteNonQuery();
        }
        catch (MySqlException exdb)
        {
            //throw new System.Exception("An error has occurred " + exdb.ToString());
        }
        finally
        {
            //Deallocate command object            
            objCmd.Dispose();
            objCnn.Close();
			objCnn.Dispose();
        }
        return intRetVal;
    }

    public object ExecScalarVal_Vapt(string strSQL, string strDatabase, MySqlParameter[] objparam)
    {
        MySqlConnection objCnn = new MySqlConnection();

        string errMessage = "";

        objCnn = OpenDB();

        MySqlCommand objCmd = PrepareCommand(objCnn, CommandType.Text, strSQL, objparam);

        object obj = null;
        if (objCnn != null && objCnn.State == ConnectionState.Open)
        {
            try
            {
                obj = objCmd.ExecuteScalar();
            }
            catch (MySqlException exdb)
            {
                errMessage = ("ERROR SOURCE: " + (exdb.Source + "<br/>"));
                errMessage = (errMessage + (" ERROR MESSAGE: " + (exdb.Message + "<br/>")));
                errMessage = (errMessage + (" ERROR CODE: " + (exdb.ErrorCode.ToString() + "<br/>")));
                errMessage = (errMessage + (" ERROR CODE: " + (exdb.StackTrace.ToString() + "<br/>")));
            }
            finally
            {
                //Deallocate command object
                objCnn.Close();
            }
        }
        else
        {
            objCmd.Dispose();
        }
        objCnn.Dispose();
        objCmd.Dispose();

        return obj;
    }
    public object ExecScalarVal(string strSQL, string strDatabase)
    {
        MySqlConnection objCnn = new MySqlConnection();

        string errMessage = "";

        objCnn = OpenDB();

        MySqlCommand objCmd = new MySqlCommand(strSQL.Trim(), objCnn);
        objCmd.CommandType = CommandType.Text;
        object obj = null;
        if (objCnn != null && objCnn.State == ConnectionState.Open)
        {
            try
            {
                obj = objCmd.ExecuteScalar();
            }
            catch (MySqlException exdb)
            {
                errMessage = ("ERROR SOURCE: " + (exdb.Source + "<br/>"));
                errMessage = (errMessage + (" ERROR MESSAGE: " + (exdb.Message + "<br/>")));
                errMessage = (errMessage + (" ERROR CODE: " + (exdb.ErrorCode.ToString() + "<br/>")));
                errMessage = (errMessage + (" ERROR CODE: " + (exdb.StackTrace.ToString() + "<br/>")));
            }
            finally
            {
                //Deallocate command object
                objCnn.Close();
            }
        }
        else
        {
            objCmd.Dispose();
        }
        objCnn.Dispose();
        objCmd.Dispose();

        return obj;
    }

    public DataSet RunSPRetDataset(string strSQL, string strTblName)
    {
        MySqlConnection objCnn = new MySqlConnection();

        objCnn = OpenDB();

        //Instantiate a new DataSet object
        DataSet objDS = new DataSet();

        //Instantiate a new DataAdapter object
        MySqlDataAdapter objDA = new MySqlDataAdapter(strSQL.Trim(), objCnn);
        objDA.SelectCommand.CommandType = CommandType.Text;
        //Populates the DataSet object
        try
        {
            objDA.Fill(objDS, strTblName);
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
            objCnn.Close();
        }

        //Deallocate DataAdapter object and cut database connection
        CloseDB(objCnn);
        objDA.Dispose();
        objCnn.Dispose();

        return objDS;
    }

    public DataSet RunSPRetDataset_Vapt(string strSQL, string strTblName, MySqlParameter[] objparam)
    {
        MySqlConnection objCnn = new MySqlConnection();
        objCnn = OpenDB();

        //Instantiate a new DataSet object
        DataSet objDS = new DataSet();

        MySqlCommand cmd = PrepareCommand(objCnn, CommandType.Text, strSQL, objparam);

        //Instantiate a new DataAdapter object
        MySqlDataAdapter objDA = new MySqlDataAdapter(cmd);
        //Populates the DataSet object
        try
        {
            objDA.Fill(objDS, strTblName);
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
            objCnn.Close();
        }

        //Deallocate DataAdapter object and cut database connection
        CloseDB(objCnn);
        objDA.Dispose();
        objCnn.Dispose();

        return objDS;
    }
    	
	private MySqlCommand PrepareCommand(MySqlConnection conn, CommandType cmdType, string cmdText, MySqlParameter[] cmdParms)
    {
        MySqlCommand cmd = new MySqlCommand();
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }
        cmd.Connection = conn;

        cmd.CommandText = cmdText;
        cmd.CommandTimeout = 120000;
        cmd.CommandType = cmdType;

        if (cmdParms != null)
        {

            foreach (MySqlParameter parm in cmdParms)
            {
                cmd.Parameters.Add(parm);
            }
        }

        return cmd;

    }

    private void PrepareCommand(MySqlConnection conn, MySqlTransaction trans, MySqlCommand cmd, CommandType cmdType, string cmdText, MySqlParameter[] cmdParms)
    {

        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }
        cmd.Connection = conn;

        cmd.CommandText = cmdText;
        cmd.CommandTimeout = 120000;
        if (trans != null)

            cmd.Transaction = trans;

        cmd.CommandType = cmdType;

        if (cmdParms != null)
        {

            foreach (MySqlParameter parm in cmdParms)
            {
                cmd.Parameters.Add(parm);
            }
        }
    }

    public int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] cmdParms)
    {
        MySqlCommand cmd = new MySqlCommand();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            PrepareCommand(conn, null, cmd, cmdType, cmdText, cmdParms);

            int val = cmd.ExecuteNonQuery();
            if (cmd.Parameters != null && cmd.Parameters.Count > 0)
            {
                varOutputRet = Convert.ToInt32(cmd.Parameters[0].Value);
            }
            cmd.Parameters.Clear();

            return val;
        }
    }

    public int ExecuteNonQuery(MySqlConnection conn, CommandType cmdType, string cmdText, params MySqlParameter[] cmdParms)
    {

        MySqlCommand cmd = new MySqlCommand();

        PrepareCommand(conn, null, cmd, cmdType, cmdText, cmdParms);

        int val = cmd.ExecuteNonQuery();

        try
        {
            if (cmd.Parameters != null && cmd.Parameters.Count > 0)
            {
                varOutputRet = Convert.ToInt32(cmd.Parameters[0].Value);
            }
        }
        catch (Exception ex)
        {
            varOutputRet = 0;
        }
        cmd.Parameters.Clear();

        return val;
    }


    public int ExecuteNonQuery(MySqlTransaction trans, CommandType cmdType, string cmdText, params MySqlParameter[] cmdParms)
    {

        MySqlCommand cmd = new MySqlCommand();

        PrepareCommand(trans.Connection, trans, cmd, cmdType, cmdText, cmdParms);

        int val = cmd.ExecuteNonQuery();

        if (cmd.Parameters != null && cmd.Parameters.Count > 0)
        {
            varOutputRet = Convert.ToInt32(cmd.Parameters[0].Value);
        }
        cmd.Parameters.Clear();

        return val;
    }

    public object ExecuteScalar(MySqlTransaction trans, CommandType cmdType, string cmdText, params MySqlParameter[] cmdParms)
    {
        MySqlCommand cmd = new MySqlCommand();

        PrepareCommand(trans.Connection, trans, cmd, cmdType, cmdText, cmdParms);

        object val = cmd.ExecuteScalar();

        if (cmd.Parameters != null && cmd.Parameters.Count > 0)
        {
            varOutputRet = Convert.ToInt32(cmd.Parameters[0].Value);
        }
        cmd.Parameters.Clear();

        return val;

    }

    public DataSet ExecuteDataSet(MySqlTransaction trans, CommandType cmdType, string cmdText, params MySqlParameter[] cmdParms)
    {
        MySqlCommand cmd = new MySqlCommand();

        PrepareCommand(trans.Connection, trans, cmd, cmdType, cmdText, cmdParms);

        DataSet ds = new DataSet();

        MySqlDataAdapter sda = new MySqlDataAdapter(cmd);

        sda.Fill(ds);

        cmd.Parameters.Clear();

        return ds;

    }

    public string getConnectionString_Vapt(clsDataAccessor dataAccessor, string companyCode)
    {
        string sql = "SELECT COMPID, COMPNAME, `SERVER`, `DATABASE`, UID, `PASSWORD`,PORTNO,SPV05 AS COMUUID FROM DEFINITIONS_XMLCOMP WHERE ACTIVE = 'Y' AND compName=@compName";
        MySqlConnection objCnn = new MySqlConnection();
        objCnn = OpenDB();

        MySqlParameter[] objParam =
                {
                    new MySqlParameter("@compName",  companyCode )
                 };

        MySqlCommand objcmd = PrepareCommand(objCnn, CommandType.Text, sql, objParam);
        DataSet ds_Databaseinfor = new DataSet();
        try
        {

            MySqlDataAdapter sda = new MySqlDataAdapter(objcmd);
            sda.Fill(ds_Databaseinfor);

        }
        catch (MySqlException ex)
        {
            ds_Databaseinfor = new DataSet();
        }

        string servername = "", databasename = "", userid = "", password = "", portno = "";
        if (ds_Databaseinfor == null || ds_Databaseinfor.Tables.Count == 0 || ds_Databaseinfor.Tables[0].Rows.Count == 0)
        {
            dbname = "";
            return "";
        }
        else
        {
            DataRow dsRow = ds_Databaseinfor.Tables[0].Rows[0];
            servername = dsRow["SERVER"].ToString();
            databasename = dsRow["DATABASE"].ToString();
            userid = dsRow["UID"].ToString();
            password = dsRow["PASSWORD"].ToString();
            portno = dsRow["PORTNO"].ToString();
            dbname = databasename;
            return "SERVER=" + servername + ";Database=" + databasename + ";UID=" + userid + ";PASSWORD=" + password + ";Port=" + portno + ";CharSet=utf8;Convert Zero Datetime=True;Allow User Variables=True;";
        }
    }

	public List<ItemPrice> GetPrices(string ItemID, string RetailID)
    {
        List<ItemPrice> prices = new List<ItemPrice>();
        
        string sqlstr = "SELECT inventory_unit.Item_UnitID as Item_UnitID,(SELECT Nick FROM list_units WHERE ID= ItemUnit) AS UOM , inventory_unit.RTLSellPx as Price, inventory_unit.ItemUnitDef as ItemUnitDef, IF(inventory_unit.ItemUnitDef='Y',inventory_retail.OnHandQty,0) AS OnHandQty " +
                                " FROM  inventory_unit " +
                                " LEFT JOIN inventory_retail ON inventory_unit.itemID = inventory_retail.ItemID" +
                                " WHERE inventory_unit.RecordStatus<>'DELETED' AND inventory_unit.ItemID=@ItemID AND inventory_retail.RetailID=@RetailID ORDER BY inventory_unit.ItemID ASC, inventory_unit.ItemUnitDef DESC";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@ItemID", ItemID),
            new MySqlParameter("@RetailID", RetailID)
        };

        DataTable dt = GetData_Vapt(sqlstr, objparam);

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            prices.Add(new ItemPrice
            {
                Item_UnitID = Convert.ToString(dt.Rows[i]["Item_UnitID"])
                ,
                UOM = Convert.ToString(dt.Rows[i]["UOM"])
                ,
                RTLSellPx = Convert.ToDecimal(dt.Rows[i]["Price"])
                ,
                ItemUnitDef = Convert.ToString(dt.Rows[i]["ItemUnitDef"])
                ,
                OnHandQty = Convert.ToString(dt.Rows[i]["OnHandQty"])
            });
        }
        return prices;
    }
	
	public List<Supplier> GetSuppliers(string ItemID)
    {
        List<Supplier> Suppliers = new List<Supplier>();

        string sqlstr = "SELECT SupplierID, SupBarCode, DefaultSupplier " +
                                " FROM  inventory_supbar WHERE recordstatus<>'DELETED' AND ItemID=@ItemID ORDER BY SupplierID ASC, DefaultSupplier DESC ";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@ItemID", ItemID)
        };
        DataTable dt = GetData_Vapt(sqlstr, objparam);
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            Suppliers.Add(new Supplier
            {
                SupplierID = Convert.ToString(dt.Rows[i]["SupplierID"])
                ,
                SupBarCode = Convert.ToString(dt.Rows[i]["SupBarCode"])
                ,
                DefSupplier = Convert.ToString(dt.Rows[i]["DefaultSupplier"])
            });
        }
        return Suppliers;
    }
	
	public string checkItem(string ItemID)
    {
        string sqlstr = "SELECT ItemID FROM  inventory  WHERE RecordStatus <> 'DELETED' AND ItemID=@ItemID";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@ItemID", ItemID)
        };
        DataTable dt = GetData_Vapt(sqlstr, objparam);
        if (dt.Rows.Count == 0)
        {
            return ItemID + " not found.";
        }
        else
        {
            return "OK";
        }
    }
	
	public string checkItem (string ItemID, string UOM)
    {
        string sqlstr = "SELECT inventory.ItemID " +
                                " FROM  inventory " +
                                " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                                " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                                " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID=@ItemID AND list_units.Nick=@UOM";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@ItemID", ItemID),
            new MySqlParameter("@UOM", UOM)
        };

        DataTable dt = GetData_Vapt(sqlstr, objparam);

        if (dt.Rows.Count == 0)
        {
            return ItemID + " with " + UOM + " not found.";
        }
        else
        {
            return "OK";
        }
    }

    public string checkPayment(string payment)
    {
        string sqlstr = "SELECT ID FROM LIST_PAYMENTMETHODS WHERE RECORDSTATUS <> 'DELETED' AND NICK=@Nick";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@Nick", payment)
        };
        DataTable dt = GetData_Vapt(sqlstr, objparam);
        if (dt.Rows.Count == 0)
        {
            return "Payment (" + payment + ") not found.";
        }
        else
        {
            return "OK";
        }
    }

/*
    public string saveSales(SalesMaster sales)
    {
        //Retail sales
        Guid SalesID = Guid.NewGuid();
        decimal SalesBalTtl = 0;
        decimal TotalChangeAmt = 0;

        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        InventoryAgings aging = new InventoryAgings();
        string agingJson = @"{""ItemAging"":[";
        if (sales.TransNo == null)
        {
            return "Missing detail: Trans No";
        }
        else if (sales.RetailID == null)
        {
            return "Missing detail: Retail ID";
        }
        else if (sales.SalesDate == null)
        {
            return "Missing detail: Sales Date";
        }
        else if (sales.SalesStatus == null)
        {
            return "Missing detail: Sales Status";
        }
        else if (sales.SalesTaxTtl == null)
        {
            return "Missing detail: Sales Tax Total";
        }
        else if (sales.SalesRounding == null)
        {
            return "Missing detail: Sales Rounding";
        }
        else if (sales.ItemSales.Count() == 0)
        {
            return "Missing item sales detail.";
        }
        else if (sales.SalesPayments.Count() == 0)
        {
            return "Missing sales payment detail.";
        }
        else
        {
            DataTable SalesDT = GetData(string.Format("SELECT *" +
                            " FROM retail_sales " +
                            " WHERE RecordStatus <> 'DELETED' AND SalesNo = '{0}' AND RetailID = '{1}' AND SalesStatus = '{2}' AND SalesDate = '{3}'", sales.TransNo.ToString(), sales.RetailID.ToString(), sales.SalesStatus.ToString(), sales.SalesDate.ToString("yyyy-MM-dd")));
            if (SalesDT.Rows.Count == 0)
            {
                for (int i = 0; i < sales.ItemSales.Count(); i++)
                {
                    Guid SalesDetailID = Guid.NewGuid();
 					string strRetailID = sales.RetailID.ToString();

                    //response = checkItem(sales.ItemSales[i].ItemID.ToString(), sales.ItemSales[i].ItemUOMDesc.ToString());
                    //if (response == "OK")
                    //{
                    if (sales.ItemSales[i].ItemQty == 0 || sales.ItemSales[i].ItemQty == null)
                    {
                        return "Wrong item qty.";
                        break;
                    }
                    else
                    {
                        SalesBalTtl = SalesBalTtl + sales.ItemSales[i].ItemTotal;
                        string ItemUOM = sales.ItemSales[i].ItemUOMDesc.ToString();
						string ItemID = sales.ItemSales[i].ItemID.ToString();
                        string ItemSKU = "";
                        string SupplierID = "";
                        decimal ItemActQty = 0;
                        decimal ItemUnitCost = 0;
                        decimal ItemAveCost = 0;
                        string supBarcode = "";
                        string ItemPoint = "N";
                        string currency = "";
                        decimal ExchRate = 1;
                        string ItemBaseUOMID = "";
                        string ItemBaseUOM = "";
                        decimal ItemBaseActQty = 1;
                        decimal baseQty = 1;
						decimal SellPrice = sales.ItemSales[i].ItemPrice;
                        if (sales.ItemSales[i].ItemID.ToString() != "")
                        {
                            DataTable ItemDT = GetData(string.Format("SELECT inventory.ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode, inventory.ItemDescp," +
                            "inventory_unit.ItemUnit AS ItemUOM, list_units.Nick AS ItemUOMDesc, inventory_unit.ItemActQty, inventory_unit.RTLSellPx AS ItemUnitPrice," +
                            "inventory_unit.PurchaseCost AS ItemUnitCost, inventory.ItemAveCost, inventory.ItemSKUSup AS SupplierID, inventory.ItemCategory AS CategoryID," +
                            "inventory.ItemDepartment AS DepartmentID, inventory.ItemGroup AS GroupID, inventory.ItemBrand as BrandID, inventory.ItemPoint " +
                            " FROM inventory " +
                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                            " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID = '{0}' AND list_units.Nick = '{1}'" +
                            " LIMIT 1", sales.ItemSales[i].ItemID.ToString(), sales.ItemSales[i].ItemUOMDesc.ToString()));

                            if (ItemDT.Rows.Count == 0)
                            {
                                return "Item : " + sales.ItemSales[i].ItemID.ToString() + " with UOM :" + sales.ItemSales[i].ItemUOMDesc.ToString() + " not found";
                            }

                            ItemID = Convert.ToString(ItemDT.Rows[0]["ItemID"]);
                            ItemSKU = Convert.ToString(ItemDT.Rows[0]["ItemSKU"]);
                            ItemUOM = Convert.ToString(ItemDT.Rows[0]["ItemUOM"]);
                            ItemActQty = Convert.ToDecimal(ItemDT.Rows[0]["ItemActQty"]);
                            ItemUnitCost = Convert.ToDecimal(ItemDT.Rows[0]["ItemUnitCost"]);
                            ItemAveCost = Convert.ToDecimal(ItemDT.Rows[0]["ItemAveCost"]);
                            SupplierID = Convert.ToString(ItemDT.Rows[0]["SupplierID"]);
                            ItemPoint = Convert.ToString(ItemDT.Rows[0]["ItemPoint"]);

                            if (sales.ItemSales[i].SupBarCode == null)
                            {
                                supBarcode = Convert.ToString(ItemDT.Rows[0]["supBarcode"]);
                            }
                            else
                            {
                                supBarcode = sales.ItemSales[i].SupBarCode.ToString();
                            }

                            DataTable DefaultDT = GetData("SELECT DefaultCurrency, exchange_rate.ExchRate" +
                            " FROM definitions LEFT JOIN exchange_rate ON definitions.DefaultCountry = exchange_rate.CountryID AND definitions.DefaultCurrency = exchange_rate.ExchCurr");

                            currency = Convert.ToString(DefaultDT.Rows[0]["DefaultCurrency"]);
                            ExchRate = Convert.ToDecimal(DefaultDT.Rows[0]["ExchRate"]);

                            DataTable BaseUOMDT = GetData(string.Format("SELECT iu.ItemUnit as ItemBaseUOMID, iu.ItemActQty as ItemBaseActQty, lu.Nick AS ItemBaseUOM" +
                            " FROM inventory_unit iu" +
                            " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                            " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemUnitDef='Y' AND iu.ItemID = '{0}'", ItemID));

                            if (BaseUOMDT.Rows.Count == 0)
                            {
                                BaseUOMDT = GetData(string.Format("SELECT iu.ItemUnit as ItemBaseUOMID, iu.ItemActQty as ItemBaseActQty, lu.Nick AS ItemBaseUOM" +
                                        " FROM inventory_unit iu" +
                                        " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                        " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemActQty=1 AND iu.ItemID = '{0}'", ItemID));
                            }

                            ItemBaseUOMID = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOMID"]);
                            ItemBaseUOM = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOM"]);
                            ItemBaseActQty = Convert.ToDecimal(BaseUOMDT.Rows[0]["ItemBaseActQty"]);

                        }

                        baseQty = sales.ItemSales[i].ItemQty * ItemActQty;
                        Guid AgingID = Guid.NewGuid();

                        //inventory_aging
                        agingJson = agingJson + "{";
                        agingJson = agingJson + string.Format(@"""ID"":""{0}"",""SupplierID"":""{1}"",""RetailID"":""{2}"",""ItemID"":""{3}"",""ItemSKU"":""{4}"",
                                        ""TransID"":""{5}"",""TransNo"":""{6}"",""TransDate"":""{7}"",""ItemUOMID"":""{8}"",""ItemUOM"":""{9}"",""ItemBaseUOMID"":""{10}"",""ItemBaseUOM"":""{11}"",
                                        ""Qty"":{12},""ItemActualQty"":{13},""CurrencyID"":""{14}"",""ExcRate"":{15},""CostUnitPx"":{16},""LocalCostUnitPx"":{17},""CreateTime"":""{18}"",""BatchNo"":"""",
                                        ""HSCode"":"""",""ExpireDate"":"""",""ExpiryDay"":0,""ItemDefActualQty"":{19},""PDQty"":0,""SoldQty"":{20},""TrfInQty"":0,""TrfOutQty"":0,""AdjQty"":0,""RetQty"":0,""SDQty"":0,""KitQty"":0,
                                        ""DekitQty"":0,""ReserveQty"":0,""InTransitQty"":0,""QtyBalance"":0,""RFID"":"""",""SellPrice"":{21}", AgingID, SupplierID, strRetailID, sales.ItemSales[i].ItemID.ToString(), ItemSKU,
                                        SalesID, sales.TransNo, sales.SalesDate, ItemUOM, sales.ItemSales[i].ItemUOMDesc, ItemBaseUOMID, ItemBaseUOM,
                                        0, ItemActQty, currency, ExchRate, ItemUnitCost, ItemUnitCost, sales.SalesDate, ItemBaseActQty, sales.ItemSales[i].ItemQty,SellPrice);
                        agingJson = agingJson + "},";
                        //inventory_aging
                        
                        string queryInsertItem = "INSERT INTO retail_sales_detail " +
                            "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                            " VALUE " +
                            "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";

                        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                        {
                            try
                            {
                                objCnn.Open();
                                using (MySqlCommand cmd = new MySqlCommand(queryInsertItem, objCnn))
                                {
                                    cmd.Parameters.AddWithValue("@DetailID", SalesDetailID);
                                    cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                    cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                    cmd.Parameters.AddWithValue("@ItemID", sales.ItemSales[i].ItemID.ToString());
                                    //cmd.Parameters.AddWithValue("@SupBarCode", Convert.ToString(ItemDT.Rows[0]["SupBarCode"]));
                                    cmd.Parameters.AddWithValue("@SupBarCode", supBarcode.ToString());
                                    cmd.Parameters.AddWithValue("@ItemQty", sales.ItemSales[i].ItemQty);
                                    cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                    cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                    cmd.Parameters.AddWithValue("@ItemQtyAct", ItemActQty);
                                    cmd.Parameters.AddWithValue("@ItemUnitPrice", SellPrice);
                                    cmd.Parameters.AddWithValue("@ItemUnitCost", ItemUnitCost);
                                    cmd.Parameters.AddWithValue("@ItemAveCost", ItemAveCost);
                                    cmd.Parameters.AddWithValue("@ItemDiscAmt", 0);
                                    cmd.Parameters.AddWithValue("@ItemSubTotal", sales.ItemSales[i].ItemTotal - sales.ItemSales[i].ItemTax);
                                    cmd.Parameters.AddWithValue("@ItemTaxTotal", sales.ItemSales[i].ItemTax);
                                    cmd.Parameters.AddWithValue("@ItemTotal", sales.ItemSales[i].ItemTotal);
                                    cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                                    cmd.Parameters.AddWithValue("@CollectionRetailID", sales.RetailID.ToString());
                                    cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                    cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                    cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                    cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                    cmd.ExecuteNonQuery();
                                }
                                if (sales.ItemSales[i].ItemDisc != 0)
                                {
                                    Guid SalesDetailID2 = Guid.NewGuid();
                                    string queryInsertDiscount = "INSERT IGNORE INTO retail_sales_detail " +
                                            "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                            " VALUE " +
                                            "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                                    try
                                    {
                                        using (MySqlCommand cmd = new MySqlCommand(queryInsertDiscount, objCnn))
                                        {
                                            cmd.Parameters.AddWithValue("@DetailID", SalesDetailID2);
                                            cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                            cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                            cmd.Parameters.AddWithValue("@ItemID", 0);
                                            cmd.Parameters.AddWithValue("@SupBarCode", 0);
                                            cmd.Parameters.AddWithValue("@ItemQty", 1);
                                            cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                            cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                            cmd.Parameters.AddWithValue("@ItemQtyAct", 1);
                                            cmd.Parameters.AddWithValue("@ItemUnitPrice", 0);
                                            cmd.Parameters.AddWithValue("@ItemUnitCost", 0);
                                            cmd.Parameters.AddWithValue("@ItemAveCost", 0);
                                            cmd.Parameters.AddWithValue("@ItemDiscAmt", sales.ItemSales[i].ItemDisc);
                                            cmd.Parameters.AddWithValue("@ItemSubTotal", 0);
                                            cmd.Parameters.AddWithValue("@ItemTaxTotal", 0);
                                            cmd.Parameters.AddWithValue("@ItemTotal", 0);
                                            cmd.Parameters.AddWithValue("@SupplierID", 0);
                                            cmd.Parameters.AddWithValue("@CollectionRetailID", sales.RetailID.ToString());
                                            cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                            cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        return ex.ToString();
                                    }

                                }
                                //return "Success";
                            }
                            catch (Exception ex)
                            {
                                return ex.ToString();
                            }
                        }
                    }
                    //}
                    //else
                    //{
                    //    return response;
                    //}
                }
				agingJson = agingJson.Remove(agingJson.Length - 1) + "]}";

                for (int i = 0; i < sales.SalesPayments.Count(); i++)
                {
                    Guid SalesPaymentID = Guid.NewGuid();

                    //response = checkPayment(sales.SalesPayments[i].strPayment.ToString());
                    //if (response != "OK")
                    //{
                    //    return response;
                    //    break;
                    //}
                    //else
                    //{
                    TotalChangeAmt = TotalChangeAmt + sales.SalesPayments[i].ChangeAmount;
                    //DataTable PaymentDT = GetData(string.Format("SELECT ID" +
                    //            " FROM list_paymentmethods " +
                    //            " WHERE RecordStatus <> 'DELETED' AND Nick = '{0}' AND ID='{1}'", sales.SalesPayments[i].strPayment.ToString(), sales.SalesPayments[i].paymentID.ToString()));
                    DataTable PaymentDT = GetData(string.Format("SELECT ID" +
                                " FROM list_paymentmethods " +
                                " WHERE RecordStatus <> 'DELETED' AND SPV05='{0}' OR ID='{0}'", sales.SalesPayments[i].paymentID.ToString()));

                    string queryInsertPayment = "INSERT INTO retail_sales_payment " +
                        " (SalesPaymentID, SalesID, RetailID, PaymentID, SalesPayTtl, SalesBalTtl, ChangeAmount, Close_RetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                        " VALUE " +
                        " (@SalesPaymentID, @SalesID, @RetailID, @PaymentID, @SalesPayTtl, @SalesBalTtl, @ChangeAmount, @Close_RetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsertPayment, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@SalesPaymentID", SalesPaymentID);
                                cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                //cmd.Parameters.AddWithValue("@PaymentID", sales.SalesPayments[i].paymentID.ToString());
                                cmd.Parameters.AddWithValue("@PaymentID", Convert.ToString(PaymentDT.Rows[0]["ID"]));
                                cmd.Parameters.AddWithValue("@SalesPayTtl", sales.SalesPayments[i].SalesPayTtl);
                                cmd.Parameters.AddWithValue("@SalesBalTtl", sales.SalesPayments[i].SalesBalTtl);
                                cmd.Parameters.AddWithValue("@ChangeAmount", sales.SalesPayments[i].ChangeAmount);
                                cmd.Parameters.AddWithValue("@Close_RetailID", sales.RetailID.ToString());
                                cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                cmd.ExecuteNonQuery();
                            }

                            //return "Success";
                        }
                        catch (Exception ex)
                        {
                            return ex.ToString();
                        }
                    }
                    //}
                }
                DataTable Sales2DT = GetData("SELECT DefaultGST,DefaultGSTVal" +
                                    " FROM definitions ");
                string queryInsertSales = "INSERT INTO retail_sales " +
                            " (SalesID, RetailID, SalesNo, SalesTax, SalesTaxVal, SalesDate, CloseRetailID, CloseDate, CloseTime, SalesStatus, SalesSubTtl, SalesTaxTtl, SalesBalTtl, SalesPayTtl, SalesChangeAmt, SalesRounding, CreateTime, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                            " VALUE " +
                            " (@SalesID, @RetailID, @SalesNo, @SalesTax, @SalesTaxVal, @SalesDate, @CloseRetailID, @CloseDate, @CloseTime, @SalesStatus, @SalesSubTtl, @SalesTaxTtl, @SalesBalTtl, @SalesPayTtl, @SalesChangeAmt, @SalesRounding, @CreateTime, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsertSales, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@SalesID", SalesID);
                            cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                            cmd.Parameters.AddWithValue("@SalesNo", sales.TransNo.ToString());
                            cmd.Parameters.AddWithValue("@SalesTax", Convert.ToString(Sales2DT.Rows[0]["DefaultGST"]));
                            cmd.Parameters.AddWithValue("@SalesTaxVal", Convert.ToDecimal(Sales2DT.Rows[0]["DefaultGSTVal"]));
                            cmd.Parameters.AddWithValue("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@CloseRetailID", sales.RetailID.ToString());
                            cmd.Parameters.AddWithValue("@CloseDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@CloseTime", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@SalesStatus", sales.SalesStatus.ToString());
                            cmd.Parameters.AddWithValue("@SalesSubTtl", SalesBalTtl - sales.SalesTaxTtl);
                            cmd.Parameters.AddWithValue("@SalesTaxTtl", sales.SalesTaxTtl);
                            cmd.Parameters.AddWithValue("@SalesBalTtl", SalesBalTtl);
                            cmd.Parameters.AddWithValue("@SalesPayTtl", SalesBalTtl);
                            cmd.Parameters.AddWithValue("@SalesChangeAmt", TotalChangeAmt);
                            cmd.Parameters.AddWithValue("@SalesRounding", sales.SalesRounding);
                            cmd.Parameters.AddWithValue("@CreateTime", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@PendingSync", "Y");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                            cmd.ExecuteNonQuery();
                        }
                        try
                        {
                            aging = serializer.Deserialize<InventoryAgings>(agingJson);
                        }
                        catch (Exception ex)
                        {
                            return ex.ToString();
                        }
                        SaveInventoryAging(aging);

                        return new JavaScriptSerializer().Serialize(new { Status = "Success", SalesID = SalesID });

                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }
                }

            }
            else
            {
                return "Error: Duplicate sales record.";
            }
        }
    }
	*/
	
	public decimal getSubtotalDue(SalesMaster sales)
    {
        decimal totalamt = 0;
        for (int i = 0; i < sales.ItemSales.Count(); i++)
        {
            totalamt += sales.ItemSales[i].ItemTotal;
        }
        return totalamt;
    }
	
    public void SaveDisc(string salesid, string itemdetailid, string discdetailid, string disccategory, string discid, string retailid, string terminalid, int disclevel, decimal subtotaldue, decimal discamount, decimal totaldiscamt)
    {
        string disctype = "";
        string sql_type = "Select DiscType from list_discount_sales where ID=@DiscID";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@DiscID", discid)
        };
        DataTable disctypedt = GetData_Vapt(sql_type, objparam);
        if (disctypedt.Rows.Count > 0)
            disctype = disctypedt.Rows[0]["DiscType"].ToString();

        string sql = "Insert into inventory_tran_discount(ID, SalesID, ItemDetailID, DiscDetailID, RetailID, TerminalID, DiscCategory, DiscType, DiscLevel, DiscID, DiscAmount, TotalDiscAmount, PendingSync, LastUpdate,LockUpdate, RecordStatus, RecordUpdate, QueueStatus) values ";
        sql += " (@ID, @SalesID, @ItemDetailID, @DiscDetailID, @RetailID, @TerminalID, @DiscCategory, @DiscType, @DiscLevel, @DiscID, @DiscAmount, @TotalDiscAmount, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus) ";
        Guid discountuuid = Guid.NewGuid();
        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
        {
            try
            {
                objCnn.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, objCnn))
                {
                    cmd.Parameters.AddWithValue("@ID", discountuuid);
                    cmd.Parameters.AddWithValue("@SalesID", salesid);
                    cmd.Parameters.AddWithValue("@ItemDetailID", itemdetailid);
                    cmd.Parameters.AddWithValue("@DiscDetailID", discdetailid);
                    cmd.Parameters.AddWithValue("@RetailID", retailid);
                    cmd.Parameters.AddWithValue("@TerminalID", terminalid);
                    cmd.Parameters.AddWithValue("@DiscCategory", disccategory);
                    cmd.Parameters.AddWithValue("@DiscType", disctype);
                    cmd.Parameters.AddWithValue("@DiscLevel", disclevel);
                    cmd.Parameters.AddWithValue("@DiscID", discid);
                    cmd.Parameters.AddWithValue("@DiscAmount", discamount);
                    cmd.Parameters.AddWithValue("@TotalDiscAmount", totaldiscamt * -1);
                    cmd.Parameters.AddWithValue("@PendingSync", "Y");
                    cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                    cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {

            }
        }
    }


    public string saveSales(List<SalesMaster> list_sales)
    {
        string transnoarray = "";
        for (int x2 = 0; x2 < list_sales.Count; x2++)
        {
            SalesMaster sales = (SalesMaster)list_sales[x2];
            //Retail sales
            Guid SalesID = Guid.NewGuid();
            decimal SalesBalTtl = 0;
            decimal TotalChangeAmt = 0;

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            InventoryAgings aging = new InventoryAgings();
            string agingJson = @"{""ItemAging"":[";
            if (sales.TransNo == null)
            {
                return "Missing detail: Trans No";
            }
            else if (sales.RetailID == null)
            {
                return "Missing detail: Retail ID";
            }
            else if (sales.SalesDate == null)
            {
                return "Missing detail: Sales Date";
            }
            else if (sales.SalesStatus == null)
            {
                return "Missing detail: Sales Status";
            }
            else if (sales.ItemSales.Count() == 0)
            {
                return "Missing item sales detail.";
            }
            else if (sales.SalesPayments.Count() == 0)
            {
                return "Missing sales payment detail.";
            }
            else
            {
				bool blnContinue = true;
				string salesno = "";
				string salesstatus="";
				if (sales.SalesStatus == "VOID")
				{
					salesno = sales.TransNo + "V";
					salesstatus = sales.SalesStatus.ToString();
				}
				else
				{
					salesno = sales.TransNo;
					salesstatus=sales.SalesStatus.ToString();
				}
								
				string strSQL = "select * from retail_sales where SalesNo=@SalesNo and SalesStatus=@SalesStatus AND SalesDate=@SalesDate AND RetailID=@RetailID";
				MySqlParameter[] objparam =
				{
					new MySqlParameter("@SalesNo", sales.TransNo.ToString()),
					new MySqlParameter("@RetailID", sales.RetailID.ToString()),
					new MySqlParameter("@SalesStatus", "SALES"),
					new MySqlParameter("@SalesDate",  sales.SalesDate.ToString("yyyy-MM-dd"))
				};
				DataTable SalesDT = GetData_Vapt(strSQL, objparam);					
				if (SalesDT.Rows.Count == 0)
				{
					if (sales.SalesStatus == "VOID")
					{
						string blnStatus = saveMissingSales(sales);
					}
				}else {
                    string sID = SalesDT.Rows[0]["SalesID"].ToString();
                    strSQL ="SELECT COUNT(*) as cnt FROM retail_sales_detail WHERE SalesID IN(@SalesID)";
                    MySqlParameter[] objparamSD =
				    {
					    new MySqlParameter("@SalesID", sID.ToString())
				    };
                    DataTable SalesDDT = GetData_Vapt(strSQL, objparamSD);
                    int sdcnt = int.Parse(SalesDDT.Rows[0]["cnt"].ToString());
                    SalesDDT.Clear();
                    SalesDDT.Dispose();

                    strSQL="SELECT COUNT(*) as cnt FROM retail_sales_payment WHERE SalesID IN(@SalesID)";
                    MySqlParameter[] objparamSP =
				    {
					    new MySqlParameter("@SalesID", sID.ToString())
				    };
                    DataTable SalesPDT = GetData_Vapt(strSQL, objparamSP);
                    int pdcnt = int.Parse(SalesPDT.Rows[0]["cnt"].ToString());
                    SalesPDT.Clear();
                    SalesPDT.Dispose();

                    if (sdcnt != 0 && pdcnt != 0)
                    {
                        blnContinue = false;
                    }
                    else
                    {
                        strSQL = "DELETE from retail_sales where SalesID IN(@SalesID)";
                        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                        {
                            try
                            {
                                objCnn.Open();
                                using (MySqlCommand cmd = new MySqlCommand(strSQL, objCnn))
                                {
                                    cmd.Parameters.AddWithValue("@SalesID", sID.ToString());
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                return ex.ToString();
                            }
                        }
                        blnContinue = true;
                    }
                }
				SalesDT.Dispose();
				SalesDT.Clear();
		
				strSQL = "select * from retail_sales where SalesNo=@SalesNo and SalesStatus=@SalesStatus AND SalesDate=@SalesDate AND RetailID=@RetailID";
				MySqlParameter[] objparam1 =
				{
					new MySqlParameter("@SalesNo", salesno.ToString()),
					new MySqlParameter("@RetailID", sales.RetailID.ToString()),
					new MySqlParameter("@SalesStatus", salesstatus.ToString()),
					new MySqlParameter("@SalesDate",  sales.SalesDate.ToString("yyyy-MM-dd"))
				};
				SalesDT = GetData_Vapt(strSQL, objparam1);	
				if (SalesDT.Rows.Count != 0)
				{
                    string vID = SalesDT.Rows[0]["SalesID"].ToString();
                    strSQL ="SELECT COUNT(*) as cnt FROM retail_sales_detail WHERE SalesID IN(@SalesID)";
                    MySqlParameter[] objparamSDV =
				    {
					    new MySqlParameter("@SalesID", vID.ToString())
				    };
                    DataTable SalesDDT = GetData_Vapt(strSQL, objparamSDV);
                    int sdcnt = int.Parse(SalesDDT.Rows[0]["cnt"].ToString());
                    SalesDDT.Clear();
                    SalesDDT.Dispose();

                    strSQL="SELECT COUNT(*) as cnt FROM retail_sales_payment WHERE SalesID IN(@SalesID)";
                    MySqlParameter[] objparamSPV =
				    {
					    new MySqlParameter("@SalesID", vID.ToString())
				    };
                    DataTable SalesPDT = GetData_Vapt(strSQL, objparamSPV);
                    int pdcnt = int.Parse(SalesPDT.Rows[0]["cnt"].ToString());
                    SalesPDT.Clear();
                    SalesPDT.Dispose();

                    if (sdcnt != 0 && pdcnt != 0)
                    {
                        blnContinue = false;
                    }
                    else
                    {
                        strSQL = "DELETE from retail_sales where SalesID IN(@SalesID)";
                        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                        {
                            try
                            {
                                objCnn.Open();
                                using (MySqlCommand cmd = new MySqlCommand(strSQL, objCnn))
                                {
                                    cmd.Parameters.AddWithValue("@SalesID", vID.ToString());
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                return ex.ToString();
                            }
                        }
                        blnContinue = true;
                    }
				}
				else
				{
					blnContinue = true;
				}
				SalesDT.Dispose();
				SalesDT.Clear();

				if (blnContinue == true)
				{
                    decimal temp_groupitemdiscamt = 0;
                    decimal groupdisc_perc = sales.SalesDiscPerc;
                    decimal groupdisc_amt = (sales.SalesDiscAmt == 0 ? sales.SalesTotalDiscount : sales.SalesDiscAmt);
                    decimal groupdisc_totaldue = getSubtotalDue(sales);
                    for (int i = 0; i < sales.ItemSales.Count(); i++)
                    {
                        Guid SalesDetailID = Guid.NewGuid();
                        Guid SalesDetailIDV = Guid.NewGuid();
                        string strRetailID = sales.RetailID.ToString();
						string sqlstr ="";
						
                        if (sales.ItemSales[i].ItemQty == 0)
                        {
                            return "Wrong item qty.";
                        }
                        else
                        {
                            SalesBalTtl = SalesBalTtl + sales.ItemSales[i].ItemTotal;
                            string ItemUOM = sales.ItemSales[i].ItemUOMDesc.ToString();
                            string ItemID = sales.ItemSales[i].ItemID.ToString();
                            string ItemSKU = "";
                            string SupplierID = "";
                            decimal ItemActQty = 0;
                            decimal ItemUnitCost = 0;
                            decimal ItemAveCost = 0;
                            string supBarcode = "";
                            string ItemPoint = "N";
                            string currency = "";
                            decimal ExchRate = 1;
                            string ItemBaseUOMID = "";
                            string ItemBaseUOM = "";
                            decimal ItemBaseActQty = 1;
                            decimal baseQty = 1;
                            decimal SellPrice = sales.ItemSales[i].ItemPrice;
                            if (sales.ItemSales[i].ItemID.ToString() != "")
                            {
                                sqlstr = "SELECT inventory.ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode, inventory.ItemDescp," +
                                "inventory_unit.ItemUnit AS ItemUOM, list_units.Nick AS ItemUOMDesc, inventory_unit.ItemActQty, inventory_unit.RTLSellPx AS ItemUnitPrice," +
                                "inventory_unit.PurchaseCost AS ItemUnitCost, inventory.ItemAveCost, inventory.ItemSKUSup AS SupplierID, inventory.ItemCategory AS CategoryID," +
                                "inventory.ItemDepartment AS DepartmentID, inventory.ItemGroup AS GroupID, inventory.ItemBrand as BrandID, inventory.ItemPoint " +
                                " FROM inventory " +
                                " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                                " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                                " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                                " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID=@ItemID AND list_units.Nick=@Nick LIMIT 1";

                                MySqlParameter[] objparam2 =
                                {
                                new MySqlParameter("@ItemID", sales.ItemSales[i].ItemID.ToString()),
                                new MySqlParameter("@Nick", sales.ItemSales[i].ItemUOMDesc.ToString())
                            };

                                DataTable ItemDT = GetData_Vapt(sqlstr, objparam2);

                                if (ItemDT.Rows.Count == 0)
                                {
                                    return "Item : " + sales.ItemSales[i].ItemID.ToString() + " with UOM :" + sales.ItemSales[i].ItemUOMDesc.ToString() + " not found";
                                }

                                ItemID = Convert.ToString(ItemDT.Rows[0]["ItemID"]);
                                ItemSKU = Convert.ToString(ItemDT.Rows[0]["ItemSKU"]);
                                ItemUOM = Convert.ToString(ItemDT.Rows[0]["ItemUOM"]);
                                ItemActQty = Convert.ToDecimal(ItemDT.Rows[0]["ItemActQty"]);
                                ItemUnitCost = Convert.ToDecimal(ItemDT.Rows[0]["ItemUnitCost"]);
                                ItemAveCost = Convert.ToDecimal(ItemDT.Rows[0]["ItemAveCost"]);
                                SupplierID = Convert.ToString(ItemDT.Rows[0]["SupplierID"]);
                                ItemPoint = Convert.ToString(ItemDT.Rows[0]["ItemPoint"]);

                                if (sales.ItemSales[i].SupBarCode == null)
                                {
                                    supBarcode = Convert.ToString(ItemDT.Rows[0]["supBarcode"]);
                                }
                                else
                                {
                                    supBarcode = sales.ItemSales[i].SupBarCode.ToString();
                                }

                                DataTable DefaultDT = GetData("SELECT DefaultCurrency, exchange_rate.ExchRate" +
                                " FROM definitions LEFT JOIN exchange_rate ON definitions.DefaultCountry = exchange_rate.CountryID AND definitions.DefaultCurrency = exchange_rate.ExchCurr");
                                currency = Convert.ToString(DefaultDT.Rows[0]["DefaultCurrency"]);
                                ExchRate = Convert.ToDecimal(DefaultDT.Rows[0]["ExchRate"]);

                                sqlstr = "SELECT iu.ItemUnit as ItemBaseUOMID, iu.ItemActQty as ItemBaseActQty, lu.Nick AS ItemBaseUOM" +
                                " FROM inventory_unit iu" +
                                " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemUnitDef='Y' AND iu.ItemID=@ItemID";

                                MySqlParameter[] objparam3 =
                                {
									new MySqlParameter("@ItemID", ItemID)
								};

                                DataTable BaseUOMDT = GetData_Vapt(sqlstr, objparam3);

                                if (BaseUOMDT.Rows.Count == 0)
                                {
                                    sqlstr = "SELECT iu.ItemUnit as ItemBaseUOMID, iu.ItemActQty as ItemBaseActQty, lu.Nick AS ItemBaseUOM" +
                                            " FROM inventory_unit iu" +
                                            " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                            " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemActQty=1 AND iu.ItemID=@ItemID";
                                    BaseUOMDT = GetData_Vapt(sqlstr, objparam3);
                                }

                                ItemBaseUOMID = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOMID"]);
                                ItemBaseUOM = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOM"]);
                                ItemBaseActQty = Convert.ToDecimal(BaseUOMDT.Rows[0]["ItemBaseActQty"]);

                            }

                            baseQty = sales.ItemSales[i].ItemQty * ItemActQty;
                            Guid AgingID = Guid.NewGuid();

                            //inventory_aging
                            agingJson = agingJson + "{";
                            agingJson = agingJson + string.Format(@"""ID"":""{0}"",""SupplierID"":""{1}"",""RetailID"":""{2}"",""ItemID"":""{3}"",""ItemSKU"":""{4}"",
                                        ""TransID"":""{5}"",""TransNo"":""{6}"",""TransDate"":""{7}"",""ItemUOMID"":""{8}"",""ItemUOM"":""{9}"",""ItemBaseUOMID"":""{10}"",""ItemBaseUOM"":""{11}"",
                                        ""Qty"":{12},""ItemActualQty"":{13},""CurrencyID"":""{14}"",""ExcRate"":{15},""CostUnitPx"":{16},""LocalCostUnitPx"":{17},""CreateTime"":""{18}"",""BatchNo"":"""",
                                        ""HSCode"":"""",""ExpireDate"":"""",""ExpiryDay"":0,""ItemDefActualQty"":{19},""PDQty"":0,""SoldQty"":{20},""TrfInQty"":0,""TrfOutQty"":0,""AdjQty"":0,""RetQty"":0,""SDQty"":0,""KitQty"":0,
                                        ""DekitQty"":0,""ReserveQty"":0,""InTransitQty"":0,""QtyBalance"":0,""RFID"":"""",""SellPrice"":{21}", AgingID, SupplierID, strRetailID, sales.ItemSales[i].ItemID.ToString(), ItemSKU,
                                            SalesID, salesno, sales.SalesDate, ItemUOM, sales.ItemSales[i].ItemUOMDesc, ItemBaseUOMID, ItemBaseUOM,
                                            0, ItemActQty, currency, ExchRate, ItemUnitCost, ItemUnitCost, sales.SalesDate, ItemBaseActQty, sales.ItemSales[i].ItemQty * ItemActQty, SellPrice);
                            agingJson = agingJson + "},";
                            //inventory_aging

                            decimal temp_itemgroupamt = 0;
                            if (i == sales.ItemSales.Length - 1)
                            {
                                temp_itemgroupamt = groupdisc_amt - temp_groupitemdiscamt;
                            }
                            else
                            {
                                if (groupdisc_perc > 0)
                                {
                                    temp_itemgroupamt = (sales.ItemSales[i].ItemTotal) * groupdisc_perc / 100;
                                    temp_itemgroupamt = Math.Round(temp_itemgroupamt, 2);
                                    temp_groupitemdiscamt += temp_itemgroupamt;
                                }
                                else
                                {
                                    temp_itemgroupamt = (sales.ItemSales[i].ItemTotal) / groupdisc_totaldue * groupdisc_amt;
                                    temp_itemgroupamt = Math.Round(temp_itemgroupamt, 2);
                                    temp_groupitemdiscamt += temp_itemgroupamt;
                                }
                            }

                            string queryInsertItem = "INSERT INTO retail_sales_detail " +
                                "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt,ItemDiscGroupAmt, GroupDiscPerc, GroupDiscPerc2, GroupDiscPerc3, GroupDiscAmt, GroupDiscAmt2, GroupDiscAmt3, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                " VALUE " +
                                "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemDiscGroupAmt, @GroupDiscPerc, @GroupDiscPerc2, @GroupDiscPerc3, @GroupDiscAmt, @GroupDiscAmt2, @GroupDiscAmt3, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";

                            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                            {
                                try
                                {
                                    objCnn.Open();
                                    using (MySqlCommand cmd = new MySqlCommand(queryInsertItem, objCnn))
                                    {
                                        cmd.Parameters.AddWithValue("@DetailID", SalesDetailID);
                                        cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                        cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                        cmd.Parameters.AddWithValue("@ItemID", sales.ItemSales[i].ItemID.ToString());
                                        //cmd.Parameters.AddWithValue("@SupBarCode", Convert.ToString(ItemDT.Rows[0]["SupBarCode"]));
                                        cmd.Parameters.AddWithValue("@SupBarCode", supBarcode.ToString());
                                        cmd.Parameters.AddWithValue("@ItemQty", sales.ItemSales[i].ItemQty);
                                        cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                        cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                        cmd.Parameters.AddWithValue("@ItemQtyAct", ItemActQty);
                                        cmd.Parameters.AddWithValue("@ItemUnitPrice", SellPrice);
                                        cmd.Parameters.AddWithValue("@ItemUnitCost", ItemUnitCost);
                                        cmd.Parameters.AddWithValue("@ItemAveCost", ItemAveCost);
                                        cmd.Parameters.AddWithValue("@ItemDiscAmt", 0);
                                        //cmd.Parameters.AddWithValue("@ItemDiscGroupAmt", sales.ItemSales[i].ItemDiscGroupAmt);
                                        cmd.Parameters.AddWithValue("@ItemDiscGroupAmt", groupdisc_perc > 0 ? temp_itemgroupamt *-1 : 0);
                                        cmd.Parameters.AddWithValue("@GroupDiscPerc", groupdisc_perc);
                                        cmd.Parameters.AddWithValue("@GroupDiscPerc2", 0);
                                        cmd.Parameters.AddWithValue("@GroupDiscPerc3", 0);
                                        cmd.Parameters.AddWithValue("@GroupDiscAmt", groupdisc_perc > 0 ? 0 : temp_itemgroupamt * -1);
                                        cmd.Parameters.AddWithValue("@GroupDiscAmt2", 0);
                                        cmd.Parameters.AddWithValue("@GroupDiscAmt3", 0);
                                        //cmd.Parameters.AddWithValue("@GroupDiscPerc2", sales.ItemSales[i].GroupDiscPerc2);
                                        //cmd.Parameters.AddWithValue("@GroupDiscPerc3", sales.ItemSales[i].GroupDiscPerc3);
                                        //                           cmd.Parameters.AddWithValue("@GroupDiscAmt", sales.ItemSales[i].GroupDiscAmt);
                                        //cmd.Parameters.AddWithValue("@GroupDiscAmt2", sales.ItemSales[i].GroupDiscAmt2);
                                        //cmd.Parameters.AddWithValue("@GroupDiscAmt3", sales.ItemSales[i].GroupDiscAmt3);
                                        cmd.Parameters.AddWithValue("@ItemSubTotal", sales.ItemSales[i].ItemTotal - sales.ItemSales[i].ItemTax);
                                        cmd.Parameters.AddWithValue("@ItemTaxTotal", sales.ItemSales[i].ItemTax);
                                        cmd.Parameters.AddWithValue("@ItemTotal", sales.ItemSales[i].ItemTotal);
                                        cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                                        cmd.Parameters.AddWithValue("@CollectionRetailID", sales.RetailID.ToString());
                                        cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                        cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                        cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                        cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                        cmd.ExecuteNonQuery();
                                    }

                                    if (sales.ItemSales[i].ItemDisc != 0)
                                    {
                                        Guid SalesDetailID2 = Guid.NewGuid();
                                        Guid SalesDetailID2V = Guid.NewGuid();
                                        string queryInsertDiscount = "INSERT IGNORE INTO retail_sales_detail " +
                                                "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt, ItemDiscGroupAmt, GroupDiscPerc, GroupDiscPerc2, GroupDiscPerc3, GroupDiscAmt, GroupDiscAmt2, GroupDiscAmt3, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                                " VALUE " +
                                                "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemDiscGroupAmt, @GroupDiscPerc, @GroupDiscPerc2, @GroupDiscPerc3, @GroupDiscAmt, @GroupDiscAmt2, @GroupDiscAmt3, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                                        try
                                        {
                                            using (MySqlCommand cmd = new MySqlCommand(queryInsertDiscount, objCnn))
                                            {
                                                cmd.Parameters.AddWithValue("@DetailID", SalesDetailID2);
                                                cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                                cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                                cmd.Parameters.AddWithValue("@ItemID", 0);
                                                cmd.Parameters.AddWithValue("@SupBarCode", 0);
                                                cmd.Parameters.AddWithValue("@ItemQty", 1);
                                                cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                                cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                                cmd.Parameters.AddWithValue("@ItemQtyAct", 1);
                                                cmd.Parameters.AddWithValue("@ItemUnitPrice", 0);
                                                cmd.Parameters.AddWithValue("@ItemUnitCost", 0);
                                                cmd.Parameters.AddWithValue("@ItemAveCost", 0);
                                                cmd.Parameters.AddWithValue("@ItemDiscAmt", sales.ItemSales[i].ItemDisc);

                                                cmd.Parameters.AddWithValue("@ItemDiscGroupAmt", 0);
                                                cmd.Parameters.AddWithValue("@GroupDiscPerc", 0);
                                                cmd.Parameters.AddWithValue("@GroupDiscPerc2", 0);
                                                cmd.Parameters.AddWithValue("@GroupDiscPerc3", 0);
                                                cmd.Parameters.AddWithValue("@GroupDiscAmt", 0);
                                                cmd.Parameters.AddWithValue("@GroupDiscAmt2", 0);
                                                cmd.Parameters.AddWithValue("@GroupDiscAmt3", 0);

                                                //                                 cmd.Parameters.AddWithValue("@ItemDiscGroupAmt", sales.ItemSales[i].ItemDiscGroupAmt);
                                                //cmd.Parameters.AddWithValue("@GroupDiscPerc", sales.ItemSales[i].GroupDiscPerc);
                                                //cmd.Parameters.AddWithValue("@GroupDiscPerc2", sales.ItemSales[i].GroupDiscPerc2);
                                                //cmd.Parameters.AddWithValue("@GroupDiscPerc3", sales.ItemSales[i].GroupDiscPerc3);
                                                //cmd.Parameters.AddWithValue("@GroupDiscAmt", sales.ItemSales[i].GroupDiscAmt);
                                                //cmd.Parameters.AddWithValue("@GroupDiscAmt2", sales.ItemSales[i].GroupDiscAmt2);
                                                //cmd.Parameters.AddWithValue("@GroupDiscAmt3", sales.ItemSales[i].GroupDiscAmt3);
                                                cmd.Parameters.AddWithValue("@ItemSubTotal", 0);
                                                cmd.Parameters.AddWithValue("@ItemTaxTotal", 0);
                                                cmd.Parameters.AddWithValue("@ItemTotal", 0);
                                                cmd.Parameters.AddWithValue("@SupplierID", 0);
                                                cmd.Parameters.AddWithValue("@CollectionRetailID", sales.RetailID.ToString());
                                                cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                                cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                                cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                                cmd.ExecuteNonQuery();

                                            }

                                            SaveDisc(SalesID.ToString(), SalesDetailID.ToString(), SalesDetailID2.ToString(), "L", sales.ItemSales[i].DiscID1, sales.RetailID, "1", 1, 0, sales.ItemSales[i].ItemDisc, 0);
                                        }
                                        catch (Exception ex)
                                        {
                                            return ex.ToString();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return ex.ToString();
                                }
                            }
                        }
                    }
                    agingJson = agingJson.Remove(agingJson.Length - 1) + "]}";

                    for (int i = 0; i < sales.SalesPayments.Count(); i++)
                    {
                        Guid SalesPaymentID = Guid.NewGuid();
                        Guid SalesPaymentIDV = Guid.NewGuid();

                        decimal SalesBalTtlPY = 0;
                        decimal saleschangeamt = 0;
                        if (sales.SalesPayments[i].SalesBalTtl == 0 && sales.SalesTotalAmount != 0)
                        {
                            SalesBalTtlPY = sales.SalesTotalAmount;
                            saleschangeamt = sales.SalesPayments[i].SalesPayTtl - sales.SalesTotalAmount;
                        }
                        else
                        {
                            SalesBalTtlPY = sales.SalesPayments[i].SalesBalTtl;
                            saleschangeamt = sales.SalesPayments[i].ChangeAmount;
                        }

                        TotalChangeAmt = TotalChangeAmt + saleschangeamt;

                        string sqlstr = "SELECT ID FROM list_paymentmethods WHERE RecordStatus <> 'DELETED' AND (SecondaryID = @PaymentID AND SPV05=@PaymentID) OR (ID=@PaymentID)";
						MySqlParameter[] objparam4 =
							{
								new MySqlParameter("@PaymentID", sales.SalesPayments[i].paymentID.ToString())
							};
						DataTable PaymentDT = GetData_Vapt(sqlstr, objparam4);
						string PayID ="";
						if (PaymentDT.Rows.Count > 1)
						{
							PaymentDT.Clear();

                            sqlstr = "SELECT ID FROM list_paymentmethods WHERE RecordStatus <> 'DELETED' AND (SecondaryID = @PaymentID AND SPV05=@PaymentID) OR (ID=@PaymentID) AND Nick=@Code";
							MySqlParameter[] objparamP4 =
							{
								new MySqlParameter("@PaymentID", sales.SalesPayments[i].paymentID.ToString()),
								new MySqlParameter("@Code", sales.SalesPayments[i].strPayment.ToString())
							};
							PaymentDT = GetData_Vapt(sqlstr, objparamP4);                        
							PayID = Convert.ToString(PaymentDT.Rows[0]["ID"]);
						}
						else {
							if (PaymentDT.Rows.Count == 0)
							{
                                PayID = saveMissingPaymentMethod(sales.SalesPayments[i].strPayment.ToString(), sales.SalesPayments[i].paymentID.ToString());
							}
							else
							{
								PayID = Convert.ToString(PaymentDT.Rows[0]["ID"]);
							}
						}

                        string pRef = "";
                        string pStatus = "";
                        string pIssueCountry = "";

                        if (sales.SalesPayments[i].PaymentReference == null)
                        {
                            pRef = "";
                        }
                        else
                        {
                            pRef = sales.SalesPayments[i].PaymentReference.ToString();
                        }

                        if (sales.SalesPayments[i].PaymentStatus == null)
                        {
                            pStatus = "";
                        }
                        else
                        {
                            pStatus = sales.SalesPayments[i].PaymentStatus.ToString();
                        }

                        if (sales.SalesPayments[i].Issuer_country == null)
                        {
                            pIssueCountry = "";
                        }
                        else
                        {
                            pIssueCountry = sales.SalesPayments[i].Issuer_country.ToString();
                        }
                        
                        string queryInsertPayment = "INSERT INTO retail_sales_payment " +
                            " (SalesPaymentID, SalesID, RetailID, PaymentID, SalesPayTtl, SalesBalTtl, ChangeAmount, Close_RetailID, TipsAmount, PaymentReference,PaymentStatus,OthersPayment,OthersPaymentRef," +
                            "PaymentCardNo,TID,MerchantID,PaymentInvoiceNo,PaymentApprovalCode,Issuer_country,Issuer_bank, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                            " VALUE " +
                            " (@SalesPaymentID, @SalesID, @RetailID, @PaymentID, @SalesPayTtl, @SalesBalTtl, @ChangeAmount, @Close_RetailID,@TipsAmount, @PaymentReference,@PaymentStatus,@OthersPayment,@OthersPaymentRef, " +
                            "@PaymentCardNo,@TID,@MerchantID,@PaymentInvoiceNo,@PaymentApprovalCode,@Issuer_country,@Issuer_bank, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                        {
                            try
                            {
                                objCnn.Open();
                                using (MySqlCommand cmd = new MySqlCommand(queryInsertPayment, objCnn))
                                {
                                    cmd.Parameters.AddWithValue("@SalesPaymentID", SalesPaymentID);
                                    cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                    cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                    //cmd.Parameters.AddWithValue("@PaymentID", sales.SalesPayments[i].paymentID.ToString());
                                    cmd.Parameters.AddWithValue("@PaymentID", PayID.ToString());
                                    cmd.Parameters.AddWithValue("@SalesPayTtl", sales.SalesPayments[i].SalesPayTtl == 0 ? SalesBalTtl : sales.SalesPayments[i].SalesPayTtl);
                                    cmd.Parameters.AddWithValue("@SalesBalTtl", SalesBalTtlPY);
                                    cmd.Parameters.AddWithValue("@ChangeAmount", saleschangeamt);
                                    cmd.Parameters.AddWithValue("@Close_RetailID", sales.RetailID.ToString());
                                    cmd.Parameters.AddWithValue("@TipsAmount", sales.SalesPayments[i].TipsAmount);
                                    cmd.Parameters.AddWithValue("@PaymentReference", pRef.ToString());
                                    cmd.Parameters.AddWithValue("@PaymentStatus", pStatus.ToString());
                                    cmd.Parameters.AddWithValue("@OthersPayment", sales.SalesPayments[i].OthersPayment.ToString());
                                    cmd.Parameters.AddWithValue("@OthersPaymentRef", sales.SalesPayments[i].OthersPaymentRef.ToString());
                                    cmd.Parameters.AddWithValue("@PaymentCardNo", sales.SalesPayments[i].PaymentCardNo.ToString());
                                    cmd.Parameters.AddWithValue("@TID", sales.SalesPayments[i].TID.ToString());
                                    cmd.Parameters.AddWithValue("@MerchantID", sales.SalesPayments[i].MerchantID.ToString());
                                    cmd.Parameters.AddWithValue("@PaymentInvoiceNo", sales.SalesPayments[i].PaymentInvoiceNo.ToString());
                                    cmd.Parameters.AddWithValue("@PaymentApprovalCode", sales.SalesPayments[i].PaymentApprovalCode.ToString());
                                    cmd.Parameters.AddWithValue("@Issuer_country", pIssueCountry.ToString());
                                    cmd.Parameters.AddWithValue("@Issuer_bank", sales.SalesPayments[i].Issuer_bank.ToString());
                                    cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                    cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                    cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                    cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                return ex.ToString();
                            }
                        }
                    }

                    DataTable Sales2DT = GetData("SELECT DefaultGST,DefaultGSTVal FROM definitions ");
                    SalesBalTtl = sales.SalesTotalAmount;
                    string queryInsertSales = "INSERT INTO retail_sales " +
                                " (SalesID, RetailID, SalesNo, SalesTax, SalesTaxVal, SalesDate, SalesDisc, SalesDisc2, SalesDisc3, SalesDiscAmt, SalesDiscAmt2,  SalesDiscAmt3,  SalesDiscGroupPct, SalesDiscGroupAmt,CloseRetailID, CloseDate, CloseTime, SalesStatus, SalesTotalGroupDisc," +
                                " SalesSubTtl, SalesTaxTtl, SalesBalTtl, SalesPayTtl, SalesChangeAmt, SalesRounding, CreateTime, ContraSalesID, ContraSalesNo, ContraSalesDate, ContraCreateTime, ContraSalesStatus,PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                " VALUE " +
                                " (@SalesID, @RetailID, @SalesNo, @SalesTax, @SalesTaxVal, @SalesDate,@SalesDiscPerc, @SalesDiscPerc2, @SalesDiscPerc3, @SalesDiscAmt, @SalesDiscAmt2,  @SalesDiscAmt3,  @SalesDiscGroupPct, @SalesDiscGroupAmt,  @CloseRetailID, @CloseDate, @CloseTime, @SalesStatus, @SalesTotalGroupDisc," +
                                " @SalesSubTtl, @SalesTaxTtl, @SalesBalTtl, @SalesPayTtl, @SalesChangeAmt, @SalesRounding, @CreateTime,@ContraSalesID, @ContraSalesNo, @ContraSalesDate, @ContraCreateTime, @ContraSalesStatus, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsertSales, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                cmd.Parameters.AddWithValue("@SalesNo", salesno);
                                cmd.Parameters.AddWithValue("@SalesTax", Convert.ToString(Sales2DT.Rows[0]["DefaultGST"]));
                                cmd.Parameters.AddWithValue("@SalesTaxVal", Convert.ToDecimal(Sales2DT.Rows[0]["DefaultGSTVal"]));
                                cmd.Parameters.AddWithValue("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@SalesDiscPerc", sales.SalesDiscPerc);
                                cmd.Parameters.AddWithValue("@SalesDiscPerc2", sales.SalesDiscPerc2);
                                cmd.Parameters.AddWithValue("@SalesDiscPerc3", sales.SalesDiscPerc3);
                                cmd.Parameters.AddWithValue("@SalesDiscAmt", sales.SalesDiscAmt);
                                cmd.Parameters.AddWithValue("@SalesDiscAmt2", sales.SalesDiscAmt2);
                                cmd.Parameters.AddWithValue("@SalesDiscAmt3", sales.SalesDiscAmt3);
                                cmd.Parameters.AddWithValue("@SalesDiscGroupPct", sales.SalesDiscPerc > 0 ? sales.SalesTotalDiscount * -1 : 0);
                                cmd.Parameters.AddWithValue("@SalesDiscGroupAmt", sales.SalesDiscAmt > 0 ? sales.SalesDiscAmt * -1 : 0);
                                cmd.Parameters.AddWithValue("@CloseRetailID", sales.RetailID.ToString());
                                cmd.Parameters.AddWithValue("@CloseDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@CloseTime", sales.SalesDate);
                                cmd.Parameters.AddWithValue("@SalesStatus", sales.SalesStatus.ToString());
                                cmd.Parameters.AddWithValue("@SalesTotalGroupDisc", sales.SalesTotalDiscount * -1);
                                cmd.Parameters.AddWithValue("@SalesSubTtl", SalesBalTtl - sales.SalesTaxTtl);
                                cmd.Parameters.AddWithValue("@SalesTaxTtl", sales.SalesTaxTtl);
                                cmd.Parameters.AddWithValue("@SalesBalTtl", SalesBalTtl);
                                cmd.Parameters.AddWithValue("@SalesPayTtl", SalesBalTtl);
                                cmd.Parameters.AddWithValue("@SalesChangeAmt", TotalChangeAmt);
                                cmd.Parameters.AddWithValue("@SalesRounding", sales.SalesRounding);
                                cmd.Parameters.AddWithValue("@CreateTime", sales.SalesDate);
                                cmd.Parameters.AddWithValue("@ContraSalesID", sales.ContraSalesID);
                                cmd.Parameters.AddWithValue("@ContraSalesNo", sales.ContraSalesNo);
                                cmd.Parameters.AddWithValue("@ContraSalesDate", (sales.ContraSalesDate == null || sales.ContraSalesDate == "") ? "0000-00-00" : sales.ContraSalesDate);
                                cmd.Parameters.AddWithValue("@ContraCreateTime", (sales.ContraCreateTime == null || sales.ContraCreateTime == "") ? "0000-00-00 00:00:00" : sales.ContraCreateTime);
                                cmd.Parameters.AddWithValue("@ContraSalesStatus", sales.ContraSalesStatus);
                                cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                cmd.ExecuteNonQuery();
                            }

                            if (sales.DiscID1 != "0" && sales.DiscID1.Length != 0)
                            {
                                SaveDisc(SalesID.ToString(), "", "", "G", sales.DiscID1, sales.RetailID, "1", 1, groupdisc_totaldue, (sales.SalesDiscPerc == 0 ? sales.SalesDiscAmt : sales.SalesDiscPerc), sales.SalesTotalDiscount);
                            }

                            MySqlCommand objcm = new MySqlCommand("RecalculateOnHandQtyBySales", objCnn);
                            objcm.CommandType = CommandType.StoredProcedure;
                            objcm.Parameters.AddWithValue("@pID", SalesID);
                            objcm.Parameters.AddWithValue("@pTransNo", salesno);
                            objcm.Parameters.AddWithValue("@pRetailID", sales.RetailID.ToString());

                            int x1 = objcm.ExecuteNonQuery();
                            objcm.Dispose();

                            try
                            {
                                aging = serializer.Deserialize<InventoryAgings>(agingJson);
                            }
                            catch (Exception ex)
                            {
                                return ex.ToString();
                            }

                            SaveInventoryAging(aging);

							transnoarray += sales.TransNo.ToString() + ",";
                        }
                        catch (Exception ex)
                        {
                            return ex.ToString();
                        }
                    }

                }
            }
        }
        string returnstatus = "Success";
        if (transnoarray.Length > 0)
        {
            transnoarray = transnoarray.TrimEnd(',');
            returnstatus = "Success";
        }
        else
        {
            returnstatus = "Fail";
        }

        return new JavaScriptSerializer().Serialize(new { Status = returnstatus, TransNo = transnoarray });
    }



    public string saveMissingSales(SalesMaster sales)
    {
        //Retail sales
        Guid SalesID = Guid.NewGuid();
        decimal SalesBalTtl = 0;
        decimal TotalChangeAmt = 0;

        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        InventoryAgings aging = new InventoryAgings();
        string agingJson = @"{""ItemAging"":[";

        bool blnContinue = true;
        string salesno = sales.TransNo;

        string strSQL = "select * from retail_sales where SalesNo=@SalesNo and SalesStatus=@SalesStatus AND SalesDate=@SalesDate AND RetailID=@RetailID";
        MySqlParameter[] objparam =
		{
			new MySqlParameter("@SalesNo", sales.TransNo.ToString()),
			new MySqlParameter("@RetailID", sales.RetailID.ToString()),
			new MySqlParameter("@SalesStatus", "SALES"),
			new MySqlParameter("@SalesDate",  sales.SalesDate.ToString("yyyy-MM-dd"))
		};
        DataTable SalesDT = GetData_Vapt(strSQL, objparam);
        if (SalesDT.Rows.Count != 0)
        {
			string sID = SalesDT.Rows[0]["SalesID"].ToString();
			strSQL ="SELECT COUNT(*) as cnt FROM retail_sales_detail WHERE SalesID IN(@SalesID)";
			MySqlParameter[] objparamSD =
			{
				new MySqlParameter("@SalesID", sID.ToString())
			};
			DataTable SalesDDT = GetData_Vapt(strSQL, objparamSD);
			int sdcnt = int.Parse(SalesDDT.Rows[0]["cnt"].ToString());
			SalesDDT.Clear();
			SalesDDT.Dispose();

			strSQL="SELECT COUNT(*) as cnt FROM retail_sales_payment WHERE SalesID IN(@SalesID)";
			MySqlParameter[] objparamSP =
			{
				new MySqlParameter("@SalesID", sID.ToString())
			};
			DataTable SalesPDT = GetData_Vapt(strSQL, objparamSP);
			int pdcnt = int.Parse(SalesPDT.Rows[0]["cnt"].ToString());
			SalesPDT.Clear();
			SalesPDT.Dispose();

			if (sdcnt != 0 && pdcnt != 0)
			{
				blnContinue = false;
			}
			else
			{
				strSQL = "DELETE from retail_sales where SalesID IN(@SalesID)";
				using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
				{
					try
					{
						objCnn.Open();
						using (MySqlCommand cmd = new MySqlCommand(strSQL, objCnn))
						{
							cmd.Parameters.AddWithValue("@SalesID", sID.ToString());
							cmd.ExecuteNonQuery();
						}
					}
					catch (Exception ex)
					{
						return ex.ToString();
					}
				}
				blnContinue = true;
			}
        }
        else
        {
            blnContinue = true;
        }
        SalesDT.Dispose();
        SalesDT.Clear();

        if (blnContinue == true)
        {
            decimal temp_groupitemdiscamt = 0;
            decimal groupdisc_perc = sales.SalesDiscPerc;
            decimal groupdisc_amt = (sales.SalesDiscAmt == 0 ? sales.SalesTotalDiscount : sales.SalesDiscAmt);
            decimal groupdisc_totaldue = getSubtotalDue(sales);
            for (int i = 0; i < sales.ItemSales.Count(); i++)
            {
                Guid SalesDetailID = Guid.NewGuid();
                Guid SalesDetailIDV = Guid.NewGuid();
                string strRetailID = sales.RetailID.ToString();
				string sqlstr ="";
				
                if (sales.ItemSales[i].ItemQty == 0)
                {
                    return "Wrong item qty.";
                }
                else
                {
                    SalesBalTtl = SalesBalTtl + sales.ItemSales[i].ItemTotal;
                    string ItemUOM = sales.ItemSales[i].ItemUOMDesc.ToString();
                    string ItemID = sales.ItemSales[i].ItemID.ToString();
                    string ItemSKU = "";
                    string SupplierID = "";
                    decimal ItemActQty = 0;
                    decimal ItemUnitCost = 0;
                    decimal ItemAveCost = 0;
                    string supBarcode = "";
                    string ItemPoint = "N";
                    string currency = "";
                    decimal ExchRate = 1;
                    string ItemBaseUOMID = "";
                    string ItemBaseUOM = "";
                    decimal ItemBaseActQty = 1;
                    decimal baseQty = 1;
                    decimal SellPrice = sales.ItemSales[i].ItemPrice;
                    if (sales.ItemSales[i].ItemID.ToString() != "")
                    {
                        sqlstr = "SELECT inventory.ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode, inventory.ItemDescp," +
                        "inventory_unit.ItemUnit AS ItemUOM, list_units.Nick AS ItemUOMDesc, inventory_unit.ItemActQty, inventory_unit.RTLSellPx AS ItemUnitPrice," +
                        "inventory_unit.PurchaseCost AS ItemUnitCost, inventory.ItemAveCost, inventory.ItemSKUSup AS SupplierID, inventory.ItemCategory AS CategoryID," +
                        "inventory.ItemDepartment AS DepartmentID, inventory.ItemGroup AS GroupID, inventory.ItemBrand as BrandID, inventory.ItemPoint " +
                        " FROM inventory " +
                        " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                        " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                        " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                        " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID=@ItemID AND list_units.Nick=@Nick LIMIT 1";

                        MySqlParameter[] objparam1 =
						{
							new MySqlParameter("@ItemID", sales.ItemSales[i].ItemID.ToString()),
							new MySqlParameter("@Nick", sales.ItemSales[i].ItemUOMDesc.ToString())
						};

                        DataTable ItemDT = GetData_Vapt(sqlstr, objparam1);

                        if (ItemDT.Rows.Count == 0)
                        {
                            return "Item : " + sales.ItemSales[i].ItemID.ToString() + " with UOM :" + sales.ItemSales[i].ItemUOMDesc.ToString() + " not found";
                        }

                        ItemID = Convert.ToString(ItemDT.Rows[0]["ItemID"]);
                        ItemSKU = Convert.ToString(ItemDT.Rows[0]["ItemSKU"]);
                        ItemUOM = Convert.ToString(ItemDT.Rows[0]["ItemUOM"]);
                        ItemActQty = Convert.ToDecimal(ItemDT.Rows[0]["ItemActQty"]);
                        ItemUnitCost = Convert.ToDecimal(ItemDT.Rows[0]["ItemUnitCost"]);
                        ItemAveCost = Convert.ToDecimal(ItemDT.Rows[0]["ItemAveCost"]);
                        SupplierID = Convert.ToString(ItemDT.Rows[0]["SupplierID"]);
                        ItemPoint = Convert.ToString(ItemDT.Rows[0]["ItemPoint"]);

                        if (sales.ItemSales[i].SupBarCode == null)
                        {
                            supBarcode = Convert.ToString(ItemDT.Rows[0]["supBarcode"]);
                        }
                        else
                        {
                            supBarcode = sales.ItemSales[i].SupBarCode.ToString();
                        }

                        DataTable DefaultDT = GetData("SELECT DefaultCurrency, exchange_rate.ExchRate" +
                        " FROM definitions LEFT JOIN exchange_rate ON definitions.DefaultCountry = exchange_rate.CountryID AND definitions.DefaultCurrency = exchange_rate.ExchCurr");

                        currency = Convert.ToString(DefaultDT.Rows[0]["DefaultCurrency"]);
                        ExchRate = Convert.ToDecimal(DefaultDT.Rows[0]["ExchRate"]);

                        sqlstr = "SELECT iu.ItemUnit as ItemBaseUOMID, iu.ItemActQty as ItemBaseActQty, lu.Nick AS ItemBaseUOM" +
                        " FROM inventory_unit iu" +
                        " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                        " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemUnitDef='Y' AND iu.ItemID=@ItemID";

                        MySqlParameter[] objparam2 =
						{
							new MySqlParameter("@ItemID", ItemID)
						};

                        DataTable BaseUOMDT = GetData_Vapt(sqlstr, objparam2);

                        if (BaseUOMDT.Rows.Count == 0)
                        {
                            sqlstr = "SELECT iu.ItemUnit as ItemBaseUOMID, iu.ItemActQty as ItemBaseActQty, lu.Nick AS ItemBaseUOM" +
                                    " FROM inventory_unit iu" +
                                    " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                    " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemActQty=1 AND iu.ItemID=@ItemID";
                            BaseUOMDT = GetData_Vapt(sqlstr, objparam2);
                        }

                        ItemBaseUOMID = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOMID"]);
                        ItemBaseUOM = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOM"]);
                        ItemBaseActQty = Convert.ToDecimal(BaseUOMDT.Rows[0]["ItemBaseActQty"]);

                    }

                    baseQty = sales.ItemSales[i].ItemQty * ItemActQty;
                    Guid AgingID = Guid.NewGuid();

                    //inventory_aging
                    agingJson = agingJson + "{";
                    agingJson = agingJson + string.Format(@"""ID"":""{0}"",""SupplierID"":""{1}"",""RetailID"":""{2}"",""ItemID"":""{3}"",""ItemSKU"":""{4}"",
							""TransID"":""{5}"",""TransNo"":""{6}"",""TransDate"":""{7}"",""ItemUOMID"":""{8}"",""ItemUOM"":""{9}"",""ItemBaseUOMID"":""{10}"",""ItemBaseUOM"":""{11}"",
							""Qty"":{12},""ItemActualQty"":{13},""CurrencyID"":""{14}"",""ExcRate"":{15},""CostUnitPx"":{16},""LocalCostUnitPx"":{17},""CreateTime"":""{18}"",""BatchNo"":"""",
							""HSCode"":"""",""ExpireDate"":"""",""ExpiryDay"":0,""ItemDefActualQty"":{19},""PDQty"":0,""SoldQty"":{20},""TrfInQty"":0,""TrfOutQty"":0,""AdjQty"":0,""RetQty"":0,""SDQty"":0,""KitQty"":0,
							""DekitQty"":0,""ReserveQty"":0,""InTransitQty"":0,""QtyBalance"":{22},""RFID"":"""",""SellPrice"":{21}", AgingID, SupplierID, strRetailID, sales.ItemSales[i].ItemID.ToString(), ItemSKU,
                                    SalesID, sales.TransNo, sales.SalesDate, ItemUOM, sales.ItemSales[i].ItemUOMDesc, ItemBaseUOMID, ItemBaseUOM,
                                    0, ItemActQty, currency, ExchRate, ItemUnitCost, ItemUnitCost, sales.SalesDate, ItemBaseActQty, baseQty * -1, SellPrice, baseQty * -1);
                    agingJson = agingJson + "},";

                    decimal temp_itemgroupamt = 0;
                    if (i == sales.ItemSales.Length - 1)
                    {
                        temp_itemgroupamt = groupdisc_amt - temp_groupitemdiscamt;
                    }
                    else
                    {
                        if (groupdisc_perc > 0)
                        {
                            temp_itemgroupamt = (sales.ItemSales[i].ItemTotal) * groupdisc_perc / 100;
                            temp_itemgroupamt = Math.Round(temp_itemgroupamt, 2);
                            temp_groupitemdiscamt += temp_itemgroupamt;
                        }
                        else
                        {
                            temp_itemgroupamt = (sales.ItemSales[i].ItemTotal) / groupdisc_totaldue * groupdisc_amt;
                            temp_itemgroupamt = Math.Round(temp_itemgroupamt, 2);
                            temp_groupitemdiscamt += temp_itemgroupamt;
                        }
                    }

                    string queryInsertItem = "INSERT INTO retail_sales_detail " +
                        "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt,ItemDiscGroupAmt, GroupDiscPerc, GroupDiscPerc2, GroupDiscPerc3, GroupDiscAmt, GroupDiscAmt2, GroupDiscAmt3, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                        " VALUE " +
                        "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemDiscGroupAmt, @GroupDiscPerc, @GroupDiscPerc2, @GroupDiscPerc3, @GroupDiscAmt, @GroupDiscAmt2, @GroupDiscAmt3, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";

                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsertItem, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@DetailID", SalesDetailID);
                                cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                cmd.Parameters.AddWithValue("@ItemID", sales.ItemSales[i].ItemID.ToString());
                                cmd.Parameters.AddWithValue("@SupBarCode", supBarcode.ToString());
                                cmd.Parameters.AddWithValue("@ItemQty", sales.ItemSales[i].ItemQty * -1);
                                cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                cmd.Parameters.AddWithValue("@ItemQtyAct", ItemActQty);
                                cmd.Parameters.AddWithValue("@ItemUnitPrice", SellPrice);
                                cmd.Parameters.AddWithValue("@ItemUnitCost", ItemUnitCost);
                                cmd.Parameters.AddWithValue("@ItemAveCost", ItemAveCost);
                                cmd.Parameters.AddWithValue("@ItemDiscAmt", 0);
                                cmd.Parameters.AddWithValue("@ItemDiscGroupAmt", groupdisc_perc > 0 ? temp_itemgroupamt * -1 : 0);
                                cmd.Parameters.AddWithValue("@GroupDiscPerc", groupdisc_perc);
                                cmd.Parameters.AddWithValue("@GroupDiscPerc2", 0);
                                cmd.Parameters.AddWithValue("@GroupDiscPerc3", 0);
                                cmd.Parameters.AddWithValue("@GroupDiscAmt", groupdisc_perc > 0 ? 0 : temp_itemgroupamt * -1);
                                cmd.Parameters.AddWithValue("@GroupDiscAmt2", 0);
                                cmd.Parameters.AddWithValue("@GroupDiscAmt3", 0);
                                cmd.Parameters.AddWithValue("@ItemSubTotal", (sales.ItemSales[i].ItemTotal - sales.ItemSales[i].ItemTax) * -1);
                                cmd.Parameters.AddWithValue("@ItemTaxTotal", (sales.ItemSales[i].ItemTax) * -1);
                                cmd.Parameters.AddWithValue("@ItemTotal", sales.ItemSales[i].ItemTotal * -1);
                                cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                                cmd.Parameters.AddWithValue("@CollectionRetailID", sales.RetailID.ToString());
                                cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                cmd.ExecuteNonQuery();
                            }

                            if (sales.ItemSales[i].ItemDisc != 0)
                            {
                                Guid SalesDetailID2 = Guid.NewGuid();
                                string queryInsertDiscount = "INSERT IGNORE INTO retail_sales_detail " +
                                        "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt, ItemDiscGroupAmt, GroupDiscPerc, GroupDiscPerc2, GroupDiscPerc3, GroupDiscAmt, GroupDiscAmt2, GroupDiscAmt3, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                        " VALUE " +
                                        "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemDiscGroupAmt, @GroupDiscPerc, @GroupDiscPerc2, @GroupDiscPerc3, @GroupDiscAmt, @GroupDiscAmt2, @GroupDiscAmt3, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                                try
                                {
                                    using (MySqlCommand cmd = new MySqlCommand(queryInsertDiscount, objCnn))
                                    {
                                        cmd.Parameters.AddWithValue("@DetailID", SalesDetailID2);
                                        cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                        cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                        cmd.Parameters.AddWithValue("@ItemID", 0);
                                        cmd.Parameters.AddWithValue("@SupBarCode", 0);
                                        cmd.Parameters.AddWithValue("@ItemQty", 1);
                                        cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                        cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                        cmd.Parameters.AddWithValue("@ItemQtyAct", 1);
                                        cmd.Parameters.AddWithValue("@ItemUnitPrice", 0);
                                        cmd.Parameters.AddWithValue("@ItemUnitCost", 0);
                                        cmd.Parameters.AddWithValue("@ItemAveCost", 0);
                                        cmd.Parameters.AddWithValue("@ItemDiscAmt", sales.ItemSales[i].ItemDisc * -1);

                                        cmd.Parameters.AddWithValue("@ItemDiscGroupAmt", 0);
                                        cmd.Parameters.AddWithValue("@GroupDiscPerc", 0);
                                        cmd.Parameters.AddWithValue("@GroupDiscPerc2", 0);
                                        cmd.Parameters.AddWithValue("@GroupDiscPerc3", 0);
                                        cmd.Parameters.AddWithValue("@GroupDiscAmt", 0);
                                        cmd.Parameters.AddWithValue("@GroupDiscAmt2", 0);
                                        cmd.Parameters.AddWithValue("@GroupDiscAmt3", 0);

                                        cmd.Parameters.AddWithValue("@ItemSubTotal", 0);
                                        cmd.Parameters.AddWithValue("@ItemTaxTotal", 0);
                                        cmd.Parameters.AddWithValue("@ItemTotal", 0);
                                        cmd.Parameters.AddWithValue("@SupplierID", 0);
                                        cmd.Parameters.AddWithValue("@CollectionRetailID", sales.RetailID.ToString());
                                        cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                        cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                        cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                        cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                        cmd.ExecuteNonQuery();
                                    }

                                    SaveDisc(SalesID.ToString(), SalesDetailID.ToString(), SalesDetailID2.ToString(), "L", sales.ItemSales[i].DiscID1, sales.RetailID, "1", 1, 0, sales.ItemSales[i].ItemDisc * -1, 0);
                                }
                                catch (Exception ex)
                                {
                                    return ex.ToString();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return ex.ToString();
                        }
                    }
                }
            }
            agingJson = agingJson.Remove(agingJson.Length - 1) + "]}";

            for (int i = 0; i < sales.SalesPayments.Count(); i++)
            {
                Guid SalesPaymentID = Guid.NewGuid();
                decimal SalesBalTtlPY = 0;
                decimal saleschangeamt = 0;
                if (sales.SalesPayments[i].SalesBalTtl == 0 && sales.SalesTotalAmount != 0)
                {
                    SalesBalTtlPY = sales.SalesTotalAmount * -1;
                    saleschangeamt = sales.SalesPayments[i].SalesPayTtl - sales.SalesTotalAmount;
                }
                else
                {
                    SalesBalTtlPY = sales.SalesPayments[i].SalesBalTtl * -1;
                    saleschangeamt = sales.SalesPayments[i].ChangeAmount;
                }

                TotalChangeAmt = TotalChangeAmt + saleschangeamt;
                if (saleschangeamt == 0)
                {
                    saleschangeamt = 0;
                }
                else
                {
                    saleschangeamt = saleschangeamt * -1;
                }
                //TotalChangeAmt = TotalChangeAmt + sales.SalesPayments[i].ChangeAmount;

                string sqlstr = "SELECT ID FROM list_paymentmethods WHERE RecordStatus <> 'DELETED' AND (SecondaryID = @PaymentID AND SPV05=@PaymentID) OR (ID=@PaymentID)";
                MySqlParameter[] objparam4 =
							{
								new MySqlParameter("@PaymentID", sales.SalesPayments[i].paymentID.ToString())
							};
                DataTable PaymentDT = GetData_Vapt(sqlstr, objparam4);
                string PayID = "";
                if (PaymentDT.Rows.Count > 1)
                {
                    PaymentDT.Clear();

                    sqlstr = "SELECT ID FROM list_paymentmethods WHERE RecordStatus <> 'DELETED' AND (SecondaryID = @PaymentID AND SPV05=@PaymentID) OR (ID=@PaymentID) AND Nick=@Code";
                    MySqlParameter[] objparamP4 =
							{
								new MySqlParameter("@PaymentID", sales.SalesPayments[i].paymentID.ToString()),
								new MySqlParameter("@Code", sales.SalesPayments[i].strPayment.ToString())
							};
                    PaymentDT = GetData_Vapt(sqlstr, objparamP4);
                    PayID = Convert.ToString(PaymentDT.Rows[0]["ID"]);
                }
                else
                {
                    if (PaymentDT.Rows.Count == 0)
                    {
                        PayID = saveMissingPaymentMethod(sales.SalesPayments[i].strPayment.ToString(), sales.SalesPayments[i].paymentID.ToString());
                    }
                    else
                    {
                        PayID = Convert.ToString(PaymentDT.Rows[0]["ID"]);
                    }
                }

                string pRef = "";
                string pStatus = "";
                string pIssueCountry = "";

                if (sales.SalesPayments[i].PaymentReference == null)
                {
                    pRef = "";
                }
                else
                {
                    pRef = sales.SalesPayments[i].PaymentReference.ToString();
                }

                if (sales.SalesPayments[i].PaymentStatus == null)
                {
                    pStatus = "";
                }
                else
                {
                    pStatus = sales.SalesPayments[i].PaymentStatus.ToString();
                }

                if (sales.SalesPayments[i].Issuer_country == null)
                {
                    pIssueCountry = "";
                }
                else
                {
                    pIssueCountry = sales.SalesPayments[i].Issuer_country.ToString();
                }

                string queryInsertPayment = "INSERT INTO retail_sales_payment " +
				" (SalesPaymentID, SalesID, RetailID, PaymentID, SalesPayTtl, SalesBalTtl, ChangeAmount, Close_RetailID, TipsAmount, PaymentReference,PaymentStatus,OthersPayment,OthersPaymentRef," +
				"PaymentCardNo,TID,MerchantID,PaymentInvoiceNo,PaymentApprovalCode,Issuer_country,Issuer_bank, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
				" VALUE " +
				" (@SalesPaymentID, @SalesID, @RetailID, @PaymentID, @SalesPayTtl, @SalesBalTtl, @ChangeAmount, @Close_RetailID,@TipsAmount, @PaymentReference,@PaymentStatus,@OthersPayment,@OthersPaymentRef, " +
				"@PaymentCardNo,@TID,@MerchantID,@PaymentInvoiceNo,@PaymentApprovalCode,@Issuer_country,@Issuer_bank, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
				using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsertPayment, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@SalesPaymentID", SalesPaymentID);
                            cmd.Parameters.AddWithValue("@SalesID", SalesID);
                            cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                            //cmd.Parameters.AddWithValue("@PaymentID", sales.SalesPayments[i].paymentID.ToString());
                            cmd.Parameters.AddWithValue("@PaymentID", Convert.ToString(PaymentDT.Rows[0]["ID"]));
                            cmd.Parameters.AddWithValue("@SalesPayTtl", sales.SalesPayments[i].SalesPayTtl * -1);
                            cmd.Parameters.AddWithValue("@SalesBalTtl", SalesBalTtlPY);
                            cmd.Parameters.AddWithValue("@ChangeAmount", saleschangeamt);
                            cmd.Parameters.AddWithValue("@Close_RetailID", sales.RetailID.ToString());
							cmd.Parameters.AddWithValue("@TipsAmount", sales.SalesPayments[i].TipsAmount * -1);
                            cmd.Parameters.AddWithValue("@PaymentReference", pRef.ToString());
                            cmd.Parameters.AddWithValue("@PaymentStatus", pStatus.ToString());
							cmd.Parameters.AddWithValue("@OthersPayment", sales.SalesPayments[i].OthersPayment.ToString());
							cmd.Parameters.AddWithValue("@OthersPaymentRef", sales.SalesPayments[i].OthersPaymentRef.ToString());
							cmd.Parameters.AddWithValue("@PaymentCardNo", sales.SalesPayments[i].PaymentCardNo.ToString());
							cmd.Parameters.AddWithValue("@TID", sales.SalesPayments[i].TID.ToString());
							cmd.Parameters.AddWithValue("@MerchantID", sales.SalesPayments[i].MerchantID.ToString());
							cmd.Parameters.AddWithValue("@PaymentInvoiceNo", sales.SalesPayments[i].PaymentInvoiceNo.ToString());
							cmd.Parameters.AddWithValue("@PaymentApprovalCode", sales.SalesPayments[i].PaymentApprovalCode.ToString());
                            cmd.Parameters.AddWithValue("@Issuer_country", pIssueCountry.ToString());
							cmd.Parameters.AddWithValue("@Issuer_bank", sales.SalesPayments[i].Issuer_bank.ToString());						
                            cmd.Parameters.AddWithValue("@PendingSync", "Y");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }
                }
            }

            DataTable Sales2DT = GetData("SELECT DefaultGST,DefaultGSTVal FROM definitions ");
            SalesBalTtl = sales.SalesTotalAmount;
			string queryInsertSales = "INSERT INTO retail_sales " +
			" (SalesID, RetailID, SalesNo, SalesTax, SalesTaxVal, SalesDate, SalesDisc, SalesDisc2, SalesDisc3, SalesDiscAmt, SalesDiscAmt2,  SalesDiscAmt3,  SalesDiscGroupPct, SalesDiscGroupAmt,CloseRetailID, CloseDate, CloseTime, SalesStatus, SalesTotalGroupDisc," +
			" SalesSubTtl, SalesTaxTtl, SalesBalTtl, SalesPayTtl, SalesChangeAmt, SalesRounding, CreateTime, ContraSalesID, ContraSalesNo, ContraSalesDate, ContraCreateTime, ContraSalesStatus,PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
			" VALUE " +
			" (@SalesID, @RetailID, @SalesNo, @SalesTax, @SalesTaxVal, @SalesDate,@SalesDiscPerc, @SalesDiscPerc2, @SalesDiscPerc3, @SalesDiscAmt, @SalesDiscAmt2,  @SalesDiscAmt3,  @SalesDiscGroupPct, @SalesDiscGroupAmt,  @CloseRetailID, @CloseDate, @CloseTime, @SalesStatus, @SalesTotalGroupDisc," +
			" @SalesSubTtl, @SalesTaxTtl, @SalesBalTtl, @SalesPayTtl, @SalesChangeAmt, @SalesRounding, @CreateTime,@ContraSalesID, @ContraSalesNo, @ContraSalesDate, @ContraCreateTime, @ContraSalesStatus, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
            {
                try
                {
                    objCnn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(queryInsertSales, objCnn))
                    {
                        cmd.Parameters.AddWithValue("@SalesID", SalesID);
                        cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                        cmd.Parameters.AddWithValue("@SalesNo", salesno);
                        cmd.Parameters.AddWithValue("@SalesTax", Convert.ToString(Sales2DT.Rows[0]["DefaultGST"]));
                        cmd.Parameters.AddWithValue("@SalesTaxVal", Convert.ToDecimal(Sales2DT.Rows[0]["DefaultGSTVal"]));
                        cmd.Parameters.AddWithValue("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@SalesDiscPerc", sales.SalesDiscPerc);
                        cmd.Parameters.AddWithValue("@SalesDiscPerc2", sales.SalesDiscPerc2);
                        cmd.Parameters.AddWithValue("@SalesDiscPerc3", sales.SalesDiscPerc3);
                        cmd.Parameters.AddWithValue("@SalesDiscAmt", sales.SalesDiscAmt * -1);
                        cmd.Parameters.AddWithValue("@SalesDiscAmt2", sales.SalesDiscAmt2 * -1);
                        cmd.Parameters.AddWithValue("@SalesDiscAmt3", sales.SalesDiscAmt3 * -1);
                        cmd.Parameters.AddWithValue("@SalesDiscGroupPct", sales.SalesDiscPerc > 0 ? sales.SalesTotalDiscount * -1 : 0);
                        cmd.Parameters.AddWithValue("@SalesDiscGroupAmt", sales.SalesDiscAmt != 0 ? sales.SalesDiscAmt : 0);
                        cmd.Parameters.AddWithValue("@CloseRetailID", sales.RetailID.ToString());
                        cmd.Parameters.AddWithValue("@CloseDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@CloseTime", sales.SalesDate);
                        cmd.Parameters.AddWithValue("@SalesStatus", "SALES");
                        cmd.Parameters.AddWithValue("@SalesTotalGroupDisc", sales.SalesTotalDiscount * -1);
                        cmd.Parameters.AddWithValue("@SalesSubTtl", (SalesBalTtl - sales.SalesTaxTtl) * -1);
                        cmd.Parameters.AddWithValue("@SalesTaxTtl", sales.SalesTaxTtl * -1);
                        cmd.Parameters.AddWithValue("@SalesBalTtl", SalesBalTtl * -1);
                        cmd.Parameters.AddWithValue("@SalesPayTtl", SalesBalTtl * -1);
                        cmd.Parameters.AddWithValue("@SalesChangeAmt", TotalChangeAmt * -1);
                        cmd.Parameters.AddWithValue("@SalesRounding", sales.SalesRounding * -1);
                        cmd.Parameters.AddWithValue("@CreateTime", sales.SalesDate);
						cmd.Parameters.AddWithValue("@ContraSalesID", SalesID);
						cmd.Parameters.AddWithValue("@ContraSalesNo", salesno);
						cmd.Parameters.AddWithValue("@ContraSalesDate", sales.SalesDate.ToString("yyyy-MM-dd"));
						cmd.Parameters.AddWithValue("@ContraCreateTime", sales.SalesDate);
						cmd.Parameters.AddWithValue("@ContraSalesStatus", "VOID");			
                        cmd.Parameters.AddWithValue("@PendingSync", "Y");
                        cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                        cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                        cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                        cmd.ExecuteNonQuery();

                    }

                    if (sales.DiscID1 != "0" && sales.DiscID1.Length == 0)
                    {
                        SaveDisc(SalesID.ToString(), "", "", "G", sales.DiscID1, sales.RetailID, "1", 1, groupdisc_totaldue * -1, (sales.SalesDiscPerc == 0 ? sales.SalesDiscAmt * -1 : sales.SalesDiscPerc), sales.SalesTotalDiscount * -1);
                    }

                    MySqlCommand objcm = new MySqlCommand("RecalculateOnHandQtyBySales", objCnn);
                    objcm.CommandType = CommandType.StoredProcedure;
                    objcm.Parameters.AddWithValue("@pID", SalesID);
                    objcm.Parameters.AddWithValue("@pTransNo", salesno);
                    objcm.Parameters.AddWithValue("@pRetailID", sales.RetailID.ToString());

                    int x1 = objcm.ExecuteNonQuery();
                    objcm.Dispose();
                    try
                    {
                        aging = serializer.Deserialize<InventoryAgings>(agingJson);
                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }

                    SaveInventoryAging(aging);

                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            return "Success";
        }
        return ""; 
    }

	public string deleteDuplicateSales(string salesid){
		bool blnDelete=false;
        string strSQL = "select * from retail_sales where SalesID=@SalesID";
        MySqlParameter[] objparam =
		{
			new MySqlParameter("@SalesID", salesid.ToString()),
		};
        DataTable SalesDT = GetData_Vapt(strSQL, objparam);
        if (SalesDT.Rows.Count != 0)
        {
            blnDelete = true;
        }
        else
        {
            blnDelete = false;
        }
        SalesDT.Dispose();
        SalesDT.Clear();		
		
		if(blnDelete==true){
			string query = "DELETE FROM retail_sales_detail WHERE SALESID=@SalesID" ;
			using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
			{
				try
				{
					objCnn.Open();			
					using (MySqlCommand cmd = new MySqlCommand(query, objCnn))
					{
						cmd.Parameters.AddWithValue("@SalesID", salesid.ToString());
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					return ex.ToString();
				}

				query = "DELETE FROM retail_sales_payment WHERE SALESID=@SalesID" ;
				try
				{
					using (MySqlCommand cmd = new MySqlCommand(query, objCnn))
					{
						cmd.Parameters.AddWithValue("@SalesID", salesid.ToString());
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					return ex.ToString();
				}

				query = "DELETE FROM inventory_tran_discount WHERE SALESID=@SalesID" ;
				try
				{
					using (MySqlCommand cmd = new MySqlCommand(query, objCnn))
					{
						cmd.Parameters.AddWithValue("@SalesID", salesid.ToString());
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					return ex.ToString();
				}
			}
			return salesid.ToString();
		}
		return "";		
	}
   
    public string saveStockTransfer(List<StockTransfer> list_st)
    {
        string transfernoarray = "";
        string sqlstr = "";
        clsDataAccessor objDa = new clsDataAccessor();
        for (int x = 0; x < list_st.Count; x++)
        {
            StockTransfer transfer = (StockTransfer)list_st[x];
            Guid TransferID = Guid.NewGuid();

            if (transfer.IDRef == null)
            {
                return "Missing detail: IDRef";
            }
            else if (transfer.FromRetailerID == null)
            {
                return "Missing detail: From Retail ID";
            }
            else if (transfer.ToRetailerID == null)
            {
                return "Missing detail: To Retail ID";
            }
            else if (transfer.StkTransDate == null)
            {
                return "Missing detail: Transfer Date";
            }

            string sql = "select * from stocktransfer where IDRef=@IDRef and recordstatus <> 'DELETED'";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@IDRef", transfer.IDRef)
            };

            DataTable dt = GetData_Vapt(sql, objparam);
            if (dt.Rows.Count > 0)
            {
                return "Duplicate record:" + transfer.IDRef;
            }
            else
            {
                for (int i = 0; i < transfer.ItemTransfer.Length; i++)
                {
                    Guid DetailID = Guid.NewGuid();
                    StockTransferDetail transferdetail = (StockTransferDetail)transfer.ItemTransfer[i];
                    string ItemID = transferdetail.ItemID;
                    string ItemUOMID = "", ItemBaseUOMID = "", ItemBaseUOM = "";
                    decimal ItemActualQty = 0, ItemCostUnit = 0;
                    if (ItemID.Length > 0)
                    {
                        if (transferdetail.ItemUOMDesc.Length > 0)
                        {
                            sqlstr = "SELECT inventory_unit.ItemUnit AS ItemUOM, inventory_unit.ItemActQty, inventory_unit.PurchaseCost AS ItemUnitCost " +
                               " FROM inventory " +
                               " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                               " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                               " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                               " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID=@ItemID AND list_units.Nick=@Nick LIMIT 1";

                            MySqlParameter[] objparam1 =
                            {
                                new MySqlParameter("@ItemID", ItemID),
                                new MySqlParameter("@Nick", transferdetail.ItemUOMDesc)
                            };
                            dt = GetData_Vapt(sqlstr, objparam1);

                            if (dt.Rows.Count > 0)
                            {
                                ItemUOMID = dt.Rows[0]["ItemUOM"].ToString();
                                ItemActualQty = decimal.Parse(dt.Rows[0]["ItemActQty"].ToString());
                                ItemCostUnit = decimal.Parse(dt.Rows[0]["ItemUnitCost"].ToString());
                            }
                            else
                            {
                                return "Item : " + ItemID + " with UOM :" + transferdetail.ItemUOMDesc.ToString() + " not found";
                            }
                        }

                        sqlstr = "SELECT iu.ItemUnit as ItemBaseUOMID,lu.Nick AS ItemBaseUOM" +
                                    " FROM inventory_unit iu" +
                                    " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                    " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemUnitDef='Y' AND iu.ItemID=@ItemID";

                        MySqlParameter[] objparam2 =
                        {
                                new MySqlParameter("@ItemID", ItemID)
                        };

                        DataTable BaseUOMDT = GetData_Vapt(sqlstr, objparam2);

                        if (BaseUOMDT.Rows.Count == 0)
                        {
                            sqlstr = "SELECT iu.ItemUnit as ItemBaseUOMID, lu.Nick AS ItemBaseUOM" +
                                    " FROM inventory_unit iu" +
                                    " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                    " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemActQty=1 AND iu.ItemID=@ItemID";
                            BaseUOMDT = GetData_Vapt(sqlstr, objparam2);
                        }

                        ItemBaseUOMID = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOMID"]);
                        ItemBaseUOM = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOM"]);
                    }
                    decimal ItemTotalCost = transferdetail.Qty * ItemCostUnit;
                    sqlstr = "insert into stocktransfer_detail (";
                    sqlstr += "StkTrans_DetailID, ID, ItemID, Qty, RcvdQty, ItemUnitID, ItemUnit, ItemBaseUnitID, ItemBaseUnit, ActualQty, StkTrans_UnitPx, StkTrans_TtlPx, StkTrans_DRemark, FromRetailerID, ToRetailerID, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus) Values (";
                    sqlstr += "@StkTrans_DetailID, @ID, @ItemID, @Qty, @RcvdQty, @ItemUnitID, @ItemUnit, @ItemBaseUnitID, @ItemBaseUnit, @ActualQty, @StkTrans_UnitPx, @StkTrans_TtlPx, @StkTrans_DRemark, @FromRetailerID, @ToRetailerID, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus) ;";

                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(sqlstr, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@StkTrans_DetailID", DetailID);
                                cmd.Parameters.AddWithValue("@ID", TransferID);
                                cmd.Parameters.AddWithValue("@ItemID", ItemID);
                                cmd.Parameters.AddWithValue("@Qty", transferdetail.Qty);
                                cmd.Parameters.AddWithValue("@RcvdQty", transferdetail.RcvdQty);
                                cmd.Parameters.AddWithValue("@ItemUnitID", ItemUOMID);
                                cmd.Parameters.AddWithValue("@ItemUnit", transferdetail.ItemUOMDesc);
                                cmd.Parameters.AddWithValue("@ItemBaseUnitID", ItemBaseUOMID);
                                cmd.Parameters.AddWithValue("@ItemBaseUnit", ItemBaseUOM);
                                cmd.Parameters.AddWithValue("@ActualQty", ItemActualQty);
                                cmd.Parameters.AddWithValue("@StkTrans_UnitPx", ItemCostUnit);
                                cmd.Parameters.AddWithValue("@StkTrans_TtlPx", ItemTotalCost);
                                cmd.Parameters.AddWithValue("@StkTrans_DRemark", transferdetail.StkTrans_DRemark);
                                cmd.Parameters.AddWithValue("@FromRetailerID", transferdetail.FromRetailerID);
                                cmd.Parameters.AddWithValue("@ToRetailerID", transferdetail.ToRetailerID);
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", transfer.CreateTime);
                                cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }

                sqlstr = "insert into stocktransfer (";
                sqlstr += "ID, FromRetailerID, ToRetailerID, IDRef, StkTransDate, StkTrans_Remark, LastUser1stConfirm, LastUpdate1stConfirm, RecordStatus1stConfirm, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus) Values (";
                sqlstr += "@ID, @FromRetailerID, @ToRetailerID, @IDRef, @StkTransDate, @StkTrans_Remark, @LastUser1stConfirm, @LastUpdate1stConfirm, @RecordStatus1stConfirm, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)  ;";

                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(sqlstr, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", TransferID);
                            cmd.Parameters.AddWithValue("@FromRetailerID", transfer.FromRetailerID);
                            cmd.Parameters.AddWithValue("@ToRetailerID", transfer.ToRetailerID);
                            cmd.Parameters.AddWithValue("@IDRef", transfer.IDRef);
                            cmd.Parameters.AddWithValue("@StkTransDate", transfer.StkTransDate);
                            cmd.Parameters.AddWithValue("@StkTrans_Remark", transfer.StkTrans_Remark);
                            cmd.Parameters.AddWithValue("@LastUser1stConfirm", transfer.LastUser1stConfirm);
                            cmd.Parameters.AddWithValue("@LastUpdate1stConfirm", transfer.LastUpdate1stConfirm);
                            cmd.Parameters.AddWithValue("@RecordStatus1stConfirm", transfer.RecordStatus1stConfirm);
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", transfer.CreateTime);
                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            transfernoarray += TransferID + ",";
        }
        if (transfernoarray.Length > 0)
        {
            transfernoarray = transfernoarray.TrimEnd(',');
        }
        return new JavaScriptSerializer().Serialize(new { Status = "Success", TransNo = transfernoarray });
    }

    public string saveStockAdjust(List<StockAdjust> list_st)
    {
        string adjustnoarray = "";
        string sqlstr = "";
        clsDataAccessor objDa = new clsDataAccessor();
        for (int x = 0; x < list_st.Count; x++)
        {
            StockAdjust adjust = (StockAdjust)list_st[x];
            Guid AdjustID = Guid.NewGuid();

            if (adjust.IDRef == null)
            {
                return "Missing detail: IDRef";
            }
            else if (adjust.RetailerID == null)
            {
                return "Missing detail: RetailerID ID";
            }
            else if (adjust.StkAdjDate == null)
            {
                return "Missing detail: Adjust Date";
            }

            string sql = "select * from stockadj where IDRef=@IDRef and recordstatus <> 'DELETED'";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@IDRef", adjust.IDRef)
            };

            DataTable dt = GetData_Vapt(sql, objparam);
            if (dt.Rows.Count > 0)
            {
                return "Duplicate record:" + adjust.IDRef;
            }
            else
            {
                for (int i = 0; i < adjust.ItemAdjust.Length; i++)
                {
                    Guid DetailID = Guid.NewGuid();
                    StockAdjustDetail adjdetail = (StockAdjustDetail)adjust.ItemAdjust[i];
                    string ItemID = adjdetail.ItemID;
                    string ItemUOMID = "", ItemBaseUOMID = "", ItemBaseUOM = "";
                    decimal ItemActualQty = 0, ItemCostUnit = 0;
                    if (ItemID.Length > 0)
                    {
                        if (adjdetail.ItemUOMDesc.Length > 0)
                        {
                            sqlstr = "SELECT inventory_unit.ItemUnit AS ItemUOM, inventory_unit.ItemActQty, inventory_unit.PurchaseCost AS ItemUnitCost " +
                               " FROM inventory " +
                               " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                               " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                               " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                               " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID=@ItemID AND list_units.Nick=@Nick LIMIT 1";

                            MySqlParameter[] objparam1 =
                            {
                                new MySqlParameter("@ItemID", ItemID),
                                new MySqlParameter("@Nick", adjdetail.ItemUOMDesc)
                            };
                            dt = GetData_Vapt(sqlstr, objparam1);

                            if (dt.Rows.Count > 0)
                            {
                                ItemUOMID = dt.Rows[0]["ItemUOM"].ToString();
                                ItemActualQty = decimal.Parse(dt.Rows[0]["ItemActQty"].ToString());
                                ItemCostUnit = decimal.Parse(dt.Rows[0]["ItemUnitCost"].ToString());
                            }
                            else
                            {
                                return "Item : " + ItemID + " with UOM :" + adjdetail.ItemUOMDesc.ToString() + " not found";
                            }
                        }

                        sqlstr = "SELECT iu.ItemUnit as ItemBaseUOMID,lu.Nick AS ItemBaseUOM" +
                                    " FROM inventory_unit iu" +
                                    " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                    " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemUnitDef='Y' AND iu.ItemID=@ItemID";

                        MySqlParameter[] objparam2 =
                        {
                                new MySqlParameter("@ItemID", ItemID)
                        };

                        DataTable BaseUOMDT = GetData_Vapt(sqlstr, objparam2);

                        if (BaseUOMDT.Rows.Count == 0)
                        {
                            sqlstr = "SELECT iu.ItemUnit as ItemBaseUOMID, lu.Nick AS ItemBaseUOM" +
                                    " FROM inventory_unit iu" +
                                    " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                    " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemActQty=1 AND iu.ItemID=@ItemID";
                            BaseUOMDT = GetData_Vapt(sqlstr, objparam2);
                        }

                        ItemBaseUOMID = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOMID"]);
                        ItemBaseUOM = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOM"]);
                    }
                    decimal ItemTotalCost = adjdetail.Qty * ItemCostUnit;
                    sqlstr = "insert into stockadj_detail (";
                    sqlstr += "StkAdj_DetailID, ID, ItemID, Qty, ItemUnitID, ItemUnit, ItemBaseUnitID, ItemBaseUnit, ActualQty, StkAdj_UnitPx, StkAdj_TtlPx, StkAdj_DRemark, StkAdj_Type, RetailerID, TransStatus, LastUpdate, LockUpdate, RecordStatus, RecStatus, RecordUpdate, QueueStatus) Values (";
                    sqlstr += "@StkAdj_DetailID, @ID, @ItemID, @Qty,@ItemUnitID, @ItemUnit, @ItemBaseUnitID, @ItemBaseUnit, @ActualQty, @StkAdj_UnitPx, @StkAdj_TtlPx, @StkAdj_DRemark,@StkAdj_Type, @RetailerID, @TransStatus, @LastUpdate, @LockUpdate, @RecordStatus, @RecStatus, @RecordUpdate, @QueueStatus) ;";

                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(sqlstr, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@StkAdj_DetailID", DetailID);
                                cmd.Parameters.AddWithValue("@ID", AdjustID);
                                cmd.Parameters.AddWithValue("@ItemID", ItemID);
                                cmd.Parameters.AddWithValue("@Qty", adjdetail.Qty);
                                cmd.Parameters.AddWithValue("@ItemUnitID", ItemUOMID);
                                cmd.Parameters.AddWithValue("@ItemUnit", adjdetail.ItemUOMDesc);
                                cmd.Parameters.AddWithValue("@ItemBaseUnitID", ItemBaseUOMID);
                                cmd.Parameters.AddWithValue("@ItemBaseUnit", ItemBaseUOM);
                                cmd.Parameters.AddWithValue("@ActualQty", ItemActualQty);
                                cmd.Parameters.AddWithValue("@StkAdj_UnitPx", ItemCostUnit);
                                cmd.Parameters.AddWithValue("@StkAdj_TtlPx", ItemTotalCost);
                                cmd.Parameters.AddWithValue("@StkAdj_DRemark", adjdetail.StkAdj_DRemark);
                                cmd.Parameters.AddWithValue("@StkAdj_Type", adjdetail.StkAdj_Type);
                                cmd.Parameters.AddWithValue("@RetailerID", adjdetail.RetailerID);
                                cmd.Parameters.AddWithValue("@TransStatus", adjdetail.TransStatus);
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecStatus", adjust.RecordStatus);
                                cmd.Parameters.AddWithValue("@RecordUpdate", adjust.CreateTime);
                                cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }

                sqlstr = "insert into stockadj (";
                sqlstr += "ID, RetailerID, IDRef, StkAdj_Type, StkAdjDate, StkAdj_Remark, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus) Values (";
                sqlstr += "@ID, @RetailerID, @IDRef,@StkAdj_Type, @StkAdjDate, @StkAdj_Remark, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)  ;";

                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(sqlstr, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", AdjustID);
                            cmd.Parameters.AddWithValue("@RetailerID", adjust.RetailerID);
                            cmd.Parameters.AddWithValue("@IDRef", adjust.IDRef);
                            cmd.Parameters.AddWithValue("@StkAdj_Type", adjust.StkAdj_Type);
                            cmd.Parameters.AddWithValue("@StkAdjDate", adjust.StkAdjDate);
                            cmd.Parameters.AddWithValue("@StkAdj_Remark", adjust.StkAdj_Remark);
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@RecordStatus", adjust.RecordStatus);
                            cmd.Parameters.AddWithValue("@RecordUpdate", adjust.CreateTime);
                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            adjustnoarray += AdjustID + ",";
        }
        if (adjustnoarray.Length > 0)
        {
            adjustnoarray = adjustnoarray.TrimEnd(',');
        }
        return new JavaScriptSerializer().Serialize(new { Status = "Success", TransNo = adjustnoarray });
    }

    public string saveStockTake(List<StockTake> list_st)
    {
        string takenoarray = "";
        string sqlstr = "";
        clsDataAccessor objDa = new clsDataAccessor();
        for (int x = 0; x < list_st.Count; x++)
        {
            StockTake take = (StockTake)list_st[x];
            Guid TakeID = Guid.NewGuid();

            if (take.IDRef == null)
            {
                return "Missing detail: IDRef";
            }
            else if (take.RetailerID == null)
            {
                return "Missing detail: RetailerID ID";
            }
            else if (take.StkTakeDate == null)
            {
                return "Missing detail: Stock Take Date";
            }

            string sql = "select * from stocktake where IDRef=@IDRef and Display = 'Y'  and RecordStatus <> 'DELETED'";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@IDRef", take.IDRef)
            };

            DataTable dt = GetData_Vapt(sql, objparam);
            if (dt.Rows.Count > 0)
            {
                return "Duplicate record:" + take.IDRef;
            }
            else
            {
                for (int i = 0; i < take.ItemTake.Length; i++)
                {
                    Guid DetailID = Guid.NewGuid();
                    StockTakeDetail takedetail = (StockTakeDetail)take.ItemTake[i];
                    string ItemID = takedetail.ItemID;
                    string ItemUOMID = "", ItemSupBarcode = "";
                    decimal ItemActualQty = 0, ItemCostUnit = 0, ItemBaseCost = 0;
                    if (ItemID.Length > 0)
                    {
                        if (takedetail.UOM.Length > 0)
                        {
                            sqlstr = "SELECT inventory_unit.ItemUnit AS ItemUOM, inventory_unit.ItemActQty, inventory_unit.PurchaseCost AS ItemUnitCost, Inventory_supbar.SupBarcode " +
                               " FROM inventory " +
                               " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                               " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                               " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                               " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID=@ItemID AND list_units.Nick=@Nick LIMIT 1";

                            MySqlParameter[] objparam1 =
                            {
                                new MySqlParameter("@ItemID", ItemID),
                                new MySqlParameter("@Nick", takedetail.UOM)
                            };
                            dt = GetData_Vapt(sqlstr, objparam1);

                            if (dt.Rows.Count > 0)
                            {
                                ItemUOMID = dt.Rows[0]["ItemUOM"].ToString();
                                ItemSupBarcode = dt.Rows[0]["SupBarcode"].ToString();
                                ItemActualQty = decimal.Parse(dt.Rows[0]["ItemActQty"].ToString());
                                ItemCostUnit = decimal.Parse(dt.Rows[0]["ItemUnitCost"].ToString());
                            }
                            else
                            {
                                return "Item : " + ItemID + " with UOM :" + takedetail.UOM + " not found";
                            }
                        }

                        sqlstr = "SELECT iu.PurchaseCost as ItemBaseCost " +
                                    " FROM inventory_unit iu" +
                                    " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                    " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemUnitDef='Y' AND iu.ItemID=@ItemID";

                        MySqlParameter[] objparam2 =
                        {
                                new MySqlParameter("@ItemID", ItemID)
                        };

                        DataTable BaseUOMDT = GetData_Vapt(sqlstr, objparam2);

                        if (BaseUOMDT.Rows.Count == 0)
                        {
                            sqlstr = "SELECT iu.PurchaseCost as ItemBaseCost " +
                                    " FROM inventory_unit iu" +
                                    " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                    " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemActQty=1 AND iu.ItemID=@ItemID";
                            BaseUOMDT = GetData_Vapt(sqlstr, objparam2);
                        }

                        ItemBaseCost = Convert.ToDecimal(BaseUOMDT.Rows[0]["ItemBaseCost"]);
                    }

                    decimal ItemTotalsysCost = takedetail.SystemQty * ItemCostUnit;
                    decimal ItemTotalcntCost = takedetail.CountQty * ItemCostUnit;
                    int ItemVarianceQty = takedetail.SystemQty - takedetail.CountQty;
                    sqlstr = "insert into stocktake_detail (";
                    sqlstr += "StkTake_DetailID, ID, serialNo, ItemID, SystemQty, CountQty, VarianceQty, StkTakeSys_UnitPx, StkTakeSys_TtlPx, StkTakeCnt_TtlPx, StkTake_DRemark, SupBarCode, ItemCost,";
                    sqlstr += "ItemUnitID, UnitID, UOM, ItemActQty, ItemBaseCost, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus) Values (";
                    sqlstr += "@StkTake_DetailID, @ID, @serialNo, @ItemID, @SystemQty, @CountQty, @VarianceQty, @StkTakeSys_UnitPx, @StkTakeSys_TtlPx, @StkTakeCnt_TtlPx, @StkTake_DRemark, @SupBarCode, @ItemCost,";
                    sqlstr += "@ItemUnitID, @UnitID, @UOM, @ItemActQty, @ItemBaseCost, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus);";

                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(sqlstr, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@StkTake_DetailID", DetailID);
                                cmd.Parameters.AddWithValue("@ID", TakeID);
                                cmd.Parameters.AddWithValue("@serialNo", takedetail.serialNo);
                                cmd.Parameters.AddWithValue("@ItemID", ItemID);
                                cmd.Parameters.AddWithValue("@SystemQty", takedetail.SystemQty);
                                cmd.Parameters.AddWithValue("@CountQty", takedetail.CountQty);
                                cmd.Parameters.AddWithValue("@VarianceQty", ItemVarianceQty);
                                cmd.Parameters.AddWithValue("@StkTakeSys_UnitPx", ItemCostUnit);
                                cmd.Parameters.AddWithValue("@StkTakeSys_TtlPx", ItemTotalsysCost);
                                cmd.Parameters.AddWithValue("@StkTakeCnt_TtlPx", ItemTotalcntCost);
                                cmd.Parameters.AddWithValue("@StkTake_DRemark", takedetail.StkTake_DRemark);
                                cmd.Parameters.AddWithValue("@SupBarCode", ItemSupBarcode);
                                cmd.Parameters.AddWithValue("@ItemCost", ItemCostUnit);
                                cmd.Parameters.AddWithValue("@ItemUnitID", ItemUOMID);
                                cmd.Parameters.AddWithValue("@UnitID", takedetail.UnitID);
                                cmd.Parameters.AddWithValue("@UOM", takedetail.UOM);
                                cmd.Parameters.AddWithValue("@ItemActQty", ItemActualQty);
                                cmd.Parameters.AddWithValue("@ItemBaseCost", ItemBaseCost);
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                cmd.Parameters.AddWithValue("@RecordStatus", take.RecordStatus);
                                cmd.Parameters.AddWithValue("@RecordUpdate", take.CreateTime);
                                cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }

                sqlstr = "insert into stocktake (";
                sqlstr += "ID, RetailerID, IDRef, StkTakeDate, RemarkID, StkTake_Remark, StkAdjID, PostedDate, bitStockTakeByBatch, bitIncludeZeroQty, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus) Values (";
                sqlstr += "@ID, @RetailerID, @IDRef,@StkTakeDate, @RemarkID, @StkTake_Remark, @StkAdjID, @PostedDate, @bitStockTakeByBatch, @bitIncludeZeroQty, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)  ;";

                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(sqlstr, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", TakeID);
                            cmd.Parameters.AddWithValue("@RetailerID", take.RetailerID);
                            cmd.Parameters.AddWithValue("@IDRef", take.IDRef);
                            cmd.Parameters.AddWithValue("@StkTakeDate", take.StkTakeDate);
                            cmd.Parameters.AddWithValue("@RemarkID", take.RemarkID);
                            cmd.Parameters.AddWithValue("@StkTake_Remark", take.StkTake_Remark);
                            cmd.Parameters.AddWithValue("@StkAdjID", take.StkAdjID);
                            cmd.Parameters.AddWithValue("@PostedDate", take.PostedDate);
                            cmd.Parameters.AddWithValue("@bitStockTakeByBatch", take.bitStockTakeByBatch);
                            cmd.Parameters.AddWithValue("@bitIncludeZeroQty", take.bitIncludeZeroQty);
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@RecordStatus", take.RecordStatus);
                            cmd.Parameters.AddWithValue("@RecordUpdate", take.CreateTime);
                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            takenoarray += TakeID + ",";
        }
        if (takenoarray.Length > 0)
        {
            takenoarray = takenoarray.TrimEnd(',');
        }
        return new JavaScriptSerializer().Serialize(new { Status = "Success", TransNo = takenoarray });
    }

    public string saveAndroidSales(SalesMaster sales)
    {
        //Retail sales
        Guid SalesID = Guid.NewGuid();
        decimal SalesBalTtl = 0;
        decimal TotalChangeAmt = 0;

        if (sales.TransNo == null)
        {
            return "Missing detail: Trans No";
        }
        else if (sales.RetailID == null)
        {
            return "Missing detail: Retail ID";
        }
        else if (sales.SalesDate == null)
        {
            return "Missing detail: Sales Date";
        }
        else if (sales.SalesStatus == null)
        {
            return "Missing detail: Sales Status";
        }
        else if (sales.SalesTaxTtl == null)
        {
            return "Missing detail: Sales Tax Total";
        }
        else if (sales.SalesRounding == null)
        {
            return "Missing detail: Sales Rounding";
        }
        else if (sales.ItemSales.Count() == 0)
        {
            return "Missing item sales detail.";
        }
        else if (sales.SalesPayments.Count() == 0)
        {
            return "Missing sales payment detail.";
        }
        else
        {
            bool blnContinue = true;
            string salesno = "";
            string salesstatus="";
            if (sales.SalesStatus == "VOID")
            {
                salesno = sales.TransNo + "V";
                salesstatus = sales.SalesStatus.ToString();
            }
            else
            {
                salesno = sales.TransNo;
                salesstatus=sales.SalesStatus.ToString();
            }

            string sqlstr = "SELECT * FROM retail_sales  WHERE RecordStatus <> 'DELETED' AND SalesNo=@SalesNo AND RetailID=@RetailID AND SalesStatus=@SalesStatus AND SalesDate=@SalesDate";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@SalesNo", sales.TransNo.ToString()),
                new MySqlParameter("@RetailID", sales.RetailID.ToString()),
                new MySqlParameter("@SalesStatus", "SALES"),
                new MySqlParameter("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd")),
            };
            DataTable SalesDT = GetData_Vapt(sqlstr, objparam);
            if (SalesDT.Rows.Count == 0)
            {
                if (sales.SalesStatus == "VOID")
                {
                    string blnStatus = saveAndroidMissingSales(sales);
                }
            }
            SalesDT.Dispose();
            SalesDT.Clear();

            MySqlParameter[] objparam1 =
            {
                new MySqlParameter("@SalesNo", salesno),
                new MySqlParameter("@RetailID", sales.RetailID.ToString()),
                new MySqlParameter("@SalesStatus",salesstatus),
                new MySqlParameter("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd")),
            };

            SalesDT = GetData_Vapt(sqlstr, objparam1);
            if (SalesDT.Rows.Count != 0)
            {
                blnContinue = false;
            }
            else
            {
                blnContinue = true;
            }
            SalesDT.Dispose();
            SalesDT.Clear();

            if (blnContinue == true)
            {
                SalesBalTtl = sales.SalesTotalAmount;
                for (int i = 0; i < sales.ItemSales.Count(); i++)
                {
                    Guid SalesDetailID = Guid.NewGuid();

                    //response = checkItem(sales.ItemSales[i].ItemID.ToString(), sales.ItemSales[i].ItemUOMDesc.ToString());
                    //if (response == "OK")
                    //{
                    if (sales.ItemSales[i].ItemQty == 0 || sales.ItemSales[i].ItemQty == null)
                    {
                        return "Wrong item qty.";
                        break;
                    }
                    else
                    {
                        string ItemUOM = sales.ItemSales[i].ItemUOMDesc.ToString();
                        string SupplierID = "";
                        decimal ItemActQty = 0;
                        decimal ItemUnitCost = 0;
                        decimal ItemAveCost = 0;

                        string queryInsertItem = "INSERT INTO retail_sales_detail " +
                            "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                            " VALUE " +
                            "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";

                        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                        {
                            try
                            {
                                objCnn.Open();
                                using (MySqlCommand cmd = new MySqlCommand(queryInsertItem, objCnn))
                                {
                                    cmd.Parameters.AddWithValue("@DetailID", SalesDetailID);
                                    cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                    cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                    cmd.Parameters.AddWithValue("@ItemID", sales.ItemSales[i].ItemID.ToString());
                                    //cmd.Parameters.AddWithValue("@SupBarCode", Convert.ToString(ItemDT.Rows[0]["SupBarCode"]));
                                    cmd.Parameters.AddWithValue("@SupBarCode", sales.ItemSales[i].SupBarCode.ToString());
                                    cmd.Parameters.AddWithValue("@ItemQty", sales.ItemSales[i].ItemQty);
                                    cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                    cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                    cmd.Parameters.AddWithValue("@ItemQtyAct", ItemActQty);
                                    cmd.Parameters.AddWithValue("@ItemUnitPrice", sales.ItemSales[i].ItemPrice);
                                    cmd.Parameters.AddWithValue("@ItemUnitCost", ItemUnitCost);
                                    cmd.Parameters.AddWithValue("@ItemAveCost", ItemAveCost);
                                    cmd.Parameters.AddWithValue("@ItemDiscAmt", 0);
                                    cmd.Parameters.AddWithValue("@ItemSubTotal", sales.ItemSales[i].ItemTotal - sales.ItemSales[i].ItemTax);
                                    cmd.Parameters.AddWithValue("@ItemTaxTotal", sales.ItemSales[i].ItemTax);
                                    cmd.Parameters.AddWithValue("@ItemTotal", sales.ItemSales[i].ItemTotal);
                                    cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                                    cmd.Parameters.AddWithValue("@CollectionRetailID", sales.RetailID.ToString());
                                    cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                    cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                    cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                    cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                    cmd.ExecuteNonQuery();
                                }
                                if (sales.ItemSales[i].ItemDisc != 0)
                                {
                                    Guid SalesDetailID2 = Guid.NewGuid();
                                    string queryInsertDiscount = "INSERT IGNORE INTO retail_sales_detail " +
                                            "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                            " VALUE " +
                                            "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                                    try
                                    {
                                        using (MySqlCommand cmd = new MySqlCommand(queryInsertDiscount, objCnn))
                                        {
                                            cmd.Parameters.AddWithValue("@DetailID", SalesDetailID2);
                                            cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                            cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                            cmd.Parameters.AddWithValue("@ItemID", 0);
                                            cmd.Parameters.AddWithValue("@SupBarCode", 0);
                                            cmd.Parameters.AddWithValue("@ItemQty", 1);
                                            cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                            cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                            cmd.Parameters.AddWithValue("@ItemQtyAct", 1);
                                            cmd.Parameters.AddWithValue("@ItemUnitPrice", 0);
                                            cmd.Parameters.AddWithValue("@ItemUnitCost", 0);
                                            cmd.Parameters.AddWithValue("@ItemAveCost", 0);
                                            cmd.Parameters.AddWithValue("@ItemDiscAmt", sales.ItemSales[i].ItemDisc);
                                            cmd.Parameters.AddWithValue("@ItemSubTotal", 0);
                                            cmd.Parameters.AddWithValue("@ItemTaxTotal", 0);
                                            cmd.Parameters.AddWithValue("@ItemTotal", 0);
                                            cmd.Parameters.AddWithValue("@SupplierID", 0);
                                            cmd.Parameters.AddWithValue("@CollectionRetailID", sales.RetailID.ToString());
                                            cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                            cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        return ex.ToString();
                                    }

                                }
                                //return "Success";
                            }
                            catch (Exception ex)
                            {
                                return ex.ToString();
                            }
                        }
                    }
                }

                for (int i = 0; i < sales.SalesPayments.Count(); i++)
                {
                    Guid SalesPaymentID = Guid.NewGuid();
                    TotalChangeAmt = TotalChangeAmt + sales.SalesPayments[i].ChangeAmount;
                    sqlstr = "SELECT ID FROM list_paymentmethods  WHERE RecordStatus <> 'DELETED' AND SPV05=@PaymentID";
                    MySqlParameter[] objparam2 =
                    {
                        new MySqlParameter("@PaymentID", sales.SalesPayments[i].paymentID.ToString())
                    };
                    DataTable PaymentDT = GetData_Vapt(sqlstr, objparam2);

                    string pRef = "";
                    string pStatus = "";
                    string pIssueCountry = "";

                    if (sales.SalesPayments[i].PaymentReference == null)
                    {
                        pRef = "";
                    }
                    else {
                        pRef = sales.SalesPayments[i].PaymentReference.ToString();
                    }

                    if (sales.SalesPayments[i].PaymentStatus == null)
                    {
                        pStatus = "";
                    }
                    else
                    {
                        pStatus = sales.SalesPayments[i].PaymentStatus.ToString();
                    }

                    if (sales.SalesPayments[i].Issuer_country == null)
                    {
                        pIssueCountry = "";
                    }
                    else
                    {
                        pIssueCountry = sales.SalesPayments[i].Issuer_country.ToString();
                    }

                    if (sales.SalesPayments[i].strPayment.ToString() != " ")
                    {
                        string queryInsertPayment = "INSERT INTO retail_sales_payment " +
                            " (SalesPaymentID, SalesID, RetailID, PaymentID, SalesPayTtl, SalesBalTtl, ChangeAmount, Close_RetailID,TipsAmount, PaymentReference,PaymentStatus,OthersPayment,OthersPaymentRef," +
                              "PaymentCardNo,TID,MerchantID,PaymentInvoiceNo,PaymentApprovalCode,Issuer_country,Issuer_bank, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                            " VALUE " +
                            " (@SalesPaymentID, @SalesID, @RetailID, @PaymentID, @SalesPayTtl, @SalesBalTtl, @ChangeAmount, @Close_RetailID,@TipsAmount, @PaymentReference,@PaymentStatus,@OthersPayment,@OthersPaymentRef, " +
                              "@PaymentCardNo,@TID,@MerchantID,@PaymentInvoiceNo,@PaymentApprovalCode,@Issuer_country,@Issuer_bank, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                        {
                            try
                            {
                                objCnn.Open();
                                using (MySqlCommand cmd = new MySqlCommand(queryInsertPayment, objCnn))
                                {
                                    cmd.Parameters.AddWithValue("@SalesPaymentID", SalesPaymentID);
                                    cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                    cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                    cmd.Parameters.AddWithValue("@PaymentID", Convert.ToString(PaymentDT.Rows[0]["ID"]));
                                    cmd.Parameters.AddWithValue("@SalesPayTtl", sales.SalesPayments[i].SalesPayTtl == 0 ? SalesBalTtl : sales.SalesPayments[i].SalesPayTtl);
                                    cmd.Parameters.AddWithValue("@SalesBalTtl", sales.SalesPayments[i].SalesBalTtl == 0 ? SalesBalTtl : sales.SalesPayments[i].SalesBalTtl);
                                    cmd.Parameters.AddWithValue("@ChangeAmount", sales.SalesPayments[i].ChangeAmount);
                                    cmd.Parameters.AddWithValue("@Close_RetailID", sales.RetailID.ToString());
                                    cmd.Parameters.AddWithValue("@TipsAmount", sales.SalesPayments[i].TipsAmount);
                                    cmd.Parameters.AddWithValue("@PaymentReference", pRef.ToString());
                                    cmd.Parameters.AddWithValue("@PaymentStatus", pStatus.ToString());
                                    cmd.Parameters.AddWithValue("@OthersPayment", sales.SalesPayments[i].OthersPayment.ToString());
                                    cmd.Parameters.AddWithValue("@OthersPaymentRef", sales.SalesPayments[i].OthersPaymentRef.ToString());
                                    cmd.Parameters.AddWithValue("@PaymentCardNo", sales.SalesPayments[i].PaymentCardNo.ToString());
                                    cmd.Parameters.AddWithValue("@TID", sales.SalesPayments[i].TID.ToString());
                                    cmd.Parameters.AddWithValue("@MerchantID", sales.SalesPayments[i].MerchantID.ToString());
                                    cmd.Parameters.AddWithValue("@PaymentInvoiceNo", sales.SalesPayments[i].PaymentInvoiceNo.ToString());
                                    cmd.Parameters.AddWithValue("@PaymentApprovalCode", sales.SalesPayments[i].PaymentApprovalCode.ToString());
                                    cmd.Parameters.AddWithValue("@Issuer_country", pIssueCountry.ToString());
                                    cmd.Parameters.AddWithValue("@Issuer_bank", sales.SalesPayments[i].Issuer_bank.ToString());
                                    cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                    cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                    cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                    cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                    cmd.ExecuteNonQuery();
                                }

                            }
                            catch (Exception ex)
                            {
                                return ex.ToString();
                            }
                        }
                    }                    
                }

                string status = "";

                DataTable Sales2DT = GetData("SELECT DefaultGST,DefaultGSTVal FROM definitions ");

                string queryInsertSales = "INSERT INTO retail_sales " +
                            " (SalesID, RetailID, SalesNo, SalesTax, SalesTaxVal, SalesDate, CloseRetailID, CloseDate, CloseTime, SalesStatus, SalesSubTtl, SalesTaxTtl, SalesBalTtl, SalesPayTtl, SalesChangeAmt, SalesRounding, CreateTime, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                            " VALUE " +
                            " (@SalesID, @RetailID, @SalesNo, @SalesTax, @SalesTaxVal, @SalesDate, @CloseRetailID, @CloseDate, @CloseTime, @SalesStatus, @SalesSubTtl, @SalesTaxTtl, @SalesBalTtl, @SalesPayTtl, @SalesChangeAmt, @SalesRounding, @CreateTime, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsertSales, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@SalesID", SalesID);
                            cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                            cmd.Parameters.AddWithValue("@SalesNo", salesno.ToString());
                            cmd.Parameters.AddWithValue("@SalesTax", Convert.ToString(Sales2DT.Rows[0]["DefaultGST"]));
                            cmd.Parameters.AddWithValue("@SalesTaxVal", Convert.ToDecimal(Sales2DT.Rows[0]["DefaultGSTVal"]));
                            cmd.Parameters.AddWithValue("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@CloseRetailID", sales.RetailID.ToString());
                            cmd.Parameters.AddWithValue("@CloseDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@CloseTime", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@SalesStatus", sales.SalesStatus.ToString());
                            cmd.Parameters.AddWithValue("@SalesSubTtl", SalesBalTtl - sales.SalesTaxTtl);
                            cmd.Parameters.AddWithValue("@SalesTaxTtl", sales.SalesTaxTtl);
                            cmd.Parameters.AddWithValue("@SalesBalTtl", SalesBalTtl);
                            cmd.Parameters.AddWithValue("@SalesPayTtl", SalesBalTtl + TotalChangeAmt);
                            cmd.Parameters.AddWithValue("@SalesChangeAmt", TotalChangeAmt);
                            cmd.Parameters.AddWithValue("@SalesRounding", sales.SalesRounding);
                            cmd.Parameters.AddWithValue("@CreateTime", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@PendingSync", "Y");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                            cmd.ExecuteNonQuery();
                        }

                        status= "Success";
                    }
                    catch (Exception ex)
                    {
                        Sales2DT.Dispose();
                        Sales2DT.Clear();

                        return ex.ToString();
                    }
                }
                Sales2DT.Dispose();
                Sales2DT.Clear();

                return status;
            }
            else
            {
                return "Error: Duplicate sales record.";
            }
        }
    }
    public string saveAndroidMissingSales(SalesMaster sales)
    {
        //Retail sales
        Guid SalesID = Guid.NewGuid();
        decimal SalesBalTtl = 0;
        decimal TotalChangeAmt = 0;

        bool blnContinue = true;
        string salesno = sales.TransNo;
        
        string sqlstr = "SELECT * FROM retail_sales WHERE RecordStatus <> 'DELETED' AND SalesNo=@SalesNo AND RetailID=@RetailID AND SalesStatus=@SalesStatus AND SalesDate=@SalesDate";
        MySqlParameter[] objparam =
            {
                new MySqlParameter("@SalesNo", sales.TransNo.ToString()),
                new MySqlParameter("@RetailID", sales.RetailID.ToString()),
                new MySqlParameter("@SalesStatus", "SALES"),
                new MySqlParameter("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd")),
            };
        DataTable SalesDT = GetData_Vapt(sqlstr, objparam);
        if (SalesDT.Rows.Count != 0)
        {
            blnContinue = false;
        }
        else
        {
            blnContinue = true;
        }
        SalesDT.Dispose();
        SalesDT.Clear();

        if (blnContinue == true)
        {
            SalesBalTtl = sales.SalesTotalAmount *-1;
            for (int i = 0; i < sales.ItemSales.Count(); i++)
            {
                Guid SalesDetailID = Guid.NewGuid();
                if (sales.ItemSales[i].ItemQty == 0 || sales.ItemSales[i].ItemQty == null)
                {
                    return "Wrong item qty.";
                    break;
                }
                else
                {
                    string ItemUOM = sales.ItemSales[i].ItemUOMDesc.ToString();
                    string SupplierID = "";
                    decimal ItemActQty = 0;
                    decimal ItemUnitCost = 0;
                    decimal ItemAveCost = 0;

                    string queryInsertItem = "INSERT INTO retail_sales_detail " +
                        "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                        " VALUE " +
                        "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";

                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsertItem, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@DetailID", SalesDetailID);
                                cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                cmd.Parameters.AddWithValue("@ItemID", sales.ItemSales[i].ItemID.ToString());
                                //cmd.Parameters.AddWithValue("@SupBarCode", Convert.ToString(ItemDT.Rows[0]["SupBarCode"]));
                                cmd.Parameters.AddWithValue("@SupBarCode", sales.ItemSales[i].SupBarCode.ToString());
                                cmd.Parameters.AddWithValue("@ItemQty", sales.ItemSales[i].ItemQty * -1);
                                cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                cmd.Parameters.AddWithValue("@ItemQtyAct", ItemActQty);
                                cmd.Parameters.AddWithValue("@ItemUnitPrice", sales.ItemSales[i].ItemPrice);
                                cmd.Parameters.AddWithValue("@ItemUnitCost", ItemUnitCost);
                                cmd.Parameters.AddWithValue("@ItemAveCost", ItemAveCost);
                                cmd.Parameters.AddWithValue("@ItemDiscAmt", 0);
                                cmd.Parameters.AddWithValue("@ItemSubTotal", (sales.ItemSales[i].ItemTotal - sales.ItemSales[i].ItemTax) * -1);
                                cmd.Parameters.AddWithValue("@ItemTaxTotal", (sales.ItemSales[i].ItemTax) * -1);
                                cmd.Parameters.AddWithValue("@ItemTotal", sales.ItemSales[i].ItemTotal *-1);
                                cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                                cmd.Parameters.AddWithValue("@CollectionRetailID", sales.RetailID.ToString());
                                cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                cmd.ExecuteNonQuery();
                            }
                            if (sales.ItemSales[i].ItemDisc != 0)
                            {
                                Guid SalesDetailID2 = Guid.NewGuid();
                                string queryInsertDiscount = "INSERT IGNORE INTO retail_sales_detail " +
                                        "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                        " VALUE " +
                                        "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                                try
                                {
                                    using (MySqlCommand cmd = new MySqlCommand(queryInsertDiscount, objCnn))
                                    {
                                        cmd.Parameters.AddWithValue("@DetailID", SalesDetailID2);
                                        cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                        cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                        cmd.Parameters.AddWithValue("@ItemID", 0);
                                        cmd.Parameters.AddWithValue("@SupBarCode", 0);
                                        cmd.Parameters.AddWithValue("@ItemQty", 1);
                                        cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                        cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                        cmd.Parameters.AddWithValue("@ItemQtyAct", 1);
                                        cmd.Parameters.AddWithValue("@ItemUnitPrice", 0);
                                        cmd.Parameters.AddWithValue("@ItemUnitCost", 0);
                                        cmd.Parameters.AddWithValue("@ItemAveCost", 0);
                                        cmd.Parameters.AddWithValue("@ItemDiscAmt", sales.ItemSales[i].ItemDisc *-1);
                                        cmd.Parameters.AddWithValue("@ItemSubTotal", 0);
                                        cmd.Parameters.AddWithValue("@ItemTaxTotal", 0);
                                        cmd.Parameters.AddWithValue("@ItemTotal", 0);
                                        cmd.Parameters.AddWithValue("@SupplierID", 0);
                                        cmd.Parameters.AddWithValue("@CollectionRetailID", sales.RetailID.ToString());
                                        cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                        cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                        cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                        cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return ex.ToString();
                                }

                            }
                            //return "Success";
                        }
                        catch (Exception ex)
                        {
                            return ex.ToString();
                        }
                    }
                }
            }

            for (int i = 0; i < sales.SalesPayments.Count(); i++)
            {
                Guid SalesPaymentID = Guid.NewGuid();

                TotalChangeAmt = TotalChangeAmt + sales.SalesPayments[i].ChangeAmount;
                sqlstr = "SELECT ID FROM list_paymentmethods WHERE RecordStatus <> 'DELETED' AND SPV05=@PaymentID";
                MySqlParameter[] objparam1 =
                {
                    new MySqlParameter("@PaymentID", sales.SalesPayments[i].paymentID.ToString())
                };

                DataTable PaymentDT = GetData_Vapt(sqlstr, objparam1);

                string pRef = "";
                string pStatus = "";
                string pIssueCountry = "";
                if (sales.SalesPayments[i].PaymentReference == null)
                {
                    pRef = "";
                }
                else
                {
                    pRef = sales.SalesPayments[i].PaymentReference.ToString();
                }
                if (sales.SalesPayments[i].PaymentStatus == null)
                {
                    pStatus = "";
                }
                else
                {
                    pStatus = sales.SalesPayments[i].PaymentStatus.ToString();
                }

                if (sales.SalesPayments[i].Issuer_country == null)
                {
                    pIssueCountry = "";
                }
                else
                {
                    pIssueCountry = sales.SalesPayments[i].Issuer_country.ToString();
                }

                if (sales.SalesPayments[i].strPayment.ToString() != " ")
                {
                    string queryInsertPayment = "INSERT INTO retail_sales_payment " +
                            " (SalesPaymentID, SalesID, RetailID, PaymentID, SalesPayTtl, SalesBalTtl, ChangeAmount, Close_RetailID,TipsAmount, PaymentReference,PaymentStatus,OthersPayment,OthersPaymentRef," +
                              "PaymentCardNo,TID,MerchantID,PaymentInvoiceNo,PaymentApprovalCode,Issuer_country,Issuer_bank, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                            " VALUE " +
                            " (@SalesPaymentID, @SalesID, @RetailID, @PaymentID, @SalesPayTtl, @SalesBalTtl, @ChangeAmount, @Close_RetailID,@TipsAmount, @PaymentReference,@PaymentStatus,@OthersPayment,@OthersPaymentRef, " +
                              "@PaymentCardNo,@TID,@MerchantID,@PaymentInvoiceNo,@PaymentApprovalCode,@Issuer_country,@Issuer_bank, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsertPayment, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@SalesPaymentID", SalesPaymentID);
                                cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                                cmd.Parameters.AddWithValue("@PaymentID", Convert.ToString(PaymentDT.Rows[0]["ID"]));
                                cmd.Parameters.AddWithValue("@SalesPayTtl", sales.SalesPayments[i].SalesPayTtl * -1);
                                cmd.Parameters.AddWithValue("@SalesBalTtl", sales.SalesPayments[i].SalesBalTtl * -1);
                                cmd.Parameters.AddWithValue("@ChangeAmount", sales.SalesPayments[i].ChangeAmount);
                                cmd.Parameters.AddWithValue("@Close_RetailID", sales.RetailID.ToString());

                                cmd.Parameters.AddWithValue("@TipsAmount", sales.SalesPayments[i].TipsAmount * -1);
                                cmd.Parameters.AddWithValue("@PaymentReference", pRef.ToString());
                                cmd.Parameters.AddWithValue("@PaymentStatus", pStatus.ToString());
                                cmd.Parameters.AddWithValue("@OthersPayment", sales.SalesPayments[i].OthersPayment.ToString());
                                cmd.Parameters.AddWithValue("@OthersPaymentRef", sales.SalesPayments[i].OthersPaymentRef.ToString());
                                cmd.Parameters.AddWithValue("@PaymentCardNo", sales.SalesPayments[i].PaymentCardNo.ToString());
                                cmd.Parameters.AddWithValue("@TID", sales.SalesPayments[i].TID.ToString());
                                cmd.Parameters.AddWithValue("@MerchantID", sales.SalesPayments[i].MerchantID.ToString());
                                cmd.Parameters.AddWithValue("@PaymentInvoiceNo", sales.SalesPayments[i].PaymentInvoiceNo.ToString());
                                cmd.Parameters.AddWithValue("@PaymentApprovalCode", sales.SalesPayments[i].PaymentApprovalCode.ToString());
                                cmd.Parameters.AddWithValue("@Issuer_country", pIssueCountry.ToString());
                                cmd.Parameters.AddWithValue("@Issuer_bank", sales.SalesPayments[i].Issuer_bank.ToString());
                                cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            return ex.ToString();
                        }
                    }
                }
            }
            decimal SalesTtlTax = sales.SalesTaxTtl * -1;
            decimal subtotal = SalesBalTtl - SalesTtlTax;

            DataTable Sales2DT = GetData("SELECT DefaultGST,DefaultGSTVal FROM definitions ");
            string queryInsertSales = "INSERT INTO retail_sales " +
                        " (SalesID, RetailID, SalesNo, SalesTax, SalesTaxVal, SalesDate, CloseRetailID, CloseDate, CloseTime, SalesStatus, SalesSubTtl, SalesTaxTtl, SalesBalTtl, SalesPayTtl, SalesChangeAmt, SalesRounding, CreateTime, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                        " VALUE " +
                        " (@SalesID, @RetailID, @SalesNo, @SalesTax, @SalesTaxVal, @SalesDate, @CloseRetailID, @CloseDate, @CloseTime, @SalesStatus, @SalesSubTtl, @SalesTaxTtl, @SalesBalTtl, @SalesPayTtl, @SalesChangeAmt, @SalesRounding, @CreateTime, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
            {
                try
                {
                    objCnn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(queryInsertSales, objCnn))
                    {
                        cmd.Parameters.AddWithValue("@SalesID", SalesID);
                        cmd.Parameters.AddWithValue("@RetailID", sales.RetailID.ToString());
                        cmd.Parameters.AddWithValue("@SalesNo", salesno.ToString());
                        cmd.Parameters.AddWithValue("@SalesTax", Convert.ToString(Sales2DT.Rows[0]["DefaultGST"]));
                        cmd.Parameters.AddWithValue("@SalesTaxVal", Convert.ToDecimal(Sales2DT.Rows[0]["DefaultGSTVal"]));
                        cmd.Parameters.AddWithValue("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@CloseRetailID", sales.RetailID.ToString());
                        cmd.Parameters.AddWithValue("@CloseDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@CloseTime", sales.SalesDate);
                        cmd.Parameters.AddWithValue("@SalesStatus", "SALES");
                        cmd.Parameters.AddWithValue("@SalesSubTtl", subtotal);
                        cmd.Parameters.AddWithValue("@SalesTaxTtl", SalesTtlTax);
                        cmd.Parameters.AddWithValue("@SalesBalTtl", SalesBalTtl);
                        cmd.Parameters.AddWithValue("@SalesPayTtl", SalesBalTtl + TotalChangeAmt);
                        cmd.Parameters.AddWithValue("@SalesChangeAmt", TotalChangeAmt);
                        cmd.Parameters.AddWithValue("@SalesRounding", sales.SalesRounding);
                        cmd.Parameters.AddWithValue("@CreateTime", sales.SalesDate);
                        cmd.Parameters.AddWithValue("@PendingSync", "Y");
                        cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                        cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                        cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Sales2DT.Dispose();
                    Sales2DT.Clear();

                    return ex.ToString();
                }
            }
            Sales2DT.Dispose();
            Sales2DT.Clear();

            return "Success";
        }
        return "";
    }
	
    public string saveOnlineSales(SalesMaster sales)
    {
        //Retail sales
        Guid SalesID = Guid.NewGuid();
        Guid CustSalesID = Guid.NewGuid();
        decimal SalesBalTtl = 0;
        decimal TotalChangeAmt = 0;
        decimal TotalMemberSpending = 0;

        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        InventoryAgings aging = new InventoryAgings();
        string agingJson = @"{""ItemAging"":[";

        if (sales.TransNo == null)
        {
            return "Missing detail: Trans No";
        }
        else if (sales.RetailID == null)
        {
            return "Missing detail: Retail ID";
        }
        else if (sales.SalesDate == null)
        {
            return "Missing detail: Sales Date";
        }
        else if (sales.SalesStatus == null)
        {
            return "Missing detail: Sales Status";
        }
        else if (sales.SalesTaxTtl == null)
        {
            return "Missing detail: Sales Tax Total";
        }
        else if (sales.SalesRounding == null)
        {
            return "Missing detail: Sales Rounding";
        }
        else if (sales.ItemSales.Count() == 0)
        {
            return "Missing item sales detail.";
        }
        else if (sales.SalesPayments.Count() == 0)
        {
            return "Missing sales payment detail.";
        }
        else
        {
            string strMemberID = "";
            string strRetailID = "1";
			string strMemberName ="";
			decimal amtPerPoint = 0;
			string strMemberHP ="";
			decimal dblRedemptPoint =0;
            decimal dblEarnPoint = 0;
		    decimal totalRedeemPoint=0;
			
            string sqlstr = "SELECT RetailID FROM retailer WHERE RetailType=@RetailType AND RecordStatus <> 'DELETED' ";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@RetailType", sales.RetailID.ToString())
            };
            DataTable DT = GetData_Vapt(sqlstr, objparam);
            if (DT.Rows.Count != 0)
            {
                strRetailID = Convert.ToString(DT.Rows[0]["RetailID"]);
            }
            DT.Clear();

            if (sales.MemberID != "" && sales.MemberID != "-1")
            {
				/*
                string strICNO = sales.MemberID.Substring(0, 4);
                string strHph = sales.MemberID.Substring(4, (sales.MemberID.Length - 4));
				
                DT = GetData(string.Format("SELECT c.ID AS CustID, c.CustomerFirstName AS CustName, c.hph AS CustHP, ct.AmountperPoint " +
                    "FROM customer c INNER JOIN customer_type ct ON ct.CustTypeID =  c.customertype " +
                    "WHERE RIGHT(c.CustICNO,4)='{0}' AND c.hph='{1}'", strICNO, strHph));*/
                DT = GetData(string.Format("SELECT IF(RedemptAmountPoint='Y',RedemptPoint * SpendAmount,0) AS RedemptPoint FROM customer_definitions WHERE RecordStatus<>'DELETED'"));
                if (DT.Rows.Count != 0)
                {
                    dblRedemptPoint = Convert.ToDecimal(DT.Rows[0]["RedemptPoint"]);
                }
                DT.Clear();
				
				strMemberID = sales.MemberID ;
				
                sqlstr = "SELECT c.ID AS CustID, c.CustomerFirstName AS CustName, c.hph AS CustHP, ct.AmountperPoint " +
                    "FROM customer c INNER JOIN customer_type ct ON ct.CustTypeID =  c.customertype " +
                    "WHERE c.ID=@MemberID";
                MySqlParameter[] objparam1 =
                {
                    new MySqlParameter("@MemberID", strMemberID)
                };

                DT = GetData_Vapt(sqlstr, objparam1);

                if (DT.Rows.Count != 0)
                {
                    //strMemberID = Convert.ToString(DT.Rows[0]["CustID"]);
					strMemberName = Convert.ToString(DT.Rows[0]["CustName"]);
					strMemberHP = Convert.ToString(DT.Rows[0]["CustHP"]);
					amtPerPoint = Convert.ToDecimal(DT.Rows[0]["AmountperPoint"]);
                }
                DT.Clear();
            }

            sqlstr = "SELECT * FROM retail_sales WHERE RecordStatus <> 'DELETED' AND SalesNo=@SalesNo AND RetailID=@RetailID AND SalesStatus=@SalesStatus AND SalesDate=@SalesDate";
            MySqlParameter[] objparam2 =
                {
                new MySqlParameter("@SalesNo", sales.TransNo.ToString()),
                new MySqlParameter("@RetailID", strRetailID),
                new MySqlParameter("@SalesStatus", sales.SalesStatus.ToString()),
                new MySqlParameter("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd")),
            };
            DataTable SalesDT = GetData_Vapt(sqlstr, objparam);
            if (SalesDT.Rows.Count == 0)
            {
                string newSalesNo = "";
                decimal salessubtotal=0, salesTax = 0, salesrounding = 0, salesdiscamt1 = 0, salesdiscamt2 = 0, salesdiscamt3 = 0;
                decimal totaldisc = 0, memdiscamt=0,shippingfee=0;
                string contraid = "";
                string contrano = "";
                string controlretailid = "";
                DateTime? contradate = null;

                if (sales.SalesStatus.ToString() == "VOID")
                {
                    sqlstr = "SELECT SalesID,SalesNo,SalesDate,RetailID FROM retail_sales  WHERE RecordStatus <> 'DELETED'  AND SalesNo=@SalesNo AND RetailID=@RetailID";
                    MySqlParameter[] objparam3 =
                    {
                        new MySqlParameter("@SalesNo", sales.TransNo),
                        new MySqlParameter("@RetailID", strRetailID)
                    };
                    DataTable VoidSalesDT = GetData_Vapt(sqlstr, objparam3);
                    if (VoidSalesDT.Rows.Count != 0)
                    {
                        contraid = Convert.ToString(VoidSalesDT.Rows[0]["SalesID"]);
                        contrano = Convert.ToString(VoidSalesDT.Rows[0]["SalesNo"]);
                        controlretailid = Convert.ToString(VoidSalesDT.Rows[0]["RetailID"]);
                        contradate = Convert.ToDateTime(VoidSalesDT.Rows[0]["SalesDate"]);
                    }
                    newSalesNo = sales.TransNo.ToString() + "-V";
                    TotalChangeAmt = 0;
                    salesTax = (sales.SalesTaxTtl*-1);
                    salesrounding = (sales.SalesRounding*-1);
                    salesdiscamt1 = sales.SalesDiscAmt;
                    salesdiscamt2 = sales.SalesDiscAmt2;
                    salesdiscamt2 = sales.SalesDiscAmt3;
                    totaldisc = sales.SalesTotalDiscount;
                    memdiscamt = sales.MemberAmt;
                    shippingfee = (sales.ShippingFee * -1);
                }
                else {
                    newSalesNo = sales.TransNo.ToString();
                    salesTax = sales.SalesTaxTtl;
                    salesrounding = sales.SalesRounding;
                    salesdiscamt1 = (sales.SalesDiscAmt * -1);
                    salesdiscamt2 = (sales.SalesDiscAmt2 * -1);
                    salesdiscamt2 = (sales.SalesDiscAmt3 * -1);
                    totaldisc = (sales.SalesTotalDiscount * -1);
                    memdiscamt = (sales.MemberAmt * -1);
                    shippingfee = sales.ShippingFee;
                }

                for (int i = 0; i < sales.ItemSales.Count(); i++)
                {
                    Guid SalesDetailID = Guid.NewGuid();

                    //response = checkItem(sales.ItemSales[i].ItemID.ToString(), sales.ItemSales[i].ItemUOMDesc.ToString());
                    //if (response == "OK")
                    //{
                    if (sales.ItemSales[i].ItemQty == 0 || sales.ItemSales[i].ItemQty == null)
                    {
                        return "Wrong item qty.";
                        break;
                    }
                    else
                    {
                        decimal SoldQty =0;
                        decimal itemdiscamt = 0, itemdiscamt2 = 0, itemdiscamt3 = 0;
                        decimal itemtotaldisc = 0, itemtotal = 0, itemtax =0, itemsubtotal = 0;
                        decimal itemprice = sales.ItemSales[i].ItemPrice;

                        if (sales.SalesStatus.ToString() == "VOID"){
                            SoldQty = (sales.ItemSales[i].ItemQty *-1);
                            itemdiscamt = sales.ItemSales[i].ItemDisc;
                            itemdiscamt2 =sales.ItemSales[i].ItemDisc2;
                            itemdiscamt3 =sales.ItemSales[i].ItemDisc3;
                            itemtotaldisc = sales.ItemSales[i].ItemTotalDisc;
                            itemtax = (sales.ItemSales[i].ItemTax*-1);
                            itemtotal = (sales.ItemSales[i].ItemTotal*-1);
                            itemsubtotal = itemtotal - itemtax;
                        } else {
                            SoldQty = sales.ItemSales[i].ItemQty;
                            itemdiscamt = (sales.ItemSales[i].ItemDisc *-1);
                            itemdiscamt2 =(sales.ItemSales[i].ItemDisc2 *-1);
                            itemdiscamt3 =(sales.ItemSales[i].ItemDisc3 *-1);
                            itemtotaldisc = (sales.ItemSales[i].ItemTotalDisc *-1);
                            itemtax = sales.ItemSales[i].ItemTax;
                            itemtotal = sales.ItemSales[i].ItemTotal;
                            itemsubtotal = itemtotal - itemtax;
                        }
                        SalesBalTtl = SalesBalTtl + itemtotal;
                        
                        string ItemUOM = sales.ItemSales[i].ItemUOMDesc.ToString();
                        string SupplierID = "";
                        decimal ItemActQty = 0;
                        decimal ItemUnitCost = 0;
                        decimal ItemAveCost = 0;
                        string ItemID = "";
                        string ItemBaseUOMID = "";
                        string ItemBaseUOM = "";
                        decimal ItemBaseActQty = 0;
                        string currency = "";
                        decimal ExchRate = 1;
                        decimal baseQty = 0;
                        string ItemPoint = "";
                        decimal ItemTotalDisc = 0;

                        if (sales.ItemSales[i].ItemID.ToString() != "")
                        {
                            sqlstr = "SELECT inventory.ItemID AS ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode, " +
                            " inventory.ItemDescp, inventory_unit.ItemUnit AS ItemUOM, list_units.Nick AS ItemUOMDesc, inventory_unit.ItemActQty, " +
                            " inventory_unit.RTLSellPx AS ItemUnitPrice, inventory_unit.PurchaseCost AS ItemUnitCost, " +
                            "(IF(inventory_retail.ItemCost=0,inventory.ItemAveCost,inventory_retail.ItemCost)) AS ItemAveCost, inventory.ItemPoint," +
                            " inventory.ItemSKUSup AS SupplierID, inventory.ItemCategory AS CategoryID, inventory.ItemDepartment AS DepartmentID, inventory.ItemGroup AS GroupID, inventory.ItemBrand as BrandID" +
                            " FROM inventory " +
                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                            " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                            " LEFT JOIN inventory_retail ON inventory_retail.ItemID = inventory.ItemID AND inventory_retail.RetailID=@RetailID " +
                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemSKU=@ItemSKU AND list_units.Nick=@UOM " +
                            " AND (inventory_supbar.DefaultSupplier='Y' OR inventory_supbar.Display='Y') LIMIT 1";

                            MySqlParameter[] objparam4 =
                            {
                                new MySqlParameter("@ItemSKU", sales.ItemSales[i].SupBarCode.ToString()),
                                new MySqlParameter("@UOM", sales.ItemSales[i].ItemUOMDesc.ToString()),
                                new MySqlParameter("@RetailID", strRetailID)
                            };
                            DataTable ItemDT = GetData_Vapt(sqlstr, objparam4);
                            if (ItemDT.Rows.Count == 0)
                            {
                                return "Item : " + sales.ItemSales[i].ItemID.ToString() + " with UOM :" + sales.ItemSales[i].ItemUOMDesc.ToString() + " not found"; 
                            }
                            ItemID = Convert.ToString(ItemDT.Rows[0]["ItemID"]);
                            ItemUOM = Convert.ToString(ItemDT.Rows[0]["ItemUOM"]);
                            ItemActQty = Convert.ToDecimal(ItemDT.Rows[0]["ItemActQty"]);
                            ItemUnitCost = Convert.ToDecimal(ItemDT.Rows[0]["ItemUnitCost"]);
                            ItemAveCost = Convert.ToDecimal(ItemDT.Rows[0]["ItemAveCost"]);
                            SupplierID = Convert.ToString(ItemDT.Rows[0]["SupplierID"]);
                            ItemPoint = Convert.ToString(ItemDT.Rows[0]["ItemPoint"]);
                            

                            DataTable DefaultDT = GetData("SELECT DefaultCurrency, exchange_rate.ExchRate" +
                            " FROM definitions LEFT JOIN exchange_rate ON definitions.DefaultCountry = exchange_rate.CountryID AND definitions.DefaultCurrency = exchange_rate.ExchCurr");

                            currency = Convert.ToString(DefaultDT.Rows[0]["DefaultCurrency"]);
                            ExchRate = Convert.ToDecimal(DefaultDT.Rows[0]["ExchRate"]);

                            sqlstr = "SELECT iu.ItemUnit as ItemBaseUOMID, iu.ItemActQty as ItemBaseActQty, lu.Nick AS ItemBaseUOM" +
                            " FROM inventory_unit iu" +
                            " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                            " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemUnitDef='Y' AND iu.ItemID=@ItemID";
                            MySqlParameter[] objparam5 =
                            {
                                new MySqlParameter("@ItemID", ItemID)
                            };

                            DataTable BaseUOMDT = GetData_Vapt(sqlstr, objparam5);
							if(BaseUOMDT.Rows.Count == 0) {
                                sqlstr = "SELECT iu.ItemUnit as ItemBaseUOMID, iu.ItemActQty as ItemBaseActQty, lu.Nick AS ItemBaseUOM" +
                                        " FROM inventory_unit iu" +
                                        " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                                        " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemActQty=1 AND iu.ItemID=@ItemID";
                                BaseUOMDT = GetData_Vapt(sqlstr, objparam5);
							}
							
                            ItemBaseUOMID = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOMID"]);
                            ItemBaseUOM = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOM"]);
                            ItemBaseActQty = Convert.ToDecimal(BaseUOMDT.Rows[0]["ItemBaseActQty"]);
                        }

                        ////insert customer_salesdetails
                        if (strMemberID != "")
                        {
                            if (ItemPoint == "Y")
                            {
                                TotalMemberSpending = TotalMemberSpending + itemtotal;
                            }
                        }
                        baseQty = SoldQty * ItemActQty;
                        Guid AgingID = Guid.NewGuid();

                        //inventory_aging
                        agingJson = agingJson + "{";
                        agingJson = agingJson + string.Format(@"""ID"":""{0}"",""SupplierID"":""{1}"",""RetailID"":""{2}"",""ItemID"":""{3}"",""ItemSKU"":""{4}"",
""TransID"":""{5}"",""TransNo"":""{6}"",""TransDate"":""{7}"",""ItemUOMID"":""{8}"",""ItemUOM"":""{9}"",""ItemBaseUOMID"":""{10}"",""ItemBaseUOM"":""{11}"",
""Qty"":{12},""ItemActualQty"":{13},""CurrencyID"":""{14}"",""ExcRate"":{15},""CostUnitPx"":{16},""LocalCostUnitPx"":{17},""CreateTime"":""{18}"",""BatchNo"":"""",
""HSCode"":"""",""ExpireDate"":"""",""ExpiryDay"":0,""ItemDefActualQty"":{19},""PDQty"":0,""SoldQty"":{20},""TrfInQty"":0,""TrfOutQty"":0,""AdjQty"":0,""RetQty"":0,""SDQty"":0,""KitQty"":0,
""DekitQty"":0,""ReserveQty"":0,""InTransitQty"":0,""QtyBalance"":0,""RFID"":"""",""SellPrice"":""{21}""", AgingID, SupplierID, strRetailID, ItemID, sales.ItemSales[i].ItemID,
SalesID, newSalesNo, sales.SalesDate, ItemUOM, sales.ItemSales[i].ItemUOMDesc, ItemBaseUOMID, ItemBaseUOM,
0, ItemActQty, currency, ExchRate, ItemAveCost, ItemAveCost, sales.SalesDate, ItemBaseActQty, SoldQty, itemprice);
                        agingJson = agingJson + "},";
                        //inventory_aging
                        string queryInsertItem = "INSERT INTO retail_sales_detail " +
                            "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost," +
                            "ItemDisc,ItemDisc2,ItemDisc3,ItemDiscAmt,ItemDiscAmt2,ItemDiscAmt3," +
                            "ItemTotalDisc, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                            " VALUE " +
                            "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost," +
                            "@ItemDiscPerc,@ItemDiscPerc2,@ItemDiscPerc3,@ItemDisc,@ItemDisc2,@ItemDisc3," +
                            "@ItemTotalDisc, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";

                        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                        {
                            try
                            {
                                objCnn.Open();
                                using (MySqlCommand cmd = new MySqlCommand(queryInsertItem, objCnn))
                                {
                                    cmd.Parameters.AddWithValue("@DetailID", SalesDetailID);
                                    cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                    cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                                    cmd.Parameters.AddWithValue("@ItemID", ItemID);
                                    //cmd.Parameters.AddWithValue("@SupBarCode", Convert.ToString(ItemDT.Rows[0]["SupBarCode"]));
                                    cmd.Parameters.AddWithValue("@SupBarCode", sales.ItemSales[i].SupBarCode.ToString());
                                    cmd.Parameters.AddWithValue("@ItemQty", SoldQty);
                                    cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                    cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                    cmd.Parameters.AddWithValue("@ItemQtyAct", ItemActQty);
                                    cmd.Parameters.AddWithValue("@ItemUnitPrice", itemprice);
                                    cmd.Parameters.AddWithValue("@ItemUnitCost", ItemAveCost);
                                    cmd.Parameters.AddWithValue("@ItemAveCost", ItemAveCost);
                                    cmd.Parameters.AddWithValue("@ItemDiscPerc", sales.ItemSales[i].ItemDiscPerc);
                                    cmd.Parameters.AddWithValue("@ItemDiscPerc2", sales.ItemSales[i].ItemDiscPerc2);
                                    cmd.Parameters.AddWithValue("@ItemDiscPerc3", sales.ItemSales[i].ItemDiscPerc3);
                                    cmd.Parameters.AddWithValue("@ItemDisc", itemdiscamt);
                                    cmd.Parameters.AddWithValue("@ItemDisc2", itemdiscamt2);
                                    cmd.Parameters.AddWithValue("@ItemDisc3", itemdiscamt3);
                                    cmd.Parameters.AddWithValue("@ItemTotalDisc", itemtotaldisc);
                                    cmd.Parameters.AddWithValue("@ItemSubTotal", itemsubtotal);
                                    cmd.Parameters.AddWithValue("@ItemTaxTotal", itemtax);
                                    cmd.Parameters.AddWithValue("@ItemTotal", itemtotal);
                                    cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                                    cmd.Parameters.AddWithValue("@CollectionRetailID", strRetailID);
                                    cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                    cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                    cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                    cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                    cmd.ExecuteNonQuery();
                                }
                                if (sales.ItemSales[i].ItemDisc != 0 || sales.ItemSales[i].ItemDisc2 != 0 || sales.ItemSales[i].ItemDisc3 != 0)
                                {
                                    Guid SalesDetailID2 = Guid.NewGuid();
                                    string queryInsertDiscount = "INSERT IGNORE INTO retail_sales_detail " +
                                            "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                            " VALUE " +
                                            "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                                    try
                                    {
                                        using (MySqlCommand cmd = new MySqlCommand(queryInsertDiscount, objCnn))
                                        {
                                            cmd.Parameters.AddWithValue("@DetailID", SalesDetailID2);
                                            cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                            cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                                            cmd.Parameters.AddWithValue("@ItemID", 0);
                                            cmd.Parameters.AddWithValue("@SupBarCode", "Less");
                                            cmd.Parameters.AddWithValue("@ItemQty", 1);
                                            cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                            cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                            cmd.Parameters.AddWithValue("@ItemQtyAct", 1);
                                            cmd.Parameters.AddWithValue("@ItemUnitPrice", itemdiscamt);
                                            cmd.Parameters.AddWithValue("@ItemUnitCost", 0);
                                            cmd.Parameters.AddWithValue("@ItemAveCost", 0);
                                            cmd.Parameters.AddWithValue("@ItemDiscAmt", itemdiscamt + itemdiscamt2 + itemdiscamt3);
                                            cmd.Parameters.AddWithValue("@ItemSubTotal", 0);
                                            cmd.Parameters.AddWithValue("@ItemTaxTotal", 0);
                                            cmd.Parameters.AddWithValue("@ItemTotal", 0);
                                            cmd.Parameters.AddWithValue("@SupplierID", 0);
                                            cmd.Parameters.AddWithValue("@CollectionRetailID", strRetailID);
                                            cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                            cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        return ex.ToString();
                                    }

                                }
                                else if (sales.ItemSales[i].ItemDiscPerc != 0 || sales.ItemSales[i].ItemDiscPerc2 != 0 || sales.ItemSales[i].ItemDiscPerc3 != 0)
                                {
                                    decimal DiscAmt = 0;
                                    if (sales.ItemSales[i].ItemDiscPerc != 0)
                                    {
                                        DiscAmt = sales.ItemSales[i].ItemPrice * (sales.ItemSales[i].ItemDiscPerc / 100);
                                    }
                                    if (sales.ItemSales[i].ItemDiscPerc2 != 0)
                                    {
                                        DiscAmt = DiscAmt + (sales.ItemSales[i].ItemPrice - DiscAmt) * (sales.ItemSales[i].ItemDiscPerc2 / 100);
                                    }
                                    if (sales.ItemSales[i].ItemDiscPerc3 != 0)
                                    {
                                        DiscAmt = DiscAmt + (sales.ItemSales[i].ItemPrice - DiscAmt) * (sales.ItemSales[i].ItemDiscPerc3 / 100);
                                    }

                                    if (sales.SalesStatus.ToString() == "SALES")
                                    {
                                        DiscAmt = DiscAmt * -1;
                                    }

                                    Guid SalesDetailID2 = Guid.NewGuid();
                                    string queryInsertDiscount = "INSERT IGNORE INTO retail_sales_detail " +
                                            "(Sales_DetailID, SalesID, RetailID, ItemID, SupBarCode, ItemQty, ItemUOM, ItemUOMDesc, ItemQtyAct, ItemUnitPrice, ItemUnitCost, ItemAveCost, ItemDiscAmt, ItemSubTotal, ItemTaxTotal, ItemTotal, SupplierID, CollectionRetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                            " VALUE " +
                                            "(@DetailID, @SalesID, @RetailID, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @ItemUOMDesc, @ItemQtyAct, @ItemUnitPrice, @ItemUnitCost, @ItemAveCost, @ItemDiscAmt, @ItemSubTotal, @ItemTaxTotal, @ItemTotal, @SupplierID, @CollectionRetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                                    try
                                    {
                                        using (MySqlCommand cmd = new MySqlCommand(queryInsertDiscount, objCnn))
                                        {
                                            cmd.Parameters.AddWithValue("@DetailID", SalesDetailID2);
                                            cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                            cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                                            cmd.Parameters.AddWithValue("@ItemID", 0);
                                            cmd.Parameters.AddWithValue("@SupBarCode", "Less");
                                            cmd.Parameters.AddWithValue("@ItemQty", 1);
                                            cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                            cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                            cmd.Parameters.AddWithValue("@ItemQtyAct", 1);
                                            cmd.Parameters.AddWithValue("@ItemUnitPrice", DiscAmt);
                                            cmd.Parameters.AddWithValue("@ItemUnitCost", 0);
                                            cmd.Parameters.AddWithValue("@ItemAveCost", 0);
                                            cmd.Parameters.AddWithValue("@ItemDiscAmt", DiscAmt);
                                            cmd.Parameters.AddWithValue("@ItemSubTotal", 0);
                                            cmd.Parameters.AddWithValue("@ItemTaxTotal", 0);
                                            cmd.Parameters.AddWithValue("@ItemTotal", 0);
                                            cmd.Parameters.AddWithValue("@SupplierID", 0);
                                            cmd.Parameters.AddWithValue("@CollectionRetailID", strRetailID);
                                            cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                            cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        return ex.ToString();
                                    }

                                }
                                //return "Success";
                            }
                            catch (Exception ex)
                            {
                                return ex.ToString();
                            }
                        }
                        //***inventory voucher
                        if (sales.ItemSales[i].ItemVoucher.Count() != 0)
                        {
                            for (int x = 0; x < sales.ItemSales[i].ItemVoucher.Count(); x++)
                            {
                                string queryUpdateOrder = "UPDATE inventory_voucher " +
                                        " SET SoldFromRetailID = @SoldFromRetailID, SoldTransID = @SoldTransID, bitSold = 'Y', LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                                        " WHERE ItemID=@ItemID AND SerialNo=@SerialNo";

                                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                                {
                                    try
                                    {
                                        objCnn.Open();

                                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateOrder, objCnn))
                                        {
                                            cmd.Parameters.AddWithValue("@SoldFromRetailID", strRetailID);
                                            cmd.Parameters.AddWithValue("@SoldTransID", SalesID);
                                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@ItemID", ItemID);
                                            cmd.Parameters.AddWithValue("@SerialNo", sales.ItemSales[i].ItemVoucher[x].VoucherNo.ToString());
                                            cmd.ExecuteNonQuery();
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        return ex.ToString();
                                    }
                                }
                            }

                        }
                    }
                }

                //insert customer_salesdetails
                if (strMemberID != "")
                {
                    dblEarnPoint = TotalMemberSpending * amtPerPoint;

                    string queryInsertCustSales = "INSERT INTO customer_salesdetails " +
                        " (ID,RetailID,TransDate,TransID,CustID,CustName,CustHP,bitCreateNew,TotalAmount,bitLoyalty,LoyaltyPoint,LastUpdate,RecordUpdate)" +
                        " VALUE " +
                        " (@ID,@RetailID,@TransDate,@TransID,@CustID,@CustName,@CustHP,@bitCreateNew,@TotalAmount,@bitLoyalty,@LoyaltyPoint,@LastUpdate,@RecordUpdate)";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsertCustSales, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@ID", CustSalesID);
                                cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                                cmd.Parameters.AddWithValue("@TransDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@TransID", SalesID);
                                cmd.Parameters.AddWithValue("@CustID", strMemberID);
                                cmd.Parameters.AddWithValue("@CustName", strMemberName);
                                cmd.Parameters.AddWithValue("@CustHP", strMemberHP);
                                cmd.Parameters.AddWithValue("@bitCreateNew", "Y");
                                cmd.Parameters.AddWithValue("@TotalAmount", TotalMemberSpending);
                                cmd.Parameters.AddWithValue("@bitLoyalty", "Y");
                                cmd.Parameters.AddWithValue("@LoyaltyPoint", TotalMemberSpending * amtPerPoint);
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            return ex.ToString();
                        }
                    }
                }

                agingJson = agingJson.Remove(agingJson.Length - 1) + "]}";
                string strPaymentID = "";
                decimal salespayttl = 0, salebalttl = 0, changeamt = 0;

                for (int i = 0; i < sales.SalesPayments.Count(); i++)
                {
                    if (sales.SalesStatus.ToString() == "VOID")
                    {
                        salespayttl = (sales.SalesPayments[i].SalesBalTtl * -1);
                        salebalttl = (sales.SalesPayments[i].SalesBalTtl * -1);
                        changeamt = 0;
                    }
                    else
                    {
                        salespayttl = sales.SalesPayments[i].SalesPayTtl;
                        salebalttl = sales.SalesPayments[i].SalesBalTtl;
                        changeamt = sales.SalesPayments[i].ChangeAmount;
                    }

                    Guid SalesPaymentID = Guid.NewGuid();
                    sqlstr = "SELECT ID FROM list_paymentmethods WHERE RecordStatus <> 'DELETED' ";

                    string strWhere = "";
                    strWhere = " AND Nick=@StrPayment ";

                    if (sales.SalesPayments[i].paymentID.ToString() != "")
                    {
                        strWhere = strWhere + " AND ID=@PaymentID";
                    }
					sqlstr += strWhere;
                    TotalChangeAmt = TotalChangeAmt + changeamt;
                    MySqlParameter[] objparam6 =
                    {
                        new MySqlParameter("@StrPayment", sales.SalesPayments[i].strPayment.ToString()),
                        new MySqlParameter("@PaymentID", sales.SalesPayments[i].paymentID.ToString())
                    };
                    
                    DataTable PaymentDT = GetData_Vapt(sqlstr, objparam6);
                    if (PaymentDT.Rows.Count == 0)
                    {
                        strPaymentID = Convert.ToString(Guid.NewGuid());

                        string queryInsertPaymentType = "INSERT INTO list_paymentmethods(ID,VALUE,Nick,FULL,Display,DisplayNo,ButtonName,ButtonGroup,SPV05,LastUpdate, LockUpdate, RecordStatus, RecordUpdate)" +
                            " VALUE " +
                            " (@ID, @VALUE, @VALUE, @VALUE, @Display, @DisplayNo, @ButtonName, @ButtonGroup,@ID, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate)";
                        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                        {
                            try
                            {
                                objCnn.Open();
                                using (MySqlCommand cmd = new MySqlCommand(queryInsertPaymentType, objCnn))
                                {
                                    cmd.Parameters.AddWithValue("@ID", strPaymentID);
                                    cmd.Parameters.AddWithValue("@VALUE", sales.SalesPayments[i].strPayment.ToString());
                                    cmd.Parameters.AddWithValue("@Display", "Y");
                                    cmd.Parameters.AddWithValue("@DisplayNo", "0");
                                    cmd.Parameters.AddWithValue("@ButtonName", "cmdPayOTHERS");
                                    cmd.Parameters.AddWithValue("@ButtonGroup", "OTHERS");
                                    cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                    cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                return ex.ToString();
                            }
                        }
                    }
                    else {
                        strPaymentID = Convert.ToString(PaymentDT.Rows[0]["ID"]);
                    }
                    PaymentDT.Clear();
                    PaymentDT.Dispose();

                    string queryInsertPayment = "INSERT INTO retail_sales_payment " +
                        " (SalesPaymentID, SalesID, RetailID, PaymentID, SalesPayTtl, SalesBalTtl, ChangeAmount, Close_RetailID, PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                        " VALUE " +
                        " (@SalesPaymentID, @SalesID, @RetailID, @PaymentID, @SalesPayTtl, @SalesBalTtl, @ChangeAmount, @Close_RetailID, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsertPayment, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@SalesPaymentID", SalesPaymentID);
                                cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                                cmd.Parameters.AddWithValue("@PaymentID", strPaymentID);
                                cmd.Parameters.AddWithValue("@SalesPayTtl", salespayttl);
                                cmd.Parameters.AddWithValue("@SalesBalTtl", salebalttl);
                                cmd.Parameters.AddWithValue("@ChangeAmount", changeamt);
                                cmd.Parameters.AddWithValue("@Close_RetailID", strRetailID);
                                cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                                cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                cmd.ExecuteNonQuery();
                            }

                            //return "Success";
                        }
                        catch (Exception ex)
                        {
                            return sales.SalesPayments[i].strPayment.ToString() + " payment type not found."; 
                            //return ex.ToString();
                        }
                    }
					
					// update customer sales details for redemption point 
                    if (strMemberID != "")
                    {
                        if(sales.SalesPayments[i].strPayment.ToString()=="REDEMPTPOINT") {
                            totalRedeemPoint += dblRedemptPoint * salespayttl;

                            string queryUpdateCustSales = "UPDATE customer_salesdetails SET bitRedeem='Y',RedeemPoint= @RedeemPoint  WHERE RetailID=@RetailID AND TransID=@TransID AND TerminalID=@TerminalID AND CustID=@MemberID";
                            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                            {
                                try
                                {
                                    objCnn.Open();
                                    using (MySqlCommand cmd = new MySqlCommand(queryUpdateCustSales, objCnn))
                                    {
                                        cmd.Parameters.AddWithValue("@RedeemPoint", totalRedeemPoint);
                                        cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                                        cmd.Parameters.AddWithValue("@TransID", SalesID);
                                        cmd.Parameters.AddWithValue("@TerminalID", 1);
                                        cmd.Parameters.AddWithValue("@MemberID", strMemberID);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return ex.ToString();
                                }
                            }
                        }
                    }
					
                    //retail_sales_voucher && inventory_voucher
                    if (sales.SalesPayments[i].SaleVoucher.Count() != 0)
                    {
                        for (int y = 0; y < sales.SalesPayments[i].SaleVoucher.Count(); y++)
                        {
                            if(sales.SalesPayments[i].SaleVoucher[y].VoucherNo != "")
                            {
                                Guid SalesVoucherID = Guid.NewGuid();
                                string queryInsertSalesVoucher = "INSERT INTO retail_sales_voucher " +
                                " (ID,SalesID,RetailID,PaymentID,Voucher_RefNo,Voucher_Amount,LastUpdate,RecordUpdate)" +
                                " VALUE " +
                                " (@SalesVoucherID, @SalesID, @RetailID, @PaymentID, @Voucher_RefNo, @Voucher_Amount, @LastUpdate, @RecordUpdate)";
                                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                                {
                                    try
                                    {
                                        objCnn.Open();
                                        using (MySqlCommand cmd = new MySqlCommand(queryInsertSalesVoucher, objCnn))
                                        {
                                            cmd.Parameters.AddWithValue("@SalesVoucherID", SalesVoucherID);
                                            cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                            cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                                            cmd.Parameters.AddWithValue("@PaymentID", Convert.ToString(PaymentDT.Rows[0]["ID"]));
                                            cmd.Parameters.AddWithValue("@Voucher_RefNo", sales.SalesPayments[i].SaleVoucher[y].VoucherNo);
                                            cmd.Parameters.AddWithValue("@Voucher_Amount", sales.SalesPayments[i].SaleVoucher[y].VoucherAmount);
                                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                            cmd.ExecuteNonQuery();
                                        }

                                        //return "Success";
                                    }
                                    catch (Exception ex)
                                    {
                                        return ex.ToString();
                                    }
                                }

                                string queryUpdateOrder = "UPDATE inventory_voucher  SET RedeemFromRetailID = @RedeemFromRetailID, RedeemTransID = @RedeemTransID, bitRedeem = 'Y', LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                                            " WHERE SerialNo=@SerialNo";

                                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                                {
                                    try
                                    {
                                        objCnn.Open();

                                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateOrder, objCnn))
                                        {
                                            cmd.Parameters.AddWithValue("@RedeemFromRetailID", strRetailID);
                                            cmd.Parameters.AddWithValue("@RedeemTransID", SalesID);
                                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@SerialNo", sales.ItemSales[i].ItemVoucher[y].VoucherNo.ToString());
                                            cmd.ExecuteNonQuery();
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        return ex.ToString();
                                    }
                                }
                            }
                            
                        }
                    }
                }

                //retailSalesPerson
                DataTable Sales2DT = GetData("SELECT DefaultGST,DefaultGSTVal" +
                                    " FROM definitions ");
                string queryInsertSales = "INSERT INTO retail_sales " +
                            " (SalesID, RetailID, SalesNo, SalesTax, SalesTaxVal, SalesDate, CloseRetailID, CloseDate, CloseTime, " +
                            " SalesStatus, SalesSubTtl, SalesTaxTtl, SalesBalTtl, SalesPayTtl, SalesChangeAmt, SalesRounding,SalesDisc,SalesDisc2,SalesDisc3," +
                            "SalesDiscAmt,SalesDiscAmt2,SalesDiscAmt3,SalesDiscGroupAmt,SalesTotalGroupDisc,ContraRetailID,ContraSalesID,ContraSalesNo,ContraSalesDate, " +
                            "CreateTime, MemberID, MemberDisc,MemberAmt,ShippingFee,PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                            " VALUE " +
                            " (@SalesID, @RetailID, @SalesNo, @SalesTax, @SalesTaxVal, @SalesDate, @CloseRetailID, @CloseDate, @CloseTime, @SalesStatus, @SalesSubTtl, @SalesTaxTtl, @SalesBalTtl, @SalesPayTtl, @SalesChangeAmt, @SalesRounding," +
                            " @SalesDiscPerc,@SalesDiscPerc2,@SalesDiscPerc3,@SalesDiscAmt,@SalesDiscAmt2,@SalesDiscAmt3,@SalesDiscGroupAmt,@SalesTotalDiscount, " +
                            "@ContraRetailID,@ContraSalesID,@ContraSalesNo,@ContraSalesDate," +
                            " @CreateTime, @MemberID, @MemberDisc, @MemberAmt,@ShippingFee, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsertSales, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@SalesID", SalesID);
                            cmd.Parameters.AddWithValue("@RetailID", strRetailID.ToString());
                            cmd.Parameters.AddWithValue("@SalesNo", newSalesNo);
                            //cmd.Parameters.AddWithValue("@SalesTax", Convert.ToString(Sales2DT.Rows[0]["DefaultGST"]));
                            cmd.Parameters.AddWithValue("@SalesTax", sales.SalesTaxType);
                            //cmd.Parameters.AddWithValue("@SalesTaxVal", Convert.ToDecimal(Sales2DT.Rows[0]["DefaultGSTVal"]));
                            cmd.Parameters.AddWithValue("@SalesTaxVal", sales.SalesTaxRate);
                            cmd.Parameters.AddWithValue("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@CloseRetailID", strRetailID.ToString());
                            cmd.Parameters.AddWithValue("@CloseDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@CloseTime", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@SalesStatus", sales.SalesStatus.ToString());
                            cmd.Parameters.AddWithValue("@SalesSubTtl", SalesBalTtl - salesTax);
                            cmd.Parameters.AddWithValue("@SalesTaxTtl", salesTax);
                            cmd.Parameters.AddWithValue("@SalesBalTtl", SalesBalTtl);
                            cmd.Parameters.AddWithValue("@SalesPayTtl", SalesBalTtl);
                            cmd.Parameters.AddWithValue("@SalesChangeAmt", TotalChangeAmt);
                            cmd.Parameters.AddWithValue("@SalesRounding", salesrounding);
                            cmd.Parameters.AddWithValue("@SalesDiscPerc", sales.SalesDiscPerc);
                            cmd.Parameters.AddWithValue("@SalesDiscPerc2", sales.SalesDiscPerc2);
                            cmd.Parameters.AddWithValue("@SalesDiscPerc3", sales.SalesDiscPerc3);
                            cmd.Parameters.AddWithValue("@SalesDiscAmt", salesdiscamt1);
                            cmd.Parameters.AddWithValue("@SalesDiscAmt2", salesdiscamt2);
                            cmd.Parameters.AddWithValue("@SalesDiscAmt3", salesdiscamt3);
                            cmd.Parameters.AddWithValue("@SalesDiscGroupAmt", salesdiscamt1 + salesdiscamt2 + salesdiscamt3);
                            cmd.Parameters.AddWithValue("@SalesTotalDiscount", totaldisc);
                            cmd.Parameters.AddWithValue("@ContraRetailID", controlretailid);
                            cmd.Parameters.AddWithValue("@ContraSalesID", contraid);
                            cmd.Parameters.AddWithValue("@ContraSalesNo", contrano);
                            cmd.Parameters.AddWithValue("@ContraSalesDate", contradate);
                            cmd.Parameters.AddWithValue("@CreateTime", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@MemberID", strMemberID.ToString());
                            cmd.Parameters.AddWithValue("@MemberDisc", sales.MemberDisc);
                            cmd.Parameters.AddWithValue("@MemberAmt", memdiscamt);
                            cmd.Parameters.AddWithValue("@ShippingFee", shippingfee);
                            cmd.Parameters.AddWithValue("@PendingSync", "N");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                            cmd.ExecuteNonQuery();
                        }
                        try
                        {
                            aging = serializer.Deserialize<InventoryAgings>(agingJson);
                        }
                        catch (Exception ex)
                        {
                            return ex.ToString();
                        }
                        SaveInventoryAging(aging);

                        if (sales.SalesStatus.ToString() == "VOID")
                        {
                            // Update the contra void sales for the orginal sales records
                            string queryUpdateSales = "UPDATE Retail_sales  SET ContraRetailID = @RetailID, ContraSalesID = @SalesID, ContraSalesNo = @SalesNo, " +
                                                " ContraSalesDate = @SalesDate WHERE SalesID = @ContraID ";
                            using (MySqlCommand cmd = new MySqlCommand(queryUpdateSales, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@SalesID", SalesID);
                                cmd.Parameters.AddWithValue("@RetailID", strRetailID.ToString());
                                cmd.Parameters.AddWithValue("@SalesNo", newSalesNo);
                                cmd.Parameters.AddWithValue("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@ContraID", contraid);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        decimal TotalPoint = 0;
                        decimal TotalRP = 0;
                        if (strMemberID != "")
                        {
                            if (totalRedeemPoint != 0)
                            {
                                TotalRP = updMemberRedeemPoint(strMemberID, sales.SalesDate.ToString("yyyy-MM-dd"), totalRedeemPoint);
                            }

                            TotalPoint = calcMemberPoint(strMemberID, sales.SalesDate.ToString("yyyy-MM-dd"), dblEarnPoint, TotalMemberSpending);
                        }
                        objCnn.Close();

                        return new JavaScriptSerializer().Serialize(new { Status = "Success", MemberID = strMemberID, MemberBalPoint = TotalPoint, RedeemPoint = TotalRP, MemberPurchase = TotalMemberSpending});
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        return ex.ToString();
                    }
                }
            }
            else
            {
                return "Error: Duplicate sales record.";
            }
        }
    }


	public DataTable GetData_Vapt(string query, MySqlParameter[] objparam)
    {
        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
        {
            objCnn.Open();

            MySqlCommand objcmd = PrepareCommand(objCnn, CommandType.Text, query, objparam);

            MySqlDataAdapter objDA = new MySqlDataAdapter(objcmd);

            using (DataTable dt = new DataTable())
            {
                objDA.Fill(dt);
                return dt;
            }
        }
    }

    public DataTable GetData(string query)
    {
        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
        {
            objCnn.Open();

            MySqlDataAdapter objDA = new MySqlDataAdapter(query, objCnn);
            objDA.SelectCommand.CommandType = CommandType.Text;

            using (DataTable dt = new DataTable())
            {
                objDA.Fill(dt);
                return dt;
            }
			objCnn.Close();
        }
    }
	
	public decimal calcPromoPrice(string strSql, decimal Qty)
    {
        decimal newSellingPrice = 0, SellingPrice = 0;
        decimal MaxTierAmt = 0, MaxTierPerc = 0, MaxTierMemberAmt = 0, MaxTierMemberPerc = 0;
        decimal ItemQty = 0, Amt = 0, Perc = 0, MemberAmt = 0, MemberPerc = 0, ItemQty2 = 0, Amt2 = 0, Perc2 = 0, MemberAmt2 = 0, MemberPerc2 = 0, ItemQty3 = 0, Amt3 = 0, Perc3 = 0, MemberAmt3 = 0, MemberPerc3 = 0;
        decimal ItemQty4 = 0, Amt4 = 0, Perc4 = 0, MemberAmt4 = 0, MemberPerc4 = 0, ItemQty5 = 0, Amt5 = 0, Perc5 = 0, MemberAmt5 = 0, MemberPerc5 = 0, ItemQty6 = 0, Amt6 = 0, Perc6 = 0, MemberAmt6 = 0, MemberPerc6 = 0;
        decimal ItemQty7 = 0, Amt7 = 0, Perc7 = 0, MemberAmt7 = 0, MemberPerc7 = 0, ItemQty8 = 0, Amt8 = 0, Perc8 = 0, MemberAmt8 = 0, MemberPerc8 = 0, ItemQty9 = 0, Amt9 = 0, Perc9 = 0, MemberAmt9 = 0, MemberPerc9 = 0;
        decimal ItemQty10 = 0, Amt10 = 0, Perc10 = 0, MemberAmt10 = 0, MemberPerc10 = 0;


        DataTable dt = GetData(strSql);
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            SellingPrice = Convert.ToDecimal(dt.Rows[i]["RTLSellPx"]);
            ItemQty = Convert.ToDecimal(dt.Rows[i]["Item_Qty"]);
            Amt = Convert.ToDecimal(dt.Rows[i]["Item_Amt"]);
            Perc = Convert.ToDecimal(dt.Rows[i]["Item_Percentage"]);
            MemberAmt = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt"]);
            MemberPerc = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc"]);
            ItemQty2 = Convert.ToDecimal(dt.Rows[i]["Item_Qty2"]);
            Amt2 = Convert.ToDecimal(dt.Rows[i]["Item_Amt2"]);
            Perc2 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage2"]);
            MemberAmt2 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt2"]);
            MemberPerc2 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc2"]);
            ItemQty3 = Convert.ToDecimal(dt.Rows[i]["Item_Qty3"]);
            Amt3 = Convert.ToDecimal(dt.Rows[i]["Item_Amt3"]);
            Perc3 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage3"]);
            MemberAmt3 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt3"]);
            MemberPerc3 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc3"]);
            ItemQty4 = Convert.ToDecimal(dt.Rows[i]["Item_Qty4"]);
            Amt4 = Convert.ToDecimal(dt.Rows[i]["Item_Amt4"]);
            Perc4 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage4"]);
            MemberAmt4 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt4"]);
            MemberPerc4 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc4"]);
            ItemQty5 = Convert.ToDecimal(dt.Rows[i]["Item_Qty5"]);
            Amt5 = Convert.ToDecimal(dt.Rows[i]["Item_Amt5"]);
            Perc5 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage5"]);
            MemberAmt5 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt5"]);
            MemberPerc5 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc5"]);
            ItemQty6 = Convert.ToDecimal(dt.Rows[i]["Item_Qty6"]);
            Amt6 = Convert.ToDecimal(dt.Rows[i]["Item_Amt6"]);
            Perc6 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage6"]);
            MemberAmt6 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt6"]);
            MemberPerc6 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc6"]);
            ItemQty7 = Convert.ToDecimal(dt.Rows[i]["Item_Qty7"]);
            Amt7 = Convert.ToDecimal(dt.Rows[i]["Item_Amt7"]);
            Perc7 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage7"]);
            MemberAmt7 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt7"]);
            MemberPerc7 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc7"]);
            ItemQty8 = Convert.ToDecimal(dt.Rows[i]["Item_Qty8"]);
            Amt8 = Convert.ToDecimal(dt.Rows[i]["Item_Amt8"]);
            Perc8 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage8"]);
            MemberAmt8 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt8"]);
            MemberPerc8 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc8"]);
            ItemQty9 = Convert.ToDecimal(dt.Rows[i]["Item_Qty9"]);
            Amt9 = Convert.ToDecimal(dt.Rows[i]["Item_Amt9"]);
            Perc9 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage9"]);
            MemberAmt9 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt9"]);
            MemberPerc9 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc9"]);
            ItemQty10 = Convert.ToDecimal(dt.Rows[i]["Item_Qty10"]);
            Amt10 = Convert.ToDecimal(dt.Rows[i]["Item_Amt10"]);
            Perc10 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage10"]);
            MemberAmt10 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt10"]);
            MemberPerc10 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc10"]);
        }

        if ((ItemQty10 != 0 && ItemQty10 > ItemQty9) && Qty >= ItemQty10)
        {
                MaxTierAmt = Amt10;
                MaxTierPerc = Perc10;
                MaxTierMemberAmt = MemberAmt10;
                MaxTierMemberPerc = MemberPerc10;
        }
        else if ((ItemQty9 != 0 && ItemQty9 > ItemQty8) && Qty >= ItemQty9)
        {
                MaxTierAmt = Amt9;
                MaxTierPerc = Perc9;
                MaxTierMemberAmt = MemberAmt9;
                MaxTierMemberPerc = MemberPerc9;
        }
        else if ((ItemQty8 != 0 && ItemQty8 > ItemQty7) && Qty >= ItemQty8)
        {
                MaxTierAmt = Amt8;
                MaxTierPerc = Perc8;
                MaxTierMemberAmt = MemberAmt8;
                MaxTierMemberPerc = MemberPerc8;
        }
        else if ((ItemQty7 != 0 && ItemQty7 > ItemQty6) && Qty >= ItemQty7)
        {
                MaxTierAmt = Amt7;
                MaxTierPerc = Perc7;
                MaxTierMemberAmt = MemberAmt7;
                MaxTierMemberPerc = MemberPerc7;
        }
        else if ((ItemQty6 != 0 && ItemQty6 > ItemQty5) && Qty >= ItemQty6)
        {
                MaxTierAmt = Amt6;
                MaxTierPerc = Perc6;
                MaxTierMemberAmt = MemberAmt6;
                MaxTierMemberPerc = MemberPerc6;
        }
        else if ((ItemQty5 != 0 && ItemQty5 > ItemQty4) && Qty >= ItemQty5)
        {
                MaxTierAmt = Amt5;
                MaxTierPerc = Perc5;
                MaxTierMemberAmt = MemberAmt5;
                MaxTierMemberPerc = MemberPerc5;
        }
        else if ((ItemQty4 != 0 && ItemQty4 > ItemQty3) && Qty >= ItemQty4)
        {
                MaxTierAmt = Amt4;
                MaxTierPerc = Perc4;
                MaxTierMemberAmt = MemberAmt4;
                MaxTierMemberPerc = MemberPerc4;
        }
        else if ((ItemQty3 != 0 && ItemQty3 > ItemQty2) && Qty >= ItemQty3)
        {
                MaxTierAmt = Amt3;
                MaxTierPerc = Perc3;
                MaxTierMemberAmt = MemberAmt3;
                MaxTierMemberPerc = MemberPerc3;
        }
        else if ((ItemQty2 != 0 && ItemQty2 > ItemQty) && Qty >= ItemQty2)
        {
                MaxTierAmt = Amt2;
                MaxTierPerc = Perc2;
                MaxTierMemberAmt = MemberAmt2;
                MaxTierMemberPerc = MemberPerc2;
        }
        else if ((ItemQty != 0 && ItemQty < ItemQty2) && Qty >= ItemQty)
        {
            MaxTierAmt = Amt;
            MaxTierPerc = Perc;
            MaxTierMemberAmt = MemberAmt;
            MaxTierMemberPerc = MemberPerc;
        }
        else
        {
            newSellingPrice = SellingPrice * Qty;
        }

        string member = "N";
        //Calculate the promo price
        if (member == "Y")
        {
            if (MaxTierPerc != 0 && MaxTierMemberPerc != 0)
            {
                newSellingPrice = SellingPrice - (SellingPrice * MaxTierPerc / 100);
                newSellingPrice = (newSellingPrice - (newSellingPrice * MaxTierMemberPerc / 100)) * Qty;
            }
            else if (MaxTierAmt != 0 && MaxTierMemberAmt != 0)
            {
                newSellingPrice = (SellingPrice - MaxTierAmt - MaxTierMemberAmt) * Qty;
            }
        }
        else
        {
            if (MaxTierAmt != 0)
            {
                if (MaxTierMemberAmt == 0)
                {
                    newSellingPrice = (SellingPrice - MaxTierAmt) * Qty;
                }
            }
            else if (MaxTierPerc != 0)
            {
                if (MaxTierMemberPerc == 0)
                {
                    newSellingPrice = (SellingPrice - (SellingPrice * MaxTierPerc / 100)) * Qty;
                }
            }
            if (member == "Y")
            {
                if (MaxTierMemberAmt != 0)
                {
                    if (MaxTierAmt == 0)
                    {
                        newSellingPrice = (SellingPrice - MaxTierMemberAmt) * Qty;
                    }
                }
                else if (MaxTierMemberPerc != 0)
                {
                    if (MaxTierPerc == 0)
                    {
                        newSellingPrice = (SellingPrice - (SellingPrice * MaxTierMemberPerc / 100)) * Qty;
                    }
                }
            }
        }
        return newSellingPrice;
    }
	
	public string saveOrders(SalesOrder orders)
    {
        string response = "";
        //Retail sales order
        

        if (orders.RefID == null)
        {
            return "Missing detail: Ref. ID.";
        }
        else if (orders.RefNo == null)
        {
            return "Missing detail: Ref. No.";
        }
        else if (orders.QueueNo == null)
        {
            return "Missing detail: Queue No.";
        }
        else if (orders.RetailID == null)
        {
            return "Missing detail: Retail ID.";
        }
        else if (orders.TerminalID == null)
        {
            return "Missing detail: Terminal ID.";
        }
        else if (orders.ItemOrder.Count() == 0)
        {
            return "Missing order details.";
        }
        else
        {
            string sqlstr = "SELECT * FROM retail_sales_order WHERE RecordStatus <> 'DELETED' AND RefID=@RefID AND RefNo=@RefNo AND RetailID=@RetailID";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@RefID", orders.RefID),
                new MySqlParameter("@RefNo", orders.RefNo),
                new MySqlParameter("@RetailID", orders.RetailID)
            };

            DataTable OrderDT = GetData_Vapt(sqlstr, objparam);

            if (OrderDT.Rows.Count == 0)
            {
                for (int i = 0; i < orders.ItemOrder.Count(); i++)
                {
                    response = checkItem(orders.ItemOrder[i].ParentID.ToString(),orders.ItemOrder[i].ItemUOM.ToString());
                    if (response == "OK")
                    {
                        Guid ID = Guid.NewGuid();
                        int childCount = orders.ItemOrder[i].ItemOrderDetails.Count();

                        if (childCount != 0)
                        {
                            //insert into DB with child item details
                            for (int j = 0; j < orders.ItemOrder[i].ItemOrderDetails.Count(); j++)
                            {
                                ID = Guid.NewGuid();
                                response = checkItem(orders.ItemOrder[i].ItemOrderDetails[j].ItemID.ToString());
                                if (response == "OK")
                                {
                                    if (orders.ItemOrder[i].ItemOrderDetails[j].ItemQty == 0 || orders.ItemOrder[i].ParentQty == 0)
                                    {
                                        return "Wrong item qty.";
                                        break;
                                    }
                                    else
                                    {
                                        sqlstr = "SELECT inventory.ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode" +
                                            " FROM inventory " +
                                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID=@ItemID LIMIT 1";
                                        MySqlParameter[] objparam1 =
                                        {
                                            new MySqlParameter("@ItemID", orders.ItemOrder[i].ParentID)
                                        };
                                        DataTable ItemDT = GetData_Vapt(sqlstr, objparam1);

                                        string queryInsertOrder = "INSERT INTO retail_sales_order " +
                                            "(ID, RefID, RefNo, QueueNo, RetailID, OrderDate, OrderTime, ParentItemID, ParentQty, ItemID, SupBarCode, ItemQty, ItemUOM, PrepareStatus, TerminalID, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                            " VALUE " +
                                            "(@ID, @RefID, @RefNo, @QueueNo, @RetailID, @OrderDate, @OrderTime, @ParentItemID, @ParentQty, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @PrepareStatus, @TerminalID, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                                        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                                        {
                                            try
                                            {
                                                objCnn.Open();
                                                using (MySqlCommand cmd = new MySqlCommand(queryInsertOrder, objCnn))
                                                {
                                                    cmd.Parameters.AddWithValue("@ID", ID);
                                                    cmd.Parameters.AddWithValue("@RefID", orders.RefID.ToString());
                                                    cmd.Parameters.AddWithValue("@RefNo", orders.RefNo.ToString());
                                                    cmd.Parameters.AddWithValue("@QueueNo", orders.QueueNo.ToString());
                                                    cmd.Parameters.AddWithValue("@RetailID", orders.RetailID.ToString());
                                                    cmd.Parameters.AddWithValue("@TerminalID", orders.TerminalID.ToString());
                                                    cmd.Parameters.AddWithValue("@OrderDate", DateTime.Today);
                                                    cmd.Parameters.AddWithValue("@OrderTime", DateTime.Now);
                                                    cmd.Parameters.AddWithValue("@ParentItemID", orders.ItemOrder[i].ParentID.ToString());
                                                    cmd.Parameters.AddWithValue("@ParentQty", orders.ItemOrder[i].ParentQty);
                                                    cmd.Parameters.AddWithValue("@SupBarCode", Convert.ToString(ItemDT.Rows[0]["SupBarCode"]));
                                                    cmd.Parameters.AddWithValue("@ItemUOM", orders.ItemOrder[i].ItemUOM.ToString());
                                                    cmd.Parameters.AddWithValue("@ItemID", orders.ItemOrder[i].ItemOrderDetails[j].ItemID.ToString());
                                                    cmd.Parameters.AddWithValue("@ItemQty", orders.ItemOrder[i].ItemOrderDetails[j].ItemQty);
                                                    cmd.Parameters.AddWithValue("@PrepareStatus", "Y");
                                                    cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                                    cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                                    cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                                    cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                                    cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                                    cmd.ExecuteNonQuery();
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                return ex.ToString();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    return response;
                                }
                            }
                        }
                        else
                        {
                            //insert into DB without child item details
                            if (orders.ItemOrder[i].ParentQty == null || orders.ItemOrder[i].ParentQty == 0)
                            {
                                return "Wrong item qty.2";
                                break;
                            }
                            else
                            {
                                sqlstr = "SELECT inventory.ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode" +
                                    " FROM inventory " +
                                    " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                                    " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                                    " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID=@ItemID LIMIT 1";

                                MySqlParameter[] objparam1 =
                                {
                                    new MySqlParameter("@ItemID", orders.ItemOrder[i].ParentID)
                                };
                                DataTable ParentDT = GetData_Vapt(sqlstr, objparam1);

                                string queryInsertOrder = "INSERT INTO retail_sales_order " +
                                    "(ID, RefID, RefNO, QueueNo, RetailID, OrderDate, OrderTime, ParentItemID, ParentQty, ItemID, SupBarCode, ItemQty, ItemUOM, PrepareStatus, TerminalID, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                                    " VALUE " +
                                    "(@ID, @RefID, @RefNO, @QueueNo, @RetailID, @OrderDate, @OrderTime, @ParentItemID, @ParentQty, @ItemID, @SupBarCode, @ItemQty, @ItemUOM, @PrepareStatus, @TerminalID, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                                {
                                    try
                                    {
                                        objCnn.Open();
                                        using (MySqlCommand cmd = new MySqlCommand(queryInsertOrder, objCnn))
                                        {
                                            cmd.Parameters.AddWithValue("@ID", ID);
                                            cmd.Parameters.AddWithValue("@RefID", orders.RefID.ToString());
                                            cmd.Parameters.AddWithValue("@RefNo", orders.RefNo.ToString());
                                            cmd.Parameters.AddWithValue("@QueueNo", orders.QueueNo.ToString());
                                            cmd.Parameters.AddWithValue("@RetailID", orders.RetailID.ToString());
                                            cmd.Parameters.AddWithValue("@TerminalID", orders.TerminalID.ToString());
                                            cmd.Parameters.AddWithValue("@OrderDate", DateTime.Today);
                                            cmd.Parameters.AddWithValue("@OrderTime", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@ParentItemID", orders.ItemOrder[i].ParentID.ToString());
                                            cmd.Parameters.AddWithValue("@ParentQty", orders.ItemOrder[i].ParentQty);
                                            cmd.Parameters.AddWithValue("@SupBarCode", Convert.ToString(ParentDT.Rows[0]["SupBarCode"]));
                                            cmd.Parameters.AddWithValue("@ItemUOM", orders.ItemOrder[i].ItemUOM.ToString());
                                            cmd.Parameters.AddWithValue("@ItemID", orders.ItemOrder[i].ParentID.ToString());
                                            cmd.Parameters.AddWithValue("@ItemQty", orders.ItemOrder[i].ParentQty);
                                            cmd.Parameters.AddWithValue("@PrepareStatus", "Y");
                                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        return ex.ToString();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return response;
                    }
                }
            }
            else
            {
                return "Error: Duplicate order record found.";
            }
            return "Success";
        }
    }
	
	public string cancelOrders(CancelOrder calOrder)
    {
        if (calOrder.RefID == null)
        {
            return "Missing detail: Ref. ID.";
        }
        else if (calOrder.RefNo == null)
        {
            return "Missing detail: Ref. No.";
        }
        else if (calOrder.RetailID == null)
        {
            return "Missing detail: Retail ID.";
        }
        else if (calOrder.ParentItemID == null)
        {
            return "Missing detail: Parent Item ID.";
        }
        else
        {
            string sqlstr = "SELECT * FROM retail_sales_order WHERE RecordStatus <> 'DELETED' AND RefID=@RefID AND RefNo=@RefNo AND RetailID=@RetailID AND ParentItemID=@ParentItemID";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@RefID", calOrder.RefID),
                new MySqlParameter("@RefNo", calOrder.RefNo),
                new MySqlParameter("@RetailID", calOrder.RetailID),
                new MySqlParameter("@ParentItemID", calOrder.ParentItemID),
            };
            DataTable OrderDT = GetData_Vapt(sqlstr, objparam);
            if (OrderDT.Rows.Count != 0)
            {
                string queryUpdateOrder = "Update retail_sales_order SET CancelStatus = @CancelStatus, RefundStatus = @RefundStatus, LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                                        " WHERE RefID=@RefID AND RefNo=@RefNo AND RetailID=@RetailID AND ParentItemID=@ParentItemID";

                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();

                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateOrder, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@CancelStatus", "Y");
                            cmd.Parameters.AddWithValue("@RefundStatus", "Y");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RefID", calOrder.RefID);
                            cmd.Parameters.AddWithValue("@RefNo", calOrder.RefNo);
                            cmd.Parameters.AddWithValue("@RetailID", calOrder.RetailID);
                            cmd.Parameters.AddWithValue("@ParentItemID", calOrder.ParentItemID);
                            cmd.ExecuteNonQuery();
                        }

                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }
                }
                return "Success";
            }
            else
            {
                return "Error: Record not found.";
            }
        }
    }
	
	public string savePD(purchaseDelivery PD)
    {
        Guid PDKeyCol = Guid.NewGuid();

        if (PD.IDRef == null)
        {
            return "Missing detail: ID Ref.";
        }
        else if (PD.DONo == null)
        {
            return "Missing detail: DO No.";
        }
        else if (PD.PDItems.Count() == 0)
        {
            return "Missing purchase delivery items detail.";
        }
        else
        {
            string sqlstr = "SELECT * FROM prch_delivery WHERE RecordStatus <> 'DELETED' AND IDRef=@IDRef AND DONo=@DONo";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@IDRef", PD.IDRef),
                new MySqlParameter("@DONo", PD.DONo)
            };
            DataTable pdDT = GetData_Vapt(sqlstr, objparam);
            if (pdDT.Rows.Count != 0)
            {
                //retrieve from PD
                string PD_ID = Convert.ToString(pdDT.Rows[0]["ID"]);
                string PD_Exch = Convert.ToString(pdDT.Rows[0]["Exch"]);
                decimal PD_ExchRate = Convert.ToDecimal(pdDT.Rows[0]["ExchRate"]);
                string PD_Gst = Convert.ToString(pdDT.Rows[0]["Gst"]);
                string PD_GSTIncEx = Convert.ToString(pdDT.Rows[0]["GSTIncEx"]);
                decimal PD_GstRate = Convert.ToDecimal(pdDT.Rows[0]["GstRate"]);

                
                for (int i = 0; i < PD.PDItems.Count(); i++)
                {
                    PDKeyCol = Guid.NewGuid();
                    //retrieve from inventory
                    sqlstr = "SELECT inventory.ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode AS SupBarItem, inventory_supbar.Item_SupBarID AS SupBarItemID, inventory_unit.ItemUnit AS ItemUnitID, inventory_unit.ItemActQty AS ActualQty, B.ItemUnit AS ItemBaseUnitID, C.Nick AS ItemBaseUnit" +
                            " FROM inventory " +
                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                            " LEFT JOIN (SELECT inventory.ItemID, inventory_unit.ItemUnit " +
                            " FROM inventory_unit " +
                            " LEFT JOIN inventory ON inventory_unit.ItemID = inventory.ItemID " +
                            " WHERE inventory_unit.RecordStatus <> 'DELETED' AND inventory_unit.ItemActQty = 1 AND inventory.ItemSKU=@ItemSKU ) AS B ON B.ItemID = inventory.ItemID " +
                            " LEFT JOIN list_units C ON B.ItemUnit = C.ID " +
                            " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemSKU=@ItemSKU AND list_units.Nick=@ItemUOM";
                    MySqlParameter[] objparam1 =
                    {
                        new MySqlParameter("@ItemSKU", PD.PDItems[i].ItemSKU),
                        new MySqlParameter("@ItemUOM", PD.PDItems[i].ItemUOM)
                    };

                    DataTable itemDT = GetData_Vapt(sqlstr, objparam1);

                    if (itemDT.Rows.Count != 0)
                    {
                        if (PD.PDItems[i].ItemQty == 0 || PD.PDItems[i].ItemQty == null)
                        {
                            return "Error: Wrong item qty.";
                        }
                        else if (PD.PDItems[i].ItemPrice == 0 || PD.PDItems[i].ItemPrice == null)
                        {
                            return "Error: Wrong item price.";
                        }
                        else
                        {
                            decimal Tax = 0;
                            decimal SubTotal = 0;
                            decimal Total = 0;

                            //Include GST
                            if (PD_Gst == "Y" && PD_GSTIncEx == "1")
                            {
                                Tax = (PD.PDItems[i].ItemPrice * PD.PDItems[i].ItemQty) * PD_GstRate / (100 + PD_GstRate);
                                Total = PD.PDItems[i].ItemPrice * PD.PDItems[i].ItemQty;
                                SubTotal = Total;
                            }
                            //Exclude GST
                            else if (PD_Gst == "Y" && PD_GSTIncEx == "2")
                            {
                                Tax = (PD.PDItems[i].ItemPrice * PD.PDItems[i].ItemQty) * PD_GstRate / 100;
                                Total = (PD.PDItems[i].ItemPrice * PD.PDItems[i].ItemQty) + Tax;
                                SubTotal = (PD.PDItems[i].ItemPrice * PD.PDItems[i].ItemQty);
                            }
                            else
                            {
                                Tax = 0;
                                Total = PD.PDItems[i].ItemPrice * PD.PDItems[i].ItemQty;
                                SubTotal = Total;
                            }

                            string queryInsertPDItem = "INSERT INTO prch_delivery_item " +
                                    "(KeyCol,ItemID,ItemSKU,SupBarItem,SupBarItemID,ID,ItemQty,ActualQty,Currency,ExchRate,GST,GSTRate,ItemPrice,ItemUnit,ItemUnitID,ItemBaseUnit,ItemBaseUnitID,ItemBal,ItemSubTotal,ItemGST,Total,LocalItemPrice,LocalItemSubTotal,LocalItemGST,LocalTotal,ItemFOC,dteDelivery,LastUpdate,RecordStatus,RecordUpdate,QueueStatus)" +
                                    " VALUE " +
                                    "(@KeyCol,@ItemID,@ItemSKU,@SupBarItem,@SupBarItemID,@ID,@ItemQty,@ActualQty,@Currency,@ExchRate,@GST,@GSTRate,@ItemPrice,@ItemUnit,@ItemUnitID,@ItemBaseUnit,@ItemBaseUnitID,@ItemBal,@ItemSubTotal,@ItemGST,@Total,@LocalItemPrice,@LocalItemSubTotal,@LocalItemGST,@LocalTotal,@ItemFOC,@dteDelivery,@LastUpdate,@RecordStatus,@RecordUpdate,@QueueStatus)";
                            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                            {
                                try
                                {
                                    objCnn.Open();
                                    using (MySqlCommand cmd = new MySqlCommand(queryInsertPDItem, objCnn))
                                    {
                                        cmd.Parameters.AddWithValue("@KeyCol", PDKeyCol);
                                        cmd.Parameters.AddWithValue("@ItemID", Convert.ToString(itemDT.Rows[0]["ItemID"]));
                                        cmd.Parameters.AddWithValue("@ItemSKU", PD.PDItems[i].ItemSKU.ToString());
                                        cmd.Parameters.AddWithValue("@SupBarItem", PD.PDItems[i].SupBarCode.ToString());
                                        cmd.Parameters.AddWithValue("@SupBarItemID", Convert.ToString(itemDT.Rows[0]["SupBarItemID"]));
                                        cmd.Parameters.AddWithValue("@ID", PD_ID);
                                        cmd.Parameters.AddWithValue("@ItemQty", PD.PDItems[i].ItemQty);
                                        cmd.Parameters.AddWithValue("@ActualQty", Convert.ToDecimal(itemDT.Rows[0]["ActualQty"]));
                                        cmd.Parameters.AddWithValue("@Currency", PD_Exch);
                                        cmd.Parameters.AddWithValue("@ExchRate", PD_ExchRate);
                                        cmd.Parameters.AddWithValue("@GST", PD_Gst);
                                        cmd.Parameters.AddWithValue("@GSTRate", PD_GstRate);
                                        cmd.Parameters.AddWithValue("@ItemPrice", PD.PDItems[i].ItemPrice);
                                        cmd.Parameters.AddWithValue("@ItemUnit", PD.PDItems[i].ItemUOM.ToString());
                                        cmd.Parameters.AddWithValue("@ItemUnitID", Convert.ToString(itemDT.Rows[0]["ItemUnitID"]));
                                        cmd.Parameters.AddWithValue("@ItemBaseUnit", Convert.ToString(itemDT.Rows[0]["ItemBaseUnit"]));
                                        cmd.Parameters.AddWithValue("@ItemBaseUnitID", Convert.ToString(itemDT.Rows[0]["ItemBaseUnitID"]));
                                        cmd.Parameters.AddWithValue("@ItemBal", PD.PDItems[i].ItemQty);
                                        cmd.Parameters.AddWithValue("@ItemSubTotal", SubTotal);
                                        cmd.Parameters.AddWithValue("@ItemGST", Tax);
                                        cmd.Parameters.AddWithValue("@Total", Total);
                                        cmd.Parameters.AddWithValue("@LocalItemPrice", PD.PDItems[i].ItemPrice * PD_ExchRate);
                                        cmd.Parameters.AddWithValue("@LocalItemSubTotal", SubTotal * PD_ExchRate);
                                        cmd.Parameters.AddWithValue("@LocalItemGST", Tax * PD_ExchRate);
                                        cmd.Parameters.AddWithValue("@LocalTotal", Total * PD_ExchRate);
                                        cmd.Parameters.AddWithValue("@ItemFOC", "N");
                                        cmd.Parameters.AddWithValue("@dteDelivery", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                        cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@QueueStatus", "READY");
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return ex.ToString();
                                }
                            }

                            //update PD
                            sqlstr = "SELECT * FROM prch_delivery WHERE RecordStatus <> 'DELETED' AND IDRef=@IDRef AND DONo=@DONo";
                            MySqlParameter[] objparam2 =
                            {
                                new MySqlParameter("@IDRef", PD.IDRef),
                                new MySqlParameter("@DONo", PD.DONo)
                            };

                            DataTable newPDDT = GetData_Vapt(sqlstr, objparam2);
            
                            string queryUpdatePD = "Update prch_delivery " +
                                        " SET BalSubTotal=@BalSubTotal, BalTax=@BalTax, BalTotal=@BalTotal, BalPayable=@BalPayable, LocalBalSubTotal=@LocalBalSubTotal, LocalTax=@LocalTax, LocalTotal=@LocalTotal, LocalBalPayable=@LocalBalPayable, LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                                        " WHERE RecordStatus <> 'DELETED' AND IDRef=@IDRef AND DONo=@DONo";

                            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                            {
                                try
                                {
                                    objCnn.Open();

                                    using (MySqlCommand cmd = new MySqlCommand(queryUpdatePD, objCnn))
                                    {
                                        cmd.Parameters.AddWithValue("@BalSubTotal", Convert.ToDecimal(newPDDT.Rows[0]["BalSubTotal"]) + SubTotal);
                                        cmd.Parameters.AddWithValue("@BalTax", Convert.ToDecimal(newPDDT.Rows[0]["BalTax"]) + Tax);
                                        cmd.Parameters.AddWithValue("@BalTotal", Convert.ToDecimal(newPDDT.Rows[0]["BalTotal"]) + Total);
                                        cmd.Parameters.AddWithValue("@BalPayable", Convert.ToDecimal(newPDDT.Rows[0]["BalPayable"]) + Total);
                                        cmd.Parameters.AddWithValue("@LocalBalSubTotal", Convert.ToDecimal(newPDDT.Rows[0]["LocalBalSubTotal"]) + (SubTotal * PD_ExchRate));
                                        cmd.Parameters.AddWithValue("@LocalTax", Convert.ToDecimal(newPDDT.Rows[0]["LocalTax"]) + (Tax * PD_ExchRate));
                                        cmd.Parameters.AddWithValue("@LocalTotal", Convert.ToDecimal(newPDDT.Rows[0]["LocalTotal"]) + (Total * PD_ExchRate));
                                        cmd.Parameters.AddWithValue("@LocalBalPayable", Convert.ToDecimal(newPDDT.Rows[0]["LocalBalPayable"]) + (Total * PD_ExchRate));
                                        cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@IDRef", PD.IDRef.ToString());
                                        cmd.Parameters.AddWithValue("@DONo", PD.DONo.ToString());
                                        cmd.ExecuteNonQuery();
                                    }

                                }
                                catch (Exception ex)
                                {
                                    return ex.ToString();
                                }
                            }
                            //return "success";
                        }
                    }
                    else
                    {
                        return "Error: ItemSKU (" + PD.PDItems[i].ItemSKU + ") not found.";
                    }
                }
            }
            else
            {
                return "Error: Record not found.";
            }
            return "success";
        }
    }
	
	public string saveSalesDelivery_v3(SalesDelivery SD)
	{
		string response = "";
		string SD_ID = "";

		if (SD.INVID == null)
		{
			return "Missing detail: Inv ID.";
		}
		else if (SD.INVRef == null)
		{
			return "Missing detail: Inv Ref.";
		}
		else if (SD.INVDate == null)
		{
			return "Missing detail: Trans Date.";
		}
		else if (SD.RecipientName == null)
		{
			return "Missing detail: Recipient Name.";
		}
		else if (SD.RecipientAddr == null)
		{
			return "Missing detail: Recipient Address.";
		}
		else if (SD.RecipientPostCode == null)
		{
			return "Missing detail: Recipient Postcode.";
		}
		else if (SD.RecipientAttn == null)
		{
			return "Missing detail: Recipient Attn.";
		}
		else if (SD.RecipientTel == null)
		{
			return "Missing detail: Recipient Tel No.";
		}
		else if (SD.Remarks == null)
		{
			return "Missing detail: Remark.";
		}
		else if (SD.GST != "Y" && SD.GST != "N")
		{
			return "Invalid GST: Only accept Y or N.";
		}
		else
		{
            string sqlstr = "SELECT * FROM salesdelivery WHERE RecordStatus <> 'DELETED'  AND INVID=@INVID AND INVRef=@INVRef";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@INVID", SD.INVID),
                new MySqlParameter("@INVRef", SD.INVRef)
            };
            DataTable sdDT = GetData_Vapt(sqlstr, objparam);
			if (sdDT.Rows.Count > 0)
			{
				string FinalPaymentFlag = sdDT.Rows[0]["BITFINALPAYMENT"].ToString();
				string IDSTR = sdDT.Rows[0]["ID"].ToString();

				if (FinalPaymentFlag == "Y")
				{
					return "success";
				}
				else
				{
					sqlstr = string.Format("DELETE FROM SALESDELIVERY WHERE ID = '{0}' ; ", IDSTR);
					sqlstr += string.Format("DELETE FROM SALESDELIVERY_ITEM WHERE ID = '{0}' ;", IDSTR);
					using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
					{
						try
						{
							objCnn.Open();

							using (MySqlCommand cmd = new MySqlCommand(sqlstr, objCnn))
							{
								cmd.ExecuteNonQuery();
							}
						}
						catch (Exception ex)
						{
							return ex.ToString();
						}
					}
				}
			}

			clsDataAccessor MyDA = new clsDataAccessor();
			MySqlParameter[] objParam =  
				{          
					MyDA.OutputParam("pLastID",MySqlDbType.Double,10),
					MyDA.InputParam("pType", MySqlDbType.VarChar,3,SD.Type),
					MyDA.InputParam("pDate", MySqlDbType.Date,10,SD.INVDate),  
					MyDA.InputParam("pCompanyAddr",MySqlDbType.Text,1000, SD.CompanyAddr),   
					MyDA.InputParam("pRecipientAddr",MySqlDbType.Text,1000,SD.RecipientAddr),
					MyDA.InputParam("pSN_Ref",MySqlDbType.VarChar,25,SD.SN_Ref),
					MyDA.InputParam("pRecipientName",MySqlDbType.VarChar,200,SD.RecipientName),
					MyDA.InputParam("pRecipientAttn",MySqlDbType.VarChar,200,SD.RecipientAttn),
					MyDA.InputParam("pRecipientTel",MySqlDbType.VarChar,50,SD.RecipientTel),
					MyDA.InputParam("pRecipientFax",MySqlDbType.VarChar,50,SD.RecipientFax),
					MyDA.InputParam("pCompanyTel",MySqlDbType.VarChar,50,SD.CompanyTel),
					MyDA.InputParam("pCompanyFax",MySqlDbType.VarChar,50,SD.CompanyFax),
					MyDA.InputParam("pRecipientPostcode",MySqlDbType.VarChar,10,SD.RecipientPostCode),
					MyDA.InputParam("pIDRef",MySqlDbType.VarChar,25,SD.IDRef), 
					MyDA.InputParam("pRetailerID",MySqlDbType.Int32,4,SD.RetailerID),                                    
					MyDA.InputParam("pINVID",MySqlDbType.Double,10,SD.INVID),                     
					MyDA.InputParam("pINVRef",MySqlDbType.VarChar,25,SD.INVRef),
					MyDA.InputParam("pINVDate", MySqlDbType.Date,10,SD.INVDate),  
					MyDA.InputParam("pINVRetailerID", MySqlDbType.Int32,4,SD.INVRetailerID),  
					MyDA.InputParam("pGst",MySqlDbType.VarChar,1,SD.GST),   
					MyDA.InputParam("pGSTIncEx",MySqlDbType.VarChar,10, SD.GSTIncEx),
					MyDA.InputParam("pGstRate", MySqlDbType.Decimal,12,SD.GstRate),
					MyDA.InputParam("pBalSubTotal", MySqlDbType.Decimal,12,SD.BalSubTotal),
					MyDA.InputParam("pBalTax", MySqlDbType.Decimal,12,SD.BalTax),
					MyDA.InputParam("pTotalDiscount",MySqlDbType.Decimal,12,SD.TotalDiscount),  
					MyDA.InputParam("pBalTotal", MySqlDbType.Decimal,12,SD.BalTotal),
					MyDA.InputParam("pBalPayable",MySqlDbType.Decimal,12,SD.BalPayable),   
					MyDA.InputParam("pLocalBalSubTotal",MySqlDbType.Decimal,12,SD.LocalBalSubTotal),
					MyDA.InputParam("pLocalTax",MySqlDbType.Decimal,12,SD.LocalTax),   
					MyDA.InputParam("pLocalTotalDiscount",MySqlDbType.Decimal,12,SD.LocalTotalDiscount),  
					MyDA.InputParam("pLocalTotal",MySqlDbType.Decimal,12,SD.LocalTotal),  
					MyDA.InputParam("pLocalBalPayable",MySqlDbType.Decimal,12,SD.LocalBalPayable),   
					MyDA.InputParam("pRemarks",MySqlDbType.Text,1000,SD.Remarks),                                                       
					MyDA.InputParam("pDocument_Status",MySqlDbType.VarChar,30,SD.Document_Status), 
					MyDA.InputParam("pOutStandingBal",MySqlDbType.Decimal,12,SD.OutStandingBal),                                    
					MyDA.InputParam("pLocalOutStandingBal",MySqlDbType.Decimal,12,SD.LocalOutStandingBal), 
					MyDA.InputParam("pDepositAmount",MySqlDbType.Decimal,12,SD.DepositAmount), 
					MyDA.InputParam("pLastUser",MySqlDbType.VarChar,50,SD.LastUser),                            
					MyDA.InputParam("pLastUpdate",MySqlDbType.Timestamp,10, SD.LastUpdate), 
					MyDA.InputParam("pLockUser",MySqlDbType.VarChar,50,SD.LockUser), 
					MyDA.InputParam("pLockUpdate",MySqlDbType.Timestamp,20,SD.LockUpdate), 
					MyDA.InputParam("pLockStatus",MySqlDbType.VarChar,30, "0"),
					MyDA.InputParam("pRecordStatus",MySqlDbType.VarChar,30,SD.RecordStatus),
					MyDA.InputParam("pRecordUpdate",MySqlDbType.Timestamp,20,SD.RecordUpdate),                     
					MyDA.InputParam("pQueueStatus",MySqlDbType.VarChar,50,SD.QueueStatus),
					MyDA.InputParam("pTerminalID",MySqlDbType.Int32,3,SD.TerminalID)
				 };

			using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
			{
				try
				{
					objCnn.Open();
					MyDA.ExecuteNonQuery(objCnn, CommandType.StoredProcedure, "DCS_SalesDelivery_Update", objParam);
					SD_ID = MyDA.varOutputRet.ToString();
				}
				catch (Exception ex)
				{
					return ex.ToString();
				}
			}


			for (int i = 0; i < SD.items.Count(); i++)
			{
				SalesDeliveryItem item = SD.items[i];

				if (item.ItemQty == 0 || item.ItemQty == null)
				{
					return "Error: Wrong item qty.";
				}
				else if (item.ItemPrice == null)
				{
					return "Error: Wrong item price.";
				}
				else
				{
					MyDA = new clsDataAccessor();
					MySqlParameter[] objParam1 = 
							{           
								MyDA.OutputParam("pLastID",MySqlDbType.Double,10),
								MyDA.InputParam("pItemID", MySqlDbType.Int32,4, item.ItemID),
								MyDA.InputParam("pSupBarItem",MySqlDbType.VarChar,25, item.SupBarItem),   
								MyDA.InputParam("pSupBarItemID",MySqlDbType.Int32,4,item.SupBarItemID),
								MyDA.InputParam("pID", MySqlDbType.Int32,4,SD_ID),
								MyDA.InputParam("pItemSKU",MySqlDbType.VarChar,25,item.ItemSKU), 
								MyDA.InputParam("pItemDesc",MySqlDbType.VarChar,100,item.ItemDesc),                                    
								MyDA.InputParam("pItemRemark",MySqlDbType.VarChar,1000,item.ItemRemark),                     
								MyDA.InputParam("pItemQty",MySqlDbType.Decimal,14, item.ItemQty),
								MyDA.InputParam("pItemSOQty",MySqlDbType.Decimal,14, 0.000),
								MyDA.InputParam("pActualQty", MySqlDbType.Decimal,14,item.ActualQty),                                       
								MyDA.InputParam("pGST",MySqlDbType.VarChar,1,item.GST),   
								MyDA.InputParam("pGSTType",MySqlDbType.VarChar,3, item.GSTType),
								MyDA.InputParam("pGstRate", MySqlDbType.Decimal,12,item.GstRate),
								MyDA.InputParam("pItemPrice", MySqlDbType.Decimal,12,item.ItemPrice),
								MyDA.InputParam("pItemUnit", MySqlDbType.VarChar,10,item.ItemUnit),
								MyDA.InputParam("pItemUnitID", MySqlDbType.Int32,4,item.ItemUnitID),
								MyDA.InputParam("pItemDiscAmt", MySqlDbType.Decimal,12,item.ItemDiscAmt),
								MyDA.InputParam("pDisc_pcn1",MySqlDbType.Decimal,14,item.Disc_pcn1),  
								MyDA.InputParam("pDisc_pcn2", MySqlDbType.Decimal,14,item.Disc_pcn2),
								MyDA.InputParam("pDisc_pcn3",MySqlDbType.Decimal,14,item.Disc_pcn3),   
								MyDA.InputParam("pItemSubTotal",MySqlDbType.Decimal,14,item.ItemSubTotal),   
								MyDA.InputParam("pItemGST",MySqlDbType.Decimal,14,item.ItemGST),
								MyDA.InputParam("pTotalDisc",MySqlDbType.Decimal,14,item.TotalDisc),   
								MyDA.InputParam("pTotal",MySqlDbType.Decimal,14,item.Total),  
								MyDA.InputParam("pLocalItemPrice",MySqlDbType.Decimal,12,item.LocalItemPrice),  
								MyDA.InputParam("pLocalItemDiscAmt",MySqlDbType.Decimal,14,item.LocalItemDiscAmt),                                                      
								MyDA.InputParam("pLocalTotalDisc",MySqlDbType.Decimal,14,item.LocalTotalDisc), 
								MyDA.InputParam("pLocalItemSubTotal",MySqlDbType.Decimal,14,item.LocalItemSubTotal),                                    
								MyDA.InputParam("pLocalItemGST",MySqlDbType.Decimal,14,item.LocalItemGST), 
								MyDA.InputParam("pLocalTotal",MySqlDbType.Decimal,14,item.LocalTotal), 
								MyDA.InputParam("pItemFoc",MySqlDbType.VarChar,1,item.ItemFoc),    
								MyDA.InputParam("pItem_ac_asset",MySqlDbType.Int32,4,0),    
								MyDA.InputParam("pLastUser",MySqlDbType.VarChar,50,item.LastUser),                            
								MyDA.InputParam("pLastUpdate",MySqlDbType.Timestamp,20,item.LastUpdate), 
								MyDA.InputParam("pLockUser",MySqlDbType.VarChar,50,item.LockUser),                                                                
								MyDA.InputParam("pLockUpdate",MySqlDbType.Timestamp,20,item.LockUpdate), 
								MyDA.InputParam("pLockStatus",MySqlDbType.VarChar,30,"0"),
								MyDA.InputParam("pRecordStatus",MySqlDbType.VarChar,30,item.RecordStatus),
								MyDA.InputParam("pRecordUpdate",MySqlDbType.Timestamp,20,item.RecordUpdate),                     
								MyDA.InputParam("pQueueStatus",MySqlDbType.VarChar,50,item.QueueStatus),
								MyDA.InputParam("pTerminalID",MySqlDbType.Int32,3,item.TerminalID),
								MyDA.InputParam("pRetailID",MySqlDbType.Int32,3,item.RetailID)
							};
					using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
					{
						try
						{
							objCnn.Open();
							MyDA.ExecuteNonQuery(objCnn, CommandType.StoredProcedure, "DCS_SalesDelivery_Item_Update", objParam1);
						}
						catch (Exception ex)
						{
							return ex.ToString();
						}
					}
				}
			}

			string queryUpdateSDID = "UPDATE SALESDELIVERY SET RETAILSDID = " + SD.RetailSDID + " WHERE ID = " + SD_ID + ";";
			queryUpdateSDID += "UPDATE SALESDELIVERY_ITEM SET RETAILSDID = " + SD.RetailSDID + " WHERE ID = " + SD_ID + ";";

			using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
			{
				try
				{
					objCnn.Open();

					using (MySqlCommand cmd = new MySqlCommand(queryUpdateSDID, objCnn))
					{
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					return ex.ToString();
				}
			}

			return "success";
		}
	}

	
    public string saveSalesDelivery_v4(SalesDelivery SD)
    {
        Guid SD_ID = Guid.NewGuid();
        Guid SDKeyCol = Guid.NewGuid();

        if (SD.INVID == null)
        {
            return "Missing detail: Inv ID.";
        }
        else if (SD.INVRef == null)
        {
            return "Missing detail: Inv Ref.";
        }
        else if (SD.TransDate == null)
        {
            return "Missing detail: Trans Date.";
        }
        else if (SD.RecipientName == null)
        {
            return "Missing detail: Recipient Name.";
        }
        else if (SD.RecipientAddress == null)
        {
            return "Missing detail: Recipient Address.";
        }
        else if (SD.RecipientPostCode == null)
        {
            return "Missing detail: Recipient Postcode.";
        }
        else if (SD.RecipientAttn == null)
        {
            return "Missing detail: Recipient Attn.";
        }
        else if (SD.RecipientTel == null)
        {
            return "Missing detail: Recipient Tel No.";
        }
        //else if (SD.Remark == null)
        //{
        //    return "Missing detail: Remark.";
        //}
        else if (SD.GST != "Y" && SD.GST != "N")
        {
            return "Invalid GST: Only accept Y or N.";
        }
        else if (SD.SalesDeliveryItems.Count() == 0)
        {
            return "Missing sales delivery items detail.";
        }
        else
        {
            string sqlstr = "SELECT * FROM salesdelivery WHERE RecordStatus <> 'DELETED' AND INVID=@INVID AND INVRef=@INVRef";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@INVID", SD.INVID),
                new MySqlParameter("@INVRef", SD.INVRef)
            };
            DataTable sdDT = GetData_Vapt(sqlstr, objparam);
            if (sdDT.Rows.Count == 0)
            {
                SD_ID = Guid.NewGuid();
                //get default info from definition table
                DataTable defaultDT = GetData("SELECT D.DefaultRetailID, D.DefaultCurrency, D.DefaultCountry, D.CompanyName, E.ExchRate, " +
                    " D.CompanyAdd1, D.CompanyTel, D.CompanyFax, D.DefaultGST, D.DefaultGSTVal, D.DefaultGSTIncExc, D.PrefixSDO, D.SDOStartNo, D.SDONextNo" +
                    " FROM definitions D" +
                    " LEFT JOIN exchange_rate E ON E.ExchCurr=D.DefaultCurrency AND E.Display='Y' AND E.RecordStatus<>'DELETED'" +
                    " WHERE D.RecordStatus<>'DELETED' ");

                string DefaultRetailID = Convert.ToString(defaultDT.Rows[0]["DefaultRetailID"]);
                string DefaultCompanyID = "1";
                string DefaultCompanyName = Convert.ToString(defaultDT.Rows[0]["CompanyName"]);
                string DefaultCompanyAdd = Convert.ToString(defaultDT.Rows[0]["CompanyAdd1"]);
                string DefaultCompanyTel = Convert.ToString(defaultDT.Rows[0]["CompanyTel"]);
                string DefaultCompanyFax = Convert.ToString(defaultDT.Rows[0]["CompanyFax"]);
                string DefaultCurrency = Convert.ToString(defaultDT.Rows[0]["DefaultCurrency"]);
                string DefaultCountry = Convert.ToString(defaultDT.Rows[0]["DefaultCountry"]);
                string DefaultGST = Convert.ToString(defaultDT.Rows[0]["DefaultGST"]);
                decimal DefaultGSTVal = Convert.ToDecimal(defaultDT.Rows[0]["DefaultGSTVal"]);
                string DefaultGSTIncExc = Convert.ToString(defaultDT.Rows[0]["DefaultGSTIncExc"]);
                string PrefixSDO = Convert.ToString(defaultDT.Rows[0]["PrefixSDO"]);
                decimal SDOStartNo = Convert.ToDecimal(defaultDT.Rows[0]["SDOStartNo"]);
                decimal SDONextNo = Convert.ToDecimal(defaultDT.Rows[0]["SDONextNo"]);
                decimal ExchRate = Convert.ToDecimal(defaultDT.Rows[0]["ExchRate"]);
                decimal SDTax = 0;
                decimal SDSubTotal = 0;
                decimal SDTotal = 0;
                string IDRef = PrefixSDO + "/" + (SDOStartNo + SDONextNo).ToString() + "/" + DateTime.Now.Year;

                for (int i = 0; i < SD.SalesDeliveryItems.Count(); i++)
                {

                    SDKeyCol = Guid.NewGuid();

                    //retrieve from inventory
                    sqlstr = "SELECT inventory.ItemID, inventory.ItemSKU, inventory.ItemDescp, inventory.ItemOtherLanguage, inventory_supbar.SupBarCode AS SupBarItem, inventory_supbar.Item_SupBarID AS SupBarItemID, inventory_unit.ItemUnit AS ItemUnitID, inventory_unit.ItemActQty AS ActualQty, B.ItemUnit AS ItemBaseUnitID, C.Nick AS ItemBaseUnit" +
                            " FROM inventory " +
                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                            " LEFT JOIN (SELECT inventory.ItemID, inventory_unit.ItemUnit " +
                            " FROM inventory_unit " +
                            " LEFT JOIN inventory ON inventory_unit.ItemID = inventory.ItemID " +
                            " WHERE inventory_unit.RecordStatus <> 'DELETED' AND inventory_unit.ItemActQty = 1 AND inventory.ItemID=@ItemID ) AS B ON B.ItemID = inventory.ItemID " +
                            " LEFT JOIN list_units C ON B.ItemUnit = C.ID " +
                            " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID=@ItemID AND inventory_supbar.Item_SupBarID=@SupBarID AND list_units.Nick=@ItemUOM";
                    MySqlParameter[] objparam1 =
                    {
                        new MySqlParameter("@ItemID", SD.SalesDeliveryItems[i].ItemID),
                        new MySqlParameter("@SupBarID", SD.SalesDeliveryItems[i].SupBarItemID),
                        new MySqlParameter("@ItemUOM", SD.SalesDeliveryItems[i].ItemUOM),
                    };
                    DataTable itemDT = GetData_Vapt(sqlstr, objparam1);

                    if (itemDT.Rows.Count != 0)
                    {
                        if (SD.SalesDeliveryItems[i].ItemQty == 0 || SD.SalesDeliveryItems[i].ItemQty == null)
                        {
                            return "Error: Wrong item qty.";
                        }
                        else if (SD.SalesDeliveryItems[i].ItemPrice == null)
                        {
                            return "Error: Wrong item price.";
                        }
                        else
                        {
                            decimal Tax = 0;
                            decimal SubTotal = 0;
                            decimal Total = 0;

                            //Include GST
                            if (SD.GST == "Y" && DefaultGSTIncExc == "1")
                            {
                                Tax = (SD.SalesDeliveryItems[i].ItemPrice * SD.SalesDeliveryItems[i].ItemQty) * DefaultGSTVal / (100 + DefaultGSTVal);
                                Total = SD.SalesDeliveryItems[i].ItemPrice * SD.SalesDeliveryItems[i].ItemQty;
                                SubTotal = Total;
                            }
                            //Exclude GST
                            else if (SD.GST == "Y" && DefaultGSTIncExc == "2")
                            {
                                Tax = (SD.SalesDeliveryItems[i].ItemPrice * SD.SalesDeliveryItems[i].ItemQty) * DefaultGSTVal / 100;
                                Total = (SD.SalesDeliveryItems[i].ItemPrice * SD.SalesDeliveryItems[i].ItemQty) + Tax;
                                SubTotal = (SD.SalesDeliveryItems[i].ItemPrice * SD.SalesDeliveryItems[i].ItemQty);
                            }
                            else
                            {
                                Tax = 0;
                                Total = SD.SalesDeliveryItems[i].ItemPrice * SD.SalesDeliveryItems[i].ItemQty;
                                SubTotal = Total;
                            }

                            string queryInsertSDItem = "INSERT INTO salesdelivery_item " +
                                    " (KeyCol, ItemID, SupBarItem, SupBarItemID, ID, ItemSKU, ItemQty, ActualQty, ActualSOQty, Currency, ExchRate, GST, GSTType, GSTRate, ItemPrice, " +
                                    " ItemUnit, ItemUnitID, ItemBaseUnit, ItemBaseUnitID, ItemBal, ItemSubTotal, ItemGST, Total, LocalItemPrice, LocalItemSubTotal, LocalItemGST, LocalTotal, LastUpdate, RecordUpdate) " +
                                    " VALUE " +
                                    " (@KeyCol, @ItemID, @SupBarItem, @SupBarItemID, @ID, @ItemSKU, @ItemQty, @ActualQty, @ActualSOQty, @Currency, @ExchRate, @GST, @GSTType, @GSTRate, @ItemPrice, " +
                                    " @ItemUnit, @ItemUnitID, @ItemBaseUnit, @ItemBaseUnitID, @ItemBal, @ItemSubTotal, @ItemGST, @Total, @LocalItemPrice, @LocalItemSubTotal, @LocalItemGST, @LocalTotal, @LastUpdate, @RecordUpdate)";
                            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                            {
                                try
                                {
                                    objCnn.Open();
                                    using (MySqlCommand cmd = new MySqlCommand(queryInsertSDItem, objCnn))
                                    {
                                        cmd.Parameters.AddWithValue("@KeyCol", SDKeyCol);
                                        cmd.Parameters.AddWithValue("@ItemID", Convert.ToString(itemDT.Rows[0]["ItemID"]));
                                        cmd.Parameters.AddWithValue("@SupBarItem", Convert.ToString(itemDT.Rows[0]["SupBarItem"]));
                                        cmd.Parameters.AddWithValue("@SupBarItemID", Convert.ToString(itemDT.Rows[0]["SupBarItemID"]));
                                        cmd.Parameters.AddWithValue("@ID", SD_ID);
                                        cmd.Parameters.AddWithValue("@ItemSKU", Convert.ToString(itemDT.Rows[0]["ItemSKU"]));
                                        //cmd.Parameters.AddWithValue("@ItemDesc", Convert.ToString(itemDT.Rows[0]["ItemDescp"]));
                                        //cmd.Parameters.AddWithValue("@ItemOtherLanguage", Convert.ToString(itemDT.Rows[0]["ItemOtherLanguage"]));
                                        cmd.Parameters.AddWithValue("@ItemQty", SD.SalesDeliveryItems[i].ItemQty);
                                        cmd.Parameters.AddWithValue("@ActualQty", Convert.ToDecimal(itemDT.Rows[0]["ActualQty"]));
                                        cmd.Parameters.AddWithValue("@ActualSOQty", 1);
                                        cmd.Parameters.AddWithValue("@Currency", DefaultCurrency);
                                        cmd.Parameters.AddWithValue("@ExchRate", ExchRate);
                                        cmd.Parameters.AddWithValue("@GST", DefaultGST);
                                        cmd.Parameters.AddWithValue("@GSTType", DefaultGSTIncExc);
                                        cmd.Parameters.AddWithValue("@GSTRate", DefaultGSTVal);
                                        cmd.Parameters.AddWithValue("@ItemPrice", SD.SalesDeliveryItems[i].ItemPrice);
                                        cmd.Parameters.AddWithValue("@ItemUnit", SD.SalesDeliveryItems[i].ItemUOM.ToString());
                                        cmd.Parameters.AddWithValue("@ItemUnitID", Convert.ToString(itemDT.Rows[0]["ItemUnitID"]));
                                        cmd.Parameters.AddWithValue("@ItemBaseUnit", Convert.ToString(itemDT.Rows[0]["ItemBaseUnit"]));
                                        cmd.Parameters.AddWithValue("@ItemBaseUnitID", Convert.ToString(itemDT.Rows[0]["ItemBaseUnitID"]));
                                        cmd.Parameters.AddWithValue("@ItemBal", SD.SalesDeliveryItems[i].ItemQty);
                                        cmd.Parameters.AddWithValue("@ItemSubTotal", SubTotal);
                                        cmd.Parameters.AddWithValue("@ItemGST", Tax);
                                        cmd.Parameters.AddWithValue("@Total", Total);
                                        cmd.Parameters.AddWithValue("@LocalItemPrice", SD.SalesDeliveryItems[i].ItemPrice * ExchRate);
                                        cmd.Parameters.AddWithValue("@LocalItemSubTotal", SubTotal * ExchRate);
                                        cmd.Parameters.AddWithValue("@LocalItemGST", Tax * ExchRate);
                                        cmd.Parameters.AddWithValue("@LocalTotal", Total * ExchRate);
                                        //cmd.Parameters.AddWithValue("@PendingSync", "Y");
                                        cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return ex.ToString();
                                }
                            }
                            SDTax = SDTax + Tax;
                            SDSubTotal = SDSubTotal + SubTotal;
                            SDTotal = SDTotal + Total;
                        }
                    }
                    else
                    {
                        return "Error: Item (" + SD.SalesDeliveryItems[i].ItemID.ToString() + ") not found.";
                    }
                }

                //insert SD table
                string queryInsertSD = "INSERT INTO salesdelivery " +
                                    " (ID, TYPE, CompanyID, CoyCode, CompanyAddr, CompanyTel, CompanyFax, RecipientName, RecipientAddr, RecipientPostCode, RecipientAttn, RecipientTel, IDRef, RetailerID, DATE, INVID, INVRef, INVDate, " +
                                    " Gst, GSTIncEx, GstRate, BalSubTotal, BalTax, BalTotal, BalPayable, LocalBalSubTotal, LocalTax, LocalTotal, LocalBalPayable, Exch, ExchRate, vchRemarks, LastUpdate, RecordUpdate, RecStatus) " +
                                    " VALUE " +
                                    " (@ID, @TYPE, @CompanyID, @CoyCode, @CompanyAddr, @CompanyTel, @CompanyFax, @RecipientName, @RecipientAddr, @RecipientPostCode, @RecipientAttn, @RecipientTel, @IDRef, @RetailerID, @DATE, @INVID, @INVRef, @INVDate, " +
                                    " @Gst, @GSTIncEx, @GstRate, @BalSubTotal, @BalTax, @BalTotal, @BalPayable, @LocalBalSubTotal, @LocalTax, @LocalTotal, @LocalBalPayable, @Exch, @ExchRate, @vchRemarks, @LastUpdate, @RecordUpdate, @RecStatus)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsertSD, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", SD_ID);
                            cmd.Parameters.AddWithValue("@TYPE", "SDO");
                            cmd.Parameters.AddWithValue("@CompanyID", DefaultCompanyID);
                            cmd.Parameters.AddWithValue("@CoyCode", "DEFAULT");
                            cmd.Parameters.AddWithValue("@CompanyAddr", DefaultCompanyAdd);
                            cmd.Parameters.AddWithValue("@CompanyTel", DefaultCompanyTel);
                            cmd.Parameters.AddWithValue("@CompanyFax", DefaultCompanyFax);
                            cmd.Parameters.AddWithValue("@RecipientName", SD.RecipientName.ToString());
                            cmd.Parameters.AddWithValue("@RecipientAddr", SD.RecipientAddress.ToString());
                            cmd.Parameters.AddWithValue("@RecipientPostCode", SD.RecipientPostCode.ToString());
                            cmd.Parameters.AddWithValue("@RecipientAttn", SD.RecipientAttn.ToString());
                            cmd.Parameters.AddWithValue("@RecipientTel", SD.RecipientTel.ToString());
                            cmd.Parameters.AddWithValue("@IDRef", IDRef);
                            cmd.Parameters.AddWithValue("@RetailerID", DefaultRetailID);
                            cmd.Parameters.AddWithValue("@DATE", SD.TransDate);
                            cmd.Parameters.AddWithValue("@INVID", SD.INVID.ToString());
                            cmd.Parameters.AddWithValue("@INVRef", SD.INVRef.ToString());
                            cmd.Parameters.AddWithValue("@INVDate", SD.TransDate);
                            cmd.Parameters.AddWithValue("@Gst", SD.GST.ToString());
                            cmd.Parameters.AddWithValue("@GSTIncEx", DefaultGSTIncExc);
                            cmd.Parameters.AddWithValue("@GstRate", DefaultGSTVal);
                            cmd.Parameters.AddWithValue("@BalSubTotal", SDSubTotal);
                            cmd.Parameters.AddWithValue("@BalTax", SDTax);
                            cmd.Parameters.AddWithValue("@BalTotal", SDTotal);
                            cmd.Parameters.AddWithValue("@BalPayable", SDTotal);
                            cmd.Parameters.AddWithValue("@LocalBalSubTotal", SDSubTotal * ExchRate);
                            cmd.Parameters.AddWithValue("@LocalTax", SDTax * ExchRate);
                            cmd.Parameters.AddWithValue("@LocalTotal", SDTotal * ExchRate);
                            cmd.Parameters.AddWithValue("@LocalBalPayable", SDTotal * ExchRate);
                            cmd.Parameters.AddWithValue("@Exch", DefaultCurrency);
                            cmd.Parameters.AddWithValue("@ExchRate", ExchRate);
                            cmd.Parameters.AddWithValue("@vchRemarks", SD.Remark);
                            //cmd.Parameters.AddWithValue("@PendingSync", "Y");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
							cmd.Parameters.AddWithValue("@RecStatus", "NEW");
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }
                }

                //update definition table
                string queryUpdateSDONext = "Update definitions " +
                            " SET SDONextNo= @SDONextNo, LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                            " WHERE RecordStatus <> 'DELETED'";

                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();

                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateSDONext, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@SDONextNo", SDONextNo + 1);
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }
                }

                return "success";
            }
            else
            {
                return "Error: Duplicate sales found.";
            }

        }
    }

    public string saveECateoriesData(ECategories EC)
    {
        string response = "";

        if (EC.ECategory.Count() == 0)
        {
            return "No records found.";
        }
        else
        {
            string queryInsertCategory = "";
            for (int i = 0; i < EC.ECategory.Count(); i++)
            {
                string sqlstr = "SELECT * FROM list_categories WHERE RecordStatus <> 'DELETED' AND ID=@ID";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@ID", EC.ECategory[i].cat_id.ToString())
                };
                DataTable catDT = GetData_Vapt(sqlstr, objparam);
                if (catDT.Rows.Count == 0)
                {
                    queryInsertCategory = "INSERT INTO list_categories(ID, VALUE, OtherLanguage, Nick, FULL, Display, LastUpdate, RecordStatus, RecordUpdate) " +
                                " VALUE " +
                                " (@ID,@descp,@OtherLanguage,@Code,@full,@Display,@LastUpdate,@RecordStatus, @RecordUpdate)";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsertCategory, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@ID", EC.ECategory[i].cat_id.ToString());
                                cmd.Parameters.AddWithValue("@Code", EC.ECategory[i].cat_code.ToString());
                                cmd.Parameters.AddWithValue("@OtherLanguage", EC.ECategory[i].cat_otherlanguage.ToString());
                                cmd.Parameters.AddWithValue("@descp", EC.ECategory[i].cat_descp.ToString());
                                cmd.Parameters.AddWithValue("@full", "");
                                cmd.Parameters.AddWithValue("@Display", "Y");
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", EC.ECategory[i].cat_createdate.ToString());
                                cmd.ExecuteNonQuery();
                            }
                            objCnn.Close();
                            response = "Success";
                        }
                        catch (Exception ex)
                        {
                            objCnn.Close();
                            response = ex.ToString();
                        }
                    }
                }
            }
            return response;
        }
    }

    public string updateECateoriesData(ECategories EC)
    {
        string response = "";

        if (EC.ECategory.Count() == 0)
        {
            return "No records found.";
        }
        else
        {
            string queryUpdateCategory = "";
            for (int i = 0; i < EC.ECategory.Count(); i++)
            {
                string sqlstr = "SELECT * FROM list_categories WHERE RecordStatus <> 'DELETED' AND ID=@ID";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@ID", EC.ECategory[i].cat_id.ToString())
                };
                DataTable catDT = GetData_Vapt(sqlstr, objparam);
                if (catDT.Rows.Count != 0)
                {
                    queryUpdateCategory = "UPDATE list_categories SET VALUE=@descp,OtherLanguage=@OtherLanguage,Nick=@Code,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ID=@ID";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryUpdateCategory, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@ID", EC.ECategory[i].cat_id.ToString());
                                cmd.Parameters.AddWithValue("@Code", EC.ECategory[i].cat_code.ToString());
                                cmd.Parameters.AddWithValue("@OtherLanguage", EC.ECategory[i].cat_otherlanguage.ToString());
                                cmd.Parameters.AddWithValue("@descp", EC.ECategory[i].cat_descp.ToString());
                                cmd.Parameters.AddWithValue("@full", "");
                                cmd.Parameters.AddWithValue("@Display", "Y");
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", EC.ECategory[i].cat_createdate.ToString());
                                cmd.ExecuteNonQuery();
                            }
                            objCnn.Close();
                            response = "Success";
                        }
                        catch (Exception ex)
                        {
                            objCnn.Close();
                            response = ex.ToString();
                        }
                    }
                }
            }
            return response;
        }
    }

    public string deleteECateoriesData(string cat_id)
    {
        string response = "";

        if (cat_id == "")
        {
            return "Category ID cannot be empty or null";
        }
        else
        {
            string queryUpdateCategory = "";
            string sqlstr = "SELECT * FROM list_categories WHERE RecordStatus <> 'DELETED' AND ID=@ID";
            MySqlParameter[] objparam =
            {
                    new MySqlParameter("@ID", cat_id.ToString())
            };
            DataTable catDT = GetData_Vapt(sqlstr, objparam);
            if (catDT.Rows.Count != 0)
            {
                queryUpdateCategory = "UPDATE list_categories SET RecordStatus=@RecordStatus,Display=@Display,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ID=@ID";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateCategory, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", cat_id.ToString());
                            cmd.Parameters.AddWithValue("@Display", "N");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "DELETED");
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        response = "Success";
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        response = ex.ToString();
                    }
                }
            }
            return response;
        }
    }


    public string saveMemberData(MemberInfor member)
    {
        Guid ID = Guid.NewGuid();

        if (member.VendorMemberID == null)
        {
            return "Missing detail: Member ID from Vendor";
        }
        else if (member.MemberName == null)
        {
            return "Missing detail: Member Name";
        }
        else if (member.HPH == null)
        {
            return "Missing detail: Member HPH";
        }
        else if (member.Email == null)
        {
            return "Missing detail: Member Email";
        }
        else if (member.DOB == null)
        {
            return "Missing detail: Member DOB";
        }
        else if (member.CreateDate == null)
        {
            return "Missing detail: Member Register/ Create Date";
        } 
        else
        {
            //DataTable memberDT = null;
            string strDOB = DateTime.Parse(member.DOB.ToString()).ToString("yyyy-MM-dd", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            if (member.MemberID.ToString() == "-1")
            {
                string strCustCode = "";
                string strEndDate = "";

                string sqlstr = "SELECT CUSTTYPEID FROM CUSTOMER_TYPE WHERE BLNDEFAULT = 'Y' AND bitFrontendLock='N' and RecordStatus<>'DELETED' and Display='Y'";
                DataTable DT = GetData(string.Format(sqlstr));
                string DefaultMemberType = Convert.ToString(DT.Rows[0]["CUSTTYPEID"]);

                string strCountryID = "";
                sqlstr = "SELECT ID FROM list_countries WHERE Nick LIKE concat('%', @Country,'%') OR FULL LIKE concat('%', @Country,'%')";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@Country", member.Country)
                };

                DT = GetData_Vapt(sqlstr, objparam);
                if (DT.Rows.Count != 0)
                {
                    strCountryID = Convert.ToString(DT.Rows[0]["ID"]);
                }
                DT.Clear();
                DT.Dispose();

                string strGenderID = "";
                sqlstr = "SELECT ID FROM list_sexes WHERE Nick=@Gender AND RecordStatus<>'DELEETD' AND Display='Y'";
                MySqlParameter[] objparam1 =
                {
                    new MySqlParameter("@Gender", member.Gender)
                };
                DT = GetData_Vapt(sqlstr, objparam);
                if (DT.Rows.Count != 0)
                {
                    strGenderID = Convert.ToString(DT.Rows[0]["ID"]);
                }
                DT.Clear();
                DT.Dispose();

                string strRetailID = "0";
                sqlstr = "SELECT RetailID FROM retailer WHERE RetailType='ONLINE' AND RecordStatus<>'DELETED'";
                DT = GetData(string.Format(sqlstr));
                if (DT.Rows.Count != 0)
                {
                    strRetailID = Convert.ToString(DT.Rows[0]["RetailID"]);
                }

                DT.Clear();
                DT.Dispose();

                DataTable memdefDT = GetData(string.Format("SELECT bitAutoCustCode,bitCustCodeEqualICNO,vchPrefixCust,CustStartNo,CustNextNo," +
                                                           "noOfYear,PointCutOffMonth ,IF(DATE_FORMAT(NOW(),'%Y-%m-%d') > (LAST_DAY(CONCAT(SUBSTR(CURDATE(),1,4),LPAD(PointCutOffMonth,2,'0'),'01'))), " +
                                                           "LAST_DAY(CONCAT(SUBSTR(CURDATE(),1,4)+1,LPAD(PointCutOffMonth,2,'0'),'01')),LAST_DAY(CONCAT(SUBSTR(CURDATE(),1,4),LPAD(PointCutOffMonth,2,'0'),'01')) " +
                                                           ") AS ExpiredDate FROM customer_definitions "));
                string AutoCustCode = Convert.ToString(memdefDT.Rows[0]["bitAutoCustCode"]);
                string CustCodeEqualICNO = Convert.ToString(memdefDT.Rows[0]["bitCustCodeEqualICNO"]);
                string PrefixCust = Convert.ToString(memdefDT.Rows[0]["vchPrefixCust"]);
                string CustStartNo = Convert.ToString(memdefDT.Rows[0]["CustStartNo"]);
                string CustNextNo = Convert.ToString(memdefDT.Rows[0]["CustNextNo"]);
                string PointExpDate = Convert.ToString(memdefDT.Rows[0]["ExpiredDate"]);
                int noOfyear = int.Parse(memdefDT.Rows[0]["noOfYear"].ToString());

                if (noOfyear == 0)
                {
                    strEndDate = "2035-01-01";
                }
                else
                {
                    strEndDate = DateTime.Parse(member.CreateDate.ToString()).AddYears(noOfyear).ToString("yyyy-MM-dd", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                }
                decimal start = Convert.ToDecimal(CustStartNo.ToString());
                decimal next = Convert.ToDecimal(CustNextNo.ToString()) + 1;

                decimal runNo = start + next;
                if (AutoCustCode == "Y")
                {
                    if (CustCodeEqualICNO == "Y")
                    {
                        strCustCode = member.NRIC.ToString();
                    }
                    else
                    {
                        strCustCode = PrefixCust + (runNo).ToString();
                    }
                }
                else
                {
                    if (CustCodeEqualICNO == "Y")
                    {
                        strCustCode = member.NRIC.ToString();
                    }
                    else { strCustCode = ""; }
                }
                //update definition table
                string queryUpdateCustNextNo = "Update customer_definitions " +
                            " SET CustNextNo= @CustNextNo, LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate ";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateCustNextNo, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@CustNextNo", next);
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        return ex.ToString();
                    }
                }
                string uniqueNo = member.HPH.ToString() + strDOB.Replace("-", "");

                string queryInsertMember = "INSERT INTO customer " +
                            " (ID,UniqueNo,custcode,CustICNo,email,hph,CustomerFirstName,CustomerDOB,CustomerSexID,customertype," +
                            " CustomerAddress1,CustomerAddress2,CustomerAddress3,CustomerPostcode,CustomerCountryID,CustomerStartDate, CustomerEndDate,RetailID," +
                            " ExpiryPointDate,Display,VendorMemberID,LastUpdate, LockUpdate, RecordStatus, RecordUpdate,QueueStatus)" +
                            " VALUE " +
                            " (@ID,@UniqueNo,@CustCode,@ICNO,@email,@hph,@Name,@DOB,@Gender,@CustType,@Addr1,@Addr2,@Addr3,@PostalCode,@Country,@StartDate,@EndDate," +
                            " @RetailID,@ExpiryPointDate, @Display,@VendorMemberID,@LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate,@QueueStatus)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsertMember, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", ID);
                            cmd.Parameters.AddWithValue("@UniqueNo", uniqueNo);
                            cmd.Parameters.AddWithValue("@CustCode", strCustCode);
                            cmd.Parameters.AddWithValue("@ICNO", member.NRIC.ToString());
                            cmd.Parameters.AddWithValue("@email", member.Email.ToString());
                            cmd.Parameters.AddWithValue("@hph", member.HPH.ToString());
                            cmd.Parameters.AddWithValue("@Name", member.MemberName.ToString());
                            cmd.Parameters.AddWithValue("@DOB", strDOB);
                            cmd.Parameters.AddWithValue("@Gender", strGenderID);
                            cmd.Parameters.AddWithValue("@CustType", DefaultMemberType);
                            cmd.Parameters.AddWithValue("@Addr1", member.Address1.ToString());
                            cmd.Parameters.AddWithValue("@Addr2", member.Address2.ToString());
                            cmd.Parameters.AddWithValue("@Addr3", member.Address3.ToString());
                            cmd.Parameters.AddWithValue("@PostalCode", member.PostalCode.ToString());
                            cmd.Parameters.AddWithValue("@Country", strCountryID);
                            cmd.Parameters.AddWithValue("@StartDate", member.CreateDate.ToString());
                            cmd.Parameters.AddWithValue("@EndDate", strEndDate);
                            cmd.Parameters.AddWithValue("@ExpiryPointDate", PointExpDate);
                            cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                            cmd.Parameters.AddWithValue("@VendorMemberID", member.VendorMemberID.ToString());
                            cmd.Parameters.AddWithValue("@Display", "Y");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();

                        return new JavaScriptSerializer().Serialize(new { Status = "Success", POSMemberID = ID });
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        return ex.ToString();
                    }
                }
            }
            else
            {
                return "Error: Duplicate Member record.";
            }
        }
    }

    public string updateMemberData(MemberInfor member)
    {
        if (member.MemberID == null)
        {
            return "Missing detail: Member ID";
        }
        else if (member.MemberName == null)
        {
            return "Missing detail: Member Name";
        }
        else if (member.HPH == null)
        {
            return "Missing detail: Member HPH";
        }
        else if (member.Email == null)
        {
            return "Missing detail: Member Email";
        }
        else if (member.DOB == null)
        {
            return "Missing detail: Member DOB";
        }
        else
        {
            DataTable memberDT = null;
            string strDOB = DateTime.Parse(member.DOB.ToString()).ToString("yyyy-MM-dd", System.Globalization.DateTimeFormatInfo.InvariantInfo);

			string sqlstr = "";
            if (member.MemberID.ToString() == "" || member.MemberID.ToString() == "-1")
            {
                sqlstr = "SELECT * FROM customer WHERE RecordStatus <> 'DELETED' AND CustICNO=@CustICNO AND hph=@Hph AND CustomerFirstName=@CustName";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@CustICNO", member.NRIC),
                    new MySqlParameter("@Hph", member.HPH),
                    new MySqlParameter("@CustName", member.MemberName)
                };
                memberDT = GetData_Vapt(sqlstr, objparam);
            }
            else
            {
                sqlstr = "SELECT * FROM customer WHERE RecordStatus <> 'DELETED' AND ID=@MemberID";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@MemberID", member.MemberID)
                };
                memberDT = GetData_Vapt(sqlstr, objparam);
            }
            if (memberDT.Rows.Count == 0)
            {
                return "Error: No Member found.";
            } else {
                string ID = Convert.ToString(memberDT.Rows[0]["ID"]);

                string strCountryID = "";
                sqlstr = "SELECT ID FROM list_countries WHERE Nick LIKE concat('%',@Country, '%') OR FULL LIKE concat('%',@Country, '%')";
                MySqlParameter[] objparam1 =
                {
                    new MySqlParameter("@Country", member.Country)
                };
                DataTable DT = GetData_Vapt(sqlstr, objparam1);
                if (DT.Rows.Count != 0)
                {
                    strCountryID = Convert.ToString(DT.Rows[0]["ID"]);
                }
                DT.Clear();
                DT.Dispose();

                string strGenderID = "";
                sqlstr = "SELECT ID FROM list_sexes WHERE Nick LIKE concat('%', @Gender, '%')";
                MySqlParameter[] objparam2 =
                {
                    new MySqlParameter("@Gender", member.Gender)
                };
                DT = GetData_Vapt(sqlstr, objparam1);
                if (DT.Rows.Count != 0)
                {
                    strGenderID = Convert.ToString(DT.Rows[0]["ID"]);
                }
                DT.Clear();
                DT.Dispose();
                string queryUpdateMember = "Update customer SET CustICNo=@ICNO,email=@email,hph=@hph,CustomerFirstName=@Name," +
                            "CustomerDOB=@DOB,CustomerAddress1=@Addr1,CustomerAddress2=@Addr2,CustomerAddress3=@Addr3," +
                            "CustomerPostcode=@PostalCode,CustomerCountryID=@Country,Display=@Display,VendorMemberID=@VendorMemberID,LastUpdate=@LastUpdate," +
                            "LockUpdate=@LockUpdate, RecordStatus=@RecordStatus, RecordUpdate=@RecordUpdate,QueueStatus=@QueueStatus " + 
                            "Where ID=@ID ";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateMember, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", ID);
                            cmd.Parameters.AddWithValue("@ICNO", member.NRIC.ToString());
                            cmd.Parameters.AddWithValue("@email", member.Email.ToString());
                            cmd.Parameters.AddWithValue("@hph", member.HPH.ToString());
                            cmd.Parameters.AddWithValue("@Name", member.MemberName.ToString());
                            cmd.Parameters.AddWithValue("@DOB", strDOB);
                            cmd.Parameters.AddWithValue("@Gender", member.Gender.ToString());
                            cmd.Parameters.AddWithValue("@Addr1", member.Address1.ToString());
                            cmd.Parameters.AddWithValue("@Addr2", member.Address2.ToString());
                            cmd.Parameters.AddWithValue("@Addr3", member.Address3.ToString());
                            cmd.Parameters.AddWithValue("@PostalCode", member.PostalCode.ToString());
                            cmd.Parameters.AddWithValue("@Country", strCountryID);
                            cmd.Parameters.AddWithValue("@Display", "Y");
                            cmd.Parameters.AddWithValue("@VendorMemberID", member.VendorMemberID.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();

                        return new JavaScriptSerializer().Serialize(new { Status = "Success", POSMemberID = ID });
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        return ex.ToString();
                    }
                }
            }
        }
    }
    
    public string CombineSellOrder(OrderResponse orderResponse)
    {
        var result = "";
        int NewRecord = 0;
        Item item = new Item();
        var statusCode = orderResponse.meta.statusCode.ToString();

        var recordCount = orderResponse.response.data.Count;
        //orderResponse.response.data[1].id;

        List<SD> SalesDelivery = new List<SD>();

        for (int i = 0; i < recordCount; i++)
        {

            var OrderStatus = orderResponse.response.data[i].status_label;
            var PONo = orderResponse.response.data[i].id;
            var RecipientName = orderResponse.response.data[i].address_shipping.name;
            var Address = orderResponse.response.data[i].address_shipping.address;
            var Tel = orderResponse.response.data[i].address_shipping.phone;
            if (Tel == null)
            {
                Tel = "";
            }

            var PostCode = orderResponse.response.data[i].address_shipping.postcode;
            var MarketPlaceID = "";
            try
            {
                MarketPlaceID = orderResponse.response.data[i].integration.marketplace_id.ToString();
            }
            catch (Exception ex)
            {
                MarketPlaceID = "";
            }

            var payment = orderResponse.response.data[i].payment_method;
            var Total = orderResponse.response.data[i].price;
            var OrderDateTime = orderResponse.response.data[i].created_at;
            var OrderDate = Convert.ToDateTime(orderResponse.response.data[i].created_at).ToString("yyyy-MM-dd");
            var Today = System.DateTime.Now.Date.ToString("yyyy-MM-dd");

            var itemCount = orderResponse.response.data[i].items.Count;
            //result = result + " ***** " + PONo + " " + OrderStatus + " " + RecipientName + " " + itemCount;

            string sqlstr = "";
            //if (OrderStatus != "Cancelled")
            if (OrderStatus == "Paid")
            {
                sqlstr = "SELECT * FROM salesdelivery WHERE PONo=@PONo AND RecipientName=@Name AND RecipientTel=@Tel AND MarketPlaceID=@PlaceID";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@PONo", PONo),
                    new MySqlParameter("@Name", RecipientName),
                    new MySqlParameter("@Tel", Tel),
                    new MySqlParameter("@PlaceID", MarketPlaceID),

                };
                DataTable sdDT = GetData_Vapt(sqlstr, objparam);

                //UPDATE definitions SET SDONextNo=SDONextNo+1 WHERE RecordStatus<>'DELETED'
                if (sdDT.Rows.Count == 0)
                {

                    //update definition table
                    string queryUpdateSDONext = "Update definitions " +
                                " SET SDONextNo= SDONextNo+1, LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                                " WHERE RecordStatus <> 'DELETED'";

                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        objCnn.Open();

                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateSDONext, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }

                    //get SDONextNo
                    string sqldefinitions = "SELECT PrefixSDO,SDOStartNo,SDONextNo,DefaultGSTVal,DefaultGSTIncExc,DefaultCurrency FROM definitions WHERE RecordStatus<>'DELETED'";
                    DataTable definitionsDT = GetData(string.Format(sqldefinitions));
                    string PrefixSDO = Convert.ToString(definitionsDT.Rows[0]["PrefixSDO"]);
                    int SDOStartNo = Convert.ToInt32(definitionsDT.Rows[0]["SDOStartNo"]);
                    int SDONextNo = Convert.ToInt32(definitionsDT.Rows[0]["SDONextNo"]);
                    string GstVal = Convert.ToString(definitionsDT.Rows[0]["DefaultGSTVal"]);
                    string GstIncExc = Convert.ToString(definitionsDT.Rows[0]["DefaultGSTIncExc"]);
                    string Currency = Convert.ToString(definitionsDT.Rows[0]["DefaultCurrency"]);
                    int Code = SDOStartNo + SDONextNo;
					var txtIDRef = PrefixSDO + "/" + Code + "/" + DateTime.Now.Year;
                    var txtNewID = Guid.NewGuid();


                    //insert new Sales Delivery record
                    string queryInsertSalesDelivery = "INSERT INTO salesdelivery(ID,RecipientName,RecipientAddr,RecipientPostCode,RecipientAttn,RecipientTel,OrderStatus,PONo,IDRef,MarketplaceID,DATE,INVDate,Gst,GSTIncEx,GstRate,BalSubTotal,BalTotal,BalPayable,LocalBalSubTotal,LocalTotal,LocalBalPayable,Exch,LastUpdate,RecordUpdate) " +
                                " VALUES " +
                                " (@txtNewID, @RecipientName, @Address, @PostCode, @RecipientAttn, @Tel, @OrderStatus, @PONo, @txtIDRef, @MktpID, @Date, @INVDate, @Gst, @GstIncExc, @GstVal, @BalSubTotal, @BalTotal, @BalPayable, @LocalBalSubTotal, @LocalTotal, @LocalBalPayable, @Currency, @LastUpdate, @RecordUpdate)";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsertSalesDelivery, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@txtNewID", txtNewID.ToString());
                                cmd.Parameters.AddWithValue("@RecipientName", RecipientName);
                                cmd.Parameters.AddWithValue("@Address", Address);
                                cmd.Parameters.AddWithValue("@PostCode", PostCode);
                                cmd.Parameters.AddWithValue("@RecipientAttn", RecipientName);
                                cmd.Parameters.AddWithValue("@Tel", Tel);
                                cmd.Parameters.AddWithValue("@OrderStatus", OrderStatus);
                                cmd.Parameters.AddWithValue("@PONo", PONo);
                                cmd.Parameters.AddWithValue("@txtIDRef", txtIDRef);
                                cmd.Parameters.AddWithValue("@MktpID", MarketPlaceID);
                                cmd.Parameters.AddWithValue("@Date", OrderDate);
                                cmd.Parameters.AddWithValue("@INVDate", OrderDate);
                                cmd.Parameters.AddWithValue("@Gst", "N");
                                cmd.Parameters.AddWithValue("@GstIncExc", GstIncExc);
                                cmd.Parameters.AddWithValue("@GstVal", GstVal);
                                cmd.Parameters.AddWithValue("@BalSubTotal", Total);
                                cmd.Parameters.AddWithValue("@BalTotal", Total);
                                cmd.Parameters.AddWithValue("@BalPayable", Total);
                                cmd.Parameters.AddWithValue("@LocalBalSubTotal", Total);
                                cmd.Parameters.AddWithValue("@LocalTotal", Total);
                                cmd.Parameters.AddWithValue("@LocalBalPayable", Total);
                                cmd.Parameters.AddWithValue("@Currency", Currency);
                                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                cmd.ExecuteNonQuery();
                            }
                            objCnn.Close();
                            NewRecord = NewRecord + 1;
                            //return result = "Success";
                        }
                        catch (Exception ex)
                        {
                            objCnn.Close();
                            return result = ex.ToString();
                        }
                    }


                    if (itemCount != 0)
                    {
                        List<SD_Item> SD_Items = new List<SD_Item>();

                        for (int y = 0; y < itemCount; y++)
                        {
                            SD_Item sd_items = new SD_Item();

                            item = JsonConvert.DeserializeObject<Item>(orderResponse.response.data[i].items[y].ToString());
                            //item = orderResponse.response.data[i].items[y].ToString(); ;

                            //result = result + " " + orderResponse.response.data[i].items[y].sku + " " + orderResponse.response.data[i].items[y].name;

                            //var ItemSKU = item.sku;
                            //var ItemName = item.name;
                            var ItemSKU = "187CBB1401";
                            var ItemName = "N RHUMBA TIGHTS";
                            var ItemQty = item.quantity;
                            var ItemTotalPrice = item.paid_price;
                            if(ItemTotalPrice == null)
                            {
                                ItemTotalPrice = "0";
                            }

                            //result = result + " " + item.sku + " " + item.name;

                            //check item
                            string sqlInventory = "SELECT inventory.ItemID, inventory_supbar.Item_SupBarID, inventory_supbar.SupBarCode, inventory_unit.ItemUnit AS ItemUnitID," +
                                " inventory_unit.RTLSellPx AS ItemSellingPrice, inventory_unit.ItemUnit AS ItemBaseUnit, inventory_unit.ItemActQty, list_units.Value as UOM" +
                                " FROM inventory" +
                                " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID" +
                                " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID AND inventory_unit.ItemUnitDef = 'Y' AND inventory_unit.RecordStatus <> 'DELETED'" +
                                " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID" +
                                " WHERE inventory.ItemSKU=@ItemSKU AND inventory.ItemName=@ItemName";
                            MySqlParameter[] objparam1 =
                            {
                                new MySqlParameter("@ItemSKU", ItemSKU),
                                new MySqlParameter("@ItemName", ItemName)
                            };
                            DataTable inventoryDT = GetData_Vapt(sqlInventory, objparam1);
                            if (inventoryDT.Rows.Count != 0)
                            {
                                string ItemID = Convert.ToString(inventoryDT.Rows[0]["ItemID"]);
                                string Item_SupBarID = Convert.ToString(inventoryDT.Rows[0]["Item_SupBarID"]);
                                string SupBarCode = Convert.ToString(inventoryDT.Rows[0]["SupBarCode"]);
                                string ItemUnitID = Convert.ToString(inventoryDT.Rows[0]["ItemUnitID"]);
                                string ItemSellingPrice = Convert.ToString(inventoryDT.Rows[0]["ItemSellingPrice"]);
                                string ItemBaseUnit = Convert.ToString(inventoryDT.Rows[0]["ItemBaseUnit"]);
                                string ItemActQty = Convert.ToString(inventoryDT.Rows[0]["ItemActQty"]);
                                string UOM = Convert.ToString(inventoryDT.Rows[0]["UOM"]);

                                var txtKeyCol = Guid.NewGuid();

                                //insert Sales_delivery item
                                string queryInsertSalesDeliveryItem = "INSERT INTO salesdelivery_item (KeyCol,ItemID,SupBarItem,SupBarItemID,ID,ItemQty,ActualQty,ActualSOQty,Currency,GST,GSTRate,ItemPrice,ItemUnit," +
											" ItemUnitID,ItemBaseUnit,ItemBaseUnitID,ItemBal,ItemSubTotal,Total,LocalItemPrice,LocalItemSubTotal,LocalTotal,LastUpdate,RecordUpdate) " +
                                            " VALUES " +
                                            " (@txtKeyCol, @ItemID, @SupBarCode, @Item_SupBarID, @txtNewID, @ItemQty, @ItemActQty, @ActualSOQty, @Currency, @Gst, @GstVal, @ItemPrice, @UOM, @ItemUnitID, @ItemBaseUnit, @ItemBaseUnitID, @ItemBal, @ItemSubTotal, @Total, @LocalItemPrice, @LocalItemSubTotal, @LocalTotal, @LastUpdate, @RecordUpdate)";
                                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                                {
                                    try
                                    {
                                        objCnn.Open();
                                        using (MySqlCommand cmd = new MySqlCommand(queryInsertSalesDeliveryItem, objCnn))
                                        {
                                            cmd.Parameters.AddWithValue("@txtKeyCol", txtKeyCol.ToString());
                                            cmd.Parameters.AddWithValue("@ItemID", ItemID);
                                            cmd.Parameters.AddWithValue("@SupBarCode", SupBarCode);
                                            cmd.Parameters.AddWithValue("@Item_SupBarID", Item_SupBarID);
                                            cmd.Parameters.AddWithValue("@txtNewID", txtNewID);
                                            cmd.Parameters.AddWithValue("@ItemQty", ItemQty);
                                            cmd.Parameters.AddWithValue("@ItemActQty", ItemActQty);
                                            cmd.Parameters.AddWithValue("@ActualSOQty", 0);
                                            cmd.Parameters.AddWithValue("@Currency", Currency);
                                            cmd.Parameters.AddWithValue("@Gst", "N");
                                            cmd.Parameters.AddWithValue("@GstVal", GstVal);
                                            cmd.Parameters.AddWithValue("@ItemPrice", ItemSellingPrice);
                                            cmd.Parameters.AddWithValue("@UOM", UOM);
                                            cmd.Parameters.AddWithValue("@ItemUnitID", ItemUnitID);
                                            cmd.Parameters.AddWithValue("@ItemBaseUnit", UOM);
                                            cmd.Parameters.AddWithValue("@ItemBaseUnitID", ItemBaseUnit);
                                            cmd.Parameters.AddWithValue("@ItemBal", ItemQty);
                                            cmd.Parameters.AddWithValue("@ItemSubTotal", ItemTotalPrice);
                                            cmd.Parameters.AddWithValue("@Total", ItemTotalPrice);
                                            cmd.Parameters.AddWithValue("@LocalItemPrice", ItemSellingPrice);
                                            cmd.Parameters.AddWithValue("@LocalItemSubTotal", ItemTotalPrice);
                                            cmd.Parameters.AddWithValue("@LocalTotal", ItemTotalPrice);
                                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                                            cmd.ExecuteNonQuery();
                                        }
                                        objCnn.Close();


                                        //add into sd_item class for json
                                        sd_items.KeyCol = txtKeyCol.ToString();
                                        sd_items.ItemID = ItemID;
                                        sd_items.SupBarCode = SupBarCode;
                                        sd_items.Item_SupBarID = Item_SupBarID;
                                        sd_items.ItemQty = ItemQty.ToString();
                                        sd_items.ItemPrice = ItemSellingPrice;
                                        sd_items.UOM = UOM;
                                        sd_items.Total = ItemTotalPrice;

                                        SD_Items.Add(sd_items);


                                        

                                        //return result = "Success";
                                        //response = "Success";
                                    }
                                    catch (Exception ex)
                                    {
                                        objCnn.Close();
                                        return result = ex.ToString();
                                        //response = ex.ToString();
                                    }
                                }
                            }
                        }

                        //add into sd class for json
                        SD salesdelivery = new SD()
                        {
                            ID = txtNewID.ToString(),
                            RecipientName = RecipientName,
                            Address = Address,
                            PostCode = PostCode,
                            RecipientAttn = RecipientName,
                            Tel = Tel,
                            OrderStatus = OrderStatus,
                            PONo = PONo.ToString(),
                            IDRef = txtIDRef,
                            Date = OrderDate,
                            TotalAmount = Total.ToString(),
                            SD_Items = SD_Items
                        };
                        SalesDelivery.Add(salesdelivery);

                    }
                    else
                    {
                        //no sd item
                        SD salesdelivery = new SD()
                        {
                            ID = txtNewID.ToString(),
                            RecipientName = RecipientName,
                            Address = Address,
                            PostCode = PostCode,
                            RecipientAttn = RecipientName,
                            Tel = Tel,
                            OrderStatus = OrderStatus,
                            PONo = PONo.ToString(),
                            IDRef = txtIDRef,
                            Date = OrderDate,
                            TotalAmount = Total.ToString()
                        };
                        SalesDelivery.Add(salesdelivery);
                    }
                }
                //else
                //{
                //    return "old record";
                //}
            }         

        }
        List<OrderReturn> OrderReturns = new List<OrderReturn>();
        if(NewRecord == 0)
        {
            //return "Error : No new order record.";
            OrderReturn orderreturns = new OrderReturn()
            {
                Message = "No new order record."
            };
            OrderReturns.Add(orderreturns);
            string json = JsonConvert.SerializeObject(orderreturns, Formatting.Indented);
            return json;
        }
        else
        {
            //return NewRecord + " new order record(s) updated.";
            OrderReturn orderreturns = new OrderReturn()
            {
                Message = NewRecord + " new order record(s) updated.",
                SDs = SalesDelivery
            };
            OrderReturns.Add(orderreturns);
            string json = JsonConvert.SerializeObject(orderreturns, Formatting.Indented);
            return json;
        }

        //return result;
        //return orderResponse.response.data[1].id.ToString();
    }


    public string CombineSellAcceptOrder(string RetailID, string User, string SDID, string IDRef)
    {
        //update definition table
        string queryUpdateSD = "UPDATE salesdelivery " +
                    " SET RetailerID = @RetailerID, TrackingNumber = @IDRef, OrderStatus = @OrderStatus, LastUser = @LastUser, LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                    " WHERE ID = @SDID";
        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
        {
            try
            {
                objCnn.Open();
                using (MySqlCommand cmd = new MySqlCommand(queryUpdateSD, objCnn))
                {
                    cmd.Parameters.AddWithValue("@RetailerID", RetailID);
                    cmd.Parameters.AddWithValue("@IDRef", IDRef);
                    cmd.Parameters.AddWithValue("@OrderStatus", "Ready");
                    cmd.Parameters.AddWithValue("@LastUser", User);
                    cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@SDID", SDID);
                    cmd.ExecuteNonQuery();
                }
                objCnn.Close();
            }
            catch (Exception ex)
            {
                objCnn.Close();
                return ex.ToString();
            }
        }

        return "OK";
    }

    public string CombineSellCancelOrder(string RetailID, string User, string IDRef, string PONo)
    {
        //update definition table
        string queryUpdateSD = "UPDATE salesdelivery " +
                    " SET RetailerID = @RetailerID, OrderStatus = @OrderStatus, RecordStatus = @RecordStatus, LastUser=@LastUser, LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                    " WHERE PONo = @PONo AND IDRef = @IDRef";
        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
        {
            try
            {
                objCnn.Open();
                using (MySqlCommand cmd = new MySqlCommand(queryUpdateSD, objCnn))
                {
                    cmd.Parameters.AddWithValue("@RetailerID", RetailID);
                    cmd.Parameters.AddWithValue("@OrderStatus", "Cancelled");
                    cmd.Parameters.AddWithValue("@RecordStatus", "DELETED");
                    cmd.Parameters.AddWithValue("@LastUser", User);
                    cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@PONo", PONo);
                    cmd.Parameters.AddWithValue("@IDRef", IDRef);
                    cmd.ExecuteNonQuery();
                }
                objCnn.Close();
            }
            catch (Exception ex)
            {
                objCnn.Close();
                return ex.ToString();
            }
        }

        return "OK";
    }
    
    public string updateWarrantyStatus(string companyCode, string status)
    {
        string sqlstr = "SELECT * FROM TBLLICENSE WHERE COMPANY=@Company";
        MySqlParameter[] objparam =
            {
            new MySqlParameter("@Company", companyCode)
        };
        DataTable licenseDT = GetData_Vapt(sqlstr, objparam);

        if (licenseDT.Rows.Count == 0)
        {

            return "This company not found.";
        }
        else
        {
            string queryUpdateLicense = "UPDATE tbllicense " +
                    " SET MaintenanceStatus = 'Y'" +
                    " WHERE Company = @Company";
            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
            {
                try
                {
                    objCnn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(queryUpdateLicense, objCnn))
                    {
                        cmd.Parameters.AddWithValue("@Company", companyCode);
                        cmd.ExecuteNonQuery();
                    }
                    objCnn.Close();
                }
                catch (Exception ex)
                {
                    objCnn.Close();
                    return ex.ToString();
                }
            }

            return "Updated.";
        }
    }

    public string saveMissingPaymentMethod(string pName,string pID) {
        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
        {
            objCnn.Open();
            Guid paymentMethodID = Guid.NewGuid();
            string queryInsertPaymentMethod = "INSERT INTO list_paymentmethods " +
                    "(ID, Nick, VALUE,Full, Display,ButtonGroup,SecondaryID,SPV05, RecordUpdate)" +
                    " VALUE " +
                    "(@ID, @Nick,@Nick, @Full, @Display,@Nick,@SPV05,@SPV05, @RecordUpdate)";
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(queryInsertPaymentMethod, objCnn))
                {
                    cmd.Parameters.AddWithValue("@ID", paymentMethodID.ToString());
                    cmd.Parameters.AddWithValue("@Nick", pName.ToString());
                    cmd.Parameters.AddWithValue("@Full", pName.ToString());
                    cmd.Parameters.AddWithValue("@Display", "Y");
                    cmd.Parameters.AddWithValue("@SPV05", pID.ToString());
                    cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return paymentMethodID.ToString();
        }        
    }
	
    public string updatePaymentMethod(PaymentMethod paymentMethod)
    {
		string sqlstr = "";
        for (int i = 0; i < paymentMethod.PaymentMethods.Count(); i++)
        {
            sqlstr = "SELECT * FROM list_paymentmethods WHERE Nick=@StrPayment";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@StrPayment", paymentMethod.PaymentMethods[i].Name)
            };
            DataTable paymentMethodDT = GetData_Vapt(sqlstr, objparam);
            if (paymentMethodDT.Rows.Count == 0)
            {
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    objCnn.Open();
                    Guid paymentMethodID = Guid.NewGuid();
                    string queryInsertPaymentMethod = "INSERT INTO list_paymentmethods " +
                            "(ID, Nick, VALUE,Full, Display,ButtonGroup,SecondaryID,SPV05, RecordUpdate)" +
                            " VALUE " +
                            "(@ID, @Nick,@Nick, @Full, @Display,@Nick,@SPV05,@SPV05, @RecordUpdate)";
                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand(queryInsertPaymentMethod, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", paymentMethodID.ToString());
                            cmd.Parameters.AddWithValue("@Nick", paymentMethod.PaymentMethods[i].Name);
                            cmd.Parameters.AddWithValue("@Full", paymentMethod.PaymentMethods[i].Full);
                            cmd.Parameters.AddWithValue("@Display", paymentMethod.PaymentMethods[i].Display);
                            cmd.Parameters.AddWithValue("@SPV05", paymentMethod.PaymentMethods[i].ID);
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }
                }
            }
            else
            {
                string queryUpdatePaymentMethod = "UPDATE list_paymentmethods " +
                   " SET Nick = @Name,VALUE = @Name, Full = @Full, Display = @Display,ButtonGroup=@Name,SecondaryID=@ID,SPV05=@ID, RecordUpdate = @RecordUpdate" +
                   " WHERE Nick = @Name";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdatePaymentMethod, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@Name", paymentMethod.PaymentMethods[i].Name);
                            cmd.Parameters.AddWithValue("@Full", paymentMethod.PaymentMethods[i].Full);
                            cmd.Parameters.AddWithValue("@Display", paymentMethod.PaymentMethods[i].Display);
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@ID", paymentMethod.PaymentMethods[i].ID);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        return ex.ToString();
                    }
                }
            }
        }
        return "Updated";
    }

    public string SaveInventoryAging(InventoryAgings INV)
    {
        string response = "";
        string OnHandQtyJson = @"{""ItemOnHandQty"":[";
		var a = 0;

        if (INV.ItemAging.Count() == 0)
        {
            return "No records found.";
        }
        else
        {
            string queryInsert = "";
            for (int i = 0; i < INV.ItemAging.Count(); i++)
            {
                decimal dblbalanceQty = 0;
                string strItemID = INV.ItemAging[i].ItemID.ToString();
                string strRetailID = INV.ItemAging[i].RetailID.ToString();
                string strDate = DateTime.Now.ToString("yyyy-MM-dd");
                decimal itemActualQty = decimal.Parse(INV.ItemAging[i].ItemActualQty.ToString());
                decimal dblDefActualQty = decimal.Parse(INV.ItemAging[i].ItemDefActualQty.ToString());
				decimal ItemSellPrice = 0;

                dblbalanceQty = INV.ItemAging[i].PDQty + ((INV.ItemAging[i].SoldQty * itemActualQty) * -1) + INV.ItemAging[i].TrfInQty + INV.ItemAging[i].TrfOutQty;
                dblbalanceQty = dblbalanceQty + INV.ItemAging[i].AdjQty + INV.ItemAging[i].RetQty + INV.ItemAging[i].SDQty;
                dblbalanceQty = dblbalanceQty + INV.ItemAging[i].KitQty + INV.ItemAging[i].DekitQty + INV.ItemAging[i].ReserveQty + INV.ItemAging[i].InTransitQty;

                if ((INV.ItemAging[i].SellPrice) == null)
                {
                    ItemSellPrice = 0;
                }
                else {
                    ItemSellPrice = INV.ItemAging[i].SellPrice;
                }
                queryInsert = "INSERT IGNORE inventory_aging (ID,SupplierID,RetailID,ItemID,ItemSKU,TransID,TransNo,TransDate," +
                              "ItemUOMID,ItemUOM,ItemBaseUOMID,ItemBaseUOM,Qty,ItemActualQty," +
                              "CurrencyID,ExcRate,CostUnitPx,LocalCostUnitPx,SellPrice,CreateTime,BatchNo," +
                              "HSCode,ExpireDate,ExpiryDay,PDQty,SoldQty,TrfInQty,TrfOutQty,AdjQty," +
                              "RetQty,SDQty,KitQty,DekitQty,ReserveQty,InTransitQty,QtyBalance,TerminalID,RFID,PendingSync)" +
                              " VALUE " +
                              "(@ID, @SupplierID, @RetailID, @ItemID, @ItemSKU, @TransID, @TransNo,@TransDate,@ItemUOMID," +
                              "@ItemUOM,@ItemBaseUOMID,@ItemBaseUOM,@Qty, @ItemActualQty, @CurrencyID, @ExcRate, @CostUnitPx," +
                              "@LocalCostUnitPx,@SellPrice,  @CreateTime, @BatchNo, @HSCode, @ExpireDate,@ExpiryDay, @PDQty, @SoldQty, @TrfInQty," +
                              "@TrfOutQty,@AdjQty, @RetQty, @SDQty, @KitQty, @DekitQty,@ReserveQty, @InTransitQty, @QtyBalance, @TerminalID, @RFID," +
                              "@PendingSync) ";

                /*queryInsert += "ON DUPLICATE KEY UPDATE SupplierID=@SupplierID,RetailID=@RetailID,ItemID=@ItemID,ItemSKU,TransID,TransNo,TransDate," +
                              "ItemUOMID,ItemUOM=@ItemUOM,ItemBaseUOMID=@ItemBaseUOMID,ItemBaseUOM,Qty,ItemActualQty," +
                              "CurrencyID,ExcRate,CostUnitPx,LocalCostUnitPx,CreateTime,BatchNo," +
                              "HSCode,ExpireDate,ExpiryDay,PDQty,SoldQty,TrfInQty,TrfOutQty,AdjQty," +
                              "RetQty,SDQty,KitQty,DekitQty,ReserveQty,InTransitQty,QtyBalance," + 
                              "TerminalID,RFID,PendingSync,LastUpdate,LockUpdate,RecordStatus,RecordUpdate"; */
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsert, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", INV.ItemAging[i].ID.ToString());
                            cmd.Parameters.AddWithValue("@SupplierID", INV.ItemAging[i].SupplierID.ToString());
                            cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                            cmd.Parameters.AddWithValue("@ItemID", strItemID);
                            cmd.Parameters.AddWithValue("@ItemSKU", INV.ItemAging[i].ItemSKU.ToString());
                            cmd.Parameters.AddWithValue("@TransID", INV.ItemAging[i].TransID.ToString());
                            cmd.Parameters.AddWithValue("@TransNo", INV.ItemAging[i].TransNo.ToString());
                            cmd.Parameters.AddWithValue("@TransDate", INV.ItemAging[i].TransDate);
                            cmd.Parameters.AddWithValue("@ItemUOMID", INV.ItemAging[i].ItemUOMID.ToString());
                            cmd.Parameters.AddWithValue("@ItemUOM", INV.ItemAging[i].ItemUOM.ToString());
                            cmd.Parameters.AddWithValue("@ItemBaseUOMID", INV.ItemAging[i].ItemBaseUOMID.ToString());
                            cmd.Parameters.AddWithValue("@ItemBaseUOM", INV.ItemAging[i].ItemBaseUOM.ToString());
                            cmd.Parameters.AddWithValue("@Qty", INV.ItemAging[i].Qty);
                            cmd.Parameters.AddWithValue("@ItemActualQty", INV.ItemAging[i].ItemActualQty);
                            cmd.Parameters.AddWithValue("@CurrencyID", INV.ItemAging[i].CurrencyID.ToString());
                            cmd.Parameters.AddWithValue("@ExcRate", INV.ItemAging[i].ExcRate);
                            cmd.Parameters.AddWithValue("@CostUnitPx", INV.ItemAging[i].CostUnitPx);
                            cmd.Parameters.AddWithValue("@LocalCostUnitPx", INV.ItemAging[i].LocalCostUnitPx);
							cmd.Parameters.AddWithValue("@SellPrice", ItemSellPrice);
                            cmd.Parameters.AddWithValue("@CreateTime", INV.ItemAging[i].CreateTime);
                            cmd.Parameters.AddWithValue("@BatchNo", INV.ItemAging[i].BatchNo.ToString());
                            cmd.Parameters.AddWithValue("@HSCode", INV.ItemAging[i].HSCode.ToString());
                            cmd.Parameters.AddWithValue("@ExpireDate", INV.ItemAging[i].ExpireDate);
                            cmd.Parameters.AddWithValue("@ExpiryDay", INV.ItemAging[i].ExpiryDay);
                            cmd.Parameters.AddWithValue("@PDQty", INV.ItemAging[i].PDQty);
                            cmd.Parameters.AddWithValue("@SoldQty", ((INV.ItemAging[i].SoldQty * itemActualQty) * -1));
                            cmd.Parameters.AddWithValue("@TrfInQty", INV.ItemAging[i].TrfInQty);
                            cmd.Parameters.AddWithValue("@TrfOutQty", INV.ItemAging[i].TrfOutQty);
                            cmd.Parameters.AddWithValue("@AdjQty", INV.ItemAging[i].AdjQty);
                            cmd.Parameters.AddWithValue("@RetQty", INV.ItemAging[i].RetQty);
                            cmd.Parameters.AddWithValue("@SDQty", INV.ItemAging[i].SDQty);
                            cmd.Parameters.AddWithValue("@KitQty", INV.ItemAging[i].KitQty);
                            cmd.Parameters.AddWithValue("@DekitQty", INV.ItemAging[i].DekitQty);
                            cmd.Parameters.AddWithValue("@ReserveQty", INV.ItemAging[i].ReserveQty);
                            cmd.Parameters.AddWithValue("@InTransitQty", INV.ItemAging[i].InTransitQty);
                            cmd.Parameters.AddWithValue("@QtyBalance", dblbalanceQty);
                            cmd.Parameters.AddWithValue("@TerminalID", "0");
                            cmd.Parameters.AddWithValue("@RFID", INV.ItemAging[i].RFID.ToString());
                            cmd.Parameters.AddWithValue("@PendingSync", "N");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        response = "Success";

                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        response = ex.ToString();
                    }
                }

                string strSQL = "UPDATE INVENTORY_RETAIL SET PendingSync =@PendingSync, RECORDUPDATE = @RecordUpdate," +
                                "ONHANDQTY = getItemOnHandQty(@ItemID,@RetailID,@TodayDate)," +
                                "OnHandDefQty = FLOOR(getItemOnHandQty(@ItemID,@RetailID,@TodayDate) / @ActualQty ) WHERE ItemID = @ItemID AND RETAILID = @RetailID";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(strSQL, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                            cmd.Parameters.AddWithValue("@ItemID", strItemID);
                            cmd.Parameters.AddWithValue("@TodayDate", strDate);
                            cmd.Parameters.AddWithValue("@ActualQty", dblDefActualQty);
                            cmd.Parameters.AddWithValue("@PendingSync", "Y");
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                    }
                }
                
                string sqlstr = "SELECT  RetailID, ItemID, OnHandQty, OnHandDefQty  FROM inventory_retail  WHERE ItemID=@ItemID AND RetailID=@RetailID";
                MySqlParameter[] objparam1 =
                {
                    new MySqlParameter("@ItemID", strItemID),
                    new MySqlParameter("@RetailID", strRetailID)
                };
                DataTable DT = GetData_Vapt(sqlstr, objparam1);
                //if (DT.Rows.Count != 0)
                //{
                //    strRetailID = Convert.ToString(DT.Rows[0]["RetailID"]);
                //}
				double onhandqty = 0.000, onhandqtydef = 0.000;
				if ( DT.Rows[0]["OnHandQty"] == null || DT.Rows[0]["OnHandQty"].ToString().Length == 0)
				{
					onhandqty = 0.000;
				}else{
					onhandqty = double.Parse(DT.Rows[0]["OnHandQty"].ToString());
				}
				if ( DT.Rows[0]["OnHandDefQty"] == null || DT.Rows[0]["OnHandDefQty"].ToString().Length == 0)
				{
					onhandqtydef = 0.000;
				}else{
					onhandqtydef = double.Parse(DT.Rows[0]["OnHandDefQty"].ToString());
				}

                OnHandQtyJson = OnHandQtyJson + "{";
                OnHandQtyJson = OnHandQtyJson + string.Format(@"""RetailID"":""{0}"",""ItemID"":""{1}"",""OnHandQty"":{2},""OnHandDefQty"":{3}"
                    , strRetailID, strItemID, onhandqty , onhandqtydef);
                OnHandQtyJson = OnHandQtyJson + "},";

				a = 1;
            }
            OnHandQtyJson = OnHandQtyJson.Remove(OnHandQtyJson.Length - a) + "]}";
            return OnHandQtyJson;
        }
    }

    public decimal calcMemberPoint(string strMemberID,string SalesDate,decimal dblEarnPoint,decimal SalesAmt)
    {
        decimal BalPoint = 0;
        decimal TotalSalesAmt = 0;
        decimal TotalEarnPoint = 0;
        decimal TotalRP = 0;
        decimal TotalAdjustPoint = 0;

        string sqlstr = "SELECT SUM(TotalAmount) AS SalesAmt,getMemberOpeningPoint('{0}') + SUM(LoyaltyPoint - RedeemPoint + AdjustPoint) AS TotalPoint," +
                            "SUM(LoyaltyPoint) AS EarnPoint,SUM(RedeemPoint) AS RedeemPoint, SUM(AdjustPoint) AS AdjPoint " +
                            " FROM customer_salesdetails WHERE CustID=@MemberID";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@MemberID", strMemberID)
        };

        DataTable DT = GetData_Vapt(sqlstr, objparam);
        if (DT.Rows.Count != 0)
        {
            BalPoint = Convert.ToDecimal(DT.Rows[0]["TotalPoint"]);
            TotalSalesAmt = Convert.ToDecimal(DT.Rows[0]["SalesAmt"]);
            TotalEarnPoint = Convert.ToDecimal(DT.Rows[0]["EarnPoint"]);
            TotalRP = Convert.ToDecimal(DT.Rows[0]["RedeemPoint"]);
            TotalAdjustPoint = Convert.ToDecimal(DT.Rows[0]["AdjPoint"]);
        }
        DT.Clear();

        string strSQL = "UPDATE customer SET TotalLP=@TotalLP,TotalEP=@TotalEP,TotalRP=@TotalRP,TotalAP=@TotalAP," +
                    "TotalYTDSales=@TotalYTDSales,TotalDTDSales=@TotalDTDSales,LastPurchaseDate=@LastPurchaseDate,LastEarnPoint=@LastEarnPoint," +
                    "RECORDUPDATE = @RecordUpdate WHERE ID=@MemberID";
        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
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
					cmd.Parameters.AddWithValue("@TotalDTDSales", SalesAmt);
                    cmd.Parameters.AddWithValue("@LastPurchaseDate", SalesDate);
                    cmd.Parameters.AddWithValue("@LastEarnPoint", dblEarnPoint);
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

        return BalPoint;
    }

    public decimal updMemberRedeemPoint(string strMemberID, string SalesDate, decimal dblRedeemPoint)
    {
        string strSQL = "UPDATE customer SET LastRedeemDate=@LastRedeemDate,LastRedeemPoint=@LastRedeemPoint WHERE ID=@MemberID";
        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
        {
            try
            {
                objCnn.Open();
                using (MySqlCommand cmd = new MySqlCommand(strSQL, objCnn))
                {
                    cmd.Parameters.AddWithValue("@LastRedeemDate", SalesDate);
                    cmd.Parameters.AddWithValue("@LastRedeemPoint", dblRedeemPoint);
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

        return dblRedeemPoint;
    }


    public string saveCategoriesData(OnlineCategories ONLINEC)
    {
        string response = "";

        if (ONLINEC.OnlineCategory.Count() == 0)
        {
            return "No records found.";
        }
        else
        {
            string queryInsertCategory = "";
            for (int i = 0; i < ONLINEC.OnlineCategory.Count(); i++)
            {
                string sqlstr = "SELECT * FROM list_inv_categories WHERE RecordStatus <> 'DELETED' AND ID=@CatID";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@CatID", ONLINEC.OnlineCategory[i].cat_id)
                };
                DataTable catDT = GetData_Vapt(sqlstr, objparam);
                if (catDT.Rows.Count == 0)
                {
                    queryInsertCategory = "INSERT INTO list_inv_categories(ID, VALUE, OtherLanguage, Nick, FULL, Display,CatImage,LastUser, LastUpdate, RecordStatus, RecordUpdate) " +
                                " VALUE " +
                                " (@ID,@descp,@OtherLanguage,@Code,@full,@Display,@CatImage,@LastUser,@LastUpdate,@RecordStatus, @RecordUpdate)";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsertCategory, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@ID", ONLINEC.OnlineCategory[i].cat_id.ToString());
                                cmd.Parameters.AddWithValue("@Code", ONLINEC.OnlineCategory[i].cat_code.ToString());
                                cmd.Parameters.AddWithValue("@OtherLanguage", ONLINEC.OnlineCategory[i].cat_otherlanguage.ToString());
                                cmd.Parameters.AddWithValue("@descp", ONLINEC.OnlineCategory[i].cat_descp.ToString());
                                cmd.Parameters.AddWithValue("@full", "");
                                cmd.Parameters.AddWithValue("@CatImage", ONLINEC.OnlineCategory[i].cat_image.ToString());
                                cmd.Parameters.AddWithValue("@Display", ONLINEC.OnlineCategory[i].cat_display.ToString());
                                cmd.Parameters.AddWithValue("@LastUser", ONLINEC.OnlineCategory[i].cat_updateby.ToString());
                                cmd.Parameters.AddWithValue("@LastUpdate",  ONLINEC.OnlineCategory[i].cat_lastupdatetime.ToString());
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", ONLINEC.OnlineCategory[i].cat_lastupdatetime.ToString());
                                
                                cmd.ExecuteNonQuery();
                            }
                            objCnn.Close();
                            response = "Success";
                        }
                        catch (Exception ex)
                        {
                            objCnn.Close();
                            response = ex.ToString();
                        }
                    }
                }
            }
            return response;
        }
    }

    public string updateCategoriesData(OnlineCategories ONLINEC)
    {
        string response = "";

        if (ONLINEC.OnlineCategory.Count() == 0)
        {
            return "No records found.";
        }
        else
        {
            string queryUpdateCategory = "";
            for (int i = 0; i < ONLINEC.OnlineCategory.Count(); i++)
            {
                string sqlstr = "SELECT * FROM list_inv_categories WHERE RecordStatus <> 'DELETED' AND ID=@CatID";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@CatID", ONLINEC.OnlineCategory[i].cat_id)
                };
                DataTable catDT = GetData_Vapt(sqlstr, objparam);

                if (catDT.Rows.Count != 0)
                {
                    queryUpdateCategory = "UPDATE list_inv_categories SET VALUE=@descp,OtherLanguage=@OtherLanguage,Nick=@Code,CatImage=@CatImage,LastUser=@LastUser,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ID=@ID";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryUpdateCategory, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@ID", ONLINEC.OnlineCategory[i].cat_id.ToString());
                                cmd.Parameters.AddWithValue("@Code", ONLINEC.OnlineCategory[i].cat_code.ToString());
                                cmd.Parameters.AddWithValue("@OtherLanguage", ONLINEC.OnlineCategory[i].cat_otherlanguage.ToString());
                                cmd.Parameters.AddWithValue("@descp", ONLINEC.OnlineCategory[i].cat_descp.ToString());
                                cmd.Parameters.AddWithValue("@full", "");
                                cmd.Parameters.AddWithValue("@CatImage", ONLINEC.OnlineCategory[i].cat_image.ToString());
                                cmd.Parameters.AddWithValue("@Display", ONLINEC.OnlineCategory[i].cat_display.ToString());
                                cmd.Parameters.AddWithValue("@LastUser", ONLINEC.OnlineCategory[i].cat_updateby.ToString());
                                cmd.Parameters.AddWithValue("@LastUpdate", ONLINEC.OnlineCategory[i].cat_lastupdatetime.ToString());
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", ONLINEC.OnlineCategory[i].cat_lastupdatetime.ToString());
                                cmd.ExecuteNonQuery();
                            }
                            objCnn.Close();
                            response = "Success";
                        }
                        catch (Exception ex)
                        {
                            objCnn.Close();
                            response = ex.ToString();
                        }
                    }
                }
            }
            return response;
        }
    }

    public string deleteCategoriesData(string cat_id)
    {
        string response = "";

        if (cat_id == "")
        {
            return "Category ID cannot be empty or null";
        }
        else
        {
            string queryUpdateCategory = "";
            string sqlstr = "SELECT * FROM list_inv_categories WHERE RecordStatus <> 'DELETED' AND ID=@CatID";
            MySqlParameter[] objparam =
            {
                    new MySqlParameter("@CatID", cat_id)
                };
            DataTable catDT = GetData_Vapt(sqlstr, objparam);
            if (catDT.Rows.Count != 0)
            {
                queryUpdateCategory = "UPDATE list_inv_categories SET RecordStatus=@RecordStatus,Display=@Display,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ID=@ID";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateCategory, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", cat_id.ToString());
                            cmd.Parameters.AddWithValue("@Display", "N");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "DELETED");
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        response = "Success";
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        response = ex.ToString();
                    }
                }
            }
            return response;
        }
    }

    public string saveProductData(OnlineInventory OI, string Retailer)
    {
        string response = "";
        if (OI.ItemID == null)
        {
            return "No records found.";
        }
        else
        {
            string blnGst = "N";
            string GSTIncEx = "I";
            decimal GstRate = 0;

            string sqlstr = "SELECT DefaultGST,DefaultGSTVal, DefaultGSTIncExc FROM definitions ";
            DataTable DT = GetData(string.Format(sqlstr));
            if (DT.Rows.Count != 0)
            {
                blnGst = Convert.ToString(DT.Rows[0]["DefaultGST"]);
                GSTIncEx = Convert.ToString(DT.Rows[0]["DefaultGSTIncExc"]);
                GstRate = Convert.ToDecimal(DT.Rows[0]["DefaultGSTVal"]);
            }
            DT.Clear();
            DT.Dispose();

            string SupplierID = "";
            sqlstr = "SELECT CompanyID FROM company WHERE CompanyName='DEFAULT' AND CompanyStatus='Y' AND RecordStatus<>'DELETED'";
            DT = GetData(string.Format(sqlstr));
            if (DT.Rows.Count != 0)
            {
                SupplierID = Convert.ToString(DT.Rows[0]["CompanyID"]);
            }
            DT.Clear();
            DT.Dispose();

            string queryInsert = "";
            sqlstr = "SELECT ArtCodeID FROM inventory WHERE RecordStatus <> 'DELETED' AND ItemID=@ItemID";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@ItemID", OI.ItemID)
            };
            DataTable InvDT = GetData_Vapt(sqlstr, objparam);
            if (InvDT.Rows.Count == 0)
            {
                string DeprtID = "";
                DataTable deptDT = GetData(string.Format("SELECT ID from list_inv_departments WHERE Nick='DEFAULT' AND " +
                                    " RecordStatus <> 'DELETED'"));
                if (deptDT.Rows.Count == 0)
                {
                    Guid DepartmentID = Guid.NewGuid();
                    DeprtID = DepartmentID.ToString();
                    queryInsert = "INSERT INTO list_inv_departments(ID,Nick,Value,Display,LastUser,LastUpdate,RecordStatus,RecordUpdate) " +
                                                  " VALUE " +
                                                  " (@DeprtID,@Nick,@Value,@Display,@LastUser,@LastUpdate,@RecordStatus, @RecordUpdate)";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsert, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@DeprtID", DepartmentID.ToString());
                                cmd.Parameters.AddWithValue("@Nick", "DEFAULT");
                                cmd.Parameters.AddWithValue("@Value", "DEFAULT");
                                cmd.Parameters.AddWithValue("@Display", OI.ItemDisplay.ToString());
                                cmd.Parameters.AddWithValue("@LastUser", OI.CreateBy.ToString());
                                cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                                cmd.ExecuteNonQuery();
                            }
                            objCnn.Close();
                        }
                        catch (Exception ex)
                        {
                            objCnn.Close();
                        }
                    }
                }
                else
                {
                    DeprtID = Convert.ToString(deptDT.Rows[0]["ID"]);
                }

                Guid ArtCodeID = Guid.NewGuid();

                queryInsert = "INSERT INTO artcode(ArtCodeID,ArtName,ArtOtherLanguage,ArtDescp,ArtNo,ArtCategory,ArtDepartment,SupplierID," +
                              "ArtTax,RtlSellPx,eOriginalPx,ArtOther,Display,DateCreate,LastUser,LastUpdate,RecordStatus,RecordUpdate) " +
                              " VALUE " +
                              " (@ArtCodeID,@ArtName,@OtherLanguage,@ArtDescp,@ArtNo,@ArtCategory,@ArtDepartment,@SupplierID,@ArtTax,@RtlSellPx," +
                              "@eOriginalPx,@ArtOther,@Display,@DateCreate,@LastUser,@LastUpdate,@RecordStatus, @RecordUpdate)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsert, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ArtCodeID", ArtCodeID.ToString());
                            cmd.Parameters.AddWithValue("@ArtName", OI.ItemDescp.ToString());
                            cmd.Parameters.AddWithValue("@OtherLanguage", OI.ItemOtherLanguage.ToString());
                            cmd.Parameters.AddWithValue("@ArtDescp", OI.ItemDescp.ToString());
                            cmd.Parameters.AddWithValue("@ArtNo", OI.ItemSKU.ToString());
                            cmd.Parameters.AddWithValue("@ArtCategory", OI.ItemCategory.ToString());
                            cmd.Parameters.AddWithValue("@ArtDepartment", DeprtID.ToString());
                            cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                            cmd.Parameters.AddWithValue("@ArtTax", blnGst);
                            cmd.Parameters.AddWithValue("@RtlSellPx", OI.ItemOriPrice.ToString());
                            cmd.Parameters.AddWithValue("@eOriginalPx", OI.ItemOriPrice);
                            cmd.Parameters.AddWithValue("@ArtOther", OI.ItemRemark.ToString());
                            cmd.Parameters.AddWithValue("@Display", OI.ItemDisplay.ToString());
                            cmd.Parameters.AddWithValue("@DateCreate", OI.CreateTime.ToString());
                            cmd.Parameters.AddWithValue("@LastUser", OI.CreateBy.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                    }
                }

                queryInsert = "INSERT INTO inventory(ItemID,ItemSKU,ItemName,ItemDescp,ItemOtherLanguage,ArtCodeID," +
                              "ItemCategory,ItemDepartment,ItemType,ItemSalesType,ItemOther,ItemRtlPx,eOriginalPx,ItemGST,ItemDisplay,ItemSKUSup," +
                              "ItemPromo,ItemPicName,ItemPic,ItemPicFront,ItemCommission,ItemOpenPrice,ItemPoint, " +
                              "ItemDiscount,DateCreate,ItemModifier,LastUser, LastUpdate, RecordStatus, RecordUpdate) " +
                            " VALUE " +
                            " (@ItemID,@ItemSKU,@ItemName,@ItemDescp,@OtherLanguage,@ArtCodeID,@ItemCategory,@ItemDepartment,@ItemType," +
                            "@ItemSalesType,@ItemOther,@ItemRtlPx,@eOriginalPx,@ItemGST,@ItemDisplay,@ItemSKUSup,@ItemPromo,@ItemPicName,@ItemPic," +
                            "@ItemPicFront,@ItemCommssion,@ItemOpenPrice,@ItemPoint,@ItemDiscount,@DateCreate,@ItemModifier," +
                            "@LastUser,@LastUpdate,@RecordStatus, @RecordUpdate)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsert, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ItemID", OI.ItemID.ToString());
                            cmd.Parameters.AddWithValue("@ArtCodeID", ArtCodeID.ToString());
                            cmd.Parameters.AddWithValue("@ItemName", OI.ItemDescp.ToString());
                            cmd.Parameters.AddWithValue("@OtherLanguage", OI.ItemOtherLanguage.ToString());
                            cmd.Parameters.AddWithValue("@ItemDescp", OI.ItemDescp.ToString());
                            cmd.Parameters.AddWithValue("@ItemSKU", OI.ItemSKU.ToString());
                            cmd.Parameters.AddWithValue("@ItemCategory", OI.ItemCategory.ToString());
                            cmd.Parameters.AddWithValue("@ItemDepartment", DeprtID.ToString());
                            cmd.Parameters.AddWithValue("@ItemSKUSup", SupplierID);
                            cmd.Parameters.AddWithValue("@ItemGST", blnGst);
                            cmd.Parameters.AddWithValue("@ItemType", "N");
                            cmd.Parameters.AddWithValue("@ItemSalesType", "I");
                            cmd.Parameters.AddWithValue("@ItemPromo", "N");
                            cmd.Parameters.AddWithValue("@ItemCommssion", "N");
                            cmd.Parameters.AddWithValue("@ItemOpenPrice", "N");
                            cmd.Parameters.AddWithValue("@ItemDiscount", "N");
                            cmd.Parameters.AddWithValue("@ItemPoint", "N");
                            cmd.Parameters.AddWithValue("@ItemPicName", OI.ItemImageName.ToString());
                            cmd.Parameters.AddWithValue("@ItemPic", OI.ItemImage.ToString());
                            cmd.Parameters.AddWithValue("@ItemPicFront", OI.ItemImage.ToString());
                            cmd.Parameters.AddWithValue("@ItemRtlPx", Convert.ToDecimal(OI.ItemOriPrice.ToString()));
                            cmd.Parameters.AddWithValue("@eOriginalPx", OI.ItemOriPrice);
                            cmd.Parameters.AddWithValue("@ItemOther", OI.ItemRemark.ToString());
                            cmd.Parameters.AddWithValue("@ItemDisplay", OI.ItemDisplay.ToString());
                            cmd.Parameters.AddWithValue("@ItemModifier", "");
                            cmd.Parameters.AddWithValue("@DateCreate", OI.CreateTime.ToString());
                            cmd.Parameters.AddWithValue("@LastUser", OI.CreateBy.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        response = "Success";
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        response = ex.ToString();
                    }
                }
                if (Retailer.ToString() == "0")
                {
                    string result = saveRetailerProductData(OI, blnGst, GSTIncEx);
                }
                else
                {
                    Guid ID = Guid.NewGuid();
                    queryInsert = "INSERT INTO inventory_retail(Item_RetailID,ItemID,RetailID,ItemGST,GSTIncExc,OnHandQty," +
                                "ItemTopMenu,LastUser, LastUpdate, RecordStatus, RecordUpdate) " +
                                " VALUE " +
                                " (@ID,@ItemID,@Retailer,@ItemGST,@GSTIncExc,@ItemTopMenu,@OnhandQty," +
                                "@LastUser,@LastUpdate,@RecordStatus, @RecordUpdate)";
                    using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            objCnn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(queryInsert, objCnn))
                            {
                                cmd.Parameters.AddWithValue("@ID", ID.ToString());
                                cmd.Parameters.AddWithValue("@ItemID", OI.ItemID.ToString());
                                cmd.Parameters.AddWithValue("@Retailer", Retailer.ToString());
                                cmd.Parameters.AddWithValue("@ItemGST", blnGst);
                                cmd.Parameters.AddWithValue("@GSTIncExc", GSTIncEx);
                                cmd.Parameters.AddWithValue("@ItemTopMenu", OI.TopMenu.ToString());
								cmd.Parameters.AddWithValue("@OnHandQty", OI.OnHandQty.ToString());
                                cmd.Parameters.AddWithValue("@LastUser", OI.CreateBy.ToString());
                                cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                                cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                                cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                                cmd.ExecuteNonQuery();
                            }
                            objCnn.Close();
                        }
                        catch (Exception ex)
                        {
                            objCnn.Close();
                        }
                    }
                }

                Guid Item_UnitID = Guid.NewGuid();
                string ItemUOM = "";
                if (OI.ItemUOM.ToString() == "")
                {
                    sqlstr = "SELECT ID FROM list_units WHERE blnDefault='Y' AND RecordStatus<>'DELETED'";
                    DT = GetData(string.Format(sqlstr));
                    if (DT.Rows.Count != 0)
                    {
                        ItemUOM = Convert.ToString(DT.Rows[0]["ID"]);
                    }
                    DT.Clear();
                    DT.Dispose();
                }
                else {
                    sqlstr = "SELECT ID FROM list_units WHERE Nick=@ItemUOM";
                    MySqlParameter[] objparam1 =
                    {
                        new MySqlParameter("@ItemUOM", OI.ItemUOM)
                    };

                    DT = GetData_Vapt(sqlstr, objparam1);

                    if (DT.Rows.Count != 0)
                    {
                        ItemUOM = Convert.ToString(DT.Rows[0]["ID"]);
                    }
                    DT.Clear();
                    DT.Dispose();
                }

                queryInsert = "INSERT INTO inventory_unit(Item_UnitID,ItemID,ItemQty,ItemUnit,ItemUnitDef,ItemActQty,RTLSellPx,eOriginalPx," +
                            "ItemWeight,ItemCustomWidth,ItemCustomDepth,ItemCustomHeight,ItemMeasurement,ItemBoxSize,LastUser, LastUpdate, RecordStatus, RecordUpdate) " +
                            " VALUE " +
                            " (@Item_UnitID,@ItemID,@ItemQty,@ItemUnit,@ItemUnitDef,@ItemActQty,@RTLSellPx,@eOriginalPx,@ItemWeight," +
                            "@ItemCustomWidth,@ItemCustomDepth,@ItemCustomHeight,@ItemMeasurement,@ItemBoxSize,@LastUser,@LastUpdate,@RecordStatus, @RecordUpdate)";

                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsert, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@Item_UnitID", Item_UnitID.ToString());
                            cmd.Parameters.AddWithValue("@ItemID", OI.ItemID.ToString());
                            cmd.Parameters.AddWithValue("@ItemQty", 1);
                            cmd.Parameters.AddWithValue("@ItemUnit", ItemUOM);
                            cmd.Parameters.AddWithValue("@ItemUnitDef", "Y");
                            cmd.Parameters.AddWithValue("@ItemActQty", 1);
                            cmd.Parameters.AddWithValue("@RTLSellPx", OI.ItemPrice);
                            cmd.Parameters.AddWithValue("@eOriginalPx", OI.ItemOriPrice);
                            cmd.Parameters.AddWithValue("@ItemWeight", OI.ItemWeight);
                            cmd.Parameters.AddWithValue("@ItemCustomWidth", OI.ItemWidth);
                            cmd.Parameters.AddWithValue("@ItemCustomDepth", OI.ItemDepth);
                            cmd.Parameters.AddWithValue("@ItemCustomHeight", OI.ItemHeight);
                            cmd.Parameters.AddWithValue("@ItemMeasurement", OI.ItemMeasurement);
                            cmd.Parameters.AddWithValue("@ItemBoxSize", OI.ItemBoxSize.ToString());
                            cmd.Parameters.AddWithValue("@LastUser", OI.CreateBy.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                    }
                }

                Guid Item_SupBarID = Guid.NewGuid();
                queryInsert = "INSERT INTO inventory_supbar(Item_SupBarID,ItemID,SupplierID,SupBarCode,DefaultSupplier,Display," +
                             "LastUser, LastUpdate, RecordStatus, RecordUpdate) " +
                             " VALUE " +
                             " (@Item_SupBarID,@ItemID,@SupplierID,@SupBarCode,@DefaultSupplier,@Display," +
                             "@LastUser,@LastUpdate,@RecordStatus, @RecordUpdate)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsert, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@Item_SupBarID", Item_SupBarID.ToString());
                            cmd.Parameters.AddWithValue("@ItemID", OI.ItemID.ToString());
                            cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                            cmd.Parameters.AddWithValue("@SupBarCode", OI.ItemSKU.ToString());
                            cmd.Parameters.AddWithValue("@DefaultSupplier", "Y");
                            cmd.Parameters.AddWithValue("@Display", OI.ItemDisplay.ToString());
                            cmd.Parameters.AddWithValue("@LastUser", OI.CreateBy.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                    }
                }
            }
            else
            {
                string ArtCodeID = Convert.ToString(InvDT.Rows[0]["ArtCodeID"]);

                string queryUpdate = "UPDATE artcode SET ArtName=@ArtName,ArtOtherLanguage=@OtherLanguage,ArtDescp=@ArtDescp,ArtCategory=@ArtCategory,SupplierID=@SupplierID," +
                              "RtlSellPx=@ItemRtlPx,ArtOther=@ArtOther,Display=@Display,LastUser=@LastUser, LastUpdate=@LastUpdate, RecordStatus=@RecordStatus, RecordUpdate=@RecordUpdate  " +
                              " WHERE ArtCodeID=@ArtCodeID";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ArtCodeID", ArtCodeID.ToString());
                            cmd.Parameters.AddWithValue("@ArtName", OI.ItemDescp.ToString());
                            cmd.Parameters.AddWithValue("@OtherLanguage", OI.ItemOtherLanguage.ToString());
                            cmd.Parameters.AddWithValue("@ArtDescp", OI.ItemDescp.ToString());
                            cmd.Parameters.AddWithValue("@ArtCategory", OI.ItemCategory.ToString());
                            cmd.Parameters.AddWithValue("@SupplierID", SupplierID);
                            cmd.Parameters.AddWithValue("@ItemRtlPx", Convert.ToDecimal(OI.ItemOriPrice.ToString()));
                            cmd.Parameters.AddWithValue("@ArtOther", OI.ItemRemark.ToString());
                            cmd.Parameters.AddWithValue("@Display", OI.ItemDisplay.ToString());
                            cmd.Parameters.AddWithValue("@LastUser", OI.LastUpdateBy.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                    }
                }

                queryUpdate = "UPDATE inventory SET ItemSKU=@ItemSKU,ItemName=@ItemName,ItemDescp=@ItemDescp,ItemOtherLanguage=@OtherLanguage," +
                              "ItemCategory=@ItemCategory,ItemOther=@ItemOther,ItemRtlPx=@ItemRtlPx,ItemPicName=@ItemPicName,ItemPic=@ItemPic," +
                              "ItemPicFront=@ItemPicFront,LastUser=@LastUser, LastUpdate=@LastUpdate, RecordStatus=@RecordStatus, RecordUpdate=@RecordUpdate " +
                              "WHERE ItemID=@ItemID ";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ItemID", OI.ItemID.ToString());
                            cmd.Parameters.AddWithValue("@ItemName", OI.ItemDescp.ToString());
                            cmd.Parameters.AddWithValue("@OtherLanguage", OI.ItemOtherLanguage.ToString());
                            cmd.Parameters.AddWithValue("@ItemDescp", OI.ItemDescp.ToString());
                            cmd.Parameters.AddWithValue("@ItemSKU", OI.ItemSKU.ToString());
                            cmd.Parameters.AddWithValue("@ItemCategory", OI.ItemCategory.ToString());
                            cmd.Parameters.AddWithValue("@ItemPicName", OI.ItemImageName.ToString());
                            cmd.Parameters.AddWithValue("@ItemPic", OI.ItemImage.ToString());
                            cmd.Parameters.AddWithValue("@ItemPicFront", OI.ItemImage.ToString());
                            cmd.Parameters.AddWithValue("@ItemRtlPx", Convert.ToDecimal(OI.ItemOriPrice.ToString()));
                            cmd.Parameters.AddWithValue("@ItemOther", OI.ItemRemark.ToString());
                            cmd.Parameters.AddWithValue("@ItemDisplay", OI.ItemDisplay.ToString());
                            cmd.Parameters.AddWithValue("@LastUser", OI.LastUpdateBy.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        response = "Success";
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        response = ex.ToString();
                    }
                }

                queryUpdate = "UPDATE inventory_retail SET ItemTopMenu=@ItemTopMenu,LastUser=@LastUser, OnHandQty=@OnHandQty, LastUpdate=@LastUpdate," +
                              "RecordStatus=@RecordStatus, RecordUpdate=@RecordUpdate WHERE ItemID=@ItemID AND RecordStatus<>'DELETED'";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ItemID", OI.ItemID.ToString());
                            cmd.Parameters.AddWithValue("@Retailer", Retailer.ToString());
                            cmd.Parameters.AddWithValue("@ItemTopMenu", OI.TopMenu.ToString());
							cmd.Parameters.AddWithValue("@OnHandQty", OI.OnHandQty.ToString());
                            cmd.Parameters.AddWithValue("@LastUser", OI.CreateBy.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                    }
                }

                queryUpdate = "UPDATE inventory_unit SET RTLSellPx=@RTLSellPx,eOriginalPx=@eOriginalPx," +
                            "ItemWeight=@ItemWeight,ItemCustomWidth=@ItemCustomWidth,ItemCustomDepth=@ItemCustomDepth,ItemCustomHeight=@ItemCustomHeight," +
                            "ItemMeasurement=@ItemMeasurement,ItemBoxSize=@ItemBoxSize," + 
                            "LastUser=@LastUser, LastUpdate=@LastUpdate, RecordStatus=@RecordStatus, RecordUpdate=@RecordUpdate " +
                            "WHERE ItemID=@ItemID AND ItemUnitDef=@ItemUnitDef AND ItemActQty=@ItemActQty";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ItemID", OI.ItemID.ToString());
                            cmd.Parameters.AddWithValue("@ItemUnitDef", "Y");
                            cmd.Parameters.AddWithValue("@ItemActQty", 1);
                            cmd.Parameters.AddWithValue("@RTLSellPx", OI.ItemPrice);
                            cmd.Parameters.AddWithValue("@eOriginalPx", OI.ItemOriPrice);
                            cmd.Parameters.AddWithValue("@ItemWeight", OI.ItemWeight);
                            cmd.Parameters.AddWithValue("@ItemCustomWidth", OI.ItemWidth);
                            cmd.Parameters.AddWithValue("@ItemCustomDepth", OI.ItemDepth);
                            cmd.Parameters.AddWithValue("@ItemCustomHeight", OI.ItemHeight);
                            cmd.Parameters.AddWithValue("@ItemMeasurement", OI.ItemMeasurement);
                            cmd.Parameters.AddWithValue("@ItemBoxSize", OI.ItemBoxSize.ToString());
                            cmd.Parameters.AddWithValue("@LastUser", OI.LastUpdateBy.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                    }
                }
            }
        }
        return response;
    }

    public string saveRetailerProductData(OnlineInventory OI, string blnGst, string GSTIncEx)
    {
        string Retailer = "";

        DataTable DT = GetData(string.Format("SELECT RetailID FROM retailer WHERE RecordStatus<>'DELETED' AND Display='Y'"));
        if (DT.Rows.Count != 0)
        {
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Retailer = Convert.ToString(DT.Rows[i]["RetailID"]);

                Guid ID = Guid.NewGuid();
                string queryInsert = "INSERT INTO inventory_retail(Item_RetailID,ItemID,RetailID,ItemGST,GSTIncExc," +
                            "ItemTopMenu,OnhandQty, LastUser, LastUpdate, RecordStatus, RecordUpdate) " +
                            " VALUE " +
                            " (@ID,@ItemID,@Retailer,@ItemGST,@GSTIncExc,@ItemTopMenu,@OnhandQty," +
                            "@LastUser,@LastUpdate,@RecordStatus, @RecordUpdate)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsert, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", ID.ToString());
                            cmd.Parameters.AddWithValue("@ItemID", OI.ItemID.ToString());
                            cmd.Parameters.AddWithValue("@Retailer", Retailer.ToString());
                            cmd.Parameters.AddWithValue("@ItemGST", blnGst);
                            cmd.Parameters.AddWithValue("@GSTIncExc", GSTIncEx);
							cmd.Parameters.AddWithValue("@OnhandQty", OI.OnHandQty.ToString());
                            cmd.Parameters.AddWithValue("@ItemTopMenu", OI.TopMenu.ToString());
                            cmd.Parameters.AddWithValue("@LastUser", OI.CreateBy.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", OI.LastUpdateTime.ToString());
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", OI.LastUpdateTime.ToString());

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                    }
                }
            }
            return "success insert retailer";
        }
        else
        {
            return "no retailer found!";
        }
    }


    public string deleteProductData(string RetailerID,string ItemID)
    {
        string response = "";

        if (ItemID == "")
        {
            return "Product ID cannot be empty or null";
        }
        else
        {
            string queryUpdate = "";
            queryUpdate = "SELECT ArtCodeID FROM inventory WHERE RecordStatus <> 'DELETED' AND ItemID=@ItemID";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@ItemID", ItemID)
            };
            DataTable InvDT = GetData_Vapt(queryUpdate, objparam);
            if (InvDT.Rows.Count != 0)
            {
                string artcodeid = Convert.ToString(InvDT.Rows[0]["ArtCodeID"]);

                queryUpdate = "UPDATE artcode SET RecordStatus=@RecordStatus,Display=@Display,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ArtCodeID=@ArtCodeID; " +
                            "UPDATE inventory SET RecordStatus=@RecordStatus,ItemDisplay=@Display,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ItemID=@ItemID; " +
                            "UPDATE inventory_retail SET RecordStatus=@RecordStatus,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ItemID=@ItemID; " +
                            "UPDATE inventory_supbar SET RecordStatus=@RecordStatus,Display=@Display,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ItemID=@ItemID; " +
                            "UPDATE inventory_unit SET RecordStatus=@RecordStatus,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ItemID=@ItemID;";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ArtCodeID", artcodeid);
                            cmd.Parameters.AddWithValue("@ItemID", ItemID.ToString());
                            cmd.Parameters.AddWithValue("@Display", "N");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "DELETED");
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        response = "Success";
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        response = ex.ToString();
                    }
                }
            }
            return response;
        }
    }

    public string updateOrderingReceiptNo(string RetailID)
    {
        string queryUpdate = "UPDATE definitions_terminal SET NextReceiptNo = NextReceiptNo + 1 WHERE RetailerID=@RetailID AND RecordStatus<>'DELETED' AND TerminalID=1";
        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
        {
            try
            {
                objCnn.Open();
                using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                {
                    cmd.Parameters.AddWithValue("@RetailID", RetailID);
                    cmd.ExecuteNonQuery();
                }
                objCnn.Close();
                return "success";
            }
            catch (Exception ex)
            {
                objCnn.Close();
                return ex.ToString();
            }
        }
    }

    public string SaveDefinitionOnlineSetting(OnlineDefinitions OD, string Retailer)
    {
        string queryUpdate = "UPDATE definitions_terminal SET IsCustomDeliveryFee =@IsCustomDeliveryFee,IsOnlinePay=@IsOnlinePay " + 
                             " WHERE RetailerID=@RetailID AND RecordStatus<>'DELETED' AND TerminalID=1";
        using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
        {
            try
            {
                objCnn.Open();
                using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                {
                    cmd.Parameters.AddWithValue("@RetailID", Retailer);
                    cmd.Parameters.AddWithValue("@IsCustomDeliveryFee", OD.IsCustomDeliveryFee.ToString());
                    cmd.Parameters.AddWithValue("@IsOnlinePay", OD.IsOnlinePay.ToString());

                    cmd.ExecuteNonQuery();
                }
                objCnn.Close();
                return "success";
            }
            catch (Exception ex)
            {
                objCnn.Close();
                return ex.ToString();
            }
        }
    }

    public string saveOnLineUser(OnlineUser OUser, string Retailer)
    {
        string response = "";
        string queryInsert = "SELECT * FROM USERS WHERE RECORDSTATUS<>'DELETED' AND USER=@User";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@User", OUser.user_code)
        };
        DataTable UserDT = GetData_Vapt(queryInsert, objparam);
        if (UserDT.Rows.Count == 0)
        {
            queryInsert = "INSERT INTO users(ID,USER,PASSWORD,email,UsersFirstName,hph,Display,LastUser,LastUpdate,RecordStatus,RecordUpdate) " +
                          " VALUE " +
                          " (@ID,@USER,@PASSWORD,@email,@UsersFirstName,@hph,@Display,@LastUser,@LastUpdate,@RecordStatus, @RecordUpdate)";
            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
            {
                try
                {
                    objCnn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(queryInsert, objCnn))
                    {
                        cmd.Parameters.AddWithValue("@ID",OUser.user_id.ToString());
                        cmd.Parameters.AddWithValue("@USER", OUser.user_code.ToString());
                        cmd.Parameters.AddWithValue("@PASSWORD", OUser.user_password.ToString());
                        cmd.Parameters.AddWithValue("@email", OUser.user_email.ToString());
                        cmd.Parameters.AddWithValue("@UsersFirstName", OUser.user_name.ToString());
                        cmd.Parameters.AddWithValue("@hph", OUser.user_contactno.ToString());                        
                        cmd.Parameters.AddWithValue("@Display", OUser.user_display.ToString());
                        cmd.Parameters.AddWithValue("@LastUser", OUser.user_createby.ToString());
                        cmd.Parameters.AddWithValue("@LastUpdate", OUser.user_lastupdatetime.ToString());
                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                        cmd.Parameters.AddWithValue("@RecordUpdate", OUser.user_lastupdatetime.ToString());

                        cmd.ExecuteNonQuery();
                    }
                    objCnn.Close();
                    response = "Success";
                }
                catch (Exception ex)
                {
                    objCnn.Close();
                    response = ex.ToString();
                }
            }
        }
        else
        {

            string queryUpdate = "UPDATE users SET PASSWORD=@PASSWORD,email=@email,UsersFirstName=@UsersFirstName,hph=@hph,Display=@Display," +
                                 "LastUser=@LastUser, LastUpdate=@LastUpdate, RecordStatus=@RecordStatus, RecordUpdate=@RecordUpdate " +
                                 " WHERE ID=@ID ";
            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
            {
                try
                {
                    objCnn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                    {
                        cmd.Parameters.AddWithValue("@ID", OUser.user_id.ToString());
                        cmd.Parameters.AddWithValue("@PASSWORD", OUser.user_password.ToString());
                        cmd.Parameters.AddWithValue("@email", OUser.user_email.ToString());
                        cmd.Parameters.AddWithValue("@UsersFirstName", OUser.user_name.ToString());
                        cmd.Parameters.AddWithValue("@hph", OUser.user_contactno.ToString());     
                        cmd.Parameters.AddWithValue("@Display", OUser.user_display.ToString());
                        cmd.Parameters.AddWithValue("@LastUser", OUser.user_createby.ToString());
                        cmd.Parameters.AddWithValue("@LastUpdate", OUser.user_lastupdatetime.ToString());
                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                        cmd.Parameters.AddWithValue("@RecordUpdate", OUser.user_lastupdatetime.ToString());

                        cmd.ExecuteNonQuery();
                    }
                    objCnn.Close();
                    response = "Success";
                }
                catch (Exception ex)
                {
                    objCnn.Close();
                    response = ex.ToString();
                }
            }
        }
        return response;
    }

    public string deleteOnLineUser(string UserID)
    {
        string response = "";
        if (UserID == null)
        {
            return "No records found.";
        }
        else
        {
            string queryUpdate = "SELECT * FROM USERS WHERE RecordStatus <> 'DELETED' AND ID=@UserID";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@UserID", UserID)
            };
            DataTable UserDT = GetData_Vapt(queryUpdate, objparam);
            if (UserDT.Rows.Count != 0)
            {
                queryUpdate = "UPDATE users SET RecordStatus=@RecordStatus,Display=@Display,LastUpdate=@LastUpdate," +
                              " RecordUpdate=@RecordUpdate WHERE ID=@ID";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", UserID.ToString());
                            cmd.Parameters.AddWithValue("@Display", "N");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "DELETED");
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        response = "Success";
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        response = ex.ToString();
                    }
                }
            }
        }
        return response;
    }

    public string saveDeliverySchedule(OnlineDeliveryInfor ODS, string Retailer)
    {
        string response = "";
        string queryUpdate = "";
        string scheduleID = "";

        string sqlstr = "SELECT ID " +
                        " FROM definitions_deliveryscheduler " +
                        " WHERE RecordStatus <> 'DELETED' AND RetailerID=@RetailerID AND ScheduleType=@ScheduleType AND OperateDayName=@OperateDayName";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@RetailerID", Retailer),
            new MySqlParameter("@ScheduleType", ODS.ScheduleType),
            new MySqlParameter("@OperateDayName", ODS.OperateDayName),
        };
        DataTable DT = GetData_Vapt(sqlstr, objparam);

        if (DT.Rows.Count != 0)
        {
            scheduleID = Convert.ToString(DT.Rows[0]["ID"]);
        }
        else
        {
            sqlstr = "SELECT ID " +
                            " FROM definitions_deliveryscheduler " +
                            " WHERE RecordStatus <> 'DELETED' AND ID=@ScheduleID";
            MySqlParameter[] objparam1 =
            {
                new MySqlParameter("@ScheduleID", ODS.ScheduleID)
            };
            DT = GetData_Vapt(sqlstr, objparam1);

            if (DT.Rows.Count != 0)
            {
                scheduleID = Convert.ToString(DT.Rows[0]["ID"]);
            }
            else {
                scheduleID = "";
            }
        }
        if (scheduleID != "")
        {
            queryUpdate = "UPDATE definitions_deliveryscheduler SET ScheduleType=@ScheduleType,OperateDayName=@OperateDayName,OperateTimeStart=@OperateTimeStart," +
                          "OperateTimeClose=@OperateTimeClose, TimeSlotStart=@TimeSlotStart, TimeSlotClose=@TimeSlotClose, IntervalTime=@IntervalTime," + 
                          "DeliveryMinValueOrder=@MinValueOrder, DeliveryMaxValueOrder=@MaxValueOrder," +
                          "DeliveryFee=@DeliveryFee,CreateBy=@CreateBy, CreateTime=@CreateTime,Active=@Display," +
                          "RecordStatus=@RecordStatus,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ID=@ID";
            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
            {
                try
                {
                    objCnn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                    {
                        cmd.Parameters.AddWithValue("@ID", scheduleID);
                        cmd.Parameters.AddWithValue("@Retailer", Retailer.ToString());
                        cmd.Parameters.AddWithValue("@ScheduleType", ODS.ScheduleType.ToString());
                        cmd.Parameters.AddWithValue("@OperateDayName", ODS.OperateDayName.ToString());
                        cmd.Parameters.AddWithValue("@OperateTimeStart", ODS.OperateTimeStart.ToString("HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@OperateTimeClose", ODS.OperateTimeClose.ToString("HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@TimeSlotStart", ODS.TimeSlotStart.ToString("HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@TimeSlotClose", ODS.TimeSlotClose.ToString("HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@IntervalTime",ODS.IntervalTime);
                        cmd.Parameters.AddWithValue("@MinValueOrder",ODS.MinValueOrder);
                        cmd.Parameters.AddWithValue("@MaxValueOrder",ODS.MaxValueOrder);
                        cmd.Parameters.AddWithValue("@DeliveryFee",ODS.DeliveryFee);
                        cmd.Parameters.AddWithValue("@Display", ODS.IsActive.ToString());
                        cmd.Parameters.AddWithValue("@CreateBy", ODS.CreateBy.ToString());
                        cmd.Parameters.AddWithValue("@CreateTime", ODS.CreateTime.ToString());
                        cmd.Parameters.AddWithValue("@LastUser", ODS.LastUpdateBy.ToString());
                        cmd.Parameters.AddWithValue("@LastUpdate", ODS.LastUpdateTime.ToString());
                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                        cmd.Parameters.AddWithValue("@RecordUpdate", ODS.LastUpdateTime.ToString());
                        cmd.ExecuteNonQuery();
                    }
                    objCnn.Close();
                    response = "Success";
                }
                catch (Exception ex)
                {
                    objCnn.Close();
                    response = ex.ToString();
                }
            }
        } else {
            queryUpdate = "INSERT INTO definitions_deliveryscheduler(ID,RetailerID,ScheduleType,OperateDayName,OperateTimeStart," +
                         "OperateTimeClose,TimeSlotStart,TimeSlotClose,IntervalTime,DeliveryMinValueOrder,DeliveryMaxValueOrder," +
                         "DeliveryFee,CreateBy,CreateTime,Active,LastUser, LastUpdate, RecordStatus, RecordUpdate) " +
                         " VALUE " +
                         "(@ID,@Retailer,@ScheduleType,@OperateDayName,@OperateTimeStart,@OperateTimeClose,@TimeSlotStart,@TimeSlotClose," +
                         "@IntervalTime,@MinValueOrder,@MaxValueOrder,@DeliveryFee,@CreateBy,@CreateTime," +
                         "@Display,@CreateBy,@LastUpdate,@RecordStatus,@RecordUpdate)";
            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
            {
                try
                {
                    objCnn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                    {
                        cmd.Parameters.AddWithValue("@ID", ODS.ScheduleID.ToString());
                        cmd.Parameters.AddWithValue("@Retailer", Retailer.ToString());
                        cmd.Parameters.AddWithValue("@ScheduleType", ODS.ScheduleType.ToString());
                        cmd.Parameters.AddWithValue("@OperateDayName", ODS.OperateDayName.ToString());
                        cmd.Parameters.AddWithValue("@OperateTimeStart", ODS.OperateTimeStart.ToString("HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@OperateTimeClose", ODS.OperateTimeClose.ToString("HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@TimeSlotStart", ODS.TimeSlotStart.ToString("HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@TimeSlotClose", ODS.TimeSlotClose.ToString("HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@IntervalTime", ODS.IntervalTime);
                        cmd.Parameters.AddWithValue("@MinValueOrder",ODS.MinValueOrder);
                        cmd.Parameters.AddWithValue("@MaxValueOrder",ODS.MaxValueOrder);
                        cmd.Parameters.AddWithValue("@DeliveryFee",ODS.DeliveryFee);
                        cmd.Parameters.AddWithValue("@Display", ODS.IsActive.ToString());
                        cmd.Parameters.AddWithValue("@CreateBy", ODS.CreateBy.ToString());
                        cmd.Parameters.AddWithValue("@CreateTime", ODS.CreateTime.ToString());
                        cmd.Parameters.AddWithValue("@LastUser", ODS.LastUpdateBy.ToString());
                        cmd.Parameters.AddWithValue("@LastUpdate", ODS.LastUpdateTime.ToString());
                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                        cmd.Parameters.AddWithValue("@RecordUpdate", ODS.LastUpdateTime.ToString());
                        cmd.ExecuteNonQuery();
                    }
                    objCnn.Close();
                    response = "Success";
                }
                catch (Exception ex)
                {
                    objCnn.Close();
                    response = ex.ToString();
                }
            }
        }
        
        return response;
    }

    private string TimeSpan(DateTime dateTime)
    {
        throw new NotImplementedException();
    }

    public List<PromoRetail> GetPromoRetailer(string PromoID)
    {
        List<PromoRetail> PromoRetails = new List<PromoRetail>();
        string sqlstr = "SELECT Promo_RetailID,Retailer.RetailCode FROM promo_retailer INNER JOIN " +
                            "retailer ON retailer.RetailID=promo_retailer.Promo_RetailID WHERE PromoID=@PromoID " +
                            "AND promo_retailer.RecordStatus<>'DELETED' AND promo_retailer.Display='Y' ORDER BY RetailCode ASC";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@PromoID", PromoID)
        };
        DataTable dt = GetData_Vapt(sqlstr, objparam);
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            PromoRetails.Add(new PromoRetail
            {
                RetailID = Convert.ToString(dt.Rows[i]["Promo_RetailID"])
                ,
                RetailCode = Convert.ToString(dt.Rows[i]["RetailCode"])
            });
        }
        return PromoRetails;
    }

    public List<PromoItem> GetPromoItems(string PromoID)
    {
        List<PromoItem> PromoItems = new List<PromoItem>();

        string sqlstr = "SELECT ItemID,ItemUOM,Item_UnitID,ItemPrice,ItemMemberPrice,ItemUnit,MixID," +
                        "Item_Qty,Item_Amt, Item_Percentage,Item_MemberAmt,Item_MemberPerc, " +
                        "Item_Qty2,Item_Amt2, Item_Percentage2,Item_MemberAmt2,Item_MemberPerc2, " +
                        "Item_Qty3,Item_Amt3, Item_Percentage3,Item_MemberAmt3,Item_MemberPerc3, " +
                        "Item_Qty4,Item_Amt4, Item_Percentage4,Item_MemberAmt4,Item_MemberPerc4, " +
                        "Item_Qty5,Item_Amt5, Item_Percentage5,Item_MemberAmt5,Item_MemberPerc5, " +
                        "Item_Qty6,Item_Amt6, Item_Percentage6,Item_MemberAmt6,Item_MemberPerc6, " +
                        "Item_Qty7,Item_Amt7, Item_Percentage7,Item_MemberAmt7,Item_MemberPerc7, " +
                        "Item_Qty8,Item_Amt8, Item_Percentage8,Item_MemberAmt8,Item_MemberPerc8, " +
                        "Item_Qty9,Item_Amt9, Item_Percentage9,Item_MemberAmt9,Item_MemberPerc9, " +
                        "Item_Qty10,Item_Amt10, Item_Percentage10,Item_MemberAmt10,Item_MemberPerc10 " +
                        "FROM promo_item WHERE PromoID=@PromoID AND RecordStatus<>'DELETED'";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@PromoID", PromoID)
        };
        DataTable dt = GetData_Vapt(sqlstr, objparam);
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            PromoItems.Add(new PromoItem
            {
                ItemID = Convert.ToString(dt.Rows[i]["ItemID"]),
                ItemUOM = Convert.ToString(dt.Rows[i]["ItemUOM"]),
                Item_UnitID = Convert.ToString(dt.Rows[i]["Item_UnitID"]),
                ItemPrice = Convert.ToDecimal(dt.Rows[i]["ItemPrice"]),
                ItemMemberPrice = Convert.ToDecimal(dt.Rows[i]["ItemMemberPrice"]),
                ItemUnit = Convert.ToString(dt.Rows[i]["ItemUnit"]),
                MixID = Convert.ToString(dt.Rows[i]["MixID"]),
                Item_Qty = Convert.ToDecimal(dt.Rows[i]["Item_Qty"]),
                Item_Amt = Convert.ToDecimal(dt.Rows[i]["Item_Amt"]),
                Item_Percentage = Convert.ToDecimal(dt.Rows[i]["Item_Percentage"]),
                Item_MemberAmt = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt"]),
                Item_MemberPerc = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc"]),
                Item_Qty2 = Convert.ToDecimal(dt.Rows[i]["Item_Qty2"]),
                Item_Amt2 = Convert.ToDecimal(dt.Rows[i]["Item_Amt2"]),
                Item_Percentage2 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage2"]),
                Item_MemberAmt2 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt2"]),
                Item_MemberPerc2 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc2"]),
                Item_Qty3 = Convert.ToDecimal(dt.Rows[i]["Item_Qty3"]),
                Item_Amt3 = Convert.ToDecimal(dt.Rows[i]["Item_Amt3"]),
                Item_Percentage3 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage3"]),
                Item_MemberAmt3 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt3"]),
                Item_MemberPerc3 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc3"]),
                Item_Qty4 = Convert.ToDecimal(dt.Rows[i]["Item_Qty4"]),
                Item_Amt4 = Convert.ToDecimal(dt.Rows[i]["Item_Amt4"]),
                Item_Percentage4 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage4"]),
                Item_MemberAmt4 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt4"]),
                Item_MemberPerc4 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc4"]),
                Item_Qty5 = Convert.ToDecimal(dt.Rows[i]["Item_Qty5"]),
                Item_Amt5 = Convert.ToDecimal(dt.Rows[i]["Item_Amt5"]),
                Item_Percentage5 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage5"]),
                Item_MemberAmt5 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt5"]),
                Item_MemberPerc5 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc5"]),
                Item_Qty6 = Convert.ToDecimal(dt.Rows[i]["Item_Qty6"]),
                Item_Amt6 = Convert.ToDecimal(dt.Rows[i]["Item_Amt6"]),
                Item_Percentage6 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage6"]),
                Item_MemberAmt6 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt6"]),
                Item_MemberPerc6 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc6"]),
                Item_Qty7 = Convert.ToDecimal(dt.Rows[i]["Item_Qty7"]),
                Item_Amt7 = Convert.ToDecimal(dt.Rows[i]["Item_Amt7"]),
                Item_Percentage7 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage7"]),
                Item_MemberAmt7 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt7"]),
                Item_MemberPerc7 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc7"]),
                Item_Qty8 = Convert.ToDecimal(dt.Rows[i]["Item_Qty8"]),
                Item_Amt8 = Convert.ToDecimal(dt.Rows[i]["Item_Amt8"]),
                Item_Percentage8 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage8"]),
                Item_MemberAmt8 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt8"]),
                Item_MemberPerc8 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc8"]),
                Item_Qty9 = Convert.ToDecimal(dt.Rows[i]["Item_Qty9"]),
                Item_Amt9 = Convert.ToDecimal(dt.Rows[i]["Item_Amt9"]),
                Item_Percentage9 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage9"]),
                Item_MemberAmt9 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt9"]),
                Item_MemberPerc9 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc9"]),
                Item_Qty10 = Convert.ToDecimal(dt.Rows[i]["Item_Qty10"]),
                Item_Amt10 = Convert.ToDecimal(dt.Rows[i]["Item_Amt10"]),
                Item_Percentage10 = Convert.ToDecimal(dt.Rows[i]["Item_Percentage10"]),
                Item_MemberAmt10 = Convert.ToDecimal(dt.Rows[i]["Item_MemberAmt10"]),
                Item_MemberPerc10 = Convert.ToDecimal(dt.Rows[i]["Item_MemberPerc10"])

            });
        }
        return PromoItems;
    }

    public List<MixMatch> GetPromoMixMatch(string PromoID)
    {
        List<MixMatch> PromoMixMatch = new List<MixMatch>();
        string strsql = "SELECT PromoMixID,Promo_Qty,Promo_Amount,Promo_MemberAmount FROM promo_mixmatch " +
                            "WHERE PromoID=@PromoID AND RecordStatus<>'DELETED'";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@PromoID", PromoID)
        };
        DataTable dt = GetData_Vapt(strsql, objparam);
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            PromoMixMatch.Add(new MixMatch
            {
                Promo_MixID = Convert.ToString(dt.Rows[i]["PromoMixID"]),               
                Promo_Qty = Convert.ToDecimal(dt.Rows[i]["Promo_Qty"]),
                Promo_Amount = Convert.ToDecimal(dt.Rows[i]["Promo_Amount"]),
                Promo_MemberAmount = Convert.ToDecimal(dt.Rows[i]["Promo_MemberAmount"])
            });
        }
        return PromoMixMatch;
    }

    public string saveWebPageDesign(OnlineWebDesignInfor WD)
    {
        string response = "";
        string queryUpdate = "";
        string strsql = "SELECT ID  FROM webdesignsetting WHERE RecordStatus <> 'DELETED' AND ID=@FrameID";
        MySqlParameter[] objparam =
        {
            new MySqlParameter("@FrameID", WD.FrameID)
        };
        DataTable DT = GetData_Vapt(strsql, objparam);
        if (DT.Rows.Count != 0)
        {
            queryUpdate = "UPDATE webdesignsetting SET CompanyLogoURL=@CompanyLogoURL,TopBannerBackColor=@TopBannerBackColor,vchTitle=@PageTitle,vchContent=@PageContent,PageImage=@PageImage," +
                          "vchBackColor=@BackColor,Display=@Display," +
                          "RecordStatus=@RecordStatus,LastUpdate=@LastUpdate,RecordUpdate=@RecordUpdate WHERE ID=@ID";
            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
            {
                try
                {
                    objCnn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                    {
                        cmd.Parameters.AddWithValue("@ID", WD.FrameID.ToString());
                        cmd.Parameters.AddWithValue("@PageTitle", WD.PageTitle.ToString());
                        cmd.Parameters.AddWithValue("@PageContent", WD.PageContent.ToString());
                        cmd.Parameters.AddWithValue("@PageImage", WD.PageImage.ToString());
                        cmd.Parameters.AddWithValue("@CompanyLogoURL", WD.LogoImg.ToString());
                        cmd.Parameters.AddWithValue("@TopBannerBackColor", WD.TopBannerBackColor.ToString());
                        cmd.Parameters.AddWithValue("@BackColor", WD.BackgroundColor.ToString());
                        cmd.Parameters.AddWithValue("@Display", WD.Display.ToString());
                        cmd.Parameters.AddWithValue("@CreateBy", WD.CreateBy.ToString());
                        cmd.Parameters.AddWithValue("@CreateTime", WD.CreateTime.ToString());
                        cmd.Parameters.AddWithValue("@LastUser", WD.LastUpdateBy.ToString());
                        cmd.Parameters.AddWithValue("@LastUpdate", WD.LastUpdateTime.ToString());
                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                        cmd.Parameters.AddWithValue("@RecordUpdate", WD.LastUpdateTime.ToString());
                        cmd.ExecuteNonQuery();
                    }
                    objCnn.Close();
                    response = "Success";
                }
                catch (Exception ex)
                {
                    objCnn.Close();
                    response = ex.ToString();
                }
            }
        } else {
            queryUpdate = "INSERT INTO webdesignsetting(ID,vchTitle,CompanyLogoURL,TopBannerBackColor,vchContent,PageImage,vchBackColor,Display,LastUser, LastUpdate, RecordStatus, RecordUpdate) " +
                         " VALUE " +
                         "(@ID,@PageTitle,@CompanyLogoURL,@TopBannerBackColor,@PageContent,@PageImage,@BackColor,@Display,@CreateBy,@LastUpdate,@RecordStatus,@RecordUpdate)";
            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
            {
                try
                {
                    objCnn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                    {
                        cmd.Parameters.AddWithValue("@ID", WD.FrameID.ToString());
                        cmd.Parameters.AddWithValue("@PageTitle", WD.PageTitle.ToString());
                        cmd.Parameters.AddWithValue("@PageContent", WD.PageContent.ToString());
                        cmd.Parameters.AddWithValue("@PageImage", WD.PageImage.ToString());
                        cmd.Parameters.AddWithValue("@CompanyLogoURL", WD.LogoImg.ToString());
                        cmd.Parameters.AddWithValue("@TopBannerBackColor", WD.TopBannerBackColor.ToString());
                        cmd.Parameters.AddWithValue("@BackColor", WD.BackgroundColor.ToString());
                        cmd.Parameters.AddWithValue("@Display", WD.Display.ToString());
                        cmd.Parameters.AddWithValue("@CreateBy", WD.CreateBy.ToString());
                        cmd.Parameters.AddWithValue("@CreateTime", WD.CreateTime.ToString());
                        cmd.Parameters.AddWithValue("@LastUser", WD.LastUpdateBy.ToString());
                        cmd.Parameters.AddWithValue("@LastUpdate", WD.LastUpdateTime.ToString());
                        cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                        cmd.Parameters.AddWithValue("@RecordUpdate", WD.LastUpdateTime.ToString());
                        cmd.ExecuteNonQuery();
                    }
                    objCnn.Close();
                    response = "Success";
                }
                catch (Exception ex)
                {
                    objCnn.Close();
                    response = ex.ToString();
                }
            }
        }
        
        return response;
    }

    public string deleteWebPageDesign(string FrameID)
    {
        string response = "";
        if (FrameID == null)
        {
            return "No records found.";
        }
        else
        {
            string queryUpdate = "SELECT *" +
                            " FROM webdesignsetting " +
                            " WHERE RecordStatus <> 'DELETED' AND ID=@FrameID";
            MySqlParameter[] objparam =
            {
                new MySqlParameter("@FrameID", FrameID)
            };
            DataTable UserDT = GetData_Vapt(queryUpdate, objparam);
            if (UserDT.Rows.Count != 0)
            {
                queryUpdate = "UPDATE webdesignsetting SET RecordStatus=@RecordStatus,Display=@Display,LastUpdate=@LastUpdate," +
                              " RecordUpdate=@RecordUpdate WHERE ID=@ID";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", FrameID.ToString());
                            cmd.Parameters.AddWithValue("@Display", "N");
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "DELETED");
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        response = "Success";
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        response = ex.ToString();
                    }
                }
            }
        }
        return response;
    }

    public string updateOnlineSalesPayment(onlineSalesPayment salesPayment)
    {
        string statusstr = "FAIL";
        int xpayment = 0;
        string salesid = "";
        if (salesPayment.salesorder_payment.Count() > 0)
        {
            for (int i = 0; i < salesPayment.salesorder_payment.Count(); i++)
            {
                SalesOnlineOrder_Payment payment = salesPayment.salesorder_payment[i];

                string paymentuuid = "";
                string strsql = "SELECT ID FROM list_paymentmethods " +
                            " WHERE 2C2PPaymentID IN(SELECT ID FROM list_3rdparty_payment WHERE Nick=@OthersPayment) " +
                            " AND RecordStatus<>'DELETED'";
                MySqlParameter[] objparam =
                {
                    new MySqlParameter("@OthersPayment", payment.OthersPayment)
                };
                DataTable paymentMethodDT = GetData_Vapt(strsql, objparam);
                if (paymentMethodDT.Rows.Count != 0)
                {
                    paymentuuid = Convert.ToString(paymentMethodDT.Rows[0]["ID"]);
                }
                paymentMethodDT.Clear();
                paymentMethodDT.Dispose();

                salesid = payment.SalesID.ToString();

                string queryUpdateSales = "UPDATE RETAIL_SALES SET SALESSTATUS = 'SALES' WHERE SALESID =@SALESID";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateSales, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@SALESID", salesid);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        return ex.ToString();
                    }
                }

                queryUpdateSales = "UPDATE RETAIL_SALES_PAYMENT SET PAYMENTID = @PaymentID,PaymentReference=@PaymentReference," +
                                "OTHERSPAYMENT = @OTHERSPAYMENT, OTHERSPAYMENTREF = @OTHERSPAYMENTREF,PaymentCardNo=PaymentCardNo," + 
                                "TID=@TID,MerchantID=@MerchantID,PaymentInvoiceNo=@PaymentInvoiceNo,PaymentApprovalCode=@PaymentApprovalCode " +
                                "WHERE SALESID =@SALESID";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateSales, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@SALESID", salesid);
                            cmd.Parameters.AddWithValue("@PaymentID", paymentuuid);
                            cmd.Parameters.AddWithValue("@PaymentReference", payment.OthersPaymentRef.ToString());
                            cmd.Parameters.AddWithValue("@OTHERSPAYMENT", payment.OthersPayment.ToString());
                            cmd.Parameters.AddWithValue("@OTHERSPAYMENTREF", payment.OthersPaymentRef.ToString());
                            cmd.Parameters.AddWithValue("@PaymentCardNo", payment.PaymentCardNo.ToString() );
                            cmd.Parameters.AddWithValue("@TID", payment.TID.ToString());
                            cmd.Parameters.AddWithValue("@MerchantID", payment.MerchantID.ToString());
                            cmd.Parameters.AddWithValue("@PaymentInvoiceNo", payment.PaymentInvoiceNo.ToString());
                            cmd.Parameters.AddWithValue("@PaymentApprovalCode", payment.PaymentApprovalCode.ToString());
                            cmd.Parameters.AddWithValue("@Issuer_country", payment.Issuer_country.ToString());
                            cmd.Parameters.AddWithValue("@Issuer_bank", payment.Issuer_bank.ToString());
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();

                        statusstr = "Success";
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        return ex.ToString();
                    }
                }
            }
        }
        return new JavaScriptSerializer().Serialize(new { Status = statusstr, SalesID = salesid });
    }
    
    public string saveDiscount(List<DiscountType> dtlist)
    {
        string response = "";
        string queryUpdate = "";
        for (int i = 0; i < dtlist.Count; i++)
        {
            DiscountType dt = (DiscountType)dtlist[i];
            string strsql = "SELECT ID  FROM list_discount_sales WHERE RecordStatus <> 'DELETED' AND ID=@ID";
            MySqlParameter[] objparam =
            {
            new MySqlParameter("@ID", dt.ID)
        };

            DataTable DT = GetData_Vapt(strsql, objparam);

            if (DT.Rows.Count != 0)
            {
                queryUpdate = "UPDATE list_discount_sales SET Nick=@Nick,Value=@Value,ButtonName=@ButtonName,PrintOnReceipt=@PrintOnReceipt,Full=@Full," +
                              "ItemDisc=@ItemDisc,GroupDisc=@GroupDisc,DiscType=@DiscType,DiscAmount=@DiscAmount,OpenDisc=@OpenDisc,Display=@Display WHERE ID=@ID";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", dt.ID);
                            cmd.Parameters.AddWithValue("@Nick", dt.Nick);
                            cmd.Parameters.AddWithValue("@Value", dt.Value);
                            cmd.Parameters.AddWithValue("@ButtonName", dt.ButtonName);
                            cmd.Parameters.AddWithValue("@PrintOnReceipt", dt.PrintOnReceipt);
                            cmd.Parameters.AddWithValue("@Full", dt.Full);
                            cmd.Parameters.AddWithValue("@ItemDisc", dt.ItemDisc);
                            cmd.Parameters.AddWithValue("@GroupDisc", dt.GroupDisc);
                            cmd.Parameters.AddWithValue("@DiscType", dt.DiscType);
                            cmd.Parameters.AddWithValue("@DiscAmount", dt.DiscAmount);
                            cmd.Parameters.AddWithValue("@OpenDisc", dt.OpenDisc);
                            cmd.Parameters.AddWithValue("@Display", dt.Display);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        response = "Success";
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        response = ex.ToString();
                    }
                }
            }
            else
            {
                queryUpdate = "INSERT INTO list_discount_sales(ID, Nick, Value, ButtonName, PrintOnReceipt, Full, ItemDisc, GroupDisc, DiscType, DiscAmount, OpenDisc, Display) " +
                             " VALUE " +
                             "(@ID,@Nick,@Value,@ButtonName,@PrintOnReceipt,@Full,@ItemDisc,@GroupDisc,@DiscType,@DiscAmount,@OpenDisc,@Display)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryUpdate, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@ID", dt.ID);
                            cmd.Parameters.AddWithValue("@Nick", dt.Nick.Length > 50 ? dt.Nick.Substring(0, 50) : dt.Nick);
                            cmd.Parameters.AddWithValue("@Value", dt.Value.Length > 40 ? dt.Value.Substring(0, 40) : dt.Value);
                            cmd.Parameters.AddWithValue("@ButtonName", dt.ButtonName.Length > 15 ? dt.ButtonName.Substring(0, 15) : dt.ButtonName);
                            cmd.Parameters.AddWithValue("@PrintOnReceipt", dt.PrintOnReceipt.Length > 40 ? dt.PrintOnReceipt.Substring(0, 40) : dt.PrintOnReceipt);
                            cmd.Parameters.AddWithValue("@Full", dt.Full);
                            cmd.Parameters.AddWithValue("@ItemDisc", dt.ItemDisc);
                            cmd.Parameters.AddWithValue("@GroupDisc", dt.GroupDisc);
                            cmd.Parameters.AddWithValue("@DiscType", dt.DiscType);
                            cmd.Parameters.AddWithValue("@DiscAmount", dt.DiscAmount);
                            cmd.Parameters.AddWithValue("@OpenDisc", dt.OpenDisc);
                            cmd.Parameters.AddWithValue("@Display", dt.Display);
                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        response = "Success";
                    }
                    catch (Exception ex)
                    {
                        objCnn.Close();
                        response = ex.ToString();
                    }
                }
            }
        }

        return response;
    }
}