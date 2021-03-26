using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class clsDataAccessor
{
    public string connectionstring;

    public int varOutputRet;
    //Initialize Datatable Globally
    public DataTable pDt = new DataTable();

    private int intresult;
    public string message = " ";

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
        }
        return intRetVal;
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

        if (cmd.Parameters != null && cmd.Parameters.Count > 0)
        {
            varOutputRet = Convert.ToInt32(cmd.Parameters[0].Value);
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

    public string getConnectionString(clsDataAccessor dataAccessor, string companyCode)
    {
        string sql = "SELECT COMPID, COMPNAME, `SERVER`, `DATABASE`, UID, `PASSWORD`,PORTNO,SPV05 AS COMUUID FROM DEFINITIONS_XMLCOMP WHERE ACTIVE = 'Y' AND compName='" + companyCode + "'";
        DataSet ds_Databaseinfor = dataAccessor.RunSPRetDataset(sql, "DEFINITIONS_XMPCOMP");
        string servername = "", databasename = "", userid = "", password = "", portno = "";
        DataRow dsRow = ds_Databaseinfor.Tables[0].Rows[0];
        servername = dsRow["SERVER"].ToString();
        databasename = dsRow["DATABASE"].ToString();
        userid = dsRow["UID"].ToString();
        password = dsRow["PASSWORD"].ToString();
        portno = dsRow["PORTNO"].ToString();
        return "SERVER=" + servername + ";Database=" + databasename + ";UID=" + userid + ";PASSWORD=" + password + ";Port=" + portno + ";CharSet=utf8;Convert Zero Datetime=True;";
    }

	public List<ItemPrice> GetPrices(string ItemID, string RetailID)
    {
        List<ItemPrice> prices = new List<ItemPrice>();
        
        DataTable dt = GetData(string.Format("SELECT inventory_unit.Item_UnitID as Item_UnitID,(SELECT Nick FROM list_units WHERE ID= ItemUnit) AS UOM , inventory_unit.RTLSellPx as Price, inventory_unit.ItemUnitDef as ItemUnitDef, IF(inventory_unit.ItemUnitDef='Y',inventory_retail.OnHandQty,0) AS OnHandQty " +
                                " FROM  inventory_unit " +
                                " LEFT JOIN inventory_retail ON inventory_unit.itemID = inventory_retail.ItemID" +
                                " WHERE inventory_unit.RecordStatus<>'DELETED' AND inventory_unit.ItemID='{0}' AND inventory_retail.RetailID='{1}' ORDER BY inventory_unit.ItemID ASC, inventory_unit.ItemUnitDef DESC ", ItemID, RetailID));
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

        DataTable dt = GetData(string.Format("SELECT SupplierID, SupBarCode, DefaultSupplier " +
                                " FROM  inventory_supbar WHERE recordstatus<>'DELETED' AND ItemID='{0}' ORDER BY SupplierID ASC, DefaultSupplier DESC ", ItemID));
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
        DataTable dt = GetData(string.Format("SELECT ItemID " +
                                " FROM  inventory " +
                                " WHERE RecordStatus <> 'DELETED' AND ItemID = '{0}'", ItemID));
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
        DataTable dt = GetData(string.Format("SELECT inventory.ItemID " +
                                " FROM  inventory " +
                                " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                                " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                                " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID = '{0}' AND list_units.Nick = '{1}'", ItemID, UOM));
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
        DataTable dt = GetData(string.Format("SELECT ID " +
                                " FROM list_paymentmethods " +
                                " WHERE RecordStatus <> 'DELETED' AND Nick = '{0}'", payment));
        if (dt.Rows.Count == 0)
        {
            return "Payment (" + payment +") not found.";
        }
        else
        {
            return "OK";
        }
    }

    public string saveSales(SalesMasterDCS sales)
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
            DataTable SalesDT = GetData(string.Format("SELECT *" +
                            " FROM retail_sales " +
                            " WHERE RecordStatus <> 'DELETED' AND SalesNo = '{0}' AND RetailID = '{1}' AND SalesStatus = '{2}' AND SalesDate = '{3}'", sales.TransNo.ToString(), sales.RetailID.ToString(), sales.SalesStatus.ToString(), sales.SalesDate.ToString("yyyy-MM-dd")));
            if (SalesDT.Rows.Count == 0)
            {
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
                        SalesBalTtl = SalesBalTtl + sales.ItemSales[i].ItemTotal;
                        string ItemUOM = sales.ItemSales[i].ItemUOMDesc.ToString();
                        string SupplierID = "";
                        decimal ItemActQty = 0;
                        decimal ItemUnitCost = 0;
                        decimal ItemAveCost = 0;
                        if (sales.ItemSales[i].ItemID.ToString() != "")
                        {
                            DataTable ItemDT = GetData(string.Format("SELECT inventory.ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode, inventory.ItemDescp, inventory_unit.ItemUnit AS ItemUOM, list_units.Nick AS ItemUOMDesc, inventory_unit.ItemActQty, inventory_unit.RTLSellPx AS ItemUnitPrice, inventory_unit.PurchaseCost AS ItemUnitCost, inventory.ItemAveCost, inventory.ItemSKUSup AS SupplierID, inventory.ItemCategory AS CategoryID, inventory.ItemDepartment AS DepartmentID, inventory.ItemGroup AS GroupID, inventory.ItemBrand as BrandID" +
                            " FROM inventory " +
                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                            " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID = '{0}' AND list_units.Nick = '{1}'" +
                            " LIMIT 1", sales.ItemSales[i].ItemID.ToString(), sales.ItemSales[i].ItemUOMDesc.ToString()));

                            ItemUOM = Convert.ToString(ItemDT.Rows[0]["ItemUOM"]);
                            ItemActQty = Convert.ToDecimal(ItemDT.Rows[0]["ItemActQty"]);
                            ItemUnitCost = Convert.ToDecimal(ItemDT.Rows[0]["ItemUnitCost"]);
                            ItemAveCost = Convert.ToDecimal(ItemDT.Rows[0]["ItemAveCost"]);
                            SupplierID = Convert.ToString(ItemDT.Rows[0]["SupplierID"]);
                        }

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
                    //}
                    //else
                    //{
                    //    return response;
                    //}
                }

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
                                cmd.Parameters.AddWithValue("@PaymentID", sales.SalesPayments[i].paymentID.ToString());
                                //cmd.Parameters.AddWithValue("@PaymentID", Convert.ToString(PaymentDT.Rows[0]["ID"]));
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

                        return "Success";
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
            if (sales.SalesStatus == "VOID")
            {
                salesno = sales.TransNo + "V";
            }
            else {
                salesno = sales.TransNo;
            }

            DataTable SalesDT = GetData(string.Format("SELECT *" +
                            " FROM retail_sales " +
                            " WHERE RecordStatus <> 'DELETED' AND SalesNo = '{0}' AND RetailID = '{1}' AND SalesStatus = '{2}' AND SalesDate = '{3}'", salesno.ToString(), sales.RetailID.ToString(), sales.SalesStatus.ToString(), sales.SalesDate.ToString("yyyy-MM-dd")));
            if (SalesDT.Rows.Count != 0)
            {
                if (sales.SalesStatus == "VOID")
                {
                    blnContinue = true;
                }
                else
                {
                    blnContinue = false;
                }
            }
            else
            {
                blnContinue = true;
            }

            if (blnContinue==true)
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
                                    cmd.Parameters.AddWithValue("@ItemUnitPrice", (sales.ItemSales[i].ItemPrice *-1));
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
                    DataTable PaymentDT = GetData(string.Format("SELECT ID" +
                                " FROM list_paymentmethods " +
                                " WHERE RecordStatus <> 'DELETED' AND SPV05='{0}'",sales.SalesPayments[i].paymentID.ToString()));

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

                        return "Success";
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

    public string saveOnlineSales(SalesMaster sales)
    {
        //Retail sales
        Guid SalesID = Guid.NewGuid();
        Guid CustSalesID = Guid.NewGuid();
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
            string strMemberID = "";
            string strRetailID = "1";

            DataTable DT = GetData(string.Format("SELECT RetailID FROM retailer WHERE RetailType='{0}'  " +
                            " AND RecordStatus <> 'DELETED' ", sales.RetailID.ToString()));
            if (DT.Rows.Count != 0)
            {
                strRetailID = Convert.ToString(DT.Rows[0]["RetailID"]);
            }
            DT.Clear();

            if (sales.MemberID != "" && sales.MemberID != "-1")
            {
                string strICNO = sales.MemberID.Substring(0, 4);
                string strHph = sales.MemberID.Substring(4, (sales.MemberID.Length - 4));

                DT = GetData(string.Format("SELECT c.ID AS CustID, c.CustomerFirstName AS CustName, c.hph AS CustHP, ct.AmountperPoint " +
                    "FROM customer c INNER JOIN customer_type ct ON ct.CustTypeID =  c.customertype " +
                    "WHERE RIGHT(c.CustICNO,4)='{0}' AND c.hph='{1}'", strICNO, strHph));
                if (DT.Rows.Count != 0)
                {
                    strMemberID = Convert.ToString(DT.Rows[0]["CustID"]);
                }
                DT.Clear();
            }

            DataTable SalesDT = GetData(string.Format("SELECT *" +
                            " FROM retail_sales " +
                            " WHERE RecordStatus <> 'DELETED' AND SalesNo = '{0}' AND RetailID = '{1}' AND SalesStatus = '{2}' AND SalesDate = '{3}'", sales.TransNo.ToString(), strRetailID.ToString(), sales.SalesStatus.ToString(), sales.SalesDate.ToString("yyyy-MM-dd")));
            if (SalesDT.Rows.Count == 0)
            {
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
                        SalesBalTtl = SalesBalTtl + sales.ItemSales[i].ItemTotal;
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

                        if (sales.ItemSales[i].ItemID.ToString() != "")
                        {
                            DataTable ItemDT = GetData(string.Format("SELECT inventory.ItemID AS ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode, " +
                            " inventory.ItemDescp, inventory_unit.ItemUnit AS ItemUOM, list_units.Nick AS ItemUOMDesc, inventory_unit.ItemActQty, " +
                            " inventory_unit.RTLSellPx AS ItemUnitPrice, inventory_unit.PurchaseCost AS ItemUnitCost, inventory.ItemAveCost, " +
                            " inventory.ItemSKUSup AS SupplierID, inventory.ItemCategory AS CategoryID, inventory.ItemDepartment AS DepartmentID, inventory.ItemGroup AS GroupID, inventory.ItemBrand as BrandID" +
                            " FROM inventory " +
                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                            " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemSKU = '{0}' AND list_units.Nick = '{1}'" +
                            " LIMIT 1", sales.ItemSales[i].ItemID.ToString(), sales.ItemSales[i].ItemUOMDesc.ToString()));
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

                            DataTable DefaultDT = GetData("SELECT DefaultCurrency, exchange_rate.ExchRate" +
                            " FROM definitions LEFT JOIN exchange_rate ON definitions.DefaultCountry = exchange_rate.CountryID AND definitions.DefaultCurrency = exchange_rate.ExchCurr");

                            currency = Convert.ToString(DefaultDT.Rows[0]["DefaultCurrency"]);
                            ExchRate = Convert.ToDecimal(DefaultDT.Rows[0]["ExchRate"]);

                            DataTable BaseUOMDT = GetData(string.Format("SELECT iu.ItemUnit as ItemBaseUOMID, iu.ItemActQty as ItemBaseActQty, lu.Nick AS ItemBaseUOM" +
                            " FROM inventory_unit iu" +
                            " LEFT JOIN list_units lu ON iu.ItemUnit = lu.ID" +
                            " WHERE iu.RecordStatus <> 'DELETED' AND iu.ItemUnitDef='Y' AND iu.ItemID = '{0}'", ItemID));

                            ItemBaseUOMID = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOMID"]);
                            ItemBaseUOM = Convert.ToString(BaseUOMDT.Rows[0]["ItemBaseUOM"]);
                            ItemBaseActQty = Convert.ToDecimal(BaseUOMDT.Rows[0]["ItemBaseActQty"]);
                        }

                        //insert customer_salesdetails
                        if (strMemberID != "")
                        {
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
                                        cmd.Parameters.AddWithValue("@CustName", Convert.ToString(DT.Rows[0]["CustName"]));
                                        cmd.Parameters.AddWithValue("@CustHP", Convert.ToString(DT.Rows[0]["CustHP"]));
                                        cmd.Parameters.AddWithValue("@bitCreateNew", "N");
                                        cmd.Parameters.AddWithValue("@TotalAmount", sales.SalesTotalAmount);
                                        cmd.Parameters.AddWithValue("@bitLoyalty", "Y");
                                        cmd.Parameters.AddWithValue("@LoyaltyPoint", sales.SalesTotalAmount * Convert.ToDecimal(DT.Rows[0]["AmountperPoint"]));
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
                        


                        baseQty = sales.ItemSales[i].ItemQty * ItemActQty;
                        Guid AgingID = Guid.NewGuid();

                        //inventory_aging
                        agingJson = agingJson + "{";
                        agingJson = agingJson + string.Format(@"""ID"":""{0}"",""SupplierID"":""{1}"",""RetailID"":""{2}"",""ItemID"":""{3}"",""ItemSKU"":""{4}"",
""TransID"":""{5}"",""TransNo"":""{6}"",""TransDate"":""{7}"",""ItemUOMID"":""{8}"",""ItemUOM"":""{9}"",""ItemBaseUOMID"":""{10}"",""ItemBaseUOM"":""{11}"",
""Qty"":{12},""ItemActualQty"":{13},""CurrencyID"":""{14}"",""ExcRate"":{15},""CostUnitPx"":{16},""LocalCostUnitPx"":{17},""CreateTime"":""{18}"",""BatchNo"":"""",
""HSCode"":"""",""ExpireDate"":"""",""ExpiryDay"":0,""ItemDefActualQty"":{19},""PDQty"":0,""SoldQty"":{20},""TrfInQty"":0,""TrfOutQty"":0,""AdjQty"":0,""RetQty"":0,""SDQty"":0,""KitQty"":0,
""DekitQty"":0,""ReserveQty"":0,""InTransitQty"":0,""QtyBalance"":{21},""RFID"":""""", AgingID, SupplierID, strRetailID, ItemID, sales.ItemSales[i].ItemID,
SalesID, sales.TransNo, sales.SalesDate, ItemUOM, sales.ItemSales[i].ItemUOMDesc, ItemBaseUOMID, ItemBaseUOM,
sales.ItemSales[i].ItemQty, ItemActQty, currency, ExchRate, ItemUnitCost, ItemUnitCost, sales.SalesDate, ItemBaseActQty, baseQty, baseQty);
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
                                    cmd.Parameters.AddWithValue("@ItemQty", sales.ItemSales[i].ItemQty);
                                    cmd.Parameters.AddWithValue("@ItemUOM", ItemUOM);
                                    cmd.Parameters.AddWithValue("@ItemUOMDesc", sales.ItemSales[i].ItemUOMDesc.ToString());
                                    cmd.Parameters.AddWithValue("@ItemQtyAct", ItemActQty);
                                    cmd.Parameters.AddWithValue("@ItemUnitPrice", sales.ItemSales[i].ItemPrice);
                                    cmd.Parameters.AddWithValue("@ItemUnitCost", ItemUnitCost);
                                    cmd.Parameters.AddWithValue("@ItemAveCost", ItemAveCost);
                                    cmd.Parameters.AddWithValue("@ItemDiscPerc", sales.ItemSales[i].ItemDiscPerc);
                                    cmd.Parameters.AddWithValue("@ItemDiscPerc2", sales.ItemSales[i].ItemDiscPerc2);
                                    cmd.Parameters.AddWithValue("@ItemDiscPerc3", sales.ItemSales[i].ItemDiscPerc3);
                                    cmd.Parameters.AddWithValue("@ItemDisc", sales.ItemSales[i].ItemDisc);
                                    cmd.Parameters.AddWithValue("@ItemDisc2", sales.ItemSales[i].ItemDisc2);
                                    cmd.Parameters.AddWithValue("@ItemDisc3", sales.ItemSales[i].ItemDisc3);
                                    cmd.Parameters.AddWithValue("@ItemTotalDisc", sales.ItemSales[i].ItemDisc + sales.ItemSales[i].ItemDisc2 + sales.ItemSales[i].ItemDisc3);
                                    cmd.Parameters.AddWithValue("@ItemSubTotal", sales.ItemSales[i].ItemTotal - sales.ItemSales[i].ItemTax);
                                    cmd.Parameters.AddWithValue("@ItemTaxTotal", sales.ItemSales[i].ItemTax);
                                    cmd.Parameters.AddWithValue("@ItemTotal", sales.ItemSales[i].ItemTotal);
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
                                            cmd.Parameters.AddWithValue("@RetailID", strRetailID);
                                            cmd.Parameters.AddWithValue("@ItemID", 0);
                                            cmd.Parameters.AddWithValue("@SupBarCode", "Less");
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
                                string queryUpdateOrder = string.Format("UPDATE inventory_voucher " +
                                        " SET SoldFromRetailID = @SoldFromRetailID, SoldTransID = @SoldTransID, bitSold = 'Y', LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                                        " WHERE ItemID = '{0}' AND SerialNo = '{1}'", ItemID, sales.ItemSales[i].ItemVoucher[x].VoucherNo.ToString());

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
                agingJson = agingJson.Remove(agingJson.Length - 1) + "]}";
                for (int i = 0; i < sales.SalesPayments.Count(); i++)
                {
                    Guid SalesPaymentID = Guid.NewGuid();
                    var strWhere = "";
                    strWhere = " AND Nick ='" + sales.SalesPayments[i].strPayment.ToString() + "'";

                    if (sales.SalesPayments[i].paymentID.ToString() != "")
                    {
                        strWhere = strWhere + " AND ID='" + sales.SalesPayments[i].paymentID.ToString() + "'";
                    }
                    TotalChangeAmt = TotalChangeAmt + sales.SalesPayments[i].ChangeAmount;
                    DataTable PaymentDT = GetData(string.Format("SELECT ID" +
                                " FROM list_paymentmethods " +
                                " WHERE RecordStatus <> 'DELETED' {0}", strWhere));

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
                                //cmd.Parameters.AddWithValue("@PaymentID", sales.SalesPayments[i].paymentID.ToString());
                                cmd.Parameters.AddWithValue("@PaymentID", Convert.ToString(PaymentDT.Rows[0]["ID"]));
                                cmd.Parameters.AddWithValue("@SalesPayTtl", sales.SalesPayments[i].SalesPayTtl);
                                cmd.Parameters.AddWithValue("@SalesBalTtl", sales.SalesPayments[i].SalesBalTtl);
                                cmd.Parameters.AddWithValue("@ChangeAmount", sales.SalesPayments[i].ChangeAmount);
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

                                string queryUpdateOrder = string.Format("UPDATE inventory_voucher " +
                                            " SET RedeemFromRetailID = @RedeemFromRetailID, RedeemTransID = @RedeemTransID, bitRedeem = 'Y', LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                                            " WHERE SerialNo = '{0}'", sales.ItemSales[i].ItemVoucher[y].VoucherNo.ToString());

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
                /*if (sales.SalesPersons.Count() != 0)
                {
                    for (int i = 0; i < sales.SalesPersons.Count(); i++)
                    {

                    }
                }*/

                //retailSalesPerson
                DataTable Sales2DT = GetData("SELECT DefaultGST,DefaultGSTVal" +
                                    " FROM definitions ");
                string queryInsertSales = "INSERT INTO retail_sales " +
                            " (SalesID, RetailID, SalesNo, SalesTax, SalesTaxVal, SalesDate, CloseRetailID, CloseDate, CloseTime, " +
                            " SalesStatus, SalesSubTtl, SalesTaxTtl, SalesBalTtl, SalesPayTtl, SalesChangeAmt, SalesRounding,SalesDisc,SalesDisc2,SalesDisc3," +
                            "SalesDiscAmt,SalesDiscAmt2,SalesDiscAmt3,SalesDiscGroupAmt,SalesTotalGroupDisc, " +
                            "CreateTime, MemberID, MemberDisc,MemberAmt,PendingSync, LastUpdate, LockUpdate, RecordStatus, RecordUpdate, QueueStatus)" +
                            " VALUE " +
                            " (@SalesID, @RetailID, @SalesNo, @SalesTax, @SalesTaxVal, @SalesDate, @CloseRetailID, @CloseDate, @CloseTime, @SalesStatus, @SalesSubTtl, @SalesTaxTtl, @SalesBalTtl, @SalesPayTtl, @SalesChangeAmt, @SalesRounding," +
                            " @SalesDiscPerc,@SalesDiscPerc2,@SalesDiscPerc3,@SalesDiscAmt,@SalesDiscAmt2,@SalesDiscAmt3,@SalesDiscGroupAmt,@SalesTotalDiscount, " +
                            " @CreateTime, @MemberID, @MemberDisc, @MemberAmt, @PendingSync, @LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate, @QueueStatus)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsertSales, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@SalesID", SalesID);
                            cmd.Parameters.AddWithValue("@RetailID", strRetailID.ToString());
                            cmd.Parameters.AddWithValue("@SalesNo", sales.TransNo.ToString());
                            //cmd.Parameters.AddWithValue("@SalesTax", Convert.ToString(Sales2DT.Rows[0]["DefaultGST"]));
                            cmd.Parameters.AddWithValue("@SalesTax", sales.SalesTaxType);
                            //cmd.Parameters.AddWithValue("@SalesTaxVal", Convert.ToDecimal(Sales2DT.Rows[0]["DefaultGSTVal"]));
                            cmd.Parameters.AddWithValue("@SalesTaxVal", sales.SalesTaxRate);
                            cmd.Parameters.AddWithValue("@SalesDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@CloseRetailID", strRetailID.ToString());
                            cmd.Parameters.AddWithValue("@CloseDate", sales.SalesDate.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@CloseTime", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@SalesStatus", sales.SalesStatus.ToString());
                            cmd.Parameters.AddWithValue("@SalesSubTtl", SalesBalTtl - sales.SalesTaxTtl);
                            cmd.Parameters.AddWithValue("@SalesTaxTtl", sales.SalesTaxTtl);
                            cmd.Parameters.AddWithValue("@SalesBalTtl", SalesBalTtl);
                            cmd.Parameters.AddWithValue("@SalesPayTtl", SalesBalTtl);
                            cmd.Parameters.AddWithValue("@SalesChangeAmt", TotalChangeAmt);
                            cmd.Parameters.AddWithValue("@SalesRounding", sales.SalesRounding);
                            cmd.Parameters.AddWithValue("@SalesDiscPerc", sales.SalesDiscPerc);
                            cmd.Parameters.AddWithValue("@SalesDiscPerc2", sales.SalesDiscPerc2);
                            cmd.Parameters.AddWithValue("@SalesDiscPerc3", sales.SalesDiscPerc3);
                            cmd.Parameters.AddWithValue("@SalesDiscAmt", sales.SalesDiscAmt);
                            cmd.Parameters.AddWithValue("@SalesDiscAmt2", sales.SalesDiscAmt2);
                            cmd.Parameters.AddWithValue("@SalesDiscAmt3", sales.SalesDiscAmt3);
                            cmd.Parameters.AddWithValue("@SalesDiscGroupAmt", sales.SalesDiscAmt + sales.SalesDiscAmt2 + sales.SalesDiscAmt3);
                            cmd.Parameters.AddWithValue("@SalesTotalDiscount", sales.SalesTotalDiscount);
                            cmd.Parameters.AddWithValue("@CreateTime", sales.SalesDate);
                            cmd.Parameters.AddWithValue("@MemberID", strMemberID.ToString());
                            cmd.Parameters.AddWithValue("@MemberDisc", sales.MemberDisc);
                            cmd.Parameters.AddWithValue("@MemberAmt", sales.MemberAmt);
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
                        return "Success";
                        
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
            newSellingPrice = SellingPrice*Qty;
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
            DataTable OrderDT = GetData(string.Format("SELECT *" +
                            " FROM retail_sales_order " +
                            " WHERE RecordStatus <> 'DELETED' AND RefID = '{0}' AND RefNo = '{1}' AND RetailID = '{2}'", orders.RefID.ToString(), orders.RefNo.ToString(), orders.RetailID.ToString()));
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
                                        DataTable ItemDT = GetData(string.Format("SELECT inventory.ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode" +
                                            " FROM inventory " +
                                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID = '{0}'" +
                                            " LIMIT 1", orders.ItemOrder[i].ParentID.ToString()));

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
                                DataTable ParentDT = GetData(string.Format("SELECT inventory.ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode" +
                                    " FROM inventory " +
                                    " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                                    " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                                    " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID = '{0}'" +
                                    " LIMIT 1", orders.ItemOrder[i].ParentID.ToString()));

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
            DataTable OrderDT = GetData(string.Format("SELECT *" +
                            " FROM retail_sales_order " +
                            " WHERE RecordStatus <> 'DELETED' AND RefID = '{0}' AND RefNo = '{1}' AND RetailID = '{2}' AND ParentItemID = '{3}'", calOrder.RefID.ToString(), calOrder.RefNo.ToString(), calOrder.RetailID.ToString(), calOrder.ParentItemID.ToString()));
            if (OrderDT.Rows.Count != 0)
            {
                string queryUpdateOrder = string.Format("Update retail_sales_order " +
                                        " SET CancelStatus = @CancelStatus, RefundStatus = @RefundStatus, LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                                        " WHERE RefID = '{0}' AND RefNO = '{1}' AND RetailID = '{2}' AND ParentItemID = '{3}'", calOrder.RefID.ToString(), calOrder.RefNo.ToString(), calOrder.RetailID.ToString(), calOrder.ParentItemID.ToString());

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
            DataTable pdDT = GetData(string.Format("SELECT *" +
                            " FROM prch_delivery " +
                            " WHERE RecordStatus <> 'DELETED' AND IDRef = '{0}' AND DONo = '{1}'", PD.IDRef.ToString(), PD.DONo.ToString()));
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
                    DataTable itemDT = GetData(string.Format("SELECT inventory.ItemID, inventory.ItemSKU, inventory_supbar.SupBarCode AS SupBarItem, inventory_supbar.Item_SupBarID AS SupBarItemID, inventory_unit.ItemUnit AS ItemUnitID, inventory_unit.ItemActQty AS ActualQty, B.ItemUnit AS ItemBaseUnitID, C.Nick AS ItemBaseUnit" +
                            " FROM inventory " +
                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                            " LEFT JOIN (SELECT inventory.ItemID, inventory_unit.ItemUnit " +
                            " FROM inventory_unit " +
                            " LEFT JOIN inventory ON inventory_unit.ItemID = inventory.ItemID " +
                            " WHERE inventory_unit.RecordStatus <> 'DELETED' AND inventory_unit.ItemActQty = 1 AND inventory.ItemSKU = '{0}' ) AS B ON B.ItemID = inventory.ItemID " +
                            " LEFT JOIN list_units C ON B.ItemUnit = C.ID " +
                            " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemSKU = '{1}' AND list_units.Nick = '{2}'", PD.PDItems[i].ItemSKU.ToString(), PD.PDItems[i].ItemSKU.ToString(),PD.PDItems[i].ItemUOM.ToString()));
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
							DataTable newPDDT = GetData(string.Format("SELECT *" +
                            " FROM prch_delivery " +
                            " WHERE RecordStatus <> 'DELETED' AND IDRef = '{0}' AND DONo = '{1}'", PD.IDRef.ToString(), PD.DONo.ToString()));
            
                            string queryUpdatePD = string.Format("Update prch_delivery " +
                                        " SET BalSubTotal=@BalSubTotal, BalTax=@BalTax, BalTotal=@BalTotal, BalPayable=@BalPayable, LocalBalSubTotal=@LocalBalSubTotal, LocalTax=@LocalTax, LocalTotal=@LocalTotal, LocalBalPayable=@LocalBalPayable, LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                                        " WHERE RecordStatus <> 'DELETED' AND IDRef = '{0}' AND DONo = '{1}'", PD.IDRef.ToString(), PD.DONo.ToString());

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
			DataTable sdDT = GetData(string.Format("SELECT *" +
						   " FROM salesdelivery " +
						   " WHERE RecordStatus <> 'DELETED'  AND INVID = '{0}' AND INVRef = '{1}'", SD.INVID.ToString(), SD.INVRef.ToString()));
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
					string sqlstr = string.Format("DELETE FROM SALESDELIVERY WHERE ID = '{0}' ; ", IDSTR);
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

/*	public string saveSalesDelivery_v3(SalesDelivery SD)
    {
        string response = "";
        string SD_ID = "";
        //Guid SD_ID = Guid.NewGuid();
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
            DataTable sdDT = GetData(string.Format("SELECT *" +
                            " FROM salesdelivery " +
                            " WHERE RecordStatus <> 'DELETED' AND INVID = '{0}' AND INVRef = '{1}'", SD.INVID.ToString(), SD.INVRef.ToString()));
            if (sdDT.Rows.Count == 0)
            {
                
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


                //insert SD table
                string queryInsertSD = "INSERT INTO salesdelivery " +
                                    " ( TYPE, CompanyID, CoyCode, CompanyAddr, CompanyTel, CompanyFax, RecipientName, RecipientAddr, RecipientPostCode, RecipientAttn, RecipientTel, IDRef, RetailerID, DATE, INVID, INVRef, INVDate, " +
                                    " Gst, GSTIncEx, GstRate, BalSubTotal, BalTax, BalTotal, BalPayable, LocalBalSubTotal, LocalTax, LocalTotal, LocalBalPayable, Exch, ExchRate, vchRemarks, LastUpdate, RecordUpdate, RecStatus) " +
                                    " VALUE " +
                                    " ( @TYPE, @CompanyID, @CoyCode, @CompanyAddr, @CompanyTel, @CompanyFax, @RecipientName, @RecipientAddr, @RecipientPostCode, @RecipientAttn, @RecipientTel, @IDRef, @RetailerID, @DATE, @INVID, @INVRef, @INVDate, " +
                                    " @Gst, @GSTIncEx, @GstRate, @BalSubTotal, @BalTax, @BalTotal, @BalPayable, @LocalBalSubTotal, @LocalTax, @LocalTotal, @LocalBalPayable, @Exch, @ExchRate, @vchRemarks, @LastUpdate, @RecordUpdate, @RecStatus)";
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(queryInsertSD, objCnn))
                        {
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
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
							cmd.Parameters.AddWithValue("@RecStatus", "NEW");
                            cmd.ExecuteNonQuery();
                            SD_ID = cmd.LastInsertedId.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }
                }


                for (int i = 0; i < SD.SalesDeliveryItems.Count(); i++)
                {

                    SDKeyCol = Guid.NewGuid();

                    //retrieve from inventory
                    DataTable itemDT = GetData(string.Format("SELECT inventory.ItemID, inventory.ItemSKU, inventory.ItemDescp, inventory.ItemOtherLanguage, inventory_supbar.SupBarCode AS SupBarItem, inventory_supbar.Item_SupBarID AS SupBarItemID, inventory_unit.ItemUnit AS ItemUnitID, inventory_unit.ItemActQty AS ActualQty, B.ItemUnit AS ItemBaseUnitID, C.Nick AS ItemBaseUnit" +
                            " FROM inventory " +
                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                            " LEFT JOIN (SELECT inventory.ItemID, inventory_unit.ItemUnit " +
                            " FROM inventory_unit " +
                            " LEFT JOIN inventory ON inventory_unit.ItemID = inventory.ItemID " +
                            " WHERE inventory_unit.RecordStatus <> 'DELETED' AND inventory_unit.ItemActQty = 1 AND inventory.ItemID = '{0}' ) AS B ON B.ItemID = inventory.ItemID " +
                            " LEFT JOIN list_units C ON B.ItemUnit = C.ID " +
                            " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID = '{1}' AND inventory_supbar.Item_SupBarID = '{2}' AND list_units.Nick = '{3}'", SD.SalesDeliveryItems[i].ItemID.ToString(), SD.SalesDeliveryItems[i].ItemID.ToString(), SD.SalesDeliveryItems[i].SupBarItemID.ToString(), SD.SalesDeliveryItems[i].ItemUOM.ToString()));
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
                                    " (ItemID, SupBarItem, SupBarItemID, ID, ItemSKU, ItemDesc, ItemOtherLanguage, ItemQty, ActualQty, ActualSOQty, Currency, ExchRate, GST, GSTType, GSTRate, ItemPrice, " +
                                    " ItemUnit, ItemUnitID, ItemBaseUnit, ItemBaseUnitID, ItemBal, ItemSubTotal, ItemGST, Total, LocalItemPrice, LocalItemSubTotal, LocalItemGST, LocalTotal, LastUpdate, RecordUpdate) " +
                                    " VALUE " +
                                    " (@ItemID, @SupBarItem, @SupBarItemID, @ID, @ItemSKU, @ItemDesc, @ItemOtherLanguage, @ItemQty, @ActualQty, @ActualSOQty, @Currency, @ExchRate, @GST, @GSTType, @GSTRate, @ItemPrice, " +
                                    " @ItemUnit, @ItemUnitID, @ItemBaseUnit, @ItemBaseUnitID, @ItemBal, @ItemSubTotal, @ItemGST, @Total, @LocalItemPrice, @LocalItemSubTotal, @LocalItemGST, @LocalTotal, @LastUpdate, @RecordUpdate)";
                            using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                            {
                                try
                                {
                                    objCnn.Open();
                                    using (MySqlCommand cmd = new MySqlCommand(queryInsertSDItem, objCnn))
                                    {
                                        cmd.Parameters.AddWithValue("@ItemID", Convert.ToString(itemDT.Rows[0]["ItemID"]));
                                        cmd.Parameters.AddWithValue("@SupBarItem", Convert.ToString(itemDT.Rows[0]["SupBarItem"]));
                                        cmd.Parameters.AddWithValue("@SupBarItemID", Convert.ToString(itemDT.Rows[0]["SupBarItemID"]));
                                        cmd.Parameters.AddWithValue("@ID", SD_ID);
                                        cmd.Parameters.AddWithValue("@ItemSKU", Convert.ToString(itemDT.Rows[0]["ItemSKU"]));
                                        cmd.Parameters.AddWithValue("@ItemDesc", Convert.ToString(itemDT.Rows[0]["ItemDescp"]));
                                        cmd.Parameters.AddWithValue("@ItemOtherLanguage", Convert.ToString(itemDT.Rows[0]["ItemOtherLanguage"]));
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

                //update SD table
                string queryUpdateSD = "Update salesdelivery " +
                            " SET BalSubTotal=@BalSubTotal, BalTax=@BalTax, BalTotal=@BalTotal, BalPayable=@BalPayable, LocalBalSubTotal=@LocalBalSubTotal, LocalTax=@LocalTax, LocalTotal=@LocalTotal, LocalBalPayable=@LocalBalPayable, LastUpdate = @LastUpdate, RecordUpdate = @RecordUpdate " +
                            " WHERE RecordStatus <> 'DELETED'";

                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        objCnn.Open();

                        using (MySqlCommand cmd = new MySqlCommand(queryUpdateSD, objCnn))
                        {
                            cmd.Parameters.AddWithValue("@BalSubTotal", SDSubTotal);
                            cmd.Parameters.AddWithValue("@BalTax", SDTax);
                            cmd.Parameters.AddWithValue("@BalTotal", SDTotal);
                            cmd.Parameters.AddWithValue("@BalPayable", SDTotal);
                            cmd.Parameters.AddWithValue("@LocalBalSubTotal", SDSubTotal * ExchRate);
                            cmd.Parameters.AddWithValue("@LocalTax", SDTax * ExchRate);
                            cmd.Parameters.AddWithValue("@LocalTotal", SDTotal * ExchRate);
                            cmd.Parameters.AddWithValue("@LocalBalPayable", SDTotal * ExchRate);
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
	*/
	
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
            DataTable sdDT = GetData(string.Format("SELECT *" +
                            " FROM salesdelivery " +
                            " WHERE RecordStatus <> 'DELETED' AND INVID = '{0}' AND INVRef = '{1}'", SD.INVID.ToString(), SD.INVRef.ToString()));
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
                    DataTable itemDT = GetData(string.Format("SELECT inventory.ItemID, inventory.ItemSKU, inventory.ItemDescp, inventory.ItemOtherLanguage, inventory_supbar.SupBarCode AS SupBarItem, inventory_supbar.Item_SupBarID AS SupBarItemID, inventory_unit.ItemUnit AS ItemUnitID, inventory_unit.ItemActQty AS ActualQty, B.ItemUnit AS ItemBaseUnitID, C.Nick AS ItemBaseUnit" +
                            " FROM inventory " +
                            " LEFT JOIN inventory_supbar ON inventory.ItemID = inventory_supbar.ItemID " +
                            " LEFT JOIN inventory_unit ON inventory.ItemID = inventory_unit.ItemID " +
                            " LEFT JOIN (SELECT inventory.ItemID, inventory_unit.ItemUnit " +
                            " FROM inventory_unit " +
                            " LEFT JOIN inventory ON inventory_unit.ItemID = inventory.ItemID " +
                            " WHERE inventory_unit.RecordStatus <> 'DELETED' AND inventory_unit.ItemActQty = 1 AND inventory.ItemID = '{0}' ) AS B ON B.ItemID = inventory.ItemID " +
                            " LEFT JOIN list_units C ON B.ItemUnit = C.ID " +
                            " LEFT JOIN list_units ON inventory_unit.ItemUnit = list_units.ID " +
                            " WHERE inventory.RecordStatus <> 'DELETED' AND inventory.ItemID = '{1}' AND inventory_supbar.Item_SupBarID = '{2}' AND list_units.Nick = '{3}'", SD.SalesDeliveryItems[i].ItemID.ToString(), SD.SalesDeliveryItems[i].ItemID.ToString(), SD.SalesDeliveryItems[i].SupBarItemID.ToString(), SD.SalesDeliveryItems[i].ItemUOM.ToString()));
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
                DataTable catDT = GetData(string.Format("SELECT *" +
                                " FROM list_categories " +
                                " WHERE RecordStatus <> 'DELETED' AND ID = '{0}'", EC.ECategory[i].cat_id.ToString()));
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
                DataTable catDT = GetData(string.Format("SELECT *" +
                                " FROM list_categories " +
                                " WHERE RecordStatus <> 'DELETED' AND ID = '{0}'", EC.ECategory[i].cat_id.ToString()));
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
            DataTable catDT = GetData(string.Format("SELECT *" +
                            " FROM list_categories " +
                            " WHERE RecordStatus <> 'DELETED' AND ID = '{0}'", cat_id.ToString()));
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

        if (member.MemberName == null)
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
        else if (member.CreateDate == null) {
            return "Missing detail: Member Register/ Create Date";
        }
        else
        {
            DataTable memberDT = GetData(string.Format("SELECT *" +
                            " FROM customer " +
                            " WHERE RecordStatus <> 'DELETED' AND hph = '{0}' AND CustomerFirstName = '{1}' AND CustomerDOB = '{2}'", member.HPH.ToString(), member.MemberName.ToString(), member.DOB.ToString()));
            if (memberDT.Rows.Count == 0)
            {
                string strCustCode="";
                string strEndDate="";

                string sqlstr = "SELECT CUSTTYPEID FROM CUSTOMER_TYPE WHERE BLNDEFAULT = 'Y' AND bitFrontendLock='N' and RecordStatus<>'DELETED' and Display='Y'";
                DataTable DT = GetData(string.Format(sqlstr));
                string DefaultMemberType = Convert.ToString(DT.Rows[0]["CUSTTYPEID"]);

                string strCountryID = "";
                sqlstr = "SELECT ID FROM list_countries WHERE Nick LIKE '%" + member.Country.ToString() + "%' OR FULL LIKE '%" + member.Country.ToString() + "%'";
                DT = GetData(string.Format(sqlstr));
                if (DT.Rows.Count != 0)
                {
                    strCountryID = Convert.ToString(DT.Rows[0]["ID"]);
                }
                DT.Clear();
                DT.Dispose();

                string strRetailID = "0";
                sqlstr = "SELECT RetailID FROM retailer WHERE RetailType='ONLINE'";
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

                if (noOfyear==0) {
                    strEndDate ="2035-01-01";
                }
                else
                {
                    strEndDate = DateTime.Parse(member.CreateDate.ToString()).AddYears(noOfyear).ToString("yyyy-MM-dd", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                }

                int runNo = int.Parse(CustStartNo.ToString()) + int.Parse(CustNextNo.ToString()) + 1;
                if(AutoCustCode=="Y") {
                    if(CustCodeEqualICNO=="Y") {
                        strCustCode=member.NRIC.ToString();
                    } else {
                        strCustCode = PrefixCust + (runNo).ToString();
                    }
                } else {
                    if(CustCodeEqualICNO=="Y") {
                         strCustCode=member.NRIC.ToString();
                    } else { strCustCode="";}
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
                            cmd.Parameters.AddWithValue("@CustNextNo", CustNextNo + 1);
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
                string uniqueNo = member.HPH.ToString()  + member.DOB.ToString().Replace("-", "");

                string queryInsertMember = "INSERT INTO customer " +
                            " (ID,UniqueNo,custcode,CustICNo,email,hph,CustomerFirstName,CustomerDOB,CustomerSexID,customertype," +
                            " CustomerAddress1,CustomerAddress2,CustomerAddress3,CustomerPostcode,CustomerCountryID,CustomerStartDate, CustomerEndDate,RetailID," +
                            " ExpiryPointDate,Display,SPV01 ,LastUpdate, LockUpdate, RecordStatus, RecordUpdate,QueueStatus)" +
                            " VALUE " +
                            " (@ID,@UniqueNo,@CustCode,@ICNO,@email,@hph,@Name,@DOB,@Gender,@CustType,@Addr1,@Addr2,@Addr3,@PostalCode,@Country,@StartDate,@EndDate," +
                            " @ExpiryPointDate,@RetailID, @Display,@MemberID,@LastUpdate, @LockUpdate, @RecordStatus, @RecordUpdate,@QueueStatus)";
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
                            cmd.Parameters.AddWithValue("@DOB", member.DOB.ToString());
                            cmd.Parameters.AddWithValue("@Gender", member.Gender.ToString());
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
                            cmd.Parameters.AddWithValue("@Display", "Y");
                            cmd.Parameters.AddWithValue("@MemberID", member.MemberID.ToString());
                            cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@LockUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@RecordStatus", "READY");
                            cmd.Parameters.AddWithValue("@RecordUpdate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@QueueStatus", "READY");

                            cmd.ExecuteNonQuery();
                        }
                        objCnn.Close();
                        return "Success";
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

            
            //if (OrderStatus != "Cancelled")
            if (OrderStatus == "Paid")
            {
                DataTable sdDT = GetData(string.Format("SELECT *" +
                            " FROM salesdelivery " +
                            " WHERE PONo = '{0}' AND RecipientName = '{1}' AND RecipientTel = '{2}' AND MarketPlaceID = '{3}'", PONo.ToString(), RecipientName.ToString(), Tel.ToString(), MarketPlaceID.ToString()));

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
                                " WHERE inventory.ItemSKU = '{0}' AND inventory.ItemName = '{1}'";
                            DataTable inventoryDT = GetData(string.Format(sqlInventory, ItemSKU, ItemName));
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
        DataTable licenseDT = GetData(string.Format("SELECT *" +
                            " FROM tbllicense " +
                            " WHERE Company = '{0}'", companyCode.ToString()));

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

    public string updatePaymentMethod(PaymentMethod paymentMethod)
    {
        for (int i = 0; i < paymentMethod.PaymentMethods.Count(); i++)
        {
            DataTable paymentMethodDT = GetData(string.Format("SELECT *" +
                            " FROM list_paymentmethods " +
                            " WHERE Nick = '{0}'", paymentMethod.PaymentMethods[i].Name));
            if (paymentMethodDT.Rows.Count == 0)
            {
                using (MySqlConnection objCnn = new MySqlConnection(connectionstring))
                {
                    objCnn.Open();
                    Guid paymentMethodID = Guid.NewGuid();
                    string queryInsertPaymentMethod = "INSERT INTO list_paymentmethods " +
                            "(ID, Nick, VALUE,Full, Display,ButtonGroup, SPV05, RecordUpdate)" +
                            " VALUE " +
                            "(@ID, @Nick,@Nick, @Full, @Display,@Nick,@SPV05, @RecordUpdate)";
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
                    " SET Nick = @Name,VALUE = @Name, Full = @Full, Display = @Display,ButtonGroup=@Name, RecordUpdate = @RecordUpdate" +
                    " WHERE ID = @ID";
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

                dblbalanceQty = INV.ItemAging[i].PDQty + ((INV.ItemAging[i].SoldQty * itemActualQty) * -1) + INV.ItemAging[i].TrfInQty + INV.ItemAging[i].TrfOutQty;
                dblbalanceQty = dblbalanceQty + INV.ItemAging[i].AdjQty + INV.ItemAging[i].RetQty + INV.ItemAging[i].SDQty;
                dblbalanceQty = dblbalanceQty + INV.ItemAging[i].KitQty + INV.ItemAging[i].DekitQty + INV.ItemAging[i].ReserveQty + INV.ItemAging[i].InTransitQty;

                queryInsert = "INSERT IGNORE inventory_aging (ID,SupplierID,RetailID,ItemID,ItemSKU,TransID,TransNo,TransDate," +
                              "ItemUOMID,ItemUOM,ItemBaseUOMID,ItemBaseUOM,Qty,ItemActualQty," +
                              "CurrencyID,ExcRate,CostUnitPx,LocalCostUnitPx,CreateTime,BatchNo," +
                              "HSCode,ExpireDate,ExpiryDay,PDQty,SoldQty,TrfInQty,TrfOutQty,AdjQty," +
                              "RetQty,SDQty,KitQty,DekitQty,ReserveQty,InTransitQty,QtyBalance,TerminalID,RFID,PendingSync)" +
                              " VALUE " +
                              "(@ID, @SupplierID, @RetailID, @ItemID, @ItemSKU, @TransID, @TransNo,@TransDate,@ItemUOMID," +
                              "@ItemUOM,@ItemBaseUOMID,@ItemBaseUOM,@Qty, @ItemActualQty, @CurrencyID, @ExcRate, @CostUnitPx," +
                              "@LocalCostUnitPx, @CreateTime, @BatchNo, @HSCode, @ExpireDate,@ExpiryDay, @PDQty, @SoldQty, @TrfInQty," +
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
                                "OnHandDefQty = (getItemOnHandQty(@ItemID,@RetailID,@TodayDate) / @ActualQty ) WHERE ItemID = @ItemID AND RETAILID = @RetailID";
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
                
                DataTable DT = GetData(string.Format("SELECT  RetailID, ItemID, OnHandQty, OnHandDefQty " +
                            " FROM inventory_retail " +
                            " WHERE ItemID='{0}' AND RetailID='{1}'", strItemID, strRetailID));
                //if (DT.Rows.Count != 0)
                //{
                //    strRetailID = Convert.ToString(DT.Rows[0]["RetailID"]);
                //}
                OnHandQtyJson = OnHandQtyJson + "{";
                OnHandQtyJson = OnHandQtyJson + string.Format(@"""RetailID"":""{0}"",""ItemID"":""{1}"",""OnHandQty"":{2},""OnHandDefQty"":{3}"
                    , strRetailID, strItemID, Convert.ToDecimal(DT.Rows[0]["OnHandQty"]), Convert.ToDecimal(DT.Rows[0]["OnHandDefQty"]));
                OnHandQtyJson = OnHandQtyJson + "},";

            }

            OnHandQtyJson = OnHandQtyJson.Remove(OnHandQtyJson.Length - 1) + "]}";
            return OnHandQtyJson;
        }
    }



}