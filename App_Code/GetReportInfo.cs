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
using System.Collections;

/// <summary>
/// Summary description for GetReportInfo
/// </summary>
public class GetReportInfo
{
    public static ArrayList GetReportPara(string ModuleID,string SubID)
    {
        ArrayList ParamList = new ArrayList();

        ParamList.Add("CostDecimal");
        ParamList.Add("PriceDecimal");
        ParamList.Add("AmountDecimal");
        ParamList.Add("QtyDecimal");

        switch (ModuleID)
        {
            case "RSALE0001":                
            case "RSALE0008":
            case "RSALE0004":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaTransNo");
                ParamList.Add("ParaSalesPerson");
                break;
            case "RSALE0002":
            case "RSALE0005":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaArtNo");
                ParamList.Add("ParaCat");
                ParamList.Add("ParaGroup");
                ParamList.Add("ParaDept");
                ParamList.Add("ParaSubDept");
                ParamList.Add("ParaBrand");
                ParamList.Add("ParaHeels");
                ParamList.Add("ParaItemType");
                ParamList.Add("ParaSalesType");
                ParamList.Add("ParaActivate");
                break;
            case "RSALE0003":
            case "RSALE0013":
            case "RSALE0016":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaSalesPerson");
                break;
            case "RSALE0006":
            case "RSALE0015":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                break;
            case "RSALE0007":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptRetailer");
                ParamList.Add("showMember");
                ParamList.Add("showSalesman");
                break;
            case "RSALE0009":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptCat");
                ParamList.Add("rptBrand");
                ParamList.Add("rptItemType");
                ParamList.Add("rptSalesPerson");
                ParamList.Add("rptDateType");
                break;
            case "RSALE0010":
            case "RSALE0021":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaDept");
                ParamList.Add("ParaSupplier");
                ParamList.Add("ParaArtNo");
                ParamList.Add("ParaSalesType");
                ParamList.Add("ParaBrand");
                ParamList.Add("ParaItemType");
                ParamList.Add("ParaSalesPerson");
                ParamList.Add("ParaCat");
                break;
            case "RSALE0011":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaSalesPerson");
                ParamList.Add("ParaTransNo");
                break;
            case "RSALE0012":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaSalesPerson");
                ParamList.Add("ParaTransNo");
                break;
            case "RSALE0018":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                break;
            /*case "RSALE0021":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaTransNo");
                ParamList.Add("ParaSalesPerson");
                ParamList.Add("ParaSupplier");
                break;*/
            case "RSTK0001":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptartid");
                ParamList.Add("rptbrandid");
                ParamList.Add("rptdeptid");
                ParamList.Add("rptcatid");
                ParamList.Add("rptsuppid");
                ParamList.Add("rptsalestype");
                ParamList.Add("rptitemtype");
                ParamList.Add("rptgroupid");
                ParamList.Add("rptothers");
                break;
            case "RSTK0002":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptartid");
                ParamList.Add("rptbrandid");
                ParamList.Add("rptdeptid");
                ParamList.Add("rptcatid");
                ParamList.Add("rptsubdept");
                ParamList.Add("rptsalestype");
                ParamList.Add("rptitemtype");
                ParamList.Add("rptgroupid");
                ParamList.Add("rptheels");
                ParamList.Add("rptactivate");
                break;
            case "RSTK0003":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptartid");
                ParamList.Add("rptbrandid");
                ParamList.Add("rptdeptid");
                ParamList.Add("rptcatid");
                ParamList.Add("rptsuppid");
                ParamList.Add("rptsalestype");
                ParamList.Add("rptitemtype");
                ParamList.Add("rptgroupid");
                ParamList.Add("rptothers");
                break;
            case "RSTK0004":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptbrandid");
                ParamList.Add("rptdeptid");
                ParamList.Add("rptcatid");
                ParamList.Add("rptsuppid");
                ParamList.Add("rptsalestype");
                ParamList.Add("rptitemtype");
                ParamList.Add("rptgroupid");
                break;
            case "RSTK0005":
                ParamList.Add("rptTitle");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptartid");
                ParamList.Add("rptbrandid");
                ParamList.Add("rptdeptid");
                ParamList.Add("rptcatid");
                ParamList.Add("rptheels");
                ParamList.Add("rptsalestype");
                ParamList.Add("rptitemtype");
                ParamList.Add("rptgroupid");
                ParamList.Add("rptsubdept");
                break;
            case "RSTK0006":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptSalesType");
                ParamList.Add("rptItemType");
                break;
            case "RSTK0007":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptartid");
                break;
            case "RMNT0007":
                ParamList.Add("ParaTitle");
                if (SubID == "M/L-10")
                {
                    ParamList.Add("ParaSNO");
                    ParamList.Add("ParaCode");
                    ParamList.Add("ParaSubCode");
                    ParamList.Add("ParaDesc");
                    ParamList.Add("ParaPicture");
                    ParamList.Add("ParaDisplay");
                }
                break;
            case "RTRN0001": 
            case "RTRN0002": 
            case "RTRN0003": 
            case "RTRN0004":
            case "RTRN0005": 
            case "RTRN0006": 
            case "RTRN0007":
            case "RTRN0008":
                ParamList.Add("rptTitle");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptDate");
                ParamList.Add("rptTransNo");
                ParamList.Add("rptartid");
                ParamList.Add("rptbrandid");
                ParamList.Add("rptdeptid");
                ParamList.Add("rptcatid");
                ParamList.Add("rptsalestype");
                ParamList.Add("rptgroupid");
                ParamList.Add("rptitemtype");
                ParamList.Add("rptsuppid");
                ParamList.Add("rptStatus");
                ParamList.Add("showBaseCost");
                break;
            case "RTRN0009":
                ParamList.Add("rptTitle");
                ParamList.Add("paramGST");
                break;
            case "RMNT0001":
                ParamList.Add("rptTitle");
                ParamList.Add("rptCountry");
                ParamList.Add("rptCompanytype");
                ParamList.Add("rptCompany");
                ParamList.Add("rptDisplay");
                /*ParamList.Add("ParaSNO");
                ParamList.Add("ParaSNO");
                ParamList.Add("ParaSNO");
                ParamList.Add("ParaSNO");
                ParamList.Add("ParaSNO"); */
                break;
            case "RMNT0004":
                ParamList.Add("rptTitle");
                ParamList.Add("rptartid");
                ParamList.Add("rptbrandid");
                ParamList.Add("rptdeptid");
                ParamList.Add("rptcatid");
                ParamList.Add("rptgroupid");
                ParamList.Add("rptactivate");
                break;
            case "RMNT0005":
                ParamList.Add("rptTitle");
                ParamList.Add("rptartid");
                ParamList.Add("rptbrandid");
                ParamList.Add("rptdeptid");
                ParamList.Add("rptcatid");
                ParamList.Add("rptheels");
                ParamList.Add("rptsalestype");
                ParamList.Add("rptitemtype");
                ParamList.Add("rptgroupid");
                ParamList.Add("rptsubdept");
                ParamList.Add("rptactivate");
                break;
            case "RMNT0006":
                ParamList.Add("rptTitle");
                ParamList.Add("rptCountry");
                ParamList.Add("rptDisplay");
                break;
            case "RMNT0008":
                ParamList.Add("rptTitle");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptartid");
                ParamList.Add("rptbrandid");
                ParamList.Add("rptdeptid");
                ParamList.Add("rptcatid");
                ParamList.Add("rptheels");
                ParamList.Add("rptsalestype");
                ParamList.Add("rptitemtype");
                ParamList.Add("rptgroupid");
                ParamList.Add("rptsubdept");
                ParamList.Add("rptactivate");
                break;
            case "RMNT0009":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptpromotype");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptartid");
                ParamList.Add("rptbrandid");
                ParamList.Add("rptdeptid");
                ParamList.Add("rptcatid");
                ParamList.Add("rptitemtype");
                ParamList.Add("rptgroupid");
                ParamList.Add("rptactivate");
                ParamList.Add("rptstatus");
                break;
            case "RMEM0001":
            case "RMEM0003":
            case "RMEM0004":
            case "RMEM0005":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaCustType");
                ParamList.Add("ParaCustCode");
                break;
            case "RMEM0006":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaCustType");
                ParamList.Add("ParaCustCode");
                ParamList.Add("ParaMemberStatus");
                break;
            case "RMEM0002":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaCustType");
                ParamList.Add("ParaCustCode");
                ParamList.Add("ParaJoinDate");
                ParamList.Add("ParaDOB");
                ParamList.Add("ParaActivate");
                ParamList.Add("ParaSortBy");
                break;
            case "RSALE0017":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptRetailer");
                ParamList.Add("rptSortBy");
                break;
            case "RRENT0001":
                ParamList.Add("rptTitle");
                ParamList.Add("rptDate");
                ParamList.Add("rptRetailer");
                ParamList.Add("optMonth");
                ParamList.Add("optYear");
                ParamList.Add("nextMonth");
                ParamList.Add("nextYear");
                ParamList.Add("UserID");
                ParamList.Add("UserFirstName");
                ParamList.Add("UserLastName");
                ParamList.Add("PaymentPlan");
                ParamList.Add("CompanyName");
                ParamList.Add("RetailName");
                ParamList.Add("monthlySales");
                ParamList.Add("ServiceFeeRate");
                ParamList.Add("AdvanceRent");
                ParamList.Add("SecurityDepositAmt");
                ParamList.Add("DeductedAmount");
                ParamList.Add("Amount1");
                ParamList.Add("Amount2");
                break;
            case "RSALE0019":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaTransNo");
                ParamList.Add("ParaSalesPerson");
                ParamList.Add("ParaPaymentType");
                break;
            case "RM0078":
                ParamList.Add("ParaTitle");
                break;
            case "RSALE0022":
            case "RSALE0023":
                ParamList.Add("ParaTitle");
                ParamList.Add("ParaTransDate");
                ParamList.Add("ParaRetailer");
                ParamList.Add("ParaCat");
                ParamList.Add("ParaGroup");
                ParamList.Add("ParaDept");
                ParamList.Add("ParaBrand");
                ParamList.Add("ParaHeels");
                ParamList.Add("ParaItemType");
                break;
        }

        return ParamList;
    }
}
