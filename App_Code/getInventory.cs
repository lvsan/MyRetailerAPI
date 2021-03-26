using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Summary description for getInventory and others master files
/// </summary>
public class getInventory
{
    public string ItemID { get; set; }
    public string ItemSKU { get; set; }
    public string ItemDescp { get; set; }
    public List<Supplier> Suppliers { get; set; }
    public List<ItemPrice> Prices { get; set; }

}

public class ItemPrice
{
    public string Item_UnitID { get; set; }
    public string UOM { get; set; }
    public decimal RTLSellPx { get; set; }
    public string ItemUnitDef { get; set; }
    public string OnHandQty { get; set; }
}

public class Supplier
{
    public string SupplierID { get; set; }
    public string SupBarCode { get; set; }
    public string DefSupplier { get; set; }
}
public class ECategories{
    public ECategoriesDetail[] ECategory { get; set; }
}

public class ECategoriesDetail
{
    public string cat_id { get; set; }
    public string cat_code { get; set; }
    public string cat_descp { get; set; }
    public string cat_otherlanguage { get; set; }
    public string cat_createdate { get; set; }

}

public class OnlineInventory
{
    public string ItemID { get; set; }
    public string ItemSKU { get; set; }
    public string ItemDescp { get; set; }
    public string ItemOtherLanguage { get; set; }
    public string ItemUOM { get; set; }
    public string ItemImage { get; set; }
    public string ItemImageName { get; set; }
    public string ItemRemark { get; set; }
    public decimal ItemOriPrice { get; set; }
    public decimal ItemPrice { get; set; }
    public string ItemCategory { get; set; }
    public string ItemDisplay { get; set; }
    public string CreateBy { get; set; }
    public string TopMenu { get; set; }
    public decimal ItemWeight { get; set; }
    public decimal ItemWidth { get; set; }
    public decimal ItemDepth { get; set; }
    public decimal ItemHeight { get; set; }
    public decimal ItemMeasurement { get; set; }
    public string ItemBoxSize { get; set; }
    public DateTime CreateTime { get; set; }
    public string LastUpdateBy { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public List<ItemBOM> ItemBOM { get; set; }
	public decimal OnHandQty { get; set; }
}

public class ItemBOM
{
    public string ParentItemID { get; set; }
    public string ChildItemID { get; set; }
    public string UOM { get; set; }
    public decimal ChildItemQty { get; set; }
    public decimal ItemActQty { get; set; }
    public decimal RTLSellPx { get; set; }
    public string ItemModifier { get; set; }
    public string ItemStatus { get; set; }
}

public class OnlineCategories
{
    public OnlineCategoriesDetail[] OnlineCategory { get; set; }
}

public class OnlineCategoriesDetail
{
    public string cat_id { get; set; }
    public string cat_code { get; set; }
    public string cat_descp { get; set; }
    public string cat_otherlanguage { get; set; }
    public string cat_image { get; set; }
    public string cat_display { get; set; }
    public string cat_createby { get; set; }
    public DateTime cat_createdate { get; set; }
    public string cat_updateby { get; set; }
    public DateTime cat_lastupdatetime { get; set; }
}


public class OnlineUser
{
    public string user_id { get; set; }
    public string user_code { get; set; }
    public string user_name { get; set; }
    public string user_password { get; set; }
    public string user_email { get; set; }
    public string user_contactno { get; set; }
    public string user_display { get; set; }
    public string user_createby { get; set; }
    public DateTime user_createdate { get; set; }
    public string user_updateby { get; set; }
    public DateTime user_lastupdatetime { get; set; }
}

public class OnlineDefinitions
{
    public string IsCustomDeliveryFee { get; set; }
    public string IsOnlinePay { get; set; }
    public string Merchant_ID { get; set; }
    public string SecretKey { get; set; }
    public string IsIntegrateDelivery { get; set; }
    public string IntergrateDeliveryAgency { get; set; }
    public string DeliveryClientId { get; set; }
    public string DeliveryClientSecret { get; set; }
}


public class OnlineDeliveryInfor
{
    public string ScheduleID { get; set; }
    public string RetailerID { get; set; }
    public string RetailCode { get; set; }
    public string ScheduleType { get; set; }
    public string OperateDayName { get; set; }
    public DateTime OperateTimeStart { get; set; }
    public DateTime OperateTimeClose { get; set; }
    public DateTime TimeSlotStart { get; set; }
    public DateTime TimeSlotClose { get; set; }
    public int IntervalTime { get; set; }
    public decimal MinValueOrder { get; set; }
    public decimal MaxValueOrder { get; set; }
    public decimal DeliveryFee { get; set; }
    public string IsActive { get; set; }
    public string CreateBy { get; set; }
    public DateTime CreateTime { get; set; }
    public string LastUpdateBy { get; set; }
    public DateTime LastUpdateTime { get; set; }
}

public class OnlineWebDesignInfor
{
    public string PageTitle { get; set; }
    public string PageImage { get; set; }
    public string PageContent { get; set; }
    public string LogoImg { get; set; }
    public string TopBannerBackColor { get; set; }
    public string BackgroundColor { get; set; }
    public string FrameID { get; set; }
    public string Display { get; set; }
    public string CreateBy { get; set; }
    public DateTime CreateTime { get; set; }
    public string LastUpdateBy { get; set; }
    public DateTime LastUpdateTime { get; set; }
}
