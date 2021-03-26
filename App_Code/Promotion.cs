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
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Summary description for Promotion
/// </summary>
public class Promotion
{
    public string PromoID { get; set; }
    public string PromoName { get; set; }
    public string Promo_Type_ID { get; set; }
    public string Promo_TypeCode { get; set; }
    public string Promo_DateFrom { get; set; }
    public string Promo_DateTo { get; set; }
    public List<PromoRetail> PromoRetails { get; set; }
    public List<PromoItem> PromoItems { get; set; }
    public List<MixMatch> PromoMixMatch { get; set; }
}

public class PromoRetail
{
    public string RetailID { get; set; }
    public string RetailCode { get; set; }
}

public class PromoItem
{
    public string ItemID { get; set; }
    public string ItemUOM { get; set; }
    public string Item_UnitID { get; set; }
    public decimal ItemPrice { get; set; }
    public decimal ItemMemberPrice { get; set; }
    public string ItemUnit { get; set; }
    public string MixID { get; set; }
    public decimal Item_Qty { get; set; }
    public decimal Item_Amt { get; set; }
    public decimal Item_Percentage { get; set; }
    public decimal Item_MemberAmt { get; set; }
    public decimal Item_MemberPerc { get; set; }
    public decimal Item_Qty2 { get; set; }
    public decimal Item_Amt2 { get; set; }
    public decimal Item_Percentage2 { get; set; }
    public decimal Item_MemberAmt2 { get; set; }
    public decimal Item_MemberPerc2 { get; set; }
    public decimal Item_Qty3 { get; set; }
    public decimal Item_Amt3 { get; set; }
    public decimal Item_Percentage3 { get; set; }
    public decimal Item_MemberAmt3 { get; set; }
    public decimal Item_MemberPerc3 { get; set; }
    public decimal Item_Qty4 { get; set; }
    public decimal Item_Amt4 { get; set; }
    public decimal Item_Percentage4 { get; set; }
    public decimal Item_MemberAmt4 { get; set; }
    public decimal Item_MemberPerc4 { get; set; }
    public decimal Item_Qty5 { get; set; }
    public decimal Item_Amt5 { get; set; }
    public decimal Item_Percentage5 { get; set; }
    public decimal Item_MemberAmt5 { get; set; }
    public decimal Item_MemberPerc5 { get; set; }
    public decimal Item_Qty6 { get; set; }
    public decimal Item_Amt6 { get; set; }
    public decimal Item_Percentage6 { get; set; }
    public decimal Item_MemberAmt6 { get; set; }
    public decimal Item_MemberPerc6 { get; set; }
    public decimal Item_Qty7 { get; set; }
    public decimal Item_Amt7 { get; set; }
    public decimal Item_Percentage7 { get; set; }
    public decimal Item_MemberAmt7 { get; set; }
    public decimal Item_MemberPerc7 { get; set; }
    public decimal Item_Qty8 { get; set; }
    public decimal Item_Amt8 { get; set; }
    public decimal Item_Percentage8 { get; set; }
    public decimal Item_MemberAmt8 { get; set; }
    public decimal Item_MemberPerc8 { get; set; }
    public decimal Item_Qty9 { get; set; }
    public decimal Item_Amt9 { get; set; }
    public decimal Item_Percentage9 { get; set; }
    public decimal Item_MemberAmt9 { get; set; }
    public decimal Item_MemberPerc9 { get; set; }
    public decimal Item_Qty10 { get; set; }
    public decimal Item_Amt10 { get; set; }
    public decimal Item_Percentage10 { get; set; }
    public decimal Item_MemberAmt10 { get; set; }
    public decimal Item_MemberPerc10 { get; set; }

}

public class MixMatch
{
    public string Promo_MixID { get; set; }
    public decimal Promo_Qty { get; set; }
    public decimal Promo_Amount { get; set; }
    public decimal Promo_MemberAmount { get; set; }

}
