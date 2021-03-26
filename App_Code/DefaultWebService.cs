using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Reporting.WebForms;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.Script.Services;
using System.IO;

/// <summary>
/// Summary description for DefaultSavePDFServer
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class DefaultWebService : System.Web.Services.WebService {

    private string connServer = "";
    private string connDatabase = "";
    private string connUid = "";
    private string connPwd = "";
    private string connString = "";
    private string reportDS = "";
    private string reportFile = "";
    private string reportSubDS = "";
    private string subrptsql = "";
    private string rptSubID = "";

    public DefaultWebService () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public void SavePDFServer()
    {
        HttpRequest Request = HttpContext.Current.Request;
        ReportViewer ReportViewer1 = new ReportViewer();
        connServer = Request.Form["connServer"];
        connDatabase = Request.Form["connDatabase"];
        connUid = Request.Form["connUid"];
        connPwd = Request.Form["connPwd"];
        connString = "Server=" + connServer + ";Database=" + connDatabase + ";Uid=" + connUid + ";Pwd=" + connPwd + ";AllowUserVariables=True;";
        string sql = Request.Form["sql"];
        subrptsql = Request.Form["subrptsql"];
        string reportID = Request.Form["reportID"];
        string rptFilename = Request.Form["rptFileName"] + ".rdlc";
        //string rptFilename_subRpt = Request.Form["rptFileName_SubRpt"] + ".rdlc";
        string rptReportType = Request.Form["ModuleID"];

        if (!String.IsNullOrEmpty(sql) && !String.IsNullOrEmpty(reportID))
        {

            List<ReportParameter> param = new List<ReportParameter>();

            ArrayList reportParam = GetReportInfo.GetReportPara(rptReportType, rptSubID);

            if (String.IsNullOrEmpty(Request.Form["WithLogo"]) == false)
                param.Add(new ReportParameter("WithLogo", Request.Form["WithLogo"]));

            for (int i = 0; i < reportParam.Count; i++)
            {
                param.Add(new ReportParameter(reportParam[i].ToString(), Request.Form[reportParam[i].ToString()]));
            }

            reportDS = Request.Form["DS"] + "_" + Request.Form["DSTable"];

            if (String.IsNullOrEmpty(Request.Form["DSSubTable"]) == false)
                reportSubDS = Request.Form["DS"] + "_" + Request.Form["DSSubTable"];

            reportFile = "Reports/" + rptFilename;
            //Main Report
            ReportDataSource datasource = new ReportDataSource(reportDS, GetData(sql));
            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(datasource);
            ReportViewer1.LocalReport.ReportPath = Server.MapPath(reportFile);
            //Header
            ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LoadSubReportHeader);

            ReportViewer1.ShowPrintButton = true;
            ReportViewer1.ShowFindControls = true;
            //ReportViewer1.ShowExportControls = false;

            if (String.IsNullOrEmpty(Request.Form["DSSubTable"]) == false)
            {
                if (subrptsql != "")
                {
                    //Sub Report 1
                    ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LoadSubReportDataSource);
                    ReportDataSource datasourcesub = new ReportDataSource(reportSubDS, GetData(subrptsql));
                    ReportViewer1.LocalReport.DataSources.Add(datasourcesub);
                }
            }

            ReportViewer1.LocalReport.SetParameters(param.ToArray());
            ReportViewer1.LocalReport.Refresh();
        }
        Warning[] warnings;
        string[] streamids;
        string mimeType;
        string encoding;
        string filenameExtension;

        byte[] fileBytes = ReportViewer1.LocalReport.Render(
            "PDF", null, out mimeType, out encoding, out filenameExtension,
            out streamids, out warnings);

        // Could save to a database or file here as well.
        //FileStream fs = new FileStream(Server.MapPath(Request.Form["UserID"] + "-" + Request.Form["UserFirstName"] + Request.Form["UserLastName"] + "-" + Request.Form["optMonth"] + "-" + Request.Form["optYear"] + "-report.pdf"), FileMode.Create);
        FileStream fs = new FileStream(@"" + Request.Form["savePath"], FileMode.Create);

        //byte[] data = new byte[fs.Length];

        fs.Write(fileBytes, 0, fileBytes.Length);

        fs.Close();
    }

    public void LoadSubReportDataSource(object sender, SubreportProcessingEventArgs e)
    {
        ReportDataSource datasource = new ReportDataSource(this.reportSubDS, GetData(this.subrptsql));
        e.DataSources.Add(datasource);
    }

    public void LoadSubReportHeader(object sender, SubreportProcessingEventArgs e)
    {
        ReportDataSource datasource = new ReportDataSource("ReportHeader_Header", ReportHeaderSql());
        e.DataSources.Add(datasource);
    }

    private DataTable GetData(string query)
    {

        DataTable tbl = new DataTable();
        try
        {
            MySqlCommand cmd = new MySqlCommand(query);

            using (MySqlConnection con = new MySqlConnection(this.connString))
            {
                using (MySqlDataAdapter sda = new MySqlDataAdapter())
                {
                    cmd.Connection = con;
                    cmd.CommandTimeout = 0;
                    sda.SelectCommand = cmd;
                    sda.SelectCommand.CommandTimeout = 0;
                    sda.Fill(tbl);
                }
            }
        }
        catch (Exception ex)
        {
            //Response.Clear();
            //Response.Write(ex.Message + ":" + ex.StackTrace + ":::::" + query);
            //Response.End();
        }
        return tbl;
    }

    private DataTable ReportHeaderSql()
    {
        string sql = @"SELECT definitions.VaultID,definitions.CompanyName ,definitions.CompanyName_OtherLanguage,
                        definitions.CompanyAdd1,definitions.CompanyAdd2,definitions.CompanyAdd3,definitions.CompanyPost,
                        definitions.CompanyTel,definitions.CompanyFax,definitions.CompanyRegNo,definitions.CompanyTaxReg,
                        the_vault.`obj_id` AS obj_id, the_vault.`object` AS object, the_vault.`obj_name` AS obj_name,
                        the_vault.obj_type as obj_type 
                        FROM the_vault RIGHT OUTER JOIN definitions ON the_vault.`obj_id` = definitions.`VaultID`";
        DataTable table = GetData(sql);
        return table;
    }


    private DataTable ReportSalesDetailSql()
    {
        DataTable table = GetData(this.subrptsql);
        return table;
    }
}

