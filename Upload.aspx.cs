using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

public partial class Upload : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        System.Configuration.KeyValueConfigurationElement msmqSetting = rootWebConfig1.AppSettings.Settings["MSMQ_CONFIG"];

        ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
        configMap.ExeConfigFilename = msmqSetting.Value;
        Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
        string POSGATEPath = config.AppSettings.Settings["QueueFolder"].Value;

        string fileName = Path.GetFileName(Request.Files[0].FileName);
        string path = POSGATEPath + Request.Form["CompanyID"] + "\\" + Request.Form["RetailID"] + "\\" + Request.Form["TerminalID"];
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        try
        {
            Request.Files[0].SaveAs(path + "\\" + fileName);
            Response.Write("SUCCESS");
        }
        catch (Exception ex)
        {
            Response.Write("FAIL");
        }
    }
}
