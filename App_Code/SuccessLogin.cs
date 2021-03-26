using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

/// <summary>
/// Summary description for SuccessLogin
/// </summary>
public class SuccessLogin
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public string expires_in { get; set; }

	public SuccessLogin()
	{
		//
		// TODO: Add constructor logic here
		//
	}
}