using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.IO;

public partial class uploadImage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Image1.ImageUrl = lblUrl.Text;
    }
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        
        if (FileUpload1.HasFile)
            try
            {
                if (FileUpload1.FileBytes.Length > 10000000)
                {
                    lblUrl.Text = "File size cannot be larger than 10MB";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "myModal", "$('#divPopUp').modal('show');", true);
                    return;
                }
                string filetype = Path.GetExtension(FileUpload1.FileName);
				string imageName = txtInput.Text + filetype;
                string filePath = Path.Combine(Server.MapPath("~/Images"),imageName);
                FileUpload1.SaveAs(filePath);
                lblUrl.Text = filePath;

            }
            catch (Exception ex)
            {
                lblUrl.Text = "ERROR: " + ex.Message.ToString();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "myModal", "$('#divPopUp').modal('show');", true);
                return;
            }
        else
        {
            lblUrl.Text = "You have not specified a file.";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "myModal", "$('#divPopUp').modal('show');", true);
            return; ;
        }
    }
}
