using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Xml;

public class clsXmlTransfer
{
    public static string CDataToXml(DataTable dt)
    {
        if (dt != null)
        {
            MemoryStream ms = null;
            XmlTextWriter XmlWt = null;
            try
            {
                ms = new MemoryStream();
                XmlWt = new XmlTextWriter(ms, Encoding.Unicode);
                dt.WriteXml(XmlWt);
                int count = (int)ms.Length;
                byte[] temp = new byte[count];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(temp, 0, count);
                UnicodeEncoding ucode = new UnicodeEncoding();
                string returnValue = ucode.GetString(temp).Trim();
                return returnValue;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (XmlWt != null)
                {
                    XmlWt.Close();
                    ms.Close();
                    ms.Dispose();
                }
            }
        }
        else
        {
            return "";
        }
    }

    public static string CDataToXml(DataSet ds, int tableIndex)
    {
        if (tableIndex != -1)
        {
            return CDataToXml(ds.Tables[tableIndex]);
        }
        else
        {
            return CDataToXml(ds.Tables[0]);
        }
    }

    public static string CDataToXml(DataSet ds)
    {
        return CDataToXml(ds, -1);
    }

    public static string CDataToXml(DataView dv)
    {
        return CDataToXml(dv.Table);
    }
}

public class XmlToData
{
    public static DataSet CXmlToDataSet(string xmlStr)
    {
        if (!string.IsNullOrEmpty(xmlStr))
        {
            StringReader StrStream = null;
            XmlTextReader Xmlrdr = null;
            try
            {
                DataSet ds = new DataSet();
                StrStream = new StringReader(xmlStr);
                Xmlrdr = new XmlTextReader(StrStream);
                ds.ReadXml(Xmlrdr);
                return ds;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (Xmlrdr != null)
                {
                    Xmlrdr.Close();
                    StrStream.Close();
                    StrStream.Dispose();
                }
            }
        }
        else
        {
            return null;
        }
    }

    public static DataTable CXmlToDatatTable(string xmlStr, int tableIndex)
    {
        return CXmlToDataSet(xmlStr).Tables[tableIndex];
    }

    public static DataTable CXmlToDatatTable(string xmlStr)
    {
        return CXmlToDataSet(xmlStr).Tables[0];
    }
}