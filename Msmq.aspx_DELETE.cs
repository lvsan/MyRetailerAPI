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
using System.Messaging;
//using TestMSMQ.Clasess;

public partial class Msmq : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        MessageQueue myQueue;
        //string str_foldername = CompanyCode + "_" + RetailID + "_" + TerminalID + "_IN";
        string str_foldername = "dcs_3_1_OUT";
        string path_queue = @".\private$\" + str_foldername;

        if (MessageQueue.Exists(path_queue))
        {
            myQueue = new MessageQueue(path_queue);
            myQueue.DefaultPropertiesToSend.Recoverable = true;

            myQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

            System.Messaging.Message[] myMessage = myQueue.GetAllMessages();
            XmlMessageFormatter formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

            if (myMessage != null && myMessage.Length > 0)
            {
                int TrueCount = 0;
                int a = 0;
                for (a = 0; a < myMessage.Length; a++)
                {
                    myMessage[a].Formatter = formatter;
                    string context = myMessage[a].Body.ToString();

                    if (context != null && context.Length > 0)
                    {
                        DataSet ds_Message = XmlToData.CXmlToDataSet(context);
                        string tablename = ds_Message.Tables[0].TableName;
                        Response.Write(tablename);
                    }
                }
                //myQueue.Purge();
            }
        }
    }
}
